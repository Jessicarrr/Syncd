using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models
{
    public enum VideoState
    {
        Unstarted = -1,
        Ended = 0,
        Playing = 1,
        Paused = 2,
        Buffering = 3,
        VideoCued = 5,
        WaitForBuffering = 50,
        None = -100
    }
}
