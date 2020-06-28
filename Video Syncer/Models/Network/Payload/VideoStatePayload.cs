using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network.Payload
{
    public class VideoStatePayload
    {
        public VideoState currentVideoState { get; set; }
        public double videoTimeSeconds { get; set; }
        public string currentYoutubeVideoId { get; set; }
        public string currentYoutubeVideoTitle { get; set; }
    }
}
