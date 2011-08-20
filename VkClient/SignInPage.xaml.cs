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
using VkClient.Classes.Auth;
using VkClient.Classes;

namespace VkClient
{
    public partial class SignInPage : PhoneApplicationPage
    {
        // Constructor
        public SignInPage()
        {
            InitializeComponent();
            this.Loaded+=new RoutedEventHandler(SignIn_Loaded);
        }

        void SignIn_Loaded(object sender, RoutedEventArgs e)
        {
            
            this.Dispatcher.BeginInvoke(() =>
                {
                    var authUri = OAuthRequest.GetAuthUrl();
                    webBrowser.Navigate(new Uri(authUri));
                });
        }

        private void SignInButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    var accessInfo = new accessInfoBag
                    {
                        token = webBrowser.Source.Fragment.Substring(14, 63),
                        uid = webBrowser.Source.Fragment.Substring(103)
                    };
                    accessInfoStore.Save(accessInfo);
                    VkTools.Instance.Start(accessInfo);
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                });
        }

    }
}