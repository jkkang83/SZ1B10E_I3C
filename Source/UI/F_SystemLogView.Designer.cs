namespace FZ4P.UI
{
    partial class F_SystemLogView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel2 = new System.Windows.Forms.Panel();
            this.HistoryMessage = new System.Windows.Forms.RichTextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lbl_LogViewTitle = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.HistoryMessage);
            this.panel2.Location = new System.Drawing.Point(0, 62);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(800, 407);
            this.panel2.TabIndex = 2;
            // 
            // HistoryMessage
            // 
            this.HistoryMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HistoryMessage.Location = new System.Drawing.Point(0, 0);
            this.HistoryMessage.Name = "HistoryMessage";
            this.HistoryMessage.Size = new System.Drawing.Size(800, 407);
            this.HistoryMessage.TabIndex = 1;
            this.HistoryMessage.Text = "";
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 407);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(800, 62);
            this.panel3.TabIndex = 3;
            // 
            // lbl_LogViewTitle
            // 
            this.lbl_LogViewTitle.AutoSize = true;
            this.lbl_LogViewTitle.Font = new System.Drawing.Font("굴림", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbl_LogViewTitle.Location = new System.Drawing.Point(12, 9);
            this.lbl_LogViewTitle.Name = "lbl_LogViewTitle";
            this.lbl_LogViewTitle.Size = new System.Drawing.Size(232, 48);
            this.lbl_LogViewTitle.TabIndex = 0;
            this.lbl_LogViewTitle.Text = "Log View";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbl_LogViewTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 62);
            this.panel1.TabIndex = 1;
            // 
            // F_SystemLogView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 469);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "F_SystemLogView";
            this.Text = "F_SystemLogView";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.F_SystemLogView_FormClosing);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox HistoryMessage;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lbl_LogViewTitle;
        private System.Windows.Forms.Panel panel1;
    }
}