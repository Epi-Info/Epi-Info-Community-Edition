using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    /// <summary>
    /// A structure used to store the mean, median, mode, min, and max of a given numeric column. Also
    /// used to store ANOVA, Kruskal-Wallis and other advanced statistics when appropriate.
    /// </summary>
    public struct DescriptiveStatistics
    {
        public double? mean;
        public double? grandMean;
        public double? median;
        public double? mode;
        public double? min;
        public double? max;
        public double? q1;
        public double? q3;
        public double? sum;
        public double observations;
        public double unweightedObservations;
        public double? stdDev;
        public double? variance;
        public double? stdError;
        public double? tZero;
        public double? tZeroP;
        public string mainVariable;
        public string crosstabValue;
        public string errorMessage;

        public double? ssBetween;
        public double? dfBetween;
        public double? msBetween;
        public double? ssWithin;
        public double? dfWithin;
        public double? dfError;
        public double? msWithin;
        public double? fStatistic;
        public double? anovaPValue;
        public double? chiSquare;
        public double? bartlettPValue;
        public double? kruskalWallisH;
        public double? kruskalPValue;
        public double? kurtosis;
        public double? skewness;
        public double meansDiff;
        public double stdDevDiff;
        public double equalLCLMean;
        public double equalUCLMean;
        public double unequalLCLMean;
        public double unequalUCLMean;
        public int df;
        public double SatterthwaiteDF;
        public double tStatistic;
        public double pEqual;
        public double tStatisticUnequal;
        public double pUneqal;
        public int crosstabs;
    }
}
