using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Video_Syncer.Models;
using Video_Syncer.Models.Network.Payload;
using Video_Syncer.Models.Network.Payload.StateUpdates;
using Video_Syncer.Models.Network.RequestResponses.Enum;
using Video_Syncer.Models.Network.RequestResponses.Impl;
using Video_Syncer.Models.Network.StateUpdates.Enum;
using Video_Syncer.Models.Network.StateUpdates.Impl;
using Video_Syncer.Views.Room;

namespace Video_Syncer.Controllers
{
    public class RoomController : Controller
    {
        public ILogger logger;
        public IRoomManagerSingleton roomManager;


        public RoomController(ILogger<RoomController> logger, IRoomManagerSingleton roomManager)
        {
            this.logger = logger;
            this.roomManager = roomManager;
        }

        [Route("room/{id}")]
        public IActionResult FindRoom(string id)
        {
            Room room = TryGetRoom(id);

            if (room == null)
            {
                return View("RoomNotFound");
            }
            else
            {
                if(room.UserManager.IsFull())
                {
                    return View("RoomFull");
                }
                else
                {
                    string sessionID = HttpContext.Session.Id;

                    if(room.UserManager.IsSessionIdBanned(sessionID))
                    {
                        return RedirectToAction("Banned", "Home");
                    }

                    room.UserManager.GetSessionIdList().Add(sessionID);

                    RoomModel model = new RoomModel()
                    {
                        id = room.id,
                        name = room.name,
                        userList = room.UserManager.GetUserList()
                    };
                    return View("Room", model);
                }
                
            }

        }

        private Room TryGetRoom(string roomId)
        {
            try
            {
                Room room = roomManager.GetRoom(roomId);
                return room;
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return null;
            }
        }

        [Route("new")]
        public IActionResult Index()
        {
            Room room = roomManager.CreateNewRoom();

            return RedirectToAction(room.id, "room");
        }

        public async Task ConnectToWebSocket()
        {
            if(HttpContext.WebSockets.IsWebSocketRequest)
            {
                logger.LogInformation("[VSY] new web socket connected");
                Trace.TraceInformation("[VSY] new web socket connected");

                WebSocket socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleWebSocketConnection(HttpContext, socket);
            }
        }
        private async Task HandleWebSocketConnection(HttpContext context, WebSocket socket)
        {
            var sessionID = context.Session.Id;

            var receivedBytes = new Byte[1024];
            var sentBytes = new Byte[1024];

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            WebSocketReceiveResult receivedResult = await socket.ReceiveAsync(
                new ArraySegment<byte>(receivedBytes), cancellationToken);

            while(!receivedResult.CloseStatus.HasValue)
            {
                string receivedText = System.Text.Encoding.UTF8.GetString(receivedBytes, 0, receivedResult.Count);

                logger.LogInformation("[VSY] receivedText = " + receivedText);
                Trace.TraceInformation("[VSY] receivedText = " + receivedText);

                try
                {
                    dynamic unknownObject = JObject.Parse(receivedText);

                    string response = await ValidateAndHandleRequest(context, socket, unknownObject);

                    sentBytes = System.Text.Encoding.UTF8.GetBytes(response);

                    await socket.SendAsync(sentBytes,
                        WebSocketMessageType.Text, receivedResult.EndOfMessage, CancellationToken.None);

                    receivedResult = await socket.ReceiveAsync(
                        new ArraySegment<byte>(receivedBytes), cancellationToken);
                }
                catch(JsonReaderException e)
                {
                    logger.LogError("[VSY] Error reading json in RoomController.HandleWebSocketConnection. error = " + e.Message + "\n" + e.StackTrace);
                    Trace.TraceError("[VSY] Error reading json in RoomController.HandleWebSocketConnection. error = " + e.Message + "\n" + e.StackTrace);
                    // TODO : send an error message back to the user

                    receivedResult = await socket.ReceiveAsync(
                        new ArraySegment<byte>(receivedBytes), cancellationToken);

                }
                catch(WebSocketException e2)
                {
                    logger.LogError("[VSY] WebSocketException in RoomController.HandleWebSocketConnection. error = " + e2.Message + "\n" + e2.StackTrace);
                    Trace.TraceError("[VSY] WebSocketException in RoomController.HandleWebSocketConnection. error = " + e2.Message + "\n" + e2.StackTrace);
                    break;
                }
            }

            logger.LogInformation("[VSY] Connection closed. Status code = " + receivedResult.CloseStatus 
                + ", description = '" + receivedResult.CloseStatusDescription + "'");
            Trace.TraceInformation("[VSY] Connection closed. Status code = " + receivedResult.CloseStatus
                + ", description = '" + receivedResult.CloseStatusDescription + "'");

            try
            {
                await socket.CloseAsync(receivedResult.CloseStatus.Value,
                       receivedResult.CloseStatusDescription, CancellationToken.None);

            }
            catch(InvalidOperationException e)
            {
                logger.LogError("[VSY] InvalidOperationException in RoomController.HandleWebSocketConnection. error = " + e.Message + "\n" + e.StackTrace);
                Trace.TraceError("[VSY] InvalidOperationException in RoomController.HandleWebSocketConnection. error = " + e.Message + "\n" + e.StackTrace);
            }
            
            
        }

