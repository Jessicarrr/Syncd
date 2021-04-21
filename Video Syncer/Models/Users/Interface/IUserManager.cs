using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Users.Interface
{
    public interface IUserManager
    {
        public bool IsSessionIdBanned(string sessionID);
        public bool Kick(User user, User recipient);
        public bool Ban(User user, User recipient);
        public bool IsAdmin(User user);
        public bool CreateNewAdmin(User user);
        public bool RemoveAdmin(User user);
        public User Join(string name, string sessionID, IPAddress ipAddress);
        public bool ChangeName(int userId, string newName);
        public bool AddToUserList(User user);
        public void RemoveFromUserList(User user);
        public bool RemoveFromUserList(int userId);
        public void ForceLeaveAllTimedOutUsers();
        public void SetStateForUser(int userId, VideoState state);
        public void SetStateForAll(VideoState state);
        public bool AllHasState(VideoState state);
        public bool SomeoneHasState(VideoState state);
        public VideoState GetStateForUser(int userId);
        public bool UpdateLastConnectionTime(int userId);
        public User UpdateUser(int userId, double seconds);
        public User CreateNewUser(string name, string sessionID, IPAddress ipAddress);
        public bool IsFull();
        public bool UserIdExists(int id);

        public User GetUserById(int id);

        public bool IsUserSessionIDMatching(int userId, string sessionID);

        public int GetNumUsers();

        public List<User> GetUserList();
        public List<string> GetSessionIdList();
    }
}
