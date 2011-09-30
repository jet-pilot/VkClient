using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using WinPhoneApp.Data.Feed;
using WinPhoneApp.Data;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data.Profile;
using System.ComponentModel;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.Diagnostics;

namespace WinPhoneApp
{
    public partial class MainPage
    {
        private FeedList _fl;
        private FeedList _wl;
        private MyProfile _mp;
        ApplicationBarIconButton _pict;
        ApplicationBarIconButton _camera;
        private readonly List<int> _uidlist = new List<int>();

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPageLoaded;
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            Client.Instance.ActiveChanged += ClientActiveChanged;
            UpdateUI();

            _pict = new ApplicationBarIconButton { IconUri = new Uri("/Images/appbar.cupcake.png", UriKind.RelativeOrAbsolute), Text = "фото" };
            _pict.Click += PictClick;

            _camera = new ApplicationBarIconButton
                          {
                              IconUri = new Uri("/Images/appbar.feature.camera.rest.png", UriKind.RelativeOrAbsolute),
                              Text = "камера"
                          };
            _camera.Click += CameraClick;
        }

        private void ClientActiveChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(UpdateUI);
        }

        private void UpdateUI()
        {
            var started = Client.Instance.Active;
            if (!started) { NavigationService.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative)); }
            else
            {
                ListProfileCallback();
                ListFeedsCallback();
                ListWallCallback();
            }
        }


        #region получаем новости

        private void ListFeedsCallback()
        {
            if (_fl != null) { return; }
            _fl = new FeedList();
            var web =
                (HttpWebRequest)
                WebRequest.Create(
                    string.Format("https://api.vk.com/method/newsfeed.get?uid={0}&count=20&access_token={1}",
                                  Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);
                                         var responseReader = new StreamReader(response.GetResponseStream());
                                         var responseString = responseReader.ReadToEnd();

                                         try
                                         {
                                             var o = JObject.Parse(responseString);
                                             var responseFeeds = (JArray)o["response"]["items"];
                                             var responseProfiles = (JArray)o["response"]["profiles"];
                                             var responseGroups = (JArray)o["response"]["groups"];
                                             foreach (var feed in responseFeeds)
                                             {
                                                 switch ((string)feed["type"])
                                                 {
                                                     case "post":
                                                         {
                                                             var text = (string)feed["text"];
                                                             var attachments = feed.SelectToken("attachments", false);
                                                             if (text.Length > 100)
                                                             {
                                                                 text = text.Substring(0, 100);
                                                             }
                                                             var feedItem = new FeedItem
                                                                                {
                                                                                    Date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)feed["date"])),
                                                                                    Text = text + "...",
                                                                                    CntComments = "комментариев: " + (int)feed["comments"]["count"]
                                                                                };
                                                             if ((string)feed["post_source"]["data"] == "profile_photo")
                                                             {
                                                                 feedItem.Text = "обновил(а) фотографию на странице";
                                                             }
                                                             else if (attachments != null)
                                                             {
                                                                 feedItem.Text = feedItem.Text + "\n + " + attachments.Count() + " файл.";
                                                             }

                                                             if ((int)feed["source_id"] > 0)
                                                             {

                                                                 var profile = responseProfiles.Single(c => (int)c["uid"] == (int)feed["source_id"]);
                                                                 feedItem.Author = (string)profile["first_name"] + " " + (string)profile["last_name"];
                                                                 feedItem.Avatar = (string)profile["photo"];
                                                                 feedItem.Uid = (int)profile["uid"];
                                                             }
                                                             else
                                                             {
                                                                 var group = responseGroups.Single(c => (int)c["gid"] == Math.Abs((int)feed["source_id"]));
                                                                 feedItem.Author = (string)group["name"];
                                                                 feedItem.Avatar = (string)group["photo"];
                                                             }
                                                             feedItem.PostId = (int)feed["post_id"];
                                                             feedItem.Text = feedItem.Text.Replace("<br>", "\n");
                                                             feedItem.Text = feedItem.Text.Replace("&quot;", "\"");
                                                             _fl.Add(feedItem);
                                                             break;
                                                         }
                                                     case "photo":
                                                         {
                                                             var profile =
                                                                 responseProfiles.Single(
                                                                     c => (int)c["uid"] == (int)feed["source_id"]);
                                                             var feedItem = new FeedItem
                                                                                {
                                                                                    Author = (string)profile["first_name"] + " " + (string)profile["last_name"],
                                                                                    Avatar = (string)profile["photo"],
                                                                                    Text = "добавил(а) " + (int)feed["photos"][0] + " фот.",
                                                                                    Date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)feed["date"]))
                                                                                };
                                                             _fl.Add(feedItem);
                                                             break;
                                                         }
                                                     case "photo_tag":
                                                         {
                                                             var profile =
                                                                 responseProfiles.Single(
                                                                     c => (int)c["uid"] == (int)feed["source_id"]);
                                                             var feedItem = new FeedItem
                                                                                {
                                                                                    Author = (string)profile["first_name"] + " " + (string)profile["last_name"],
                                                                                    Avatar = (string)profile["photo"],
                                                                                    Text = "отмечен(а) на " + (int)feed["photo_tags"][0] + " фот.",
                                                                                    Date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)feed["date"])),
                                                                                };
                                                             _fl.Add(feedItem);
                                                             break;
                                                         }
                                                     case "friend":
                                                         {
                                                             var user =
                                                                 responseProfiles.Single(
                                                                     c => (int)c["uid"] == (int)feed["source_id"]);
                                                             var feedItem = new FeedItem
                                                                                {
                                                                                    Author = (string)user["first_name"] + " " + (string)user["last_name"],
                                                                                    Avatar = (string)user["photo"],
                                                                                    Date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)feed["date"])),
                                                                                    Text = "добавил(а) в друзья: "
                                                                                };
                                                             var friendsAdd = (JArray)feed["friends"];
                                                             for (var i = 1; i < friendsAdd.Count; i++)
                                                             {
                                                                 var friend =
                                                                     responseProfiles.Single(c => (int)c["uid"] == (int)friendsAdd[i]["uid"]);
                                                                 feedItem.FriendList.Add(
                                                                     new MyProfile((string)friend["first_name"], (string)friend["last_name"], (string)friend["photo"]));
                                                             }
                                                             _fl.Add(feedItem);
                                                             Debug.WriteLine(feedItem.FriendList.Count);
                                                             break;
                                                         }
                                                     case "note":
                                                         {
                                                             break;
                                                         }
                                                     default:
                                                         {
                                                             new Exception("ТВОЮ МАТЬ!!!11");
                                                             break;
                                                         }
                                                 }
                                             }
                                             Dispatcher.BeginInvoke(() =>
                                                                        {
                                                                            feedListBox.ItemsSource = _fl;
                                                                            if (!progressBar1.IsIndeterminate) return;
                                                                            progressBar1.IsIndeterminate = false;
                                                                        });
                                         }
                                         catch (Exception exception)
                                         {
                                             Dispatcher.BeginInvoke(() =>
                                                                             {
                                                                                 MessageBox.Show(exception.Message);
                                                                                 if (!progressBar1.IsIndeterminate) return;
                                                                                 progressBar1.IsIndeterminate = false;
                                                                             });
                                         }
                                     }, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }

        #endregion

        #region получаем стену

        private void ListWallCallback()
        {
            if (_wl != null) { return; }
            _wl = new FeedList();
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/wall.get?uid={0}&count=20&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);
                                         var responseReader = new StreamReader(response.GetResponseStream());
                                         var responseString = responseReader.ReadToEnd();

                                         try
                                         {
                                             var o = JObject.Parse(responseString);
                                             var responseFeeds = (JArray)o["response"];
                                             for (int i = 1; i < responseFeeds.Count; i++)
                                             {
                                                 var text = (string)responseFeeds[i]["text"];
                                                 var attachments = responseFeeds[i].SelectToken("attachments", false);
                                                 if (text.Length > 100)
                                                 {
                                                     text = text.Substring(0, 100);
                                                 }
                                                 var wallItem = new FeedItem
                                                                    {
                                                                        Date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)responseFeeds[i]["date"])),
                                                                        Text = text + "...",
                                                                        Uid = (int)responseFeeds[i]["from_id"],
                                                                        PostId = (int)responseFeeds[i]["id"],
                                                                        CntComments = "комментариев: " + (int)responseFeeds[i]["comments"]["count"]
                                                                    };
                                                 if ((string)responseFeeds[i]["post_source"]["data"] == "profile_photo")
                                                 {
                                                     wallItem.Text = "обновил(а) фотографию на странице";
                                                 }
                                                 else if (attachments != null)
                                                 {
                                                     wallItem.Text = wallItem.Text + "\n + " + attachments.Count() + " файл.";
                                                 }
                                                 _wl.Add(wallItem);
                                                 _uidlist.Add((int)responseFeeds[i]["from_id"]);

                                             }
                                             Dispatcher.BeginInvoke(ListProfileWallCallback);

                                         }
                                         catch (Exception exception)
                                         {
                                             Dispatcher.BeginInvoke(() =>
                                                                        {
                                                                            MessageBox.Show(exception.Message);
                                                                            if (!progressBar1.IsIndeterminate) return;
                                                                            progressBar1.IsIndeterminate = false;
                                                                        });
                                         }
                                     }, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }

        private void ListProfileWallCallback()
        {
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
            var requestString = string.Format("https://api.vk.com/method/getProfiles?access_token={0}&fields=photo&uids=", Client.Instance.Access_token.token);
            requestString = _uidlist.Aggregate(requestString, (current, item) => current + ("," + item));
            var web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);
                                         var responseReader = new StreamReader(response.GetResponseStream());

                                         try
                                         {
                                             var responseStringStatus = responseReader.ReadToEnd();
                                             var o = JObject.Parse(responseStringStatus);
                                             var responseArray = (JArray)o["response"];
                                             foreach (var item in _wl)
                                             {
                                                 var item1 = item;
                                                 foreach (var uid in responseArray.Where(uid => item1.Uid == (int)uid["uid"]))
                                                 {
                                                     item.Author = uid["first_name"] + " " + uid["last_name"]; item.Avatar = (string)uid["photo"]; break;
                                                 }
                                             }
                                             Dispatcher.BeginInvoke(() =>
                                                                        {
                                                                            wallListBox.ItemsSource = _wl;
                                                                            if (progressBar1.IsIndeterminate) return;
                                                                            progressBar1.IsIndeterminate = false;
                                                                        });
                                         }
                                         catch (Exception exception)
                                         {
                                             Dispatcher.BeginInvoke(() => MessageBox.Show(exception.Message));
                                         }
                                     }, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }


        #endregion

        #region получаем профиль

        private void ListProfileCallback()
        {
            if (_mp != null) { return; }
            _mp = new MyProfile();
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/getProfiles?fields=photo&uid={0}&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);

                                         var responseReader = new StreamReader(response.GetResponseStream());

                                         try
                                         {
                                             var responseStringprofile = responseReader.ReadToEnd();
                                             var o = JObject.Parse(responseStringprofile);
                                             var responseArray = (JArray)o["response"];

                                             _mp = new MyProfile
                                                       {
                                                           Full_name = (string)responseArray[0]["first_name"] + " " + (string)responseArray[0]["last_name"],
                                                           Photo = (string)responseArray[0]["photo"]
                                                       };
                                             Dispatcher.BeginInvoke(() =>
                                             {
                                                 StatusCallback();
                                                 if (!progressBar1.IsIndeterminate) return;
                                                 progressBar1.IsIndeterminate = false;
                                             });
                                         }
                                         catch (Exception ex)
                                         {
                                             Dispatcher.BeginInvoke(() =>
                                                                        {
                                                                            MessageBox.Show(ex.Message);
                                                                            if (!progressBar1.IsIndeterminate) return;
                                                                            progressBar1.IsIndeterminate = false;
                                                                        });
                                         }
                                     }, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }


        private void StatusCallback()
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/status.get?uid={0}&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);

                                         var responseReader = new StreamReader(response.GetResponseStream());

                                         try
                                         {
                                             var responseStringStatus = responseReader.ReadToEnd();
                                             var o = JObject.Parse(responseStringStatus);
                                             Dispatcher.BeginInvoke(() =>
                                                                        {
                                                                            _mp.Status = (string)o["response"]["text"];
                                                                            MyProfilePanel.DataContext = _mp;
                                                                            if (!progressBar1.IsIndeterminate) return;
                                                                            progressBar1.IsIndeterminate = false;
                                                                        });
                                         }
                                         catch (Exception ex)
                                         {
                                             Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
                                         }
                                     }, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }

        #endregion

        private void NavigateToProfile(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as FeedItem;
            if (item != null)
                NavigationService.Navigate(new Uri("/ProfilePage.xaml?uid=" + item.Uid, UriKind.Relative));
        }

        private void NavigateToFeed(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as FeedItem;
            if (item != null)
                NavigationService.Navigate(new Uri("/FeedPage.xaml?uid=" + item.Uid + "&post_id=" + item.PostId, UriKind.Relative));
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (MessageBox.Show("Вы действительно хотите выйти из приложения?", "выйти", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void HyperlinkButtonClick(object sender, RoutedEventArgs e)
        {
            var wbt = new WebBrowserTask();
            var btn = (HyperlinkButton)e.OriginalSource;
            wbt.Uri = btn.NavigateUri;
            wbt.Show();
        }

        #region отправка сообщения на стену
        private void PostSend(object sender, EventArgs e)
        {
            //GetWallUploadServerCallback();
            if (PostBox.Text.Length > 0)
            {
                PostSendCallback(PostBox.Text);
            }
            else
            {
                Dispatcher.BeginInvoke(
                    () => MessageBox.Show("Невозможно отправить пустое сообщение, напишите хоть что-нибудь"));
            }
        }

        private void PostSendCallback(string message)
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/wall.post?owner_id={0}&message={1}&access_token={2}", Client.Instance.Access_token.uid, message, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(ResponcePreparePost, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }
        public void ResponcePreparePost(IAsyncResult e)
        {
            var request = (HttpWebRequest)e.AsyncState;
            var response = (HttpWebResponse)request.EndGetResponse(e);

            var responseReader = new StreamReader(response.GetResponseStream());

            string responseString = responseReader.ReadToEnd();

            try
            {
                var o = JObject.Parse(responseString);
                Dispatcher.BeginInvoke(() =>
                {
                    var postId = (int)o["response"]["post_id"];
                    Debug.WriteLine(postId.ToString());
                    PostBox.Text = "";
                    progressBar1.IsIndeterminate = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show(ex.Message));
            }
        }
        #endregion

        private void PostBoxGotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.IsVisible = true;
            ApplicationBar.Buttons.Add(_pict);
            ApplicationBar.Buttons.Add(_camera);
        }

        private void PostBoxLostFocus(object sender, RoutedEventArgs e)
        {
            while (ApplicationBar.Buttons.Count > 0)
            {
                ApplicationBar.Buttons.RemoveAt(0);
            }
            ApplicationBar.IsVisible = false;
        }

        private void pct_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {

            }
        }

        void PictClick(object sender, EventArgs e)
        {
            var pct = new PhotoChooserTask();
            pct.Completed += pct_Completed;
            pct.Show();
        }

        void CameraClick(object sender, EventArgs e)
        {
            var pct = new CameraCaptureTask();
            pct.Completed += pct_Completed;
            pct.Show();
        }

        private void NavigateToMessagePage(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MessagesPage.xaml", UriKind.Relative));
        }

        private void NavigateToFriendListPage(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/FriendListPage.xaml", UriKind.Relative));
        }

        private void NavigateToSettingsPage(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        #region получаем ссылку для загрузки

        /*
        private void GetWallUploadServerCallback()
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/photos.getWallUploadServer?uid={0}&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(ResponcePrepareGetWallUploadServer, web);
            progressBar1.IsIndeterminate = true;
        }

        private void ResponcePrepareGetWallUploadServer(IAsyncResult e)
        {
            var request = (HttpWebRequest)e.AsyncState;
            var response = (HttpWebResponse)request.EndGetResponse(e);

            var responseReader = new StreamReader(response.GetResponseStream());

            string responseString = responseReader.ReadToEnd();

            try
            {
                var o = JObject.Parse(responseString);
                _uploadUrl = (string)o["response"]["upload_url"];
                Debug.WriteLine(_uploadUrl);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }
*/

        #endregion
    }
}