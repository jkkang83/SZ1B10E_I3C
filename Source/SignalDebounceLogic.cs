using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FZ4P
{
    public class SignalDebounceLogic : IDisposable
    {
        private bool disposedValue;
        private CancellationTokenSource _debounceCts;
        private int _debounceMs = 100;
        private TargetSignalType _targetSignalType;
        private bool _signalState = false;
        private bool _debounceState = false;

        public EventHandler<DebounceEventArgs> SignalChanged;

        public bool DebounceState => _debounceState;

        #region Fluent Setting
        public SignalDebounceLogic()
        {
        }
        public SignalDebounceLogic SetDebounceMs(int ms)
        {
            _debounceMs = ms;
            return this;
        }
        public SignalDebounceLogic SetTagetSignalType(TargetSignalType type)
        {
            _targetSignalType = type;
            return this;
        }
        public SignalDebounceLogic SetInitState(bool initializeSignalState)
        {
            _signalState = initializeSignalState;
            return this;
        }
        #endregion

        #region public Method
        public void StartSignal()
        {
            StartDebounce();
        }
        public void StopSignal()
        {
            CancelDebounce();
        }
        #endregion

        #region private Method
        private async void StartDebounce()
        {
            CancelDebounce();

            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            try
            {
                _debounceState = true;
                await Task.Delay(_debounceMs, token);

                if (!token.IsCancellationRequested)
                {
                    _debounceState = false;
                    _signalState = !_signalState;
                    SignalChanged?.Invoke(this, new DebounceEventArgs(true, _targetSignalType, _signalState));
                }
                else 
                {
                    _debounceState = false;
                }
            }
            catch
            {
                _debounceState = false;
            }
        }
        private void CancelDebounce()
        {
            if (_debounceCts == null)
                return;

            _debounceCts.Cancel();
            _debounceCts.Dispose();
            _debounceCts = null;
        }
        #endregion 

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리형 상태(관리형 개체)를 삭제합니다.
                    CancelDebounce();
                }

                // TODO: 비관리형 리소스(비관리형 개체)를 해제하고 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.
                disposedValue = true;
            }
        }

        // // TODO: 비관리형 리소스를 해제하는 코드가 'Dispose(bool disposing)'에 포함된 경우에만 종료자를 재정의합니다.
        // ~SignalDebounceLogic()
        // {
        //     // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
