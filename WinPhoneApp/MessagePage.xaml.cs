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
using WinPhoneApp.Data.Message;
using System.IO;
using WinPhoneApp.Data;
using Newtonsoft.Json.Linq;

namespace WinPhoneApp
{
    public partial class MessagePage : PhoneApplicationPage
    {
        private MessageItem message = new MessageItem();
        public MessagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string mid = "";

            if (NavigationContext.QueryString.TryGetValue("mid", out mid))

            { message.Mid = Convert.ToInt32(mid); ListMessagesCallback(); }
        }


        private void ListMessagesCallback()
        {
            string requestString = string.Format("https://api.vkontakte.ru/method/messages.getById?access_token={0}&mid={1}", Client.Instance.Access_token.token, message.Mid);
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

            JObject o = JObject.Parse(responseStringStatus);
            JArray responseArray = (JArray)o["response"];
            try
            {
                this.Dispatcher.BeginInvoke(() =>
                    {
                        this.message.Uid = (int)responseArray[1]["uid"];
                        this.message.Title = (string)responseArray[1]["title"];
                        this.message.Body = (string)responseArray[1]["body"];
                        ProfileCallback();
                        this.progressBar1.IsIndeterminate = false;
                    });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(ex.Message);
                    });
            }
        }


        private void ProfileCallback()
        {
            string requestString = string.Format("https://api.vkontakte.ru/method/getProfiles?access_token={0}&uid={1}", Client.Instance.Access_token.token, message.Uid);
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareProfile), web);
            this.progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareProfile(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringStatus = responseReader.ReadToEnd();

            JObject o = JObject.Parse(responseStringStatus);
            JArray responseArray = (JArray)o["response"];
            try
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    this.message.Name = (string)responseArray[0]["first_name"] + " " + (string)responseArray[0]["last_name"];
                    this.name.Text = message.Name;
                    this.title.Text = message.Title;
                    this.body.Text = message.Body;
                    this.progressBar1.IsIndeterminate = false;
                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(ex.Message);
                });
            }
        }

    }
}