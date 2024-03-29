﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc.Async;
using Syncd.Models.Network.StateUpdates.Interface;

namespace Syncd.Models.Network.Rooms.Interface
{
    public interface IConnectionManager
    {
        public Task<int> CheckAndRemoveDisconnectedUsers(Room room, CancellationToken token);
        public Task SendUpdateToAll(Room room, IUpdate update, CancellationToken cancellationToken);
        public Task SendUpdateToAllExcept(User user, Room room, IUpdate update, CancellationToken cancellationToken);
        public Task SendUpdateToUser(User user, Room room, IUpdate update, CancellationToken token);
        public Task SendUpdateToAdmins(Room room, IUpdate update, CancellationToken cancellationToken);
    }
}
