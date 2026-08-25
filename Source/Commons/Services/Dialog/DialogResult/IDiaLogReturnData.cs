using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Services.DialogResult
{
    /// <summary>
    /// 리턴 데이터를 View화면에게 책임을 넘긴다.
    /// </summary>
    public interface IDiaLogReturnData 
    {
        void DiaLogResultFileName(string fileFullName, int index = 0);
    }
}
