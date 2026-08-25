using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Commons.Helper
{
    public static class AppHelper
    {
        public static bool IsDebuggerMode()
        {
            bool ret;
            //IDE 소스수정모드이면 edit 할수 있게 
            if (System.Diagnostics.Debugger.IsAttached == true)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }
            return ret;
        }

        public static void InvokeOnUIThread(this Control control, Action action)
        {
            if (control.IsHandleCreated && !control.IsDisposed)
            {
                try
                {
                    control.BeginInvoke(action);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"BeginInvoke 실패: {ex.Message}");
                }
            }
        }
    }
}
