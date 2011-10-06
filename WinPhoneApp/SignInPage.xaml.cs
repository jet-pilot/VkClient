using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Client.Instance.Active != true) return;
            NavigationService.GoBack();
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
                        NavigationService.GoBack();
                        //NavigationService.Navigate(new System.Uri("/MainPage.xaml", System.UriKind.Relative));
                    }
                });

        }


        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            Client.Instance.exit = true;
        }

    }
}