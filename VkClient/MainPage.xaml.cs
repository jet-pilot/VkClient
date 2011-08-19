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
using VkClient.Classes;
using VkClient.Classes.Auth;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Linq;
using VkClient.Classes.feed;

namespace VkClient
{
    public partial class MainPage : PhoneApplicationPage
    {
        static accessInfoBag token = accessInfoStore.Load();
                
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            VkTools.Instance.ActiveChanged += new EventHandler(VkToolsActiveChanged);
            if (token != null) { this.onfeedload(); }
            this.UpdateUI();
        }

        private void VkToolsActiveChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() => { this.UpdateUI(); this.onfeedload(); });
        }

        private void UpdateUI()
        {
            var started = VkTools.Instance.Active;
            this.mainPane.Visibility = started ? Visibility.Visible : Visibility.Collapsed;
            this.unauthorizedPane.Visibility = started ? Visibility.Collapsed : Visibility.Visible;
            token = accessInfoStore.Load();
        }

        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            this.LocalSignIn();
        }

        private void LocalSignIn()
        {
            NavigationService.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));
        }


        #region тестим HttpWebRequest/HttpWebResponse новости

        string responseStringfeed;

        //string request = string.Format("https://api.vkontakte.ru/method/newsfeed.get.xml?uid={0}&filters=post&access_token={1}", token.uid, token.token);

        private void onfeedload()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/newsfeed.get.xml?uid={0}&filters=post&access_token={1}", token.uid, token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetRequestStream(RequestPrepare, web);
            progressBar1.IsIndeterminate = true;
        }

        private void RequestPrepare(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            request.BeginGetResponse(new AsyncCallback(ResponsePrepare), request);
        }

        private void ResponsePrepare(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            responseStringfeed = responseReader.ReadToEnd();

            this.Dispatcher.BeginInvoke(() =>
                {
                    XElement xmlFeeds = XElement.Parse(responseStringfeed);
                    feedListBox.ItemsSource = from feed in xmlFeeds.Descendants("item")
                                              select new FeedItem
                                              {
                                                  text = feed.Element("text").Value
                                              };
                    progressBar1.IsIndeterminate = false;
                });
            
        }

        #endregion
        
    }
}