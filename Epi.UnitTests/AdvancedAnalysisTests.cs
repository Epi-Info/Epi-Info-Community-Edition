using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Epi.Windows;
using Epi.Core;
using StatisticsRepository;
using EpiInfo.Plugin;
using System.Collections.Generic;
using Epi;
using Epi.Data;
using EpiDashboard;
using EpiDashboard.Controls;
using Epi.Fields;
using System.Data;
using System.Xml;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using EpiDashboard.NutStat;
using EpiDashboard.Gadgets.Reporting;

namespace Epi.UnitTests
{
    [TestClass]
    public class AdvancedAnalysisTests : StatisticsTests
    {
        private struct VariableRow
        {
            public string variableName;
            public double oddsRatio;
            public double ninetyFivePercent;
            public double ci;
            public double coefficient;
            public double se;
            public double Z;
            public double P;
            public double Ftest;
            public double stdError;
            public double coefficientLower;
            public double coefficientUpper;
        }

        private struct RegressionResults
        {
            public List<VariableRow> variables;
            public string convergence;
            public int iterations;
            public double finalLikelihood;
            public int casesIncluded;
            public double scoreStatistic;
            public double scoreDF;
            public double scoreP;
            public double LRStatistic;
            public double LRDF;
            public double LRP;
            public string errorMessage;
            public StatisticsRepository.LogisticRegression.LogisticRegressionResults regressionResults;
        }

