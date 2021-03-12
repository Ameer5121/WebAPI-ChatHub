using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public sealed class UserCredentials
    {

        public string UserName { get; set; }
        public string DecryptedPassword { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
    }
}
