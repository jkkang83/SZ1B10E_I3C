using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.UI
{
    //싱글톤으로 살아있어야됨.

    public partial class F_SystemLogView : Form
    {
        private const int LineMaxCount = 500;
        private List<string> SystemMessageLog = new List<string>();
        public F_SystemLogView()
        {
            InitializeComponent();
            initRichBoxText();
        }

        public void TextView(string SystemMessage)
        {
            SystemMessageLog.Add(SystemMessage);
            AddRichBox(SystemMessage);
        }

        private void AddRichBox(string SystemMessage)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(
                    new Action(() =>
                    {
                        HistoryMessage.AppendText(SystemMessage);
                        int lineCount = HistoryMessage.Lines.Length;
                        if (lineCount > LineMaxCount)
                        {
                            HistoryMessage.Clear();
                        }
                    }
                ));
            }
            else
            {
                HistoryMessage.AppendText(SystemMessage);
                int lineCount = HistoryMessage.Lines.Length;
                if (lineCount > LineMaxCount)
                {
                    HistoryMessage.Clear();
                }
            }
        }

        public void initRichBoxText()
        {
            HistoryMessage.Clear();
        }

        private void F_SystemLogView_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
