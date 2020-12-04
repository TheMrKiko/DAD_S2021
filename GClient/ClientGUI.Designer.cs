namespace GC {
    partial class ClientGUI {
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
            this.pid = new System.Windows.Forms.TextBox();
            this.objid = new System.Windows.Forms.TextBox();
            this.logs = new System.Windows.Forms.TextBox();
            this.btwrite = new System.Windows.Forms.Button();
            this.newvalue = new System.Windows.Forms.TextBox();
            this.lbConv = new System.Windows.Forms.Label();
            this.readbt = new System.Windows.Forms.Button();
            this.serverid = new System.Windows.Forms.TextBox();
            this.listbt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pid
            // 
            this.pid.Location = new System.Drawing.Point(89, 20);
            this.pid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pid.Name = "pid";
            this.pid.PlaceholderText = "Partition ID";
            this.pid.Size = new System.Drawing.Size(87, 23);
            this.pid.TabIndex = 1;
            // 
            // objid
            // 
            this.objid.Location = new System.Drawing.Point(182, 20);
            this.objid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.objid.Name = "objid";
            this.objid.PlaceholderText = "Object ID";
            this.objid.Size = new System.Drawing.Size(87, 23);
            this.objid.TabIndex = 2;
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(10, 101);
            this.logs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.logs.Multiline = true;
            this.logs.Name = "logs";
            this.logs.ReadOnly = true;
            this.logs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logs.Size = new System.Drawing.Size(436, 210);
            this.logs.TabIndex = 4;
            // 
            // btwrite
            // 
            this.btwrite.Location = new System.Drawing.Point(10, 53);
            this.btwrite.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btwrite.Name = "btwrite";
            this.btwrite.Size = new System.Drawing.Size(74, 24);
            this.btwrite.TabIndex = 5;
            this.btwrite.Text = "Write";
            this.btwrite.UseVisualStyleBackColor = true;
            this.btwrite.Click += new System.EventHandler(this.BtWrite_Click);
            // 
            // newvalue
            // 
            this.newvalue.Location = new System.Drawing.Point(89, 54);
            this.newvalue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.newvalue.Name = "newvalue";
            this.newvalue.PlaceholderText = "Value";
            this.newvalue.Size = new System.Drawing.Size(357, 23);
            this.newvalue.TabIndex = 6;
            // 
            // lbConv
            // 
            this.lbConv.AutoSize = true;
            this.lbConv.Location = new System.Drawing.Point(10, 84);
            this.lbConv.Name = "lbConv";
            this.lbConv.Size = new System.Drawing.Size(32, 15);
            this.lbConv.TabIndex = 10;
            this.lbConv.Text = "Logs";
            // 
            // readbt
            // 
            this.readbt.Location = new System.Drawing.Point(10, 19);
            this.readbt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.readbt.Name = "readbt";
            this.readbt.Size = new System.Drawing.Size(74, 24);
            this.readbt.TabIndex = 5;
            this.readbt.Text = "Read";
            this.readbt.UseVisualStyleBackColor = true;
            this.readbt.Click += new System.EventHandler(this.BtRead_Click);
            // 
            // serverid
            // 
            this.serverid.Location = new System.Drawing.Point(291, 20);
            this.serverid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.serverid.Name = "serverid";
            this.serverid.PlaceholderText = "Server ID (op)";
            this.serverid.Size = new System.Drawing.Size(79, 23);
            this.serverid.TabIndex = 2;
            // 
            // listbt
            // 
            this.listbt.Location = new System.Drawing.Point(376, 19);
            this.listbt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listbt.Name = "listbt";
            this.listbt.Size = new System.Drawing.Size(70, 24);
            this.listbt.TabIndex = 11;
            this.listbt.Text = "List";
            this.listbt.UseVisualStyleBackColor = true;
            this.listbt.Click += new System.EventHandler(this.Listbt_Click);
            // 
            // ClientGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 351);
            this.Controls.Add(this.listbt);
            this.Controls.Add(this.serverid);
            this.Controls.Add(this.readbt);
            this.Controls.Add(this.lbConv);
            this.Controls.Add(this.newvalue);
            this.Controls.Add(this.btwrite);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.objid);
            this.Controls.Add(this.pid);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ClientGUI";
            this.Text = "ClientGUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox pid;
        private System.Windows.Forms.TextBox objid;
        private System.Windows.Forms.TextBox logs;
        private System.Windows.Forms.Button btwrite;
        private System.Windows.Forms.TextBox newvalue;
        private System.Windows.Forms.Label lbConv;
        private System.Windows.Forms.Button readbt;
        private System.Windows.Forms.TextBox serverid;
        private System.Windows.Forms.Button listbt;
    }
}

