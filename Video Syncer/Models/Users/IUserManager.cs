using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Users
{
    public interface IUserManager
    {
        public bool HasFirstUserJoined
        {
            get;
            set;
        }

        public bool ChangeName(int userId, string newName);
        public bool AddToUserList(User user);
        public void RemoveFromUserList(User user);
        public bool RemoveFromUserList(int userId);
        public void ForceLeaveAllTimedOutUsers();
        public void SetStateForUser(int userId, VideoState state);
        public void SetStateForAll(VideoState state);
        public bool AllHasState(VideoState state);
        public VideoState GetStateForUser(int userId);
        public bool UpdateLastConnectionTime(int userId);
        public User UpdateUser(int userId, double seconds);
        public User CreateNewUser(string name, string sessionID);
        public bool IsFull();
        public bool UserIdExists(int id);

        public User GetUserById(int id);

        public bool IsUserSessionIDMatching(int userId, string sessionID);

        public int GetNumUsers();

        public List<User> GetUserList();
        public List<string> GetSessionIdList();
    }
}
