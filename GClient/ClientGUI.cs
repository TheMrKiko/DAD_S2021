﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GC {
    public partial class ClientGUI : Form {
        ClientLogic clientLogic;
        public ClientGUI(string username, string url, string file) {
            InitializeComponent();
            clientLogic = new ClientLogic(this, username, url, file);
        }

        private void btReg_Click(object sender, EventArgs e) {
            foreach (string nick in clientLogic.Register(tbNick.Text, tbPort.Text)) {
                tbRegResult.Text += nick + "\r\n";
            }
            tbNick.Enabled = false;
            tbPort.Enabled = false;
        }

        public void AddMsgtoGUI(string m) { tbConv.Text += m + "\r\n"; }

        private void form1_Closing(object sender, FormClosingEventArgs e) {
            clientLogic.ServerShutdown();
        }

        private async void btSend_Click(object sender, EventArgs e) {
            string m = tbMsg.Text;
            await clientLogic.BcastMsg(m);
            tbConv.Text += "me: " + tbMsg.Text + "\r\n";
            tbMsg.Text = "";
        }
    }
}
