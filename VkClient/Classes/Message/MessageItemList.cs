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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using VkClient.Classes.Auth;

namespace VkClient.Classes.Message
{
    public class MessageItemList //: ObservableCollection<MessageItem>
    {
        private List<MessageItem> messages = new List<MessageItem>();
        
        private object syncRoot = new Object();

        //public MessageItemList()
        //{
        //    ObservableCollection<MessageItem> ololo = new ObservableCollection<MessageItem>();
        //    //Add(new MessageItem("Anne Wallace", "Did you have fun on your trip?", "It's awesome that you got the chance to ...", "1:34p", true));
        //    //Add(new MessageItem("Jeff Smith", "Awesome job!", "I just went through the code you submitted ... ", "12:22p", true));
        //    //Add(new MessageItem("Adriana Giorgi", "(no subject)", "Hello there. It's been really long since we ...", "11:11a", true));
        //    //Add(new MessageItem("Richard Carey", "RE: Welcome aboard!", "Hey, Jose, Congratulations on your offer! Can't wait to ...", "10:34a", true));
        //    //Add(new MessageItem("Bruno Denuit", "Where are we going for lunch today?", "I vote for the thai place across the street ...", "9:48a", false));
        //    //Add(new MessageItem("Dave Barnett", "The supply room seems to be missing some ...", "Okay, who's the funny guy who thought it ...", "Thu", false));
        //    //Add(new MessageItem("Gregory Weber", "Please send me the documents regarding ...", "I'm going to need them so I can schedule ...", "Thu", false));
        //}

        public ObservableCollection<MessageItem> GetMessages()
        {
            List<MessageItem> items;

            lock (this.syncRoot)
            {
                items = new List<MessageItem>(this.messages);
            }

            return new ObservableCollection<MessageItem>(items);
        }

        public static void ListDialogsCallback()
        {
            accessInfoBag token = accessInfoStore.Load();
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/messages.get.xml?filters=4&preview_length=50&access_token={0}", token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponsePrivateMessage), web);
        }

        private static void ResponsePrivateMessage(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringmessages = responseReader.ReadToEnd();

            XElement xmlMessages = XElement.Parse(responseStringmessages);
            var items = from message in xmlMessages.Descendants("message") select new MessageItem(message.Element("uid").Value, message.Element("title").Value, message.Element("body").Value, message.Element("date").Value, (message.Element("read_state").Value == "0") ? true : false);
            messages.AddRange(items);
        }

        public ObservableCollection<MessageItem> GetItems()
        {
            List<MessageItem> items;

            lock (this.syncRoot)
            {
                items = new List<MessageItem>(this.messages);
            }

            return new ObservableCollection<MessageItem>(items);
        }

        public void AddRange(IEnumerable<MessageItem> newItems)
        {
            lock (this.syncRoot)
            {
                this.messages.AddRange(newItems);
            }
        }
    }
}
