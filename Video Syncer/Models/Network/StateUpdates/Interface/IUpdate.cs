using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncd.Models.Network.StateUpdates.Enum;

namespace Syncd.Models.Network.StateUpdates.Interface
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
