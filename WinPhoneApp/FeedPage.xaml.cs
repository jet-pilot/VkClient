using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json.Linq;
using WinPhoneApp.Data;
using WinPhoneApp.Data.Feed;
using WinPhoneApp.Data.Photo;

namespace WinPhoneApp
{
    public partial class FeedPage : PhoneApplicationPage
    {
        private FeedItem FeedItem { get; set; }
        private string _uid;
        private string _postId;
        private readonly List<int> _uidlist;
        public FeedPage()
        {
            _uidlist = new List<int>();
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!NavigationContext.QueryString.TryGetValue("uid", out _uid)) return;
            if (!NavigationContext.QueryString.TryGetValue("post_id", out _postId)) return;
            ListFeedsCallback();
        }

        /// <summary>
        /// получаем новость
        /// </summary>
        private void ListFeedsCallback()
        {
            if (FeedItem != null) { return; }
            FeedItem = new FeedItem();
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vk.com/method/wall.getById?posts={0}_{1}&access_token={2}", _uid, _postId, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
            {
                var request = (HttpWebRequest)e.AsyncState;
                var response = (HttpWebResponse)request.EndGetResponse(e);
                var responseReader = new StreamReader(response.GetResponseStream());
                var responseString = responseReader.ReadToEnd();

                try
                {
                    var o = JObject.Parse(responseString);
                    var responseFeed = (JArray)o["response"];

                    foreach (var feed in responseFeed)
                    {
                        var attachments = feed.SelectToken("attachments", false);
                        FeedItem.Date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble((int)feed["date"]));
                        FeedItem.CntComments = "комментариев: " + (int)feed["comments"]["count"];
                        FeedItem.Text = (string)feed["text"];
                        if ((string)feed["post_source"]["data"] == "profile_photo")
                        {
                            FeedItem.Text = "обновил(а) фотографию на странице";
                        }
                        if (attachments != null)
                        {
                            foreach (var attachment in attachments)
                            {
                                switch ((string)attachment["type"])
                                {
                                    case "photo":
                                        {
                                            if (FeedItem.Image == null) { FeedItem.Image = new PhotoItemList(); }
                                            var image = attachment.SelectToken("photo", false);
                                            if (image != null)
                                            {
                                                FeedItem.Image.Add(new PhotoItem((int)image["pid"], (int)image["owner_id"], (string)image["src"], (string)image["src_big"]));
                                            }

                                            break;
                                        }
                                    case "link":
                                        {
                                            if (FeedItem.Link == null) { FeedItem.Link = new LinkItemList(); }
                                            var link = attachment.SelectToken("link", false);
                                            if (link != null)
                                            {
                                                FeedItem.Link.Add(new LinkItem((string)link["url"], (string)link["title"], (string)link["description"], (string)link["image_src"]));
                                            }
                                            break;
                                        }
                                    case "audio":
                                        {
                                            if (FeedItem.Audio == null) { FeedItem.Audio = new AudioItemList(); }
                                            var audio = attachment.SelectToken("audio", false);
                                            if (audio != null)
                                            {
                                                FeedItem.Audio.Add(new AudioItem());
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        FeedItem.Uid = (int)feed["from_id"];
                        _uidlist.Add(FeedItem.Uid);
                        FeedItem.Text = FeedItem.Text.Replace("<br>", "\n");

                    }
                    Dispatcher.BeginInvoke(ListCommentsCallback);
                }
                catch (Exception exception)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(exception.Message);
                        progressBar1.IsIndeterminate = false;
                    });
                }
            }, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }

        /// <summary>
        /// получаем профили
        /// </summary>
        private void ListProfileCallback()
        {
            var requestString = string.Format("https://api.vk.com/method/getProfiles?access_token={0}&fields=photo&uids=", Client.Instance.Access_token.token);
            requestString = _uidlist.Aggregate(requestString, (current, item) => current + ("," + item));
            var web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
            {
                var request = (HttpWebRequest)e.AsyncState;
                var response = (HttpWebResponse)request.EndGetResponse(e);
                var responseReader = new StreamReader(response.GetResponseStream());

                try
                {
                    var responseStringStatus = responseReader.ReadToEnd();
                    var o = JObject.Parse(responseStringStatus);
                    var responseArray = (JArray)o["response"];
                    foreach (var uid in responseArray.Where(uid => FeedItem.Uid == (int)uid["uid"]))
                    {
                        FeedItem.Author = (string)uid["first_name"] + " " + (string)uid["last_name"]; FeedItem.Avatar = (string)uid["photo"]; break;
                    }

                    foreach (var comment in FeedItem.Comments)
                    {
                        foreach (var user in responseArray.Where(user => comment.Uid == (int)user["uid"]))
                        {
                            comment.FullName = (string)user["first_name"] + " " + (string)user["last_name"];
                            comment.Photo = (string)user["photo"];
                            break;
                        }
                    }

                    Dispatcher.BeginInvoke(() =>
                                               {
                                                   FeedPanel.DataContext = FeedItem;
                                                   progressBar1.IsIndeterminate = false;
                                               });
                }
                catch (Exception exception)
                {
                    Dispatcher.BeginInvoke(() =>
                                               {
                                                   MessageBox.Show(exception.Message);
                                                   progressBar1.IsIndeterminate = false;
                                               });
                }
            }, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }

        /// <summary>
        /// получаем комменты
        /// </summary>
        private void ListCommentsCallback()
        {
            var requestString = string.Format("https://api.vk.com/method/wall.getComments?access_token={0}&owner_id={1}&post_id={2}&sort=desc", Client.Instance.Access_token.token, _uid, _postId);
            requestString = _uidlist.Aggregate(requestString, (current, item) => current + ("," + item));
            var web = (HttpWebRequest)WebRequest.Create(requestString);
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
            {
                var request = (HttpWebRequest)e.AsyncState;
                var response = (HttpWebResponse)request.EndGetResponse(e);
                var responseReader = new StreamReader(response.GetResponseStream());

                try
                {
                    var responseString = responseReader.ReadToEnd();
                    var o = JObject.Parse(responseString);
                    try
                    {

                        var responseArray = (JArray)o["response"];
                        for (var i = 1; i < responseArray.Count; i++)
                        {
                            var commentItem = new CommentItem
                                                  {
                                                      Cid = (int)responseArray[i]["cid"],
                                                      Uid = (int)responseArray[i]["uid"],
                                                      Date =
                                                          new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(
                                                              Convert.ToDouble((int)responseArray[i]["date"])),
                                                      Text = (string)responseArray[i]["text"]
                                                  };
                            //if ((int)responseArray[i]["reply_to_uid"] > 0) { commentItem.ReplyToUid = (int)responseArray[i]["reply_to_uid"]; }
                            //if ((int)responseArray[i]["reply_to_cid"] > 0) { commentItem.ReplyToCid = (int)responseArray[i]["reply_to_cid"]; }
                            FeedItem.Comments.Add(commentItem);
                            _uidlist.Add((int)responseArray[i]["uid"]);
                        }
                    }
                    catch
                    {
                        switch ((int)o["error"]["error_code"])
                        {
                            case 212:
                                {
                                    Dispatcher.BeginInvoke(() =>
                                                               {
                                                                   CommentBox.Visibility = Visibility.Collapsed;
                                                                   MessageBox.Show("Вы не можете читать и оставлять комментарии");
                                                               });
                                    break;
                                }
                        }
                    }
                    Dispatcher.BeginInvoke(ListProfileCallback);
                }
                catch (Exception exception)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show(exception.Message));
                }
            }, web);
            if (progressBar1.IsIndeterminate == false)
            {
                progressBar1.IsIndeterminate = true;
            }
        }

        /// <summary>
        /// переход по ссылке в посте
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HyperlinkButtonClick(object sender, RoutedEventArgs e)
        {
            var wbt = new WebBrowserTask();
            var btn = (HyperlinkButton)e.OriginalSource;
            wbt.Uri = btn.NavigateUri;
            wbt.Show();
        }

        #region отправка сообщения на стену
        private void CommentSend(object sender, EventArgs e)
        {
            if (CommentBox.Text.Length > 0)
            {
                var text = CommentBox.Text.Replace("#", "№");
                CommentSendCallback(text);
            }
            else
            {
                Dispatcher.BeginInvoke(
                    () => MessageBox.Show("Невозможно отправить пустое сообщение, напишите хоть что-нибудь"));
            }
        }

        private void CommentSendCallback(string message)
        {
            var web = (HttpWebRequest)WebRequest.Create(string.Format("https://api.vkontakte.ru/method/wall.addComment?owner_id={0}&post_id={1}&text={2}&access_token={3}", _uid, _postId, message, Client.Instance.Access_token.token));
            web.Method = "POST";
            web.ContentType = "application/x-www-form-urlencoded";
            web.BeginGetResponse(delegate(IAsyncResult e)
                                     {
                                         var request = (HttpWebRequest)e.AsyncState;
                                         var response = (HttpWebResponse)request.EndGetResponse(e);
                                         var responseReader = new StreamReader(response.GetResponseStream());
                                         var responseString = responseReader.ReadToEnd();
                                         try
                                         {
                                             var o = JObject.Parse(responseString);
                                             Dispatcher.BeginInvoke(() =>
                                             {
                                                 var CId = (int)o["response"]["cid"];
                                                 Debug.WriteLine(CId.ToString());
                                                 CommentBox.Text = "";
                                                 progressBar1.IsIndeterminate = false;
                                             });
                                         }
                                         catch (Exception ex)
                                         {
                                             Dispatcher.BeginInvoke(() => MessageBox.Show(ex.Message));
                                         }
                                     }, web);
            progressBar1.IsIndeterminate = true;
        }
        #endregion

    }
}