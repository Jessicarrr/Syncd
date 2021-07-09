using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncd.Models.Network.StateUpdates.Enum;
using Syncd.Models.Network.StateUpdates.Interface;

namespace Syncd.Models.Network.StateUpdates.Impl
{
    public class RoomDataUpdate : IUpdate
    {
        public UpdateType updateType { get; set; }
        public object payload { get; set; }
    }
}
