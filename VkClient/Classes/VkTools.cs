using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using VkClient.Classes.Auth;
using VkClient.Classes.feed;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using VkClient.Classes.Profile;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using VkClient.Classes.Photo;
using VkClient.Classes.Message;
using System.Collections.ObjectModel;

namespace VkClient.Classes
{
    public class VkTools
    {
        #region Singleton

        private static volatile VkTools instance;

        private static object instanceSyncRoot = new Object();

        public static VkTools Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceSyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new VkTools();
                        }
                    }
                }

                return instance;
            }
        }

        #endregion

        private volatile bool active;

        public EventHandler ActiveChanged;
        public event EventHandler FeedChanged;
        public event EventHandler ProfileChanged;

        private accessInfoBag token;

        private static readonly int RefreshInterval = 30;
        private Timer refreshTimer;

        public FeedItemList Feeds { get; private set; }
        public ProfileItem user { get; private set; }
        public List<PhotoItem> FeedPhotos { get; private set; }
        public MessageItemList Messages { get; private set; }

        private VkTools()
        {
            this.token = accessInfoStore.Load();

            this.refreshTimer = new Timer(this.RefreshTimerCallback, null, -1, -1);

            this.Feeds = new FeedItemList();
            this.user = new ProfileItem();
        }

        public bool Active
        {
            get { return this.active; }
        }

        private void OnActiveChanged()
        {
            var handler = this.ActiveChanged;

            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public void Start(accessInfoBag accessInfo)
        {
            if (this.active)
            {
                return;
            }

            if (accessInfo == null)
            {
                return;
            }
            this.StartTimer();
        }

        #region Timer
        private void StartTimer()
        {
            this.refreshTimer.Change(0, RefreshInterval * 1000);
            this.active = true;
            this.OnActiveChanged();
        }

        private void StopTimer()
        {
            this.active = false;
            this.refreshTimer.Change(-1, -1);
            this.OnActiveChanged();
        }

        private void RefreshTimerCallback(object state)
        {
            this.ProfileCallback();
            this.ListFeedsCallback();
        }
        #endregion

        #region новости
        private void OnFeedChanged()
        {
            var handler = this.FeedChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void ListFeedsCallback()
        {
            if (token == null) { return; }
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/newsfeed.get.xml?uid={0}&access_token={1}", token.uid, token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponsePrepare), web);
        }

        private void ResponsePrepare(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringfeed = responseReader.ReadToEnd();

            XElement xmlFeeds = XElement.Parse(responseStringfeed);

            try
            {
                XElement newxmlFeeds = new XElement("posts",
                from item in xmlFeeds.Element("items").Elements("item")
                join profile in xmlFeeds.Element("profiles").Elements("user")
                on (string)item.Element("source_id") equals (string)profile.Element("uid")
                where (string)item.Element("type") == "post"
                select new XElement("feed",
                    new XElement("first_name", profile.Element("first_name").Value + " " + profile.Element("last_name").Value),
                    new XElement("photo", profile.Element("photo").Value),
                    new XElement("text", item.Element("text").Value)
                    )
                    );

                var items = from feed in newxmlFeeds.Descendants("feed")
                            select new FeedItem
                            {
                                first_name = feed.Element("first_name").Value,
                                text = feed.Element("text").Value,
                                photo = feed.Element("photo").Value
                            };
                this.Feeds.AddRange(items);
                this.OnFeedChanged();
            }
            catch
            {
                MessageBox.Show("Новости не загрузились");
            }
            this.OnFeedChanged();
            //progressBar1.IsIndeterminate = false;


        }
        #endregion

        #region профиль

        private void OnProfileChanged()
        {
            var handler = this.ProfileChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void ProfileCallback()
        {
            if (token == null) { return; }
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/getProfiles.xml?uid={0}&fields=first_name,last_name,photo_medium&access_token={1}", token.uid, token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponseProfile), web);
        }

        private void ResponseProfile(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringProfile = responseReader.ReadToEnd();
            XElement xmlProfiles = XElement.Parse(responseStringProfile);

            
            user.first_name = xmlProfiles.Element("user").Element("first_name").Value;
            user.last_name = xmlProfiles.Element("user").Element("last_name").Value;
            user.photo = xmlProfiles.Element("user").Element("photo_medium").Value;
            user.uid = xmlProfiles.Element("user").Element("uid").Value;

            this.OnProfileChanged();
        }

        #endregion

        //#region личка

        //private void OnMessagesChanged()
        //{
        //    var handler = this.MessagesChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new EventArgs());
        //    }
        //}

        //private void ListDialogsCallback()
        //{
        //    if (token == null) { return; }
        //    HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/messages.get.xml?filters=4&preview_length=50&access_token={0}", token.token));
        //    web.Method = "POST";
        //    web.ContentType = "application/x-www-form-urlencoded";
        //    web.BeginGetResponse(new AsyncCallback(ResponsePrivateMessage), web);
        //}

        //private void ResponsePrivateMessage(IAsyncResult e)
        //{
        //    HttpWebRequest request = (HttpWebRequest)e.AsyncState;
        //    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

        //    StreamReader responseReader = new StreamReader(response.GetResponseStream());

        //    string responseStringmessages = responseReader.ReadToEnd();

        //    XElement xmlMessages = XElement.Parse(responseStringmessages);
        //    //var items = from message in xmlMessages.Descendants("response")
        //    //            select new MessageItem
        //    //            {
        //    //                Unread = ((message.Element("read_state").Value == "0") ? true : false),
        //    //                Sender = message.Element("uid").Value,
        //    //                Subject = message.Element("title").Value,
        //    //                Body = message.Element("body").Value,
        //    //                Time = message.Element("date").Value
        //    //            };
        //    //this.Messages.AddRange(items);
        //    this.OnMessagesChanged();
        //}
        //#endregion
    }
}
