using FZ4P.Commons.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Commons.Helper
{
    public static class CalculatorHelper
    {
        public static LinearModel LeastSquaresLine(IReadOnlyList<ActroPoint> points)
        {
            LinearModel resultData = new LinearModel();
            if (points == null || points.Count < 2) return resultData;

            double SUMx = 0, SUMy = 0, SUMxy = 0, SUMxx = 0;

            for (int i = 0; i < points.Count; i++)
            {
                SUMx += points[i].X;
                SUMy += points[i].Y;
                SUMxy += points[i].X * points[i].Y;
                SUMxx += points[i].X * points[i].X;
            }

            double denominator = (points.Count * SUMxx - SUMx * SUMx);

            if (Math.Abs(denominator) < 1e-10) return resultData;

            resultData.dSlope = (points.Count * SUMxy - SUMx * SUMy) / denominator;
            resultData.dYintercept = (SUMy / points.Count) - (resultData.dSlope * (SUMx / points.Count));

            return resultData;
        }

        /// <summary>
        /// X,Y 데이터중 최소 반지름 Stroke 값을 리턴한다.
        /// </summary>
        /// <param name="rateStrokes"></param>
        /// <returns></returns>
        public static double Radius(RateStrokeModel rateStrokes)
        {
            return Math.Min(rateStrokes.StrokeValueX, rateStrokes.StrokeValueY) / 2;
        }


    }
}
