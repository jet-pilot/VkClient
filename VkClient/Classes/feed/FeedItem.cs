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
using VkClient.Classes.Photo;
using System.Collections.Generic;

namespace VkClient.Classes.feed
{
    public class FeedItem
    {
        //public string type { get; set; }
        //public string source_id { get; set; }
        //public DateTime date { get; set; }
        public string post_id { get; set; }
        public string text { get; set; }
        public string first_name { get; set; }
        public string photo { get; set; }
    }


    public class FeedPhoto
    {
        public string source_id { get; set; }
        public DateTime date { get; set; }
        public List<PhotoItem> photos { get; set; }
    }
}
