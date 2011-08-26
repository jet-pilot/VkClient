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
using System.ComponentModel;

namespace VkClient.Classes.Message
{
    public class MessageItem : INotifyPropertyChanged
    {
        private bool _unread;
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Time { get; set; }

        public bool Unread
        {
            get { return _unread; }
            set
            {
                _unread = value;
                NotifyPropertyChanged("Unread");
            }
        }

        public MessageItem(string sender, string subject, string body)
        {
            Sender = sender;
            Subject = subject;
            Body = body;
        }

        public MessageItem(string sender, string subject, string body, string time, bool unread)
        {
            Sender = sender;
            Subject = subject;
            Body = body;
            Time = time;
            Unread = unread;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
