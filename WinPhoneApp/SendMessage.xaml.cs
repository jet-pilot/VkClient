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
using WinPhoneApp.Data;

namespace WinPhoneApp
{
    public partial class SendMessage : PhoneApplicationPage
    {
        private string Uid;
        public SendMessage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("uid", out Uid)) { PageTitle.Text = Uid; }
        }

        private void ListMessagesCallback()
        {
            string requestString = string.Format("https://api.vk.com/method/messages.send?access_token={0}&uid={1}&message={2}", Client.Instance.Access_token.token, Uid, body.Text);
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareMessage), web);
            this.progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareMessage(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringStatus = responseReader.ReadToEnd();
            try
            {
                JObject o = JObject.Parse(responseStringStatus);
                if ((int)o["response"] > 0) { Dispatcher.BeginInvoke(() => NavigationService.GoBack()); }
                else { Dispatcher.BeginInvoke(() => MessageBox.Show("сообщение не отправлено, попробуйте еще раз")); progressBar1.IsIndeterminate = false; }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(ex.Message);
                    progressBar1.IsIndeterminate = false;
                });
            }
        }

        private void Send_Click(object sender, EventArgs e)
        {
            ListMessagesCallback();
        }
    }
}