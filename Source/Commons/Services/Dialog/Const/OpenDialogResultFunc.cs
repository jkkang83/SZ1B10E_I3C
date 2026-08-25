using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons.Services
{
    public class OpenDialogResultFunc : IDialogResultFunc
    {
        private readonly F_Main _view;
        public OpenDialogResultFunc(F_Main view)
        {
            _view = view;
        }
        public void Execute(OpenFileDialog resultDialog,int index)
        {
            string sFileName = resultDialog.FileName;
            //_view.mUpdateDriverFWfile[index] = sFileName;
            //if(index == 0)
            //    _view.OISFWFileText = sFileName;
            //else
            //    _view.AFFWFileText = sFileName;
        }
    }
}
