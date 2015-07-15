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
            {
                string configFilePath = "C:\\EpiInfoMain\\Build\\Debug\\Configuration\\EpiInfo.Config.xml";
                Epi.DataSets.Config configDataSet = new Epi.DataSets.Config();
                configDataSet.ReadXml(configFilePath);
                Epi.Configuration.Load(configFilePath);
                System.Data.Common.DbConnectionStringBuilder dbCnnStringBuilder = new System.Data.Common.DbConnectionStringBuilder();
                string provider = "Epi.Data.Office.CsvFileFactory, Epi.Data.Office";
                IDbDriverFactory dbFactory = null;
//                Type myType = typeof(Epi.Data.Office.CsvFileFactory);
                dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(provider);
                IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);
                string savedConnectionStringDescription = "CSV File: C:\\Users\\zfj4";
                db.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\Users\\zfj4;Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
                object selectedDataSource = db;

                string name = "";
                string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\Users\\zfj4;Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
                string dataProvider = "Epi.Data.Office.CsvFileFactory, Epi.Data.Office";

                IDbDriver dbDriver = (IDbDriver)selectedDataSource;
                DashboardHelper = new DashboardHelper("EColiFoodHistory#csv", dbDriver);
                IGadgetParameters inputs = new GadgetParameters();
                DashboardHelper.UserVarsNeedUpdating = true;
                PopulateDataSet(inputs);
            }
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
        }

        public void PopulateDataSet(IGadgetParameters inputs = null)
        {
            DataTable unfilteredTable = new DataTable("unfilteredTable");
            unfilteredTable.CaseSensitive = true;

            if (inputs != null)
            {
                inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_GADGET_STATUS_CACHING_WAIT);
            }

//            lock (syncLock)
            {
                if (ds == null || ds.Tables.Count == 0 || (ds.Tables[0].Rows.Count == 0 && ds.Tables[0].Columns.Count == 0))
                {
//                    stopwatch.Start();

                    if (inputs != null)
                    {
                        inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_GADGET_STATUS_CACHING);
                    }

//                    else
                    {
//                        if (string.IsNullOrEmpty(this.CustomQuery))
                        {
                            unfilteredTable = DashboardHelper.Database.GetTableData(DashboardHelper.TableName);
                        }
//                        else
//                       {
//                           Query selectQuery = Database.CreateQuery(this.CustomQuery);
//                            unfilteredTable = Database.Select(selectQuery);
//                       }
                    }

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

//                            RelateInto(unfilteredTable, relatedTable, conn.ParentKeyField, conn.ChildKeyField, conn.UseUnmatched, inputs);
                        }
                    }

                    DashboardHelper.AddSystemVariablesToTable(unfilteredTable);
                    DashboardHelper.AddPermanentVariablesToTable(unfilteredTable);
                    ds = new DataSet();
                    ds.Tables.Add(unfilteredTable); //ds.Tables.Add(ConvertTableColumns(unfilteredTable));
                    ds.Tables[0].CaseSensitive = true;
                    mainTable = ds.Tables[0];
//                    stopwatch.Stop();

//                    ConstructTableColumnNames();

//                    if (UserVarsNeedUpdating)
//                    {
//                        ApplyDashboardRules(inputs);
//                    }

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

//                    TimeToCache = stopwatch.Elapsed.ToString();
//                    DataView tempView = new DataView(ds.Tables[0], GenerateFilterCriteria(), string.Empty, DataViewRowState.CurrentRows);
//                    RecordCount = tempView.Count;
                }
                else
                {
                    if (inputs != null)
                    {
                        inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_GADGET_STATUS_PROCESSING);
                    }
                }

                if (DashboardHelper.UserVarsNeedUpdating)
                {
//                    ApplyDashboardRules(inputs);
                }

//                LastCacheTime = DateTime.Now;
            }

            if (inputs != null)
            {
                inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_INSTRUCTIONS_FINISHED_CACHING);
                inputs.UpdateGadgetProgress(0);
            }
        }
    }
}
