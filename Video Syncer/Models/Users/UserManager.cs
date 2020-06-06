using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.logging;

namespace Video_Syncer.Models.Users
{
    public class UserManager : IUserManager
    {
        public List<User> userList = new List<User>();
        private int maxUsers = 500;
        public int disconnectedUserThresholdSeconds = 20;
        private string roomId;

        public List<string> allowedSessionIds = new List<string>();

        public int usernameCharacterLimit = 25;

        private ILogger logger;

        public bool HasFirstUserJoined { get; set; }

        public UserManager(string roomId)
        {
            this.roomId = roomId;
            logger = LoggingHandler.CreateLogger<UserManager>();
        }

        public UserManager()
        {
            logger = LoggingHandler.CreateLogger<UserManager>();
        }

        public int GetNumUsers()
        {
            return userList.Count;
        }

        public List<User> GetUserList()
        {
            return userList;
        }

        public List<string> GetSessionIdList()
        {
            return allowedSessionIds;
        }

        public bool ChangeName(int userId, string newName)
        {
            if(String.IsNullOrEmpty(newName))
            {
                return false;
            }

            User relevantUser = userList.FirstOrDefault(obj => obj.id == userId);

            if(relevantUser == null)
            {
                return false;
            }

            if(newName.Length > usernameCharacterLimit)
            {
                relevantUser.name = newName.Substring(0, usernameCharacterLimit);
                return true;
            }

            relevantUser.name = newName;
            return true;
        }

        public bool AddToUserList(User user)
        {
            if (!UserIdExists(user.id) && !IsFull())
            {
                userList.Add(user);
                return true;
            }
            return false;
        }

        public void RemoveFromUserList(User user)
        {
            if (UserIdExists(user.id))
            {
                logger.LogInformation("[VSY] User removed! \"" + user.name + "\" (id: " + user.id + ") from room " + roomId);
                userList.Remove(user);
            }
        }

        public bool RemoveFromUserList(int userId)
        {
            if (UserIdExists(userId))
            {
                User user = GetUserById(userId);

                userList.Remove(user);
                return true;
            }
            return false;
        }


        public void ForceLeaveAllTimedOutUsers()
        {
            //CTrace.WriteLine("Called ForceLeaveAllTimedOutUsersAsync in room " + id);

            foreach (User user in userList)
            {
                if (user.SecondsSinceLastConnection() >= disconnectedUserThresholdSeconds)
                {
                    logger.LogInformation("[VSY] User \"" + user.name + "\" (id: " + user.id + ") timed out from room " + roomId);
                    RemoveFromUserList(user);
                }
            }
        }

        public void SetStateForUser(int userId, VideoState state)
        {
            foreach (User user in userList)
            {
                if (userId == user.id)
                {
                    user.videoState = state;
                }
            }
        }

        public void SetStateForAll(VideoState state)
        {
            foreach (User user in userList)
            {
                user.videoState = state;
            }

            /*System.Diagnostics.Debug.WriteLine("Set state for all. All user states are: ");
            foreach (User user in userList)
            {
                System.Diagnostics.Debug.WriteLine(user.name + ": " + user.videoState);
            }*/
        }

        public bool AllHasState(VideoState state)
        {
            IEnumerable<User> usersWithoutState = userList.Where(user => user.videoState != state);

            if(usersWithoutState.Count() > 0)
            {
                return false;
            }
            return true;
        }

        public VideoState GetStateForUser(int userId)
        {
            foreach (User user in userList)
            {
                if (userId == user.id)
                {
                    return user.videoState;
                }
            }
            return VideoState.Unstarted;
        }

        public bool UserIsBuffering()
        {
            foreach (User user in userList)
            {
                if (user.videoState == VideoState.Buffering)
                {
                    return true;
                }
            }
            return false;
        }

        public bool UpdateLastConnectionTime(int userId)
        {
            User user = GetUserById(userId);

            if(user != null)
            {
                user.UpdateLastConnectionTime();
                return true;
            }
            return false;
        }

        public User UpdateUser(int userId, double seconds)
        {
            User user = GetUserById(userId);

            if (user == null)
            {
                return null;
            }

            user.videoTimeSeconds = seconds;
            user.UpdateLastConnectionTime();
            return user;
        }

        public User GetBufferingUser()
        {
            foreach (User user in userList)
            {
                if (user.videoState == VideoState.Buffering)
                {
                    return user;
                }
            }
            return null;
        }

        public User CreateNewUser(string name, string sessionID)
        {
            int userId = this.CreateUniqueUserId();

            if (name.Length > usernameCharacterLimit)
            {
                name = name.Substring(0, usernameCharacterLimit);
            }
            User user = new User(userId, name, sessionID);
            return user;
        }

        public int CreateUniqueUserId()
        {
            Random random = new Random();
            int id = -1;
            do
            {
                id = random.Next(0, 10000);
            }
            while (UserIdExists(id));

            return id;
        }

        public bool IsFull()
        {
            if (userList.Count >= maxUsers)
            {
                return true;
            }
            return false;
        }

        public bool UserIdExists(int id)
        {
            foreach (User user in userList)
            {
                if (user.id == id)
                {
                    return true;
                }
            }
            return false;
        }
        public User GetUserById(int id)
        {
            foreach (User user in userList)
            {
                if (user.id == id)
                {
                    return user;
                }
            }
            return null;
        }

        public bool IsUserSessionIDMatching(int userId, string sessionID)
        {
            User user = GetUserById(userId);

            if(user == null)
            {
                return false;
            }

            if (String.Equals(user.sessionID, sessionID))
            {
                return true;
            }
            return false;
        }
    }
}
