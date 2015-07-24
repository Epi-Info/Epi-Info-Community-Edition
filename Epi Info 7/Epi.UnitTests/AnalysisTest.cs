using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Epi.Windows;
using EpiDashboard;
using Epi.Core;
using StatisticsRepository;

namespace Epi.UnitTests
{
    [TestClass]
    public class AnalysisTest : StatisticsTests
    {
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
