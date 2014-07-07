using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Statistics
{
    public class pmccTable
    {
        public pmccTable.SingleTableResults statTable(double X, double Y)
        {
            pmccTable.SingleTableResults singleTable = new pmccTable.SingleTableResults();
            singleTable.FisherExactP = Double.NaN;
            singleTable.FisherExact2P = Double.NaN;
            singleTable.OddsRatioFisherLower = Double.NaN;
            singleTable.OddsRatioFisherUpper = Double.NaN;
            if (X == 0.0 || Y == 0.0)
            {
                X += 0.5;
                Y += 0.5;
            }
            else
            {
            }
            if (Y > 0.5)
            {
                double[] oneTailFish = oneFish(Math.Floor(X), Math.Floor(Y));
                singleTable.FisherExactP = oneTailFish[0];
                singleTable.FisherExact2P = twoFish(oneTailFish[0], oneTailFish[1], Math.Floor(X), Math.Floor(Y));
                double[] fisherCI = fisherLimits(X, Y);
                singleTable.OddsRatioFisherLower = fisherCI[0];
                singleTable.OddsRatioFisherUpper = fisherCI[1];
            }
            singleTable.McNemarUncorrectedVal = Math.Pow(X - Y, 2.0) / (X + Y);
            singleTable.McNemarUncorrected2P = Epi.Statistics.SharedResources.PValFromChiSq(singleTable.McNemarUncorrectedVal, 1.0);
            singleTable.McNemarCorVal = Math.Pow(Math.Abs(X - Y) - 1.0, 2.0) / (X + Y);
            singleTable.McNemarCor2P = Epi.Statistics.SharedResources.PValFromChiSq(singleTable.McNemarCorVal, 1.0);
            singleTable.OddsRatioEstimate = X / Y;
            singleTable.OddsRatioLower = Math.Exp(Math.Log((double)singleTable.OddsRatioEstimate) - 1.96 * Math.Pow(1 / X + 1 / Y, 0.5));
            singleTable.OddsRatioUpper = Math.Exp(Math.Log((double)singleTable.OddsRatioEstimate) + 1.96 * Math.Pow(1 / X + 1 / Y, 0.5));
            return singleTable;
        }

        private double[] oneFish(double X, double Y)
        {
            double lowfish = 0.0;
            double upfish = 0.0;
            for (int k = 0; k <= (int) X; k++)
            {
                lowfish += choosey(X + Y, (double) k) * Math.Pow(0.5, X + Y);
            }
            for (int k = (int) X; k <= (int) (X + Y); k++)
            {
                upfish += choosey(X + Y, (double) k) * Math.Pow(0.5, X + Y);
            }
            double lowup = 0.0;
            if (upfish < lowfish) lowup = 1.0;
            double[] retval = new double[2];
            retval[0] = Math.Min(lowfish, upfish);
            retval[1] = lowup;
            return retval;
        }

        private double twoFish(double oneFish, double lowup, double X, double Y)
        {
            double p = oneFish;
            double Xp = choosey(X + Y, X) * Math.Pow(0.5, X + Y);
            if (lowup == 1.0)
            {
                for (int k = 0; k < (int) X; k++)
                {
                    double tempP = choosey(X + Y, (double) k) * Math.Pow(0.5, X + Y);
                    if (tempP < Xp)
                        p += tempP;
                }
            }
            else
            {
                for (int k = (int) X; k < (int) (X + Y); k++)
                {
                    double tempP = choosey(X + Y, (double) k) * Math.Pow(0.5, X + Y);
                    if (tempP < Xp)
                        p += tempP;
                }
            }
            return p;
        }
	
	private  double[] fisherLimits(double X, double Y)
	{
		double[] limits = new double[2];
		double F = 0.0;
		double p = 1.0;
		while (p > 0.975)
		{
			F += 0.0001;
			p = Epi.Statistics.SharedResources.PFromF(F, 2.0 * X, 2.0 * (Y + 1.0));
		}
		limits[0] =  X * F / (Y + 1.0);
		while (p > 0.025)
		{
			F += 0.0001;
            p = Epi.Statistics.SharedResources.PFromF(F, 2.0 * (X + 1.0), 2.0 * Y);
		}
		limits[1] =  (X + 1.0) * F / Y;
		return limits;
	}

        private double choosey(double chooa, double choob)
        {
            double ccccc = chooa - choob;
            if (choob < chooa / 2)
                choob = ccccc;
            double choosey = 1.0;
            for (int i = (int)choob + 1; i <= (int)chooa; i++)
                choosey = (choosey * i) / (chooa - (i - 1));

            return choosey;
        }

        public struct SingleTableResults
        {
            public double McNemarUncorrectedVal;
            public double McNemarUncorrected2P;
            public double McNemarCorVal;
            public double McNemarCor2P;
            public double FisherExactP;
            public double FisherExact2P;
            public double? OddsRatioEstimate;
            public double? OddsRatioLower;
            public double? OddsRatioUpper;
            public double OddsRatioFisherLower;
            public double OddsRatioFisherUpper;
        }
    }
}
