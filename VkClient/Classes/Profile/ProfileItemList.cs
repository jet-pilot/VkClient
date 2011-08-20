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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VkClient.Classes.Profile
{
    public class ProfileItemList
    {
        List<ProfileItem> items=new List<ProfileItem>();
        private object syncRoot = new Object();


        public ReadOnlyCollection<ProfileItem> GetItems()
        {
            List<ProfileItem> items;

            lock (this.syncRoot)
            {
                items = new List<ProfileItem>(this.items);
            }

            return new ReadOnlyCollection<ProfileItem>(items);
        }

        public void AddRange(IEnumerable<ProfileItem> newItems)
        {
            lock (this.syncRoot)
            {
                //foreach (FeedItem item in newItems)
                //{
                //    this.AddItemInterval(item);
                //}
                this.items.AddRange(newItems);
            }
        }

        private void AddItemInterval(ProfileItem newItem)
        {
            foreach (var item in this.items)
            {
                this.items.Add(newItem);
            }

        }
    }
}
