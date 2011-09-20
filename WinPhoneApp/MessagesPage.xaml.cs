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
using Microsoft.Phone.Shell;
using WinPhoneApp.Data.Message;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data;

namespace WinPhoneApp
{
    public partial class MessagesPage : PhoneApplicationPage
    {
        private MessageList ml = new MessageList();
        private List<int> uidlist=new List<int>();
        private List<int> midlist = new List<int>();
        private bool unread = true;

        ApplicationBarIconButton select;

        ApplicationBarIconButton delete;

        ApplicationBarMenuItem markAsRead;
        
        ApplicationBarMenuItem markAsUnread;

        public MessagesPage()
        {
            InitializeComponent();
            ListMessagesCallback();

            select = new ApplicationBarIconButton();
            select.IconUri = new Uri("/Images/ApplicationBar.Select.png", UriKind.RelativeOrAbsolute);
            select.Text = "select";
            select.Click += select_Click;

            delete = new ApplicationBarIconButton();
            delete.IconUri = new Uri("/Images/ApplicationBar.Delete.png", UriKind.RelativeOrAbsolute);
            delete.Text = "delete";
            //delete.Click += delete_Click;

            markAsRead = new ApplicationBarMenuItem();
            markAsRead.Text = "отметить как прочитанные";
            markAsRead.Click += markAsRead_Click;

            markAsUnread = new ApplicationBarMenuItem();
            markAsUnread.Text = "отметить как новые";
            markAsUnread.Click += markAsUnread_Click;
        }

