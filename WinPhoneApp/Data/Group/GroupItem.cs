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
using WinPhoneApp.Data.Profile;

namespace WinPhoneApp.Data.Group
{
    public class GroupList : ObservableCollection<GroupItem>
    {
        public GroupList()
            : base()
        {

        }
    }

    public class SubscriptionList : ObservableCollection<MyProfile>
    {
        public SubscriptionList()
            : base()
        {

        }
    }

    public class GroupItem : INotifyPropertyChanged
    {
        private int _gid;
        private string _name;
        private string _screenName;
        private int _isClosed;
        private string _type;
        private string _photo;
        private string _photoMedium;
        private string _photoBig;
        private int _countMember;

        public GroupItem()
        {            
        }

        public GroupItem(int gid, string name, string screenname, int isclosed, string type, string photo, string photomedium, string photobig)
        {
            this._gid = gid;
            this._name = name;
            this._screenName = screenname;
            this._isClosed = isclosed;
            this._type = type;
            this._photo = photo;
            this._photoMedium = photomedium;
            this._photoBig = photobig;
        }

        public int Gid
        {
            get { return _gid; }
            set
            {
                _gid = value;
                NotifyPropertyChanged("Gid");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string ScreenName
        {
            get { return _screenName; }
            set
            {
                _screenName = value;
                NotifyPropertyChanged("ScreenName");
            }
        }

        public int IsClosed
        {
            get { return _isClosed; }
            set
            {
                _isClosed = value;
                NotifyPropertyChanged("IsClosed");
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                NotifyPropertyChanged("Type");
            }
        }

        public string Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                NotifyPropertyChanged("Photo");
            }
        }

        public string PhotoMedium
        {
            get { return _photoMedium; }
            set
            {
                _photoMedium = value;
                NotifyPropertyChanged("PhotoMedium");
            }
        }

        public string PhotoBig
        {
            get { return _photoBig; }
            set
            {
                _photoBig = value;
                NotifyPropertyChanged("PhotoBig");
            }
        }

        public int CountMember
        {
            get { return _countMember; }
            set
            {
                _countMember = value;
                NotifyPropertyChanged("CountMember");
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
