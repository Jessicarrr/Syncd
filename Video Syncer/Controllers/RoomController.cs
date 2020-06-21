using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                int requestType = unknownObject.requestType;

                logger.LogInformation("[VSY] requestType = " + requestType);
                Trace.TraceInformation("[VSY] requestType = " + requestType);

                var newObject = (requestType, message: "hey this is a message, the rqt was " + requestType);
                var jsonStringToSend = JsonConvert.SerializeObject(newObject);

                sentBytes = System.Text.Encoding.UTF8.GetBytes(jsonStringToSend);
                await socket.SendAsync(new ArraySegment<byte>(sentBytes, 0, jsonStringToSend.Length),
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
    }
}