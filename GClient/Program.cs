using System;
using System.Windows.Forms;

namespace GC
{
    static class Program {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            string masterHostname = args[0];
            string username = args[1];
            string url = args[2];
            string file = args[3];

            AllocConsole();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientGUI(username, url, file, masterHostname));

            while (true) ;
        }
    }
}
