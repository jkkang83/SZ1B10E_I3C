using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Enums
{
    //단일 비트용
    public enum ADS1013_LSB
    {
        SPS128 = 0x00,
        SPS250 = 0x20,
        SPS490 = 0x40,
        SPS920 = 0x50,
        SPS1600 = 0x80,
        SPS2400 = 0xA0,
        SPS3300 = 0xC0,
    };

    //비트 조합용
    public enum ADS1013_MSB
    {
        OS_START = 0x80,
        MODE_SingleShot = 0x01,
    };
    public enum POINTEREGISTER
    {
        CONFIGREGISTER = 0x01,
    };
}
