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
using WinPhoneApp.Data.Auth;

namespace WinPhoneApp.Data
{
    public sealed class Client
    {
        #region Singleton

        private static volatile Client instance;

        private static object instanceSyncRoot = new Object();

        public static Client Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceSyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new Client();
                        }
                    }
                }

                return instance;
            }
        }

        #endregion

        private volatile bool _active;
        public AccessInfoBag Access_token = new AccessInfoBag();

        public bool Active
        {
            get { return this._active; }
        }

        public event EventHandler ActiveChanged;

        public void Start(AccessInfoBag accessInfo)
        {
            if (this._active)
            {
                return;
            }

            if (accessInfo == null)
            {
                return;
            }
            this._active = true;
            this.OnActiveChanged();
            Access_token = AccessInfoStore.Load();
        }

        public void Stop()
        {
            this._active = false;
            this.OnActiveChanged();
        }

        private void OnActiveChanged()
        {
            var handler = this.ActiveChanged;

            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }
}
