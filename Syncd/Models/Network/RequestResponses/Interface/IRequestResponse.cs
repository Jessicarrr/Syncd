using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncd.Models.Network.RequestResponses.Enum;

namespace Syncd.Models.Network.RequestResponses.Interface
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
