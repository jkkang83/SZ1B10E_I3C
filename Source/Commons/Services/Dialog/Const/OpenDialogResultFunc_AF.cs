using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Activation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons.Services.DialogResult
{
    public class OpenDialogResultFunc_AF : IDialogResultFunc
    {
        private readonly IDiaLogReturnData _view;
        
        public OpenDialogResultFunc_AF(IDiaLogReturnData view)
        {
            _view = view;
        }
        public void Execute(OpenFileDialog resultDialog, int index = -1)
        {
            string sFileName = resultDialog.FileName;
            _view.DiaLogResultFileName(sFileName, 0);
        }
    }
}
