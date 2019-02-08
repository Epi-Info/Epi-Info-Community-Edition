using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using Epi.Data;
using EpiInfo.Plugin;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Relate : AnalysisRule
    {
        //  <Relate_Epi_Table_Statement>
        //  ::= RELATE <Qualified ID> <JoinOpt>
        
        //  <Relate_Table_Statement>
        //  ::= RELATE <Qualified ID> <KeyDef> <JoinOpt>
        //  | RELATE File ':' <Qualified ID>  <JoinOpt> 
        //  | RELATE BraceString ':' <Qualified ID>  <JoinOpt>
        //  | RELATE  File ':' <Qualified ID> <KeyDef> <JoinOpt> 
        //  | RELATE  BraceString ':' <Qualified ID> <KeyDef> <JoinOpt>
        
        //  <Relate_Db_Table_Statement>
        //  ::= RELATE <ReadOpt> File <LinkName>  <KeyDef> <JoinOpt>
        
        //  <Relate_Db_Table_With_Identifier_Statement>
        //  ::= RELATE <ReadOpt> File ':' Identifier <LinkName>  <KeyDef> <JoinOpt>
        
        //  <Relate_File_Statement> 		            
        //  ::= RELATE <ReadOpt> File <LinkName>  <KeyDef> <JoinOpt> <FileSpec>
        
        //  <Relate_Excel_File_Statement>
        //  ::= RELATE <ReadOpt> File  ExcelRange <LinkName> <KeyDef> <JoinOpt> <FileSpec>
        
        //  <JoinOpt>
        //  ::= MATCHING
        //  | ALL
        //  | !Null

        private bool _hasRun = false;
        private string _readOpts = string.Empty;
        private string _filePath = string.Empty;
        private string _identifier = string.Empty;
        private string _linkName = string.Empty;
        private string _keyDef = string.Empty;
        private List<string> _joinOpts = new List<string>();

        public Rule_Relate(Rule_Context context, NonterminalToken nonterminal)
            : base(context)
        {
            foreach (Token token in nonterminal.Tokens)
            {
                if (token is NonterminalToken)
                {
                    NonterminalToken nextNonterminal = (NonterminalToken)token;
                    switch (nextNonterminal.Symbol.ToString())
                    {
                        case "<Qualified ID>":
                            _identifier = SetQualifiedId(nextNonterminal);
                            break;
                        case "<KeyDef>":
                        case "<KeyExprIdentifier>":
                            _keyDef = ExtractTokens(nextNonterminal.Tokens).Trim();
                            break;
                        case "<JoinOpt>":
                            _joinOpts.AddRange(ExtractTokens(nextNonterminal.Tokens).ToUpperInvariant().Split(StringLiterals.SPACE.ToCharArray()));
                            break;
                    }
                }
                else
                {
                    TerminalToken nextTerminal = (TerminalToken)token;
                    switch (nextTerminal.Symbol.ToString())
                    {
                        case "BraceString":
                            _filePath = nextTerminal.Text.Trim(new char[] { '{', '}' });
                            break;
                        case "File":
                            _filePath = nextTerminal.Text.Trim(new char[] { '\'', '"' });
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// performs execution of the RELATE command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (false == _hasRun)
            {
                try
                {
                    if (string.IsNullOrEmpty(_filePath))
                    {
                        _filePath = Context.CurrentRead.File;
                    }

                    string[] keySet = _keyDef.Replace(" . ", ".").Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> outputKeys = new List<string>();
                    List<string> relatedKeys = new List<string>();

                    foreach (string key in keySet)
                    {
                        string[] temp = key.Split(new string[] { " :: " }, StringSplitOptions.RemoveEmptyEntries);
                        outputKeys.Add(temp[0].Trim(new char[] { '[', ']' }));
                        relatedKeys.Add(temp[1].Trim(new char[] { '[', ']' }));
                    }

                    Context.GetOutput();
                    DataTable outputTable = Context.DataSet.Tables["output"];
                    DataTable relatedTable = GetDataTable(_filePath, _identifier);

                    Context.ReadDataSource(relatedTable);
                    relatedTable.TableName = "related";

                    DataTable combinedTable = JoinRelatedTables(outputTable, relatedTable, outputKeys, relatedKeys, _joinOpts);

                    Context.DataSet.Tables.Remove("Output");
                    Context.DataSet.Tables.Add(combinedTable);

                    if (Context.DataSet.Tables.Contains("datasource"))
                    {
                        Context.DataSet.Tables.Remove("datasource");
                    }

                    DataTable datasouceTable = new DataTable("datasource");

                    foreach (DataColumn column in combinedTable.Columns)
                    {
                        if (!Context.DataSet.Tables["variables"].Columns.Contains(column.ColumnName))
                        {
                            DataColumn newColumn = new DataColumn(column.ColumnName);
                            newColumn.DataType = column.DataType;
                            datasouceTable.Columns.Add(newColumn);
                        }
                    }

                    Context.DataSet.Tables.Add(datasouceTable);

                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args.Add("COMMANDNAME", CommandNames.RELATE);
                    args.Add("FILENAME", Context.CurrentRead.File);
                    args.Add("TABLENAME", Context.CurrentRead.Identifier);
                    Context.CurrentRead.RelatedTables.Add(_identifier);
                    args.Add("ROWCOUNT", combinedTable.Rows.Count.ToString());

                    Context.AnalysisCheckCodeInterface.Display(args);
                }
                catch (DuplicateNameException exception)
                {
                    throw new Exception(exception.Message);
                }
                catch (GeneralException exception)
                {
                    throw new GeneralException(exception.Message);
                }
                catch
                {
                    throw new Exception(SharedStrings.CANNOT_DETERMINE_RELATIONSHIP);
                }

                _hasRun = true;
            }

            return result;
        }

        private int GetRecordCount(string filePath, string identifier)
        {
            int recordCount = 0;

            if (filePath.ToUpperInvariant().StartsWith("CONFIG:"))
            {
                string[] datakey = filePath.Split(':');
                string ConnectionString = null;
                Configuration config = Configuration.GetNewInstance();
                
                for (int i = 0; i < config.RecentDataSources.Count; i++)
                {
                    Epi.DataSets.Config.RecentDataSourceRow row = config.RecentDataSources[i];
                    if (row.Name.ToUpperInvariant() == datakey[1].ToUpperInvariant())
                    {
                        ConnectionString = Configuration.Decrypt(row.ConnectionString);
                        break;
                    }
                }
                
                filePath = ConnectionString;
            }

            string[] Identifiers = identifier.Split('.');
            StringBuilder IdentifierBuilder = new StringBuilder();

            for (int i = 0; i < Identifiers.Length; i++)
            {
                IdentifierBuilder.Append("[");
                IdentifierBuilder.Append(Identifiers[i]);
                IdentifierBuilder.Append("].");
            }
            
            IdentifierBuilder.Length = IdentifierBuilder.Length - 1;

            if (DBReadExecute.ProjectFileName != "")
            {
                if (Context.CurrentProject.Views.Exists(identifier))
                {
                    recordCount = (int)DBReadExecute.GetScalar(filePath, "SELECT COUNT(*) FROM " + Context.CurrentProject.Views[identifier].TableName);
                }
                else
                {
                    recordCount = (int)DBReadExecute.GetScalar(filePath, "SELECT COUNT(*) FROM " + IdentifierBuilder.ToString());
                }
            }
            else
            {
                recordCount = (int)DBReadExecute.GetScalar(filePath, "SELECT COUNT(*) FROM " + IdentifierBuilder.ToString());
            }

            return recordCount;
        }

        private DataTable GetDataTable(string filePath, string identifier)
        {
            System.Data.DataTable table;

            if (filePath.ToUpperInvariant().StartsWith("CONFIG:"))
            {
                string[] datakey = filePath.Split(':');
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

                filePath = connectionString;
            }

            string[] Identifiers = identifier.Split('.');
            StringBuilder IdentifierBuilder = new StringBuilder();

            for (int i = 0; i < Identifiers.Length; i++)
            {
                IdentifierBuilder.Append("[");
                IdentifierBuilder.Append(Identifiers[i]);
                IdentifierBuilder.Append("].");
            }

            IdentifierBuilder.Length = IdentifierBuilder.Length - 1;

            if (DBReadExecute.ProjectFileName != "")
            {
                if (Context.CurrentProject.Views.Exists(identifier))
                {
                    string selectStatement = "Select * From [" + identifier + "]";
                    table = DBReadExecute.GetDataTable(Context.CurrentRead.File, selectStatement);

                    foreach (Page page in Context.CurrentProject.Views[identifier].Pages)
                    {
                        DataTable pageTable = DBReadExecute.GetDataTable(Context.CurrentRead.File, "Select  * From [" + page.TableName + "]");
                        table = JoinPagesTables(table, pageTable);
                    }
                }
                else
                {
                    if (filePath.EndsWith(".prj"))
                    {
                        Epi.Data.IDbDriver driver = DBReadExecute.GetDataDriver(filePath);
                        table = driver.GetTableData(identifier);
                        DataTable pageTables = DBReadExecute.GetDataTable(driver, "Select DISTINCT PageId FROM metaFields Where DataTableName = '" + identifier + "' AND PageId <> null");

                        foreach (DataRow row in pageTables.Rows)
                        {
                            DataTable pageTable = driver.GetTableData(identifier + row["PageId"]);
                            table = JoinPagesTables(table, pageTable);
                        }
                    }
                    else
                    table = DBReadExecute.GetDataTable(filePath, "Select * FROM " + IdentifierBuilder.ToString());
                }
            }
            else
            {
                if (filePath.EndsWith(".prj"))
                {
                    Epi.Data.IDbDriver driver = DBReadExecute.GetDataDriver(filePath);
                    table = driver.GetTableData(identifier);
                    DataTable pageTables = DBReadExecute.GetDataTable(driver, "Select DISTINCT PageId FROM metaFields Where DataTableName = '" + identifier + "' AND PageId <> null");

                    foreach (DataRow row in pageTables.Rows)
                    {
                        DataTable pageTable = driver.GetTableData(identifier + row["PageId"]);
                        table = JoinPagesTables(table, pageTable);
                    }
                }
                else
                {
                    table = DBReadExecute.GetDataTable(filePath, "Select * from " + IdentifierBuilder.ToString());
                }
            }

            return table;
        }

        public DataTable JoinPagesTables(DataTable recStatusTable, DataTable pageTable)
        {
            DataTable result = new DataTable("Output");

            using (DataSet set = new DataSet())
            {
                set.Tables.AddRange(new DataTable[] { recStatusTable.Copy(), pageTable.Copy() });
                DataColumn parentColumn = set.Tables[0].Columns["GlobalRecordId"];
                DataColumn childColumn = set.Tables[1].Columns["GlobalRecordId"];
                DataRelation dataRelation = new DataRelation(string.Empty, parentColumn, childColumn, false);
                set.Relations.Add(dataRelation);

                for (int i = 0; i < recStatusTable.Columns.Count; i++)
                {
                    result.Columns.Add(recStatusTable.Columns[i].ColumnName, recStatusTable.Columns[i].DataType);
                }

                for (int i = 0; i < pageTable.Columns.Count; i++)
                {
                    bool isDataField = (
                            pageTable.Columns[i].ColumnName.Equals("RecStatus", StringComparison.CurrentCultureIgnoreCase) ||
                            pageTable.Columns[i].ColumnName.Equals("FKEY", StringComparison.CurrentCultureIgnoreCase) ||
                            pageTable.Columns[i].ColumnName.Equals("GlobalRecordId", StringComparison.CurrentCultureIgnoreCase
                            )) == false;
                    
                    if ( isDataField )
                    {
                        if (result.Columns.Contains(pageTable.Columns[i].ColumnName) == false)
                        {
                            result.Columns.Add(pageTable.Columns[i].ColumnName, pageTable.Columns[i].DataType);
                        }
                        else
                        {
                            int count = 0;
                            foreach (DataColumn column in result.Columns)
                            {
                                if (column.ColumnName.StartsWith(pageTable.Columns[i].ColumnName))
                                {
                                    count++;
                                }
                            }
                            
                            result.Columns.Add(pageTable.Columns[i].ColumnName + count.ToString(), recStatusTable.Columns[i].DataType);
                        }
                    }
                }

                foreach (DataRow parentRow in set.Tables[0].Rows)
                {
                    DataRow resultRow = result.NewRow();
                    DataRow[] childRow = parentRow.GetChildRows(dataRelation);

                    if (childRow != null && childRow.Length > 0)
                    {
                        foreach (DataColumn dataColumn in pageTable.Columns)
                        {
                            resultRow[dataColumn.ColumnName] = childRow[0][dataColumn.ColumnName];
                        }

                        foreach (DataColumn dataColumn in recStatusTable.Columns)
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

        private Type ReturnWideDataType(object objOne, object objTwo)
        {
            Type returnType = objOne.GetType();
            return returnType;
        }

        private static Type ResolveType(string typeString)
        {
            int commaIndex = typeString.IndexOf(",");
            string className = typeString.Substring(0, commaIndex).Trim();
            string assemblyName = typeString.Substring(commaIndex + 1).Trim();

            System.Reflection.Assembly assembly = null;

            try
            {
                assembly = System.Reflection.Assembly.Load(assemblyName);
            }
            catch
            {
                try
                {
                    assembly = System.Reflection.Assembly.LoadWithPartialName(assemblyName);
                }
                catch
                {
                    throw new ArgumentException("Can't load assembly " + assemblyName);
                }
            }

            return assembly.GetType(className, false, false);
        }

        private bool ColumnDataTypesMatch(DataTable parentTable, DataTable childTable, List<string> parentKeys, List<string> childKeys)
        {
            bool[] dataColumTypesSame = new bool[parentKeys.Count];
            List<string> nameDashType = new List<string>();

            using (DataSet ds = new DataSet())
            {
                ds.Tables.AddRange(new DataTable[] { parentTable.Copy(), childTable.Copy() });

                DataColumn[] parentColumns = new DataColumn[parentKeys.Count];
                DataColumn[] childColumns = new DataColumn[childKeys.Count];

                for (int i = 0; i < parentColumns.Length; i++)
                {
                    if (false == parentTable.Columns.Contains(parentKeys[i]))
                    {
                        throw new GeneralException(string.Format("The table does not contain the column ({0}).", parentKeys[i]));
                    }
                    
                    parentColumns[i] = ds.Tables[0].Columns[parentKeys[i]];
                    childColumns[i] = ds.Tables[1].Columns[childKeys[i]];

                    dataColumTypesSame[i] = ((DataColumn)parentColumns[i]).DataType == ((DataColumn)childColumns[i]).DataType;

                    nameDashType.Add(string.Format("{0} is a(n) {1}", parentColumns[i], ((DataColumn)parentColumns[i]).DataType.Name));
                }

                for (int i = 0; i < dataColumTypesSame.Length; i++)
                {
                    if (dataColumTypesSame[i] == false)
                    {
                        string items = string.Empty;
                        foreach (string item in nameDashType)
                        {
                            items += item + Environment.NewLine;
                        }

                        string message = string.Format("The data types of the columns selected in the relate command are too dissimilar to join with.{0}{0}{1}", Environment.NewLine, items);
                        throw new GeneralException(message);
                    }
                }

                return true;
            }
        }

        protected DataTable JoinRelatedTables(DataTable parentTable, DataTable childTable, List<string> parentKeys, List<string> childKeys, List<string> options)
        {
            DataTable result = new DataTable("Output");

            using (DataSet dataSet = new DataSet())
            {
                dataSet.Tables.AddRange(new DataTable[] { parentTable.Copy(), childTable.Copy() });
                DataColumn[] parentColumns = new DataColumn[parentKeys.Count];
                DataColumn[] childColumns = new DataColumn[childKeys.Count];

                for (int i = 0; i < parentColumns.Length; i++)
                {
                    parentColumns[i] = dataSet.Tables[0].Columns[parentKeys[i]];
                }

                for (int i = 0; i < childColumns.Length; i++)
                {
                    childColumns[i] = dataSet.Tables[1].Columns[childKeys[i]];
                }

                if (false == ColumnDataTypesMatch( parentTable, childTable, parentKeys, childKeys))
                {

                }

                DataRelation relation = new DataRelation(string.Empty, parentColumns, childColumns, false);
                dataSet.Relations.Add(relation);

                for (int i = 0; i < parentTable.Columns.Count; i++)
                {
                    bool isDataField = (
                            parentTable.Columns[i].ColumnName.Equals("FirstSaveLogonName", StringComparison.CurrentCultureIgnoreCase) ||
                            parentTable.Columns[i].ColumnName.Equals("FirstSaveTime", StringComparison.CurrentCultureIgnoreCase) ||
                            parentTable.Columns[i].ColumnName.Equals("LastSaveLogonName", StringComparison.CurrentCultureIgnoreCase) ||
                            parentTable.Columns[i].ColumnName.Equals("LastSaveTime", StringComparison.CurrentCultureIgnoreCase) ||
                            parentTable.Columns[i].ColumnName.Equals("RecStatus", StringComparison.CurrentCultureIgnoreCase) ||
                            parentTable.Columns[i].ColumnName.Equals("FKEY", StringComparison.CurrentCultureIgnoreCase) ||
                            parentTable.Columns[i].ColumnName.Equals("GlobalRecordId", StringComparison.CurrentCultureIgnoreCase
                            )) == false;

                    if (isDataField)
                    {
                        result.Columns.Add(parentTable.Columns[i].ColumnName, parentTable.Columns[i].DataType);
                    }
                    else
                    {
                        if (Context.CurrentRead.RelatedTables.Count == 0)
                        {
                            result.Columns.Add(Context.CurrentRead.Identifier + "_" + parentTable.Columns[i].ColumnName, parentTable.Columns[i].DataType);
                        }
                        else
                        {
                            result.Columns.Add(Context.CurrentRead.RelatedTables[Context.CurrentRead.RelatedTables.Count - 1] + "_" + parentTable.Columns[i].ColumnName, parentTable.Columns[i].DataType);
                        }
                    }
                }

                for (int i = 0; i < childTable.Columns.Count; i++)
                {
                    bool isDataField = (
                            childTable.Columns[i].ColumnName.Equals("FirstSaveLogonName", StringComparison.CurrentCultureIgnoreCase) ||
                            childTable.Columns[i].ColumnName.Equals("FirstSaveTime", StringComparison.CurrentCultureIgnoreCase) ||
                            childTable.Columns[i].ColumnName.Equals("LastSaveLogonName", StringComparison.CurrentCultureIgnoreCase) ||
                            childTable.Columns[i].ColumnName.Equals("LastSaveTime", StringComparison.CurrentCultureIgnoreCase) ||
                            childTable.Columns[i].ColumnName.Equals("RecStatus", StringComparison.CurrentCultureIgnoreCase) ||
                            childTable.Columns[i].ColumnName.Equals("FKEY", StringComparison.CurrentCultureIgnoreCase) ||
                            childTable.Columns[i].ColumnName.Equals("GlobalRecordId", StringComparison.CurrentCultureIgnoreCase
                            )) == false;

                    if (isDataField)
                    {
                        if (!result.Columns.Contains(childTable.Columns[i].ColumnName))
                        {
                            result.Columns.Add(childTable.Columns[i].ColumnName, childTable.Columns[i].DataType);
                        }
                        else
                        {
                            int count = 1;
                            foreach (DataColumn column in result.Columns)
                            {
                                if (column.ColumnName.StartsWith(childTable.Columns[i].ColumnName))
                                {
                                    count++;
                                }

                                if (false == result.Columns.Contains(childTable.Columns[i].ColumnName + count.ToString()))
                                {
                                    break;
                                }
                            }
                            result.Columns.Add(childTable.Columns[i].ColumnName + count.ToString(), childTable.Columns[i].DataType);
                        }
                    }
                    else
                    {
                        result.Columns.Add(_identifier + "_" + childTable.Columns[i].ColumnName, childTable.Columns[i].DataType);
                    }
                }

                result.BeginLoadData();

                foreach (DataRow parentRow in dataSet.Tables[0].Rows)
                {
                    DataRow[] childRows = parentRow.GetChildRows(relation);

                    if (childRows != null && childRows.Length > 0)
                    {
                        object[] parentArray = parentRow.ItemArray;
                        foreach (DataRow childRow in childRows)
                        {
                            object[] childArray = childRow.ItemArray;
                            object[] joinArray = new object[parentArray.Length + childArray.Length];
                            Array.Copy(parentArray, 0, joinArray, 0, parentArray.Length);
                            Array.Copy(childArray, 0, joinArray, parentArray.Length, childArray.Length);
                            result.LoadDataRow(joinArray, true);
                        }
                    }
                    else if (options.Contains("ALL"))
                    {
                        object[] parentArray = parentRow.ItemArray;
                        object[] joinArray = new object[parentArray.Length];
                        Array.Copy(parentArray, 0, joinArray, 0, parentArray.Length);
                        result.LoadDataRow(joinArray, true);
                    }
                }

                result.EndLoadData();
            }

            return result;
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
    }
}

