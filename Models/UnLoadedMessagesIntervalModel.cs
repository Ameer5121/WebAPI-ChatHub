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
        public UserModel To { get; }
        public UserModel From { get; }

        [JsonConstructor]
        public UnLoadedMessagesIntervalModel(DateTime firstDate, DateTime lastDate, UserModel to, UserModel from)
        {
            FirstDate = firstDate;
            LastDate = lastDate;
            To = to;
            From = from;
        }
    }
}
