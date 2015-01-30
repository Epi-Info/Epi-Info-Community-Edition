using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Statistics
{
    public static class SingleMxN
    {
        public static double[] CalcChiSq(System.Data.DataRow[] SortedRows, Boolean classic)
        {
            double[] ChiSq = { 0.0, 0.0 };
            if (classic)
            {
                int SRLength = SortedRows.Length;
                int SRWidth = SortedRows[0].ItemArray.Length;
                double[] totals = new double[SRWidth];
                for (int i = 0; i < SRLength; i++)
                    for (int j = 1; j < SRWidth; j++)
                        totals[j] += (double)SortedRows[i][j];
                double[] ps = new double[SRWidth];
                for (int j = 2; j < SRWidth; j++)
                    ps[j] = totals[j] / totals[1];
                double[] observed = new double[SRLength * (SRWidth - 2)];
                double[] expected = new double[SRLength * (SRWidth - 2)];
                double[] OminusESqOverE = new double[SRLength * (SRWidth - 2)];
                int k = 0;
                for (int i = 0; i < SRLength; i++)
                    for (int j = 0; j < SRWidth - 2; j++)
                    {
                        observed[k] = (double)SortedRows[i][j + 2];
                        expected[k] = (double)SortedRows[i][1] * ps[j + 2];
                        OminusESqOverE[k] = Math.Pow(observed[k] - expected[k], 2.0) / expected[k];
                        ChiSq[0] += OminusESqOverE[k];
                        if (expected[k] < 1.0)
                            ChiSq[1] = 1.0;
                        if (expected[k] < 5.0 && ChiSq[1] == 0.0)
                            ChiSq[1] = 5.0;
                        k++;
                    }
            }
            else
            {
                int SRLength = SortedRows.Length;
                int SRWidth = SortedRows[0].ItemArray.Length;
                double[] totals = new double[SRWidth + 1];
                double[] rowtotals = new double[SRLength];
                for (int i = 0; i < SRLength; i++)
                {
                    for (int j = 2; j < SRWidth + 1; j++)
                    {
                        totals[j] += (double)SortedRows[i][j - 1];
                        rowtotals[i] += (double)SortedRows[i][j - 1];
                    }
                    totals[1] += rowtotals[i];
                }
                double[] ps = new double[SRWidth + 1];
                for (int j = 2; j < SRWidth + 1; j++)
                    ps[j] = totals[j] / totals[1];
                double[] observed = new double[SRLength * (SRWidth - 1)];
                double[] expected = new double[SRLength * (SRWidth - 1)];
                double[] OminusESqOverE = new double[SRLength * (SRWidth - 1)];
                int k = 0;
                for (int i = 0; i < SRLength; i++)
                    for (int j = 0; j < SRWidth - 1; j++)
                    {
                        observed[k] = (double)SortedRows[i][j + 1];
                        expected[k] = rowtotals[i] * ps[j + 2];
                        OminusESqOverE[k] = Math.Pow(observed[k] - expected[k], 2.0) / expected[k];
                        ChiSq[0] += OminusESqOverE[k];
                        if (expected[k] < 5.0)
                            ChiSq[1] = 1.0;
                        k++;
                    }
            }
            return ChiSq;
        }
    }
}
