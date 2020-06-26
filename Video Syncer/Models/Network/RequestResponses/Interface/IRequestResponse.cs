using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Network.RequestResponses.Enum;

namespace Video_Syncer.Models.Network.RequestResponses.Interface
{
    interface IRequestResponse
    {
        RequestType requestType
        {
            get;
            set;
        }
        bool success
        {
            get;
            set;
        }
        Object payload
        {
            get;
            set;
        }
    }
}
