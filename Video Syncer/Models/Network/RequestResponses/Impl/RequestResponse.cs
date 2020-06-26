using Video_Syncer.Models.Network.RequestResponses.Enum;
using Video_Syncer.Models.Network.RequestResponses.Interface;

namespace Video_Syncer.Models.Network.RequestResponses.Impl
{
    public class RequestResponse : IRequestResponse
    {
        public RequestType requestType { get; set; }
        public bool success { get; set; }
        public object payload { get; set; }
    }
}
