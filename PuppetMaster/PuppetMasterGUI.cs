using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public partial class PuppetMasterGUI : Form
    {
        readonly PuppetMasterLogic puppetLogic;
        public PuppetMasterGUI(string filename, string host)
        {
            InitializeComponent();

            puppetLogic = new PuppetMasterLogic(this, host, 10001);
            if (filename != null)
                Task.Run(() => puppetLogic.ExecuteCommands(filename));
        }

        public void AddMsgtoGUI(ConfigSteps currentConfig)
        {
            switch (currentConfig)
            {
                case ConfigSteps.ReplicateFactor:
                    break;
                case ConfigSteps.Partition:
                    r_r.Enabled = false;
                    r_send.Enabled = false;
                    break;
                case ConfigSteps.Server:
                    p_n.Enabled = false;
                    p_id.Enabled = false;
                    p_send.Enabled = false;
                    p_serverids.Enabled = false;
                    break;
                case ConfigSteps.Client:
                    s_id.Enabled = false;
                    s_min.Enabled = false;
                    s_max.Enabled = false;
                    s_url.Enabled = false;
                    s_send.Enabled = false;
                    break;
                case ConfigSteps.Commands:
                    c_url.Enabled = false;
                    c_file.Enabled = false;
                    c_send.Enabled = false;
                    c_username.Enabled = false;
                    break;
                default:
                    break;
            }
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            puppetLogic.ServerShutdown();
        }

        private void R_send_Click(object sender, EventArgs e)
        {
            int r = int.Parse(r_r.Text);
            puppetLogic.ReplicationFactor(r);
        }

        private void P_send_Click(object sender, EventArgs e)
        {
            int n = int.Parse(p_n.Text);
            List<string> lines = p_serverids.Lines.ToList();

            puppetLogic.Partitions(n, p_id.Text, lines);
        }

        private void Se_send_Click(object sender, EventArgs e)
        {
            int i = int.Parse(s_min.Text);
            int a = int.Parse(s_max.Text);
            puppetLogic.Server(s_id.Text, s_url.Text, i, a);
        }

        private void C_send_Click(object sender, EventArgs e)
        {
            puppetLogic.Client(c_username.Text, c_url.Text, c_file.Text);
        }

        private void Kill_Click(object sender, EventArgs e)
        {
            puppetLogic.Kill();
        }

        private void Freeze_Click(object sender, EventArgs e)
        {
            puppetLogic.Freeze(ct_id.Text);
        }

        private void Crash_Click(object sender, EventArgs e)
        {
            puppetLogic.Crash(ct_id.Text);
        }

        private void Unfreeze_Click(object sender, EventArgs e)
        {
            puppetLogic.Unfreeze(ct_id.Text);
        }

        private void Status_Click(object sender, EventArgs e)
        {
            puppetLogic.Status();
        }
    }
}
