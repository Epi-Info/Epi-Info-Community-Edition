using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.WPF.Dashboard
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
        public double? stdDev;
        public double? variance;
        public string mainVariable;
        public string crosstabValue;
        public string errorMessage;

        public double? ssBetween;
        public double? dfBetween;
        public double? msBetween;
        public double? ssWithin;
        public double? dfWithin;
        public double? msWithin;
        public double? fStatistic;
        public double? anovaPValue;
        public double? chiSquare;
        public double? bartlettPValue;
        public double? kruskalWallisH;
        public double? kruskalPValue;
    }
}
