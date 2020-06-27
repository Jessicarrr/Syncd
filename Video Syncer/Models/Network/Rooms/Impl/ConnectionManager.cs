using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Video_Syncer.Models.Network.Payload.StateUpdates;
using Video_Syncer.Models.Network.Rooms.Interface;
using Video_Syncer.Models.Network.StateUpdates.Interface;

namespace Video_Syncer.Models.Network.Rooms.Impl
{
    public class ConnectionManager : IConnectionManager
    {
        public async Task SendUpdateToAll(List<User> userList, IUpdate update, CancellationToken cancellationToken)
        {
            var dataToSend = new Byte[1024];
            var newString = JsonConvert.SerializeObject(update);
            dataToSend = System.Text.Encoding.UTF8.GetBytes(newString);

            foreach (User loopUser in userList)
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await loopUser.socket.SendAsync(new ArraySegment<byte>(dataToSend, 0, newString.Length),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task SendUpdateToAllExcept(User user, List<User> userList, IUpdate update, CancellationToken cancellationToken)
        {
            var dataToSend = new Byte[1024];
            var newString = JsonConvert.SerializeObject(update);
            dataToSend = System.Text.Encoding.UTF8.GetBytes(newString);

            foreach(User loopUser in userList) 
            {
                if(loopUser == user)
                {
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await loopUser.socket.SendAsync(new ArraySegment<byte>(dataToSend, 0, newString.Length),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
