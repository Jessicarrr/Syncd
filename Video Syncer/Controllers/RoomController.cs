using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Video_Syncer.Models;
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

        [HttpPost]
        public async Task ConnectToWebSocket()
        {
            if(HttpContext.WebSockets.IsWebSocketRequest)
            {
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

            while(receivedResult.CloseStatus.HasValue)
            {
                string receivedText = System.Text.Encoding.UTF8.GetString(receivedBytes).Replace("\u0000", "");
                dynamic unknownObject = JsonConvert.DeserializeObject<dynamic>(receivedText);
                string requestType = unknownObject.GetType().GetProperty("requestType").GetValue(unknownObject, null);

                logger.LogInformation("receivedText = " + receivedText);
                Trace.TraceInformation("receivedText = " + receivedText);

                logger.LogInformation("requestType = " + requestType);
                Trace.TraceInformation("requestType = " + requestType);

                sentBytes = System.Text.Encoding.UTF8.GetBytes(requestType);
                await socket.SendAsync(new ArraySegment<byte>(sentBytes, 0, requestType.Length),
                    WebSocketMessageType.Text, receivedResult.EndOfMessage, CancellationToken.None);

                receivedResult = await socket.ReceiveAsync(
                    new ArraySegment<byte>(receivedBytes), cancellationToken);
            }

            logger.LogInformation("Connection closed. Status code = " + receivedResult.CloseStatus 
                + ", '" + receivedResult.CloseStatusDescription + "'");
            Trace.TraceInformation("Connection closed. Status code = " + receivedResult.CloseStatus
                + ", '" + receivedResult.CloseStatusDescription + "'");

            await socket.CloseAsync(receivedResult.CloseStatus.Value, 
                receivedResult.CloseStatusDescription, CancellationToken.None);
        }
    }
}