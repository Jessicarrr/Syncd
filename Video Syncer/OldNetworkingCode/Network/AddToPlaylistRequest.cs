using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
{
    public class AddToPlaylistRequest : IRequest
    {
        public int userId { get; set; }
        public string roomId { get; set; }
        public string youtubeVideoId { get; set; }

    }
}
