using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WinPhoneApp.Data.Auth
{
    public class AuthorizeInfo
    {
        public virtual bool OAuthCallbackConfirmed { get; set; }
        public static string client_id = "2449855";
        public static string auth_uri = "http://api.vk.com/oauth/authorize";
        public static string redirect_uri = "http://api.vk.com/blank.html";
        public static string display = "wap";
        public static string scope = "13318";
        public static string response_type = "token";

        public static string GetAuthUrl()
        {
            return string.Format("{0}?client_id={1}&redirect_uri={2}&scope={3}&display={4}&response_type={5}", auth_uri, client_id, redirect_uri, scope, display, response_type);
        }
    }
}
