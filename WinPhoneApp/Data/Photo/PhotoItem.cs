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
    public class PhotoItemList : ObservableCollection<PhotoItem>
    {
        public PhotoItemList()
            : base()
        {

        }
    }

    public class PhotoItem : INotifyPropertyChanged
    {
        private string _pid;
        private string _owner_id;
        private string _aid;
        private string _src;
        private string _src_big;

        public PhotoItem(string pid, string owner_id, string aid, string src, string src_big)
        {
            this._pid = pid;
            this._owner_id = owner_id;
            this._aid = aid;
            this._src = src;
            this._src_big = src_big;
        }

        public string Pid
        {
            get { return _pid; }
            set
            {
                _pid = value;
                NotifyPropertyChanged("Pid");
            }
        }

        public string OwnerId
        {
            get { return _owner_id; }
            set
            {
                _owner_id = value;
                NotifyPropertyChanged("Owner_id");
            }
        }

        public string Aid
        {
            get { return _aid; }
            set
            {
                _aid = value;
                NotifyPropertyChanged("Aid");
            }
        }


        public string Src
        {
            get { return _src; }
            set
            {
                _src = value;
                NotifyPropertyChanged("Src");
            }
        }

        public string SrcBig
        {
            get { return _src_big; }
            set
            {
                _src_big = value;
                NotifyPropertyChanged("SrcBig");
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