        private void ListMessagesCallback()
        {
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/messages.get?preview_length=15&count=50&access_token={0}", Client.Instance.Access_token.token));
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
                JArray responseArray = (JArray)o["response"];
                this.Dispatcher.BeginInvoke(() =>
                {
                    for (int i = 1; i < responseArray.Count; i++)
                    {
                        if ((int)responseArray[i]["read_state"] == 1) { unread = false; }
                        DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)responseArray[i]["date"]));
                        ml.Add(new MessageItem((int)responseArray[i]["mid"], (int)responseArray[i]["uid"], (string)responseArray[i]["title"], (string)responseArray[i]["body"], unread, date));
                        this.uidlist.Add((int)responseArray[i]["uid"]);
                    }
                    ListProfileMessagesCallback();
                    this.progressBar1.IsIndeterminate = false;
                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(() => { MessageBox.Show(ex.Message + " в получении сообщений"); this.progressBar1.IsIndeterminate = false; });
            }
        }

        private void ListProfileMessagesCallback()
        {
            string requestString = string.Format("https://api.vkontakte.ru/method/getProfiles?access_token={0}&uids=", Client.Instance.Access_token.token);
            foreach (var item in this.uidlist)
            {
                requestString += "," + item;
            }
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponcePrepareProfileMessage), web);
            this.progressBar1.IsIndeterminate = true;
        }
        public void ResponcePrepareProfileMessage(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringStatus = responseReader.ReadToEnd();
            try
            {
                JObject o = JObject.Parse(responseStringStatus);
                JArray responseArray = (JArray)o["response"];
                foreach (var item in ml)
                {
                    foreach (var uid in responseArray)
                    {
                        if (item.Uid == (int)uid["uid"]) { item.Name = uid["first_name"] + " " + uid["last_name"]; break; }
                    }
                }
                this.Dispatcher.BeginInvoke(() => { this.MessageList.ItemsSource = ml; this.progressBar1.IsIndeterminate = false; });
            }
            catch
            {

            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (MessageList.IsSelectionEnabled)
            {
                MessageList.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }

        void select_Click(object sender, EventArgs e)
        {
            MessageList.IsSelectionEnabled = true;
        }

        void markAsRead_Click(object sender, EventArgs e)
        {
            foreach (MessageItem obj in MessageList.SelectedItems)
            {
                midlist.Add(obj.Mid);
            }
            MarkAsReadCallback();

            MessageList.IsSelectionEnabled = false;
        }

        void markAsUnread_Click(object sender, EventArgs e)
        {
            foreach (MessageItem obj in MessageList.SelectedItems)
            {
                midlist.Add(obj.Mid);
            }
            MarkAsUnreadCallback();

            MessageList.IsSelectionEnabled = false;
        }


        private void MessageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MultiselectList target = (MultiselectList)sender;
            ApplicationBarIconButton i = (ApplicationBarIconButton)ApplicationBar.Buttons[0];

            if (target.IsSelectionEnabled)
            {
                ApplicationBarMenuItem j = (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];
                ApplicationBarMenuItem k = (ApplicationBarMenuItem)ApplicationBar.MenuItems[1];

                if (target.SelectedItems.Count > 0)
                {
                    i.IsEnabled = j.IsEnabled = k.IsEnabled = true;
                }
                else
                {
                    i.IsEnabled = j.IsEnabled = k.IsEnabled = false;
                }
            }
            else
            {
                i.IsEnabled = true;
            }
        }

        private void MessageList_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            while (ApplicationBar.Buttons.Count > 0)
            {
                ApplicationBar.Buttons.RemoveAt(0);
            }

            while (ApplicationBar.MenuItems.Count > 0)
            {
                ApplicationBar.MenuItems.RemoveAt(0);
            }

            if ((bool)e.NewValue)
            {
                ApplicationBar.Buttons.Add(delete);
                ApplicationBarIconButton i = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                i.IsEnabled = false;

                ApplicationBar.MenuItems.Add(markAsRead);
                ApplicationBarMenuItem j = (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];
                j.IsEnabled = false;

                ApplicationBar.MenuItems.Add(markAsUnread);
                ApplicationBarMenuItem k = (ApplicationBarMenuItem)ApplicationBar.MenuItems[1];
                k.IsEnabled = false;
            }
            else
            {
                ApplicationBar.Buttons.Add(select);
            }
        }

        private void ItemContent_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageItem item = ((FrameworkElement)sender).DataContext as MessageItem;
            if (MessageList.IsSelectionEnabled)
            {
                MultiselectItem container = MessageList.ItemContainerGenerator.ContainerFromItem(item) as MultiselectItem;
                if (container != null)
                {
                    container.IsSelected = !container.IsSelected;
                }
            }
            else
            {
                midlist.Add(item.Mid);
                MarkAsReadCallback();
                NavigationService.Navigate(new Uri("/MessagePage.xaml?mid=" + item.Mid, UriKind.Relative));
            }
        }

        #region реквест маркэзанрид

        private void MarkAsUnreadCallback()
        {
            string requestString = string.Format("https://api.vkontakte.ru/method/messages.markAsNew?access_token={0}&mids=", Client.Instance.Access_token.token);
            foreach (var item in this.midlist)
            {
                requestString += "," + item;
            }
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponceMarkAsUnread), web);
            this.progressBar1.IsIndeterminate = true;
        }
        private void ResponceMarkAsUnread(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringStatus = responseReader.ReadToEnd();

            JObject o = JObject.Parse(responseStringStatus);
            try
            {
                this.Dispatcher.BeginInvoke(() =>
                    {
                        if ((int)o["response"] == 1)
                        {
                            this.progressBar1.IsIndeterminate = false;
                            foreach (var mid in midlist)
                            {
                                foreach (var item in ml)
                                {
                                    if (mid == item.Mid) { item.Unread = true; break; }
                                }
                            }
                            midlist.Clear();
                        }
                    });
            }
            catch
            {

            }
        }

        #endregion

        #region реквест маркэзрид

        private void MarkAsReadCallback()
        {
            string requestString = string.Format("https://api.vkontakte.ru/method/messages.markAsRead?access_token={0}&mids=", Client.Instance.Access_token.token);
            foreach (var item in this.midlist)
            {
                requestString += "," + item;
            }
            HttpWebRequest web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(new AsyncCallback(ResponceMarkAsRead), web);
            this.progressBar1.IsIndeterminate = true;
        }
        private void ResponceMarkAsRead(IAsyncResult e)
        {
            HttpWebRequest request = (HttpWebRequest)e.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(e);

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            string responseStringStatus = responseReader.ReadToEnd();

            JObject o = JObject.Parse(responseStringStatus);
            try
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    if ((int)o["response"] == 1)
                    {
                        this.progressBar1.IsIndeterminate = false;
                        foreach (var mid in midlist)
                        {
                            foreach (var item in ml)
                            {
                                if (mid == item.Mid) { item.Unread = false; break; }
                            }
                        }
                        midlist.Clear();
                    }
                });
            }
            catch
            {

            }
        }

        #endregion
    }
}