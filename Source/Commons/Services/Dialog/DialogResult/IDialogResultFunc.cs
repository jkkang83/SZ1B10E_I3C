using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons.Services
{
    public interface IDialogResultFunc
    {
        void Execute(OpenFileDialog resultDialog, int index = -1);
    }
}
