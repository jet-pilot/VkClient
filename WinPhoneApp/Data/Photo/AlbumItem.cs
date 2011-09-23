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

namespace WinPhoneApp.Data.Photo
{
    public class AlbumList : ObservableCollection<AlbumItem>
    {
        public AlbumList()
            : base()
        {

        }
    }

    public class AlbumItem : INotifyPropertyChanged
    {
        private string _aid;
        private string _thumbId;
        private string _ownerId;
        private string _title;
        private string _description;
        private DateTime _created;
        private DateTime _updated;
        private string _size;
        private string _privacy;
        private string _cover;

        public string Aid
        {
            get { return _aid; }
            set
            { _aid = value; NotifyPropertyChanged("Aid"); }
        }

        public string ThumbId
        {
            get { return _thumbId; }
            set { _thumbId = value; NotifyPropertyChanged("ThumbId"); }
        }

        public string OwnerId
        {
            get { return _ownerId; }
            set { _ownerId = value; NotifyPropertyChanged("OwnerId"); }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged("Title"); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; NotifyPropertyChanged("Description"); }
        }

        public DateTime Created
        {
            get { return _created; }
            set { _created = value; NotifyPropertyChanged("Created"); }
        }

        public DateTime Updated
        {
            get { return _updated; }
            set { _updated = value; NotifyPropertyChanged("Updated"); }
        }

        public string Size
        {
            get { return _size; }
            set { _size = value; NotifyPropertyChanged("Size"); }
        }

        public string Privacy
        {
            get { return _privacy; }
            set { _privacy = value; NotifyPropertyChanged("Privacy"); }
        }

        public string Cover
        {
            get { return _cover; }
            set { _cover = value; NotifyPropertyChanged("Cover"); }
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
