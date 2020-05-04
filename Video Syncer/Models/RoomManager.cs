using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Video_Syncer.Models
{
    public class RoomManager : IDisposable
    {
        private static readonly RoomManager roomManager = new RoomManager();

        public List<Room> roomList = new List<Room>();
        private CancellationTokenSource source;
        private int periodicTaskMilliseconds = 10000;

        private bool disposed = false;
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);



        public Room CreateNewRoom()
        {
            string roomId = CreateUniqueRoomId();
            string roomName = CreateRandomName();
            Room room = new Room(roomId, roomName);
            //room.userList = GetTestUsers();

            roomList.Add(room);
            return room;
        }

        public static RoomManager GetSingletonInstance()
        {
            return roomManager;
        }

        private RoomManager()
        {
            StartPeriodicTasks();
        }

        private string CreateRandomName()
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

        private Boolean RoomIdTaken(String roomId)
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
            Trace.WriteLine("[VSY] CancelPeriodicTasks() ran on room Manager");
            source.Cancel();
        }

        public async Task PeriodicDestroyEmptyRooms(CancellationToken token)
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

        private void DestroyEmptyRooms()
        {
            //Trace.WriteLine("[VSY] Called DestroyEmptyRooms()");

            foreach (Room room in roomList)
            {
                if(room.userList.Count <= 0)
                {
                    Trace.WriteLine("[VSY] Destroying room " + room.id);
                    room.Dispose();
                    roomList.Remove(room);
                }
                if(roomList.Count <= 0)
                {
                    Trace.WriteLine("[VSY] All rooms are destroyed, stopping RoomManager periodic tasks");
                    source.Cancel();
                }
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
                Trace.WriteLine("[VSY] Room Manager disposed.");
            }

            disposed = true;
        }
    }
}
