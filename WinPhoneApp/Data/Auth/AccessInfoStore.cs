﻿using System.IO.IsolatedStorage;

namespace WinPhoneApp.Data.Auth
{
    public static class AccessInfoStore
    {
        private const string tokenkey = "tokenkey";
        private const string uidkey = "uidkey";

        public static void Save(AccessInfoBag obj)
        {
            IsolatedStorageSettings.ApplicationSettings[tokenkey] = obj.token;
            IsolatedStorageSettings.ApplicationSettings[uidkey] = obj.uid;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static AccessInfoBag Load()
        {
            string Token;
            string Uid;

            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(tokenkey, out Token))
            {
                return null;
            }

            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(uidkey, out Uid))
            {
                return null;
            }

            return new AccessInfoBag
            {
                token = Token,
                uid = Uid
            };
        }
    }
}
