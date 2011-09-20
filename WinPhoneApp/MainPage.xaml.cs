using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Phone.Controls;
using WinPhoneApp.Data.Feed;
using WinPhoneApp.Data.Photo;
using WinPhoneApp.Data;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data.Profile;
using Microsoft.Phone.Tasks;

namespace WinPhoneApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private FeedList fl = new FeedList();
        private MyProfile mp;

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Client.Instance.ActiveChanged += new EventHandler(ClientActiveChanged);

            this.UpdateUI();
        }

        private void ClientActiveChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() => { this.UpdateUI(); });
        }

        private void UpdateUI()
        {
            var started = Client.Instance.Active;
            if (!started) { NavigationService.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative)); }
            else
            {
                GetFeedList();
                ListFeedsCallback();

                GetMyProfile();
                ListProfileCallback();

                ListStatusCallback();
                //MessageSendRequest();

            }
        }

        private FeedList GetFeedList()
        {
            var tmp = (FeedList)this.Resources["FeedListData"];
            if (tmp != null)
            {
                return tmp;
            }
            else
            {
                if (fl != null)
                {
                    return fl;
                }
                else
                {
                    FeedPanel.DataContext = fl;
                    return fl;
                }
            }
        }

        #region получаем новости

        private void ListFeedsCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/newsfeed.get?uid={0}&filters=post&count=20&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponsePrepare), web);
            progressBar1.IsIndeterminate = true;
        }

        private void ResponsePrepare(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringfeed = responseReader.ReadToEnd();

            JObject o = JObject.Parse(responseStringfeed);
            JArray responseFeeds = (JArray)o["response"]["items"];
            JArray responseProfiles = (JArray)o["response"]["profiles"];
            //try
            //{
            foreach (var item in responseFeeds)
            {
                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)item["date"]));
                PhotoItemList pl = new PhotoItemList();
                LinkItemList ll = new LinkItemList();
                AudioItemList al = new AudioItemList();
                foreach (var user in responseProfiles)
                {
                    var attachments = item.SelectToken("attachments", false);
                    if ((int)user["uid"] == (int)item["source_id"])
                    {
                        string name = (string)user["first_name"] + " " + (string)user["last_name"];
                        string avatar = (string)user["photo"];
                        if (attachments != null)
                        {
                            foreach (var attachment in attachments)
                            {
                                switch ((string)attachment["type"])
                                {
                                    case "photo":
                                        {
                                            var image = attachment.SelectToken("photo", false);
                                            if (image != null)
                                            {
                                                pl.Add(new PhotoItem((int)image["pid"], (int)image["owner_id"], (string)image["src"], (string)image["src_big"]));
                                            }

                                            break;
                                        }
                                    case "link":
                                        {
                                            var link = attachment.SelectToken("link", false);
                                            if (link != null)
                                            {
                                                ll.Add(new LinkItem((string)link["url"], (string)link["title"], (string)link["description"], (string)link["image_src"]));
                                            }
                                            break;
                                        }
                                    case "audio":
                                        {
                                            var audio = attachment.SelectToken("audio", false);
                                            if (audio != null)
                                            {
                                                al.Add(new AudioItem(null, null, null, null, null, null));
                                            }
                                            break;
                                        }
                                }
                            }
                            fl.Add(new FeedItem(name, avatar, (string)item["text"], date, pl, ll,al));
                        }
                        else
                        {
                            fl.Add(new FeedItem(name, avatar, (string)item["text"], date));
                        }

                    }
                }
            }
            this.Dispatcher.BeginInvoke(() =>
                {
                    feedListBox.ItemsSource = fl;

                    progressBar1.IsIndeterminate = false;

                });
            //}
            //catch
            //{
            //    this.Dispatcher.BeginInvoke(() => { MessageBox.Show("Новости не загрузились"); progressBar1.IsIndeterminate = false; });
            //}

            #region старый вариант с xml
            //XElement xmlFeeds = XElement.Parse(responseStringfeed);

            //try
            //{
            //    XElement newxmlFeeds = new XElement("posts",
            //    from item in xmlFeeds.Element("items").Elements("item")
            //    join profile in xmlFeeds.Element("profiles").Elements("user")
            //    on (string)item.Element("source_id") equals (string)profile.Element("uid")
            //    select new XElement("feed",
            //        new XElement("author", profile.Element("first_name").Value + " " + profile.Element("last_name").Value),
            //        new XElement("avatar", profile.Element("photo").Value),
            //        new XElement("text", item.Element("text").Value),
            //        new XElement("date",item.Element("date").Value)
            //        )
            //        );

            //    var items = from feed in newxmlFeeds.Descendants("feed")
            //                select new FeedItem(feed.Element("author").Value, feed.Element("avatar").Value, feed.Element("text").Value, new DateTime(1970, 1, 1, 0, 0, 0));
            //    foreach (var item in items)
            //    {
            //        fl.Add(item);
            //    }
            //    this.Dispatcher.BeginInvoke(() => { feedListBox.ItemsSource = fl; progressBar1.IsIndeterminate = false; });
            //}
            //catch
            //{
            //    this.Dispatcher.BeginInvoke(() => { MessageBox.Show("Новости не загрузились"); progressBar1.IsIndeterminate = false; });
            //}
            #endregion


        }
        #endregion

        private MyProfile GetMyProfile()
        {
            var tmp = (MyProfile)this.Resources["MyProfileData"];
            if (tmp != null)
            {
                return tmp;
            }
            else
            {
                if (mp != null)
                {
                    return mp;
                }
                else
                {
                    mp = new MyProfile();
                    MyProfilePanel.DataContext = mp;
                    return mp;
                }
            }
        }

        #region получаем профиль

        private void ListProfileCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/getProfiles?fields=photo&uid={0}&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponsePrepareProfile), web);
            progressBar1.IsIndeterminate = true;
        }

        private void ResponsePrepareProfile(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringprofile = responseReader.ReadToEnd();

            try
            {
                JObject o = JObject.Parse(responseStringprofile);
                JArray responseArray = (JArray)o["response"];

                mp = new MyProfile((string)responseArray[0]["first_name"], (string)responseArray[0]["last_name"], (string)responseArray[0]["photo"]);
                this.Dispatcher.BeginInvoke(() =>
                    {
                        ImageSource image = new BitmapImage(new Uri(mp.Photo));
                        this.photo.Source = image;
                        this.LF_name.Text = mp.First_name + " " + mp.Last_name;
                        progressBar1.IsIndeterminate = false;
                    });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }


        }

        #endregion

        private void ListStatusCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/status.get?uid={0}&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareStatus), web);
            progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareStatus(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringStatus = responseReader.ReadToEnd();

            JObject o = JObject.Parse(responseStringStatus);
            //JArray responseArray = (JArray)o["response"];
            try
            {
                this.Dispatcher.BeginInvoke(() =>
                    {
                        this.Status.Text = (string)o["response"]["text"];
                    });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }

        private void Navigate_to_MessagePage(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MessagesPage.xaml", UriKind.Relative));
        }
        //private void MessageSendRequest()
        //{
        //    string requestString = string.Format("https://api.vkontakte.ru/method/messages.send?access_token={0}&uid=9299666&message={1}", Client.Instance.Access_token.token, "Трололо");
        //    HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
        //    web.Method = "POST";
        //    web.ContentType = "application/x-www-form-urlencoded";
        //    web.BeginGetResponse(new AsyncCallback(MessageSendResponce), web);
        //    this.progressBar1.IsIndeterminate = true;
        //}
        //private void MessageSendResponce(IAsyncResult e)
        //{
        //    HttpWebRequest request = (HttpWebRequest)e.AsyncState;
        //    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

        //    StreamReader responseReader = new StreamReader(response.GetResponseStream());

        //    string responseStringStatus = responseReader.ReadToEnd();

        //    JObject o = JObject.Parse(responseStringStatus);
        //}


        private void Navigate_to_FriendListPage(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FriendListPage.xaml", UriKind.Relative));
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var wbt = new WebBrowserTask();
            HyperlinkButton btn = (HyperlinkButton)e.OriginalSource;
            wbt.URL = (string)btn.NavigateUri.AbsoluteUri;
            wbt.Show();
        }
    }
}