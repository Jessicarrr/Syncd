using Syncd.Models.Network.RequestResponses.Enum;
using Syncd.Models.Network.RequestResponses.Interface;

namespace Syncd.Models.Network.RequestResponses.Impl
{
    public class RequestResponse : IRequestResponse
    {
        public RequestType requestType { get; set; }
        public bool success { get; set; }
        public object payload { get; set; }
    }
}
