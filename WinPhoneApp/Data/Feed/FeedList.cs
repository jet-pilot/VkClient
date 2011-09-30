using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private int _postId;
        private string _author;
        private string _avatar;
        private string _text;
        private DateTime _date;
        private PhotoItemList _image;
        private LinkItemList _link;
        private AudioItemList _audio;
        private FriendList _friendList;
        private int _uid;
        private string _cntComments;

        private ObservableCollection<CommentItem> _comments;

        public ObservableCollection<CommentItem> Comments
        {
            get { return _comments; }
            set
            {
                if (_comments != value)
                {
                    _comments = value;
                    NotifyPropertyChanged("Comments");
                }
            }
        }


        public FeedItem()
        {
            FriendList = new FriendList();
            Comments=new ObservableCollection<CommentItem>();
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
            _author = author;
            _avatar = avatar;
            _text = text;
            _date = date;
        }
        /*Конструктор для поста с аттачем*/
        public FeedItem(string author, string avatar, string text, DateTime date, PhotoItemList image, LinkItemList link, AudioItemList audio)
        {
            _author = author;
            _avatar = avatar;
            _text = text;
            _date = date;
            _image = image;
            _link = link;
            _audio = audio;
        }

        /*Конструктор для стены без аттача*/
        public FeedItem(string author, string avatar, string text, DateTime date, int uid)
        {
            _author = author;
            _avatar = avatar;
            _text = text;
            _date = date;
            _uid = uid;
        }
        /*Конструктор для стены с аттачем*/
        public FeedItem(string author, string avatar, string text, DateTime date, PhotoItemList image, LinkItemList link, AudioItemList audio, int uid)
        {
            _author = author;
            _avatar = avatar;
            _text = text;
            _date = date;
            _image = image;
            _link = link;
            _audio = audio;
            _uid = uid;
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
                _avatar = value == "http://vk.com/images/question_c.gif" ? "/Images/feed_question_a.jpg" : value;
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

        public int PostId
        {
            get { return _postId; }
            set
            {
                _postId = value;
                NotifyPropertyChanged("PostId");
            }
        }

        public string CntComments
        {
            get { return _cntComments; }
            set
            {
                _cntComments = value;
                NotifyPropertyChanged("CntComments");
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

    public class CommentItem : INotifyPropertyChanged
    {
        private int _cid;

        public int Cid
        {
            get { return _cid; }
            set
            {
                if (_cid != value)
                {
                    _cid = value;
                    NotifyPropertyChanged("Cid");
                }
            }
        }

        private int _uid;

        public int Uid
        {
            get { return _uid; }
            set
            {
                if (_uid != value)
                {
                    _uid = value;
                    NotifyPropertyChanged("Uid");
                }
            }
        }

        private string _fullName;

        public string FullName
        {
            get { return _fullName; }
            set
            {
                if (_fullName != value)
                {
                    _fullName = value;
                    NotifyPropertyChanged("FullName");
                }
            }
        }

        private string _photo;

        public string Photo
        {
            get { return _photo; }
            set
            {
                if (_photo != value)
                {
                    _photo = value == "http://vk.com/images/question_c.gif" ? "/Images/question_a.jpg" : value;
                    NotifyPropertyChanged("Photo");
                }
            }
        }

        private DateTime _date;

        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    NotifyPropertyChanged("Date");
                }
            }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }

        private int _replyToCid;

        public int ReplyToCid
        {
            get { return _replyToCid; }
            set
            {
                if (_replyToCid != value)
                {
                    _replyToCid = value;
                    NotifyPropertyChanged("ReplyToCid");
                }
            }
        }

        private int _replyToUid;

        public int ReplyToUid
        {
            get { return _replyToUid; }
            set
            {
                if (_replyToUid != value)
                {
                    _replyToUid = value;
                    NotifyPropertyChanged("ReplyToUid");
                }
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
