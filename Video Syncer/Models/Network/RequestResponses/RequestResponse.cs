using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
{
    public class RequestResponse : IRequestResponse
    {
        public RequestType requestType { get; set; }
        public bool success { get; set; }
        public object payload { get; set; }
    }
}
