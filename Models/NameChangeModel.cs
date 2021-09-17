using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class NameChangeModel
    {       
        public UserModel User { get; set; }
        public string NewName { get; set; }
    }
}
