using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncd.Models.Playlist;

namespace Syncd.Models.Network.Payload
{
    public class PlaylistStatePayload
    {
        public List<PlaylistObject> playlist { get; set; }
    }
}
