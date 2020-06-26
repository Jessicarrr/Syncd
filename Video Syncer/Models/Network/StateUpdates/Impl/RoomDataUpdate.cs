using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Network.StateUpdates.Enum;
using Video_Syncer.Models.Network.StateUpdates.Interface;

namespace Video_Syncer.Models.Network.StateUpdates.Impl
{
    public class RoomDataUpdate : IUpdate
    {
        public UpdateType updateType { get; set; }
        public object payload { get; set; }
    }
}
