using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Updates;
using TLSharp.Core;

namespace telegramClient
{
     class Updater
    {
        private TelegramClient client;
     public   Updater(TelegramClient _client)
        {
            client = _client;
        }
        private Task<TLState> offset;
        private  async Task<TLState> GetOffset() => await client.SendRequestAsync<TLState>(new TLRequestGetState());



        public Task Worker()
        {
            if (client.IsUserAuthorized())
            {
                offset = Task.Run(GetOffset);
                while (true)
                {
                    
                    Thread.Sleep(500);
                    Task getUpdates = GetUpdates(offset.Result.Date, offset.Result.Pts, offset.Result.Qts);
                    // getUpdates.Wait();
                    Console.WriteLine("smth happening");
                }
            }
            return null;
        }

        private  async Task GetUpdates(int date, int pts, int qts)
        {
            var req = new TLRequestGetDifference() { Date = date, Pts = pts, Qts = qts };
            if (await client.SendRequestAsync<TLAbsDifference>(req) is TLDifference diff)
            {
                foreach (var upd in diff.NewMessages)
                {
                    offset.Result.Pts++;
                    var msg = upd as TLMessage;
                    // Console.WriteLine(msg.Message);
                    System.Windows.Forms.MessageBox.Show(msg.Message);
                }
            }
            await client.SendPingAsync();
        }


    }



}
