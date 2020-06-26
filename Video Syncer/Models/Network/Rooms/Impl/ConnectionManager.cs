using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Network.Rooms.Interface;
using Video_Syncer.Models.Network.StateUpdates.Interface;

namespace Video_Syncer.Models.Network.Rooms.Impl
{
    public class ConnectionManager : IConnectionManager
    {
        public Room Room { get; }

        public ConnectionManager(Room room)
        {
            Room = room;
        }

        public void SendUpdateToAll(IUpdate update)
        {
            throw new NotImplementedException();
        }

        public void SendUpdateToAllExcept(User user, IUpdate update)
        {
            throw new NotImplementedException();
        }
    }
}
