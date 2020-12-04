using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GC
{
    public partial class ClientGUI : Form
    {
        readonly ClientLogic clientLogic;
        public ClientGUI(string username, string url, string file, string masterHostname)
        {
            InitializeComponent();
            this.Text += $" {username}";

            clientLogic = new ClientLogic(this, username, url, masterHostname);
            Task.Run(() => clientLogic.ExecuteCommands(file));
        }

        public void PostLogtoGUI(string m)
        {
            logs.Text = $"{m}\r\n{logs.Text}";
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            clientLogic.ServerShutdown();
        }

        private void BtRead_Click(object sender, EventArgs e)
        {
            clientLogic.ReadObject(pid.Text, objid.Text, serverid.Text);
        }

        private void BtWrite_Click(object sender, EventArgs e)
        {
            clientLogic.WriteObject(pid.Text, objid.Text, newvalue.Text);
        }

        private void Listbt_Click(object sender, EventArgs e)
        {
            if (pid.Text.Length == 0)
                clientLogic.ListGlobal();
            else
                clientLogic.ListServer(pid.Text);
        }
    }
}
