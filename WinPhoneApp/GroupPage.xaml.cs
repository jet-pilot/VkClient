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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data;
using WinPhoneApp.Data.Feed;
using WinPhoneApp.Data.Group;
using WinPhoneApp.Data.Photo;

namespace WinPhoneApp
{
    public partial class GroupPage : PhoneApplicationPage
    {
        private string Gid;
        private FeedList wl;
        private GroupItem gi;
        private List<int> uidlist = new List<int>();

        public GroupPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("gid", out Gid))
            {
                ListWallCallback();
                GroupCallback();
            }
        }

        #region получаем стену

        private void ListWallCallback()
        {
            HttpWebRequest web =
                (HttpWebRequest)
                WebRequest.Create(
                    string.Format("https://api.vkontakte.ru/method/wall.get?owner_id={0}&count=20&access_token={1}",
                                  "-" + Gid, Client.Instance.Access_token.token));
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
                Debug.WriteLine(o);
                try
                {

                    JArray responseFeeds = (JArray) o["response"];
                    for (int i = 1; i < responseFeeds.Count; i++)
                    {
                        DateTime date =
                            new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int) responseFeeds[i]["date"]));
                        PhotoItemList pl = new PhotoItemList();
                        LinkItemList ll = new LinkItemList();
                        AudioItemList al = new AudioItemList();

                        var attachments = responseFeeds[i].SelectToken("attachments", false);
                        string name = " ";
                        string avatar = " ";
                        int uid = (int) responseFeeds[i]["from_id"];
                        if (attachments != null)
                        {
                            foreach (var attachment in attachments)
                            {
                                switch ((string) attachment["type"])
                                {
                                    case "photo":
                                        {
                                            var image = attachment.SelectToken("photo", false);
                                            if (image != null)
                                            {
                                                pl.Add(new PhotoItem((int) image["pid"], (int) image["owner_id"],
                                                                     (string) image["src"], (string) image["src_big"]));
                                            }

                                            break;
                                        }
                                    case "link":
                                        {
                                            var link = attachment.SelectToken("link", false);
                                            if (link != null)
                                            {
                                                ll.Add(new LinkItem((string) link["url"], (string) link["title"],
                                                                    (string) link["description"],
                                                                    (string) link["image_src"]));
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
                            wl.Add(new FeedItem(name, avatar, (string) responseFeeds[i]["text"], date, pl, ll, al, uid));
                            this.uidlist.Add((int) responseFeeds[i]["from_id"]);
                        }
                        else
                        {
                            wl.Add(new FeedItem(name, avatar, (string) responseFeeds[i]["text"], date, uid));
                            this.uidlist.Add((int) responseFeeds[i]["from_id"]);
                        }
                    }
                    this.Dispatcher.BeginInvoke(() =>
                                                    {
                                                        ListProfileWallCallback();
                                                        this.progressBar1.IsIndeterminate = false;
                                                    });
                }
                catch (Exception ex)
                {
                    switch ((int)o["error"]["error_code"])
                    {
                        case 15:
                            {
                                Dispatcher.BeginInvoke(() =>
                                                           {
                                                               WallError.Text =
                                                                   "Доступ к стене разрешен только участникам группы";
                                                               WallPostBox.Visibility = Visibility.Collapsed;
                                                               WallPanel.Visibility = Visibility.Collapsed;
                                                           });
                                
                                break;
                            }
                        default:
                            {
                                Dispatcher.BeginInvoke(() => MessageBox.Show(ex.Message));
                                break;
                            }
                    }
                    
                }
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
                Dispatcher.BeginInvoke(() => { wallListBox.ItemsSource = wl; progressBar1.IsIndeterminate = false; });
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show(ex.Message));
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
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/wall.post?owner_id={0}&message={1}&access_token={2}", Gid, message, Client.Instance.Access_token.token));
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
                            MessageBox.Show("В этой группе запрещено писать на стене");
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

        #region получаем инфо группы

        private void GroupCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/groups.getById?gid={0}&access_token={1}", Gid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponsePrepareGroup), web);
            progressBar1.IsIndeterminate = true;
        }
        private void ResponsePrepareGroup(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());
            try
            {
                string responseStringprofile = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseStringprofile);
                JArray responseArray = (JArray)o["response"];
                gi = (new GroupItem(
                    (int)responseArray[0]["gid"],
                    (string)responseArray[0]["name"],
                    (string)responseArray[0]["screen_name"],
                    (int)responseArray[0]["is_closed"],
                    (string)responseArray[0]["type"],
                    (string)responseArray[0]["photo"],
                    (string)responseArray[0]["photo_medium"],
                    (string)responseArray[0]["photo_big"]
                    ));

                this.Dispatcher.BeginInvoke(() =>
                                                {
                                                    Group.Title = gi.Name;
                                                    Debug.WriteLine(gi);
                                                    GroupCountMemberCallback();
                                                    progressBar1.IsIndeterminate = false;
                                                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }


        }

        private void GroupCountMemberCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/groups.getMembers?gid={0}&count=1&access_token={1}", Gid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareGroupCountMember), web);
            progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareGroupCountMember(IAsyncResult e)
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
                    gi.CountMember = (int)o["response"]["count"];
                    Debug.WriteLine(gi.CountMember);
                    Info.DataContext = gi;
                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
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

        private void NavigateToProfileFromWall(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FeedItem item = ((FrameworkElement)sender).DataContext as FeedItem;
            NavigationService.Navigate(new Uri("/ProfilePage.xaml?uid=" + item.Uid, UriKind.Relative));
        }


    }
}