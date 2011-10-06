using System;
using System.Diagnostics;
using System.Threading;
using WinPhoneApp.Data.Auth;
using WinPhoneApp.Data.Settings;

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
        public SettingsBag Settings = new SettingsBag();
        public bool exit = false;
        public Timer refreshTimer;

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
            if (SettingsStore.Load() == null)
            {
                SettingsStore.SaveDefaults();
            }
            Settings = SettingsStore.Load();

            refreshTimer = new Timer(RefreshTimerCallback, null, -1, -1);
            refreshTimer.Change(0, Settings.UpdateTime * 1000);

        }

        public void Stop()
        {
            this._active = false;
            refreshTimer.Change(-1, -1);
            this.OnActiveChanged();
        }
        
        public void RefreshTimerCallback(object state)
        {
            Debug.WriteLine(DateTime.Now.ToLocalTime());
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
