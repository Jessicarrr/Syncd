using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncd.Models.Playlist;
using Syncd.Models.Users.Enum;

namespace Syncd.Models.Network.Payload
{
    public class JoinRoomPayload
    {
        public int userId { get; set; }
        public string name { get; set; }
        public UserRights myRights { get; set; }
        public string currentYoutubeVideoId { get; set; }
        public string currentYoutubeVideoTitle { get; set; }
        public List<PlaylistObject> playlist { get; set; }
        public VideoState currentVideoState { get; set; }
        public double videoTimeSeconds { get; set; }
        public List<User> userList { get; set; }
    }
}
