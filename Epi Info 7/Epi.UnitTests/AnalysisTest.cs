using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Epi.Windows;
using EpiDashboard;
using Epi.Core;
using StatisticsRepository;
using System.Collections.Generic;
using System.Data;

namespace Epi.UnitTests
{
    [TestClass]
    public class AnalysisTest : StatisticsTests
    {
        [TestMethod]
        public void MeansTest()
        {
            int iterations = 100;

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\MeansTwoValueResults.csv"));
            int linesinfile = iterations;
            double[] obs0s = new double[linesinfile];
            double[] obs1s = new double[linesinfile];
            double[] total0s = new double[linesinfile];
            double[] total1s = new double[linesinfile];
            double[] mean0s = new double[linesinfile];
            double[] mean1s = new double[linesinfile];
            double[] var0s = new double[linesinfile];
            double[] var1s = new double[linesinfile];
            double[] sd0s = new double[linesinfile];
            double[] sd1s = new double[linesinfile];
            double[] min0s = new double[linesinfile];
            double[] min1s = new double[linesinfile];
            double[] q10s = new double[linesinfile];
            double[] q11s = new double[linesinfile];
            double[] median0s = new double[linesinfile];
            double[] median1s = new double[linesinfile];
            double[] q30s = new double[linesinfile];
            double[] q31s = new double[linesinfile];
            double[] max0s = new double[linesinfile];
            double[] max1s = new double[linesinfile];
            double[] mode0s = new double[linesinfile];
            double[] mode1s = new double[linesinfile];
            double[] poolmeans = new double[linesinfile];
            double[] poollcls = new double[linesinfile];
            double[] poolucls = new double[linesinfile];
            double[] sattermeans = new double[linesinfile];
            double[] satterlcls = new double[linesinfile];
            double[] satterucls = new double[linesinfile];
            double[] sds = new double[linesinfile];
            double[] equaldfs = new double[linesinfile];
            double[] equalts = new double[linesinfile];
            double[] equaltps = new double[linesinfile];
            double[] unequaldfs = new double[linesinfile];
            double[] unequalts = new double[linesinfile];
            double[] unequaltps = new double[linesinfile];
            double[] betweensss = new double[linesinfile];
            double[] withinsss = new double[linesinfile];
            double[] totalsss = new double[linesinfile];
            double[] betweendfs = new double[linesinfile];
            double[] withindfs = new double[linesinfile];
            double[] totaldfs = new double[linesinfile];
            double[] betweenmss = new double[linesinfile];
            double[] withinmss = new double[linesinfile];
            double[] fanovas = new double[linesinfile];
            double[] pfanovas = new double[linesinfile];
            double[] dfbarts = new double[linesinfile];
            double[] chibarts = new double[linesinfile];
            double[] chipbarts = new double[linesinfile];
            double[] chikwts = new double[linesinfile];
            double[] dfkwts = new double[linesinfile];
            double[] pchikwts = new double[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[1], out obs0s[i]);
                rv = Double.TryParse(values[2], out obs1s[i]);
                rv = Double.TryParse(values[3], out total0s[i]);
                rv = Double.TryParse(values[4], out total1s[i]);
                rv = Double.TryParse(values[5], out mean0s[i]);
                rv = Double.TryParse(values[6], out mean1s[i]);
                rv = Double.TryParse(values[7], out var0s[i]);
                rv = Double.TryParse(values[8], out var1s[i]);
                rv = Double.TryParse(values[9], out sd0s[i]);
                rv = Double.TryParse(values[10], out sd1s[i]);
                rv = Double.TryParse(values[11], out min0s[i]);
                rv = Double.TryParse(values[12], out min1s[i]);
                rv = Double.TryParse(values[13], out q10s[i]);
                rv = Double.TryParse(values[14], out q11s[i]);
                rv = Double.TryParse(values[15], out median0s[i]);
                rv = Double.TryParse(values[16], out median1s[i]);
                rv = Double.TryParse(values[17], out q30s[i]);
                rv = Double.TryParse(values[18], out q31s[i]);
                rv = Double.TryParse(values[19], out max0s[i]);
                rv = Double.TryParse(values[20], out max1s[i]);
                rv = Double.TryParse(values[21], out mode0s[i]);
                rv = Double.TryParse(values[22], out mode1s[i]);
                rv = Double.TryParse(values[23], out poolmeans[i]);
                rv = Double.TryParse(values[24], out poollcls[i]);
                rv = Double.TryParse(values[25], out poolucls[i]);
                rv = Double.TryParse(values[26], out sattermeans[i]);
                rv = Double.TryParse(values[27], out satterlcls[i]);
                rv = Double.TryParse(values[28], out satterucls[i]);
                rv = Double.TryParse(values[29], out sds[i]);
                rv = Double.TryParse(values[30], out equaldfs[i]);
                rv = Double.TryParse(values[31], out equalts[i]);
                rv = Double.TryParse(values[32], out equaltps[i]);
                rv = Double.TryParse(values[33], out unequaldfs[i]);
                rv = Double.TryParse(values[34], out unequalts[i]);
                rv = Double.TryParse(values[35], out unequaltps[i]);
                rv = Double.TryParse(values[36], out betweensss[i]);
                rv = Double.TryParse(values[37], out withinsss[i]);
                rv = Double.TryParse(values[38], out totalsss[i]);
                rv = Double.TryParse(values[39], out betweendfs[i]);
                rv = Double.TryParse(values[40], out withindfs[i]);
                rv = Double.TryParse(values[41], out totaldfs[i]);
                rv = Double.TryParse(values[42], out betweenmss[i]);
                rv = Double.TryParse(values[43], out withinmss[i]);
                rv = Double.TryParse(values[44], out fanovas[i]);
                rv = Double.TryParse(values[45], out pfanovas[i]);
                rv = Double.TryParse(values[46], out dfbarts[i]);
                rv = Double.TryParse(values[47], out chibarts[i]);
                rv = Double.TryParse(values[48], out chipbarts[i]);
                rv = Double.TryParse(values[49], out chikwts[i]);
                rv = Double.TryParse(values[50], out dfkwts[i]);
                rv = Double.TryParse(values[51], out pchikwts[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            EpiDashboard.MeansParameters Parameters = new MeansParameters();
            Parameters.CrosstabVariableName = "crosstabvar2";
            Parameters.ColumnNames.Add("meanvar");
            Parameters.ColumnNames.Add("crosstabvar2");
            Parameters.IncludeFullSummaryStatistics = true;

            for (int j = 0; j < iterations; j++)
            {
                getData("MeansTestData", "iteration = " + j);
                Parameters.CustomFilter = "iteration = " + j;
                Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(Parameters);
                foreach (object key in stratifiedFrequencyTables.Keys)
                {
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].observations, obs0s[j]);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].observations, obs1s[j]);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].sum.Value, total0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].sum.Value, total1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].mean.Value, mean0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].mean.Value, mean1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].variance.Value, var0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].variance.Value, var1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].stdDev.Value, sd0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].stdDev.Value, sd1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].min.Value, min0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].min.Value, min1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].q1.Value, q10s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].q1.Value, q11s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].median.Value, median0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].median.Value, median1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].q3.Value, q30s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].q3.Value, q31s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].max.Value, max0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].max.Value, max1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].mode.Value, mode0s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][1].mode.Value, mode1s[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].meansDiff, poolmeans[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].equalLCLMean, poollcls[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].equalUCLMean, poolucls[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].meansDiff, sattermeans[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].unequalLCLMean, satterlcls[j], 0.001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].unequalUCLMean, satterucls[j], 0.001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].stdDevDiff, sds[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].df, equaldfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].tStatistic, equalts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].pEqual, equaltps[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].SatterthwaiteDF, unequaldfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].tStatisticUnequal, unequalts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].pUneqal, unequaltps[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssBetween.Value, betweensss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssWithin.Value, withinsss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssBetween.Value + stratifiedFrequencyTables[(System.Data.DataTable)key][0].ssWithin.Value, totalsss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, betweendfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfWithin.Value, withindfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value + stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfWithin.Value, totaldfs[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].msBetween.Value, betweenmss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].msWithin.Value, withinmss[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].fStatistic.Value, fanovas[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].anovaPValue.Value, pfanovas[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, dfbarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].chiSquare.Value, chibarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].bartlettPValue.Value, chipbarts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].kruskalWallisH.Value, chikwts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].dfBetween.Value, dfkwts[j], 0.0001);
                    Assert.AreEqual(stratifiedFrequencyTables[(System.Data.DataTable)key][0].kruskalPValue.Value, pchikwts[j], 0.0001);
                }
            }
        }

        [TestMethod]
        public void TwoByTwoTest()
        {
            int iterations = 100;
            StatisticsRepository.cTable cTable = new StatisticsRepository.cTable();

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\TwoBy2Stats.csv"));
            int linesinfile = iterations;
            double[] chisqs = new double[linesinfile];
            double[] chisqps = new double[linesinfile];
            double[] mhchisqs = new double[linesinfile];
            double[] mhchisqps = new double[linesinfile];
            double[] yateschisqs = new double[linesinfile];
            double[] yateschisqps = new double[linesinfile];
            double[] ors = new double[linesinfile];
            double[] orlowers = new double[linesinfile];
            double[] oruppers = new double[linesinfile];
            double[] rrs = new double[linesinfile];
            double[] rrlowers = new double[linesinfile];
            double[] rruppers = new double[linesinfile];
            double[] fisherlowers = new double[linesinfile];
            double[] fisheruppers = new double[linesinfile];
            double[] fisher1s = new double[linesinfile];
            double[] fisher2s = new double[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[1], out chisqs[i]);
                rv = Double.TryParse(values[2], out chisqps[i]);
                rv = Double.TryParse(values[3], out mhchisqs[i]);
                rv = Double.TryParse(values[4], out mhchisqps[i]);
                rv = Double.TryParse(values[5], out yateschisqs[i]);
                rv = Double.TryParse(values[6], out yateschisqps[i]);
                rv = Double.TryParse(values[7], out ors[i]);
                rv = Double.TryParse(values[8], out orlowers[i]);
                rv = Double.TryParse(values[9], out oruppers[i]);
                rv = Double.TryParse(values[10], out rrs[i]);
                rv = Double.TryParse(values[11], out rrlowers[i]);
                rv = Double.TryParse(values[12], out rruppers[i]);
                rv = Double.TryParse(values[13], out fisherlowers[i]);
                rv = Double.TryParse(values[14], out fisheruppers[i]);
                rv = Double.TryParse(values[15], out fisher1s[i]);
                rv = Double.TryParse(values[16], out fisher2s[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            for (int j = 0; j < iterations; j++)
            {
                getData("LogisticTestData", "iteration = " + j);
                int yy = mainTable.Select("depvar = 1 and indepvar1 = 1").Length;
                int yn = mainTable.Select("depvar = 1 and indepvar1 = 0").Length;
                int ny = mainTable.Select("depvar = 0 and indepvar1 = 1").Length;
                int nn= mainTable.Select("depvar = 0 and indepvar1 = 0").Length;
                StatisticsRepository.cTable.SingleTableResults singleTableResults = cTable.SigTable((double)yy, (double)yn, (double)ny, (double)nn, 0.95);
                Assert.AreEqual(singleTableResults.ChiSquareUncorrectedVal, chisqs[j], 0.0001);
                Assert.AreEqual(singleTableResults.ChiSquareUncorrected2P, chisqps[j], 0.0001);
                Assert.AreEqual(singleTableResults.ChiSquareMantelVal, mhchisqs[j], 0.0001);
                Assert.AreEqual(singleTableResults.ChiSquareMantel2P, mhchisqps[j], 0.0001);
                Assert.AreEqual(singleTableResults.ChiSquareYatesVal, yateschisqs[j], 0.0001);
                Assert.AreEqual(singleTableResults.ChiSquareYates2P, yateschisqps[j], 0.0001);
                Assert.AreEqual(singleTableResults.OddsRatioEstimate.Value, ors[j], 0.0001);
                Assert.AreEqual(singleTableResults.OddsRatioLower.Value, orlowers[j], 0.0001);
                Assert.AreEqual(singleTableResults.OddsRatioUpper.Value, oruppers[j], 0.001);
                Assert.AreEqual(singleTableResults.RiskRatioEstimate.Value, rrs[j], 0.0001);
                Assert.AreEqual(singleTableResults.RiskRatioLower.Value, rrlowers[j], 0.0001);
                Assert.AreEqual(singleTableResults.RiskRatioUpper.Value, rruppers[j], 0.0001);
                Assert.AreEqual(singleTableResults.OddsRatioMLEFisherLower, fisherlowers[j], 0.0001);
                Assert.AreEqual(singleTableResults.OddsRatioMLEFisherUpper, fisheruppers[j], 0.0001);
                Assert.AreEqual(singleTableResults.FisherExactP, fisher1s[j], 0.0001);
                Assert.AreEqual(singleTableResults.FisherExact2P, fisher2s[j], 0.0001);
            }
        }
    }
}
