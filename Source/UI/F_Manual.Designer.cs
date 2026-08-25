namespace FZ4P.UI
{
    partial class F_Manual
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.rb_DetectPin_Hold = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbb_Channel = new System.Windows.Forms.ComboBox();
            this.cbb_Aixs = new System.Windows.Forms.ComboBox();
            this.lbl_ADC = new System.Windows.Forms.Label();
            this.cbb_ADC_Select = new System.Windows.Forms.ComboBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.EditCondition = new System.Windows.Forms.CheckBox();
            this.lbl_ReadHall = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btn_PositionMove = new System.Windows.Forms.Button();
            this.txt_PositionCode_AxisZ = new System.Windows.Forms.TextBox();
            this.txt_PositionCode_AxisY = new System.Windows.Forms.TextBox();
            this.txt_PositionCode_AxisX = new System.Windows.Forms.TextBox();
            this.cbb_Acturator_Model = new System.Windows.Forms.ComboBox();
            this.button13 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.lbl_ReadHall2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbl_ReadHall2);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Controls.Add(this.rb_DetectPin_Hold);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cbb_Channel);
            this.groupBox1.Controls.Add(this.cbb_Aixs);
            this.groupBox1.Controls.Add(this.lbl_ADC);
            this.groupBox1.Controls.Add(this.cbb_ADC_Select);
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.EditCondition);
            this.groupBox1.Controls.Add(this.lbl_ReadHall);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(392, 199);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PeakCurrent";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(124, 79);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(55, 16);
            this.radioButton2.TabIndex = 280;
            this.radioButton2.TabStop = true;
            this.radioButton2.Tag = "2";
            this.radioButton2.Text = "Reset";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.Click += new System.EventHandler(this.rb_DetectPin_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(60, 79);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(58, 16);
            this.radioButton1.TabIndex = 279;
            this.radioButton1.TabStop = true;
            this.radioButton1.Tag = "1";
            this.radioButton1.Text = "Detect";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Click += new System.EventHandler(this.rb_DetectPin_Click);
            // 
            // rb_DetectPin_Hold
            // 
            this.rb_DetectPin_Hold.AutoSize = true;
            this.rb_DetectPin_Hold.Location = new System.Drawing.Point(6, 79);
            this.rb_DetectPin_Hold.Name = "rb_DetectPin_Hold";
            this.rb_DetectPin_Hold.Size = new System.Drawing.Size(48, 16);
            this.rb_DetectPin_Hold.TabIndex = 278;
            this.rb_DetectPin_Hold.TabStop = true;
            this.rb_DetectPin_Hold.Tag = "0";
            this.rb_DetectPin_Hold.Text = "Hold";
            this.rb_DetectPin_Hold.UseVisualStyleBackColor = true;
            this.rb_DetectPin_Hold.Click += new System.EventHandler(this.rb_DetectPin_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(286, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 12);
            this.label2.TabIndex = 277;
            this.label2.Text = "Axis";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(264, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 12);
            this.label1.TabIndex = 276;
            this.label1.Text = "Channel";
            // 
            // cbb_Channel
            // 
            this.cbb_Channel.FormattingEnabled = true;
            this.cbb_Channel.Location = new System.Drawing.Point(322, 0);
            this.cbb_Channel.Name = "cbb_Channel";
            this.cbb_Channel.Size = new System.Drawing.Size(64, 20);
            this.cbb_Channel.TabIndex = 275;
            this.cbb_Channel.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            // 
            // cbb_Aixs
            // 
            this.cbb_Aixs.FormattingEnabled = true;
            this.cbb_Aixs.Location = new System.Drawing.Point(322, 27);
            this.cbb_Aixs.Name = "cbb_Aixs";
            this.cbb_Aixs.Size = new System.Drawing.Size(64, 20);
            this.cbb_Aixs.TabIndex = 274;
            // 
            // lbl_ADC
            // 
            this.lbl_ADC.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbl_ADC.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_ADC.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_ADC.ForeColor = System.Drawing.Color.Yellow;
            this.lbl_ADC.Location = new System.Drawing.Point(241, 123);
            this.lbl_ADC.Name = "lbl_ADC";
            this.lbl_ADC.Size = new System.Drawing.Size(135, 64);
            this.lbl_ADC.TabIndex = 273;
            this.lbl_ADC.Text = "-";
            this.lbl_ADC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbb_ADC_Select
            // 
            this.cbb_ADC_Select.FormattingEnabled = true;
            this.cbb_ADC_Select.Location = new System.Drawing.Point(160, 100);
            this.cbb_ADC_Select.Name = "cbb_ADC_Select";
            this.cbb_ADC_Select.Size = new System.Drawing.Size(64, 20);
            this.cbb_ADC_Select.TabIndex = 272;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.checkBox2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox2.ForeColor = System.Drawing.Color.White;
            this.checkBox2.Location = new System.Drawing.Point(6, 101);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(148, 19);
            this.checkBox2.TabIndex = 271;
            this.checkBox2.Text = "Detect Pin Hold/Reset";
            this.checkBox2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(139, 32);
            this.button1.TabIndex = 270;
            this.button1.Text = "Config Register Setting";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // EditCondition
            // 
            this.EditCondition.AutoSize = true;
            this.EditCondition.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.EditCondition.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EditCondition.ForeColor = System.Drawing.Color.White;
            this.EditCondition.Location = new System.Drawing.Point(311, 101);
            this.EditCondition.Name = "EditCondition";
            this.EditCondition.Size = new System.Drawing.Size(58, 19);
            this.EditCondition.TabIndex = 269;
            this.EditCondition.Text = "START";
            this.EditCondition.UseVisualStyleBackColor = false;
            this.EditCondition.CheckedChanged += new System.EventHandler(this.EditCondition_CheckedChanged);
            this.EditCondition.CheckStateChanged += new System.EventHandler(this.EditCondition_CheckStateChanged);
            // 
            // lbl_ReadHall
            // 
            this.lbl_ReadHall.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbl_ReadHall.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_ReadHall.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_ReadHall.ForeColor = System.Drawing.Color.Yellow;
            this.lbl_ReadHall.Location = new System.Drawing.Point(6, 123);
            this.lbl_ReadHall.Name = "lbl_ReadHall";
            this.lbl_ReadHall.Size = new System.Drawing.Size(112, 64);
            this.lbl_ReadHall.TabIndex = 268;
            this.lbl_ReadHall.Text = "-";
            this.lbl_ReadHall.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.checkBox1);
            this.groupBox5.Controls.Add(this.btn_PositionMove);
            this.groupBox5.Controls.Add(this.txt_PositionCode_AxisZ);
            this.groupBox5.Controls.Add(this.txt_PositionCode_AxisY);
            this.groupBox5.Controls.Add(this.txt_PositionCode_AxisX);
            this.groupBox5.Controls.Add(this.cbb_Acturator_Model);
            this.groupBox5.Controls.Add(this.button13);
            this.groupBox5.Controls.Add(this.button11);
            this.groupBox5.Controls.Add(this.button9);
            this.groupBox5.Controls.Add(this.button2);
            this.groupBox5.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.groupBox5.Location = new System.Drawing.Point(12, 271);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(392, 167);
            this.groupBox5.TabIndex = 512;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Center Move";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.checkBox1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1.ForeColor = System.Drawing.Color.White;
            this.checkBox1.Location = new System.Drawing.Point(265, -2);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(104, 19);
            this.checkBox1.TabIndex = 270;
            this.checkBox1.Text = "Power On/Off";
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.CheckStateChanged += new System.EventHandler(this.checkBox1_CheckStateChanged);
            // 
            // btn_PositionMove
            // 
            this.btn_PositionMove.BackColor = System.Drawing.Color.Silver;
            this.btn_PositionMove.Location = new System.Drawing.Point(25, 130);
            this.btn_PositionMove.Name = "btn_PositionMove";
            this.btn_PositionMove.Size = new System.Drawing.Size(344, 27);
            this.btn_PositionMove.TabIndex = 8;
            this.btn_PositionMove.Text = "Move";
            this.btn_PositionMove.UseVisualStyleBackColor = false;
            this.btn_PositionMove.Click += new System.EventHandler(this.btn_PositionMove_Click);
            // 
            // txt_PositionCode_AxisZ
            // 
            this.txt_PositionCode_AxisZ.Location = new System.Drawing.Point(261, 56);
            this.txt_PositionCode_AxisZ.Name = "txt_PositionCode_AxisZ";
            this.txt_PositionCode_AxisZ.Size = new System.Drawing.Size(108, 25);
            this.txt_PositionCode_AxisZ.TabIndex = 7;
            // 
            // txt_PositionCode_AxisY
            // 
            this.txt_PositionCode_AxisY.Location = new System.Drawing.Point(147, 56);
            this.txt_PositionCode_AxisY.Name = "txt_PositionCode_AxisY";
            this.txt_PositionCode_AxisY.Size = new System.Drawing.Size(108, 25);
            this.txt_PositionCode_AxisY.TabIndex = 6;
            // 
            // txt_PositionCode_AxisX
            // 
            this.txt_PositionCode_AxisX.Location = new System.Drawing.Point(26, 56);
            this.txt_PositionCode_AxisX.Name = "txt_PositionCode_AxisX";
            this.txt_PositionCode_AxisX.Size = new System.Drawing.Size(108, 25);
            this.txt_PositionCode_AxisX.TabIndex = 5;
            // 
            // cbb_Acturator_Model
            // 
            this.cbb_Acturator_Model.FormattingEnabled = true;
            this.cbb_Acturator_Model.Location = new System.Drawing.Point(261, 23);
            this.cbb_Acturator_Model.Name = "cbb_Acturator_Model";
            this.cbb_Acturator_Model.Size = new System.Drawing.Size(108, 25);
            this.cbb_Acturator_Model.TabIndex = 4;
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(26, 18);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(229, 32);
            this.button13.TabIndex = 3;
            this.button13.Text = "Servo Set";
            this.button13.UseVisualStyleBackColor = true;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(261, 87);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(108, 37);
            this.button11.TabIndex = 2;
            this.button11.Text = "Max";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(26, 87);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(108, 37);
            this.button9.TabIndex = 1;
            this.button9.Text = "Min";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(147, 87);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 37);
            this.button2.TabIndex = 0;
            this.button2.Text = "Mid";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // lbl_ReadHall2
            // 
            this.lbl_ReadHall2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbl_ReadHall2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_ReadHall2.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_ReadHall2.ForeColor = System.Drawing.Color.Yellow;
            this.lbl_ReadHall2.Location = new System.Drawing.Point(124, 123);
            this.lbl_ReadHall2.Name = "lbl_ReadHall2";
            this.lbl_ReadHall2.Size = new System.Drawing.Size(112, 64);
            this.lbl_ReadHall2.TabIndex = 281;
            this.lbl_ReadHall2.Text = "-";
            this.lbl_ReadHall2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // F_Manual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 450);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox1);
            this.Name = "F_Manual";
            this.Text = "F_Manual";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.F_Manual_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lbl_ReadHall;
        private System.Windows.Forms.CheckBox EditCondition;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btn_PositionMove;
        private System.Windows.Forms.TextBox txt_PositionCode_AxisZ;
        private System.Windows.Forms.TextBox txt_PositionCode_AxisY;
        private System.Windows.Forms.TextBox txt_PositionCode_AxisX;
        private System.Windows.Forms.ComboBox cbb_Acturator_Model;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.ComboBox cbb_ADC_Select;
        private System.Windows.Forms.Label lbl_ADC;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbb_Channel;
        private System.Windows.Forms.ComboBox cbb_Aixs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rb_DetectPin_Hold;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Label lbl_ReadHall2;
    }
}