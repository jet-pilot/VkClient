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
using System.Xml.Linq;
using Microsoft.Phone.Controls;
using WinPhoneApp.Data.Feed;
using WinPhoneApp.Data.Photo;
using WinPhoneApp.Data;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data.Profile;

namespace WinPhoneApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private FeedList fl = new FeedList();
        private PhotoItemList pl = new PhotoItemList();
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

                GetFeedPhotoList();
                ListFeedPhotosCallback();

                GetMyProfile();
                ListProfileCallback();

                ListStatusCallback();

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
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/newsfeed.get.xml?uid={0}&filters=post&count=20&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
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

            XElement xmlFeeds = XElement.Parse(responseStringfeed);

            try
            {
                XElement newxmlFeeds = new XElement("posts",
                from item in xmlFeeds.Element("items").Elements("item")
                join profile in xmlFeeds.Element("profiles").Elements("user")
                on (string)item.Element("source_id") equals (string)profile.Element("uid")
                select new XElement("feed",
                    new XElement("author", profile.Element("first_name").Value + " " + profile.Element("last_name").Value),
                    new XElement("avatar", profile.Element("photo").Value),
                    new XElement("text", item.Element("text").Value)
                    )
                    );

                var items = from feed in newxmlFeeds.Descendants("feed")
                            select new FeedItem(feed.Element("author").Value, feed.Element("avatar").Value, feed.Element("text").Value);
                foreach (var item in items)
                {
                    fl.Add(item);
                }
                this.Dispatcher.BeginInvoke(() => { feedListBox.ItemsSource = fl; progressBar1.IsIndeterminate = false; });
            }
            catch
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show("Новости не загрузились"); progressBar1.IsIndeterminate = false; });
            }


        }

        #endregion

        private PhotoItemList GetFeedPhotoList()
        {
            var tmp = (PhotoItemList)this.Resources["FeedPhotoListData"];
            if (tmp != null)
            {
                return tmp;
            }
            else
            {
                if (pl != null)
                {
                    return pl;
                }
                else
                {
                    FeedPhotoPanel.DataContext = pl;
                    return pl;
                }
            }
        }

        #region получаем фото

        private void ListFeedPhotosCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/newsfeed.get?filters=photo&uid={0}&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponsePreparePhotos), web);
            progressBar1.IsIndeterminate = true;
        }

        private void ResponsePreparePhotos(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringfeedphotos = responseReader.ReadToEnd();

            JObject o = JObject.Parse(responseStringfeedphotos);

            JArray responseArray = (JArray)o["response"]["items"];

            //XElement xmlFeedPhotos = XElement.Parse(responseStringfeedphotos);

            try
            {
                /*var items = from feed in xmlFeedPhotos.Element("items").Element("item").Element("photos").Elements("photo")
                            select new PhotoItem(feed.Element("pid").Value, feed.Element("owner_id").Value, feed.Element("aid").Value, feed.Element("src").Value, feed.Element("src_big").Value);*/
                foreach (var item in responseArray)
                {
                    JArray photos = (JArray)item["photos"];
                    for (int i = 1; i < photos.Count; i++)
                    {
                        PhotoItem photoItem = new PhotoItem((int)photos[i]["pid"], (int)photos[i]["owner_id"], (int)photos[i]["aid"], (string)photos[i]["src"], (string)photos[i]["src_big"]);
                        pl.Add(photoItem);
                    }
                }
                this.Dispatcher.BeginInvoke(() =>
                                                {
                                                    foreach (var item in pl)
                                                    {
                                                        Image a = new Image()
                                                                          {
                                                                              Width = 125,
                                                                              Height = 125,
                                                                              Margin = new Thickness(8)
                                                                          };
                                                        ImageSource image = new BitmapImage(new Uri(item.Src));
                                                        a.Source = image;
                                                        wrapPanel.Children.Add(a);
                                                        progressBar1.IsIndeterminate = false;
                                                    }

                                                });

                //this.Dispatcher.BeginInvoke(() => { feedListBox.ItemsSource = fl; progressBar1.IsIndeterminate = false; });
            }
            catch(Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }


        }

        private void AddItem(string photo)
        {
            Image a = new Image();
            ImageSource image = new BitmapImage(new Uri(photo));
            a.Source = image;
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
    }
}