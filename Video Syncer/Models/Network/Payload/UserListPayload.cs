using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncd.Models.Network.RequestResponses.Enum;

namespace Syncd.Models.Network.Payload
{
    public class UserListPayload
    {
        public List<User> userList { get; set; }
    }
}
