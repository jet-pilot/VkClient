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
        public ProfileItemList Profiles { get; private set; }

        private VkTools()
        {
            this.token = accessInfoStore.Load();

            this.refreshTimer = new Timer(this.RefreshTimerCallback, null, -1, -1);

            this.Feeds = new FeedItemList();
            this.Profiles = new ProfileItemList();
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
            this.ListFeedsCallback();
            this.ProfileCallback();
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
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/newsfeed.get.xml?uid={0}&filters=post&access_token={1}", token.uid, token.token));
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
            XElement newxmlFeeds = new XElement("response",
                from item in xmlFeeds.Element("items").Elements("item")
                join profile in xmlFeeds.Element("profiles").Elements("user")
                on (string)item.Element("source_id") equals
                    (string)profile.Element("uid")
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
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/getProfiles.xml?uid={0}&fields=first_name,last_name,photo&access_token={1}", token.uid, token.token));
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
            var items = from profile in xmlProfiles.Element("response").Elements("user")
                        select new ProfileItem
                        {
                            uid=profile.Element("uid").Value,
                            name = profile.Element("last_name").Value + " " + profile.Element("first_name").Value,
                            photo = profile.Element("photo_medium").Value
                        };
            this.Profiles.AddRange(items);
            this.OnFeedChanged();
        }

        #endregion
    }
}
