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
            getData("LogisticTestData", "iteration = 1");

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
    }
}
