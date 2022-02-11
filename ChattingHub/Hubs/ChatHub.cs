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
        public static DataModel Data { get; } = new DataModel();
        private void SendNewMessages(IHubContext<ChatHub> hub)
        {
            var currentMessage = Data.Messages.LastOrDefault();
            if (currentMessage.DestinationUser != null)
            {
                hub.Clients.Client(currentMessage.DestinationUser.ConnectionID).SendAsync("ReceiveMessages", Data.Messages);
                hub.Clients.Client(currentMessage.User.ConnectionID).SendAsync("ReceiveMessages", Data.Messages);
            }
            else
            {
                hub.Clients.All.SendAsync("ReceiveMessages", Data.Messages);
            }
        }

        public void SendPreviousPublicMessages(UnLoadedMessagesIntervalModel unLoadedMessagesInterval)
        {
            var previousMessages = Data.Messages.Where(x => x.DestinationUser == null &&
            x.MessageDate >= unLoadedMessagesInterval.FirstDate && x.MessageDate <= unLoadedMessagesInterval.LastDate);
            Clients.Caller.SendAsync("LoadPreviousMessages", previousMessages);
            Data.UnLoadedMessagesIntervalModels.Remove(unLoadedMessagesInterval);
        }
        public void SendPreviousPrivateMessages(UnLoadedMessagesIntervalModel unLoadedMessagesInterval)
        {
            var previousMessages = Data.Messages.Where(x => x.MessageDate >= unLoadedMessagesInterval.FirstDate && x.MessageDate <= unLoadedMessagesInterval.LastDate
            && x.DestinationUser?.DisplayName == unLoadedMessagesInterval.To.DisplayName
                            || x.User.DisplayName == unLoadedMessagesInterval.To.DisplayName
                            && x.DestinationUser?.DisplayName == unLoadedMessagesInterval.From.DisplayName).ToObservableCollection();

            Clients.Caller.SendAsync("LoadPreviousMessages", previousMessages);
            Data.UnLoadedMessagesIntervalModels.Remove(unLoadedMessagesInterval);
        }
        private void SendUsers(IHubContext<ChatHub> hub)
        {
            hub.Clients.All.SendAsync("ReceiveUsers", Data.Users);
        }
        private void SendUsers()
        {
            Clients.Others.SendAsync("ReceiveUsers", Data.Users);
        }
        /// <summary>
        /// Stores the message data and sends them back out to the connected clients
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hub"></param>
        public void AddMessageData(MessageModel data, IHubContext<ChatHub> hub)
        {
            Data.Messages.Add(data);
            SendNewMessages(hub);
        }

        public void UpdateImage(ImageUploaderModel profileImageDataModel, IHubContext<ChatHub> hub)
        {
            var userModel = Data.Users.Single(x => x.ConnectionID == profileImageDataModel.Uploader.ConnectionID);
            userModel.ProfilePicture = profileImageDataModel.Link;
            SendUsers(hub);
        }
        public void UpdateName(NameChangeModel nameChangeModel, IHubContext<ChatHub> hub)
        {
            var userModel = Data.Users.Single(x => x.ConnectionID == nameChangeModel.User.ConnectionID);
            userModel.DisplayName = nameChangeModel.NewName;
            SendUsers(hub);
        }

        public override Task OnConnectedAsync()
        {
            var connectedUser = Data.Users.LastOrDefault();
            connectedUser.ConnectionID = Context.ConnectionId;

            //Filter out messages we want to send
            var messages = Data.Messages.Where
                (x => x.DestinationUser?.DisplayName == connectedUser.DisplayName
                || x.DestinationUser != null && x.User.DisplayName == connectedUser.DisplayName || x.DestinationUser == null).ToObservableCollection();
            if (messages.Count >= 10) messages = messages.Skip(messages.Count - 5).ToObservableCollection();

            List<UnLoadedMessagesIntervalModel> unLoadedMessagesIntervals = Data.UnLoadedMessagesIntervalModels;
              if(Data.UnLoadedMessagesIntervalModels.Count != 0)
                unLoadedMessagesIntervals = Data.UnLoadedMessagesIntervalModels.Where
                (x => x.To?.DisplayName == connectedUser.DisplayName
                || x.To != null && x.From.DisplayName == connectedUser.DisplayName || x.To == null).ToList();


            Clients.Caller.SendAsync("Connected", new DataModel(messages, Data.Users, unLoadedMessagesIntervals));
            SendUsers();
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var disconnectedConnection = Context.ConnectionId;
            foreach (UserModel user in Data.Users)
            {
                if (user.ConnectionID == disconnectedConnection)
                {
                    Data.Users.Remove(user);
                    break;
                }
            }
            SendUsers();            
            return base.OnDisconnectedAsync(exception);
        }
    }
}
