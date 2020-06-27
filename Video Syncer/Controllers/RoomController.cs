using System;
using System.Diagnostics;
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
                string receivedText = System.Text.Encoding.UTF8.GetString(receivedBytes).Replace("\u0000", "");

                logger.LogInformation("[VSY] receivedText = " + receivedText);
                Trace.TraceInformation("[VSY] receivedText = " + receivedText);

                dynamic unknownObject = JObject.Parse(receivedText);

                string response = await ValidateAndHandleRequest(context, socket, unknownObject);

                sentBytes = System.Text.Encoding.UTF8.GetBytes(response);
                await socket.SendAsync(new ArraySegment<byte>(sentBytes, 0, response.Length),
                    WebSocketMessageType.Text, receivedResult.EndOfMessage, CancellationToken.None);

                receivedResult = await socket.ReceiveAsync(
                    new ArraySegment<byte>(receivedBytes), cancellationToken);
            }

            logger.LogInformation("[VSY] Connection closed. Status code = " + receivedResult.CloseStatus 
                + ", '" + receivedResult.CloseStatusDescription + "'");
            Trace.TraceInformation("[VSY] Connection closed. Status code = " + receivedResult.CloseStatus
                + ", '" + receivedResult.CloseStatusDescription + "'");

            await socket.CloseAsync(receivedResult.CloseStatus.Value,
                receivedResult.CloseStatusDescription, CancellationToken.None);
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
                response = await HandleRequest(context, socket, requestType, userId, room, unknownObject).Result;
            }
            return response;
        }

        private Task<bool> ValidateRequest(HttpContext context, RequestType requestType, dynamic unknownObject)
        {
            var currentSessionId = context.Session.Id;
            
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

                    JoinRoomPayload payload = JoinRoom(socket, name, room, context);

                    responseObject.success = payload == null ? false : true;
                    responseObject.payload = payload;

                    User newUser = room.UserManager.GetUserById(payload.userId);
                    CancellationTokenSource source = new CancellationTokenSource();
                    RoomDataUpdate update = new RoomDataUpdate()
                    {
                        updateType = UpdateType.UserListUpdate,
                        payload = room.UserManager.GetUserList()
                    };

                    await room.ConnectionManager.SendUpdateToAllExcept(room.UserManager.GetUserList(), newUser, update, source.Token);
                    break;

                case RequestType.ChangeVideoState:
                    responseObject.payload = "change video state called and not implemented yet lol";
                    break;

                case RequestType.Leave:
                    break;
                case RequestType.ChangeName:
                    break;
                case RequestType.Kick:
                    break;
                case RequestType.Ban:
                    break;
                case RequestType.MakeAdmin:
                    break;
                case RequestType.RearrangePlaylist:
                    break;
                case RequestType.PlayPlaylistVideo:
                    break;
                case RequestType.PlayVideo:
                    break;
                case RequestType.RemoveFromPlaylist:
                    break;
                case RequestType.AddToPlaylist:
                    break;
                case RequestType.TimeChange:
                    break;
            }

            return JsonConvert.SerializeObject(responseObject);
        }

        private JoinRoomPayload JoinRoom(WebSocket socket, string name, Room room, HttpContext context)
        {
            User user = room.Join(name, context.Session.Id);

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
                currentYoutubeVideoId = room.currentYoutubeVideoId,
                currentYoutubeVideoTitle = room.currentYoutubeVideoTitle,
                currentVideoState = room.UserManager.GetStateForUser(user.id),
                videoTimeSeconds = room.videoTimeSeconds
            };

            return payload;
        }
    }
}