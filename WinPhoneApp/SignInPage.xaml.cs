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

        private void webBrowser1_Navigating(object sender, NavigatingEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() => { this.progressBar1.IsIndeterminate = true; });
        }

        private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
                {
                    this.progressBar1.IsIndeterminate = false;
                    if (webBrowser.Source.AbsolutePath == "/blank.html")
                    {
                        this.webBrowser.Visibility = Visibility.Collapsed;
                        var accessInfo = new AccessInfoBag
                        {
                            token = webBrowser.Source.Fragment.Substring(14, 63),
                            uid = webBrowser.Source.Fragment.Substring(103)
                        };
                        AccessInfoStore.Save(accessInfo);
                        Client.Instance.Start(accessInfo);
                        NavigationService.Navigate(new System.Uri("/MainPage.xaml", System.UriKind.Relative));
                    }
                });

        }
    }
}