using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
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
        private FeedList _wl;
        private MyProfile _mp;
        private AlbumList _al;
        private GroupList _gl;
        private SubscriptionList _sl;
        private readonly List<int> _uidlist = new List<int>();
        private readonly List<string> _thumbIdList = new List<string>();
        private string _uid;

        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!NavigationContext.QueryString.TryGetValue("uid", out _uid)) return;
            ListWallCallback();
            ProfileCallback();
            ListAlbumCallback();
            GroupsCallback();
            SubscriptionsCallback();
        }

        #region получаем стену

        private void ListWallCallback()
        {
            if (_wl != null) { return; }
            _wl = new FeedList();
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/wall.get?owner_id={0}&count=20&access_token={1}", _uid, Client.Instance.Access_token.token));
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

        private void ProfileCallback()
        {
            if (_mp != null) { return; }
            _mp = new MyProfile();
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/getProfiles?fields=photo_medium,contacts,bdate,city,education&uid={0}&access_token={1}", _uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
            {
                var request = (HttpWebRequest)e.AsyncState;
                var response = (HttpWebResponse)request.EndGetResponse(e);

                var responseReader = new StreamReader(response.GetResponseStream());
                try
                {
                    string responseStringprofile = responseReader.ReadToEnd();
                    var o = JObject.Parse(responseStringprofile);
                    var responseArray = (JArray)o["response"];
                    _mp = new MyProfile((string)responseArray[0]["first_name"], (string)responseArray[0]["last_name"], (string)responseArray[0]["photo_medium"]);
                    _mp.Full_name = _mp.First_name + " " + _mp.Last_name;
                    _mp.Mobile_phone = (string)responseArray[0]["mobile_phone"];
                    _mp.Bdate = (string)responseArray[0]["bdate"];
                    _mp.Sex = (string)responseArray[0]["sex"];
                    _mp.City = (string)responseArray[0]["city"];
                    _mp.University = (string)responseArray[0]["university_name"];
                    Dispatcher.BeginInvoke(() =>
                    {
                        ProfileStatusCallback();
                        Profile.Title = _mp.Full_name;
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

        private void ProfileStatusCallback()
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/status.get?uid={0}&access_token={1}", _uid, Client.Instance.Access_token.token));
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
                                                 Info.DataContext = _mp;
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

        #region получаем список альбомов

        private void ListAlbumCallback()
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/photos.getAlbums?uid={0}&access_token={1}", _uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);

                                         var responseReader = new StreamReader(response.GetResponseStream());
                                         _al = new AlbumList();
                                         try
                                         {
                                             var responseString = responseReader.ReadToEnd();
                                             var o = JObject.Parse(responseString);
                                             try
                                             {
                                                 var responseArray = (JArray)o["response"];
                                                 foreach (var albumItem in responseArray.Select(album => new AlbumItem
                                                                                                             {
                                                                                                                 Aid = (string) album["aid"],
                                                                                                                 ThumbId = (string) album["thumb_id"],
                                                                                                                 OwnerId = (string) album["owner_id"],
                                                                                                                 Title = (string) album["title"],
                                                                                                                 Description = (string) album["description"],
                                                                                                                 Created = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(Convert.ToInt32((string) album["created"]))),
                                                                                                                 Updated = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(Convert.ToInt32((string) album["updated"]))),
                                                                                                                 Size = (string) album["size"] + " фот."
                                                                                                             }))
                                                 {
                                                     _thumbIdList.Add(albumItem.OwnerId + "_" + albumItem.ThumbId);
                                                     _al.Add(albumItem);
                                                 }
                                                 Dispatcher.BeginInvoke(ListAlbumThumbCallback);
                                             }
                                             catch
                                             {
                                                 Dispatcher.BeginInvoke(() =>
                                                 {
                                                     NoAlbums.Text = "альбомов нет";
                                                     progressBar1.IsIndeterminate = false;
                                                 });
                                                 return;
                                             }
                                         }
                                         catch (Exception ex)
                                         {
                                             Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
                                         }
                                     }, web);
            progressBar1.IsIndeterminate = true;
        }

        private void ListAlbumThumbCallback()
        {
            var requestString = "https://api.vkontakte.ru/method/photos.getById?access_token=" + Client.Instance.Access_token.token + "&photos=";
            requestString = _thumbIdList.Aggregate(requestString, (current, item) => current + ("," + item));
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
                                             var responseString = responseReader.ReadToEnd();
                                             var o = JObject.Parse(responseString);
                                             try
                                             {

                                                 var reponseArray = (JArray)o["response"];
                                                 foreach (var item in reponseArray)
                                                 {
                                                     foreach (var album in _al.Where(album => (string)item["pid"] == album.ThumbId))
                                                     {
                                                         album.Cover = (string)item["src_small"];
                                                     }
                                                 }
                                                 
                                                 Dispatcher.BeginInvoke(() =>
                                                 {
                                                     AlbumsPanel.DataContext = _al;
                                                     if (!progressBar1.IsIndeterminate) return;
                                                     progressBar1.IsIndeterminate = false;
                                                 });

                                             }
                                             catch
                                             {
                                                 if ((int)o["error"]["error_code"] == 200)
                                                 {
                                                     Dispatcher.BeginInvoke(() =>
                                                     {
                                                         NoAlbums.Text = "доступ закрыт";
                                                         progressBar1.IsIndeterminate = false;
                                                     });
                                                 }
                                             }
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

        #region получаем список групп

        private void GroupsCallback()
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/groups.get?uid={0}&extended=1&access_token={1}", _uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);

                                         var responseReader = new StreamReader(response.GetResponseStream());
                                         _gl = new GroupList();
                                         try
                                         {
                                             var responseString = responseReader.ReadToEnd();
                                             var o = JObject.Parse(responseString);
                                             try
                                             {
                                                 var responseArray = (JArray)o["response"];
                                                 for (int i = 1; i < responseArray.Count; i++)
                                                 {
                                                     _gl.Add(new GroupItem(
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
                                                 Dispatcher.BeginInvoke(() =>
                                                 {
                                                     GroupsPanel.DataContext = _gl;
                                                     if (!progressBar1.IsIndeterminate) return;
                                                     progressBar1.IsIndeterminate = false;
                                                 });
                                             }
                                             catch
                                             {
                                                 if ((int)o["error"]["error_code"] == 260)
                                                 {
                                                     Dispatcher.BeginInvoke(() =>
                                                     {
                                                         NoGroups.Text = "пользователь закрыл доступ к списку групп";
                                                         progressBar1.IsIndeterminate = false;
                                                     });
                                                 }
                                             }
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

        #region получаем список подписок :)

        private void SubscriptionsCallback()
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/subscriptions.get?uid={0}&access_token={1}", _uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);

                                         var responseReader = new StreamReader(response.GetResponseStream());
                                         try
                                         {
                                             var responseString = responseReader.ReadToEnd();
                                             var o = JObject.Parse(responseString);
                                             try
                                             {
                                                 _sl = new SubscriptionList();
                                                 if ((int)o["response"]["count"] > 0)
                                                 {
                                                     var responseArray = (JArray)o["response"]["users"];
                                                     foreach (var profile in responseArray.Select(user => new MyProfile {Uid = (int) user}))
                                                     {
                                                         _sl.Add(profile);
                                                     }
                                                     Dispatcher.BeginInvoke(() =>
                                                     {
                                                         ListProfileSubscriptionsCallback();
                                                         progressBar1.IsIndeterminate = false;
                                                     });
                                                 }
                                                 else
                                                 {
                                                     Dispatcher.BeginInvoke(() =>
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
                                                     Dispatcher.BeginInvoke(() =>
                                                     {
                                                         NoSubscriptions.Text = "пользователь закрыл доступ к списку подписок";
                                                         progressBar1.IsIndeterminate = false;
                                                     });
                                                 }
                                             }

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


        private void ListProfileSubscriptionsCallback()
        {
            var requestString = string.Format("https://api.vkontakte.ru/method/getProfiles?access_token={0}&fields=photo&uids=", Client.Instance.Access_token.token);
            requestString = _sl.Aggregate(requestString, (current, item) => current + ("," + item.Uid));
            var web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);

                                         var responseReader = new StreamReader(response.GetResponseStream());

                                         var responseStringStatus = responseReader.ReadToEnd();

                                         try
                                         {
                                             var o = JObject.Parse(responseStringStatus);
                                             var responseArray = (JArray)o["response"];
                                             foreach (var item in _sl)
                                             {
                                                 foreach (var uid in responseArray.Where(uid => item.Uid == (int)uid["uid"]))
                                                 {
                                                     item.Full_name = uid["first_name"] + " " + uid["last_name"]; item.Photo = (string)uid["photo"]; item.Uid = (int)uid["uid"]; break;
                                                 }
                                             }
                                             Dispatcher.BeginInvoke(() => { SubscriptionsPanel.DataContext = _sl; progressBar1.IsIndeterminate = false; });
                                         }
                                         catch (Exception ex)
                                         {
                                             Dispatcher.BeginInvoke(() =>
                                             {
                                                 MessageBox.Show(ex.Message);
                                                 progressBar1.IsIndeterminate = false;
                                             });
                                         }
                                     }, web);
            progressBar1.IsIndeterminate = true;
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
                Dispatcher.BeginInvoke(
                    () => MessageBox.Show("Невозможно отправить пустое сообщение, напишите хоть что-нибудь"));
            }
        }

        private void PostSendCallback(string message)
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/wall.post?owner_id={0}&message={1}&access_token={2}", _uid, message, Client.Instance.Access_token.token));
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
                                             var error = o.SelectToken("error", false);
                                             if (error != null)
                                             {
                                                 if ((int)error["error_code"] == 214)
                                                 {
                                                     Dispatcher.BeginInvoke(() =>
                                                     {
                                                         WallPostBox.Visibility = Visibility.Collapsed;
                                                         MessageBox.Show("Этот пользователь запретил писать у себя на стене");
                                                         progressBar1.IsIndeterminate = false;
                                                     });
                                                 }
                                             }
                                             else
                                             {
                                                 Dispatcher.BeginInvoke(() =>
                                                 {
                                                     var postId = (int)o["response"]["post_id"];
                                                     Debug.WriteLine(postId.ToString());
                                                     WallPostBox.Text = "";
                                                     progressBar1.IsIndeterminate = false;
                                                 });
                                             }
                                         }
                                         catch (Exception ex)
                                         {
                                             Dispatcher.BeginInvoke(() =>
                                             {
                                                 MessageBox.Show(ex.Message);
                                                 progressBar1.IsIndeterminate = false;
                                             });
                                         }
                                     }, web);
            progressBar1.IsIndeterminate = true;
        }
        #endregion

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var wbt = new WebBrowserTask();
            var btn = (HyperlinkButton)e.OriginalSource;
            wbt.Uri = btn.NavigateUri;
            wbt.Show();
        }

        private void SendPmToUser(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SendMessage.xaml?uid=" + _uid, UriKind.Relative));
        }

        private void NavigateToProfileFromWall(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as FeedItem;
            if (item!=null) NavigationService.Navigate(new Uri("/ProfilePage.xaml?uid=" + item.Uid, UriKind.Relative));
        }

        private void NavigateToProfileFromSubscriptions(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as MyProfile;
            if (item != null) NavigationService.Navigate(new Uri("/ProfilePage.xaml?uid=" + item.Uid, UriKind.Relative));
        }

        private void NavigateToGroup(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as GroupItem;
            if (item != null) NavigationService.Navigate(new Uri("/GroupPage.xaml?gid=" + item.Gid, UriKind.Relative));
        }

        private void NavigateToFeed(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FrameworkElement)sender).DataContext as FeedItem;
            if (item != null)
                NavigationService.Navigate(new Uri("/FeedPage.xaml?uid=" + item.Uid + "&post_id=" + item.PostId, UriKind.Relative));
        }
    }
}