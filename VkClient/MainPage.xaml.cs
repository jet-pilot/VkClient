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
using VkClient.Classes.Profile;
using System.Windows.Media.Imaging;

namespace VkClient
{
    public partial class MainPage : PhoneApplicationPage
    {
        static accessInfoBag token = accessInfoStore.Load();
                
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.Unloaded += new RoutedEventHandler(MainPage_Unloaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            VkTools.Instance.ProfileChanged += new EventHandler(ProfileChanged);
            VkTools.Instance.ActiveChanged += new EventHandler(VkToolsActiveChanged);
            VkTools.Instance.FeedChanged += new EventHandler(FeedChanged);

            this.UpdateUI();
            this.feedListBox.ItemsSource = VkTools.Instance.Feeds.GetItems();
            //this.lf_name.Text = VkTools.Instance.user.first_name + " " + VkTools.Instance.user.last_name;
            //ImageSource image = new BitmapImage(new Uri(VkTools.Instance.user.photo));
            //this.avatar.Source = image;
            
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            VkTools.Instance.FeedChanged -= FeedChanged;
            VkTools.Instance.ProfileChanged -= ProfileChanged;
        }

        private void VkToolsActiveChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() => { this.UpdateUI(); });
        }

        private void UpdateUI()
        {
            var started = VkTools.Instance.Active;
            this.mainPane.Visibility = started ? Visibility.Visible : Visibility.Collapsed;
            this.unauthorizedPane.Visibility = started ? Visibility.Collapsed : Visibility.Visible;
        }

        #region SignIn
        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            this.LocalSignIn();
        }

        private void LocalSignIn()
        {
            NavigationService.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));
        }
        #endregion

        private void FeedChanged(object sender, EventArgs e)
        {
            var items = VkTools.Instance.Feeds.GetItems();
            this.Dispatcher.BeginInvoke(() => { this.feedListBox.ItemsSource = items; });
            
        }

        private void ProfileChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
                {
                    this.lf_name.Text = VkTools.Instance.user.first_name + " " + VkTools.Instance.user.last_name;
                    ImageSource image = new BitmapImage(new Uri(VkTools.Instance.user.photo));
                    this.avatar.Source = image;
                });
        }

        //#region тестим HttpWebRequest/HttpWebResponse новости
        
        //string responseStringfeed;

        //private void onfeedload()
        //{
        //    HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/newsfeed.get.xml?uid={0}&filters=post&access_token={1}", token.uid, token.token));
        //    web.Method = "POST";
        //    web.ContentType = "application/x-www-form-urlencoded";
        //    web.BeginGetRequestStream(RequestPrepare, web);
        //    progressBar1.IsIndeterminate = true;
        //}

        //private void RequestPrepare(IAsyncResult e)
        //{
        //    HttpWebRequest request = (HttpWebRequest)e.AsyncState;
        //    request.BeginGetResponse(new AsyncCallback(ResponsePrepare), request);
        //}

        //private void ResponsePrepare(IAsyncResult e)
        //{
        //    HttpWebRequest request = (HttpWebRequest)e.AsyncState;
        //    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

        //    StreamReader responseReader = new StreamReader(response.GetResponseStream());

        //    responseStringfeed = responseReader.ReadToEnd();

        //    this.Dispatcher.BeginInvoke(() =>
        //        {
        //            XElement xmlFeeds = XElement.Parse(responseStringfeed);
        //            //feedListBox.ItemsSource = from feed in xmlFeeds.Descendants("item")
        //            //                          select new FeedItem
        //            //                          {
        //            //                              text = feed.Element("text").Value,
                                                 
        //            //                          };
        //            //MessageBox.Show(responseStringfeed);

        //            XElement newxmlFeeds = new XElement("response",
        //                from item in xmlFeeds.Element("items").Elements("item")
        //                join profile in xmlFeeds.Element("profiles").Elements("user")
        //                on (string)item.Element("source_id") equals
        //                    (string)profile.Element("uid")
        //                select new XElement("feed",
        //                    new XElement("first_name", profile.Element("first_name").Value +" "+ profile.Element("last_name").Value),
        //                    new XElement("photo", profile.Element("photo").Value),
        //                    new XElement("text", item.Element("text").Value)
        //                    )
        //                        );
        //            feedListBox.ItemsSource = from feed in newxmlFeeds.Descendants("feed")
        //                                      select new FeedItem
        //                                      {
        //                                          first_name = feed.Element("first_name").Value,
        //                                          text = feed.Element("text").Value,
        //                                          photo = feed.Element("photo").Value
        //                                      };
        //            progressBar1.IsIndeterminate = false;
        //        });
            
        //}

        //#endregion
        
    }
}