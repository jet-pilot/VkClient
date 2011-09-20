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
using System.IO;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data.Friend;
using WinPhoneApp.Data.Profile;
using WinPhoneApp.Data;

namespace WinPhoneApp
{
    public partial class SearchFriend : PhoneApplicationPage
    {
        private FriendList sfl = new FriendList();
        private FriendList el = new FriendList();
        public SearchFriend()
        {
            InitializeComponent();
        }

        private void Search_Action(object sender, EventArgs e)
        {
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
                    sfl.Add(new MyProfile((string)item["first_name"], (string)item["last_name"], (string)item["photo"]));
                }
                this.Dispatcher.BeginInvoke(() =>
                {
                    Friends.ItemsSource = sfl;
                    progressBar1.IsIndeterminate = false;
                });
            }
            catch
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show("Друзья не загрузились"); progressBar1.IsIndeterminate = false; });
            }
        }
    }
}