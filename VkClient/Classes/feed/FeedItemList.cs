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
using System.Collections.Generic;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace VkClient.Classes.feed
{
    public class FeedItemList
    {
        private List<FeedItem> items = new List<FeedItem>();
        private object syncRoot = new Object();


        public ReadOnlyCollection<FeedItem> GetItems()
        {
            List<FeedItem> items;

            lock (this.syncRoot)
            {
                items = new List<FeedItem>(this.items);
            }

            return new ReadOnlyCollection<FeedItem>(items);
        }

        public void AddRange(IEnumerable<FeedItem> newItems)
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

        private void AddItemInterval(FeedItem newItem)
        {
            foreach (var item in this.items)
            {
                if (item.post_id == newItem.post_id)
                {
                    return;
                }
            }

            this.items.Add(newItem);
            //this.items.Insert(0, newItem);
        }
    }
}
