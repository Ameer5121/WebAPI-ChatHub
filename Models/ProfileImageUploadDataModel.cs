using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class ProfileImageUploadDataModel
    {
        public UserModel Uploader { get; set; }
        public string Link { get; set; }

        public ProfileImageUploadDataModel(UserModel uploader, string link)
        {
            Uploader = uploader;
            Link = link;
        }
    }
}
