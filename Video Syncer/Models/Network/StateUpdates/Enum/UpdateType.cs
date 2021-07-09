﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncd.Models.Network.StateUpdates.Enum
{
    public enum UpdateType
    {
        UserListUpdate = 1,
        PlaylistUpdate = 2,
        VideoUpdate = 3,
        RedirectToPage = 4,
        AdminLogMessage = 5
    }
}
