using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Video_Syncer.logging;
using Video_Syncer.Models.Network.Payload.StateUpdates;
using Video_Syncer.Models.Network.Rooms.Interface;
using Video_Syncer.Models.Network.StateUpdates.Impl;
using Video_Syncer.Models.Network.StateUpdates.Interface;

namespace Video_Syncer.Models.Network.Rooms.Impl
{
    public class ConnectionManager : IConnectionManager
    {
        private ILogger logger;

        public ConnectionManager()
        {
            logger = LoggingHandler.CreateLogger<ConnectionManager>();
        }

        public async Task SendUpdateToAll(Room room, IUpdate update, CancellationToken cancellationToken)
        {
            var dataToSend = new Byte[1024];
            var newString = JsonConvert.SerializeObject(update);
            dataToSend = System.Text.Encoding.UTF8.GetBytes(newString);
            List<User> disconnectedUsers = new List<User>();

            foreach (User loopUser in room.UserManager.GetUserList())
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await loopUser.socket.SendAsync(new ArraySegment<byte>(dataToSend, 0, newString.Length),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch(WebSocketException e)
                {
                    logger.LogError("Caught exception in ConnectionManager.SendUpdateToAll. User was " + loopUser.name 
                        + " with id " + loopUser.id + " in room " + room.id 
                        + ". The user will be forced to leave the room. The exception was: " + e.Message);
                    disconnectedUsers.Add(loopUser);
                }
            }

            if (disconnectedUsers.Count > 0)
            {
                foreach (User loopUser in disconnectedUsers)
                {
                    room.Leave(loopUser);
                }

                RoomDataUpdate newUpdate = new RoomDataUpdate()
                {
                    updateType = StateUpdates.Enum.UpdateType.UserListUpdate,
                    payload = room.UserManager.GetUserList()
                };

                await SendUpdateToAll(room, newUpdate, cancellationToken);
            }
        }

        public async Task SendUpdateToAllExcept(User user, Room room, IUpdate update, CancellationToken cancellationToken)
        {
            var dataToSend = new Byte[1024];
            var newString = JsonConvert.SerializeObject(update);
            dataToSend = System.Text.Encoding.UTF8.GetBytes(newString);
            List<User> disconnectedUsers = new List<User>();

            foreach(User loopUser in room.UserManager.GetUserList()) 
            {
                if(loopUser == user)
                {
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                
                try
                {
                    await loopUser.socket.SendAsync(new ArraySegment<byte>(dataToSend, 0, newString.Length),
                    WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch(WebSocketException e)
                {
                    logger.LogError("Caught exception in ConnectionManager.SendUpdateToAll. User was " + loopUser.name
                        + " with id " + loopUser.id + " in room " + room.id
                        + ". The user will be forced to leave the room. The exception was: " + e.Message);
                    disconnectedUsers.Add(loopUser);
                }
            }

            if(disconnectedUsers.Count > 0)
            {
                foreach (User loopUser in disconnectedUsers)
                {
                    room.Leave(loopUser);
                }

                RoomDataUpdate newUpdate = new RoomDataUpdate()
                {
                    updateType = StateUpdates.Enum.UpdateType.UserListUpdate,
                    payload = room.UserManager.GetUserList()
                };

                await SendUpdateToAll(room, newUpdate, cancellationToken);
            }
        }
    }
}
