using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FZ4P.Commons.Services.Context
{
    public class OpenDialogContext
    {
        private readonly OpenFileDialog _dialog = new OpenFileDialog();
        private OpenDialogParams _param;

        public bool Open(OpenDialogParams param)
        {
            bool result = false;
            if (param != null)
            {
                _dialog.InitialDirectory = param.InitialDirectory;
                _dialog.Filter = param.Filter;
                _dialog.Title = param.Title;

                if (_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    param.resultFunction.Execute(_dialog);
                    result = true;
                }
            }

            return result;
        }
    }
}
