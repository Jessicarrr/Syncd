using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Syncd.logging;
using Syncd.Models.Network.Rooms.Impl;
using Syncd.Models.Playlist;
using Syncd.Models.Users.Impl;

namespace Syncd.Models
{
    public class RoomManagerSingleton : IDisposable, IRoomManagerSingleton
    {
        public List<Room> roomList = new List<Room>();
        private CancellationTokenSource source;
        private int periodicTaskMilliseconds = 10000;

        private bool disposed = false;
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        private ILogger logger;

        public Room CreateNewRoom()
        {
            string roomId = CreateUniqueRoomId();
            string roomName = CreateRandomRoomName();

            // Room room = ActivatorUtilities.CreateInstance<Room>(serviceProvider);
            // https://stackoverflow.com/questions/37189984/dependency-injection-with-classes-other-than-a-controller-class/44252662
            Room room = new Room(new PlaylistManager(), new UserManager(), new ConnectionManager(), roomId, roomName);
            //room.userList = GetTestUsers();

            logger.LogInformation("[VSY]New room created with name " + room.name + " and id " + room.id);
            roomList.Add(room);
            return room;
        }

        public RoomManagerSingleton()
        {
            logger = LoggingHandler.CreateLogger<RoomManagerSingleton>();

            StartPeriodicTasks();
        }

        private string CreateRandomRoomName()
        {
            string[] adjectives = new string[] { "Western","Concern","Familiar",
                                                "Fly","Official","Broad",
                                                "Comfortable","Gain",
                                                "Rich","Save",
                                                "Stand","Young",
                                                "Fail","Heavy",
                                                "Hello","Lead",
                                                "Listen","Valuable",
                                                "Worry","Handle",
                                                "Leading","Meet",
                                                "Release","Sell",
                                                "Finish","Normal",
                                                "Press","Ride",
                                                "Secret","Spread",
                                                "Spring","Tough",
                                                "Wait","Brown",
                                                "Deep","Display",
                                                "Flow","Hit",
                                                "Objective","Shoot",
                                                "Touch","Cancel",
                                                "Chemical","Cry",
                                                "Dump","Extreme",
                                                "Pushing","Conflict",
                                                "Eat","Filler",
                                                "Formal" };
            string[] nouns = new string[] { "Room", "Area", "District", "Zone", "Locale", "Space", "Territory"};

            Random random = new Random();
            string adjective = adjectives[random.Next(adjectives.Length)];
            string noun = nouns[random.Next(nouns.Length)];

            return adjective + " " + noun;
        }

        private string CreateUniqueRoomId()
        {
            string validRoomCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string randomString = "";

            do
            {
                for (int i = 0; i < 10; i++)
                {
                    randomString += validRoomCharacters[random.Next(validRoomCharacters.Length)];
                }
            }
            while (RoomIdTaken(randomString));

            return randomString;
        }

        public Room GetRoom(string roomId)
        {
            foreach (Room room in roomList)
            {
                if (room.id == roomId)
                {
                    return room;
                }
            }
            return null;
        }

        public Boolean RoomIdTaken(String roomId)
        {
            foreach(Room room in roomList)
            {
                if(room.id == roomId)
                {
                    return true;
                }
            }
            return false;
        }

        private void StartPeriodicTasks()
        {
            source = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                await PeriodicDestroyEmptyRooms(source.Token);
            }, 
            source.Token);
        }

        private void CancelPeriodicTasks()
        {
            logger.LogInformation("[VSY] CancelPeriodicTasks() ran on room Manager");
            source.Cancel();
        }

        private async Task PeriodicDestroyEmptyRooms(CancellationToken token)
        {
            while (true)
            {
                if(token.IsCancellationRequested)
                {
                    break;
                }

                new Task(() => DestroyEmptyRooms()).Start();
                await Task.Delay(periodicTaskMilliseconds);
            }
        }

        public void DestroyRoom(Room room)
        {
            logger.LogInformation("[VSY] Destroying room " + room.id);
            room.Dispose();
            roomList.Remove(room);
        }

        private void DestroyEmptyRooms()
        {
            //CTrace.WriteLine("Called DestroyEmptyRooms()");

            foreach (Room room in roomList)
            {
                int roomAgeInMinutes = room.GetMinutesSinceRoomCreation();

                if(room.UserManager.GetNumUsers() <= 0 && room.RoomShouldBeDestroyed == true)
                {
                    if (roomAgeInMinutes < 1)
                    {
                        logger.LogInformation("[VSY] Will not destroy room " + room.id + " because it is only "
                            + roomAgeInMinutes + " minute(s) old. (Room must be more than 1 minute old to destroy)");
                        return;
                    }
                    DestroyRoom(room);
                }

                /*if(roomList.Count <= 0)
                {
                    CTrace.TraceInformation("All rooms are destroyed, stopping RoomManager periodic tasks");
                    source.Cancel();
                }*/
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                CancelPeriodicTasks();
                logger.LogInformation("[VSY]Room Manager disposed.");
            }

            disposed = true;
        }
    }
}
