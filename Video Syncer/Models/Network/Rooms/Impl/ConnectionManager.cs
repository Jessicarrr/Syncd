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

        public async Task<int> CheckAndRemoveDisconnectedUsers(Room room, CancellationToken token)
        {
            var dataToSend = new Byte[1];
            List<User> disconnectedUsers = new List<User>();

            foreach (User loopUser in room.UserManager.GetUserList())
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await loopUser.socket.SendAsync(dataToSend, WebSocketMessageType.Text, true, token);
                }
                catch(WebSocketException)
                {
                    disconnectedUsers.Add(loopUser);
                }
            }

            int usersToRemove = disconnectedUsers.Count;

            if (disconnectedUsers.Count > 0)
            {
                foreach (User disconnectedUser in disconnectedUsers)
                {
                    logger.LogError("[VSY] User forced to leave the room when checking for disconnected/closed sockets. User was " + disconnectedUser.name
                        + " with id " + disconnectedUser.id + " in room " + room.id);
                    room.Leave(disconnectedUser);
                }

                RoomDataUpdate newUpdate = new RoomDataUpdate()
                {
                    updateType = StateUpdates.Enum.UpdateType.UserListUpdate,
                    payload = room.UserManager.GetUserList()
                };

                await SendUpdateToAll(room, newUpdate, token);
            }

            return usersToRemove;
        }

        public async Task SendUpdateToUser(User user, Room room, IUpdate update, CancellationToken token)
        {
            var newString = JsonConvert.SerializeObject(update);
            var dataToSend = new Byte[newString.Length] ;
            dataToSend = System.Text.Encoding.UTF8.GetBytes(newString);
            bool userDisconnected = false;
            
            if(token.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await user.socket.SendAsync(dataToSend, WebSocketMessageType.Text, true, token);
            }
            catch(WebSocketException e)
            {
                logger.LogError("Caught exception in ConnectionManager.SendUpdateToAll. User was " + user.name
                        + " with id " + user.id + " in room " + room.id
                        + ". The user will be forced to leave the room. The exception was: " + e.Message);
                userDisconnected = true;
            }

            if(userDisconnected == true)
            {
                room.Leave(user);

                RoomDataUpdate newUpdate = new RoomDataUpdate()
                {
                    updateType = StateUpdates.Enum.UpdateType.UserListUpdate,
                    payload = room.UserManager.GetUserList()
                };

                await SendUpdateToAll(room, newUpdate, token);
            }
        }

        public async Task SendUpdateToAdmins(Room room, IUpdate update, CancellationToken cancellationToken)
        {
            var newString = JsonConvert.SerializeObject(update);
            var dataToSend = new Byte[newString.Length];
            dataToSend = System.Text.Encoding.UTF8.GetBytes(newString);
            List<User> disconnectedUsers = new List<User>();

            foreach (User loopUser in room.UserManager.GetAllAdmins())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await loopUser.socket.SendAsync(dataToSend,
                        WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch(WebSocketException e)
                {
                    logger.LogError("Caught exception in ConnectionManager.SendUpdateToAdmins. User was " + loopUser.name
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

        public async Task SendUpdateToAll(Room room, IUpdate update, CancellationToken cancellationToken)
        {
            var newString = JsonConvert.SerializeObject(update);
            var dataToSend = new Byte[newString.Length];
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
                    await loopUser.socket.SendAsync(dataToSend,
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
            var newString = JsonConvert.SerializeObject(update);
            var dataToSend = new Byte[newString.Length];
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
                    await loopUser.socket.SendAsync(dataToSend,
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
