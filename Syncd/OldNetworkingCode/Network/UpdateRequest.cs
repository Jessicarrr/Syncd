using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Playlist;
using Video_Syncer.Models.Users.Admin;

namespace Video_Syncer.Models.Network
{
    public class UpdateRequest : IRequest
    {
        public string roomId { get; set; }
        public int userId { get; set; }
        public double videoTimeSeconds { get; set; }
        public int videoState { get; set; }
        public string currentYoutubeVideoId { get; set; }

    }

    public class UpdateRequestCallback : ICallback
    {
        public string name { get; set; }
        public UserRights myRights { get; set; }
        public string currentYoutubeVideoId { get; set; }

        public string currentYoutubeVideoTitle { get; set; }
        public List<PlaylistObject> playlist { get; set; }
        public VideoState currentVideoState { get; set; }
        public double videoTimeSeconds { get; set; }

        public List<User> userList { get; set; }
        public bool success { get; set; }
        public bool ShouldKick { get; set; } = false;
        public bool ShouldBan { get; set; } = false;
    }
}
