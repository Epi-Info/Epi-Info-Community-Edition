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
    }
}
