namespace PuppetMaster {
    partial class PuppetMasterGUI {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lbPeerList = new System.Windows.Forms.Label();
            this.lbConv = new System.Windows.Forms.Label();
            this.p_n = new System.Windows.Forms.TextBox();
            this.p_serverids = new System.Windows.Forms.TextBox();
            this.p_id = new System.Windows.Forms.TextBox();
            this.r_r = new System.Windows.Forms.TextBox();
            this.r_send = new System.Windows.Forms.Button();
            this.p_send = new System.Windows.Forms.Button();
            this.s_send = new System.Windows.Forms.Button();
            this.s_url = new System.Windows.Forms.TextBox();
            this.s_id = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.s_min = new System.Windows.Forms.TextBox();
            this.s_max = new System.Windows.Forms.TextBox();
            this.c_file = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.c_username = new System.Windows.Forms.TextBox();
            this.c_url = new System.Windows.Forms.TextBox();
            this.c_send = new System.Windows.Forms.Button();
            this.kill_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbPeerList
            // 
            this.lbPeerList.AutoSize = true;
            this.lbPeerList.Location = new System.Drawing.Point(176, 24);
            this.lbPeerList.Name = "lbPeerList";
            this.lbPeerList.Size = new System.Drawing.Size(64, 15);
            this.lbPeerList.TabIndex = 7;
            this.lbPeerList.Text = "2. Partition";
            // 
            // lbConv
            // 
            this.lbConv.AutoSize = true;
            this.lbConv.Location = new System.Drawing.Point(12, 24);
            this.lbConv.Name = "lbConv";
            this.lbConv.Size = new System.Drawing.Size(103, 15);
            this.lbConv.TabIndex = 10;
            this.lbConv.Text = "1. Replicate Factor";
            // 
            // p_n
            // 
            this.p_n.Location = new System.Drawing.Point(176, 42);
            this.p_n.Name = "p_n";
            this.p_n.PlaceholderText = "Nº replicas";
            this.p_n.Size = new System.Drawing.Size(64, 23);
            this.p_n.TabIndex = 11;
            // 
            // p_serverids
            // 
            this.p_serverids.Location = new System.Drawing.Point(323, 42);
            this.p_serverids.Multiline = true;
            this.p_serverids.Name = "p_serverids";
            this.p_serverids.PlaceholderText = "Server ID\'s";
            this.p_serverids.Size = new System.Drawing.Size(73, 46);
            this.p_serverids.TabIndex = 11;
            // 
            // p_id
            // 
            this.p_id.Location = new System.Drawing.Point(246, 42);
            this.p_id.Name = "p_id";
            this.p_id.PlaceholderText = "Partition ID";
            this.p_id.Size = new System.Drawing.Size(71, 23);
            this.p_id.TabIndex = 11;
            // 
            // r_r
            // 
            this.r_r.Location = new System.Drawing.Point(12, 42);
            this.r_r.Name = "r_r";
            this.r_r.PlaceholderText = "Nº replicas";
            this.r_r.Size = new System.Drawing.Size(90, 23);
            this.r_r.TabIndex = 11;
            // 
            // r_send
            // 
            this.r_send.Location = new System.Drawing.Point(108, 42);
            this.r_send.Name = "r_send";
            this.r_send.Size = new System.Drawing.Size(44, 23);
            this.r_send.TabIndex = 12;
            this.r_send.Text = "Send";
            this.r_send.UseVisualStyleBackColor = true;
            this.r_send.Click += new System.EventHandler(this.R_send_Click);
            // 
            // p_send
            // 
            this.p_send.Location = new System.Drawing.Point(402, 42);
            this.p_send.Name = "p_send";
            this.p_send.Size = new System.Drawing.Size(44, 23);
            this.p_send.TabIndex = 12;
            this.p_send.Text = "Send";
            this.p_send.UseVisualStyleBackColor = true;
            this.p_send.Click += new System.EventHandler(this.P_send_Click);
            // 
            // s_send
            // 
            this.s_send.Location = new System.Drawing.Point(352, 105);
            this.s_send.Name = "s_send";
            this.s_send.Size = new System.Drawing.Size(44, 23);
            this.s_send.TabIndex = 12;
            this.s_send.Text = "Send";
            this.s_send.UseVisualStyleBackColor = true;
            this.s_send.Click += new System.EventHandler(this.Se_send_Click);
            // 
            // s_url
            // 
            this.s_url.Location = new System.Drawing.Point(82, 105);
            this.s_url.Name = "s_url";
            this.s_url.PlaceholderText = "URL";
            this.s_url.Size = new System.Drawing.Size(158, 23);
            this.s_url.TabIndex = 11;
            // 
            // s_id
            // 
            this.s_id.Location = new System.Drawing.Point(12, 105);
            this.s_id.Name = "s_id";
            this.s_id.PlaceholderText = "Server ID";
            this.s_id.Size = new System.Drawing.Size(64, 23);
            this.s_id.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "3. Server";
            // 
            // s_min
            // 
            this.s_min.Location = new System.Drawing.Point(246, 105);
            this.s_min.Name = "s_min";
            this.s_min.PlaceholderText = "Min delay (ms)";
            this.s_min.Size = new System.Drawing.Size(47, 23);
            this.s_min.TabIndex = 11;
            this.s_min.Text = "0";
            // 
            // s_max
            // 
            this.s_max.Location = new System.Drawing.Point(299, 105);
            this.s_max.Name = "s_max";
            this.s_max.PlaceholderText = "Maz delay (ms)";
            this.s_max.Size = new System.Drawing.Size(47, 23);
            this.s_max.TabIndex = 11;
            this.s_max.Text = "0";
            // 
            // c_file
            // 
            this.c_file.Location = new System.Drawing.Point(246, 163);
            this.c_file.Name = "c_file";
            this.c_file.PlaceholderText = "Script file (local)";
            this.c_file.Size = new System.Drawing.Size(100, 23);
            this.c_file.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 145);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Client";
            // 
            // c_username
            // 
            this.c_username.Location = new System.Drawing.Point(12, 163);
            this.c_username.Name = "c_username";
            this.c_username.PlaceholderText = "Username";
            this.c_username.Size = new System.Drawing.Size(64, 23);
            this.c_username.TabIndex = 11;
            // 
            // c_url
            // 
            this.c_url.Location = new System.Drawing.Point(82, 163);
            this.c_url.Name = "c_url";
            this.c_url.PlaceholderText = "URL";
            this.c_url.Size = new System.Drawing.Size(158, 23);
            this.c_url.TabIndex = 11;
            // 
            // c_send
            // 
            this.c_send.Location = new System.Drawing.Point(352, 163);
            this.c_send.Name = "c_send";
            this.c_send.Size = new System.Drawing.Size(44, 23);
            this.c_send.TabIndex = 12;
            this.c_send.Text = "Send";
            this.c_send.UseVisualStyleBackColor = true;
            this.c_send.Click += new System.EventHandler(this.C_send_Click);
            // 
            // kill_button
            // 
            this.kill_button.Location = new System.Drawing.Point(352, 214);
            this.kill_button.Name = "kill_button";
            this.kill_button.Size = new System.Drawing.Size(94, 23);
            this.kill_button.TabIndex = 13;
            this.kill_button.Text = "Kill";
            this.kill_button.UseVisualStyleBackColor = true;
            this.kill_button.Click += new System.EventHandler(this.Kill_Click);
            // 
            // PuppetMasterGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 322);
            this.Controls.Add(this.kill_button);
            this.Controls.Add(this.c_send);
            this.Controls.Add(this.c_url);
            this.Controls.Add(this.c_username);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.c_file);
            this.Controls.Add(this.s_max);
            this.Controls.Add(this.s_min);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.s_id);
            this.Controls.Add(this.s_url);
            this.Controls.Add(this.s_send);
            this.Controls.Add(this.p_send);
            this.Controls.Add(this.r_send);
            this.Controls.Add(this.r_r);
            this.Controls.Add(this.p_id);
            this.Controls.Add(this.p_serverids);
            this.Controls.Add(this.p_n);
            this.Controls.Add(this.lbConv);
            this.Controls.Add(this.lbPeerList);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PuppetMasterGUI";
            this.Text = "Puppet Master GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.form1_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lbPeerList;
        private System.Windows.Forms.Label lbConv;
        private System.Windows.Forms.TextBox p_n;
        private System.Windows.Forms.TextBox p_serverids;
        private System.Windows.Forms.TextBox p_id;
        private System.Windows.Forms.TextBox r_r;
        private System.Windows.Forms.Button r_send;
        private System.Windows.Forms.Button p_send;
        private System.Windows.Forms.Button s_send;
        private System.Windows.Forms.TextBox s_url;
        private System.Windows.Forms.TextBox s_id;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox s_min;
        private System.Windows.Forms.TextBox s_max;
        private System.Windows.Forms.TextBox c_file;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox c_username;
        private System.Windows.Forms.TextBox c_url;
        private System.Windows.Forms.Button c_send;
        private System.Windows.Forms.Button kill_button;
    }
}

