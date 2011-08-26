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
using WinPhoneApp.Data.Photo;

namespace WinPhoneApp.Data.Feed
{
    public class FeedPhotoList : ObservableCollection<FeedPhotoItem>
    {
        public FeedPhotoList()
            : base()
        {

        }
    }

    public class FeedPhotoItem:INotifyPropertyChanged
    {
        private string _source_id;
        private string _date;
        public PhotoItemList Photos = new PhotoItemList();

        public FeedPhotoItem(string source_id,string date,PhotoItemList photos)
        {
            this._source_id = source_id;
            this._date = date;
            Photos = photos;
        }

        public string SourceId
        {
            get { return _source_id; }
            set { _source_id = value;
                NotifyPropertyChanged("SourceId");
            }
        }

        public string Date
        {
            get { return _date; }
            set { _date = value;
                NotifyPropertyChanged("Date");
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
