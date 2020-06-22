using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
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
