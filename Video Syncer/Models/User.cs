using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Users.Admin;

namespace Video_Syncer.Models
{
    public class User
    {
        public double videoTimeSeconds { get; set; }
        public VideoState videoState { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string sessionID { get; set; }
        public long lastConnectionTime { get; set; }
        public UserRights rights { get; set; } = UserRights.User;

        public User(int Id, string Name, string sessionID)
        {
            this.id = Id;
            this.name = Name;
            this.sessionID = sessionID;

            videoState = VideoState.Unstarted;
            videoTimeSeconds = -1;

            lastConnectionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(); 
        }
        
        public int SecondsSinceLastConnection()
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long timeSinceLastConnectionMillis = now - lastConnectionTime;
            int convertedToSeconds = Convert.ToInt32(timeSinceLastConnectionMillis / 1000);
            return convertedToSeconds;
        }

        public void UpdateLastConnectionTime()
        {
            lastConnectionTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(); 
        }

    }
}
