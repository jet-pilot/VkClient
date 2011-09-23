using System;
using System.Collections.Generic;
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
using WinPhoneApp.Data.Friend;
using WinPhoneApp.Data;
using System.IO;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data.Profile;

namespace WinPhoneApp
{
    public partial class FriendListPage : PhoneApplicationPage
    {
        private FriendList fl = new FriendList();
        private FriendList ofl = new FriendList();

        public FriendListPage()
        {
            InitializeComponent();
            ListFriendsCallback();
        }


        private void ListFriendsCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/friends.get?access_token={0}&fields=first_name,last_name,photo,contacts", Client.Instance.Access_token.token));
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
                JArray responseFriends = (JArray)o["response"];
                foreach (var item in responseFriends)
                {
                    fl.Add(new MyProfile((string)item["first_name"], (string)item["last_name"], (string)item["photo"], (int)item["uid"]));
                    if ((int)item["online"] == 1) { ofl.Add(new MyProfile((string)item["first_name"], (string)item["last_name"], (string)item["photo"], (int)item["uid"])); }
                }
                this.Dispatcher.BeginInvoke(() =>
                    {
                        Friends.ItemsSource = fl;
                        FriendsPanel.Header += "(" + fl.Count + ")";
                        OnlineFriends.ItemsSource = ofl;
                        OnLineFriendsPanel.Header += "(" + ofl.Count + ")";
                        progressBar1.IsIndeterminate = false;
                    });
            }
            catch
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show("Друзья не загрузились"); progressBar1.IsIndeterminate = false; });
            }
        }

        private void Navigate_to_Search(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SearchFriend.xaml", UriKind.Relative));
        }

        private void NavigateToProfile(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MyProfile item = ((FrameworkElement) sender).DataContext as MyProfile;
            NavigationService.Navigate(new Uri("/ProfilePage.xaml?uid=" + item.Uid, UriKind.Relative));
        }

    }
}