        private async Task<string> ValidateAndHandleRequest(HttpContext context, WebSocket socket, dynamic unknownObject)
        {
            string response;
            RequestType requestType = (RequestType)unknownObject.requestType;

            RequestResponse responseObject = new RequestResponse()
            {
                requestType = requestType,
                payload = null
            };

            bool isValidRequest = await ValidateRequest(context, requestType, unknownObject);

            if(!isValidRequest)
            {
                // request had a problem and is not valid.
                responseObject.success = false;
                response = JsonConvert.SerializeObject(responseObject);
            }
            else
            {
                string roomId = unknownObject.roomId;
                int? userId = unknownObject.userId;
                Room room = TryGetRoom(roomId);
                response = await HandleRequest(context, socket, requestType, userId, room, unknownObject);
            }
            return response;
        }

        private Task<bool> ValidateRequest(HttpContext context, RequestType requestType, dynamic unknownObject)
        {
            var currentSessionId = context.Session.Id;
            var address = context.Connection.RemoteIpAddress;
            
            string requestTypeHumanReadable = Enum.GetName(typeof(RequestType), requestType);

            string roomId = unknownObject.roomId;

            if(roomId == null)
            {
                return Task.FromResult(false);
            }

            Room room = TryGetRoom(roomId);

            if (room == null)
            {
                return Task.FromResult(false);
            }

            if (requestType != RequestType.Join)
            {
                int? userId = unknownObject.userId;

                if(userId == null)
                {
                    return Task.FromResult(false);
                }

                if(room.UserManager.GetUserById((int) userId) == null)
                {
                    return Task.FromResult(false);
                }

                if (!room.UserManager.IsUserSessionIDMatching((int) userId, currentSessionId))
                {
                    logger.LogWarning("[VSY] Session ID of request did not match in room \"" + room.id 
                        + "\"! Session ID of the request was " + currentSessionId + ", and request type was " 
                        + requestTypeHumanReadable);
                    return Task.FromResult(false);
                }
                
            }

            if (room.UserManager.IsSessionIdBanned(currentSessionId))
            {
                logger.LogWarning("[VSY] A banned session ID tried to make a request (request type was " + requestTypeHumanReadable
                    + "). Session ID was " + currentSessionId);
                return Task.FromResult(false);
            }

            if(requestType == RequestType.Kick || requestType == RequestType.MakeAdmin || requestType == RequestType.Ban)
            {
                int userId = unknownObject.userId;
                User requestingUser = room.UserManager.GetUserById(userId);

                if(requestingUser.rights != Models.Users.Enum.UserRights.Admin)
                {
                    return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }

        private async Task<string> HandleRequest(HttpContext context, WebSocket socket, RequestType requestType, int? userId,
            Room room, dynamic unknownObject)
        {
            RequestResponse responseObject = new RequestResponse()
            {
                requestType = requestType,
                payload = null
            };

            switch (requestType)
            {
                case RequestType.Join:
                    string name = unknownObject.name;

                    JoinRoomPayload payload = await JoinRoom(socket, name, room, context);

                    responseObject.success = payload == null ? false : true;
                    responseObject.payload = payload;

                    break;

                case RequestType.ChangeVideoState:
                    VideoState newVideoState = (VideoState) unknownObject.state;
                    VideoStatePayload videoStatePayload = await ChangeVideoState(socket, userId, room, context, newVideoState);

                    responseObject.success = videoStatePayload == null ? false : true;
                    responseObject.payload = videoStatePayload;
                    break;

                case RequestType.Leave:
                    await LeaveRoom(socket, userId, room);

                    responseObject.success = true;
                    responseObject.payload = null;
                    break;
                case RequestType.ChangeName:
                    string newName = unknownObject.newName;
                    responseObject.payload = await ChangeName(userId, room, newName);
                    responseObject.success = responseObject.payload == null ? false : true;
                    break;
                case RequestType.Kick:
                    int userToKick = unknownObject.userIdToKick;
                    bool kicked = await KickUser(userId, userToKick, room);
                    responseObject.success = kicked;
                    responseObject.payload = null;

                    break;
                case RequestType.Ban:
                    int userToBan = unknownObject.userIdToBan;
                    bool banned = await BanUser(userId, userToBan, room);

                    responseObject.success = banned;
                    responseObject.payload = null;

                    break;
                case RequestType.MakeAdmin:
                    int? targetUserId = unknownObject.userIdToMakeAdmin;
                    UserListPayload makeAdminPayload = await MakeAdmin(userId, targetUserId, room);

                    responseObject.success = makeAdminPayload != null;
                    responseObject.payload = makeAdminPayload;
                    break;
                case RequestType.RearrangePlaylist:
                    string onTopId = unknownObject.onTopId;
                    string onBottomId = unknownObject.onBottomId;

                    PlaylistStatePayload rearrangePayload = await RearrangePlaylist(userId, room, onTopId, onBottomId);
                    responseObject.success = rearrangePayload != null;
                    responseObject.payload = rearrangePayload;

                    break;
                case RequestType.PlayPlaylistVideo:
                    string videoToPlay = unknownObject.playlistItemId;
                    VideoStatePayload playPlaylistVideoPayload = await PlayPlaylistVideo(userId, room, videoToPlay);

                    responseObject.success = playPlaylistVideoPayload == null ? false : true;
                    responseObject.payload = playPlaylistVideoPayload;

                    break;
                case RequestType.PlayVideo:
                    break;
                case RequestType.RemoveFromPlaylist:
                    string playlistItemId = unknownObject.playlistItemId;
                    PlaylistStatePayload removeFromPlaylistPayload = await RemoveFromPlaylist(userId, room, playlistItemId);

                    responseObject.success = removeFromPlaylistPayload == null ? false : true;
                    responseObject.payload = removeFromPlaylistPayload;
                    break;
                case RequestType.AddToPlaylist:
                    string videoId = unknownObject.youtubeVideoId;
                    PlaylistStatePayload addToPlaylistPayload = await AddToPlaylist(userId, room, videoId);

                    responseObject.success = addToPlaylistPayload == null ? false : true;
                    responseObject.payload = addToPlaylistPayload;

                    break;
                case RequestType.TimeChange:
                    double seconds = unknownObject.videoTimeSeconds;
                    VideoStatePayload timeChangePayload = await ChangeVideoTime(userId, room, seconds);

                    responseObject.success = timeChangePayload == null ? false : true;
                    responseObject.payload = timeChangePayload;
                    break;
            }

            return JsonConvert.SerializeObject(responseObject);
        }

        private async Task<JoinRoomPayload> JoinRoom(WebSocket socket, string name, Room room, HttpContext context)
        {
            User user = room.Join(name, context.Session.Id, context.Connection.RemoteIpAddress);

            if(user == null)
            {
                return null;
            }

            user.socket = socket;

            JoinRoomPayload payload = new JoinRoomPayload()
            {
                userId = user.id,
                myRights = user.rights,
                userList = room.UserManager.GetUserList(),
                playlist = room.PlaylistManager.GetPlaylist(),
                currentYoutubeVideoId = room.currentYoutubeVideoId,
                currentYoutubeVideoTitle = room.currentYoutubeVideoTitle,
                currentVideoState = room.UserManager.GetStateForUser(user.id),
                videoTimeSeconds = room.videoTimeSeconds
            };

            CancellationTokenSource source = new CancellationTokenSource();
            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.UserListUpdate,
                payload = room.UserManager.GetUserList()
            };

            await room.ConnectionManager.SendUpdateToAllExcept(user, room, update, source.Token);

            return payload;
        }

        private async Task LeaveRoom(WebSocket socket, int? userId, Room room)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        "The user left the room, so the socket was disconnected", CancellationToken.None);
            room.Leave((int)userId);

