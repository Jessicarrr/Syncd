using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Video_Syncer.Models;
using Video_Syncer.Models.Network;
using Video_Syncer.Views.Room;

namespace Video_Syncer.Controllers
{
    public class RoomController : Controller
    {
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
                if(room.IsFull())
                {
                    return View("RoomFull");
                }
                else
                {
                    RoomModel model = new RoomModel()
                    {
                        id = room.id,
                        name = room.name,
                        userList = room.userList
                    };
                    return View("Room", model);
                }
                
            }

        }

        [HttpPost]
        public JsonResult EndVideo([FromBody]VideoStateChangeRequest request)
        {
            if (request == null)
            {
                return null;
            }

            Room room = TryGetRoom(request.roomId);
            
            if (room == null)
            {
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] End Video Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
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
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] Play Video Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
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
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] Pause Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
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
        public JsonResult AddToPlaylist([FromBody]AddToPlaylistRequest request)
        {
            if (request == null)
            {
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] Add to Playlist Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
                VideoStateChangeCallback callback2 = new VideoStateChangeCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            room.AddToPlaylist(request.youtubeVideoId);
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
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] Change Video Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
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
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] Buffer Video Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
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
                return null;
            }

            Room room = TryGetRoom(request.roomId);
            

            if (room == null)
            {
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] Time Update Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
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
                RoomManager roomManager = RoomManager.GetSingletonInstance();
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
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                return null;
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] General Update Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
                return null;
            }

            // update room
            room.UpdateVideoStatistics(
                request.videoTimeSeconds,
                request.currentYoutubeVideoId);

            // update user
            room.UpdateUser(request.userId, request.videoTimeSeconds);

            // send users back
            UpdateRequestCallback callback = new UpdateRequestCallback()
            {
                userList = room.userList,
                currentYoutubeVideoId = room.currentYoutubeVideoId,
                name = room.name,
                currentVideoState = room.GetStateForUser(request.userId),
                videoTimeSeconds = room.videoTimeSeconds,
                playlist = room.playlist
            };
            return Json(callback);

            //return Json("Server says thank you! Data received was " + request.name + " and " + request.roomId);
        }

        [HttpPost]
        public JsonResult Join([FromBody]JoinRequest request)
        {
            if (request == null)
            {
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                return Json(new { success = false });
            }
            else
            {
                string sessionID = HttpContext.Session.Id;
                User user = room.Join(request.name, sessionID);
                JoinRequestCallback callback = new JoinRequestCallback()
                {
                    userId = user.id,
                    userList = room.userList,
                    currentYoutubeVideoId = room.currentYoutubeVideoId,
                    currentVideoState = room.GetStateForUser(user.id),
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
                return null;
            }

            Room room = TryGetRoom(request.roomId);

            if (room == null)
            {
                LeaveRequestCallback callback2 = new LeaveRequestCallback()
                {
                    success = false
                };
                return Json(callback2);
            }

            string sessionID = HttpContext.Session.Id;

            if (!room.IsUserSessionIDMatching(request.userId, sessionID))
            {
                Trace.WriteLine("[VSY] Leave Room Request - session ID did not match in room \"" + room.id + "\"! Session ID of the request was " + sessionID);
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
            RoomManager roomManager = RoomManager.GetSingletonInstance();
            Room room = roomManager.CreateNewRoom();

            return RedirectToAction(room.id, "room");
        }
    }
}