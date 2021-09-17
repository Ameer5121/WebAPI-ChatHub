using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class ImageUploadDataModel
    {
        public string Base64ImageData { get; set; }
        public UserModel Uploader { get; set; }
        public ImageUploadDataModel(string base64ImageData, UserModel uploader)
        {
            Base64ImageData = base64ImageData;
            Uploader = uploader;
        }
    }
}
