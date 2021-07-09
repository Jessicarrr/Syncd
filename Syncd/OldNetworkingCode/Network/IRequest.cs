using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
{
    public interface IRequest
    {
        int userId
        {
            get;
            set;
        }

        string roomId
        {
            get;
            set;
        }
    }

    public interface ICallback
    {
        bool success
        {
            get;
            set;
        }
    }
}
