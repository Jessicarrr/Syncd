using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
{
    public class TimeChangeRequest : IRequest
    {
        public string roomId { get; set; }
        public int userId { get; set; }
        public double videoTimeSeconds { get; set; }
    }

    public class TimeChangeRequestCallback : ICallback
    {
        public Boolean success { get; set; }
    }
}
