using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncd.Models.Network.Payload.StateUpdates
{
    public class UserListStateUpdate
    {
        public List<User> userList { get; set; } = new List<User>();
    }
}
