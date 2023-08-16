using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.calitha.goldparser;
using Epi.Data;
using System.Data;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Write : AnalysisRule
    {
        string WriteMode = null;
        string FileDataFormat = null;
        string OutTarget = null;
        string[] IdentifierList = null;
        //AnalysisRule IdentifierListRule = null;
        bool isExceptionList = false;
        bool isConnectionString = false;
        protected IDbDriver db;
        protected IDbDriverFactory dbFactory;

        private string statusMessage;
        private double progress;

        private string FilePath = null;
        private string TableName;
        private List<string> VariableList = new List<string>();
        private List<string> TempVariableList = new List<string>();

        Dictionary<string, string> args = new Dictionary<string, string>();

        private const int Max_Number_Columns = 250;
        private IDbDriver OutputDriver = null;
        private DataTable CurrentDataTable = null;
        private string curFile;
        private int asciiRecordCount;

        private string jsonstring;

        delegate void WriteMethodDelegate(List<string> pListFields, IDbDriver pOutput);

        public Rule_Write(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // <Write_All_Statement> 	::= WRITE <WriteMode> <FileDataFormat> <OutTarget> '*'
            // <Write_Some_Statement> 	::= WRITE <WriteMode> <FileDataFormat> <OutTarget> <IdentifierList>
            // <Write_Except_Statement> ::= WRITE <WriteMode> <FileDataFormat> <OutTarget> '*' EXCEPT <IdentifierList>
            // <WriteMode> 				::= APPEND| REPLACE	| !Null
            // <FileDataFormat>         ::= '"Epi7"' 
            //                          | '"Epi2000"' 
            //                          | '"Epi 2000"' 
            //                          | '"Excel 8.0"' 
            //                          | '"Text"'
            // <OutTarget>              ::= Identifier | File ':' Identifier | File | BraceString ':' Identifier

            this.WriteMode = this.GetCommandElement(pToken.Tokens, 1);
            this.FileDataFormat = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '"' });

            NonterminalToken T = (NonterminalToken) pToken.Tokens[3];
            GetOutTarget(pToken.Tokens[3]);

            /*
            this.OutTarget = this.GetCommandElement(pToken.Tokens, 3).Trim(new char[] { '{','}' });

            if (((TerminalToken)T.Tokens[0]).Symbol.ToString() == "BraceString")
            {
                this.OutTarget = this.OutTarget.Replace("}","");
                isConnectionString = true;
            }*/

            if (pToken.Tokens.Length <= 5)
            {
                //this.IdentifierList = this.GetCommandElement(pToken.Tokens, 4).Replace(" . ", ".").Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                //this.IdentifierListRule = AnalysisRule.BuildStatments(pContext, pToken.Tokens[4]);
                this.IdentifierList = AnalysisRule.SpliIdentifierList(this.GetCommandElement(pToken.Tokens, 4));
            }
            else
            {
                //this.IdentifierList = this.GetCommandElement(pToken.Tokens, 6).Replace(" . ", ".").Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                //this.IdentifierListRule = AnalysisRule.BuildStatments(pContext, pToken.Tokens[6]);
                this.IdentifierList = AnalysisRule.SpliIdentifierList(this.GetCommandElement(pToken.Tokens, 6));
                isExceptionList = true;
            }
        }

        private GenericDbColumnType ConvertToGenericType(Type dataType)
        {
            //dataType.GetType();
            //return GenericDbColumnType.Unknown;
            switch (dataType.Name)
            {
                case "Int16":
                    return GenericDbColumnType.Int16;

                case "Int32":
                    return GenericDbColumnType.Int32;

                case "Int64":
                    return GenericDbColumnType.Int64;

                case "String":
                    return GenericDbColumnType.String;

                case "Byte":
                    return GenericDbColumnType.Byte;

                case "Boolean":
                    return GenericDbColumnType.Boolean;

                case "Decimal":
                    return GenericDbColumnType.Decimal;

                case "Double":
                    return GenericDbColumnType.Double;

                case "DateTime":
                    return GenericDbColumnType.DateTime;

                case "Guid":
                    return GenericDbColumnType.String;

                case "UInt16":
                    return GenericDbColumnType.UInt16;

                case "UInt32":
                    return GenericDbColumnType.UInt32;

                case "UInt64":
                    return GenericDbColumnType.UInt64;

                case "Single":
                    return GenericDbColumnType.Single;

                case "SByte":
                    return GenericDbColumnType.SByte;

                default:
                    return GenericDbColumnType.Object;


                //    case DataType.Boolean:
                //        return GenericDbColumnType.Boolean;

                //    case DataType.Date:
                //        return GenericDbColumnType.Date;

                //    case DataType.DateTime:
                //        return GenericDbColumnType.DateTime;

                //    case DataType.GUID:
                //        return GenericDbColumnType.Guid;

                //    case DataType.Number:
                //        return GenericDbColumnType.Double;

                //    case DataType.Object:
                //        return GenericDbColumnType.Object;

                //    case DataType.PhoneNumber:
                //        return GenericDbColumnType.String;

                //    case DataType.Text:
                //        return GenericDbColumnType.String;

                //    case DataType.Time:
                //        return GenericDbColumnType.Time;

                //    case DataType.Unknown:
                //        return GenericDbColumnType.Unknown;

                //    case DataType.YesNo:
                //        return GenericDbColumnType.Boolean;

                //    default:
                //        return GenericDbColumnType.Unknown;
            }
        }

        /// <summary>
        /// performs execution of the WRITE command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            Context.AnalysisCheckCodeInterface.ShowWaitDialog("Exporting data...");

            //string[] tmp = this.OutTarget.ToString().Split(':');
            //string FilePath = null;
            //if (tmp.Length <= 2)
            //{
            //    FilePath = tmp[0];
            //}
            //else
            //{
            //    FilePath = tmp[0] + ":" + tmp[1];
            //}
            //FilePath = FilePath.Trim().Trim(new char[] { '\'' });
            //string TableName;
            //if (tmp.Length > 1)
            //{
            //    TableName = tmp[tmp.Length - 1].Replace("]", "").Replace("[", "").Trim().Trim('\'');
            //}
            //else
            //{
            //    TableName = this.OutTarget;
            //    FilePath = this.Context.CurrentProject.CollectedDataConnectionString;
            //}

            CurrentDataTable = this.Context.DataSet.Tables["output"].Clone();

            foreach (DataRow row in this.Context.GetOutput(new List<string>()))
            {
                CurrentDataTable.ImportRow(row);
            }

            if (this.IdentifierList[0] == "*")
            {
                for (int i = 0; i < CurrentDataTable.Columns.Count; i++)
                {
                    IVariable var = (IVariable)this.Context.GetVariable(CurrentDataTable.Columns[i].ColumnName);

                    if (var != null)
                    {
                        if (var.VarType != VariableType.Global && var.VarType != VariableType.Permanent)
                        {
                            TempVariableList.Add(CurrentDataTable.Columns[i].ColumnName.ToUpperInvariant());
                        }
                    }
                    else
                    {
                        TempVariableList.Add(CurrentDataTable.Columns[i].ColumnName.ToUpperInvariant());
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.IdentifierList.Length; i++)
                {
                    TempVariableList.Add(this.IdentifierList[i].ToUpperInvariant());
                }
            }

            if (isExceptionList)
            {
                for (int i = CurrentDataTable.Columns.Count - 1; i > -1; i--)
                {
                    if (TempVariableList.Contains(CurrentDataTable.Columns[i].ColumnName.ToUpperInvariant()))
                    {
                        //CurrentDataTable.Columns.Remove(CurrentDataTable.Columns[i]);
                    }
                    else
                    {
                        if (this.IdentifierList[0] == "*")
                        {
                            IVariable var = (IVariable)this.Context.GetVariable(CurrentDataTable.Columns[i].ColumnName);

                            if (var != null)
                            {
                                if (var != null && var.VarType != VariableType.Global && var.VarType != VariableType.Permanent)
                                {
                                    VariableList.Add(var.Name.ToUpperInvariant());
                                }
                            }
                        }
                        else
                        {
                            VariableList.Add(CurrentDataTable.Columns[i].ColumnName.ToUpperInvariant());
                        }
                    }
                }
            }
            else // is NOT an isExceptionList
            {
                for (int i = 0; i < CurrentDataTable.Columns.Count; i++)
                {
                    if(TempVariableList.Contains(CurrentDataTable.Columns[i].ColumnName.ToUpperInvariant()))
                    {
                        VariableList.Add(CurrentDataTable.Columns[i].ColumnName.ToUpperInvariant());
                    }
                    else
                    {
                        //CurrentDataTable.Columns.Remove(CurrentDataTable.Columns[i]);
                    }
                }
            }

            try
            {
                Dictionary<string, List<TableColumn>> WideTableColumns = null;
                DataSets.Config.DataDriverDataTable dataDrivers = Configuration.GetNewInstance().DataDrivers;
                IDbDriverFactory dbFactory = null;
                foreach (DataSets.Config.DataDriverRow dataDriver in dataDrivers)
                {
                    dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(dataDriver.Type);

                    if (dbFactory.CanClaimConnectionString(FilePath))
                    {
                        break;
                    }
                }

                OutputDriver = DBReadExecute.GetDataDriver(FilePath, this.isConnectionString);

                if (OutputDriver.GetType().Name.Equals("CsvFile", StringComparison.OrdinalIgnoreCase) || this.FileDataFormat.Equals("TEXT", StringComparison.OrdinalIgnoreCase))
                {
                    if (!this.TableName.EndsWith(".txt") && !this.TableName.EndsWith(".csv") && !this.TableName.EndsWith(".json") && !this.TableName.EndsWith("#csv") && !this.TableName.EndsWith("#txt") && !this.TableName.EndsWith("#json"))
                    {
                        this.TableName = this.TableName + ".csv";
                    }
                }
                this.OutTarget = this.FilePath + ":" + this.TableName;
                this.curFile = OutputDriver.DataSource;

                if (!OutputDriver.CheckDatabaseExistance(FilePath, TableName, this.isConnectionString))
                {
                    DbDriverInfo collectDbInfo = new DbDriverInfo();
                    Type SqlDriverType = Type.GetType("Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer");
                    if (DBReadExecute.DataSource.GetType().AssemblyQualifiedName == SqlDriverType.AssemblyQualifiedName)
                    {
                        collectDbInfo.DBCnnStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
                    }
                    else
                    {
                        collectDbInfo.DBCnnStringBuilder = new System.Data.OleDb.OleDbConnectionStringBuilder();
                    }
                    collectDbInfo.DBCnnStringBuilder.ConnectionString = dbFactory.ConvertFileStringToConnectionString(FilePath);
                    //collectDbInfo.DBCnnStringBuilder = dbFactory.RequestDefaultConnection(dbFactory.FilePath.Trim());
                    OutputDriver = dbFactory.CreateDatabaseObject(collectDbInfo.DBCnnStringBuilder);
                    collectDbInfo.DBName = OutputDriver.DbName;
                    dbFactory.CreatePhysicalDatabase(collectDbInfo);
                }


                bool? deleteSuccessful = null;
                if (this.WriteMode.Equals("REPLACE", StringComparison.OrdinalIgnoreCase) && DBReadExecute.CheckDatabaseTableExistance(FilePath, TableName, this.isConnectionString))
                {
                    deleteSuccessful = OutputDriver.DeleteTable(TableName);
                }

                List<TableColumn> TableColumns = new List<TableColumn>();

                if (!DBReadExecute.CheckDatabaseTableExistance(FilePath, TableName, this.isConnectionString))
                {
                    foreach (DataColumn column in CurrentDataTable.Columns)
                    {
                        if (VariableList.Contains(column.ColumnName.ToUpperInvariant()))
                        {
                            bool isPermanentVariable = false;

                            IVariable candidate = Context.MemoryRegion.GetVariable(column.ColumnName);

                            if (candidate != null && candidate.IsVarType(VariableType.Permanent))
                            {
                                isPermanentVariable = true;
                            }

                            if (isPermanentVariable == false)
                            {
                                TableColumn newTableColumn;

                                if (column.DataType.ToString() == "System.String")
                                {
                                    if (column.MaxLength <= 0)
                                    {
                                        newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), int.MaxValue, column.AllowDBNull);
                                    }
                                    else
                                    {
                                        newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.MaxLength, column.AllowDBNull);
                                    }
                                }
                                else if (column.DataType.ToString() == "System.Guid")
                                {
                                    if (column.MaxLength <= 0)
                                    {
                                        newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), int.MaxValue, column.AllowDBNull);
                                    }
                                    else
                                    {
                                        newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.MaxLength, column.AllowDBNull);
                                    }
                                }
                                else
                                {
                                    newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.AllowDBNull);
                                }

                                newTableColumn.AllowNull = column.AllowDBNull;
                                newTableColumn.IsIdentity = column.Unique;
                                TableColumns.Add(newTableColumn);
                            }
                        }
                    }

                    if
                    (
                        (
                           (!(OutputDriver.GetType().Name.Equals("AccessDatabase", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("Access2007Database", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("ExcelWorkbook", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("Excel2007Workbook", StringComparison.OrdinalIgnoreCase))
                            && VariableList.Count <= Max_Number_Columns)
                            || OutputDriver.GetType().Name.Equals("SqlDatabase", StringComparison.OrdinalIgnoreCase)
                         )
                    )
                    {
                        if (this.TableName.EndsWith("json"))
                        {
                            DataTable table = CurrentDataTable;
                            try
                            {
                                asciiRecordCount = 0;
                                int totalRows = 0;

                                totalRows = table.Rows.Count;
                                StringBuilder jsb = new StringBuilder("[");
                                foreach (DataRow row in table.Rows)
                                {
                                    jsb.Append("\n    {");
                                    for (int i = 0; i < table.Columns.Count; i++)
                                    {
                                        string rowValue = row[i].ToString().Replace("\r\n", " ");
                                        if (rowValue.Contains(",") || rowValue.Contains("\""))
                                        {
                                            rowValue = rowValue.Replace("\"", "\"\"");
                                            rowValue = Util.InsertIn(rowValue, "\"");
                                        }
                                        jsb.Append("\n        \"" + table.Columns[i].ColumnName + "\" : ");
                                        if (table.Columns[i].DataType.FullName.Equals("System.String") ||
                                            table.Columns[i].DataType.FullName.Equals("System.DateTime") ||
                                            table.Columns[i].DataType.FullName.Equals("System.Date") ||
                                            table.Columns[i].DataType.FullName.Equals("System.Time"))
                                        {
                                            jsb.Append("\"" + rowValue + "\",");
                                        }
                                        else if (table.Columns[i].DataType.FullName.Equals("System.Boolean"))
                                        {
                                            jsb.Append(rowValue.ToLower() + ",");
                                        }
                                        else
                                        {
                                            if (String.IsNullOrEmpty(rowValue))
                                                rowValue = "null";
                                            jsb.Append(rowValue + ",");
                                        }
                                    }
                                    jsb.Remove(jsb.Length - 1, 1);
                                    jsb.Append("\n    },");
                                    asciiRecordCount++;
                                    if (asciiRecordCount % 500 == 0)
                                    {
                                        //OnSetStatusMessageAndProgressCount(string.Format(SharedStrings.DASHBOARD_EXPORT_PROGRESS, asciiRecordCount.ToString(), totalRows.ToString()), (double)asciiRecordCount);
                                    }
                                }
                                jsb.Remove(jsb.Length - 1, 1);
                                jsb.Append("\n]");
                                jsonstring = jsb.ToString();
                                //System.IO.File.WriteAllText(fileName, jsonstring);
                            }
                            catch (Exception ex)
                            {
                                this.statusMessage = ex.ToString();
                            }
                        }
                        else
                            OutputDriver.CreateTable(TableName, TableColumns);
                    }
                    else
                    {

                        if (OutputDriver.GetType().Name.Equals("ExcelWorkbook", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("Excel2007Workbook", StringComparison.OrdinalIgnoreCase))
                        {
                            WideTableColumns = this.CreateExcelWideTable(TableColumns);
                        }
                        else if(!OutputDriver.GetType().Name.Equals("CsvFile", StringComparison.OrdinalIgnoreCase))
                        {
                            WideTableColumns = this.CreateAccessWideTable(TableColumns);
                        }
                    }
                }
                else // check that column name exists in destinationl
                {

                    foreach (string columnName in VariableList)
                    {
                        bool isFound = false;
                        foreach (DataColumn column in CurrentDataTable.Columns)
                        {
                            if (column.ColumnName.ToUpperInvariant() == columnName.ToUpperInvariant())
                            {
                                isFound = true;
                                break;
                            }
                        }

                        if (!isFound)
                        {
                            TableColumn newTableColumn;
                            DataColumn column = CurrentDataTable.Columns[columnName];
                            if (column.DataType.ToString() == "System.String")
                            {
                                newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.MaxLength, column.AllowDBNull);
                            }
                            else if (column.DataType.ToString() == "System.Guid")
                            {
                                newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.MaxLength, column.AllowDBNull);
                            }
                            else
                            {
                                newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.AllowDBNull);
                            }
                            newTableColumn.AllowNull = column.AllowDBNull;
                            newTableColumn.IsIdentity = column.Unique;

                            OutputDriver.AddColumn(TableName, newTableColumn);
                        }
                    }

                    if ((OutputDriver.GetType().Name.Equals("AccessDatabase", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("Access2007Database", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("ExcelWorkbook", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("Excel2007Workbook", StringComparison.OrdinalIgnoreCase)) && VariableList.Count > Max_Number_Columns)
                    {
                        foreach (DataColumn column in CurrentDataTable.Columns)
                        {
                            if (VariableList.Contains(column.ColumnName.ToUpperInvariant()))
                            {
                                bool isPermanentVariable = false;

                                IVariable candidate = Context.MemoryRegion.GetVariable(column.ColumnName);

                                if (candidate != null && candidate.IsVarType(VariableType.Permanent))
                                {
                                    isPermanentVariable = true;
                                }

                                if (isPermanentVariable == false)
                                {
                                    TableColumn newTableColumn;

                                    if (column.DataType.ToString() == "System.String")
                                    {
                                        if (column.MaxLength <= 0)
                                        {
                                            newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), int.MaxValue, column.AllowDBNull);
                                        }
                                        else
                                        {
                                            newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.MaxLength, column.AllowDBNull);
                                        }
                                    }
                                    else if (column.DataType.ToString() == "System.Guid")
                                    {
                                        if (column.MaxLength <= 0)
                                        {
                                            newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), int.MaxValue, column.AllowDBNull);
                                        }
                                        else
                                        {
                                            newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.MaxLength, column.AllowDBNull);
                                        }
                                    }
                                    else
                                    {
                                        newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.AllowDBNull);
                                    }

                                    newTableColumn.AllowNull = column.AllowDBNull;
                                    newTableColumn.IsIdentity = column.Unique;
                                    TableColumns.Add(newTableColumn);
                                }
                            }
                        }

                        if (OutputDriver.GetType().Name.Equals("ExcelWorkbook", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("Excel2007Workbook", StringComparison.OrdinalIgnoreCase))
                        {

                            WideTableColumns = this.CreateExcelWideTable(TableColumns);
                        }
                        else
                        {
                            WideTableColumns = this.CreateAccessWideTable(TableColumns, false);
                        }
                    }
                }


                ////APPEND| REPLACE	| !Null
                //if (this.WriteMode.Equals("REPLACE", StringComparison.OrdinalIgnoreCase))
                //{
                //    WriteMethod = this.ReplaceWrite;
                //}
                //else
                //{
                //    WriteMethod = this.AppendWrite;
                //}


                if ((OutputDriver.GetType().Name.Equals("CsvFile", StringComparison.OrdinalIgnoreCase) && jsonstring is null) || this.FileDataFormat.Equals("TEXT", StringComparison.OrdinalIgnoreCase))
                {
                    if (TableColumns.Count == 0)
                    {
                        foreach (DataColumn column in CurrentDataTable.Columns)
                        {
                            if (VariableList.Contains(column.ColumnName.ToUpperInvariant()))
                            {
                                bool isPermanentVariable = false;

                                IVariable candidate = Context.MemoryRegion.GetVariable(column.ColumnName);

                                if (candidate != null && candidate.IsVarType(VariableType.Permanent))
                                {
                                    isPermanentVariable = true;
                                }

                                if (isPermanentVariable == false)
                                {
                                    TableColumn newTableColumn;

                                    if (column.DataType.ToString() == "System.String")
                                    {
                                        if (column.MaxLength <= 0)
                                        {
                                            newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), int.MaxValue, column.AllowDBNull);
                                        }
                                        else
                                        {
                                            newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.MaxLength, column.AllowDBNull);
                                        }
                                    }
                                    else if (column.DataType.ToString() == "System.Guid")
                                    {
                                        if (column.MaxLength <= 0)
                                        {
                                            newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), int.MaxValue, column.AllowDBNull);
                                        }
                                        else
                                        {
                                            newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.MaxLength, column.AllowDBNull);
                                        }
                                    }
                                    else
                                    {
                                        newTableColumn = new TableColumn(column.ColumnName.ToString(), ConvertToGenericType(column.DataType), column.AllowDBNull);
                                    }

                                    newTableColumn.AllowNull = column.AllowDBNull;
                                    newTableColumn.IsIdentity = column.Unique;
                                    TableColumns.Add(newTableColumn);
                                }
                            }
                        }
                    }
                    this.WriteCSVFile(TableColumns);
                }
                else if ((OutputDriver.GetType().Name.Equals("AccessDatabase", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("Access2007Database", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("ExcelWorkbook", StringComparison.OrdinalIgnoreCase) || OutputDriver.GetType().Name.Equals("Excel2007Workbook", StringComparison.OrdinalIgnoreCase)) && VariableList.Count > Max_Number_Columns)
                {
                    this.PopulateTable(WideTableColumns);
                }
                else if (jsonstring != null)
                {
                    System.IO.File.WriteAllText(string.Format("{0}\\{1}", curFile, TableName.ToString().Replace("#", ".")), jsonstring);
                    this.statusMessage = "Export completed successfully, ";
                    this.statusMessage += CurrentDataTable.Rows.Count.ToString() + " records written.";
                }
                else
                {
                    DataTable sourceTable = OutputDriver.GetTableData(TableName);

                    OutputDriver.IsBulkOperation = true; StringBuilder sqlquery = new StringBuilder();

                    int count = 0;

                    sqlquery.Append("create table [" + TableName + "] ( ");

                    foreach (string column in VariableList)
                    {
                        string columnName = String.Empty;
                        if (!column.Contains(".") && !OutputDriver.ColumnExists(TableName, column))
                        {
                            columnName = column;
                            if (count > 0)
                            {
                                sqlquery.Append(", ");
                            }
                            sqlquery.Append(" [" + columnName + "] " + DBReadExecute.SQLGetType(CurrentDataTable.Columns[column]));
                            count++;
                        }
                    }

                    sqlquery.Append(" )");

                    if (count > 0)
                    {
                        Query qr = OutputDriver.CreateQuery(sqlquery.ToString());
                        OutputDriver.ExecuteNonQuery(qr);
                    }

                    OutputDriver.IsBulkOperation = false;

                    //Insert data into table

                    ////Open connection
                    ////Setup Schema
                    ////Loop through records
                    ////Close connection
                    DataTable WritableTable = CurrentDataTable.Clone();
                    for (int i = WritableTable.Columns.Count - 1; i > -1; i--)
                    {
                        if (WritableTable.Columns[i].DataType == typeof(Guid))
                        {
                            WritableTable.Columns[i].DataType = typeof(String);
                        }
                    }
                    for (int i = WritableTable.Columns.Count - 1; i > -1; i--)
                    {
                        DataColumn col = WritableTable.Columns[i];

                        if (!VariableList.Contains(col.ColumnName.ToUpperInvariant()))
                        {
                            WritableTable.Columns.Remove(col);
                        }
                    }

                    foreach (DataRow row in CurrentDataTable.Select("", this.Context.SortExpression.ToString()))
                    {
                        DataRow newRow = WritableTable.NewRow();
                        foreach (string column in VariableList)
                        {
                            newRow[column] = row[column];
                        }

                        WritableTable.Rows.Add(newRow);
                    }

                    System.Data.Common.DbDataReader DataReader = WritableTable.CreateDataReader();
                    DBReadExecute.InsertBulkRows(FilePath, "Select * From [" + TableName + "]", DataReader, SetGadgetStatusHandler);

                    if (CurrentDataTable.Rows.Count > 0)
                    {
                        this.statusMessage = "Export completed successfully, ";
                    }
                    else
                    {
                        this.statusMessage = "Export was not completed successfully, ";
                    }
                    this.statusMessage += CurrentDataTable.Rows.Count.ToString() + " records written.";

                    DataReader.Close();
                }
            }
            catch (Exception ex)
            {
                this.statusMessage = "Problems exporting records: " + ex.ToString();
                System.Console.Write(ex);
            }
            finally
            {
                Context.AnalysisCheckCodeInterface.HideWaitDialog();
            }

            /*

                


               Comments 
                Records deleted in Enter or selected in Analysis are handled as in other Analysis commands.
                Defined variables may be written, allowing the creation of a new Epi Info file that makes the changes permanent.
                Global and permanent variables will not be written unless explicitly specified.
                To write only selected variables, the word EXCEPT may be inserted to indicate all variables except those following EXCEPT.
                If the output file specified does not exist, the WRITE command will attempt to create it.
                Either APPEND or REPLACE must be specified to indicate that an existing file/table by the 
                same name will be erased or records will be appended to the existing file/table. 
                If some, but not all, of the fields being written match those in an existing file during an APPEND,
                the unmatched fields are added to the output table.
                For Epi 6 REC or Access/EpiInfo table outputs, if there are no records,
                the WRITE command creates a table/file with variable information but no data.

                WRITE <METHOD> {<output type>} {<project>:}table {[<variable(s)>]} 
                WRITE <METHOD> {<output type>} {<project>:}table * EXCEPT {[<variable(s)>]}

                The <METHOD> represents either REPLACE or APPEND
                The <project> represents the path and filename of the output.
                The <variable(s)> represents one or more variable names.
                The <output type> represents the following allowable outputs:

                Database Type Specifier Element
 
                Jet "Access 97" "Access 2000"
                "Epi 2000"  <path:<table>
                dBase III  "dBase III"  <path>
                dBase IV "dBase IV"  <path>
                dBase 5.0  "dBase 5.0"  <path>
                Paradox 3.x  "Paradox 3.x"  <path>
                Paradox 4.x "Paradox 4.x" <path>
                FoxPro 2.0 "FoxPro 2.0" <path>
                FoxPro 2.5 "FoxPro 2.5" <path>
                FoxPro 2.6 "FoxPro 2.6" <path>
                Excel 3.0 "Excel 3.0" <path>
                Excel 4.0 "Excel 4.0" <path>
                Epi Info 6 "Epi6" <path>
                Text (Delimited) "Text" <path>

             */



            args.Add("COMMANDNAME", CommandNames.WRITE);
            args.Add("WRITEMODE", this.WriteMode);
            args.Add("OUTTARGET", this.OutTarget);
            args.Add("STATUS", this.statusMessage);
            //args.Add("PROGRESST", this.progress.ToString());
            //args.Add("ROWCOUNT", CurrentDataTable.Select("", this.Context.SortExpression.ToString()).Length.ToString());
            this.Context.AnalysisCheckCodeInterface.Display(args);


            return result;
        }

        private void SetGadgetStatusHandler(string pStatusMessage, double pProgress = 0)
        {
            this.statusMessage = pStatusMessage;
            this.progress = pProgress;
        }


        private void WriteCSVFile(List<TableColumn> TableColumns)
        {
            List<DataRow> Rows = this.Context.GetOutput();

            TextWriter stream = System.IO.File.AppendText(string.Format("{0}\\{1}",curFile,TableName.ToString().Replace("#",".")));
            //if (!this.WriteMode.Equals("REPLACE", StringComparison.OrdinalIgnoreCase))
            if (TableColumns.Count > Max_Number_Columns)
            {
                for (int i = 0; i < TableColumns.Count; i++)
                {

                    string s = TableColumns[i].Name.ToString();
                    if (s.IndexOfAny("\",\x0A\x0D".ToCharArray()) > -1)
                    {
                        stream.Write("\"" + s.Replace("\"", "\"\"") + "\"");
                    }
                    else
                    {
                        stream.Write(s);
                    }


                    if (i < TableColumns.Count - 1)
                    {
                        stream.Write(',');
                    }
                    else
                    {
                        stream.Write('\n');
                    }
                }
            }


            foreach (DataRow row in Rows)
            {
                int i = 0;
                foreach (TableColumn column in TableColumns)
                {
                    if (row[column.Name] != null)
                    {
                        string s = row[column.Name].ToString();
                        if (s.IndexOfAny("\",\x0A\x0D".ToCharArray()) > -1)
                        {
                            stream.Write("\"" + s.Replace("\"", "\"\"") + "\"");
                        }
                        else
                        {
                            stream.Write(s);
                        }
                    }

                    if (i < TableColumns.Count - 1)
                    {
                        stream.Write(',');
                    }
                    else
                    {
                        stream.Write('\n');
                    }
                    i++;
                }
            }
            stream.Flush();
            stream.Close();
            stream = null;
            asciiRecordCount = Rows.Count;

            if (Rows.Count > 0)
            {
                this.statusMessage = "Export completed successfully, ";
            }
            else
            {
                this.statusMessage = "Export was not completed successfully, ";
            }
            this.statusMessage += CurrentDataTable.Rows.Count.ToString() + " records written.";


        }

        private void GetOutTarget(Token pToken)
        {
            /*<OutTarget>                             ::= Identifier
                                | File ':' Identifier
                                | File 
                                | BraceString ':' Identifier*/

            if (pToken is NonterminalToken)
            {
                NonterminalToken NT = pToken as NonterminalToken;

                foreach (TerminalToken TT in NT.Tokens)
                {
                    if (TT.Symbol.ToString() == "Identifier")
                    {
                        this.TableName = TT.Text.Trim(new char[] { '[', ']' });
                        //this.TableName = this.TableName.Replace("#", ".");
                        this.TableName = this.TableName.Trim().Trim('\'');
                    }

                    if (TT.Symbol.ToString() == "File")
                    {
                        this.FilePath = TT.Text;
                    }

                    if (TT.Symbol.ToString() == "BraceString")
                    {
                        this.FilePath = TT.Text.Trim(new char[] { '{', '}' }); ;
                        isConnectionString = true;
                    }


                }
            }
            else
            {
                TerminalToken TT = pToken as TerminalToken;

                if (TT.Symbol.ToString() == "Identifier")
                {
                    this.TableName = TT.Text.Trim(new char[] { '[', ']' });
                    //this.TableName = this.TableName.Replace("#", ".");
                    this.TableName = this.TableName.Trim().Trim('\'');
                }

                if (TT.Symbol.ToString() == "File")
                {
                    this.FilePath = TT.Text;
                }

                if (TT.Symbol.ToString() == "BraceString")
                {
                    this.FilePath = TT.Text.Trim(new char[] { '{', '}' }); ;
                    isConnectionString = true;
                }
            }
        }


        private void PopulateTable(Dictionary<string, List<TableColumn>> pTableColumns)
        {

            //DataTable sourceTable = OutputDriver.GetTableData(this.TableName);
            //DataTable sourceTable = OutputDriver.GetTableData(this.TableName);
            foreach (KeyValuePair<string, List<TableColumn>> kvp in pTableColumns)
            {
                StringBuilder ColumnSQL = new StringBuilder();
                DataTable WritableTable = CurrentDataTable.Clone();
                List<TableColumn> TableColumns = kvp.Value;

                for (int i = WritableTable.Columns.Count - 1; i > -1; i--)
                {
                    DataColumn col = WritableTable.Columns[i];
                    bool isFound = false;
                    foreach (TableColumn TC in TableColumns)
                    {
                        if (TC.Name.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase))
                        {
                            isFound = true;
                            break;
                        }
                    }

                    if (!isFound)
                    {
                        WritableTable.Columns.Remove(col);
                    }
                }

                foreach (DataRow row in CurrentDataTable.Select("", this.Context.SortExpression.ToString()))
                {
                    DataRow newRow = WritableTable.NewRow();
                    foreach (TableColumn column in TableColumns)
                    {
                        newRow[column.Name] = row[column.Name];
                    }
                    WritableTable.Rows.Add(newRow);
                }


                System.Data.Common.DbDataReader DataReader = WritableTable.CreateDataReader();
                DBReadExecute.InsertBulkRows(FilePath, "Select * From [" + kvp.Key + "]", DataReader, SetGadgetStatusHandler);

                this.OutTarget += "<br/>" + FilePath + "\\" + kvp.Key;
            }


            if (CurrentDataTable.Rows.Count > 0)
            {
                this.statusMessage = "Export completed successfully, ";
            }
            else
            {
                this.statusMessage = "Export was not completed successfully, ";
            }
            this.statusMessage += CurrentDataTable.Rows.Count.ToString() + " records written.";

        }

        private Dictionary<string, List<TableColumn>> CreateAccessWideTable(List<TableColumn> pTableColumns, bool pCreateTable = true)
        {
            Dictionary<string, List<TableColumn>> result = new Dictionary<string, List<TableColumn>>();

            int columnCount = 0;
            int sequence = 0;

            List<TableColumn> primaryKeyList = new List<TableColumn>();
            foreach (TableColumn column in pTableColumns)
            {
                if (column.IsIdentity || column.IsPrimaryKey || column.Name.Equals("UniqueKey", StringComparison.OrdinalIgnoreCase) || column.Name.Equals("GlobalRecordId", StringComparison.OrdinalIgnoreCase))
                {
                    primaryKeyList.Add(column);
                }
            }
            List<TableColumn> ParcialTableColumns = new List<TableColumn>();
            ParcialTableColumns.AddRange(primaryKeyList);
            columnCount = ParcialTableColumns.Count;

            string[] tableName = new string[pTableColumns.Count / Max_Number_Columns + 1];
            tableName[0] = this.TableName;
            foreach (TableColumn column in pTableColumns)
            {
                if (columnCount < Max_Number_Columns)
                {
                    if(ParcialTableColumns.Contains(column))
                    {

                    }
                    else
                    {
                        ParcialTableColumns.Add(column);
                    }
                }
                else
                {
                    if (!OutputDriver.TableExists(tableName[sequence]))
                    {
                        if (pCreateTable)
                        {
                            OutputDriver.CreateTable(tableName[sequence], ParcialTableColumns);
                        }
                    }
                    result.Add(tableName[sequence], ParcialTableColumns);
                    sequence += 1;

                    if (sequence > 0)
                    {
                        tableName[sequence] = tableName[0] + "_Seq" + sequence.ToString();
                    }

                    ParcialTableColumns = new List<TableColumn>();
                    ParcialTableColumns.AddRange(primaryKeyList);
                    columnCount = ParcialTableColumns.Count;
                }
                columnCount += 1;
            }


            if (!OutputDriver.TableExists(tableName[sequence]))
            {
                if (pCreateTable)
                {
                    OutputDriver.CreateTable(tableName[sequence], ParcialTableColumns);
                }
            }
            result.Add(tableName[sequence], ParcialTableColumns);

            return result;
        }

        private Dictionary<string, List<TableColumn>> CreateExcelWideTable(List<TableColumn> pTableColumns, bool pCreateTable = true)
        {
            Dictionary<string, List<TableColumn>> result = new Dictionary<string, List<TableColumn>>();

            int columnCount = 0;
            int sequence = 0;

            List<TableColumn> primaryKeyList = new List<TableColumn>();
            foreach (TableColumn column in pTableColumns)
            {
                if (column.IsIdentity || column.IsPrimaryKey || column.Name.Equals("UniqueKey", StringComparison.OrdinalIgnoreCase) || column.Name.Equals("GlobalRecordId", StringComparison.OrdinalIgnoreCase))
                {
                    primaryKeyList.Add(column);
                }
            }
            List<TableColumn> ParcialTableColumns = new List<TableColumn>();
            ParcialTableColumns.AddRange(primaryKeyList);
            columnCount = ParcialTableColumns.Count;

            string[] tableName = new string[pTableColumns.Count / Max_Number_Columns + 1];
            tableName[0] = this.TableName;
            foreach (TableColumn column in pTableColumns)
            {
                if (columnCount < Max_Number_Columns)
                {
                    if (ParcialTableColumns.Contains(column))
                    {

                    }
                    else
                    {
                        ParcialTableColumns.Add(column);
                    }
                }
                else
                {
                    if (!OutputDriver.TableExists(tableName[sequence]))
                    {
                        if (pCreateTable)
                        {
                            OutputDriver.CreateTable(tableName[sequence], ParcialTableColumns);
                        }
                    }
                    result.Add(tableName[sequence], ParcialTableColumns);

                    sequence += 1;

                    if (sequence > 0)
                    {
                        tableName[sequence] = tableName[0] + "_Seq" + sequence.ToString();
                    }

                    ParcialTableColumns = new List<TableColumn>();
                    ParcialTableColumns.AddRange(primaryKeyList);
                    columnCount = ParcialTableColumns.Count;
                }
                columnCount += 1;
            }


            if (!OutputDriver.TableExists(tableName[sequence]))
            {
                if (pCreateTable)
                {
                    OutputDriver.CreateTable(tableName[sequence], ParcialTableColumns);
                }
            }
            result.Add(tableName[sequence], ParcialTableColumns);
            return result;
        }


    }


}
