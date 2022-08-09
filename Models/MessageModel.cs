using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Models
{
    public class MessageModel
    {
        public byte[] RTFData { get; set; }
        public UserModel Sender { get; set; }
        public UserModel DestinationUser { get; set; }
        public DateTime MessageDate { get; set; }
        public MessageModel(byte[] rtfData, UserModel sender, UserModel destinationUser, DateTime messageDate)
        {
            RTFData = rtfData;
            Sender = sender;
            DestinationUser = destinationUser;
            MessageDate = messageDate;
        }
    }
}
