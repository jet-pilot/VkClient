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
using System.Collections.ObjectModel;

namespace WinPhoneApp.Data
{

    public class AudioItemList : ObservableCollection<AudioItem>
    {
        public AudioItemList()
            : base()
        {

        }
    }

    public class AudioItem
    {
        private int _aid;
        private int _owner_id;
        private string _artist;
        private string _title;
        private int _duration;
        private string _url;

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public int Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }

        public int Owner_id
        {
            get { return _owner_id; }
            set { _owner_id = value; }
        }

        public int Aid
        {
            get { return _aid; }
            set { _aid = value; }
        }

        public AudioItem(string Url, string Title, string Artist, int Duration, int Owner_id, int Aid)
        {
            this._aid = Aid;
            this._artist = Artist;
            this._duration = Duration;
            this._owner_id = Owner_id;
            this._title = Title;
            this._url = Url;
        }

        public AudioItem()
        {
        }
    }
}
