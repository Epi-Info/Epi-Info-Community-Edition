using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Statistics
{
    public static class Single2x2
    {
        public static List<double> CalcPoly(List<double> yy, List<double> yn, List<double> ny, List<double> nn)
        {
            List<double> polyDD = new List<double>();
            List<double> SingleTableStats = new List<double>();
            double n0 = ny[0] + nn[0];
            double n1 = yy[0] + yn[0];
            double m1 = yy[0] + ny[0];
            double minA = Math.Max(0.0, m1 - n0);
            double maxA = Math.Min(m1, n1);
            polyDD.Add(1.0);
            double aa = minA;
            double bb = m1 - minA + 1.0;
            double cc = n1 - minA + 1.0;
            double dd = n0 - m1 + minA;

            for (int i = 1; i < (maxA - minA) + 1; i++)
            {
                polyDD.Add(polyDD[i - 1] * ((bb - i) / (aa + i)) * ((cc - i) / (dd + i)));
            }

            List<double> ETests = ExactTests(minA, yy[0], polyDD);

            SingleTableStats.Add(CalcCmle(1.0, minA, yy[0], maxA, polyDD));
            SingleTableStats.Add(CalcExactLim(false, true, SingleTableStats[0], minA, yy[0], maxA, polyDD));  /*Up Fisher*/
            SingleTableStats.Add(CalcExactLim(false, false, SingleTableStats[0], minA, yy[0], maxA, polyDD)); /*Up MidP*/
            SingleTableStats.Add(CalcExactLim(true, true, SingleTableStats[0], minA, yy[0], maxA, polyDD));   /*Low Fisher*/
            SingleTableStats.Add(CalcExactLim(true, false, SingleTableStats[0], minA, yy[0], maxA, polyDD));   /*Low MidP*/

            SingleTableStats[5] = Math.Min(ETests[3], ETests[4]);
            SingleTableStats[6] = Math.Min(ETests[0], ETests[1]);
            SingleTableStats[7] = ETests[2];

            return SingleTableStats;
        }

        public static List<double> ExactTests(double minSumA, double sumA, List<double> polyD)
        {
            List<double> ExactTests = new List<double>();
            int diff = (int) (sumA - minSumA);
            int degD = polyD.Count - 1;
            double upTail = polyD[degD];
            double twoTail = 0.0;
    	
            if (upTail <= 1.000001 * polyD[diff])
                twoTail = twoTail + upTail;
            for (int i = degD - 1; i >= diff; i--)
            {
                upTail = upTail + polyD[i];
                if (polyD[i] <= 1.000001 * polyD[diff])
                    twoTail = twoTail + polyD[i];
            }
    	
            double denom = upTail;
            for (int i = diff - 1; i >= 0; i--)
            {
                denom = denom + polyD[i];
                if (polyD[i] <= 1.000001 * polyD[diff])
                    twoTail = twoTail + polyD[i];
            }
    	
            ExactTests.Add(1.0 - (upTail - polyD[diff]) / denom);        /* Lower Fish */
            ExactTests.Add(upTail / denom);                              /* Upper Fish */
            ExactTests.Add(twoTail / denom);                             /*   Two Fish */
            ExactTests.Add(1.0 - (upTail - 0.5 * polyD[diff]) / denom);  /* Lower MidP */
            ExactTests.Add((upTail - 0.5 * polyD[diff]) / denom);        /* Upper MidP */
    	
            return ExactTests;
        }

        public static double CalcExactLim(Boolean pbLower, Boolean pbFisher, double approx, double minSumA, double sumA, double maxSumA, List<double> polyD)
        {
            double limit = 0.0;
            if (minSumA < sumA && sumA < maxSumA)
                limit = GetExactLim(pbLower, pbFisher, approx, minSumA, sumA, polyD);
            else if (sumA == minSumA)
            {
                if (!pbLower)
                    limit = GetExactLim(pbLower, pbFisher, approx, minSumA, sumA, polyD);
            }
            else if (sumA == maxSumA)
            {
                if (pbLower)
                    limit = GetExactLim(pbLower, pbFisher, approx, minSumA, sumA, polyD);
                else limit = Double.PositiveInfinity;
            }

            return limit;
        }

        public static double CalcCmle(double approx, double minSumA, double sumA, double maxSumA, List<double> polyD)
        {
            double cmle = 0.0;
            if (minSumA < sumA && sumA < maxSumA)
            {
                cmle = GetCmle(approx, minSumA, sumA, polyD);
            }
            else if (sumA == maxSumA)
                cmle = Double.PositiveInfinity;

            return cmle;
        }

        public static double GetExactLim(Boolean pbLower, Boolean pbFisher, double approx, double minSumA, double sumA, List<double> polyD)
        {
            int degN = (int)sumA - (int)minSumA;
            List<double> polyN = new List<double>();
            double pnConfLevel = 0.95;
            double value = 0.5 * (1.0 - pnConfLevel);
            if (pbLower)
                value = 0.5 * (1 + pnConfLevel);
            if (pbLower && pbFisher)
                degN = (int)sumA - (int)minSumA - 1;
            for (int i = 0; i <= degN; i++)
                polyN.Add(polyD[i]);
            if (!pbFisher)
                polyN[degN] = 0.5 * polyD[degN];
            double limit = Converge(approx, polyN, polyD, sumA, value);

            return limit;
        }

        public static double GetCmle(double approx, double minSumA, double sumA, List<double> polyD)
        {
            double value = sumA;
            int degN = polyD.Count;
            List<double> polyN = new List<double>();
            for (int i = 0; i< degN; i++)
            {
                polyN.Add((minSumA + (double)i) * polyD[i]);
            }
            double cmle = Converge(approx, polyN, polyD, sumA, value);

            return cmle;
        }
        
        public static double Converge(double approx, List<double> polyN, List<double> polyD, double sumA, double value)
        {
            double f0 = 0.0;
            double x0 = 0.0;
            double x1 = 0.0;
            double f1 = 0.0;
            List<double> coordinates = BracketRoot(approx, x0, x1, f0, f1, polyN, polyD, sumA, value);
            x0 = coordinates[0];
            x1 = coordinates[1];
            f0 = coordinates[2];
            f1 = coordinates[3];
            double cmle = Zero(x0, x1, f0, f1, polyN, polyD, sumA, value);

            return cmle;
        }
        
        public static List<double> BracketRoot(double approx, double x0, double x1, double f0, double f1, List<double> polyN, List<double> polyD, double sumA, double value)
        {
            int iter = 0;
            x1 = Math.Max(0.5, approx);
            f0 = Func(x0, polyN, polyD, sumA, value);
            f1 = Func(x1, polyN, polyD, sumA, value);
            while (f1 * f0 > 0.0 && iter < 10000)
            {
                iter = iter + 1;
                x0 = x1;
                f0 = f1;
                x1 = x1 * 1.5 * iter;
                f1 = Func(x1, polyN, polyD, sumA, value);
            }
            List<double> coordinates = new List<double>();
            coordinates.Add(x0);
            coordinates.Add(x1);
            coordinates.Add(f0);
            coordinates.Add(f1);
            return coordinates;
        }
        
        public static double Func(double r, List<double> polyN, List<double> polyD, double sumA, double value)
        {
            double numer = EvalPoly(polyN, polyN.Count - 1, r);
            double denom = EvalPoly(polyD, polyD.Count - 1, r);
            double func = 0.0;
            if (r <= 1.0)
                func = numer / denom - value;
            else
                func = (numer / Math.Pow(r, (double)polyD.Count - (double)polyN.Count)) / denom - value;

            return func;
        }
        
        public static double EvalPoly(List<double> c, int degC, double r)
        {
            double y = 0.0;
            if (r == 0.0)
                y = c[0];
            else if (r <= 1.0)
            {
                y = c[degC];
                if (r < 1.0)
                {
                    for (int i = degC - 1; i >= 0; i--)
                        y = y * r + c[i];
                }
                else
                {
                    for (int i = degC - 1; i >= 0; i--)
                        y = y + c[i];
                }
            }
            else if (r > 1.0)
            {
                y = c[0];
                r = 1.0 / r;
                for (int i = 1; i <= degC; i++)
                    y = y * r + c[i];
            }

            return y;
        }

        public static double Zero(double x0, double x1, double f0, double f1, List<double> polyN, List<double> polyD, double sumA, double value)
        {
            Boolean found = false;
            double f2 = 0.0;
            double x2 = 0.0;
            double swap = 0.0;
            int iter = 0;
            int errorRenamed = 0;

            if (Math.Abs(f0) < Math.Abs(f1))
            {
                swap = x0; x0 = x1; x1 = swap;
                swap = f0; f0 = f1; f1 = swap;
            }
            found = (f1 == 0.0);
            if (!found && f0 * f1 > 0.0)
                errorRenamed = 1;

            while (!found && iter < 10000 && errorRenamed == 0)
            {
                iter++;
                x2 = x1 - f1 * (x1 - x0) / (f1 - f0);
                f2 = Func(x2, polyN, polyD, sumA, value);
                if (f1 * f2 < 0.0)
                {
                    x0 = x1;
                    f0 = f1;
                }
                else
                    f0 = f0 * f1 / (f1 + f2);
                x1 = x2;
                f1 = f2;
                found = (Math.Abs(x1 - x0) < Math.Abs(x1) * 0.0000001 || f1 == 0.0);
            }
            double cmle = x1;
            if (!found && iter > 10000)
                cmle = Double.NaN;

            return cmle;
        }

        public static double[] MHStats(double a, double b, double c, double d, double P)
        {
            double[] MHStats = new double[15];
            double z = 0.0;
            if (P < 0.9500001 && P > 0.95 - 0.00001)
                z = 1.96;
            else if (P < 0.9900001 && P > 0.99 - 0.00001)
                z = 2.58;
            else if (P < 0.900001 && P > 0.9 + 0.00001)
                z = 1.64;
            else
                z = SharedResources.ZFromP(P);

            double n1 = a + b;
            double n0 = c + d;
            double m1 = a + c;
            double m0 = b + d;
            double n = m1 + m0;
            double re = a / n1;
            double ru = c / n0;
            double r = m1 / n;
            double pe = n1 / n;

            if (b * c < 0.00000001)
            {
                MHStats[0] = -1.0;
                MHStats[1] = -1.0;
                MHStats[2] = -1.0;
            }
            else
            {
                MHStats[0] = (a * d) / (b * c);
                if (d * a < 0.000001)
                {
                    MHStats[1] = -1;
                    MHStats[2] = -1;
                }
                else
                {
                    MHStats[1] = Math.Exp(Math.Log((a * d) / (b * c)) - z * Math.Sqrt(1 / a + 1 / b + 1 / c + 1 / d));
                    MHStats[2] = Math.Exp(Math.Log((a * d) / (b * c)) + z * Math.Sqrt(1 / a + 1 / b + 1 / c + 1 / d));
                }
            }

            if (ru < 0.00001)
            {
                MHStats[3] = -1.0;
                MHStats[4] = -1.0;
                MHStats[5] = -1.0;
            }
            else
            {
                MHStats[3] = re / ru;
                if (re < 0.00001)
                {
                    MHStats[4] = -1.0;
                    MHStats[5] = -1.0;
                }
                else
                {
                    MHStats[4] = Math.Exp(Math.Log((a / n1) / (c / n0)) - z * Math.Sqrt(d / (c * n0) + b / (n1 * a)));
                    MHStats[5] = Math.Exp(Math.Log((a / n1) / (c / n0)) + z * Math.Sqrt(d / (c * n0) + b / (n1 * a)));
                }
            }

            MHStats[6] = (re - ru) * 100;
            MHStats[7] = (re - ru - z * Math.Sqrt(re * (1 - re) / n1 + ru * (1 - ru) / n0)) * 100;
            MHStats[8] = (re - ru + z * Math.Sqrt(re * (1 - re) / n1 + ru * (1 - ru) / n0)) * 100;

            double h3 = m1 * m0 * n1 * n0;
            double phi = ((a * d) - b * c) / Math.Sqrt(h3);
            MHStats[9] = n * Math.Pow(phi, 2.0);
            MHStats[10] = SharedResources.PValFromChiSq(MHStats[9], 1);
            MHStats[11] = (n - 1) / h3 * Math.Pow((a * d - b * c), 2.0);
            MHStats[12] = SharedResources.PValFromChiSq(MHStats[11], 1);
            MHStats[13] = (n - 1) / h3 * Math.Pow((Math.Abs(a * d - b * c) - n * 0.5), 2.0);
            MHStats[14] = SharedResources.PValFromChiSq(MHStats[13], 1.0);

            return MHStats;
        }

        public static double bdtOR(List<double> yy, List<double> yn, List<double> ny, List<double> nn, double mleOR)
        {
            double bd = 0.0;
            double sumYY = 0.0;
            double sumAk = 0.0;
            double sumVar = 0.0;

            for (int i = 0; i < yy.Count; i++)
            {
                double N1k = yy[i] + ny[i];
                double N0k = yn[i] + nn[i];
                double  tk = yy[i] + yn[i];
                double Nk = yy[i] + yn[i] + ny[i] + nn[i];

                double a = 0.0;
                double b = Math.Min(tk, N1k);
                double Ak = (a + b) / 2.0;

                double precision = 0.00001;
                double psi = 0.0;

                do
                {
                    //Expected cell count for yy, under adjusted mleOR assumption
                    Ak = (a + b) / 2.0;
                    //Formula for iteratively computing expected count
                    psi = (Ak * (N0k - tk + Ak)) / ((N1k - Ak) * (tk - Ak));

                    if (psi < 0.0 || psi < mleOR)
                        a = Ak;
                    else
                        b = Ak;
                } while (Math.Abs(mleOR - psi) > precision);

                double var = 1 / (1 / Ak + 1 / (N1k - Ak) + 1 / (tk - Ak) + 1 / (N0k - tk + Ak));

                //The Breslow-Day (uncorrected) statistic
                bd += ((yy[i] - Ak) * (yy[i] - Ak)) / var;

                sumYY += yy[i];
                sumAk += Ak;
                sumVar += var;
            }

            //Tarone correction to Breslow-Day
            double correction = ((sumYY - sumAk) * (sumYY - sumAk)) / sumVar;

            bd -= correction;

            return bd;
        }

        public static double bdOR(List<double> yy, List<double> yn, List<double> ny, List<double> nn)
        {
            double bd = 0.0;

            double[] or = new double[yy.Count];
            double[] w = new double[yy.Count];

            for (int i = 0; i < yy.Count; i++)
            {
                or[i] = (yy[i] / yn[i]) / (ny[i] / nn[i]);
                w[i] = 1 / (1 / yy[i] + 1 / yn[i] + 1 / ny[i] + 1 / nn[i]);
            }

            double numerator = 0.0;
            double denominator = 0.0;

            for (int i = 0; i < yy.Count; i++)
            {
                numerator += w[i] * Math.Log(or[i]);
                denominator += w[i];
            }

            double orDirect = Math.Exp(numerator / denominator);

            for (int i = 0; i < yy.Count; i++)
                bd += Math.Pow(Math.Log(or[i]) - Math.Log(orDirect), 2.0) * w[i];

            return bd;
        }

        public static double bdRR(List<double> yy, List<double> yn, List<double> ny, List<double> nn)
        {
            double bd = 0.0;

            double[] rr = new double[yy.Count];
            double[] w = new double[yy.Count];

            for (int i = 0; i < yy.Count; i++)
            {
                rr[i] = (yy[i] / (yy[i] + yn[i])) / (ny[i] / (ny[i] + nn[i]));
                w[i] = 1 / (yn[i] / (yy[i] * (yy[i] + yn[i])) + nn[i] / (ny[i] * (ny[i] + nn[i])));
            }

            double numerator = 0.0;
            double denominator = 0.0;

            for (int i = 0; i < yy.Count; i++)
            {
                numerator += w[i] * Math.Log(rr[i]);
                denominator += w[i];
            }

            double rrDirect = Math.Exp(numerator / denominator);

            for (int i = 0; i < yy.Count; i++)
                bd += Math.Pow(Math.Log(rr[i]) - Math.Log(rrDirect), 2.0) * w[i];

            return bd;
        }
    }
}
