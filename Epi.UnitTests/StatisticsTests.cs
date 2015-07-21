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
    public class StatisticsTests
    {
        public DashboardHelper DashboardHelper { get; private set; }
        private DataSet ds;
        public DataTable mainTable;

        public void getData(string filename, string filterstring = null)
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
            db.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=..\\..\\Data;Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
            object selectedDataSource = db;

            IDbDriver dbDriver = (IDbDriver)selectedDataSource;
            DashboardHelper = new DashboardHelper(filename + "#csv", dbDriver);
            IGadgetParameters inputs = new GadgetParameters();
            DashboardHelper.UserVarsNeedUpdating = true;
            string fs = null;
            if (!String.IsNullOrEmpty(filterstring))
            {
                fs = "select * from " + filename + "#csv where " + filterstring;
            }
            PopulateDataSet(inputs, fs);
        }

        private void PopulateDataSet(IGadgetParameters inputs = null, string filterstring = null)
        {
            DataTable unfilteredTable = new DataTable("unfilteredTable");
            unfilteredTable.CaseSensitive = true;

            if (inputs != null)
            {
                inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_GADGET_STATUS_CACHING_WAIT);
            }

            {
//                if (ds == null || ds.Tables.Count == 0 || (ds.Tables[0].Rows.Count == 0 && ds.Tables[0].Columns.Count == 0))
                if (true)
                {
                    if (inputs != null)
                    {
                        inputs.UpdateGadgetStatus(SharedStrings.DASHBOARD_GADGET_STATUS_CACHING);
                    }

                    if (string.IsNullOrEmpty(filterstring))
                    {
                        unfilteredTable = DashboardHelper.Database.GetTableData(DashboardHelper.TableName);
                    }
                    else
                    {
                        Query selectQuery = DashboardHelper.Database.CreateQuery(filterstring);
                        unfilteredTable = DashboardHelper.Database.Select(selectQuery);
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
