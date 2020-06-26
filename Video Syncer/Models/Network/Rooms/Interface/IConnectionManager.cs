using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Network.StateUpdates.Interface;

namespace Video_Syncer.Models.Network.Rooms.Interface
{
    public interface IConnectionManager
    {
        Room Room
        {
            get;
        }

        public void SendUpdateToAll(IUpdate update);
        public void SendUpdateToAllExcept(User user, IUpdate update);
    }
}
