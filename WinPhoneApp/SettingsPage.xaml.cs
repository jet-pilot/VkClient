using System.Collections.Generic;
using Microsoft.Phone.Controls;

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
    }
}