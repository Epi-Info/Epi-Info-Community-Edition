using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using com.calitha.goldparser;
using Epi.Data;
using System.Data;
using System.Linq;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_Read : AnalysisRule
    {
        private string _readOptions = null;
        private string _linkName = null;
        private bool _isSimpleRead = false;

        public static bool RemoveVariables = true;
        public string File = null;
        public string Identifier = null;
        public List<string> RelatedTables = new List<string>();
        public bool IsEpi7ProjectRead = false;

        public Rule_Read(Rule_Context context, NonterminalToken nonterminal)
            : base(context)
        {
            switch (nonterminal.Rule.Lhs.ToString())
            {
                case "<Simple_Read_Statement>":
                    _isSimpleRead = true;
                    _readOptions = GetCommandElement(nonterminal.Tokens, 1);
                    if (nonterminal.Tokens[2] is NonterminalToken)
                    {
                        Identifier = SetQualifiedId(nonterminal.Tokens[2]);
                    }
                    else
                    {
                        Identifier = GetCommandElement(nonterminal.Tokens, 2).Trim(new char[] { '[', ']' });
                    }
                    break;
                case "<Read_Sql_Statement>":
                    if (nonterminal.Tokens.Length > 4)
                    {
                        File = GetCommandElement(nonterminal.Tokens, 2).Trim('"');
                        Identifier = SetQualifiedId(nonterminal.Tokens[4]);
                    }
                    else
                    {   File = GetCommandElement(nonterminal.Tokens, 1).Trim(new char[] { '{', '}' });

                        if (nonterminal.Tokens[3] is NonterminalToken)
                        {
                            Identifier = SetQualifiedId(nonterminal.Tokens[3]);
                        }
                        else
                        {
                            Identifier = GetCommandElement(nonterminal.Tokens, 3).Trim(new char[] { '[', ']' });
                        }
                    }
                    break;
                default:
                    File = GetCommandElement(nonterminal.Tokens, 1);
                    if (nonterminal.Tokens[3] is NonterminalToken)
                    {
                        Identifier = SetQualifiedId(nonterminal.Tokens[3]);
                    }
                    else
                    {
                        Identifier = GetCommandElement(nonterminal.Tokens, 3).Trim(new char[] { '[', ']' });
                    }
                    if (nonterminal.Tokens.Length > 4)
                    {
                        _linkName = GetCommandElement(nonterminal.Tokens, 4);
                    }
                    break;
            }
        }

        /// <summary>
        /// performs execution of the READ command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            DataTable outputTable;
            int recordCount = 0;

            Context.isReadMode = true;
            Context.GroupVariableList = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            if (File.ToUpperInvariant().StartsWith("CONFIG:"))
            {
                string[] datakey = File.Split(':');
                string connectionString = null;
                Configuration config = Configuration.GetNewInstance();

                for (int i = 0; i < config.RecentDataSources.Count; i++)
                {
                    Epi.DataSets.Config.RecentDataSourceRow row = config.RecentDataSources[i];
                    
                    if (row.Name.ToUpperInvariant() == datakey[1].ToUpperInvariant())
                    {
                        connectionString = Configuration.Decrypt(row.ConnectionString);
                        break;
                    }
                }

                File = connectionString;
            }

            if (_isSimpleRead)
            {
                if (Context.CurrentProject == null)
                {
                    if (Context.CurrentRead == null)
                    {
                        throw new GeneralException(SharedStrings.NO_CURRENT_PROJECT);
                    }
                    else
                    {
                        Context.AddConnection("_DB", Context.CurrentRead.File);
                        outputTable = DBReadExecute.GetDataTable(Context.CurrentRead.File, "Select TOP 2 * from " + Identifier);
                        recordCount = (int)DBReadExecute.GetScalar(Context.CurrentRead.File, "SELECT COUNT(*) FROM " + Identifier);
                        File = Context.CurrentRead.File;
                        IsEpi7ProjectRead = Context.CurrentRead.IsEpi7ProjectRead;
                        Context.CurrentRead = this;
                    }
                }
                else
                {
                    Context.AddConnection("_DB", Context.CurrentProject.CollectedDataConnectionString);
                    outputTable = DBReadExecute.GetDataTable(Context.CurrentProject.CollectedDataConnectionString, "Select TOP 2 * from " + Context.CurrentProject.views[Identifier].FromViewSQL);
                    recordCount = (int)DBReadExecute.GetScalar(Context.CurrentProject.CollectedDataConnectionString, "SELECT COUNT(*) FROM " + Identifier);
                    File = Context.CurrentProject.FilePath;
                    Context.CurrentRead = this;
                    IsEpi7ProjectRead = true;
                }
            }
            else
            {
                try
                {
                    Context.AddConnection("_DB", File);
                }
                catch (System.IO.DirectoryNotFoundException dnfex)
                {
                    Configuration config0 = Configuration.GetNewInstance();
                    string wd = config0.Directories.Working;
                    File = wd + "\\" + File;
                    Context.AddConnection("_DB", File);
                }
                Context.CurrentRead = this;

                string[] identifiers = Identifier.Split('.');
                StringBuilder identifierBuilder = new StringBuilder();

                for (int i = 0; i < identifiers.Length; i++)
                {
                    identifierBuilder.Append("[");
                    identifierBuilder.Append(identifiers[i]);
                    identifierBuilder.Append("].");
                }
                
                identifierBuilder.Length = identifierBuilder.Length - 1;

                if (DBReadExecute.ProjectFileName != "")
                {
                    IsEpi7ProjectRead = true;
                    Context.CurrentProject = new Project(DBReadExecute.ProjectFileName);

                    if (Context.CurrentProject.Views.Exists(Identifier))
                    {
                        string tableName = Context.CurrentProject.Views[Identifier].TableName;
                        DataSets.Config.DataDriverDataTable dataDrivers = Configuration.GetNewInstance().DataDrivers;

                        if (!DBReadExecute.CheckDatabaseTableExistance(DBReadExecute.ParseConnectionString(File), tableName, true))
                        {
                            throw new GeneralException(string.Format("The Datatable for [{0}] does NOT exist.\nPlease be sure the Datatable exists before trying to READ.", Identifier));
                        }
                        else
                        {
                            string query = GetEpi7ProjectRecordCountQuery(tableName);
                            recordCount = (int)DBReadExecute.GetScalar(File, query);
                            outputTable = DBReadExecute.GetDataTable(File, "Select TOP 2 * From [" + Context.CurrentProject.Views[Identifier].TableName + "]");

                            foreach (Page page in Context.CurrentProject.Views[Identifier].Pages)
                            {
                                outputTable = JoinTables(outputTable, DBReadExecute.GetDataTable(File, "Select TOP 2 * From [" + page.TableName + "]"));
                            }
                        }
                    }
                    else
                    {
                        outputTable = DBReadExecute.GetDataTable(File, "Select TOP 2 * FROM " + identifierBuilder.ToString());
                        recordCount = (int)DBReadExecute.GetScalar(File, "SELECT COUNT(*) FROM " + identifierBuilder.ToString());
                    }
                }
                else if (Identifier.ToLowerInvariant().EndsWith(".json") || File.Contains("FMT=JSON"))
                {
                    string[] jsonsplit = File.Split('=');
                    string jsonpath = "";
                    if (jsonsplit.Length > 2)
                        jsonpath = jsonsplit[2].Split(';')[0];
                    else
                        jsonpath = jsonsplit[0];
                    if (jsonpath.ToCharArray()[0] == '"')
                    {
                        int jlength = jsonpath.Length;
                        if (jsonpath.ToCharArray()[jlength - 1] == '"')
                        {
                            jsonpath = jsonpath.Substring(1, jlength - 2);
                        }
                    }
                    if (!System.IO.Directory.Exists(jsonpath))
                    {
                        Configuration config0 = Configuration.GetNewInstance();
                        string wd = config0.Directories.Working;
                        jsonpath = Path.Combine(wd, jsonpath);
                    }
                    string[] separator = new string[] { "|json|" };
                    string[] tableNames = Identifier.Split(separator, StringSplitOptions.None);
                    string jsonstring = System.IO.File.ReadAllText(jsonpath + "\\" + tableNames[0]);
                    if (String.IsNullOrEmpty(jsonstring))
                    {
                        // Epi.Windows.MsgBox.ShowInformation("File " + tableNames[0] + " is empty.");
                    }
                    else if (tableNames.Length > 1)
                    {
                        if (jsonstring.First<char>() == '[' && jsonstring.Last<char>() == ']')
                            jsonstring = jsonstring.Substring(1, jsonstring.Length - 2);
                    }
                    for (int jsoni = 1; jsoni < tableNames.Length; jsoni++)
                    {
                        string morejsonstring = System.IO.File.ReadAllText(jsonpath + "\\" + tableNames[jsoni]);
                        if (String.IsNullOrEmpty(morejsonstring))
                        {
                            // Epi.Windows.MsgBox.ShowInformation("File " + tableNames[jsoni] + " is empty.");
                            continue;
                        }
                        if (morejsonstring.First<char>() == '[' && morejsonstring.Last<char>() == ']')
                            morejsonstring = morejsonstring.Substring(1, morejsonstring.Length - 2);
                        if (!String.IsNullOrEmpty(jsonstring))
                            jsonstring += ",";
                        jsonstring = jsonstring + morejsonstring;
                    }
                    // DataTable dt = JSONtoDataTable(jsonstring);
                    if (String.IsNullOrEmpty(jsonstring))
                    {
                        jsonstring = "[]";
                    }
                    if (jsonstring.First<char>() != '[' && jsonstring.Last<char>() != ']')
                        jsonstring = "[" + jsonstring + "]";
                    outputTable = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(jsonstring);
                    recordCount = outputTable.Rows.Count;
                }
                else
                {
                    String identifier = identifierBuilder.ToString();
                    outputTable = DBReadExecute.GetDataTable(File, "Select TOP 2 * from " + identifier);
                    recordCount = int.Parse(DBReadExecute.GetScalar(File, "SELECT COUNT(*) FROM " + identifier).ToString());
                }
            }

            outputTable.TableName = "Output";

            Context.DataTableRefreshNeeded = true;

            if (Context.DataSet.Tables.Contains("Output"))
            {
                Context.DataSet.Tables.Remove("Output");
            }

            Context.DataSet.Tables.Add(outputTable);

            if (Context.DataSet.Tables.Contains("datasource"))
            {
                Context.DataSet.Tables.Remove("datasource");
            }

            DataTable datasouceTable = new DataTable("datasource");

            foreach (DataColumn column in outputTable.Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                newColumn.DataType = column.DataType;
                datasouceTable.Columns.Add(newColumn);
            }

            Context.DataSet.Tables.Add(datasouceTable);
            Context.MemoryRegion.RemoveVariablesInScope(VariableType.DataSource);
            Context.ReadDataSource(datasouceTable);
            Context.GroupVariableList = new Dictionary<string, List<string>>();

            if (Rule_Read.RemoveVariables)
            {
                if (Context.DataSet.Tables.Contains("variables"))
                {
                    Context.DataSet.Tables.Remove("variables");
                    Context.MemoryRegion.RemoveVariablesInScope(VariableType.Standard);
                }

                if (Context.SelectExpression != null)
                {
                    Context.SelectExpression.Clear();
                }
                if (Context.SubroutineList != null)
                {
                    Context.SubroutineList.Clear();
                }
                if (Context.Subroutine != null)
                {
                    Context.Subroutine.Clear();
                }

                Context.SelectString.Length = 0;
                Context.SortExpression.Length = 0;              
            }
            else
            {
                Rule_Read.RemoveVariables = true;
            }

            Context.SyncVariableAndOutputTable();
            Context.ClearParticipatingVariableList();

            Context.GetOutput();

			if (Context.ConnectionList["_DB"].ToString().Contains("REDCap"))
				recordCount = Context.GetOutput().Count;

            result = string.Format("number of records read {0}", recordCount);
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", CommandNames.READ);
            args.Add("FILENAME", File);
            args.Add("TABLENAME", Identifier);
            args.Add("ROWCOUNT", recordCount.ToString());

            Project currentProject = Context.CurrentProject;

            if (string.IsNullOrEmpty(File) && currentProject == null)
            {
                throw new GeneralException(SharedStrings.NO_CURRENT_PROJECT);
            }

            Context.AnalysisCheckCodeInterface.Display(args);
            return result;
        }

        static public string GetEpi7ProjectViewSQL(View view)
        {
            StringBuilder result = new StringBuilder("SELECT ");

            if (view.IsRelatedView)
            {
                result.Append("t.GlobalRecordId As GlobalRecordId, t.UniqueKey As UniqueKey, t.RecStatus As RecStatus, t.FKEY As FKEY, ");
            }
            else
            {
                result.Append("t.GlobalRecordId As GlobalRecordId, t.UniqueKey As UniqueKey, t.RecStatus As RecStatus, ");
            }

            foreach (Epi.Page page in view.Pages)
            {
                foreach (Epi.Fields.Field field in page.Fields)
                {
                    if (field is Epi.Fields.IDataField)
                    {
                        result.Append("[");
                        result.Append(page.TableName);
                        result.Append("]");
                        result.Append(".[");
                        result.Append(field.Name);
                        result.Append("] As [");
                        result.Append(field.Name);
                        result.Append("], ");
                    }
                }
            }

            result.Length = result.Length - 2;
            result.Append(" ");
            result.Append(view.FromViewSQL);

            return result.ToString();
        }

        static public string GetEpi7ProjectTop2ViewSQL(View view)
        {
            StringBuilder result = new StringBuilder("SELECT Top 2 ");

            if (view.IsRelatedView)
            {
                result.Append("t.GlobalRecordId As GlobalRecordId, t.UniqueKey As UniqueKey, t.RecStatus As RecStatus, t.FKEY As FKEY, ");
            }
            else
            {
                result.Append("t.GlobalRecordId As GlobalRecordId, t.UniqueKey As UniqueKey, t.RecStatus As RecStatus, ");
            }

            foreach (Epi.Page page in view.Pages)
            {
                foreach (Epi.Fields.Field field in page.Fields)
                {
                    if (field is Epi.Fields.IDataField)
                    {
                        result.Append("[");
                        result.Append(page.TableName);
                        result.Append("]");
                        result.Append(".[");
                        result.Append(field.Name);
                        result.Append("] As [");
                        result.Append(field.Name);
                        result.Append("], ");
                    }
                }
            }

            result.Length = result.Length - 2;
            result.Append(" ");
            result.Append(view.FromViewSQL);

            return result.ToString();
        }

        private string GetEpi7ProjectRecordCountQuery(string indentifier)
        {
            Configuration config = Configuration.GetNewInstance();
            StringBuilder result = new StringBuilder("SELECT COUNT(*) FROM ");

            result.Append(indentifier);
            switch(config.Settings.RecordProcessingScope)
            {
                case (int)RecordProcessingScope.Undeleted: 
                    result.Append(" Where RecStatus = 1");
                    break;
                case (int)RecordProcessingScope.Deleted: 
                    result.Append(" Where RecStatus = 0");
                    break;
                case 3: 
                default:
                    break;
            }

            return result.ToString();
        }

        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Context.AnalysisCheckCodeInterface.ReportIndeterminateTaskEnded();
        }

        void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Context.AnalysisCheckCodeInterface.ReportIndeterminateTaskStarted("Buffering data...");
            currentDataTable = Context.GetOutput();
        }

        private List<DataRow> currentDataTable;

        public DataTable JoinTables(DataTable parentTable, DataTable childTable)
        {
            DataTable result = new DataTable("Output");

            using (DataSet dataset = new DataSet())
            {
                dataset.Tables.AddRange(new DataTable[] { parentTable.Copy(), childTable.Copy() });
                DataColumn parentColumn = dataset.Tables[0].Columns["GlobalRecordId"];
                DataColumn childColumn = dataset.Tables[1].Columns["GlobalRecordId"];
                DataRelation dataRelation = new DataRelation(string.Empty, parentColumn, childColumn, false);
                dataset.Relations.Add(dataRelation);

                for (int i = 0; i < parentTable.Columns.Count; i++)
                {
                    result.Columns.Add(parentTable.Columns[i].ColumnName, parentTable.Columns[i].DataType);
                }

                for (int i = 0; i < childTable.Columns.Count; i++)
                {
                    if (false == (childTable.Columns[i].ColumnName.Equals("RecStatus", StringComparison.CurrentCultureIgnoreCase) || childTable.Columns[i].ColumnName.Equals("FKey", StringComparison.CurrentCultureIgnoreCase) || childTable.Columns[i].ColumnName.Equals("GlobalRecordId", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        if (!result.Columns.Contains(childTable.Columns[i].ColumnName))
                        {
                            result.Columns.Add(childTable.Columns[i].ColumnName, childTable.Columns[i].DataType);
                        }
                        else
                        {
                            int count = 0;
                            foreach (DataColumn column in result.Columns)
                            {
                                if (column.ColumnName.StartsWith(childTable.Columns[i].ColumnName))
                                {
                                    count++;
                                }
                            }
                            result.Columns.Add(childTable.Columns[i].ColumnName + count.ToString(), childTable.Columns[i].DataType);
                        }
                    }
                }

                foreach (DataRow parentRow in dataset.Tables[0].Rows)
                {
                    DataRow resultRow = result.NewRow();
                    DataRow[] childRow = parentRow.GetChildRows(dataRelation);

                    if (childRow != null && childRow.Length > 0)
                    {
                        foreach (DataColumn dataColumn in childTable.Columns)
                        {
                            resultRow[dataColumn.ColumnName] = childRow[0][dataColumn.ColumnName];
                        }

                        foreach (DataColumn dataColumn in parentTable.Columns)
                        {
                            resultRow[dataColumn.ColumnName] = parentRow[dataColumn.ColumnName];
                        }

                        result.Rows.Add(resultRow);
                    }
                }
                result.AcceptChanges();
            }

            return result;
        }
    }
}
