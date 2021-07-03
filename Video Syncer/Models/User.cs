using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Video_Syncer.Models.Users.Enum;

namespace Video_Syncer.Models
{
    public class User
    {

        public int id { get; set; }
        public string name { get; set; }
        public UserRights rights { get; set; } = UserRights.User;
        [JsonIgnore]
        public string sessionID { get; set; }
        [JsonIgnore]
        public IPAddress IpAddress { get; set; }
        [JsonIgnore]
        public long lastConnectionTime { get; set; }
        [JsonIgnore]
        public WebSocket socket { get; set; }
        [JsonIgnore]
        public double videoTimeSeconds { get; set; }
        [JsonIgnore]
        public VideoState videoState { get; set; }


        public User(int Id, string Name, string sessionID, IPAddress ipAddress)
        {
            this.id = Id;
            this.name = Name;
            this.sessionID = sessionID;
            this.IpAddress = ipAddress;

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
