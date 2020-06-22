using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Playlist;

namespace Video_Syncer.Models.Network
{
    public class RearrangePlaylistRequest : IRequest
    {
        public int userId { get; set; }
        public string roomId { get; set; }
        public string onTopId { get; set; }
        public string onBottomId { get; set; }
    }

    public class RearrangePlaylistCallback : ICallback
    {
        public bool success { get; set; }
        public List<PlaylistObject> newPlaylist { get; set; }
    }
}
