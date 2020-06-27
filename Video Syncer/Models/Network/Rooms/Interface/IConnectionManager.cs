using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc.Async;
using Video_Syncer.Models.Network.StateUpdates.Interface;

namespace Video_Syncer.Models.Network.Rooms.Interface
{
    public interface IConnectionManager
    {
        public Task SendUpdateToAll(List<User> userList, IUpdate update, CancellationToken cancellationToken);
        public Task SendUpdateToAllExcept(List<User> userList, User user, IUpdate update, CancellationToken cancellationToken);
    }
}
