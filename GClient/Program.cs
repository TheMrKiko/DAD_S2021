using System;
using System.Windows.Forms;

namespace GC
{
    static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            string masterHostname = args[0];
            string username = args[1];
            string url = args[2];
            string file = args[3];
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientGUI(username, url, file, masterHostname));

            while (true) ;
        }
    }
}
