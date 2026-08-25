using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.UI.CustomUI
{
    public partial class ucBarcodePannel : UserControl
    {
        public ucBarcodePannel()
        {
            InitializeComponent();
        }
        public void SetMessage(string message)
        {
            lbl_BarcodeInfor.Text = message;
        }
    }
}
