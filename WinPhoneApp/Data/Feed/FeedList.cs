using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WinPhoneApp.Data.Feed
{
    public class FeedList : ObservableCollection<FeedItem>
    {
        public FeedList()
            : base()
        {

        }
    }

    public class FeedItem : INotifyPropertyChanged
    {
        private string _author;
        private string _avatar;
        private string _text;

        public FeedItem(string author, string avatar, string text)
        {
            this._author = author;
            this._avatar = avatar;
            this._text = text;
        }

        public string Author
        {
            get { return _author; }
            set
            {
                _author = value;
                NotifyPropertyChanged("Author");
            }
        }

        public string Avatar
        {
            get { return _avatar; }
            set
            {
                _avatar = value;
                NotifyPropertyChanged("Avatar");
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                NotifyPropertyChanged("Text");
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
