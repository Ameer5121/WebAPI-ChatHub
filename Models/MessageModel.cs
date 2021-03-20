using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Models
{
    public class MessageModel
    {
      public string Message { get; set; }
      public UserModel User { get; set; }
      public UserModel DestinationUser { get; set; }
    }
}
