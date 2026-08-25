using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    public class ActroPannel : Panel
    {
        private Label CenterLabel;
        public ActroPannel()
        {
            CenterLabel = new Label();
            CenterLabel.Visible = true;
            CenterLabel.AutoSize = true;
            CenterLabel.Font = new System.Drawing.Font("맑은 고딕", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            CenterLabel.SizeChanged += CenterLabel_SizeChanged;
            this.Controls.Add(CenterLabel); 
        }

        private void CenterLabel_SizeChanged(object sender, EventArgs e)
        {
            Size size = CenterLabel.Size;

            int mergeX = (this.Width - size.Width) / 2;
            int mergeY = (this.Height - size.Height) / 2;

            CenterLabel.Location = new Point(mergeX, mergeY);
        }

        public void Text(string strText, Color foreColor = default)
        {
            CenterLabel.Text = strText;
            if (foreColor != default)
            {
                CenterLabel.ForeColor = foreColor;
            }
        }
    }
}
