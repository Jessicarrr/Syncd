using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Network.StateUpdates.Enum;

namespace Video_Syncer.Models.Network.StateUpdates.Interface
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
