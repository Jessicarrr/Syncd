using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models
{
    public interface IRoomManagerSingleton
    {
        public Room CreateNewRoom();
        public void DestroyRoom(Room room);
        public Room GetRoom(string roomId);
    }
}
