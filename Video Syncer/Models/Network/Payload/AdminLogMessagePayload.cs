using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncd.Models.Network.Payload
{
    public class AdminLogMessagePayload
    {
        public User user { get; set; }
        public string actionMessage { get; set; }
    }
}
