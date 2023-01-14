using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Updates;
using TeleSharp.TL.Upload;
using TLSharp.Core;
using TLSharp.Core.Exceptions;


namespace telegramClient
{


    public partial class Form1 : Form
    {
        static Mutex mutex = new Mutex();
        MTelegramClient mTelegramClient = null;

        public Form1()
        {

            InitializeComponent();
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(50, 50);
            listView2.SmallImageList = imageList;
            listViewUserList.Columns.Add("Контакты");
            listViewChannels.Columns.Add("Каналы");
        }

        async private void Form1_Load(object sender, EventArgs e)
        {
            mTelegramClient = new MTelegramClient();
            bool IsAuth = mTelegramClient.Connect();
            
            if (IsAuth)
            {
                hideFields();    
                await mTelegramClient.LoadUserAndChatLists();


                Task.WaitAll();
                ModelView.setContactsListView(listViewUserList, listViewChannels, mTelegramClient);
                mTelegramClient.State = await mTelegramClient.telegramClient.SendRequestAsync<TLState>(new TLRequestGetState());
                startProcessUpdates();
            }

            else
            {
                textBoxCode.Hide();
                textBoxPassword.Hide();
                buttonLogin.Hide();
                buttonLoginWithPassword.Hide();
                labelCodeFromSMS.Hide();
                labelPassword.Hide();
            }
        }

        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            
            await mTelegramClient.MakeAuthWithoutPassword(maskedTextBox1.Text, textBoxCode.Text);
            if (mTelegramClient.telegramClient.IsUserAuthorized())
            {
                
                
                hideFields();
                await mTelegramClient.LoadUserAndChatLists();

                Task.WaitAll();
                ModelView.setContactsListView(listViewUserList, listViewChannels, mTelegramClient);
                mTelegramClient.State = await mTelegramClient.telegramClient.SendRequestAsync<TLState>(new TLRequestGetState());
                startProcessUpdates();
            }
            else
            {
                if (mTelegramClient.NeedPassw)
                {
                    textBoxPassword.Show();
                    buttonLoginWithPassword.Show();
                    labelPassword.Show();
                    textBoxCode.ReadOnly = true;
                    buttonLogin.Enabled = false;
                    
                }
            }
        }

        private async void buttonGetCode_Click(object sender, EventArgs e)
        {

            if (maskedTextBox1.Text.Length == 0)
            {
                MessageBox.Show("Вы не ввели номер");
            }
            Task task = await mTelegramClient.GetHashAuth(maskedTextBox1.Text);
            task.Wait();


            textBoxCode.Show();
            buttonLogin.Show();
            labelCodeFromSMS.Show();

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // var dialogs = await telegramClient.GetUserDialogsAsync();

        }

