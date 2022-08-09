using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using ChattingHub.Helper.Extensions;
using DataBaseCMD;

namespace ChattingHub.Hubs
{
    public class ChatHub : Hub
    {
        public static DataModel Data { get; } = new DataModel();
        private static List<UserModel> _previouslyConnectedUsers = new List<UserModel>();
        private static int _userCount = -1;
        private DBCommands _dbCommands;

        public ChatHub()
        {
            _dbCommands = new DBCommands();
            LoadIntervalsAndMessages();
        }
        private void SendNewMessages(IHubContext<ChatHub> hub)
        {
            var currentMessage = Data.Messages.LastOrDefault();
            if (currentMessage.DestinationUser != null)
            {
                hub.Clients.Client(currentMessage.DestinationUser.ConnectionID).SendAsync("ReceiveMessages", Data.Messages);
                hub.Clients.Client(currentMessage.Sender.ConnectionID).SendAsync("ReceiveMessages", Data.Messages);
            }
            else
            {
                hub.Clients.All.SendAsync("ReceiveMessages", Data.Messages);
            }
        }

        public void SendPreviousPublicMessages(UnLoadedMessagesIntervalModel unLoadedMessagesInterval)
        {
            var previousMessages = _dbCommands.GetMessagesAfterInterval(unLoadedMessagesInterval);
            Clients.Caller.SendAsync("LoadPreviousMessages", previousMessages);
        }
        public void SendPreviousPrivateMessages(UnLoadedMessagesIntervalModel unLoadedMessagesInterval)
        {
            // We want to get messages that are directed to the selected user(User1) and is from (User2) or vice-versa between the time interval that is given.
            var filteredMessages = Data.Messages.Where(x => x.MessageDate >= unLoadedMessagesInterval.FirstDate && x.MessageDate <= unLoadedMessagesInterval.LastDate);
            var previousMessages = filteredMessages.Where(
            x => x.DestinationUser?.DisplayName == unLoadedMessagesInterval.User1.DisplayName
            && x.Sender?.DisplayName == unLoadedMessagesInterval.User2.DisplayName ||
                            x.Sender.DisplayName == unLoadedMessagesInterval.User1.DisplayName
                            && x.DestinationUser?.DisplayName == unLoadedMessagesInterval.User2.DisplayName).ToObservableCollection();

            Clients.Caller.SendAsync("LoadPreviousMessages", previousMessages);
        }
        private void SendUsers(IHubContext<ChatHub> hub) => hub.Clients.All.SendAsync("ReceiveUsers", Data.Users);
        private void SendUsers() => Clients.Others.SendAsync("ReceiveUsers", Data.Users);

        /// <summary>
        /// Stores the message data and sends them back out to the connected clients
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hub"></param>
        public void AddMessageData(MessageModel data, IHubContext<ChatHub> hub)
        {
            SaveMessage(data);
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
            //HotFixes
            TryRemoveIntervalDuplicates();
            _userCount++;
            if (!_previouslyConnectedUsers.Any(x => x.DisplayName == Data.Users[_userCount].DisplayName))
                _previouslyConnectedUsers.Add(Data.Users[_userCount]);

            var connectedUser = Data.Users[_userCount];
            connectedUser.ConnectionID = Context.ConnectionId;

            //All Intervals the user has
            List<UnLoadedMessagesIntervalModel> unLoadedMessagesIntervals = Data.UnLoadedMessagesIntervalModels;
            if (Data.UnLoadedMessagesIntervalModels.Count != 0)
                unLoadedMessagesIntervals = Data.UnLoadedMessagesIntervalModels.Where
                (x => x.User1?.DisplayName == connectedUser.DisplayName
                || x.User1 != null && x.User2.DisplayName == connectedUser.DisplayName || x.User1 == null).ToList();

            Clients.Caller.SendAsync("Connected", new DataModel(GetMessages(connectedUser, unLoadedMessagesIntervals), Data.Users, unLoadedMessagesIntervals));
            SendUsers();
            return base.OnConnectedAsync();
        }

        private void TryRemoveIntervalDuplicates()
        {
            List<UnLoadedMessagesIntervalModel> duplicates = new List<UnLoadedMessagesIntervalModel>();
            if (Data.UnLoadedMessagesIntervalModels.Count != 0)
            {
                for (int x = 0; x <= Data.UnLoadedMessagesIntervalModels.Count - 1; x++)
                {
                    UnLoadedMessagesIntervalModel root = Data.UnLoadedMessagesIntervalModels[x];
                    for (int y = x + 1; y <= Data.UnLoadedMessagesIntervalModels.Count - 1; y++)
                    {
                        UnLoadedMessagesIntervalModel model = Data.UnLoadedMessagesIntervalModels[y];
                        if (root.FirstDate == model.FirstDate && root.LastDate == model.LastDate) duplicates.Add(model);
                    }
                }
                foreach (var duplicate in duplicates) Data.UnLoadedMessagesIntervalModels.Remove(duplicate);
            }        
        }

        private ObservableCollection<MessageModel> GetMessages(UserModel currentUser, List<UnLoadedMessagesIntervalModel> unLoadedMessagesIntervals)
        {
            ObservableCollection<MessageModel> messagesNeeded = new ObservableCollection<MessageModel>();
            //All messages the user has
            var messages = Data.Messages.Where
                (x => x.DestinationUser?.DisplayName == currentUser.DisplayName
                || x.DestinationUser != null && x.Sender.DisplayName == currentUser.DisplayName || x.DestinationUser == null);
             
            var publicMessages = messages.TakePublicMessages();

            var publicIntervals = unLoadedMessagesIntervals.Where(x => x.User1 == null || x.User2 == null).ToList();
            publicIntervals.Sort((x, y) => DateTime.Compare(x.LastDate, y.LastDate));

            UnLoadedMessagesIntervalModel lastInterval = null;
            AddMessages(publicIntervals, publicMessages);

            foreach (UserModel user in _previouslyConnectedUsers.Where(x => x.DisplayName != currentUser.DisplayName))
            {
                var privateIntervals = unLoadedMessagesIntervals.TakePrivateIntervals(currentUser, user).ToList();
                var privateMessages = messages.TakePrivateMessages(currentUser, user);
                AddMessages(privateIntervals, privateMessages);
            }
            return messagesNeeded;
            void AddMessages(List<UnLoadedMessagesIntervalModel> intervals, IEnumerable<MessageModel> messages)
            {
                intervals.Sort((x, y) => DateTime.Compare(x.LastDate, y.LastDate));
                lastInterval = intervals.LastOrDefault();
                if (lastInterval != null)
                {
                    var unLoadedMessages = messages.Where(x => x.MessageDate > lastInterval.LastDate);
                    foreach (var message in unLoadedMessages) messagesNeeded.Add(message);
                }
                else foreach (var message in messages) messagesNeeded.Add(message);
            }
        }

        private void LoadIntervalsAndMessages()
        {
          Data.Messages = _dbCommands.GetPublicMessagesAfterLastInterval().ToObservableCollection();
          Data.UnLoadedMessagesIntervalModels = _dbCommands.GetFirst5PublicIntervals();
        }

        private void SaveMessage(MessageModel message)
        {
            _dbCommands.InsertMessage(message);
        }

        //Called when interval is sent from the user.
        public void ReduceMessages()
        {
           var newData = Data.Messages.Skip(10);
           Data.Messages = newData.ToObservableCollection();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _userCount--;
            var disconnectedConnection = Context.ConnectionId;
            foreach (UserModel user in Data.Users.ToList())
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
