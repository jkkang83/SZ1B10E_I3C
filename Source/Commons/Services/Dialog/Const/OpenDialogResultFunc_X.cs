using FZ4P.Commons.Services.DialogResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons.Services.Dialog.Const
{
    public class OpenDialogResultFunc_X : IDialogResultFunc
    {
        private readonly IDiaLogReturnData _view;

        public OpenDialogResultFunc_X(IDiaLogReturnData view)
        {
            _view = view;
        }
        public void Execute(OpenFileDialog resultDialog, int index = -1)
        {
            string sFileName = resultDialog.FileName;
            _view.DiaLogResultFileName(sFileName, 1);
        }
    }
}
