using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using TeleSharp.TL.Upload;
using TeleSharp.TL.Updates;
using TLSharp.Core.Utils;
using System.Threading;
using System.Windows.Forms;
using TLSharp.Core.Exceptions;

namespace telegramClient
{
    class MTelegramClient
    {
        private static int api_id = 0;
        private static string hash = "";
        private string hashAuth = "";
        TLUser owner = null;
        internal TelegramClient telegramClient { get; set; } = null;
        internal List<TLUser> tLUsersList { get; }
        internal List<TLChannel> tLChannelsList { get; }
        internal Dictionary<long, TLFile> icons { get; }

        public int NumberChoosenUser { get; set; } = -1;
        public int NumberChoosenChat { get; set; } = -1;

        public TLState State { get; set; } = null;
        public TLAbsDifference Difference { get; set; } = null;

        public bool NeedPassw { get; set; } = false;
        private bool IsPhoto(string filename)
        {
            string ext = filename.Substring(filename.LastIndexOf('.'));
            if (ext == ".jpg" || ext == ".png" || ext == ".gif" || ext == ".bmp")
                return true;
            else
                return false;
        }

        private bool IsTxt(string filename)
        {
            string ext = filename.Substring(filename.LastIndexOf('.'));
            if (ext == ".txt" || ext == ".doc" || ext == ".docx" || ext == ".rtf")
                return true;
            else
                return false;
        }

        public MTelegramClient()
        {
            tLUsersList = new List<TLUser>(20);
            icons = new Dictionary<long, TLFile>(20);
            tLChannelsList = new List<TLChannel>(20);
            try
            {
            telegramClient = new TelegramClient(api_id, hash);

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                Application.Exit();
                return;
            }

        }

        public bool Connect()
        {
            telegramClient.ConnectAsync().Wait();
            if (!telegramClient.IsUserAuthorized())
            {
                MessageBox.Show("Нужно зарегестрироваться");
                return false;
            }
            return true;
        }
        async public Task<Task> GetHashAuth(string number)
        {
            try
            {
                hashAuth = await telegramClient.SendCodeRequestAsync("+7 " + number);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);


            }
            return Task.Run(() => Thread.Sleep(1000));
        }

        async public Task MakeAuthWithoutPassword(string numberPhone, string code)
        {
            try
            {
                owner = await telegramClient.MakeAuthAsync("+7 " + numberPhone, hashAuth, code);
            }
            catch (CloudPasswordNeededException)
            {

                MessageBox.Show("На данном аккаунте включена двухфакторная авторизация");
                NeedPassw = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Неверно ввели смс");
            }
        }

        public TLAbsMessages LoadChatHistory()
        {
            int offset_id = 0;  ////int Only return messages starting from the specified message ID
            int offset_date = 0;//int Only return messages sent before the specified date
            int add_offset = 0;//int Number of list elements to be skipped, negative values are also accepted.
            int limit = 50;    //int //Number of results to return
            int max_id = -1;    //int //If a positive value was transferred, the method will return only messages with IDs less than max_id
            int min_id = -1;    //int //If a positive value was transferred, the method will return only messages with IDs more than min_id
            try
            {

                var tlAbsMessages = telegramClient.GetHistoryAsync(
                              new TLInputPeerChannel
                              {
                                  ChannelId = tLChannelsList[NumberChoosenChat].Id,
                                  AccessHash = tLChannelsList[NumberChoosenChat].AccessHash.Value

                              }, offset_id, offset_date, add_offset, limit, max_id, min_id).Result; 
                return tlAbsMessages;

            }
            catch (Exception)
            {
                Console.WriteLine("228gb, hello");
                Thread.Sleep(1500);
                Application.Exit();   

            }
            return null;
        }

