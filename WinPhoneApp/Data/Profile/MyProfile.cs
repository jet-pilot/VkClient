﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace WinPhoneApp.Data.Profile
{
    public class MyProfile : INotifyPropertyChanged
    {
        private int _uid;
        private string _full_name;
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
        private string _university;
        private string _status;

        public MyProfile()
        {
        }

        public MyProfile(string first_name, string last_name, string photo, int uid)
        {
            this._first_name = first_name;
            this._last_name = last_name;
            this._photo = photo;
            this._uid = uid;
        }

        public MyProfile(string first_name, string last_name, string photo)
        {
            this._first_name = first_name;
            this._last_name = last_name;
            this._photo = photo;
            this._status = "";
            this._full_name = this._first_name + " " + this._last_name;
        }

        public MyProfile(string first_name, string last_name, string photo, string mobile_phone, string home_phone)
        {
            this._first_name = first_name;
            this._last_name = last_name;
            this._photo = photo;
            this._mobile_phone = mobile_phone;
            this._home_phone = home_phone;
        }

        public int Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }

        public string Full_name
        {
            get { return _full_name; }
            set { _full_name = value; NotifyPropertyChanged("Full_name"); }
        }

        public string First_name
        {
            get { return _first_name; }
            set { _first_name = value; NotifyPropertyChanged("First_name"); }
        }

        public string Last_name
        {
            get { return _last_name; }
            set { _last_name = value; NotifyPropertyChanged("Last_name"); }
        }

        public string Nickname
        {
            get { return _nickname; }
            set { _nickname = value; NotifyPropertyChanged("Nickname"); }
        }

        public string Sex
        {
            get { return _sex; }
            set { _sex = value; NotifyPropertyChanged("Sex"); }
        }

        public string Bdate
        {
            get { return _bdate; }
            set { _bdate = value; NotifyPropertyChanged("Bdate"); }
        }

        public string City
        {
            get { return _city; }
            set { _city = value; NotifyPropertyChanged("City"); }
        }

        public string Country
        {
            get { return _country; }
            set { _country = value; NotifyPropertyChanged("Country"); }
        }

        public string Timezone
        {
            get { return _timezone; }
            set { _timezone = value; }
        }

        public string Photo
        {
            get { return _photo; }
            set
            {
                _photo = value == "http://vk.com/images/question_c.gif" ? "/Images/question_a.jpg" : value;
                NotifyPropertyChanged("Photo");
            }
        }

        public string Photo_medium
        {
            get { return _photo_medium; }
            set { _photo_medium = value; NotifyPropertyChanged("Photo_medium"); }
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
            set
            {
                _mobile_phone = value;
                NotifyPropertyChanged("Mobile_phone");
            }
        }

        public string University
        {
            get { return _university; }
            set { _university = value; NotifyPropertyChanged("University"); }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
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