            CancellationTokenSource source = new CancellationTokenSource();
            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.UserListUpdate,
                payload = room.UserManager.GetUserList()
            };

            await room.ConnectionManager.SendUpdateToAll(room, update, source.Token);
        }

        private async Task<VideoStatePayload> ChangeVideoState(WebSocket socket, int? userId, Room room, 
            HttpContext context, VideoState state)
        {
            User user = room.UserManager.GetUserById((int)userId);
            bool roomStateChanged = room.NewVideoState((int)userId, state);

            if(roomStateChanged)
            {
                VideoStatePayload payload = new VideoStatePayload()
                {
                    currentVideoState = room.GetSuggestedVideoState(),
                    videoTimeSeconds = room.videoTimeSeconds,
                    currentYoutubeVideoId = room.currentYoutubeVideoId,
                    currentYoutubeVideoTitle = room.currentYoutubeVideoTitle
                };

                RoomDataUpdate update = new RoomDataUpdate()
                {
                    updateType = UpdateType.VideoUpdate,
                    payload = payload
                };

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

                await room.ConnectionManager.SendUpdateToAllExcept(user, room, update, cancelTokenSource.Token);

                return payload;

            }
            return null;
        }

        private async Task<VideoStatePayload> ChangeVideoTime(int? userId, Room room, double seconds)
        {
            User user = room.UserManager.GetUserById((int)userId);
            room.TimeUpdate(seconds);

            VideoStatePayload payload = new VideoStatePayload()
            {
                currentVideoState = room.GetSuggestedVideoState(),
                videoTimeSeconds = room.videoTimeSeconds,
                currentYoutubeVideoId = room.currentYoutubeVideoId,
                currentYoutubeVideoTitle = room.currentYoutubeVideoTitle
            };

            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.VideoUpdate,
                payload = payload
            };

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

            await room.ConnectionManager.SendUpdateToAllExcept(user, room, update, cancelTokenSource.Token);
            return payload;
        }

        private async Task<PlaylistStatePayload> AddToPlaylist(int? userId, Room room, string videoId)
        {
            User user = room.UserManager.GetUserById((int)userId);
            room.PlaylistManager.AddToPlaylist(videoId, async delegate()
            {
                PlaylistStatePayload payload = new PlaylistStatePayload()
                {
                    playlist = room.PlaylistManager.GetPlaylist()
                };

                RoomDataUpdate update = new RoomDataUpdate()
                {
                    updateType = UpdateType.PlaylistUpdate,
                    payload = payload
                };

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

                await room.ConnectionManager.SendUpdateToAll(room, update, cancelTokenSource.Token);
            });

            PlaylistStatePayload payload = new PlaylistStatePayload()
            {
                playlist = room.PlaylistManager.GetPlaylist()
            };

            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.PlaylistUpdate,
                payload = payload
            };

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

            await room.ConnectionManager.SendUpdateToAllExcept(user, room, update, cancelTokenSource.Token);

            return payload;
        }

        private async Task<PlaylistStatePayload> RemoveFromPlaylist(int? userId, Room room, string playlistItemId)
        {
            User user = room.UserManager.GetUserById((int)userId);
            room.PlaylistManager.RemoveFromPlaylist(playlistItemId);

            PlaylistStatePayload payload = new PlaylistStatePayload()
            {
                playlist = room.PlaylistManager.GetPlaylist()
            };

            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.PlaylistUpdate,
                payload = payload
            };

            CancellationTokenSource source = new CancellationTokenSource();

            await room.ConnectionManager.SendUpdateToAllExcept(user, room, update, source.Token);

            return payload;
        }

        private async Task<VideoStatePayload> PlayPlaylistVideo(int? userId, Room room, string videoToPlay)
        {
            User user = room.UserManager.GetUserById((int)userId);
            room.PlayPlaylistVideo(videoToPlay);

            VideoStatePayload payload = new VideoStatePayload()
            {
                currentVideoState = room.GetSuggestedVideoState(),
                videoTimeSeconds = room.videoTimeSeconds,
                currentYoutubeVideoId = room.currentYoutubeVideoId,
                currentYoutubeVideoTitle = room.currentYoutubeVideoTitle
            };

            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.VideoUpdate,
                payload = payload
            };
            CancellationTokenSource source = new CancellationTokenSource();

            await room.ConnectionManager.SendUpdateToAllExcept(user, room, update, source.Token);

            return payload;
        }

        private async Task<bool> KickUser(int? userId, int? userToKickId, Room room)
        {
            if(userToKickId == null)
            {
                return false;
            }

            User user = room.UserManager.GetUserById((int)userId);
            User userToKick = room.UserManager.GetUserById((int)userToKickId);

            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.RedirectToPage,
                payload = "/Kicked"
            };
            CancellationTokenSource source = new CancellationTokenSource();

            await room.ConnectionManager.SendUpdateToUser(userToKick, room, update, source.Token);
            await Task.Delay(1000);

            bool kicked = room.UserManager.Kick(user, userToKick);

            if(kicked)
            {
                RoomDataUpdate userListUpdate = new RoomDataUpdate()
                {
                    updateType = UpdateType.UserListUpdate,
                    payload = room.UserManager.GetUserList()
                };
                
                await Task.Delay(6000);
                await room.ConnectionManager.SendUpdateToAll(room, userListUpdate, source.Token);

                return true;
            }
            return false;
            
        }

        private async Task<bool> BanUser(int? userId, int? userToBanId, Room room)
        {
            if(userToBanId == null)
            {
                return false;
            }

            User user = room.UserManager.GetUserById((int)userId);
            User userToBan = room.UserManager.GetUserById((int)userToBanId);

            if(user == null || userToBan == null)
            {
                return false;
            }

            RoomDataUpdate redirect = new RoomDataUpdate()
            {
                updateType = UpdateType.RedirectToPage,
                payload = "/Banned"
            };

            CancellationTokenSource source = new CancellationTokenSource();
            
            bool banned = room.UserManager.Ban(user, userToBan);

            await room.ConnectionManager.SendUpdateToUser(userToBan, room, redirect, source.Token);

            room.UserManager.Kick(user, userToBan);
            await Task.Delay(6000);

            RoomDataUpdate userListUpdate = new RoomDataUpdate()
            {
                updateType = UpdateType.UserListUpdate,
                payload = room.UserManager.GetUserList()
            };

            await room.ConnectionManager.SendUpdateToAll(room, userListUpdate, source.Token);
            return banned;
        }

        private async Task<UserListPayload> ChangeName(int? userId, Room room, string newName)
        {
            User user = room.UserManager.GetUserById((int)userId);
            room.UserManager.ChangeName((int) userId, newName);

            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.UserListUpdate,
                payload = room.UserManager.GetUserList()
            };

            UserListPayload payload = new UserListPayload()
            {
                userList = room.UserManager.GetUserList()
            };
            CancellationTokenSource source = new CancellationTokenSource();

            await room.ConnectionManager.SendUpdateToAllExcept(user, room, update, source.Token);
            return payload;
        }

        private async Task<UserListPayload> MakeAdmin(int? userId, int? targetUserId, Room room)
        {
            if(userId == null || targetUserId == null)
            {
                return null;
            }

            User user = room.UserManager.GetUserById((int)userId);
            User targetUser = room.UserManager.GetUserById((int)targetUserId);
            CancellationTokenSource source = new CancellationTokenSource();

            if(user == null || targetUser == null || user.rights != Models.Users.Enum.UserRights.Admin)
            {
                return null;
            }

            bool madeAdmin = room.UserManager.CreateNewAdmin(targetUser);

            if(madeAdmin)
            {
                RoomDataUpdate userListUpdate = new RoomDataUpdate()
                {
                    updateType = UpdateType.UserListUpdate,
                    payload = room.UserManager.GetUserList()
                };
                UserListPayload payload = new UserListPayload()
                {
                    userList = room.UserManager.GetUserList()
                };

                await room.ConnectionManager.SendUpdateToAllExcept(user, room, userListUpdate, source.Token);

                return payload;
            }
            return null;
        }

        private async Task<PlaylistStatePayload> RearrangePlaylist(int? userId, Room room, string onTopId, string onBottomId)
        {
            User user = room.UserManager.GetUserById((int)userId);
            bool wasSuccessful = room.PlaylistManager.RearrangePlaylist(onTopId, onBottomId);
            CancellationTokenSource source = new CancellationTokenSource();

            PlaylistStatePayload payload = new PlaylistStatePayload()
            {
                playlist = room.PlaylistManager.GetPlaylist()
            };

            RoomDataUpdate update = new RoomDataUpdate()
            {
                updateType = UpdateType.PlaylistUpdate,
                payload = payload
            };

            await room.ConnectionManager.SendUpdateToAllExcept(user, room, update, source.Token);
            return payload;
        }
    }
}