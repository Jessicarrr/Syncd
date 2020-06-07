using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
{
    public class MakeAdminRequest : IRequest
    {
        public int userId { get; set; }
        public string roomId { get; set; }
        public int userIdToMakeAdmin { get; set; }
    }

    public class KickRequest : IRequest
    {
        public int userId { get; set; }
        public string roomId { get; set; }
        public int userIdToKick { get; set; }
    }

    public class BanRequest : IRequest
    {
        public int userId { get; set; }
        public string roomId { get; set; }

        public int userIdToBan { get; set; }
    }

    public class MakeAdminCallback : ICallback
    {
        public bool success { get; set; }
    }

    public class KickCallback : ICallback
    {
        public bool success { get; set; }
    }

    public class BanCallback : ICallback
    {
        public bool success { get; set; }
    }
}
