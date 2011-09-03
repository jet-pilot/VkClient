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
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WinPhoneApp.Data.Message
{
    public class MessageList : ObservableCollection<MessageItem>
    {
        public MessageList()
            : base()
        {
        }


    }

    public class MessageItem : INotifyPropertyChanged
    {
        private int _mid;
        private int _uid;
        private string _title;
        private string _body;
        private bool _unread;
        private string _name;
        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                NotifyPropertyChanged("Date");
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public MessageItem()
        {
        }

        public MessageItem(int mid, int uid, string title, string body, bool unread, DateTime date)
        {
            this._mid = mid;
            this._uid = uid;
            this._title = title;
            this._body = body;
            this._unread = unread;
            this._date = date;
        }

        public bool Unread
        {
            get { return _unread; }
            set
            {
                _unread = value;
                NotifyPropertyChanged("Unread");
            }
        }

        public string Body
        {
            get { return _body; }
            set
            {
                _body = value;
                NotifyPropertyChanged("Body");
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }


        public int Uid
        {
            get { return _uid; }
            set
            {
                _uid = value;
                NotifyPropertyChanged("Uid");
            }
        }

        public int Mid
        {
            get { return _mid; }
            set
            {
                _mid = value;
                NotifyPropertyChanged("Mid");
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
