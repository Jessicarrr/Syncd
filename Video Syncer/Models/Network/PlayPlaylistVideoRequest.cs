using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
{
    public class PlayPlaylistVideoRequest
    {
        public string roomId { get; set; }
        public int userId { get; set; }
        public string playlistItemId { get; set; }
    }

    public class PlayPlaylistVideoCallback
    {
        public Boolean success { get; set; }
    }
}
