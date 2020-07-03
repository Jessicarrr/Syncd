using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Video_Syncer.Models.Network.RequestResponses.Enum;

namespace Video_Syncer.Models.Network.Payload
{
    public class UserListPayload
    {
        public List<User> userList { get; set; }
    }
}