      async public Task<TLAbsMessages> LoadUserDialogHistory()
        {
            int offset_id = 0;  ////int Only return messages starting from the specified message ID
            int offset_date = 0;//int Only return messages sent before the specified date
            int add_offset = 0;//int Number of list elements to be skipped, negative values are also accepted.
            int limit = 50;    //int //Number of results to return
            int max_id = -1;    //int //If a positive value was transferred, the method will return only messages with IDs less than max_id
            int min_id = -1;    //int //If a positive value was transferred, the method will return only messages with IDs more than min_id

            try
            {
                var tlAbsMessages = await telegramClient.GetHistoryAsync(
                              new TLInputPeerUser
                              {
                                  UserId = tLUsersList[NumberChoosenUser].Id,
                                  AccessHash = tLUsersList[NumberChoosenUser].AccessHash.Value


                              }, offset_id, offset_date, add_offset, limit, max_id, min_id);
                return tlAbsMessages;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ex from LoadUserDialogHistory" + ex.Message);
                Application.Exit();

            }
            return null;
        }
        async public Task<int> MakeAuthWithPassword(string passwordStr)
        {
            var passwordSettings = await telegramClient.GetPasswordSetting();

            owner = await telegramClient.MakeAuthWithPasswordAsync(passwordSettings, passwordStr);
            Connect();
            return 0;
        }
        async public Task LoadUserAndChatLists()
        {
            if (telegramClient.IsUserAuthorized())
            {
                var contacts = await telegramClient.GetContactsAsync();
                var tLDialogs = await telegramClient.GetUserDialogsAsync() as TLDialogs;

                var chats = tLDialogs.Chats.OfType<TLChannel>().ToList();

                try
                {
                    int result = contacts.Users.Count;

                    for (int i = contacts.Users.Count - 1; i >= 0; i--)
                    {
                        TLUser tLUser = contacts.Users[i] as TLUser;

                        tLUsersList.Add(tLUser);

                        var photo = tLUser.Photo as TLUserProfilePhoto;
                        if (photo == null)
                        {
                            icons.Add(tLUser.Id, null);
                        }

                        else
                        {
                            var photoLocation = (TLFileLocation)photo.PhotoBig;
                            TLFile file = null;
                            try
                            {
                                file = telegramClient.GetFile(new TLInputFileLocation()
                                {
                                    LocalId = photoLocation.LocalId,
                                    Secret = photoLocation.Secret,
                                    VolumeId = photoLocation.VolumeId
                                }, 1024 * 256).Result;
                            }

                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            icons.Add(tLUser.Id, file);
                        }
                    }
                    for (int i = 0; i < chats.Count; i++)
                    {

                        TLChannel channel = chats[i] as TLChannel;

                        tLChannelsList.Add(channel);
                        Thread.Sleep(100);

                        var photo = channel.Photo as TLChatPhoto;
                        if (photo == null)
                        {
                            icons.Add(channel.Id, null);
                        }

                        else
                        {
                            var photoLocation = (TLFileLocation)photo.PhotoBig;
                            TLFile file = null;
                            try
                            {
                                file = telegramClient.GetFile(new TLInputFileLocation()
                                {
                                    LocalId = photoLocation.LocalId,
                                    Secret = photoLocation.Secret,
                                    VolumeId = photoLocation.VolumeId
                                }, 1024 * 256).Result;


                            }
                            catch (Exception)
                            {
                            }
                            icons.Add(channel.Id, file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("1 " + ex.Message);
                    Application.Exit();
                }
            }
        }
        async public Task GetUpdates()
        {
            try
            {
                if (State == null)
                {
                    State = await telegramClient.SendRequestAsync<TLState>(new TLRequestGetState() { });
                    return;
                }
                var req = new TLRequestGetDifference()
                {
                    Date = State.Date,
                    Pts = State.Pts,
                    Qts = State.Qts,
                    PtsTotalLimit = State.UnreadCount
                };

                var adiff = await telegramClient.SendRequestAsync<TLAbsDifference>(req);

                if (!(adiff is TLDifferenceEmpty))
                {
                    if (adiff is TLDifference)
                    {
                        var diff = adiff as TLDifference;
                        State = await telegramClient.SendRequestAsync<TLState>(new TLRequestGetState() { });
                        Difference = adiff as TLDifference;
                        return;
                    }
                    else if (adiff is TLDifferenceTooLong)
                    {
                        MessageBox.Show("TLDifferenceTooLong");
                    }
                    else if (adiff is TLDifferenceSlice)
                    {
                        MessageBox.Show("TLDifferenceSlice");
                    }
                }
            }
            catch (Exception ex)
            {
                await Task.Delay(1000);
                Difference = null;
                State = null;
                MessageBox.Show($"from GetUpdates {ex.Message}");
                Thread.Sleep(5000);
                //Application.Exit();
            }
            Difference = null;
        }
        async public void SendMessageToUser(string msg)
        {

            if (NumberChoosenUser == -1)
                throw new Exception("User haven't choosen");
            TLUser tLUser = tLUsersList[NumberChoosenUser];

            try
            {
                await telegramClient.SendMessageAsync(new TLInputPeerUser() { UserId = tLUser.Id }, msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"from SendMessageToUser {ex.Message}");
                Application.Exit();
            }
        }
        async public Task SendFileToUser(FileDialog fileDialog, System.IO.StreamReader reader)
        {
            int last = fileDialog.FileName.LastIndexOf('\\');

            string filename = fileDialog.FileName.Substring(last + 1);
            var fileResult = await telegramClient.UploadFile(filename, reader);

            if (IsPhoto(filename))
            {
                await Task.Delay(1000);
                await telegramClient.SendUploadedPhoto(new TLInputPeerUser()
                {
                    UserId = tLUsersList[NumberChoosenUser].Id
                }, fileResult, filename);
                MessageBox.Show("File has been sent successfully");
            }
            else if (IsTxt(filename))
            {
                await Task.Delay(1000);
                TLDocumentAttributeFilename s = new TLDocumentAttributeFilename();
                s.FileName = filename;

                TLVector<TLAbsDocumentAttribute> attrs = new TLVector<TLAbsDocumentAttribute>() { s };
               // attrs.ToList().Add(s);
                
                await telegramClient.SendUploadedDocument(new TLInputPeerUser()
                {
                    UserId = tLUsersList[NumberChoosenUser].Id
                }, fileResult, filename, "text/plain", attrs
                );
                MessageBox.Show("File has been sent successfully");
            }
        }
    }
}
