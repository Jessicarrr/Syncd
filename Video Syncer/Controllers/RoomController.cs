using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Video_Syncer.logging;
using Video_Syncer.Models;
using Video_Syncer.Models.Network;
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

        [HttpPost]
        public JsonResult ChangeName([FromBody]ChangeUsernameRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.ChangeName");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.ChangeName. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] End Video Request - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                ChangeUsernameCallback callback2 = new ChangeUsernameCallback()
                {
                    success = false
                };
                return Json(callback2);
            }
            bool wasSuccessful = room.UserManager.ChangeName(request.userId, request.newName);

            if(!wasSuccessful)
            {
                logger.LogWarning("[VSY] RoomManager.ChangeName was not successful (wasSuccessful == " + wasSuccessful + ") with userId " 
					+ request.userId + " and new name = " + request.newName);
            }

            ChangeUsernameCallback callback = new ChangeUsernameCallback()
            {
                success = wasSuccessful
            };
            return Json(callback);
        }

        [HttpPost]
        public JsonResult EndVideo([FromBody]VideoStateChangeRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.EndVideo");
                return null;
            }

            Room room = TryGetRoom(request.roomId);
            
            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.EndVideo. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.EndVideo - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                VideoStateChangeCallback callback2 = new VideoStateChangeCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.NewVideoState(request.userId, VideoState.Ended);
            VideoStateChangeCallback callback = new VideoStateChangeCallback()
            {
                success = true
            };
            return Json(callback);

        }

        [HttpPost]
        public JsonResult PlayVideo([FromBody]VideoStateChangeRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.PlayVideo");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.PlayVideo. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.PlayVideo - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                VideoStateChangeCallback callback2 = new VideoStateChangeCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.NewVideoState(request.userId, VideoState.Playing);
            VideoStateChangeCallback callback = new VideoStateChangeCallback()
            {
                success = true
            };
            return Json(callback);
        }



        [HttpPost]
        public JsonResult PauseVideo([FromBody]VideoStateChangeRequest request)
        {
            if (request == null)
            {
                logger.LogWarning("[VSY] request was null in RoomController.PauseVideo");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.PauseVideo. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.PauseVideo - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                VideoStateChangeCallback callback2 = new VideoStateChangeCallback()
                {
                    success = false
                };
                return Json(callback2);
            }
            room.NewVideoState(request.userId, VideoState.Paused);
            VideoStateChangeCallback callback = new VideoStateChangeCallback()
            {
                success = true
            };
            return Json(callback);
        }

        [HttpPost]
        public JsonResult PlayPlaylistVideo([FromBody]PlayPlaylistVideoRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.PlayPlaylistVideo");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.PlayPlaylistVideo. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.PlayPlaylistVideo - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                RemoveFromPlaylistCallback callback2 = new RemoveFromPlaylistCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            bool wasSuccessful = room.PlayPlaylistVideo(request.playlistItemId);

            if (!wasSuccessful)
            {
                logger.LogWarning("[VSY] RoomManager.PlayPlaylistVideo was not successful (wasSuccessful == " + wasSuccessful + ") with userId "
                    + request.userId + " and new item to play playlist id = " + request.playlistItemId);
            }

            PlayPlaylistVideoCallback callback = new PlayPlaylistVideoCallback()
            {
                success = wasSuccessful
            };
            return Json(callback);
        }

        [HttpPost]
        public JsonResult RemoveFromPlaylist([FromBody]RemoveFromPlaylistRequest request)
        {
            if(request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.RemoveFromPlaylist");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.RemoveFromPlaylist. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.RemoveFromPlaylist - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                RemoveFromPlaylistCallback callback2 = new RemoveFromPlaylistCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.PlaylistManager.RemoveFromPlaylist(request.playlistItemId);
            RemoveFromPlaylistCallback callback = new RemoveFromPlaylistCallback()
            {
                success = true
            };
            return Json(callback);
        }

        [HttpPost]
        public JsonResult AddToPlaylist([FromBody]AddToPlaylistRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.AddToPlaylist");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.AddToPlaylist. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.AddToPlaylist - session ID did not match in room \""
                     + room.id + "\"! Session ID of the request was " + sessionID);
                VideoStateChangeCallback callback2 = new VideoStateChangeCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.PlaylistManager.AddToPlaylist(request.youtubeVideoId);
            VideoStateChangeCallback callback = new VideoStateChangeCallback()
            {
                success = true
            };
            return Json(callback);
        }

        [HttpPost]
        public JsonResult ChangeVideo([FromBody]VideoChangeRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.ChangeVideo");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("Room was null in RoomController.ChangeVideo. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("RoomController.ChangeVideo - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                VideoStateChangeCallback callback2 = new VideoStateChangeCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.NewVideo(request.youtubeVideoId);
            VideoStateChangeCallback callback = new VideoStateChangeCallback()
            {
                success = true
            };
            return Json(callback);
        }

        [HttpPost]
        public JsonResult BufferVideo([FromBody]VideoStateChangeRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.BufferVideo");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.BufferVideo. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.BufferVideo - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                VideoStateChangeCallback callback2 = new VideoStateChangeCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.NewVideoState(request.userId, VideoState.Buffering);
            VideoStateChangeCallback callback = new VideoStateChangeCallback()
            {
                success = true
            };
            return Json(callback);
        }

        [HttpPost]
        public JsonResult TimeUpdate([FromBody]TimeChangeRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.TimeUpdate");
                return null;
            }

            Room room = TryGetRoom(request.roomId);
            

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.TimeUpdate. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.TimeUpdate - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                LeaveRequestCallback callback2 = new LeaveRequestCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.TimeUpdate(request.videoTimeSeconds);

            TimeChangeRequestCallback callback = new TimeChangeRequestCallback()
            {
                success = true
            };
            return Json(callback);
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
                return null;
            }
        }

        [HttpPost]
        public JsonResult Update([FromBody]UpdateRequest request)
        {
            if(request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.Update");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.Update. user id = " + request.userId
                    + ", room id was " + request.roomId);
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.Update - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                return null;
            }

            // update room
            room.UpdateVideoStatistics(
                request.videoTimeSeconds,
                request.currentYoutubeVideoId);

            // update user
            room.UserManager.UpdateUser(request.userId, request.videoTimeSeconds);

            // send users back
            UpdateRequestCallback callback = new UpdateRequestCallback()
            {
                userList = room.UserManager.GetUserList(),
                currentYoutubeVideoId = room.currentYoutubeVideoId,
                currentYoutubeVideoTitle = room.currentYoutubeVideoTitle,
                name = room.name,
                currentVideoState = room.UserManager.GetStateForUser(request.userId),
                videoTimeSeconds = room.videoTimeSeconds,
                playlist = room.PlaylistManager.GetPlaylist(),
                success = true
            };
            return Json(callback);

            //return Json("Server says thank you! Data received was " + request.name + " and " + request.roomId);
        }

        [HttpPost]
        public JsonResult Join([FromBody]JoinRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.Join");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.Join. user name = " + request.name
                    + ", room id was " + request.roomId);
                return Json(new { success = false });
            }
            else
            {
                string sessionID = HttpContext.Session.Id;

                if (!room.UserManager.GetSessionIdList().Contains(sessionID))
                {
                    return Json(new { success = false });
                }

                User user = room.Join(request.name, sessionID);
                JoinRequestCallback callback = new JoinRequestCallback()
                {
                    userId = user.id,
                    userList = room.UserManager.GetUserList(),
                    currentYoutubeVideoId = room.currentYoutubeVideoId,
                    currentYoutubeVideoTitle = room.currentYoutubeVideoTitle,
                    currentVideoState = room.UserManager.GetStateForUser(user.id),
                    videoTimeSeconds = room.videoTimeSeconds
                };
                return Json(callback);
            }


            //return Json("Server says thank you! Data received was " + request.name + " and " + request.roomId);
        }

        [HttpPost]
        public JsonResult Leave([FromBody]LeaveRequest request)
        {
            if (request == null)
            {
                logger.LogError("[VSY] request was null in RoomController.Leave");
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                logger.LogWarning("[VSY] Room was null in RoomController.Leave. user id = " + request.userId
                    + ", room id was " + request.roomId);
                LeaveRequestCallback callback2 = new LeaveRequestCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.UserManager.IsUserSessionIDMatching(request.userId, sessionID))
            {
                logger.LogWarning("[VSY] RoomController.Leave - session ID did not match in room \""
                    + room.id + "\"! Session ID of the request was " + sessionID);
                LeaveRequestCallback callback2 = new LeaveRequestCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.Leave(request.userId);
            LeaveRequestCallback callback = new LeaveRequestCallback()
            {
                success = true
            };
            return Json(callback);


            //return Json("Server says thank you! Data received was " + request.name + " and " + request.roomId);
        }

        [Route("new")]
        public IActionResult Index()
        {
            Room room = roomManager.CreateNewRoom();

            return RedirectToAction(room.id, "room");
        }
    }
}