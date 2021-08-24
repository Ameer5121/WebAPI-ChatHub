using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using ChattingHub.Helper.Extensions;

namespace ChattingHub.Hubs
{
    public class ChatHub : Hub
    {
        private static DataModel _usersAndMessages = new DataModel();

        private void SendMessages(IHubContext<ChatHub> hub)
        {
            var currentMessage = _usersAndMessages.Messages.LastOrDefault();
            if (currentMessage.DestinationUser != null)
            {
                hub.Clients.Client(currentMessage.DestinationUser.ConnectionID).SendAsync("ReceiveMessages", _usersAndMessages.Messages);
                hub.Clients.Client(currentMessage.User.ConnectionID).SendAsync("ReceiveMessages", _usersAndMessages.Messages);
            }
            else
            {
                hub.Clients.All.SendAsync("ReceiveMessages", _usersAndMessages.Messages);
            }
        }

        private void SendUsers(IHubContext<ChatHub> hub)
        {
            hub.Clients.All.SendAsync("ReceiveUsers", _usersAndMessages.Users);
        }
        private void SendUsers()
        {
            Clients.Others.SendAsync("ReceiveUsers", _usersAndMessages.Users);
        }
        public void AddUserData(UserModel data)
        {
            _usersAndMessages.Users.Add(data);
        }
        /// <summary>
        /// Stores the message data and sends them back out to the connected clients
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hub"></param>
        public void AddMessageData(MessageModel data, IHubContext<ChatHub> hub)
        {
            _usersAndMessages.Messages.Add(data);
            SendMessages(hub);
        }

        public UserModel UpdateImage(string imageLink, UserModel user, IHubContext<ChatHub> hub)
        {
            var userModel = _usersAndMessages.Users.Single(x => x.ConnectionID == user.ConnectionID);
            userModel.ProfilePicture = imageLink;
            SendUsers(hub);
            return userModel;
        }

        public override Task OnConnectedAsync()
        {
            var connectedUser = _usersAndMessages.Users.LastOrDefault();
            connectedUser.ConnectionID = Context.ConnectionId; 

            Clients.Caller.SendAsync("Connected", new DataModel
            {
                Users = _usersAndMessages.Users,
                Messages = _usersAndMessages.Messages.Where
                (x => x.DestinationUser == connectedUser
                || x.DestinationUser == null).ToObservableCollection()
            });
            SendUsers();
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var disconnectedConnection = Context.ConnectionId;
            foreach (UserModel user in _usersAndMessages.Users)
            {
                if (user.ConnectionID == disconnectedConnection)
                {
                    _usersAndMessages.Users.Remove(user);
                    break;
                }
            }
            SendUsers();            
            return base.OnDisconnectedAsync(exception);
        }
    }
}
