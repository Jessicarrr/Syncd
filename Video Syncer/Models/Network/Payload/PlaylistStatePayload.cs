using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Playlist;

namespace Video_Syncer.Models.Network.Payload
{
    public class PlaylistStatePayload
    {
        public List<PlaylistObject> playlist { get; set; }
    }
}
