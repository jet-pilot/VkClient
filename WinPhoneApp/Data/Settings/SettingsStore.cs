using System.IO.IsolatedStorage;

namespace WinPhoneApp.Data.Settings
{
    public static class SettingsStore
    {
        private const string UpdateTimeKey = "updatetimekey";
        private const string ConfirmExitKey = "confirmexitley";

        public static void Save(SettingsBag obj)
        {
            IsolatedStorageSettings.ApplicationSettings[UpdateTimeKey] = obj.UpdateTime;
            IsolatedStorageSettings.ApplicationSettings[ConfirmExitKey] = obj.ConfirmExit;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static void SaveDefaults()
        {
            IsolatedStorageSettings.ApplicationSettings[UpdateTimeKey] = 3;
            IsolatedStorageSettings.ApplicationSettings[ConfirmExitKey] = true;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static SettingsBag Load()
        {
            int updateTime;
            bool confirmExit;

            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(UpdateTimeKey, out updateTime))
            {
                return null;
            }
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(ConfirmExitKey, out confirmExit))
            {
                return null;
            }

            return new SettingsBag
                       {
                           UpdateTime = updateTime,
                           ConfirmExit = confirmExit
                       };
        }
    }
}
