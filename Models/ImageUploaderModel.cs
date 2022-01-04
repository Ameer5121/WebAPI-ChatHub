using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class ImageUploaderModel
    {
        public UserModel Uploader { get; set; }
        public string Link { get; set; }

        public ImageUploaderModel(UserModel uploader, string link)
        {
            Uploader = uploader;
            Link = link;
        }
    }
}