        [TestMethod]
        public void LinearRegressionTest()
        {
            int iterations = 100;

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\LinearRegressionResults.csv"));
            int linesinfile = iterations;
            double[] constants = new double[linesinfile];
            double[] constantlcls = new double[linesinfile];
            double[] constantucls = new double[linesinfile];
            double[] constantstes = new double[linesinfile];
            double[] i1s = new double[linesinfile];
            double[] i1lcls = new double[linesinfile];
            double[] i1ucls = new double[linesinfile];
            double[] i1stes = new double[linesinfile];
            double[] i2s = new double[linesinfile];
            double[] i2lcls = new double[linesinfile];
            double[] i2ucls = new double[linesinfile];
            double[] i2stes = new double[linesinfile];
            double[] i3s = new double[linesinfile];
            double[] i3lcls = new double[linesinfile];
            double[] i3ucls = new double[linesinfile];
            double[] i3stes = new double[linesinfile];
            double[] fconstants = new double[linesinfile];
            double[] pfconstants = new double[linesinfile];
            double[] f1s = new double[linesinfile];
            double[] pf1s = new double[linesinfile];
            double[] f2s = new double[linesinfile];
            double[] pf2s = new double[linesinfile];
            double[] f3s = new double[linesinfile];
            double[] pf3s = new double[linesinfile];
            double[] regdfs = new double[linesinfile];
            double[] regsss = new double[linesinfile];
            double[] regmss = new double[linesinfile];
            double[] fstatistics = new double[linesinfile];
            double[] pvalues = new double[linesinfile];
            double[] resdfs = new double[linesinfile];
            double[] ressss = new double[linesinfile];
            double[] resmss = new double[linesinfile];
            double[] totdfs = new double[linesinfile];
            double[] totsss = new double[linesinfile];
            double[] rsquares = new double[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[1], out constants[i]);
                rv = Double.TryParse(values[2], out constantlcls[i]);
                rv = Double.TryParse(values[3], out constantucls[i]);
                rv = Double.TryParse(values[4], out constantstes[i]);
                rv = Double.TryParse(values[5], out i1s[i]);
                rv = Double.TryParse(values[6], out i1lcls[i]);
                rv = Double.TryParse(values[7], out i1ucls[i]);
                rv = Double.TryParse(values[8], out i1stes[i]);
                rv = Double.TryParse(values[9], out i2s[i]);
                rv = Double.TryParse(values[10], out i2lcls[i]);
                rv = Double.TryParse(values[11], out i2ucls[i]);
                rv = Double.TryParse(values[12], out i2stes[i]);
                rv = Double.TryParse(values[13], out i3s[i]);
                rv = Double.TryParse(values[14], out i3lcls[i]);
                rv = Double.TryParse(values[15], out i3ucls[i]);
                rv = Double.TryParse(values[16], out i3stes[i]);
                rv = Double.TryParse(values[17], out fconstants[i]);
                rv = Double.TryParse(values[18], out pfconstants[i]);
                rv = Double.TryParse(values[19], out f1s[i]);
                rv = Double.TryParse(values[20], out pf1s[i]);
                rv = Double.TryParse(values[21], out f2s[i]);
                rv = Double.TryParse(values[22], out pf2s[i]);
                rv = Double.TryParse(values[23], out f3s[i]);
                rv = Double.TryParse(values[24], out pf3s[i]);
                rv = Double.TryParse(values[25], out regdfs[i]);
                rv = Double.TryParse(values[26], out regsss[i]);
                rv = Double.TryParse(values[27], out regmss[i]);
                rv = Double.TryParse(values[28], out fstatistics[i]);
                rv = Double.TryParse(values[29], out pvalues[i]);
                rv = Double.TryParse(values[30], out resdfs[i]);
                rv = Double.TryParse(values[31], out ressss[i]);
                rv = Double.TryParse(values[32], out resmss[i]);
                rv = Double.TryParse(values[33], out totdfs[i]);
                rv = Double.TryParse(values[34], out totsss[i]);
                rv = Double.TryParse(values[35], out rsquares[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            for (int j = 0; j < iterations; j++)
            {
                getData("LinearRegressionTestData", "iteration = " + j);

                Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                inputVariableList.Add("depvar", "dependvar");
                inputVariableList.Add("intercept", "true");
                inputVariableList.Add("includemissing", "false");
                inputVariableList.Add("P", "0.95");
                inputVariableList.Add("indepvar1", "unsorted");
                inputVariableList.Add("indepvar2", "unsorted");
                inputVariableList.Add("indepvar3", "unsorted");

                DataTable regressTable = new DataTable();

                LinearRegression.LinearRegressionResults results = new LinearRegression.LinearRegressionResults();

                StatisticsRepository.LinearRegression linearRegression = new StatisticsRepository.LinearRegression();
                results = linearRegression.LinearRegression(inputVariableList, mainTable);

                double tScore = 1.9;
                while (Epi.Statistics.SharedResources.PFromT(tScore, results.residualsDf) > 0.025)
                {
                    tScore += 0.000001;
                }
                List<VariableRow> moreStats = new List<VariableRow>();
                foreach (StatisticsRepository.LinearRegression.VariableRow vrow in results.variables)
                {
                    VariableRow nrow = new VariableRow();
                    nrow.coefficient = vrow.coefficient;
                    nrow.Ftest = vrow.Ftest;
                    nrow.P = vrow.P;
                    nrow.stdError = vrow.stdError;
                    nrow.variableName = vrow.variableName;
                    nrow.coefficientLower = nrow.coefficient - tScore * nrow.stdError;
                    nrow.coefficientUpper = nrow.coefficient + tScore * nrow.stdError;
                    moreStats.Add(nrow);
                }
                double regressionFp = Epi.Statistics.SharedResources.PFromF(results.regressionF, results.regressionDf, results.residualsDf);

                Assert.AreEqual(results.variables[3].coefficient, constants[j], 0.0001);
                Assert.AreEqual(moreStats[3].coefficientLower, constantlcls[j], 0.0001);
                Assert.AreEqual(moreStats[3].coefficientUpper, constantucls[j], 0.0001);
                Assert.AreEqual(moreStats[3].stdError, constantstes[j], 0.0001);
                Assert.AreEqual(results.variables[0].coefficient, i1s[j], 0.0001);
                Assert.AreEqual(moreStats[0].coefficientLower, i1lcls[j], 0.0001);
                Assert.AreEqual(moreStats[0].coefficientUpper, i1ucls[j], 0.0001);
                Assert.AreEqual(moreStats[0].stdError, i1stes[j], 0.0001);
                Assert.AreEqual(results.variables[1].coefficient, i2s[j], 0.0001);
                Assert.AreEqual(moreStats[1].coefficientLower, i2lcls[j], 0.0001);
                Assert.AreEqual(moreStats[1].coefficientUpper, i2ucls[j], 0.0001);
                Assert.AreEqual(moreStats[1].stdError, i2stes[j], 0.0001);
                Assert.AreEqual(results.variables[2].coefficient, i3s[j], 0.0001);
                Assert.AreEqual(moreStats[2].coefficientLower, i3lcls[j], 0.0001);
                Assert.AreEqual(moreStats[2].coefficientUpper, i3ucls[j], 0.0001);
                Assert.AreEqual(moreStats[2].stdError, i3stes[j], 0.0001);
                Assert.AreEqual(results.variables[3].Ftest, fconstants[j], 0.0001);
                Assert.AreEqual(results.variables[3].P, pfconstants[j], 0.0001);
                Assert.AreEqual(results.variables[0].Ftest, f1s[j], 0.0001);
                Assert.AreEqual(results.variables[0].P, pf1s[j], 0.0001);
                Assert.AreEqual(results.variables[1].Ftest, f2s[j], 0.0001);
                Assert.AreEqual(results.variables[1].P, pf2s[j], 0.0001);
                Assert.AreEqual(results.variables[2].Ftest, f3s[j], 0.0001);
                Assert.AreEqual(results.variables[2].P, pf3s[j], 0.0001);
                Assert.AreEqual(results.regressionDf, regdfs[j], 0.0001);
                Assert.AreEqual(results.regressionSumOfSquares, regsss[j], 0.0001);
                Assert.AreEqual(results.regressionMeanSquare, regmss[j], 0.0001);
                Assert.AreEqual(results.regressionF, fstatistics[j], 0.0001);
                if (pvalues[j] < 0.001)
                    Assert.AreEqual(regressionFp, pvalues[j], 0.0001);
                else if (pvalues[j] < 0.01)
                    Assert.AreEqual(regressionFp, pvalues[j], 0.001);
                else if (pvalues[j] < 0.1)
                    Assert.AreEqual(regressionFp, pvalues[j], 0.01);
                else
                    Assert.AreEqual(regressionFp, pvalues[j], 0.1);
                Assert.AreEqual(results.residualsDf, resdfs[j], 0.0001);
                Assert.AreEqual(results.residualsSumOfSquares, ressss[j], 0.0001);
                Assert.AreEqual(results.residualsMeanSquare, resmss[j], 0.0001);
                Assert.AreEqual(results.totalDf, totdfs[j], 0.0001);
                Assert.AreEqual(results.totalSumOfSquares, totsss[j], 0.0001);
                Assert.AreEqual(results.correlationCoefficient, rsquares[j], 0.0001);
            }
        }

        [TestMethod]
        public void LogisticRegressionTest()
        {
            int iterations = 100;

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\LogisticCoefficientResults.csv"));
            int linesinfile = 3 * iterations;
            double[] coefficients = new double[linesinfile];
            double[] stderrs = new double[linesinfile];
            double[] chisqs = new double[linesinfile];
            double[] ps = new double[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[2], out coefficients[i]);
                rv = Double.TryParse(values[3], out stderrs[i]);
                rv = Double.TryParse(values[4], out chisqs[i]);
                rv = Double.TryParse(values[5], out ps[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            reader = new StreamReader(File.OpenRead(@"..\..\Data\LogisticOddsRatioResults.csv"));
            linesinfile = 2 * iterations;
            double[] oddsratios = new double[linesinfile];
            double[] lowercls = new double[linesinfile];
            double[] uppercls = new double[linesinfile];
            i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[2], out oddsratios[i]);
                rv = Double.TryParse(values[3], out lowercls[i]);
                rv = Double.TryParse(values[4], out uppercls[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            for (int j = 0; j < iterations; j++)
            {
                getData("LogisticTestData", "iteration = " + j);

                Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                inputVariableList.Add("depvar", "dependvar");
                inputVariableList.Add("intercept", "true");
                inputVariableList.Add("includemissing", "false");
                inputVariableList.Add("P", "0.95");
                inputVariableList.Add("indepvar1", "unsorted");
                inputVariableList.Add("indepvar2", "unsorted");

                DataTable regressTable = new DataTable();

                RegressionResults results = new RegressionResults();

                StatisticsRepository.LogisticRegression logisticRegression = new StatisticsRepository.LogisticRegression();
                results.regressionResults = logisticRegression.LogisticRegression(inputVariableList, mainTable);
                if (false)
                {
                    System.Diagnostics.Trace.Write(results.regressionResults.variables[0].oddsRatio + "\t" +
                        results.regressionResults.variables[0].ninetyFivePercent + "\t" +
                        results.regressionResults.variables[0].ci + "\t" +
                        results.regressionResults.variables[0].coefficient + "\t" +
                        results.regressionResults.variables[0].se + "\t" +
                        results.regressionResults.variables[0].Z + "\t" +
                        results.regressionResults.variables[0].P + "\n");
                    System.Diagnostics.Trace.Write(results.regressionResults.variables[1].oddsRatio + "\t" +
                        results.regressionResults.variables[1].ninetyFivePercent + "\t" +
                        results.regressionResults.variables[1].ci + "\t" +
                        results.regressionResults.variables[1].coefficient + "\t" +
                        results.regressionResults.variables[1].se + "\t" +
                        results.regressionResults.variables[1].Z + "\t" +
                        results.regressionResults.variables[1].P + "\n");
                }
                Assert.AreEqual(results.regressionResults.variables[0].oddsRatio, oddsratios[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].ninetyFivePercent, lowercls[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].ci, uppercls[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].coefficient, coefficients[j * 3 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].se, stderrs[j * 3 + 1], 0.01);
//                Assert.AreEqual(results.regressionResults.variables[0].Z, chisqs[j * 3 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].P, ps[j * 3 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].oddsRatio, oddsratios[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].ninetyFivePercent, lowercls[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].ci, uppercls[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].coefficient, coefficients[j * 3 + 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].se, stderrs[j * 3 + 2], 0.01);
//                Assert.AreEqual(results.regressionResults.variables[1].Z, chisqs[j * 3 + 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].P, ps[j * 3 + 2], 0.01);
            }
        }

        [TestMethod]
        public void MatchedLogisticRegressionTest()
        {
            int iterations = 100;

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\MatchedLogisticCoefficientResults.csv"));
            int linesinfile = 2 * iterations;
            double[] coefficients = new double[linesinfile];
            double[] stderrs = new double[linesinfile];
            double[] chisqs = new double[linesinfile];
            double[] ps = new double[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[2], out coefficients[i]);
                rv = Double.TryParse(values[3], out stderrs[i]);
                rv = Double.TryParse(values[4], out chisqs[i]);
                rv = Double.TryParse(values[5], out ps[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            reader = new StreamReader(File.OpenRead(@"..\..\Data\MatchedLogisticOddsRatioResults.csv"));
            linesinfile = 2 * iterations;
            double[] oddsratios = new double[linesinfile];
            double[] lowercls = new double[linesinfile];
            double[] uppercls = new double[linesinfile];
            i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = Double.TryParse(values[2], out oddsratios[i]);
                rv = Double.TryParse(values[3], out lowercls[i]);
                rv = Double.TryParse(values[4], out uppercls[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            for (int j = 0; j < iterations; j++)
            {
                getData("MatchedLogisticTestData", "iteration = " + j);

                Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                inputVariableList.Add("case", "dependvar");
                inputVariableList.Add("intercept", "true");
                inputVariableList.Add("includemissing", "false");
                inputVariableList.Add("GROUPID", "matchvar");
                inputVariableList.Add("P", "0.95");
                inputVariableList.Add("indepvar1", "unsorted");
                inputVariableList.Add("indepvar2", "unsorted");

                DataTable regressTable = new DataTable();

                RegressionResults results = new RegressionResults();

                StatisticsRepository.LogisticRegression logisticRegression = new StatisticsRepository.LogisticRegression();
                results.regressionResults = logisticRegression.LogisticRegression(inputVariableList, mainTable);
                if (false)
                {
                    System.Diagnostics.Trace.Write(results.regressionResults.variables[0].oddsRatio + "\t" +
                        results.regressionResults.variables[0].ninetyFivePercent + "\t" +
                        results.regressionResults.variables[0].ci + "\t" +
                        results.regressionResults.variables[0].coefficient + "\t" +
                        results.regressionResults.variables[0].se + "\t" +
                        results.regressionResults.variables[0].Z + "\t" +
                        results.regressionResults.variables[0].P + "\n");
                    System.Diagnostics.Trace.Write(results.regressionResults.variables[1].oddsRatio + "\t" +
                        results.regressionResults.variables[1].ninetyFivePercent + "\t" +
                        results.regressionResults.variables[1].ci + "\t" +
                        results.regressionResults.variables[1].coefficient + "\t" +
                        results.regressionResults.variables[1].se + "\t" +
                        results.regressionResults.variables[1].Z + "\t" +
                        results.regressionResults.variables[1].P + "\n");
                }
                Assert.AreEqual(results.regressionResults.variables[0].oddsRatio, oddsratios[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].ninetyFivePercent, lowercls[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].ci, uppercls[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].coefficient, coefficients[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].se, stderrs[j * 2], 0.01);
                //                Assert.AreEqual(results.regressionResults.variables[0].Z, chisqs[j * 3 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[0].P, ps[j * 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].oddsRatio, oddsratios[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].ninetyFivePercent, lowercls[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].ci, uppercls[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].coefficient, coefficients[j * 2 + 1], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].se, stderrs[j * 2 + 1], 0.01);
                //                Assert.AreEqual(results.regressionResults.variables[1].Z, chisqs[j * 3 + 2], 0.01);
                Assert.AreEqual(results.regressionResults.variables[1].P, ps[j * 2 + 1], 0.01);
            }
        }
    }
}
