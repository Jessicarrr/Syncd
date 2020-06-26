using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network.StateUpdates
{
    public interface IUpdate
    {
        UpdateType updateType
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
