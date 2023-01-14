using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace telegramClient
{
    static class Program
    {

        //:149.154.167.40:443
        //Production configuration:149.154.167.50:443
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// 
        /// 
        
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
