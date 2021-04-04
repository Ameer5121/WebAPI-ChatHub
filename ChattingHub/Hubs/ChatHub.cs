﻿using Microsoft.AspNetCore.SignalR;
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
            var currentMessage = _usersAndMessages.Messages.LastOrDefault();
            if (currentMessage.DestinationUser != null)
            {
                hub.Clients.Client(currentMessage.DestinationUser.ConnectionID).SendAsync("ReceiveData", _usersAndMessages);
                hub.Clients.Client(currentMessage.User.ConnectionID).SendAsync("ReceiveData", _usersAndMessages);
            }
            else
            {
                hub.Clients.All.SendAsync("ReceiveData", _usersAndMessages);
            }        
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