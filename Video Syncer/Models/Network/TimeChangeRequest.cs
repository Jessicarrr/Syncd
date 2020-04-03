using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
{
    public class TimeChangeRequest
    {
        public string roomId { get; set; }
        public int userId { get; set; }
        public double videoTimeSeconds { get; set; }
    }

    public class TimeChangeRequestCallback
    {
        public Boolean success { get; set; }
    }
}
