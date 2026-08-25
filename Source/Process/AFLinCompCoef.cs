using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P
{
    public class AFLinCompCoef
    {
        public const int PRE_ERROR = 5000;  // initial error vaule	
        public const int ERROR_TH = 0;  // threshold error vaule	
        public const int VT = 1;
        public const int OptParamX = 0; // 0: Value, 1: Slope
        public const int OptXFix = 31;  // OptXCandidate num
        public const int OptXCan = 32;  // OptXCandidate num
        public const int OptALL = 1;    // OptParamALL ON/OFF
        public const int OptALLNum = 2; // optimize y param
        public const int OptALLTimes = 200;     // optimize y param times
        public const int RANGE = 4; // optimize y param range

        public const int MAX_NUM_DATA = 65; // maximum number of data after Interpolation
        public const int MAX_NUM_DATA2 = 65;    // maximum number of data after Averaging	
        public const int MIN_NUM_DATA = 6;  // minimum number of data
        public const int NUM_COEF = 27; // size of output data array
        public const int NUM_COEF2_1 = 35;  // size of output data array
        public const int NUM_COEF2_2 = 38;  // size of output data array

        public const int BIT_COEF2 = 7;
        public const int BIT_COEF2G = 2;

        public const int ERROR_PREDATA = 1; // PreData() error
        public const int ERROR_CONVDATA = 2;    // ConvData() error
        public const int ERROR_CALCOEF2 = 3;    // CalLinCompCoef2() error
        public const int ERROR_CONVREG = 4; // ConvReg() error


        public float[] sarrX13 = new float[MAX_NUM_DATA];
        public float[] sarrIDEAL = new float[MAX_NUM_DATA];
        public float[] sarrINL0 = new float[MAX_NUM_DATA];
        public float[] sarrINL2 = new float[MAX_NUM_DATA];

        public float[] snorX = new float[MAX_NUM_DATA];
        public float[] sarrY13 = new float[MAX_NUM_DATA];
        public float[] snorY = new float[MAX_NUM_DATA];
        public float[] sarrIdeal = new float[MAX_NUM_DATA];
        public float[] sarrInl0 = new float[MAX_NUM_DATA];
        public float[] sarrInl2 = new float[MAX_NUM_DATA];
        public float[] sarrLin2 = new float[MAX_NUM_DATA];

        public int[] sarrRegCoef2 = new int[NUM_COEF2_1 + NUM_COEF2_2];

        public float[] sSlope = new float[MAX_NUM_DATA];
        public float[] sIntercept = new float[MAX_NUM_DATA];

        public float[] snorXLNC = new float[MAX_NUM_DATA];
        public float[] snorYLNC = new float[MAX_NUM_DATA];
        public float[] sarrX13LNC = new float[MAX_NUM_DATA];
        public float[] sarrY13LNC = new float[MAX_NUM_DATA];

        public float[] sarrNorCoef2 = new float[NUM_COEF2_1 + NUM_COEF2_2];
        public float[] sarrCoef2 = new float[NUM_COEF2_1 + NUM_COEF2_2];

        public int LinCompMain(float[] targPosi, float[] lensPosi, int numData, int pVt, int nVt, int ignInf, int ignMac, int[] linCoef, ref float resError)
        {
            int i;
            // Initialize output data
            for (i = 0; i < NUM_COEF; i++) { linCoef[i] = 0; }
            resError = PRE_ERROR;

            //Data preparation
            ringBufLinComp lineData = new ringBufLinComp();                            // Ring buffer for input data processing

            for (i = 0; i < numData; i++)
            {
                lineData.dataX[i + 1] = targPosi[i];
                lineData.dataY[i + 1] = lensPosi[i];
            }

            lineData.startIndex = 1;
            lineData.endIndex = numData;
            lineData.size = numData;

            float spPos = 0.0f;
            float snPos = 0.0f;
            if (PreData(ref lineData, pVt, nVt, ignInf, ignMac, ref spPos, ref snPos) != 0) { return ERROR_PREDATA; }

            // Normalization of data
            float smaxX = 0;
            float sminX = 0;
            float smaxY = 0;
            float sminY = 0;
            smaxY = lineData.dataY[numData];
            sminY = lineData.dataY[1];

            //int MAX_NUM_DATA	= MAX_NUM_DATA;
            int posibit = 10;
            if (ConvData(lineData, ref posibit, spPos, snPos, sarrX13, sarrY13, snorX, snorY, sarrIDEAL, sarrIdeal, ref smaxX, ref sminX, ref smaxY, ref sminY) != 0) { return ERROR_CONVDATA; }

            // INL calculation before LNC at Normalized value
            CalINL0(sarrX13, sarrY13, snorX, snorY, sarrIDEAL, sarrIdeal, sarrINL0, sarrInl0);

            // Calculation of line compensation coefficients2
            for (i = 0; i < NUM_COEF2_1 + NUM_COEF2_2; i++) { sarrRegCoef2[i] = 0; }
            if (CalLinCompCoef2(snorX, sarrInl0, snorY, sarrX13, sarrY13, spPos, snPos, smaxX, sminX, smaxY, sminY, sarrIDEAL, sarrIdeal, sarrRegCoef2, sarrINL2, sarrLin2, sarrInl2) != 0) { return ERROR_CALCOEF2; }

            // INL calculation after LNC2
            CalINL2(sarrX13, sarrY13, snorX, snorY, sarrIDEAL, sarrIdeal, sarrINL2, sarrLin2, sarrInl2, sarrRegCoef2, spPos, snPos, smaxX, sminX, smaxY, sminY, 1);

            // Residual INL2 Error
            resError = 0;
            for (i = 0; i < MAX_NUM_DATA; i++)
            {
                if (AKM_Abs(sarrINL2[i]) > resError) { resError = AKM_Abs(sarrINL2[i]); }
            }

            // Register Address
            if (ConvReg(sarrRegCoef2, linCoef) != 0) { return ERROR_CONVREG; }

            /*
            //debug----------
            if (posibit == 14){
                for(int i = 0; i < MAX_NUM_DATA; i++)			{sarrX13[i] = sarrX13[i] * 2.0f;}
            }else{
                for(int i = 0; i < MAX_NUM_DATA; i++)			{sarrX13[i] = sarrX13[i] / AKM_Pow(2.0f, 13 - posibit);}
            }

            printf("\npVt, nVt, ignInf, ignMac, posibit, spPos, snPos, &smaxX, &sminX, &smaxY, &sminY\n");
            printf("%d, %d, %d, %d, %d, %f, %f, %f, %f, %f, %f\n ", pVt, nVt, ignInf, ignMac, posibit, spPos, snPos, smaxX, sminX, smaxY, sminY);

            printf("\ntargPosi, lensPosi\n");
            for (int i = 0; i < numData; i++)					{printf("%f, %f\n ", lineData.dataX[i+1],lineData.dataY[i+1]);}

            printf("\nsarrX13[i], sarrINL0[i], sarrINL2[i], snorY[i], sarrInl0[i], sarrInl2[i], sarrLin2[i]\n");
            for(int i = 0; i < MAX_NUM_DATA; i++)				{printf("%f, %f, %f, %f, %f, %f, %f\n", sarrX13[i], sarrINL0[i], sarrINL2[i], snorY[i], sarrInl0[i], sarrInl2[i], sarrLin2[i]);}

            printf("\nsarrY13[i], snorX[i], sarrIDEAL[i], sarrIdeal[i]\n");
            for(int i = 0; i < MAX_NUM_DATA; i++)				{printf("%f, %f, %f, %f\n", sarrY13[i], snorX[i], sarrIDEAL[i], sarrIdeal[i]);}

            printf("\nCOEF2\n");
            printf("0, ");
            for (int i = 0; i < NUM_COEF2_1; i++)	{printf("%d,", sarrRegCoef2[i]);}
            printf("127\n ");
            for (int i = 0; i < NUM_COEF2_2; i++)	{printf("%d,", sarrRegCoef2[i+NUM_COEF2_1]);}
            //-----------------
            */

            return 0;
        }
        int PreData(ref ringBufLinComp lineData, int pVt, int nVt, int ignInf, int ignMac, ref float pPos, ref float nPos)
        {
            int i;
            //--------------------------------------------------
            //	Check of input data
            //--------------------------------------------------

            // Output error if number of valid data run short after delete the data within invalid range
            if (lineData.size < ignInf + ignMac + MIN_NUM_DATA) { return 1; }

            // Output error if target is not rising to the right
            for (i = lineData.startIndex; i <= lineData.endIndex - 1; i++)
            {
                if (lineData.dataX[i] >= lineData.dataX[i + 1]) { return 1; }
            }

            //--------------------------------------------------
            //	Exclude the data within invalid range from processing range
            //--------------------------------------------------
            lineData.startIndex += ignInf;
            lineData.size -= (ignInf + ignMac);
            lineData.endIndex = lineData.startIndex + lineData.size - 1;

            //------------------------------------------------------
            //	Calculation of target before calibration based on correction value by VT setting
            //------------------------------------------------------

            // Correction value by POSVT
            if (pVt > 511) { pPos = ((pVt - 1024) / 1024.0f + 0.5f); }
            else { pPos = (pVt / 1024.0f + 0.5f); }

            if (nVt > 511) { nPos = ((nVt - 1024) / 1024.0f - 0.5f); }
            else { nPos = (nVt / 1024.0f - 0.5f); }

            if (pPos == 0 && nPos == 0) { return 1; }

            return 0;
        }
        int ConvData(ringBufLinComp lineData, ref int posibit, float pPos, float nPos, float[] arrX13, float[] arrY13, float[] norX, float[] norY, float[] arrIDEAL, float[] arrIdeal, ref float maxX, ref float minX, ref float maxY, ref float minY)
        {
            float bitconv;
            int i;

            // Resolution of Position
            if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 10)) { posibit = 10; bitconv = AKM_Pow(2.0f, 13 - posibit); }
            else if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 11)) { posibit = 11; bitconv = AKM_Pow(2.0f, 13 - posibit); }
            else if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 12)) { posibit = 12; bitconv = AKM_Pow(2.0f, 13 - posibit); }
            else if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 13)) { posibit = 13; bitconv = AKM_Pow(2.0f, 13 - posibit); }
            else if (lineData.dataX[lineData.endIndex] < AKM_Pow(2.0f, 14)) { posibit = 14; bitconv = 0.5f; }
            else { return 1; }

            //--------------------------------------------------------------------------------------
            //	Data interpolation
            //--------------------------------------------------------------------------------------

            float Lensmax = maxY;
            float Lensmin = minY;
            float[] dataX = new float[MAX_NUM_DATA];
            float[] dataY = new float[MAX_NUM_DATA];

            // Related Lens value
            for (i = lineData.startIndex; i < lineData.endIndex + 1; i++)
            {
                lineData.dataY[i] -= Lensmin;
            }

            int k = 0;
            maxX = lineData.dataX[lineData.endIndex] * bitconv;
            minX = lineData.dataX[lineData.startIndex] * bitconv;
            maxY = lineData.dataY[lineData.startIndex];
            minY = lineData.dataY[lineData.startIndex];
            for (i = lineData.startIndex; i < lineData.endIndex + 1; i++)
            {
                dataX[i - lineData.startIndex] = lineData.dataX[i] * bitconv;
                dataY[i - lineData.startIndex] = lineData.dataY[i];
                if (maxY < lineData.dataY[i]) { maxY = lineData.dataY[i]; }
                if (minY > lineData.dataY[i]) { minY = lineData.dataY[i]; }
            }

            if (maxX == 0 && minX == 0) { return 1; }
            if (maxY == 0 && minY == 0) { return 1; }

            if (VT == 0)
            {
                for (i = 0; i < lineData.size - 1; i++)
                {
                    sSlope[i] = (lineData.dataY[lineData.startIndex + i + 1] - lineData.dataY[lineData.startIndex + i]) / (lineData.dataX[lineData.startIndex + i + 1] - lineData.dataX[lineData.startIndex + i]) / bitconv;
                    sIntercept[i] = lineData.dataY[lineData.startIndex + i] - sSlope[i] * lineData.dataX[lineData.startIndex + i] * bitconv;
                }
                sSlope[lineData.size - 1] = sSlope[lineData.size - 2];
                sIntercept[lineData.size - 1] = sIntercept[lineData.size - 2];

                for (i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrX13[i] = (maxX - minX) / (MAX_NUM_DATA - 1) * i + minX;
                    if (arrX13[i] != 0 && arrX13[i] > lineData.dataX[lineData.startIndex + k + 1] * bitconv)
                    {
                        if (k < lineData.size - 1)
                        {
                            k = k + 1;
                        }
                    }
                    arrY13[i] = sSlope[k] * arrX13[i] + sIntercept[k];
                }
            }

            if (VT == 1)
            {
                for (i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrY13[i] = (maxY - minY) / (MAX_NUM_DATA - 1) * i + minY;
                }
                CalApprox(dataY, dataX, arrY13, arrX13, lineData.size, MAX_NUM_DATA);
            }

            //--------------------------------------------------------------------------------------
            //	Normalization
            //--------------------------------------------------------------------------------------

            maxY = Lensmax;
            minY = Lensmin;

            for (i = 0; i < MAX_NUM_DATA; i++)
            {
                norX[i] = nPos + arrX13[i] * (pPos - nPos) / AKM_Pow(2.0f, 13);
                norY[i] = nPos + arrY13[i] * (pPos - nPos) / (maxY - minY);
            }


            //--------------------------------------------------------------------------------------
            //	Ideal Data
            //--------------------------------------------------------------------------------------

            float SlopeIDEAL, InterceptIDEAL;
            float SlopeIdeal, InterceptIdeal;

            if (VT == 0)
            {
                SlopeIDEAL = (lineData.dataY[lineData.endIndex] - lineData.dataY[lineData.startIndex]) / (maxX - minX);
                InterceptIDEAL = lineData.dataY[lineData.startIndex] - SlopeIDEAL * minX;

                SlopeIdeal = (norY[MAX_NUM_DATA - 1] - norY[0]) / (norX[MAX_NUM_DATA - 1] - norX[0]);
                InterceptIdeal = norY[0] - SlopeIdeal * norX[0];

                for (i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrIDEAL[i] = SlopeIDEAL * arrX13[i] + InterceptIDEAL;
                    arrIdeal[i] = SlopeIdeal * norX[i] + InterceptIdeal;
                }
            }

            if (VT == 1)
            {
                SlopeIDEAL = (lineData.dataY[lineData.endIndex] - lineData.dataY[lineData.startIndex]) / (maxX - minX);
                InterceptIDEAL = lineData.dataY[lineData.startIndex] - SlopeIDEAL * minX;

                SlopeIdeal = (norX[MAX_NUM_DATA - 1] - norX[0]) / (norY[MAX_NUM_DATA - 1] - norY[0]);
                InterceptIdeal = norX[0] - SlopeIdeal * norY[0];

                for (i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrIDEAL[i] = SlopeIDEAL * arrX13[i] + InterceptIDEAL;
                    arrIdeal[i] = SlopeIdeal * norY[i] + InterceptIdeal;
                }
            }

            return 0;
        }
        int ConvReg(int[] arrRegCoef2, int[] linCoef)
        {
            int[] arrRegCoefX = new int[4];
            int[] arrRegCoefY = new int[NUM_COEF2_2];
            int[] XCoef = new int[4];
            int i;
            //arrRegX
            int k = 0;
            for (i = 0; i < NUM_COEF2_1; i++)
            {
                if ((arrRegCoef2[i] & 2) == 2)
                {
                    arrRegCoefX[k] = arrRegCoef2[i];
                    XCoef[k] = i + 1;
                    if (k == 3) { break; }
                    k += 1;
                }
            }
            if (k != 3) { return 1; }

            //arrRegY
            for (i = 0; i < NUM_COEF2_2 - 5; i++)
            {
                if (i < XCoef[0])
                {
                    arrRegCoefY[i] = arrRegCoef2[NUM_COEF2_1 + i];
                }
                else if (i < XCoef[1] - 1)
                {
                    arrRegCoefY[i] = arrRegCoef2[NUM_COEF2_1 + i + 1];
                }
                else if (i < XCoef[2] - 2)
                {
                    arrRegCoefY[i] = arrRegCoef2[NUM_COEF2_1 + i + 2];
                }
                else if (i < XCoef[3] - 3)
                {
                    arrRegCoefY[i] = arrRegCoef2[NUM_COEF2_1 + i + 3];
                }
                else
                {
                    arrRegCoefY[i] = arrRegCoef2[NUM_COEF2_1 + i + 4];
                }
            }
            for (i = 0; i < 4; i++)
            {
                arrRegCoefY[NUM_COEF2_2 - 5 + i] = arrRegCoef2[NUM_COEF2_1 + XCoef[i]];
            }

            arrRegCoefY[37] = arrRegCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1];

            //Register 
            for (i = 0; i < 5; i++)
            {
                linCoef[i * 5 + 0] = (arrRegCoefY[i * 8 + 1] & 7) * 32 + (arrRegCoefY[i * 8 + 0]);
                linCoef[i * 5 + 1] = (arrRegCoefY[i * 8 + 3] & 1) * 128 + (arrRegCoefY[i * 8 + 2]) * 4 + (arrRegCoefY[i * 8 + 1] >> 3);
                linCoef[i * 5 + 2] = (arrRegCoefY[i * 8 + 4] & 15) * 16 + (arrRegCoefY[i * 8 + 3] >> 1);
                if (i < 4)
                {
                    linCoef[i * 5 + 3] = (arrRegCoefY[i * 8 + 6] & 3) * 64 + (arrRegCoefY[i * 8 + 5]) * 2 + (arrRegCoefY[i * 8 + 4] >> 4);
                    linCoef[i * 5 + 4] = (arrRegCoefY[i * 8 + 7]) * 8 + (arrRegCoefY[i * 8 + 6] >> 2);
                }
            }
            linCoef[23] = (arrRegCoefX[0]) * 2 + (arrRegCoefY[NUM_COEF2_2 - 2] >> 4);
            linCoef[24] = (arrRegCoefY[37] & 1) * 128 + arrRegCoefX[1];
            linCoef[25] = (arrRegCoefY[37] >> 1) * 128 + arrRegCoefX[2];
            linCoef[26] = arrRegCoefX[3];

            //Error check
            for (i = 0; i < 27; i++)
            {
                if (linCoef[i] < 0) { return 1; }
                if (linCoef[i] > 255) { return 1; }
            }

            return 0;
        }
        int CalApprox(float[] inputX, float[] inputY, float[] outputX, float[] outputY, int numData1, int numData2)
        {
            int i, k;
            for (i = 0; i < numData2; i++)
            {
                if (outputX[i] < inputX[0])
                {
                    k = 0;
                    float slope = (inputY[k + 1] - inputY[k]) / (inputX[k + 1] - inputX[k]);
                    float offset = inputY[k] - slope * inputX[k];

                    outputY[i] = slope * outputX[i] + offset;
                }
                else
                {
                    for (k = 0; k < numData1 - 1; k++)
                    {
                        if (inputX[k + 1] >= outputX[i] && inputX[k] <= outputX[i])
                        {

                            // Output error if there is dead zone
                            if (inputX[k + 1] == inputX[k]) { return 1; }

                            float slope = (inputY[k + 1] - inputY[k]) / (inputX[k + 1] - inputX[k]);
                            float offset = inputY[k] - slope * inputX[k];

                            outputY[i] = slope * outputX[i] + offset;
                            break;
                        }
                        else if (outputX[i] > inputX[numData1 - 1])
                        {
                            k = numData1 - 2;
                            float slope = (inputY[k + 1] - inputY[k]) / (inputX[k + 1] - inputX[k]);
                            float offset = inputY[k] - slope * inputX[k];

                            outputY[i] = slope * outputX[i] + offset;
                        }
                    }
                }
            }

            return 0;
        }
        int CalINL0(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float[] arrIDEAL, float[] arrIdeal, float[] arrINL0, float[] arrInl0)
        {
            int i;
            // -------------------------------------
            // Normalized Data 
            // -------------------------------------

            // Calculation of the difference value between the ideal value and the mesuared value when the target position to achieve a certain lens position
            for (i = 0; i < MAX_NUM_DATA; i++)
            {
                if (VT == 0) { arrInl0[i] = arrIdeal[i] - norY[i]; }
                if (VT == 1) { arrInl0[i] = norX[i] - arrIdeal[i]; }
            }

            // -------------------------------------
            // Target, Lens Position Data 
            // -------------------------------------

            // Calculation of the difference value between the ideal value and the mesuared value when the target position to achieve a certain lens position
            for (i = 0; i < MAX_NUM_DATA; i++)
            {
                if (VT == 0) { arrINL0[i] = arrIDEAL[i] - arrY13[i]; }
                if (VT == 1) { arrINL0[i] = arrIDEAL[i] - arrY13[i]; }
            }

            return 0;
        }
        int CalINL2(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float[] arrIDEAL, float[] arrIdeal, float[] arrINL2, float[] arrLin2, float[] arrInl2, int[] arrRegCoef2, float pPos, float nPos, float maxX, float minX, float maxY, float minY, int INL)
        {
            float[] arrRegAbsCoef2 = new float[NUM_COEF2_2];
            int i;
            // Normalized arrCoef2_1
            for (i = 0; i < NUM_COEF2_1; i++)
            {
                sarrNorCoef2[i] = arrRegCoef2[i] / AKM_Pow(2.0f, 7) * (pPos - nPos) + nPos;
            }

            // Signed arrCoef2_2 : YB1
            for (i = 0; i < NUM_COEF2_2 - 1; i++)
            {
                if (arrRegCoef2[NUM_COEF2_1 + i] > AKM_Pow(2.0f, 4) - 1) { arrRegAbsCoef2[i] = (arrRegCoef2[NUM_COEF2_1 + i] - 32) / AKM_Pow(2.0f, 4); }
                else { arrRegAbsCoef2[i] = arrRegCoef2[NUM_COEF2_1 + i] / AKM_Pow(2.0f, 4); }
            }

            // Incremental to Absolute arrCoef2_2: YSUM
            sarrNorCoef2[NUM_COEF2_1] = arrRegAbsCoef2[0];
            for (i = 1; i < NUM_COEF2_2 - 1; i++)
            {
                sarrNorCoef2[NUM_COEF2_1 + i] = sarrNorCoef2[NUM_COEF2_1 + i - 1] + arrRegAbsCoef2[i];
            }

            sarrNorCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1] = AKM_Pow(2.0f, arrRegCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1]) * 0.5f;

            // Calclation of the compensation amount2 
            if (VT == 0) { CalLinGain2(norX, arrRegAbsCoef2, sarrNorCoef2, pPos, nPos, arrLin2); }
            if (VT == 1) { CalLinGain2(norY, arrRegAbsCoef2, sarrNorCoef2, pPos, nPos, arrLin2); }

            for (i = 0; i < MAX_NUM_DATA; i++)
            {
                snorXLNC[i] = norX[i] - arrLin2[i];
                sarrX13LNC[i] = (snorXLNC[i] - nPos) * AKM_Pow(2.0f, 13) / (pPos - nPos);
            }

            // -------------------------------------
            // Normalized Data 
            // -------------------------------------

            // Calculation of the INL after LNC2 at Normalized Target data
            if (VT == 0)
            {
                CalApprox(snorXLNC, norY, norX, snorYLNC, MAX_NUM_DATA, MAX_NUM_DATA);
                for (i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrInl2[i] = arrIdeal[i] - snorYLNC[i];
                }
            }

            if (VT == 1)
            {
                for (i = 0; i < MAX_NUM_DATA; i++)
                {
                    arrInl2[i] = snorXLNC[i] - arrIdeal[i];
                }
            }

            // -------------------------------------
            // Target, Lens Position Data 
            // -------------------------------------

            // Caliculation of Lens data after LNC2
            if (INL == 1)
            {
                if (VT == 0)
                {
                    for (i = 0; i < MAX_NUM_DATA; i++)
                    {
                        arrINL2[i] = arrIDEAL[i] - (arrY13[i] + arrLin2[i] / (pPos - nPos) * (maxY - minY));
                    }
                }

                if (VT == 1)
                {
                    CalApprox(sarrX13LNC, arrY13, arrX13, sarrY13LNC, MAX_NUM_DATA, MAX_NUM_DATA);
                    for (i = 0; i < MAX_NUM_DATA; i++)
                    {
                        arrINL2[i] = arrIDEAL[i] - sarrY13LNC[i];
                    }
                }

            }

            return 0;
        }
        int CalLinGain2(float[] inputX, float[] YB1, float[] arrCoef2, float pPos, float nPos, float[] arrLin2)
        {
            int k = 0;
            int i;
            for (i = 0; i < MAX_NUM_DATA; i++)
            {

                switch (k)
                {
                    case 0:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1] + YB1[1] / (arrCoef2[0] - nPos) * (inputX[i] - nPos);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 1:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 2:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 3:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 4:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 5:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 6:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 7:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 8:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 9:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 10:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 11:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 12:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 13:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 14:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 15:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 16:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 17:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 18:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 19:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 20:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 21:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 22:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 23:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 24:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 25:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 26:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 27:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 28:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 29:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 30:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 31:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 32:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        break;

                    case 33:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 1])
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else if (inputX[i] < arrCoef2[k + 2])
                        {
                            k += 2;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 3] + YB1[36] / (pPos - arrCoef2[NUM_COEF2_1 - 1]) * (inputX[i] - arrCoef2[NUM_COEF2_1 - 1]);
                        }
                        break;

                    case 34:
                        if (inputX[i] < arrCoef2[k])
                        {
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + k] + YB1[k + 1] / (arrCoef2[k] - arrCoef2[k - 1]) * (inputX[i] - arrCoef2[k - 1]);
                        }
                        else
                        {
                            k += 1;
                            arrLin2[i] = arrCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 3] + YB1[36] / (pPos - arrCoef2[NUM_COEF2_1 - 1]) * (inputX[i] - arrCoef2[NUM_COEF2_1 - 1]);
                        }
                        break;

                    default:
                        arrLin2[i] = arrCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 3] + YB1[36] / (pPos - arrCoef2[NUM_COEF2_1 - 1]) * (inputX[i] - arrCoef2[NUM_COEF2_1 - 1]);
                        break;
                }

                arrLin2[i] *= arrCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1] / AKM_Pow(2.0f, 6);
            }

            return 0;
        }
        int CalLinCompCoef2(float[] norX, float[] arrINL0, float[] norY, float[] arrX13, float[] arrY13, float pPos, float nPos, float maxX, float minX, float maxY, float minY, float[] arrIDEAL, float[] arrIdeal, int[] arrRegCoef2, float[] arrINL2, float[] arrLin2, float[] arrInl2)
        {
            int i, j, m;
            int rank;
            // Candidate
            int NUM_INT = (MAX_NUM_DATA - 1) / (OptXFix + 1);
            int NUM_INT2 = 256 / NUM_INT;
            float[] arrExtremX = new float[NUM_COEF2_1];
            int[] ExtremXRank = new int[NUM_COEF2_1];
            int OptXNum = NUM_COEF2_1 - OptXFix;

            // Slope
            float[] SlopeINL = new float[MAX_NUM_DATA];
            float[] SlopeVal = new float[MAX_NUM_DATA];
            float[] SlopeValCoef = new float[OptXCan];
            int[] XCoef = new int[OptXCan];
            int[] SlopeValRank = new int[OptXCan];

            float[] arrNorCoef2X = new float[NUM_COEF2_2];
            float[] arrNorCoef2Y = new float[NUM_COEF2_2];

            //Initialization
            for (i = 0; i < NUM_COEF2_1; i++)
            {
                arrExtremX[i] = 0;
            }
            for (i = 0; i < OptXCan; i++)
            {
                SlopeValRank[i] = 100;
            }

            // modulate X param: Fix point
            for (i = 0; i < OptXFix; i++)
            {
                arrExtremX[i] = (float)(NUM_INT * (i + 1));
            }
            //------------------

            if (OptParamX == 1)
            {
                //Slope judge ------------------------
                // Calculation of Slope
                for (i = 0; i < MAX_NUM_DATA - 1; i++)
                {
                    SlopeINL[i] = (arrINL0[i + 1] - arrINL0[i]) / (arrX13[i + 1] - arrX13[i]);
                }

                // Calculation of Slope Valiation
                for (i = 0; i < MAX_NUM_DATA - 2; i++)
                {
                    SlopeVal[i] = AKM_Abs(SlopeINL[i + 1] - SlopeINL[i]);
                }

                // Slope Vaulation Ranking
                for (i = 0; i < OptXCan; i++)
                {
                    XCoef[i] = NUM_INT * i + 2;
                    SlopeValCoef[i] = SlopeVal[XCoef[i]];
                }

                // Avoiding equal ranking
                for (i = 0; i < OptXCan; i++)
                {
                    SlopeValCoef[i] += i * 0.00000001f;
                }

                Rank(OptXCan, SlopeValCoef, 1, SlopeValRank);

                // Sort SlopeValRank
                for (rank = 0; rank < OptXNum; rank++)
                {
                    for (i = 0; i < OptXCan; i++)
                    {
                        if (SlopeValRank[i] == rank)
                        {
                            arrExtremX[rank + NUM_COEF2_1 - OptXNum] = (float)XCoef[i];
                        }
                    }
                }
                //---------------------------------

            }
            else
            {

                //Error variation between Xfix
                int[] sint = new int[OptXFix + 2];
                sint[0] = 0;
                sint[OptXFix + 1] = MAX_NUM_DATA - 1;

                for (i = 0; i < OptXFix; i++)
                {
                    sint[i + 1] = (int)arrExtremX[i];
                }

                for (i = 0; i < OptXFix + 1; i++)
                {
                    SlopeVal[i] = AKM_Abs((arrINL0[sint[i + 1]] - arrINL0[sint[i]]));
                }

                // Avoiding equal ranking
                for (i = 0; i < OptXCan; i++)
                {
                    SlopeVal[i] += i * 0.00000001f;
                }

                Rank(OptXFix + 1, SlopeVal, 1, SlopeValRank);

                // Sort SlopeValRank
                for (rank = 0; rank < OptXNum; rank++)
                {
                    for (i = 0; i < OptXFix + 1; i++)
                    {
                        if (SlopeValRank[i] == rank)
                        {
                            arrExtremX[rank + NUM_COEF2_1 - OptXNum] = sint[i] + NUM_INT * 0.5f;
                        }
                    }
                }
            }

            // Sort Extreme X Value
            int s = 0;
            Rank(NUM_COEF2_1, arrExtremX, 0, ExtremXRank);
            for (rank = 0; rank < NUM_COEF2_1; rank++)
            {
                for (i = 0; i < NUM_COEF2_1; i++)
                {
                    if (ExtremXRank[i] == rank)
                    {
                        s = (int)arrExtremX[i];
                        sarrCoef2[rank] = (float)(s * NUM_INT2);
                    }
                }
            }

            //Xparam bit -> nor
            arrNorCoef2X[0] = nPos;
            arrNorCoef2X[NUM_COEF2_1 + 1] = pPos;

            for (i = 0; i < NUM_COEF2_1; i++)
            {
                arrNorCoef2X[i + 1] = nPos + sarrCoef2[i] / AKM_Pow(2.0f, 13) * (pPos - nPos);
            }

            if (VT == 0) { CalApprox(norX, arrINL0, arrNorCoef2X, arrNorCoef2Y, MAX_NUM_DATA, NUM_COEF2_1 + 2); }
            if (VT == 1) { CalApprox(norY, arrINL0, arrNorCoef2X, arrNorCoef2Y, MAX_NUM_DATA, NUM_COEF2_1 + 2); }

            //Yparam nor -> 10bit code 
            for (i = 0; i < NUM_COEF2_2; i++)
            {
                sarrCoef2[i + NUM_COEF2_1] = arrNorCoef2Y[i] / (pPos - nPos) * AKM_Pow(2.0f, 10);
            }

            int[] arrRegCoef2Sub = new int[NUM_COEF2_2];
            if (CheckParam2(sarrCoef2, arrRegCoef2, arrRegCoef2Sub) != 0) { return 1; }

            // Optimize Y parameter
            if (OptALL == 1)
            {
                float[] ResError = new float[OptALLTimes + 10];
                float ResError0 = 0;
                int[] min = new int[OptALLNum + 1];
                int[] arrRegCoef2Opt = new int[NUM_COEF2_2];
                int Gain = 0;
                ResError[0] = 0.0f;

                CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, 0, 0, 0, 0, 0);

                for (i = 1; i < OptALLTimes + 1; i++)
                {
                    OptParamSeg(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, min, arrRegCoef2, pPos, nPos);
                    CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, 0, 0, 0, 0, 0);

                    // Check residual error after OptParamAll
                    ResError[i] = 0.0f;
                    for (m = 0; m < MAX_NUM_DATA; m++)
                    {
                        if (AKM_Abs(arrInl2[m]) > ResError[i]) { ResError[i] = AKM_Abs(arrInl2[m]); }
                    }
                    if (ResError[i] < ERROR_TH / (maxY - minY) * (pPos - nPos))
                    {
                        break;
                    }

                    if (ResError[i] == ResError[i - 1])
                    {
                        int maxReg = 0;
                        int minReg = 0;
                        for (j = 0; j < OptALLNum + 1; j++)
                        {
                            if (min[j] > maxReg) { maxReg = min[j]; }
                            if (min[j] < minReg) { minReg = min[j]; }
                        }

                        //Gain Adjustment
                        if (Gain == 0 && (maxReg > 11 || minReg < -12) && arrRegCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1] < 3)
                        {
                            ResError0 = ResError[i];
                            Gain = 1;
                            for (j = 0; j < NUM_COEF2_2; j++)
                            {
                                arrRegCoef2Opt[j] = arrRegCoef2[NUM_COEF2_1 + j];
                                arrRegCoef2[NUM_COEF2_1 + j] = arrRegCoef2Sub[j];
                            }
                        }
                        else
                        {
                            if (Gain == 1)
                            {
                                if (ResError[i] > ResError0)
                                {
                                    for (j = 0; j < NUM_COEF2_2; j++)
                                    {
                                        arrRegCoef2[NUM_COEF2_1 + j] = arrRegCoef2Opt[j];
                                    }
                                }
                            }
                            break;
                        }
                    }

                    if (i == OptALLTimes && Gain == 1)
                    {
                        if (ResError[i] > ResError0)
                        {
                            for (j = 0; j < NUM_COEF2_2; j++)
                            {
                                arrRegCoef2[NUM_COEF2_1 + j] = arrRegCoef2Opt[j];
                            }
                        }
                    }
                }

            }

            return 0;
        }
        void Rank(int num, float[] dbData, int direction, int[] dbDataRank)
        {
            int i, j;
            for (i = 0; i < num; i++)
            {
                dbDataRank[i] = 0;
            }

            if (direction == 0)
            {
                for (i = 1; i < num; i++)
                {
                    for (j = 0; j < i; j++)
                    {
                        if (dbData[j] > dbData[i]) { dbDataRank[j]++; }
                        if (dbData[j] < dbData[i]) { dbDataRank[i]++; }
                    }
                }
            }
            if (direction == 1)
            {
                for (i = 1; i < num; i++)
                {
                    for (j = 0; j < i; j++)
                    {
                        if (dbData[j] < dbData[i]) { dbDataRank[j]++; }
                        if (dbData[j] > dbData[i]) { dbDataRank[i]++; }
                    }
                }
            }

        }
        int OptParamSeg(float[] arrX13, float[] arrY13, float[] norX, float[] norY, float[] arrIDEAL, float[] arrIdeal, float[] arrINL2, float[] arrLin2, float[] arrInl2, int[] min, int[] arrRegCoef2, float pPos, float nPos)
        {
            float resError = PRE_ERROR;
            float resIntError = PRE_ERROR;
            float ResErrorIni = 0.0f;
            float ResError = 0.0f;
            float ResIntError = 0.0f;
            int ResErrorSeg = 0;
            float[] ResINLSegabs = new float[NUM_COEF2_1 + 1];
            float[] arrNorCoef2X = new float[NUM_COEF2_2];
            int[] arrRegIniCoef2 = new int[NUM_COEF2_2];
            int[] arrRegSigCoef2 = new int[NUM_COEF2_2];
            int[] OptX = new int[OptALLNum];
            int[] yIni = new int[OptALLNum + 1];
            int[] Int = new int[OptALLNum];
            int i, k;
            // resualINL each Segment
            for (i = 0; i < NUM_COEF2_1 + 1; i++) { ResINLSegabs[i] = 0.0f; }

            for (i = 0; i < NUM_COEF2_1; i++)
            {
                arrNorCoef2X[i] = nPos + arrRegCoef2[i] / AKM_Pow(2.0f, 7) * (pPos - nPos);
            }

            if (VT == 0)
            {
                for (i = 0; i < MAX_NUM_DATA; i++)
                {
                    if (norX[i] < arrNorCoef2X[0])
                    {
                        k = 0;
                        if (AKM_Abs(arrInl2[i]) > ResINLSegabs[k]) { ResINLSegabs[k] = AKM_Abs(arrInl2[i]); }
                    }
                    else
                    {
                        for (k = 1; k < NUM_COEF2_1; k++)
                        {
                            if (norX[i] == arrNorCoef2X[k - 1] || (norX[i] > arrNorCoef2X[k - 1] && norX[i] < arrNorCoef2X[k]))
                            {
                                if (AKM_Abs(arrInl2[i]) > ResINLSegabs[k]) { ResINLSegabs[k] = AKM_Abs(arrInl2[i]); }
                                break;
                            }
                            else if (norX[i] >= arrNorCoef2X[NUM_COEF2_1 - 1])
                            {
                                k = NUM_COEF2_1;
                                if (AKM_Abs(arrInl2[i]) > ResINLSegabs[k]) { ResINLSegabs[k] = AKM_Abs(arrInl2[i]); }
                            }
                        }
                    }
                }
            }

            if (VT == 1)
            {
                for (i = 0; i < MAX_NUM_DATA; i++)
                {
                    if (norY[i] < arrNorCoef2X[0])
                    {
                        k = 0;
                        if (AKM_Abs(arrInl2[i]) > ResINLSegabs[k]) { ResINLSegabs[k] = AKM_Abs(arrInl2[i]); }
                    }
                    else
                    {
                        for (k = 1; k < NUM_COEF2_1; k++)
                        {
                            if (norY[i] == arrNorCoef2X[k - 1] || (norY[i] > arrNorCoef2X[k - 1] && norY[i] < arrNorCoef2X[k]))
                            {
                                if (AKM_Abs(arrInl2[i]) > ResINLSegabs[k]) { ResINLSegabs[k] = AKM_Abs(arrInl2[i]); }
                                break;
                            }
                            else if (norY[i] >= arrNorCoef2X[NUM_COEF2_1 - 1])
                            {
                                k = NUM_COEF2_1;
                                if (AKM_Abs(arrInl2[i]) > ResINLSegabs[k]) { ResINLSegabs[k] = AKM_Abs(arrInl2[i]); }
                            }
                        }
                    }
                }
            }

            //Max, Min INL Segment
            for (i = 0; i < NUM_COEF2_1 + 1; i++)
            {
                if (ResINLSegabs[i] > ResErrorIni)
                {
                    ResErrorIni = ResINLSegabs[i];
                    ResErrorSeg = i;
                }
            }

            //Xpoint of adjust Yparam 
            OptX[0] = ResErrorSeg;
            OptX[1] = ResErrorSeg + 1;

            // Signed arrCoef2_2
            for (i = 0; i < NUM_COEF2_2 - 1; i++)
            {
                arrRegIniCoef2[i] = arrRegCoef2[NUM_COEF2_1 + i];
                if (arrRegCoef2[NUM_COEF2_1 + i] > AKM_Pow(2.0f, 4) - 1) { arrRegSigCoef2[i] = (arrRegCoef2[NUM_COEF2_1 + i] - 32); }
                else { arrRegSigCoef2[i] = arrRegCoef2[NUM_COEF2_1 + i]; }
            }

            yIni[0] = (int)(arrRegSigCoef2[OptX[0]] - RANGE * 0.5f);
            if (yIni[0] < -16) { yIni[0] = -16; }
            yIni[1] = arrRegSigCoef2[OptX[1]];

            if (ResErrorSeg < NUM_COEF2_1)
            {
                yIni[OptALLNum] = arrRegSigCoef2[OptX[1] + 1] + RANGE / 2;
                if (yIni[OptALLNum] > 15) { yIni[OptALLNum] = 15; }
            }

            //Optimized Parameter Search
            for (Int[0] = 0; Int[0] < RANGE; Int[0]++)
            {
                arrRegSigCoef2[OptX[0]] = yIni[0] + Int[0];
                if (arrRegSigCoef2[OptX[0]] > 15) { arrRegSigCoef2[OptX[0]] = 15; }

                for (Int[1] = 0; Int[1] < RANGE; Int[1]++)
                {
                    arrRegSigCoef2[OptX[1]] = yIni[1] + Int[1] - Int[0];
                    if (arrRegSigCoef2[OptX[1]] > 15) { arrRegSigCoef2[OptX[1]] = 15; }
                    if (arrRegSigCoef2[OptX[1]] < -16) { arrRegSigCoef2[OptX[1]] = -16; }

                    if (ResErrorSeg < NUM_COEF2_1 - 1)
                    {
                        arrRegSigCoef2[OptX[1] + 1] = yIni[OptALLNum] - Int[1];
                        if (arrRegSigCoef2[OptX[1] + 1] < -16) { arrRegSigCoef2[OptX[1] + 1] = -16; }
                    }

                    //Y param Unsiged
                    for (i = 0; i < NUM_COEF2_2 - 1; i++)
                    {
                        if (arrRegSigCoef2[i] < 0) { arrRegCoef2[NUM_COEF2_1 + i] = arrRegSigCoef2[i] + 32; }
                        else { arrRegCoef2[NUM_COEF2_1 + i] = arrRegSigCoef2[i]; }
                    }

                    CalINL2(arrX13, arrY13, norX, norY, arrIDEAL, arrIdeal, arrINL2, arrLin2, arrInl2, arrRegCoef2, pPos, nPos, 0, 0, 0, 0, 0);

                    ResError = 0.0f;
                    ResIntError = 0.0f;

                    //Error check for All Segmet
                    for (i = 0; i < MAX_NUM_DATA; i++)
                    {
                        ResIntError += AKM_Abs(arrInl2[i]);
                        if (AKM_Abs(arrInl2[i]) > ResError) { ResError = AKM_Abs(arrInl2[i]); }
                    }

                    if (ResError < resError)
                    {
                        resError = ResError;
                        resIntError = ResIntError;

                        for (i = 0; i < OptALLNum; i++)
                        {
                            min[i] = arrRegSigCoef2[OptX[i]];

                            if (ResErrorSeg < NUM_COEF2_1 - 1)
                            {
                                min[OptALLNum] = arrRegSigCoef2[OptX[1] + 1];
                            }
                        }
                    }

                    if (ResError == resError)
                    {
                        if (ResIntError < resIntError)
                        {
                            resIntError = ResIntError;

                            for (i = 0; i < OptALLNum; i++)
                            {
                                min[i] = arrRegSigCoef2[OptX[i]];

                                if (ResErrorSeg < NUM_COEF2_1 - 1)
                                {
                                    min[OptALLNum] = arrRegSigCoef2[OptX[1] + 1];
                                }
                            }
                        }
                    }
                }
            }

            if (resError >= ResErrorIni)
            {
                for (i = 0; i < NUM_COEF2_2 - 1; i++)
                {
                    arrRegCoef2[NUM_COEF2_1 + i] = arrRegIniCoef2[i];
                }
            }
            else
            {
                arrRegSigCoef2[OptX[0]] = min[0];
                arrRegSigCoef2[OptX[1]] = min[1];
                if (arrRegSigCoef2[OptX[0]] > 15) { arrRegSigCoef2[OptX[0]] = 15; }
                if (arrRegSigCoef2[OptX[1]] > 15) { arrRegSigCoef2[OptX[1]] = 15; }
                if (arrRegSigCoef2[OptX[1]] < -16) { arrRegSigCoef2[OptX[1]] = -16; }

                if (ResErrorSeg < NUM_COEF2_1 - 1)
                {
                    arrRegSigCoef2[OptX[1] + 1] = min[OptALLNum];
                    if (arrRegSigCoef2[OptX[1] + 1] < -16) { arrRegSigCoef2[OptX[1] + 1] = -16; }
                }

                //Y param Unsiged
                for (i = 0; i < NUM_COEF2_2 - 1; i++)
                {
                    if (arrRegSigCoef2[i] < 0) { arrRegCoef2[NUM_COEF2_1 + i] = arrRegSigCoef2[i] + 32; }
                    else { arrRegCoef2[NUM_COEF2_1 + i] = arrRegSigCoef2[i]; }
                }

            }

            return 0;
        }
        int CheckParam2(float[] arrCoef2, int[] arrRegCoef2, int[] arrRegCoef2Sub)
        {
            int i, j;
            int maxReg;
            int minReg;
            int[] arrRegAbsCoef2 = new int[NUM_COEF2_2];

            // Adjustment approximation coefficients including LinGainP
            for (i = 0; i < AKM_Pow(2.0f, BIT_COEF2G); i++)
            {
                arrCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1] = 0.5f * AKM_Pow(2.0f, i);
                arrRegCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1] = i;
                maxReg = 0;
                minReg = 0;

                for (j = 0; j < NUM_COEF2_2 - 1; j++)
                {
                    arrRegAbsCoef2[j] = (int)(arrCoef2[NUM_COEF2_1 + j] / arrCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1]);
                }

                //Incremental
                arrRegCoef2[NUM_COEF2_1] = arrRegAbsCoef2[0];
                for (j = 1; j < NUM_COEF2_2 - 1; j++)
                {
                    arrRegCoef2[NUM_COEF2_1 + j] = arrRegAbsCoef2[j] - arrRegAbsCoef2[j - 1];
                    if (arrRegCoef2[NUM_COEF2_1 + j] > maxReg) { maxReg = arrRegCoef2[NUM_COEF2_1 + j]; }
                    if (arrRegCoef2[NUM_COEF2_1 + j] < minReg) { minReg = arrRegCoef2[NUM_COEF2_1 + j]; }
                }
                if (minReg > -17 && maxReg < 16)
                {
                    if (i < AKM_Pow(2.0f, BIT_COEF2G) - 1)
                    {
                        arrCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1] = 0.5f * AKM_Pow(2.0f, i + 1);
                        arrRegCoef2Sub[NUM_COEF2_2 - 1] = i + 1;

                        for (j = 0; j < NUM_COEF2_2 - 1; j++)
                        {
                            arrRegAbsCoef2[j] = (int)(arrCoef2[NUM_COEF2_1 + j] / arrCoef2[NUM_COEF2_1 + NUM_COEF2_2 - 1]);
                        }
                        //Incremental
                        arrRegCoef2Sub[0] = arrRegAbsCoef2[0];
                        for (j = 1; j < NUM_COEF2_2 - 1; j++)
                        {
                            arrRegCoef2Sub[j] = arrRegAbsCoef2[j] - arrRegAbsCoef2[j - 1];
                        }
                    }
                    break;
                }
                else if (i == AKM_Pow(2.0f, BIT_COEF2G) - 1)
                {
                    return 1;
                }
            }

            // Calculation of Register value
            for (i = 0; i < NUM_COEF2_1; i++)
            {
                arrRegCoef2[i] = AKM_Round(arrCoef2[i] / AKM_Pow(2.0f, 13) * AKM_Pow(2.0f, 7));
            }

            for (i = 0; i < NUM_COEF2_2 - 1; i++)
            {
                if (arrRegCoef2[NUM_COEF2_1 + i] < 0) { arrRegCoef2[NUM_COEF2_1 + i] += 32; }
                if (arrRegCoef2Sub[i] < 0) { arrRegCoef2Sub[i] += 32; }
            }

            return 0;
        }
        float AKM_Pow(float dbData, int num)
        {
            int i;
            float pow = 1.0f;

            for (i = 0; i < num; i++)
            {
                pow *= dbData;
            }

            return pow;
        }
        int AKM_Round(float dbData)
        {
            float round = 0;

            if (dbData > 0) { round = dbData + 0.5f; }
            else if (dbData < 0) { round = dbData - 0.5f; }

            return (int)round;
        }
        float AKM_Abs(float dbData)
        {
            float abs = dbData;

            if (dbData < 0) { abs = dbData * -1.0f; }

            return abs;
        }
    }
    public class ringBufLinComp
    {
        public float[] dataX = new float[AFLinCompCoef.MAX_NUM_DATA];
        public float[] dataY = new float[AFLinCompCoef.MAX_NUM_DATA];
        public int startIndex;
        public int endIndex;
        public int size;

    }
}
