namespace FZ4P.UI.CustomUI
{
    partial class ucBarcodePannel
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnl_bot = new System.Windows.Forms.Panel();
            this.pnl_Content = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_BarcodeInfor = new System.Windows.Forms.Label();
            this.pnl_Content.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gray;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(673, 30);
            this.panel1.TabIndex = 0;
            // 
            // pnl_bot
            // 
            this.pnl_bot.BackColor = System.Drawing.Color.Gray;
            this.pnl_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnl_bot.Location = new System.Drawing.Point(0, 190);
            this.pnl_bot.Name = "pnl_bot";
            this.pnl_bot.Size = new System.Drawing.Size(673, 30);
            this.pnl_bot.TabIndex = 1;
            // 
            // pnl_Content
            // 
            this.pnl_Content.BackColor = System.Drawing.Color.White;
            this.pnl_Content.Controls.Add(this.lbl_BarcodeInfor);
            this.pnl_Content.Controls.Add(this.label1);
            this.pnl_Content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl_Content.Location = new System.Drawing.Point(0, 30);
            this.pnl_Content.Name = "pnl_Content";
            this.pnl_Content.Size = new System.Drawing.Size(673, 160);
            this.pnl_Content.TabIndex = 2;
            this.pnl_Content.Tag = "";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("굴림", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(89, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(495, 48);
            this.label1.TabIndex = 0;
            this.label1.Text = "바코드를 읽어주세요.";
            // 
            // lbl_BarcodeInfor
            // 
            this.lbl_BarcodeInfor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_BarcodeInfor.AutoSize = true;
            this.lbl_BarcodeInfor.Font = new System.Drawing.Font("굴림", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbl_BarcodeInfor.Location = new System.Drawing.Point(311, 90);
            this.lbl_BarcodeInfor.Name = "lbl_BarcodeInfor";
            this.lbl_BarcodeInfor.Size = new System.Drawing.Size(51, 48);
            this.lbl_BarcodeInfor.TabIndex = 1;
            this.lbl_BarcodeInfor.Text = "-";
            // 
            // ucBarcodePannel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnl_Content);
            this.Controls.Add(this.pnl_bot);
            this.Controls.Add(this.panel1);
            this.Name = "ucBarcodePannel";
            this.Size = new System.Drawing.Size(673, 220);
            this.pnl_Content.ResumeLayout(false);
            this.pnl_Content.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel pnl_bot;
        private System.Windows.Forms.Panel pnl_Content;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_BarcodeInfor;
    }
}
