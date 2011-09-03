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
        private int _pid;
        private int _owner_id;
        private int _aid;
        private string _src;
        private string _src_big;


        public PhotoItem(int pid,int owner_id,string src,string src_big)
        {
            this._pid = pid;
            this._owner_id = owner_id;
            this._src = src;
            this._src_big = src_big;
        }

        public PhotoItem(int pid, int owner_id, int aid, string src, string src_big)
        {
            this._pid = pid;
            this._owner_id = owner_id;
            this._aid = aid;
            this._src = src;
            this._src_big = src_big;
        }

        public int Pid
        {
            get { return _pid; }
            set
            {
                _pid = value;
                NotifyPropertyChanged("Pid");
            }
        }

        public int OwnerId
        {
            get { return _owner_id; }
            set
            {
                _owner_id = value;
                NotifyPropertyChanged("Owner_id");
            }
        }

        public int Aid
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
