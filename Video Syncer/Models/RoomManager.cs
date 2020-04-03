using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models
{
    public class RoomManager
    {
        private static readonly RoomManager roomManager = new RoomManager();

        public List<Room> roomList = new List<Room>();

        public Room CreateNewRoom()
        {
            string roomId = CreateUniqueRoomId();
            string roomName = CreateRandomName();
            Room room = new Room(roomId, roomName);
            //room.userList = GetTestUsers();

            roomList.Add(room);
            return room;
        }

        private List<User> GetTestUsers()
        {
            var list = new List<User>();
            User user = new User(1000, "mau mau");
            User user2 = new User(1001, "jessicaroon");

            list.Add(user);
            list.Add(user2);

            return list;

        }

        public static RoomManager GetSingletonInstance()
        {
            return roomManager;
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
    }
}
