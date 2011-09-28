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
using WinPhoneApp.Data.Friend;
using WinPhoneApp.Data.Photo;

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
        private DateTime _date;
        private PhotoItemList _image;
        private LinkItemList _link;
        private AudioItemList _audio;
        private FriendList _friendList;
        private int _uid;

        public FeedItem()
        {
            FriendList = new FriendList();
        }
        
        public FriendList FriendList
        {
            get { return _friendList; }
            set
            {
                _friendList = value;
                NotifyPropertyChanged("FriendList");
            }
        }

        public AudioItemList Audio
        {
            get { return _audio; }
            set
            {
                _audio = value;
                NotifyPropertyChanged("Audio");
            }
        }

        public LinkItemList Link
        {
            get { return _link; }
            set
            {
                _link = value;
                NotifyPropertyChanged("Link");
            }
        }

        public PhotoItemList Image
        {
            get { return _image; }
            set
            {
                _image = value;
                NotifyPropertyChanged("Image");
            }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                NotifyPropertyChanged("Date");
            }
        }
        /*Конструктор для поста без аттача*/
        public FeedItem(string author, string avatar, string text, DateTime date)
        {
            this._author = author;
            this._avatar = avatar;
            this._text = text;
            this._date = date;
        }
        /*Конструктор для поста с аттачем*/
        public FeedItem(string author, string avatar, string text, DateTime date, PhotoItemList image, LinkItemList link, AudioItemList audio)
        {
            this._author = author;
            this._avatar = avatar;
            this._text = text;
            this._date = date;
            this._image = image;
            this._link = link;
            this._audio = audio;
        }

        /*Конструктор для стены без аттача*/
        public FeedItem(string author, string avatar, string text, DateTime date, int uid)
        {
            this._author = author;
            this._avatar = avatar;
            this._text = text;
            this._date = date;
            this._uid = uid;
        }
        /*Конструктор для стены с аттачем*/
        public FeedItem(string author, string avatar, string text, DateTime date, PhotoItemList image, LinkItemList link, AudioItemList audio, int uid)
        {
            this._author = author;
            this._avatar = avatar;
            this._text = text;
            this._date = date;
            this._image = image;
            this._link = link;
            this._audio = audio;
            this._uid = uid;
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

        public int Uid
        {
            get { return _uid; }
            set
            {
                _uid = value;
                NotifyPropertyChanged("Uid");
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
