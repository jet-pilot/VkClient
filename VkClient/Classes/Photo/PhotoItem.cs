using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace VkClient.Classes.Photo
{
    public class PhotoItem
    {
        public string pid { get; set; }
        public string owner_id { get; set; }
        public string aid { get; set; }
        public string src { get; set; }
        public string src_big { get; set; }
        public string src_xbig { get; set; }
    }
}
