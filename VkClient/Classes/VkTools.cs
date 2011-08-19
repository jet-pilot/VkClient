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
using System.Threading;
using VkClient.Classes.Auth;

namespace VkClient.Classes
{
    public class VkTools
    {
        #region Singleton

        private static volatile VkTools instance;

        private static object instanceSyncRoot = new Object();

        public static VkTools Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceSyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new VkTools();
                        }
                    }
                }

                return instance;
            }
        }

        #endregion

        private volatile bool active;
        public EventHandler ActiveChanged;
        private static readonly int RefreshInterval = 30;
        private Timer refreshTimer;

        private VkTools()
        {
            this.refreshTimer = new Timer(this.RefreshTimerCallback, null, -1, -1);
        }

        public bool Active
        {
            get { return this.active; }
        }

        private void OnActiveChanged()
        {
            var handler = this.ActiveChanged;

            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public void Start(accessInfoBag accessInfo)
        {
            if (this.active)
            {
                return;
            }

            if (accessInfo == null)
            {
                return;
            }
            
            this.StartTimer();
        }

        private void StartTimer()
        {
            this.refreshTimer.Change(0, RefreshInterval * 1000);
            this.active = true;
            this.OnActiveChanged();
        }

        private void StopTimer()
        {
            this.active = false;
            this.refreshTimer.Change(-1, -1);
            this.OnActiveChanged();
        }

        private void RefreshTimerCallback(object state)
        {
            
        }
    }
}
