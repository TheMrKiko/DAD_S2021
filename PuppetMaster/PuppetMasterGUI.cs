using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster {
    public partial class PuppetMasterGUI : Form {
        PuppetMasterLogic puppetLogic;
        public PuppetMasterGUI() {
            InitializeComponent();
            
            puppetLogic = new PuppetMasterLogic(this, "localhost", 10001);
        }

        private void btReg_Click(object sender, EventArgs e) {
            /*foreach (string nick in puppetLogic.Register(tbNick.Text, tbPort.Text)) {
                tbRegResult.Text += nick + "\r\n";
            }
            tbNick.Enabled = false;
            tbPort.Enabled = false;*/
        }

        public void AddMsgtoGUI(string m) {
            //tbConv.Text += m + "\r\n"; 
        }

        private void form1_Closing(object sender, FormClosingEventArgs e) {
            puppetLogic.ServerShutdown();
        }

        private async void btSend_Click(object sender, EventArgs e) {
            /*string m = tbMsg.Text;
            await puppetLogic.BcastMsg(m);
            tbConv.Text += "me: " + tbMsg.Text + "\r\n";
            tbMsg.Text = "";*/
        }

        private void R_send_Click(object sender, EventArgs e)
        {
            int r = int.Parse(r_r.Text);
            puppetLogic.ReplicationFactor(r);
        }

        private void P_send_Click(object sender, EventArgs e)
        {

        }

        private void Se_send_Click(object sender, EventArgs e)
        {
            puppetLogic.Server("h", s_url.Text, 1, 1);
        }

        private void C_send_Click(object sender, EventArgs e)
        {

        }
    }
}
