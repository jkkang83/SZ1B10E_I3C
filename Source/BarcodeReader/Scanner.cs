using FZ4P.BarcodeReader.CommandLine;
using FZ4P.Commons.Events;
using Modules.Communication.Context;
using Modules.Communication.Intefaces;
using Modules.Communication.Params;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.BarcodeReader
{
    public class Scanner
    {
        private ComunicationContext comContext;             //실제 통신 컨트롤(바코드 리더기 역할)
        private CommParamBase _params;                      //통신에 필요한 파라미터 설정
        private readonly IScanCommandLine _commandLine;

        public event EventHandler<string> OnScannerMessage;
        public event EventHandler<string> receivedEvent;
        public event EventHandler<string> OnReceveData;

        #region Constructrue
        public Scanner(CommParamBase commParam, IScanCommandLine commandLine)
        {
            _params = commParam;
            Lazyinitialize();
        }
        public Scanner(IScanCommandLine commandLine)
        {
            _commandLine = commandLine;
        }
        #endregion 

        private void Lazyinitialize()
        {
            comContext = new ComunicationContext();
            comContext.Configure(_params);
        }
        private void OnRecive(object sender, string data)
        {
            data = data.Replace("\r", "");
            OnReceveData?.Invoke(this,data);
        }

        #region public 메서드 
        public void SetParam(CommParamBase param)
        {
            _params = param;
            Lazyinitialize();
        }
        public void Connection(EventHandler<string> receivedHandler)
        {
            if (_params is null) throw new ArgumentException("Parameter가 정의되어 있지 않습니다. 확인 바랍니다.");

            if (!comContext.IsConnection)
            {
                comContext.Connection();
                
                if (receivedEvent != null)
                    comContext.RemoveReceivedEvent(receivedEvent);
                
                receivedEvent = receivedHandler;
                if (comContext.IsConnection)
                {
                    comContext.AddReceivedEvent(receivedEvent);
                    comContext.AddReceivedEvent(OnRecive);
                    OnScannerMessage?.Invoke(this, $"연결 성공.");
                }
                else 
                {
                    OnScannerMessage?.Invoke(this, $"연결 시도를 하였지만 실패하였습니다.");
                }
            }
            else
            {
                OnScannerMessage?.Invoke(this, $"연결 상태를 확인 바랍니다. Connection State : {comContext.IsConnection}");
            }
        }
        public void Connection()
        {
            if (_params is null) throw new ArgumentException("Parameter가 정의되어 있지 않습니다. 확인 바랍니다.");

            if (!comContext.IsConnection)
            {
                comContext.Connection();
                if (comContext.IsConnection)
                {
                    comContext.AddReceivedEvent(OnRecive);
                    OnScannerMessage?.Invoke(this, $"연결 성공.");
                }
                else
                {
                    OnScannerMessage?.Invoke(this, $"연결 시도를 하였지만 실패하였습니다.");
                }
            }
            else
            {
                OnScannerMessage?.Invoke(this, $"연결 상태를 확인 바랍니다. Connection State : {comContext.IsConnection}");
            }
        }
        public void DisConnection()
        {   
            var disConnectionState = comContext.DisConnection();
            if (disConnectionState)
            {
                if (receivedEvent != null)
                    comContext.RemoveReceivedEvent(receivedEvent);
                
                OnScannerMessage?.Invoke(this, $"연결 해제 성공.");
            }
            else
            {
                OnScannerMessage?.Invoke(this, $"연결 시도를 하였지만 실패하였습니다.");
            }
        }

        public void OnOffTrigger(bool bOnOff)
        {
            string Command = string.Empty;

            if (bOnOff)
                Command = _commandLine.TrigerCommandToString();
            else
                Command = _commandLine.TrigerStopCommandToString();

            comContext.Sender(Command);
        }
        #endregion public 메서드 
    }
}
