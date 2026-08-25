using FZ4P.Commons.Helper;
using Modules.Communication.Params;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.BarcodeReader.FileSelector
{
    //경로에 있는 파일을 찾아 Type에 따른 파라미터를 넘겨준다.
    public class CommunicationTypeSelector
    {
        private readonly string BaseDirPath; 
        public CommunicationTypeSelector(string Directory)
        {
            BaseDirPath = Directory;
        }

        public CommParamBase GetFileParams()
        {
            var files = Directory.GetFiles(BaseDirPath, "*.json");
            if(files.Length > 1) throw new ArgumentException($"설정 파일이 여러 개 존재합니다.Count : {files.Length}");

            var SerrialFiles = BaseDirPath + "Serrial.json";
            var TCPClientFiles = BaseDirPath + "Socket.json";
            //파일 서치
            if (File.Exists(SerrialFiles))
                return JsonHelper.Load<SerialParams>(SerrialFiles);
            else if (File.Exists(TCPClientFiles))
                return JsonHelper.Load<SocketParams>(TCPClientFiles);
            else
                throw new ArgumentException("파일이 존재하지 않습니다.");
        }
    }
}
