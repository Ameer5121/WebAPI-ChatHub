using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class UnLoadedMessagesIntervalModel
    {
        public DateTime FirstDate { get; }
        public DateTime LastDate { get; }
        public UserModel User1 { get; }
        public UserModel User2 { get; }

        [JsonConstructor]
        public UnLoadedMessagesIntervalModel(DateTime firstDate, DateTime lastDate, UserModel user1 = null, UserModel user2 = null)
        {
            FirstDate = firstDate;
            LastDate = lastDate;
            User1 = user1;
            User2 = user2;
        }
    }
}
