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
using WinPhoneApp.Data.Auth;
using WinPhoneApp.Data;

namespace WinPhoneApp
{
    public partial class SignInPage : PhoneApplicationPage
    {
        public SignInPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SignInPage_Loaded);
        }

        void SignInPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
                {
                    var authUri = AuthorizeInfo.GetAuthUrl();
                    webBrowser.Navigate(new Uri(authUri));
                });
        }

        private void SignInButton_Click(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
                {
                    var accessInfo = new AccessInfoBag
                    {
                        token = webBrowser.Source.Fragment.Substring(14, 63),
                        uid = webBrowser.Source.Fragment.Substring(103)
                    };
                    AccessInfoStore.Save(accessInfo);
                    Client.Instance.Start(accessInfo);
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                });
        }
    }
}