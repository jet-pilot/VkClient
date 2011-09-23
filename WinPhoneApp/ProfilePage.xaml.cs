using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data;
using WinPhoneApp.Data.Feed;
using WinPhoneApp.Data.Group;
using WinPhoneApp.Data.Photo;
using WinPhoneApp.Data.Profile;

namespace WinPhoneApp
{
    public partial class ProfilePage : PhoneApplicationPage
    {
        private FeedList wl;
        private MyProfile mp;
        private AlbumList al;
        private GroupList gl;
        private SubscriptionList sl;
        private List<int> uidlist = new List<int>();
        private List<string> thumbIdList = new List<string>();
        private string Uid;

        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("uid", out Uid))
            {
                ListWallCallback();
                ProfileCallback();
                ListAlbumCallback();
                GroupsCallback();
                SubscriptionsCallback();
            }
        }

        #region получаем стену

        private void ListWallCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/wall.get?owner_id={0}&count=20&access_token={1}", Uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponsePrepareWall), web);
            progressBar1.IsIndeterminate = true;
        }

        private void ResponsePrepareWall(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseString = responseReader.ReadToEnd();
            wl = new FeedList();
            try
            {
                JObject o = JObject.Parse(responseString);
                JArray responseFeeds = (JArray)o["response"];
                for (int i = 1; i < responseFeeds.Count; i++)
                {
                    DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)responseFeeds[i]["date"]));
                    PhotoItemList pl = new PhotoItemList();
                    LinkItemList ll = new LinkItemList();
                    AudioItemList al = new AudioItemList();

                    var attachments = responseFeeds[i].SelectToken("attachments", false);
                    string name = " ";
                    string avatar = "http://cs5425.vk.com/u27309041/e_0bf7e5d5.jpg";
                    int uid = (int)responseFeeds[i]["from_id"];
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
                                            al.Add(new AudioItem());
                                        }
                                        break;
                                    }
                            }
                        }
                        wl.Add(new FeedItem(name, avatar, (string)responseFeeds[i]["text"], date, pl, ll, al, uid));
                        this.uidlist.Add((int)responseFeeds[i]["from_id"]);
                    }
                    else
                    {
                        wl.Add(new FeedItem(name, avatar, (string)responseFeeds[i]["text"], date, uid));
                        this.uidlist.Add((int)responseFeeds[i]["from_id"]);
                    }
                }
                this.Dispatcher.BeginInvoke(() =>
                {
                    ListProfileWallCallback();
                    this.progressBar1.IsIndeterminate = false;
                });

            }
            catch
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show("Сообщения стены не загрузились"); progressBar1.IsIndeterminate = false; });
            }
        }

        private void ListProfileWallCallback()
        {
            string requestString = string.Format("https://api.vkontakte.ru/method/getProfiles?access_token={0}&fields=photo&uids=", Client.Instance.Access_token.token);
            foreach (var item in this.uidlist)
            {
                requestString += "," + item;
            }
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareProfileWall), web);
            this.progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareProfileWall(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringStatus = responseReader.ReadToEnd();
            try
            {
                JObject o = JObject.Parse(responseStringStatus);
                JArray responseArray = (JArray)o["response"];
                foreach (var item in wl)
                {
                    foreach (var uid in responseArray)
                    {
                        if (item.Uid == (int)uid["uid"]) { item.Author = uid["first_name"] + " " + uid["last_name"]; item.Avatar = (string)uid["photo"]; break; }
                    }
                }
                this.Dispatcher.BeginInvoke(() => { this.wallListBox.ItemsSource = wl; this.progressBar1.IsIndeterminate = false; });
            }
            catch
            {

            }
        }


        #endregion

        #region получаем профиль

        private void ProfileCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/getProfiles?fields=photo_medium,contacts,bdate,city,education&uid={0}&access_token={1}", Uid, Client.Instance.Access_token.token));
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
            try
            {
                string responseStringprofile = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseStringprofile);
                JArray responseArray = (JArray)o["response"];
                mp = new MyProfile((string)responseArray[0]["first_name"], (string)responseArray[0]["last_name"], (string)responseArray[0]["photo_medium"]);
                mp.Full_name = mp.First_name + " " + mp.Last_name;
                mp.Mobile_phone = (string)responseArray[0]["mobile_phone"];
                mp.Bdate = (string)responseArray[0]["bdate"];
                mp.Sex = (string)responseArray[0]["sex"];
                mp.City = (string)responseArray[0]["city"];
                mp.University = (string)responseArray[0]["university_name"];
                this.Dispatcher.BeginInvoke(() =>
                                                {
                                                    ProfileStatusCallback();

                                                    Profile.Title = mp.Full_name;
                                                    progressBar1.IsIndeterminate = false;
                                                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }


        }

        private void ProfileStatusCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/status.get?uid={0}&access_token={1}", Uid, Client.Instance.Access_token.token));
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

            try
            {
                string responseStringStatus = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseStringStatus);
                this.Dispatcher.BeginInvoke(() =>
                {
                    mp.Status = (string)o["response"]["text"];
                    Info.DataContext = mp;
                    Debug.WriteLine((string)o["response"]["text"]);
                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }

        #endregion

        #region получаем список альбомов

        private void ListAlbumCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/photos.getAlbums?uid={0}&access_token={1}", Uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareAlbums), web);
            progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareAlbums(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());
            al = new AlbumList();
            try
            {
                string responseString = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseString);
                try
                {
                    JArray responseArray = (JArray)o["response"];
                    foreach (var album in responseArray)
                    {
                        AlbumItem albumItem = new AlbumItem();
                        albumItem.Aid = (string)album["aid"];
                        albumItem.ThumbId = (string)album["thumb_id"];
                        albumItem.OwnerId = (string)album["owner_id"];
                        albumItem.Title = (string)album["title"];
                        albumItem.Description = (string)album["description"];
                        albumItem.Created =
                            new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(
                                Convert.ToDouble(Convert.ToInt32((string)album["created"])));
                        albumItem.Updated =
                            new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(
                                Convert.ToDouble(Convert.ToInt32((string)album["updated"])));
                        albumItem.Size = (string)album["size"] + " фот.";
                        thumbIdList.Add(albumItem.OwnerId + "_" + albumItem.ThumbId);
                        al.Add(albumItem);
                    }
                    this.Dispatcher.BeginInvoke(ListAlbumThumbCallback);
                }
                catch
                {
                    this.Dispatcher.BeginInvoke(() =>
                                                    {
                                                        this.NoAlbums.Text = "альбомов нет";
                                                        progressBar1.IsIndeterminate = false;
                                                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }

        private void ListAlbumThumbCallback()
        {
            string requestString = "https://api.vkontakte.ru/method/photos.getById?access_token=" +
                             Client.Instance.Access_token.token + "&photos=";
            foreach (var item in this.thumbIdList)
            {
                requestString += "," + item;
            }
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareAlbumThumbs), web);
            progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareAlbumThumbs(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            try
            {
                string responseString = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseString);
                try
                {

                    JArray reponseArray = (JArray)o["response"];
                    foreach (var item in reponseArray)
                    {
                        foreach (var album in al)
                        {
                            if ((string)item["pid"] == album.ThumbId)
                            {
                                album.Cover = (string)item["src_small"];
                            }
                        }
                    }
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        AlbumsPanel.DataContext = al;
                        progressBar1.IsIndeterminate = false;
                    });

                }
                catch
                {
                    if ((int)o["error"]["error_code"] == 200)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            NoAlbums.Text = "доступ закрыт";
                            progressBar1.IsIndeterminate = false;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }


        #endregion

        #region получаем список групп

        private void GroupsCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/groups.get?uid={0}&extended=1&access_token={1}", Uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareGroups), web);
            progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareGroups(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());
            gl = new GroupList();
            try
            {
                string responseString = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseString);
                try
                {
                    JArray responseArray = (JArray)o["response"];
                    for (int i = 1; i < responseArray.Count; i++)
                    {
                        gl.Add(new GroupItem(
                                   (int)responseArray[i]["gid"],
                                   (string)responseArray[i]["name"],
                                   (string)responseArray[i]["screen_name"],
                                   (int)responseArray[i]["is_closed"],
                                   (string)responseArray[i]["type"],
                                   (string)responseArray[i]["photo"],
                                   (string)responseArray[i]["photo_medium"],
                                   (string)responseArray[i]["photo_big"]
                                   ));
                    }
                    this.Dispatcher.BeginInvoke(() =>
                                                    {
                                                        GroupsPanel.DataContext = gl;
                                                        progressBar1.IsIndeterminate = false;
                                                    });
                }
                catch
                {
                    if ((int)o["error"]["error_code"] == 260)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                                                        {
                                                            NoGroups.Text = "пользователь закрыл доступ к списку групп";
                                                            progressBar1.IsIndeterminate = false;
                                                        });
                    }
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }

        #endregion

        #region получаем список подписок :)

        private void SubscriptionsCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/subscriptions.get?uid={0}&access_token={1}", Uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareSubscriptions), web);
            progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareSubscriptions(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());
            try
            {
                string responseString = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseString);
                try
                {
                    sl = new SubscriptionList();
                    if ((int)o["response"]["count"] > 0)
                    {
                        JArray responseArray = (JArray)o["response"]["users"];
                        foreach (var user in responseArray)
                        {
                            MyProfile profile = new MyProfile();
                            profile.Uid = (int)user;
                            sl.Add(profile);
                        }
                        this.Dispatcher.BeginInvoke(() =>
                                                        {
                                                            ListProfileSubscriptionsCallback();
                                                            progressBar1.IsIndeterminate = false;
                                                        });
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(() =>
                                                        {
                                                            NoSubscriptions.Text = "подписок нет";
                                                            progressBar1.IsIndeterminate = false;
                                                        });
                    }
                }
                catch
                {
                    if ((int)o["error"]["error_code"] == 260)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                        {
                            NoSubscriptions.Text = "пользователь закрыл доступ к списку групп";
                            progressBar1.IsIndeterminate = false;
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }


        private void ListProfileSubscriptionsCallback()
        {
            string requestString = string.Format("https://api.vkontakte.ru/method/getProfiles?access_token={0}&fields=photo&uids=", Client.Instance.Access_token.token);
            foreach (var item in sl)
            {
                requestString += "," + item.Uid;
            }
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareProfileSubscriptions), web);
            this.progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareProfileSubscriptions(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringStatus = responseReader.ReadToEnd();

            try
            {
                JObject o = JObject.Parse(responseStringStatus);
                JArray responseArray = (JArray)o["response"];
                foreach (var item in sl)
                {
                    foreach (var uid in responseArray)
                    {
                        if (item.Uid == (int)uid["uid"])
                        {
                            item.Full_name = uid["first_name"] + " " + uid["last_name"]; item.Photo = (string)uid["photo"]; item.Uid = (int)uid["uid"]; break;
                        }
                    }
                }
                this.Dispatcher.BeginInvoke(() => { this.SubscriptionsPanel.DataContext = sl; this.progressBar1.IsIndeterminate = false; });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() =>
                                                {
                                                    MessageBox.Show(ex.Message);
                                                    this.progressBar1.IsIndeterminate = false;
                                                });
            }
        }


        #endregion

        #region отправка сообщения на стену
        private void WallPostSend(object sender, EventArgs e)
        {
            if (WallPostBox.Text.Length > 0)
            {
                PostSendCallback(WallPostBox.Text);

            }
            else
            {
                this.Dispatcher.BeginInvoke(
                    () => MessageBox.Show("Невозможно отправить пустое сообщение, напишите хоть что-нибудь"));
            }
        }

        private void PostSendCallback(string message)
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/wall.post?owner_id={0}&message={1}&access_token={2}", Uid, message, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePreparePost), web);
            progressBar1.IsIndeterminate = true;
        }
        public void ResponcePreparePost(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseString = responseReader.ReadToEnd();

            try
            {
                JObject o = JObject.Parse(responseString);
                var error = o.SelectToken("error", false);
                if (error != null)
                {
                    if ((int)error["error_code"] == 214)
                    {
                        this.Dispatcher.BeginInvoke(() =>
                                                        {
                                                            WallPostBox.Visibility = Visibility.Collapsed;
                                                            MessageBox.Show("Этот пользователь запретил писать у себя на стене");
                                                            progressBar1.IsIndeterminate = false;
                                                        });
                    }
                }
                else
                {
                    this.Dispatcher.BeginInvoke(() =>
                                                    {
                                                        int post_id = (int)o["response"]["post_id"];
                                                        Debug.WriteLine(post_id.ToString());
                                                        WallPostBox.Text = "";
                                                        progressBar1.IsIndeterminate = false;
                                                    });
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); });
            }
        }
        #endregion

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var wbt = new WebBrowserTask();
            HyperlinkButton btn = (HyperlinkButton)e.OriginalSource;
            wbt.Uri = btn.NavigateUri;
            wbt.Show();
        }

        private void SendPmToUser(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SendMessage.xaml?uid=" + Uid, UriKind.Relative));
        }

        private void NavigateToProfileFromWall(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FeedItem item = ((FrameworkElement)sender).DataContext as FeedItem;
            NavigationService.Navigate(new Uri("/ProfilePage.xaml?uid=" + item.Uid, UriKind.Relative));
        }

        private void NavigateToProfileFromSubscriptions(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MyProfile item = ((FrameworkElement)sender).DataContext as MyProfile;
            NavigationService.Navigate(new Uri("/ProfilePage.xaml?uid=" + item.Uid, UriKind.Relative));
        }
    }
}