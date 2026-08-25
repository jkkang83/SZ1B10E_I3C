using System;
using System.Windows.Forms;

namespace FZ4P
{
    public partial class F_Barcode : Form
    {
        public Process Process { get { return STATIC.Process; } }

        public string BarcodeData { get; private set; }

        public F_Barcode()
        {
            InitializeComponent();
            STATIC.Scanner.OnReceveData += Scanner_OnReceveData;
           
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            STATIC.Scanner.OnOffTrigger(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Scanner_OnReceveData(object sender, string reciveErrorMessage)
        {
            BarcodeData = reciveErrorMessage;
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    Barcodelbl.Text = BarcodeData;    
                    this.DialogResult = DialogResultContain(BarcodeData, "ERROR");
                    this.Close();
                });
            }
            else
            {
                Barcodelbl.Text = BarcodeData;
                this.DialogResult = DialogResultContain(BarcodeData, "ERROR");
                this.Close();
            }

            
        }

        private void F_Barcode_FormClosing(object sender, FormClosingEventArgs e)
        {
            STATIC.Scanner.OnReceveData -= Scanner_OnReceveData;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            STATIC.Scanner.OnOffTrigger(true);
        }

        private DialogResult DialogResultContain(string sourceString, string targetString)
        {
            if(!sourceString.Contains(targetString))
                return DialogResult.OK;
            else
                return DialogResult.No;
        }
    }
}
