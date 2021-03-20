using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Microsoft.Extensions.Logging;

namespace ChattingHub.Hubs
{
    public class ChatHub : Hub
    {
        private static DataModel _usersAndMessages = new DataModel();
  
        private void SendMessages(IHubContext<ChatHub> hub)
        {
            hub.Clients.All.SendAsync("ReceiveData", _usersAndMessages);
        }
        public void AddUserData(UserModel data)
        {
            _usersAndMessages.Users.Add(data);
        }
        public void AddMessageData(MessageModel data, IHubContext<ChatHub> hub)
        {
            _usersAndMessages.Messages.Add(data);
            SendMessages(hub);
        }

        public override Task OnConnectedAsync()
        {
            _usersAndMessages.Users.LastOrDefault().ConnectionID = Context.ConnectionId;
            Clients.Caller.SendAsync("Connected", _usersAndMessages);
            Clients.Others.SendAsync("ReceiveData", _usersAndMessages);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var disconnectedConnection = Context.ConnectionId;
            foreach(UserModel user in _usersAndMessages.Users)
            {
                if (user.ConnectionID == disconnectedConnection)
                {
                    _usersAndMessages.Users.Remove(user);
                    break;
                }
            }
            Clients.All.SendAsync("ReceiveData", _usersAndMessages);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
