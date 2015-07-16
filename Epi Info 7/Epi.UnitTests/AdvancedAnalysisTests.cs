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
    public class AdvancedAnalysisTests
    {
        public DashboardHelper DashboardHelper { get; private set; }
        private DataSet ds;
        private DataTable mainTable;
        //        private IDbDriver db;
//       public IDbDriver Database
//        {
//            get
//            {
//                return this.db;
//            }
//        }
//        public string TableName { get; private set; }
//        public List<RelatedConnection> ConnectionsForRelate { get; private set; }

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
            getData("EColiFoodHistory");

            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
            inputVariableList.Add("ill", "dependvar");
            inputVariableList.Add("intercept", "true");
            inputVariableList.Add("includemissing", "false");
            inputVariableList.Add("P", "0.95");
            inputVariableList.Add("beansprouts", "unsorted");

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
        }

        private void getData(string filename)
        {
            string configFilePath = "..\\..\\..\\Build\\Debug\\Configuration\\EpiInfo.Config.xml";
            Epi.DataSets.Config configDataSet = new Epi.DataSets.Config();
            configDataSet.ReadXml(configFilePath);
            Epi.Configuration.Load(configFilePath);
            System.Data.Common.DbConnectionStringBuilder dbCnnStringBuilder = new System.Data.Common.DbConnectionStringBuilder();
            string provider = "Epi.Data.Office.CsvFileFactory, Epi.Data.Office";
            IDbDriverFactory dbFactory = null;
            dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(provider);
            IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);
            db.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Resources;Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
            object selectedDataSource = db;

            IDbDriver dbDriver = (IDbDriver)selectedDataSource;
            DashboardHelper = new DashboardHelper(filename + "#csv", dbDriver);
            IGadgetParameters inputs = new GadgetParameters();
            DashboardHelper.UserVarsNeedUpdating = true;
            PopulateDataSet(inputs);
        }

        private void PopulateDataSet(IGadgetParameters inputs = null)
        {
            DataTable unfilteredTable = new DataTable("unfilteredTable");
            unfilteredTable.CaseSensitive = true;

            if (inputs != null)
            {
                inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_GADGET_STATUS_CACHING_WAIT);
            }

            {
                if (ds == null || ds.Tables.Count == 0 || (ds.Tables[0].Rows.Count == 0 && ds.Tables[0].Columns.Count == 0))
                {
                    if (inputs != null)
                    {
                        inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_GADGET_STATUS_CACHING);
                    }

                    unfilteredTable = DashboardHelper.Database.GetTableData(DashboardHelper.TableName);

                    if (DashboardHelper.ConnectionsForRelate.Count > 0)
                    {
                        foreach (RelatedConnection conn in DashboardHelper.ConnectionsForRelate)
                        {
                            DataTable relatedTable = new DataTable();
                            relatedTable.CaseSensitive = true;

                            {
                                relatedTable = conn.db.GetTableData(conn.TableName);
                                relatedTable.TableName = conn.TableName;
                            }
                        }
                    }

                    DashboardHelper.AddSystemVariablesToTable(unfilteredTable);
                    DashboardHelper.AddPermanentVariablesToTable(unfilteredTable);
                    ds = new DataSet();
                    ds.Tables.Add(unfilteredTable); //ds.Tables.Add(ConvertTableColumns(unfilteredTable));
                    ds.Tables[0].CaseSensitive = true;
                    mainTable = ds.Tables[0];

                    #region Data Prep for values with spaces (Excel only)

                    if (DashboardHelper.Database.ConnectionDescription.Contains("Excel")) // only run if it's Excel
                    {
                        List<string> strColumnNames = DashboardHelper.GetFieldsAsList(ColumnDataType.Text);

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            foreach (string strColumnName in strColumnNames)
                            {
                                row[strColumnName] = row[strColumnName].ToString().Trim();
                            }
                        }
                    }
                    #endregion // Data Prep for values with trailing spaces
                }
                else
                {
                    if (inputs != null)
                    {
                        inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_GADGET_STATUS_PROCESSING);
                    }
                }
            }

            if (inputs != null)
            {
                inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_INSTRUCTIONS_FINISHED_CACHING);
                inputs.UpdateGadgetProgress(0);
            }
        }
    }
}
