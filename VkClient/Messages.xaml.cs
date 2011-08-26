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
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Collections;
using VkClient.Classes.Message;

namespace VkClient
{
    public partial class Messages : PhoneApplicationPage
    {

        ApplicationBarIconButton select;

        ApplicationBarIconButton delete;

        ApplicationBarMenuItem markAsRead;

        ApplicationBarMenuItem markAsUnread;

        MessageItemList oloo = new MessageItemList();

        public Messages()
        {
            InitializeComponent();
            oloo.ListDialogsCallback();
            EmailList.ItemsSource = oloo.GetMessages();

            select = new ApplicationBarIconButton();
            select.IconUri = new Uri("/Images/ApplicationBar.Select.png", UriKind.RelativeOrAbsolute);
            select.Text = "select";
            select.Click += select_Click;

            delete = new ApplicationBarIconButton();
            delete.IconUri = new Uri("/Images/ApplicationBar.Delete.png", UriKind.RelativeOrAbsolute);
            delete.Text = "delete";
            delete.Click += delete_Click;

            markAsRead = new ApplicationBarMenuItem();
            markAsRead.Text = "mark as read";
            markAsRead.Click += markAsRead_Click;

            markAsUnread = new ApplicationBarMenuItem();
            markAsUnread.Text = "mark as unread";
            markAsUnread.Click += markAsUnread_Click;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (EmailList.IsSelectionEnabled)
            {
                EmailList.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }

        void select_Click(object sender, EventArgs e)
        {
            EmailList.IsSelectionEnabled = true;
        }

        void delete_Click(object sender, EventArgs e)
        {
            IList source = EmailList.ItemsSource as IList;
            while (EmailList.SelectedItems.Count > 0)
            {
                source.Remove((VkClient.Data.EmailObject)EmailList.SelectedItems[0]);
            }
        }

        void markAsRead_Click(object sender, EventArgs e)
        {
            //foreach (EmailObject obj in MessagesList.SelectedItems)
            //{
            //    obj.Unread = false;
            //}

            //MessagesList.IsSelectionEnabled = false;
        }

        void markAsUnread_Click(object sender, EventArgs e)
        {
            //foreach (EmailObject obj in MessagesList.SelectedItems)
            //{
            //    obj.Unread = true;
            //}

            //MessagesList.IsSelectionEnabled = false;
        }

        private void EmailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void EmailList_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
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
            //EmailObject item = ((FrameworkElement)sender).DataContext as EmailObject;
            //if (MessagesList.IsSelectionEnabled)
            //{
            //    MultiselectItem container = MessagesList.ItemContainerGenerator.ContainerFromItem(item) as MultiselectItem;
            //    if (container != null)
            //    {
            //        container.IsSelected = !container.IsSelected;
            //    }
            //}
            //else
            //{
            //    item.Unread = false;
            //}
        }

    }
}