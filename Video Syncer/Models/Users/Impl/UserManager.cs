using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.logging;
using Video_Syncer.Models.Users.Interface;
using Video_Syncer.Models.Users.Enum;
using System.Net.WebSockets;
using System.Threading;
using System.Net;

namespace Video_Syncer.Models.Users.Impl
{
    public class UserManager : IUserManager
    {
        public List<User> userList = new List<User>();
        private readonly int maxUsers = 500;
        public int disconnectedUserThresholdSeconds = 20;
        private string roomId;

        public List<string> allowedSessionIds = new List<string>();

        public int usernameCharacterLimit = 25;

        private ILogger logger;

        protected bool HasFirstUserJoined { get; set; }

        protected List<string> adminSessionIDs { get; set; } = new List<string>();
        protected List<string> bannedSessionIDs { get; set; } = new List<string>();
        protected List<IPAddress> bannedIpAddresses { get; set; } = new List<IPAddress>();

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

        public bool IsAdmin(User user)
        {
            return user.rights == UserRights.Admin;
        }

        public bool Kick(User user, User recipient)
        {
            if(!userList.Contains(recipient))
            {
                return false;
            }
            
            Task.Run(async () => await ExecuteKick(user, recipient));
            return true;
        }

        protected async Task ExecuteKick(User user, User recipient)
        {
            await Task.Delay(5000);
            logger.LogInformation("[VSY] User \"" + recipient.name + "\" (id: " + recipient.id + ") was forcibly kicked out of the room " + roomId + " by " + user.name + "\" (id: " + user.id + ")");
            await recipient.socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Kicked", CancellationToken.None);
            RemoveFromUserList(recipient);
        }

        public bool Ban(User user, User recipient)
        {
            if(user.rights != UserRights.Admin)
            {
                return false;
            }

            CreateNewBannedUser(recipient);
            return true;
        }

        public void CreateNewBannedUser(User user)
        {
            string userSessionID = user.sessionID;

            if(!bannedSessionIDs.Contains(userSessionID))
            {
                bannedSessionIDs.Add(userSessionID);
                
            }

            if(!bannedIpAddresses.Contains(user.IpAddress))
            {
                bannedIpAddresses.Add(user.IpAddress);
            }
        }

        public bool CreateNewAdmin(User user)
        {
            user.rights = UserRights.Admin;

            if(user.sessionID == null)
            {
                return false;
            }

            if(adminSessionIDs.Contains(user.sessionID))
            {
                return true;
            }

            adminSessionIDs.Add(user.sessionID);
            return true;
        }

        public bool RemoveAdmin(User user)
        {
            user.rights = UserRights.User;

            string admin = adminSessionIDs.Where(session => session == user.sessionID).FirstOrDefault();

            if(admin == null)
            {
                return false;
            }
            else
            {
                adminSessionIDs.Remove(admin);
                return true;
            }
        }

        public bool IsSessionIdRegisteredAsAdmin(string sessionID)
        {
            IEnumerable<string> sessionIdList = adminSessionIDs.Where(id => id == sessionID);

            if (sessionIdList.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public bool IsSessionIdBanned(string sessionID)
        {
            IEnumerable<string> matchingBannedIds = bannedSessionIDs.Where(id => id == sessionID);

            if(matchingBannedIds.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public bool IsIpAddressBanned(IPAddress paramIp)
        {
            IEnumerable<IPAddress> matchingIpAddresses = bannedIpAddresses.Where(ip => ip.Equals(paramIp));

            if(matchingIpAddresses.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public User Join(string name, string sessionID, IPAddress ipAddress)
        {
            if(IsSessionIdBanned(sessionID) || IsIpAddressBanned(ipAddress))
            {
                return null;
            }

            User user = CreateNewUser(name, sessionID, ipAddress);
            logger.LogInformation("[VSY]Joining user \"" + user.name + "\"");
            AddToUserList(user);

            if(!HasFirstUserJoined)
            {
                CreateNewAdmin(user);
                HasFirstUserJoined = true;
            }
            else
            {
                if(IsSessionIdRegisteredAsAdmin(sessionID))
                {
                    user.rights = UserRights.Admin;
                }
            }

            return user;
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

        public bool SomeoneHasState(VideoState state)
        {
            IEnumerable<User> usersWithState = userList.Where(user => user.videoState == state);

            if(usersWithState.Count() > 0)
            {
                return true;
            }
            return false;
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

        public User CreateNewUser(string name, string sessionID, IPAddress ipAddress)
        {
            int userId = this.CreateUniqueUserId();

            if (name.Length > usernameCharacterLimit)
            {
                name = name.Substring(0, usernameCharacterLimit);
            }
            User user = new User(userId, name, sessionID, ipAddress);
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

        public bool IsUserIpAddressMatching(int userId, IPAddress ipAddress)
        {
            User user = GetUserById(userId);

            if (user == null)
            {
                return false;
            }

            if (ipAddress.Equals(user.IpAddress))
            {
                return true;
            }
            return false;
        }
    }
}
