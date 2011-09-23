using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
using System.ComponentModel;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.Diagnostics;

namespace WinPhoneApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private FeedList fl;
        private FeedList wl;
        private MyProfile mp;
        ApplicationBarIconButton pict;
        ApplicationBarIconButton camera;
        string uploadUrl;
        private byte[] buffer;
        private BitmapImage bitmapImage;
        private List<int> uidlist = new List<int>();

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Client.Instance.ActiveChanged += new EventHandler(ClientActiveChanged);
            this.UpdateUI();

            pict = new ApplicationBarIconButton();
            pict.IconUri = new Uri("/Images/appbar.cupcake.png", UriKind.RelativeOrAbsolute);
            pict.Text = "фото";
            pict.Click += pict_Click;

            camera = new ApplicationBarIconButton();
            camera.IconUri = new Uri("/Images/appbar.feature.camera.rest.png", UriKind.RelativeOrAbsolute);
            camera.Text = "камера";
            camera.Click += camera_Click;
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
                GetMyProfile();
                GetWallList();
            }
        }

        private FeedList GetFeedList()
        {
            if (fl != null)
            {
                return fl;
            }
            else
            {
                fl = new FeedList();
                ListFeedsCallback();
                //FeedPanel.DataContext = fl;
                return fl;
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

            try
            {
                JObject o = JObject.Parse(responseStringfeed);
                JArray responseFeeds = (JArray)o["response"]["items"];
                JArray responseProfiles = (JArray)o["response"]["profiles"];
                foreach (var item in responseFeeds)
                {
                    DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)item["date"]));
                    PhotoItemList pl = new PhotoItemList();
                    LinkItemList ll = new LinkItemList();
                    AudioItemList al = new AudioItemList();
                    int uid = (int)item["source_id"];
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
                                                    al.Add(new AudioItem());
                                                }
                                                break;
                                            }
                                    }
                                }
                                fl.Add(new FeedItem(name, avatar, (string)item["text"], date, pl, ll, al, uid));
                            }
                            else
                            {
                                fl.Add(new FeedItem(name, avatar, (string)item["text"], date, uid));
                            }

                        }
                    }
                }
                this.Dispatcher.BeginInvoke(() =>
                {
                    feedListBox.ItemsSource = fl;

                    progressBar1.IsIndeterminate = false;

                });
            }
            catch
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show("Новости не загрузились"); progressBar1.IsIndeterminate = false; });
            }


        }
        #endregion


        private FeedList GetWallList()
        {
            if (wl != null)
            {
                return wl;
            }
            else
            {
                wl = new FeedList();
                ListWallCallback();
                //FeedPanel.DataContext = fl;
                return wl;
            }
        }

        #region получаем стену

        private void ListWallCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/wall.get?uid={0}&count=20&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
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

            try
            {
                string responseString = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseString);
                JArray responseFeeds = (JArray)o["response"];
                for (int i = 1; i < responseFeeds.Count;i++ )
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

            try
            {
                string responseStringStatus = responseReader.ReadToEnd();
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

        private MyProfile GetMyProfile()
        {
            if (mp != null)
            {
                return mp;
            }
            else
            {
                mp = new MyProfile();
                ListProfileCallback();
                ListStatusCallback();
                //MyProfilePanel.DataContext = mp;
                return mp;
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

            try
            {
                string responseStringprofile = responseReader.ReadToEnd();
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

            try
            {
                string responseStringStatus = responseReader.ReadToEnd();
                JObject o = JObject.Parse(responseStringStatus);
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

        private void Navigate_to_FriendListPage(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FriendListPage.xaml", UriKind.Relative));
        }

        private void NavigateToProfile(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FeedItem item = ((FrameworkElement)sender).DataContext as FeedItem;
            NavigationService.Navigate(new Uri("/ProfilePage.xaml?uid=" + item.Uid, UriKind.Relative));
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (MessageBox.Show("Вы действительно хотите выйти из приложения?", "выйти", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var wbt = new WebBrowserTask();
            HyperlinkButton btn = (HyperlinkButton)e.OriginalSource;
            wbt.Uri = btn.NavigateUri;
            wbt.Show();
        }

        #region отправка сообщения на стену
        private void Post_send(object sender, EventArgs e)
        {
            //GetWallUploadServerCallback();
            if (PostBox.Text.Length > 0)
            {
                PostSendCallback(PostBox.Text);
            }
            else
            {
                this.Dispatcher.BeginInvoke(
                    () => MessageBox.Show("Невозможно отправить пустое сообщение, напишите хоть что-нибудь"));
            }
        }

        private void WallPostSend(object sender, EventArgs e)
        {
            //GetWallUploadServerCallback();
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
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/wall.post?owner_id={0}&message={1}&access_token={2}", Client.Instance.Access_token.uid, message, Client.Instance.Access_token.token));
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
                this.Dispatcher.BeginInvoke(() =>
                {
                    int post_id = (int)o["response"]["post_id"];
                    Debug.WriteLine(post_id.ToString());
                    PostBox.Text = "";
                    WallPostBox.Text = "";
                    progressBar1.IsIndeterminate = false;
                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); });
            }
        }
        #endregion

        private void PostBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Buttons.Add(pict);
            ApplicationBar.Buttons.Add(camera);
        }

        private void PostBox_LostFocus(object sender, RoutedEventArgs e)
        {
            while (ApplicationBar.Buttons.Count > 0)
            {
                ApplicationBar.Buttons.RemoveAt(0);
            }
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
        }

        private void pct_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(e.ChosenPhoto);
                bitmapImage = bmp;

                using (Stream filestream = e.ChosenPhoto)
                {
                    buffer = new byte[filestream.Length];
                    filestream.Read(buffer, 0, (int)filestream.Length);
                    Debug.WriteLine(buffer.Length);
                }


                PostAttachments.Items.Add(bmp);
                PostBox.Focus();
            }
        }

        void pict_Click(object sender, EventArgs e)
        {
            PhotoChooserTask pct = new PhotoChooserTask();
            pct.Completed += new EventHandler<PhotoResult>(pct_Completed);
            pct.Show();
        }

        void camera_Click(object sender, EventArgs e)
        {
            CameraCaptureTask pct = new CameraCaptureTask();
            pct.Completed += new EventHandler<PhotoResult>(pct_Completed);
            pct.Show();
        }

        #region получаем ссылку для загрузки

        private void GetWallUploadServerCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/photos.getWallUploadServer?uid={0}&access_token={1}", Client.Instance.Access_token.uid, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareGetWallUploadServer), web);
            progressBar1.IsIndeterminate = true;
        }
        private void ResponcePrepareGetWallUploadServer(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseString = responseReader.ReadToEnd();

            try
            {
                JObject o = JObject.Parse(responseString);
                uploadUrl = (string)o["response"]["upload_url"];
                Debug.WriteLine(uploadUrl);
                UploadCallback(uploadUrl, buffer);
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }

        #endregion


        #region заливаем на сервер

        private void UploadCallback(string url, byte[] data)
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(url);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), web);
            progressBar1.IsIndeterminate = true;
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // End the operation
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            string postData = "photo=";
            byte[] postdata = Encoding.UTF8.GetBytes(postData);

            byte[] byteArray = new byte[postdata.Length + buffer.Length];
            Array.Copy(postdata, 0, byteArray, 0, postdata.Length);
            Array.Copy(buffer, 0, byteArray, postdata.Length, buffer.Length);

            // Write to the request stream.
            
            postStream.Write(postdata, 0, postdata.Length);
            Debug.WriteLine(postStream.Length);
            postStream.Close();

            // Start the asynchronous operation to get the response
            request.BeginGetResponse(new AsyncCallback(ResponceUpload), request);
        }

        private void ResponceUpload(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseString = responseReader.ReadToEnd();

            try
            {
                JObject o = JObject.Parse(responseString);
                Debug.WriteLine("ответ сервера: ");
                Debug.WriteLine(o.ToString());
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message); progressBar1.IsIndeterminate = false; });
            }
        }

        #endregion

    }
}