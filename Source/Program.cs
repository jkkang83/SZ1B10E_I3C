using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isNew;
            Mutex mutex = new Mutex(true, "FZ_Test", out isNew);

            if (isNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var provider = Bootstrapper.Build();
                Application.DoEvents();

                // 이 시점에 하드웨어 설정을 읽어 Dln 객체를 생성합니다.
                STATIC.Initialize();

                var mainForm = provider.GetRequiredService<F_Main>();
                Application.Run(mainForm);

                mutex.ReleaseMutex();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            else
            {
                MessageBox.Show("Still Running Process .....");
                Application.Exit();
            }
        }
    }
}
