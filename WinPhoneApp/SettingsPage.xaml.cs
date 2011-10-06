using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Interop;
using Microsoft.Phone.Controls;
using WinPhoneApp.Data;
using WinPhoneApp.Data.Settings;

namespace WinPhoneApp
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            DataContext = this;
            InitializeComponent();
        }

        public IList<string> UpdateTimeList
        {
            get
            {
                return new List<string>
                {
                    "3 минуты",
                    "5 минут",
                    "10 минут",
                    "никогда"
                };
            }
        }

        private void ListPicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var updateTimeIndex = (int)UpdateTimePicker.SelectedIndex;
            var updateTime = 0;
            switch (updateTimeIndex)
            {
                case 0:
                    {
                        Client.Instance.refreshTimer.Change(0, 3 * 1000);
                        break;
                    }
                case 1:
                    {
                        Client.Instance.refreshTimer.Change(0, 5 * 1000);
                        break;
                    }
                case 2:
                    {
                        Client.Instance.refreshTimer.Change(0, 10 * 1000);
                        break;
                    }
                case 3:
                    {;
                        Client.Instance.refreshTimer.Change(-1, -1);
                        break;
                    }
            }
            SettingsStore.SaveUpdateTime(updateTime);
        }
    }
}