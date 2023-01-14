using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Updates;
using TeleSharp.TL.Upload;
using TLSharp.Core;

namespace telegramClient
{
    static class ModelView
    {


        public static void setContactsListView(ListView userListView, ListView channelsListView, MTelegramClient mTelegramClient)
        {
            List<TLUser> tLUsersList = mTelegramClient.tLUsersList;
            TelegramClient telegramClient = mTelegramClient.telegramClient;
            Dictionary<long, TLFile> icons = mTelegramClient.icons;

            userListView.Columns[0].Width = userListView.Width;
            ImageList userImageList = new ImageList();
            for (int i = 0; i < tLUsersList.Count; i++)
            {
                userListView.Items.Add($"{tLUsersList[i].FirstName}  {tLUsersList[i].LastName}");
                var photo = icons[tLUsersList[i].Id];
                if (photo == null)
                {
                    userImageList.Images.Add(Image.FromFile(@"E:\NetLab\telegramClient\userDefaultIcon.png"));
                    userListView.Items[i].ImageIndex = i;
                }

                else
                {
                    using (var m = new MemoryStream(photo.Bytes))
                    {
                        var img = Image.FromStream(m);

                        userImageList.Images.Add(img);
                        userListView.Items[i].ImageIndex = i;
                    }
                }
            }

            userImageList.ImageSize = new Size(32, 32);
            userListView.SmallImageList = userImageList;

            ImageList channelImageList = new ImageList();
            channelsListView.Columns[0].Width = channelsListView.Width;

            for (int i = 0; i < mTelegramClient.tLChannelsList.Count; i++)
            {
                channelsListView.Items.Add($"{mTelegramClient.tLChannelsList[i].Title}");
                var photo = icons[mTelegramClient.tLChannelsList[i].Id];
                if (photo == null)
                {
                    channelImageList.Images.Add(Image.FromFile(@"E:\NetLab\telegramClient\chatDefaultIcon.png"));
                    channelsListView.Items[i].ImageIndex = i;
                }

                else
                {
                    using (var m = new MemoryStream(photo.Bytes))
                    {
                        var img = Image.FromStream(m);
                        channelImageList.Images.Add(img);
                        channelsListView.Items[i].ImageIndex = i;
                    }
                }
            }
            channelImageList.ImageSize = new Size(32, 32);
            channelsListView.SmallImageList = channelImageList;
        }

        private static DateTime getTime(int time)
        {
            var mdt = new DateTime(1970, 1, 1, 0, 0, 0);

            return mdt.AddSeconds(time).ToLocalTime();

        }

        public static void setDialogHistory(ListView listView, TLAbsMessages tLAbsMessages, Image userIcon)
        {

            listView.Columns.Add("Имя");
            listView.Columns[0].Width = listView.Width / 3;
            listView.Columns.Add("Сообщение");
            listView.Columns[1].Width = listView.Width * 2 / 3;

            listView.SmallImageList.Images.Clear();

            listView.SmallImageList.Images.Add(userIcon);


            TLVector<TLAbsMessage> tLMessages = null;
            int count = 0;

            if (tLAbsMessages is TLMessagesSlice)
            {
                tLMessages = (tLAbsMessages as TLMessagesSlice).Messages;
                count = (tLAbsMessages as TLMessagesSlice).Messages.Count;
            }
            else
            {
                tLMessages = (tLAbsMessages as TLMessages).Messages;
                count = (tLAbsMessages as TLMessages).Messages.Count;
            }
            if (count == 0)
                return;

            for (int i = count - 1; i >= 0; i--)
            {

                var msg = tLMessages[i] as TLMessage;
                listView.Items.Add(getTime(msg.Date).ToString());
                if (msg.Message.Length == 0)
                {
                    listView.Items[count - (i + 1)].SubItems.Add("Контент");
                }

                else
                    listView.Items[count - (i + 1)].SubItems.Add(msg.Message);

                if (msg.Out)
                {
                    listView.Items[count - (i + 1)].BackColor = Color.Honeydew;
                }
                else
                {
                    listView.Items[count - (i + 1)].ImageIndex = 0;
                }
            }
            listView.Items[listView.Items.Count - 1].EnsureVisible();
        }
        public static void setChanndelHistory(ListView listView, TLAbsMessages tLAbsMessages, Image userIcon)
        {
            listView.Columns.Add("Имя");
            listView.Columns[0].Width = listView.Width / 3;
            listView.Columns.Add("Сообщение");
            listView.Columns[1].Width = listView.Width * 2 / 3;

            listView.SmallImageList.Images.Clear();

            listView.SmallImageList.Images.Add(userIcon);


            TLVector<TLAbsMessage> tLMessages = null;
            int count = 0;

            tLMessages = (tLAbsMessages as TLChannelMessages).Messages;
            count = (tLAbsMessages as TLChannelMessages).Messages.Count;

            if (count == 0)
                return;

            for (int i = count - 1; i >= 0; i--)
            {

                var msg = tLMessages[i] as TLMessage;
                if (msg == null)
                {
                    count--;
                    continue;
                    
                }
                listView.Items.Add(
                    getTime(msg.Date).ToString()
                    );


                if (msg.Message.Length == 0)
                {
                    listView.Items[count - (i + 1)].SubItems.Add("Контент");
                }

                else
                    listView.Items[count - (i + 1)].SubItems.Add(msg.Message);

                if (msg.Out)
                {

                    listView.Items[count - (i + 1)].BackColor = Color.Honeydew;
                }
                else
                {
                    listView.Items[count - (i + 1)].ImageIndex = 0;
                }
            }

            listView.Items[listView.Items.Count - 1].EnsureVisible();
        }
        internal static void ShowNewInfo(TLAbsDifference absDiff)
        {
            if (absDiff == null)
                return;

            StringBuilder message = new StringBuilder();

            var diff = absDiff as TLDifference;

            if (diff.Chats.ToList().Count > 0)
            {
                message.Append("Новое сообщение в следующих чатах:\r\n");
                var chats = diff.Chats.ToList();
                for (int i = 0; i < chats.Count; i++)
                {
                    var chat = chats[i] as TLChat;
                    if (chat == null)
                        continue;

                    message.AppendLine(chat.Title);
                }
            }

            if (diff.NewMessages.ToList().Count > 0)
            {
                message.Append("Новые сообщения от пользователей:\r\n");
                var chats = diff.NewMessages.ToList();
                for (int i = 0; i < chats.Count; i++)
                {
                    var msg = chats[i] as TLMessage;
                    if (msg == null)
                        continue;

                    TLUser from = diff.Users[i] as TLUser;
                    if (msg.Message.Length == 0)
                    {
                        if (from.Username!=null && from.Username.Length != 0)
                            message.AppendLine(from.Username + " Контент");
                        else
                            message.AppendLine($"{from.FirstName} {from.LastName}");
                    }
                    else
                    {
                        if (diff.Users.ToList().Count == 2)
                        {
                            message.Append(
                                $"from { (diff.Users[0] as TLUser).FirstName }  {(diff.Users[0] as TLUser).LastName} " +
                                $" to  { (diff.Users[1] as TLUser).FirstName}  {(diff.Users[1] as TLUser).LastName}");
                        }
                        else
                            message.Append(
                                $"{ (diff.Users[0] as TLUser).FirstName}  {(diff.Users[0] as TLUser).FirstName}");
                        message.AppendLine(msg.Message);
                    }
                }
            }
            if (message.Length != 0)
                MessageBox.Show(message.ToString());
        }
    }
}
