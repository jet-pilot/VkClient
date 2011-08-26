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

namespace WinPhoneApp.Data.Profile
{
    public class MyProfile
    {
        private int _uid;
        private string _first_name;
        private string _last_name;
        private string _nickname;
        private string _sex;
        private string _bdate;
        private string _city;
        private string _country;
        private string _timezone;
        private string _photo;
        private string _photo_medium;
        private string _photo_big;
        private string _photo_rec;
        private string _home_phone;
        private string _mobile_phone;

        public MyProfile()
        {
        }

        public MyProfile(string first_name, string last_name, string photo)
        {
            this._first_name = first_name;
            this._last_name = last_name;
            this._photo = photo;
        }

        public int Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }

        public string First_name
        {
            get { return _first_name; }
            set { _first_name = value; }
        }

        public string Last_name
        {
            get { return _last_name; }
            set { _last_name = value; }
        }

        public string Nickname
        {
            get { return _nickname; }
            set { _nickname = value; }
        }

        public string Sex
        {
            get { return _sex; }
            set { _sex = value; }
        }

        public string Bdate
        {
            get { return _bdate; }
            set { _bdate = value; }
        }

        public string City
        {
            get { return _city; }
            set { _city = value; }
        }

        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }

        public string Timezone
        {
            get { return _timezone; }
            set { _timezone = value; }
        }

        public string Photo
        {
            get { return _photo; }
            set { _photo = value; }
        }

        public string Photo_medium
        {
            get { return _photo_medium; }
            set { _photo_medium = value; }
        }

        public string Photo_big
        {
            get { return _photo_big; }
            set { _photo_big = value; }
        }

        public string Photo_rec
        {
            get { return _photo_rec; }
            set { _photo_rec = value; }
        }

        public string Home_phone
        {
            get { return _home_phone; }
            set { _home_phone = value; }
        }

        public string Mobile_phone
        {
            get { return _mobile_phone; }
            set { _mobile_phone = value; }
        }
    }
}
