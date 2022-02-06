using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Models
{
    public class DataModel
    {
        public ObservableCollection<MessageModel> Messages { get; set; } 
        public ObservableCollection<UserModel> Users { get; set; }
        public List<UnLoadedMessagesIntervalModel> UnLoadedMessagesIntervalModels { get; set; }

        public DataModel(ObservableCollection<MessageModel> messages, ObservableCollection<UserModel> users, List<UnLoadedMessagesIntervalModel> unLoadedMessagesIntervalModels)
        {
            Messages = messages;
            Users = users;
            UnLoadedMessagesIntervalModels = unLoadedMessagesIntervalModels;
        }
        public DataModel() 
        {
            Messages = new ObservableCollection<MessageModel>();
            Users = new ObservableCollection<UserModel>();
            UnLoadedMessagesIntervalModels = new List<UnLoadedMessagesIntervalModel>();
        }
        
    }
}
