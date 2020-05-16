﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.Models.Network
{
    public class ChangeUsernameRequest
    {
        public int userId { get; set; }
        public string roomId { get; set; }
        public string newName { get; set; }
    }

    public class ChangeUsernameCallback
    {
        public Boolean success { get; set; }
    }
}
