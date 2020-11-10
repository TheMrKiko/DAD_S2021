using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GC
{
    public partial class ClientGUI : Form {
        readonly ClientLogic clientLogic;
        public ClientGUI(string username, string url, string file, string masterHostname) {
            InitializeComponent();

            clientLogic = new ClientLogic(this, username, url, masterHostname);
            Task.Run(() => clientLogic.ExecuteCommands(file));
        }

        public void AddMsgtoGUI(string m) { logs.Text += m + "\r\n"; }

        private void form1_Closing(object sender, FormClosingEventArgs e) {
            clientLogic.ServerShutdown();
        }

        private async void btRead_Click(object sender, EventArgs e) {
            string m = newvalue.Text;

            //await clientLogic.BcastMsg(m);
            /*tbConv.Text += "me: " + tbMsg.Text + "\r\n";
            tbMsg.Text = "";*/
            clientLogic.ReadObject(pid.Text, objid.Text, serverid.Text);
        }

        private void btWrite_Click(object sender, EventArgs e)
        {
            /*foreach (string nick in clientLogic.Register(tbNick.Text, tbPort.Text)) {
                tbRegResult.Text += nick + "\r\n";
            }*/
            clientLogic.WriteObject(pid.Text, objid.Text, newvalue.Text);
        }
    }
}