        async private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listViewUserList.SelectedItems.Count == 0)
                return;


            listView2.Clear();

            mTelegramClient.NumberChoosenUser = listViewUserList.SelectedItems[0].Index;
            mutex.WaitOne();
            timerUpdate.Stop();
            Thread.Sleep(500);
            TLAbsMessages tlAbsMessages = await mTelegramClient.LoadUserDialogHistory();

            if (tlAbsMessages == null)
            {
                MessageBox.Show("TlAbsMessages was null");
                Application.Exit();
            }

            var dialog = tlAbsMessages as TLMessagesSlice;

            if (dialog == null)
            {
                var msgs = tlAbsMessages as TLMessages;
                ModelView.setDialogHistory(listView2, 
                    msgs, listViewUserList.SelectedItems[0].ImageList.Images[mTelegramClient.NumberChoosenUser]);
            }
            else
            ModelView.setDialogHistory(listView2, dialog, listViewUserList.SelectedItems[0].ImageList.Images[mTelegramClient.NumberChoosenUser]);

            Thread.Sleep(1500);
            mutex.ReleaseMutex();
            timerUpdate.Start();

        }



        private void timerUpdate_Tick(object sender, EventArgs e)
        {



        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBoxMsg.Text.Length == 0)
            {
                MessageBox.Show("Пустое сообщение");
                return;
            }


            if (mTelegramClient.NumberChoosenUser == -1)
            {
                MessageBox.Show("Не выбран пользователь");
                return;
            }


            mutex.WaitOne();
            timerUpdate.Stop();
            Thread.Sleep(1000);

            mTelegramClient.SendMessageToUser(textBoxMsg.Text);

            MessageBox.Show("Сообщение успешно отправлено");
            timerUpdate.Start();

            mutex.ReleaseMutex();

            textBoxMsg.Clear();
        }

        async private void buttonSendFile_Click(object sender, EventArgs e)
        {
            if (mTelegramClient.NumberChoosenUser == -1)
            {
                MessageBox.Show("Вы не выбрали получателя");
                return;
            }

            if (openFileDialogToSend.ShowDialog() == DialogResult.OK)
            {

                string filePath = openFileDialogToSend.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialogToSend.OpenFile();
                mutex.WaitOne();
                timerUpdate.Stop();
                Thread.Sleep(2000);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    try
                    {


                        await mTelegramClient.SendFileToUser(openFileDialogToSend, reader);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }



                }
                fileStream.Close();
            }
            mutex.ReleaseMutex();
            timerUpdate.Start();
        }
        private void listViewUserList_Resize(object sender, EventArgs e)
        {
            listViewUserList.Columns[0].Width = listViewUserList.Width;
            listViewChannels.Columns[0].Width = listViewChannels.Width;
        }

        async private void buttonLoginWithPassw_Click(object sender, EventArgs e)
        {
            if (textBoxPassword.Text.Length != 0)
                await mTelegramClient.MakeAuthWithPassword(textBoxPassword.Text);

            if (mTelegramClient.telegramClient.IsUserAuthorized())
            {
                await mTelegramClient.LoadUserAndChatLists();
                
                timerUpdate.Start();
                


                Task.WaitAll();
                ModelView.setContactsListView(listViewUserList, listViewChannels, mTelegramClient);
                mTelegramClient.State = await mTelegramClient.telegramClient.SendRequestAsync<TLState>(new TLRequestGetState());
                
                
                startProcessUpdates();


            }
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listViewChannels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewChannels.SelectedItems.Count == 0)
                return;


            listView2.Clear();
            mTelegramClient.NumberChoosenChat = listViewChannels.SelectedItems[0].Index;

            mutex.WaitOne();
            timerUpdate.Stop();
            Thread.Sleep(500);

            TLAbsMessages tLAbsMessages = mTelegramClient.LoadChatHistory();

            if (tLAbsMessages == null)
            {
                mutex.ReleaseMutex();
                timerUpdate.Start();
                Application.Exit();
            }

            var dialog = tLAbsMessages as TLChannelMessages;

            if (dialog == null)
            {
                var msgs = tLAbsMessages as TLMessages;
                ModelView.setChanndelHistory(listView2, msgs, 
                    listViewChannels.SelectedItems[0].ImageList.Images[mTelegramClient.NumberChoosenChat]);
            }
            else
            {
                ModelView.setChanndelHistory(listView2, dialog, listViewChannels.SelectedItems[0].ImageList.Images[mTelegramClient.NumberChoosenChat]);
            }
                Thread.Sleep(1500);
                mutex.ReleaseMutex();
                timerUpdate.Start();
            
        }

        private void button4_Click_1(object sender, EventArgs e)
        {

            
        }
        private void startProcessUpdates()
        {
            Thread requestHandler = new Thread(() =>
            {
                while (true)
              {
                    mutex.WaitOne();
                    mTelegramClient.GetUpdates();
                    Thread.Sleep(100);
                    ModelView.ShowNewInfo(mTelegramClient.Difference);
                    mutex.ReleaseMutex();
                    Thread.Sleep(2000);
                    
                }

            });
            requestHandler.IsBackground = true;
            requestHandler.Start();
            
        }

        private void hideFields()
        {
            labelNumberPhone.Hide();
            labelPassword.Hide();
            labelCodeFromSMS.Hide();
            maskedTextBox1.Hide();
            textBoxCode.Hide();
            textBoxPassword.Hide();
            buttonLogin.Hide();
            buttonGetCode.Hide();
            buttonLoginWithPassword.Hide();
        }
    }
}
