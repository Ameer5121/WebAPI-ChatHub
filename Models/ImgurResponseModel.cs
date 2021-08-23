using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class ImgurResponseModel
    {
        public ImageData Data { get; set; }

        public class ImageData
        {
            public string Link { get; set; }
        }
    }
}
