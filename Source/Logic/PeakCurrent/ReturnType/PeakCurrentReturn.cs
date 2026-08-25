using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Logic.PeakCurrent.ReturnType
{
    public class PeakCurrentReturn
    {
        public bool IsSeccess { get; set; } = false;        //메서드 성공 여부
        public double Min  { get; set; } = 0.0;             //컬렉션 최대 값
        public double Max { get; set; } = 0.0;              //컬렉션 최소 값
        public double Average { get; set; } = 0.0;          //컬렉션 평균 값
        public List<double> RestulCollection { get; set; } = new List<double>() ;   //게더링 데이터 값
    }
}
