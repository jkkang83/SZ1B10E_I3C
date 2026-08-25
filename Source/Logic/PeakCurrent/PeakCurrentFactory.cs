using FZ4P.Logic.OISPeakCurrent.Interfaces;
using FZ4P.Logic.OISPeakCurrent.Params;
using FZ4P.Logic.OISPeakCurrent.Params.Base;
using FZ4P.Logic.PeakCurrent;
using FZ4P.Logic.PeakCurrent.Configration;
using FZ4P.Logic.PeakCurrent.Configration.Base;
using FZ4P.Logic.PeakCurrent.Params;
using System;

namespace FZ4P.Logic.OISPeakCurrent
{
    public enum PeakCurrentAxis
    {
        None = 0,
        OIS_X = 1,
        OIS_Y = 2,
        AF = 3,
    }
    public class PeakCurrentFactory
    {
        public PeakCurrentFactory()
        {
                
        }

        public IPeakCurrentExecutor CreatePeakCurrentLogic(PeakCurrentParamBase _param, PeakCurrentConfigBase config)
        {
            var _config = GetPeakCurrentConfig<PeakCurrentConfigration>(config);

            switch (_param)
            {
                case AFPeakCurrentParam _:
                    return new AFPeakCurrentLogic(_config.AFFunction, _config.OIS_Function, _config.Dln, _config.LogView, _config.GPIOOutput);

                case OIS_X_PeakCurrentParam _:
                    return new OIS_X_PeakCurrentLogic(_config.AFFunction, _config.OIS_Function, _config.LogView, _config.GPIOOutput);

                case OIS_Y_PeakCurrentParam _:
                    return new OIS_Y_PeakCurrentLogic(_config.AFFunction, _config.OIS_Function, _config.LogView, _config.GPIOOutput);

                default:
                    throw new NotSupportedException(
                        $"지원하지 않는 파라미터 타입: {_param?.GetType().Name ?? "null"}");
            }
        }

        public PeakCurrentParamBase CreatePeakCurrentParams(string itemName)
        {
            PeakCurrentAxis targetType;

            targetType = GetPeakCurrentAxis(itemName);

            switch (targetType)
            {
                case PeakCurrentAxis.OIS_X :
                    return new OIS_X_PeakCurrentParam() { SelectADCNumber = 1 };
                case PeakCurrentAxis.OIS_Y :
                    return new OIS_Y_PeakCurrentParam() { SelectADCNumber = 1 };
                case PeakCurrentAxis.AF :
                    return new AFPeakCurrentParam() 
                    { 
                        SelectADCNumber = 1, 
                    };
                default:
                    throw new NotSupportedException(
                        $"지원하지 않는 파라미터 타입: {itemName ?? "null"}");
            }
        }

        private PeakCurrentAxis GetPeakCurrentAxis(string itemName)
        {
            if (itemName.Contains("OIS_X"))
                return PeakCurrentAxis.OIS_X;

            if (itemName.Contains("OIS_Y"))
                return PeakCurrentAxis.OIS_Y;

            if (itemName.Contains("AF"))
                return PeakCurrentAxis.AF;

            return PeakCurrentAxis.None;
        }
        private TReturnType GetPeakCurrentConfig<TReturnType>(PeakCurrentConfigBase item) where TReturnType : class
        {
            if (item is TReturnType config)
            {
                return config;
            }
            else
            {
                throw new NotSupportedException(
                        $"지원하지 않는 파라미터 타입: {item?.GetType().Name ?? "null"}");
            }
        }
    }
}
