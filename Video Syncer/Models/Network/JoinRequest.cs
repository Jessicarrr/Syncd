using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Users.Admin;

namespace Video_Syncer.Models.Network
{
    public class JoinRequest
    {
        public string name { get; set; }
        public string roomId { get; set; }
    }

    public class JoinRequestCallback
    {
        public int userId { get; set; }
        public string name { get; set; }
        public UserRights myRights { get; set; }
        public string currentYoutubeVideoId { get; set; }
        public string currentYoutubeVideoTitle { get;set; }
        public List<string> youtubePlaylist { get; set; }
        public VideoState currentVideoState { get; set; }
        public double videoTimeSeconds { get; set; }

        public List<User> userList { get; set; }
    }
}
