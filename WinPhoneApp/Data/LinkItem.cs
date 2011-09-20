using System;
using System.Net;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WinPhoneApp.Data
{
    public class LinkItemList : ObservableCollection<LinkItem>
    {
        public LinkItemList()
            : base()
        {

        }
    }

    public class LinkItem : INotifyPropertyChanged
    {
        private string _url;
        private string _title;
        private string _description;
        private string _image_src;

        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                NotifyPropertyChanged("Url");
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
        

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }
        

        public string Image_src
        {
            get { return _image_src; }
            set 
            {
                _image_src = value;
                NotifyPropertyChanged("Image_src");
            }
        }

        public LinkItem(string url, string title, string description, string image_src)
        {
            this._url = url;
            this._title = title;
            this._description = description;
            this._image_src = image_src;
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
