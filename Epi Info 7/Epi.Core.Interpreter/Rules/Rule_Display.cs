using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using com.calitha.goldparser;
using Epi;
using Epi.Collections;
using Epi.Data;
using Epi.Data.Services;
using Epi.Fields;

using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Display : AnalysisRule
    {
        #region Private Data Members

        string commandText = null;
        string displayType = null;
        string outputOpt = string.Empty;
        string dbVariablesOpt = string.Empty;
        string dbViewsOpt = string.Empty;
        string identifierList = string.Empty;
        string displayOpt = string.Empty;
        string outTable = string.Empty;
        //private IMetadataProvider metadata = null;

        #endregion Private Data Members

        /// <summary>
        /// Constructor - Rule_Display
        /// </summary>
        /// <param name="pToken">The token to be parsed.</param>
        /// <returns>void</returns>
        public Rule_Display(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.commandText = this.ExtractTokens(pToken.Tokens);


            // <Variables_Display_Statement>    ::= DISPLAY DBVARIABLES <DbVariablesOpt> <DisplayOpt>
            // <Views_Display_Statement> 		::= DISPLAY DBVIEWS <DbViewsOpt> <DisplayOpt>
            // <Tables_Display_Statement> 		::= DISPLAY TABLES <DbViewsOpt> <DisplayOpt>

            // <DbVariablesOpt> ::= DEFINE | FIELDVAR | LIST <IdentifierList> | <IdentifierList> | !Null
            // <DisplayOpt> ::= <DefaultDisplayOpt> | !Null
            // <DefaultDisplayOpt> ::= OUTTABLE '=' Identifier
            // <DbViewsOpt> ::= <DefaultDbViewsOpt> | !Null
            // <DefaultDbViewsOpt> ::= File

            this.displayType = this.GetCommandElement(pToken.Tokens, 1);

            switch (pToken.Rule.Lhs.ToString())
            {
                case "<Variables_Display_Statement>":
                    if (pToken.Tokens.Length > 2)
                    {
                        dbVariablesOpt = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '"' }).ToUpperInvariant();
                        if (dbVariablesOpt.ToUpperInvariant().StartsWith(CommandNames.LIST))
                        {
                            identifierList = dbVariablesOpt.Substring(4).Trim();
                        }
                    }
                    break;
                case "<Views_Display_Statement>":
                case "<Tables_Display_Statement>":
                    if (pToken.Tokens.Length > 2)
                    {
                        dbViewsOpt = this.GetCommandElement(pToken.Tokens, 2);
                    }
                    break;
            }

            if (pToken.Tokens.Length > 3)
            {
                if (!string.IsNullOrEmpty(this.GetCommandElement(pToken.Tokens, 3)))
                {
                    string tokenFour = this.GetCommandElement(pToken.Tokens, 3);
                    if (tokenFour.ToUpperInvariant().Contains("OUTTABLE"))
                    {
                        outTable = this.GetCommandElement(pToken.Tokens, 3);
                        outTable = outTable.Substring(outTable.LastIndexOf('=') + 1).Trim();
                    }
                }
            }
        }

        /// <summary>
        /// Performs execution of the DISPLAY command.
        /// </summary>
        /// <returns>Returns a null object because it's the end of the recursion.</returns>
        public override object Execute()
        {
            
            string markup = string.Empty;
            DataTable dataTable = null;

            switch (this.displayType.ToUpperInvariant())
            {
                case CommandNames.DBVARIABLES:
                    markup = GetVariableMarkup(out dataTable);
                    break;
                case CommandNames.DBVIEWS:
                    markup = GetViewMarkup(out dataTable);
                    break;
                case CommandNames.TABLES:
                    markup = GetTableMarkup(out dataTable);
                    break;
            }

            if (!string.IsNullOrEmpty(this.outTable))
            {
                WriteToOutTable(dataTable, outTable);
            }

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", CommandNames.DISPLAY);
            args.Add("COMMANDTEXT", this.commandText);
            args.Add("DISPLAYTYPE", this.displayType);
            args.Add("OUTPUTOPTION", this.outputOpt);
            args.Add("VARIABLEOPTION", this.dbVariablesOpt);
            args.Add("HTMLRESULTS", markup);

            this.Context.AnalysisCheckCodeInterface.Display(args);
                

            return null;
        }

        /// <summary>
        /// GetVariableMarkup gets both the markup used in the HTMLRESULTS section and a DataTable
        /// with a row for each DBVARIABLE and its properties.
        /// </summary>
        /// <param name="table">An instance of a DataTable containing properties of DBVARIABLES.</param>
        /// <returns>Returns the markup used in the HTMLRESULTS section.</returns>
        private string GetVariableMarkup(out DataTable table)
        {
            StringBuilder sb = new StringBuilder();
            string[] varNames = null;
            table = new DataTable();

            if (!string.IsNullOrEmpty(this.identifierList))
            {
                varNames = this.identifierList.Split(' ');
                foreach (string s in varNames) s.Trim();
            }

            VariableCollection vars = this.GetVariables(varNames, this.dbVariablesOpt);

            this.Context.GetOutput();

            /*
            table.Columns.Add(new DataColumn(ColumnNames.VARIABLE, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.TABLE, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.FIELDTYPE, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.FORMATVALUE, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.SPECIALINFO, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.PROMPT, typeof(string)));
             */

            table.Columns.Add(new DataColumn(ColumnNames.PAGE_NUMBER, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.PROMPT, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.FIELDTYPE, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.VARIABLE, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.VARIABLE_VALUE, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.FORMATVALUE, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.SPECIALINFO, typeof(string)));
            table.Columns.Add(new DataColumn(ColumnNames.TABLE, typeof(string)));

            string tableText = string.Empty;
            string pattern = string.Empty;
            IDbDriver driver = null;
            DataTable viewTable = null;
            DataRow[] rows = null;
            Dictionary<string, string> formatStrings = new Dictionary<string, string>();

            if (this.Context.CurrentRead != null)
            {

                tableText = this.Context.CurrentRead.Identifier;
                driver = Epi.Data.DBReadExecute.GetDataDriver(this.Context.CurrentRead.File);
                List<string> tableNames = driver.GetTableNames();
                string viewTableName = "view" + tableText;
                List<string> tableNamesUpper = new List<string> { };

                foreach (string name in tableNames)
                {
                    tableNamesUpper.Add(name.ToUpperInvariant());
                }

                //if TableNamesUpper contains an Epi 3 viewTable then get that table's data
                if (tableNamesUpper.Contains(viewTableName.ToUpperInvariant()))
                {
                    viewTable = driver.GetTableData(viewTableName);

                    if (varNames == null)
                    {
                        rows = viewTable.Select();
                    }
                    else
                    {
                        StringBuilder filter = new StringBuilder();
                        foreach (string name in varNames)
                        {
                            filter.Append(string.Format(" OR Name = '{0}'", name));
                        }

                        rows = viewTable.Select(filter.ToString().Substring(4));
                    }

                    foreach (DataRow row in rows)
                    {
                        formatStrings.Add(row[ColumnNames.NAME].ToString(), row[ColumnNames.FORMATSTRING].ToString());
                    }
                }
            }


            List<string> RelatedTableList = new List<string>();
            if (this.Context.CurrentRead != null)
            {

                for (int i = 0; Context.CurrentRead.RelatedTables.Count > i; i++)
                {
                    RelatedTableList.Add(Context.CurrentRead.RelatedTables[i]);
                }
            }


            List<string> varnamelist = new List<string>();
            if (varNames != null)
            {

                foreach (string name in varNames)
                {
                    varnamelist.Add(name);
                }
            }

            //foreach (IVariable var in vars)
            foreach (DataColumn dataColumn in this.Context.DataSet.Tables["output"].Columns)
            {
                string ColumnsNames = "";
                tableText = "";
                if (this.Context.CurrentRead != null)
                {

                    foreach (var RTable in RelatedTableList)
                    {

                        if (this.Context.CurrentRead.IsEpi7ProjectRead && this.Context.CurrentProject.Views.Exists(RTable))
                        {

                            View tempView = this.Context.CurrentProject.Metadata.GetViewByFullName(RTable);

                            ColumnsNames = this.Context.CurrentProject.Metadata.GetTableColumnNames(tempView.Id);


                            if (ColumnsNames.Contains(dataColumn.ToString()))
                            {
                                tableText = RTable;
                                break;
                            }
                        }
                    }


                    if (string.IsNullOrEmpty(tableText))
                    {

                        tableText = this.Context.CurrentRead.Identifier;

                    }
                }
                else
                {
                    tableText = "output";
                }


                if (varNames != null)
                {
                    if (varnamelist.Contains(dataColumn.Caption.ToString().ToUpperInvariant()))
                    {
                        table.Rows.Add(GetDataTable(dataColumn, tableText, pattern, formatStrings, rows, table));
                    }
                }
                else if (vars != null)
                {
                    
                    if (vars.Contains(dataColumn.Caption.ToString().ToUpperInvariant()))
                    {
                        if (this.dbVariablesOpt == "FIELDVAR")
                        {
                            IVariable var = (IVariable)this.Context.GetVariable(dataColumn.ColumnName);
                            if (var.VarType == VariableType.DataSource || var.VarType == VariableType.DataSourceRedefined)
                            {
                                table.Rows.Add(GetDataTable(dataColumn, tableText, pattern, formatStrings, rows, table));
                            }
                        }
                        else
                        {
                            table.Rows.Add(GetDataTable(dataColumn, tableText, pattern, formatStrings, rows, table));
                        }
                    }
                }
                else
                {
                    if (this.dbVariablesOpt == "FIELDVAR")
                    {
                        IVariable var = (IVariable)this.Context.GetVariable(dataColumn.ColumnName);
                        if (var.VarType == VariableType.DataSource || var.VarType == VariableType.DataSourceRedefined)
                        {
                            table.Rows.Add(GetDataTable(dataColumn, tableText, pattern, formatStrings, rows, table));
                        }
                    }
                    else
                    {
                        table.Rows.Add(GetDataTable(dataColumn, tableText, pattern, formatStrings, rows, table));
                    }
                }




                //pattern = string.Empty;

                //IVariable var = (IVariable)this.Context.GetVariable(dataColumn.ColumnName);

                //if (var != null && (var.VarType == VariableType.DataSource) || (var.VarType == VariableType.DataSourceRedefined))
                //{
                //    formatStrings.TryGetValue(var.Name, out pattern);
                //}
                //else
                //{
                //    tableText = "Defined";
                //}

                //DataRow row = table.NewRow();

                /*
                row[ColumnNames.VARIABLE] = var.Name;
                row[ColumnNames.TABLE] = tableText;
                row[ColumnNames.FIELDTYPE] = var.DataType.ToString();
                row[ColumnNames.FORMATVALUE] = pattern;
                row[ColumnNames.SPECIALINFO] = var.VarType.ToString();
                row[ColumnNames.PROMPT] = var.PromptText;
                */
                // row[ColumnNames.PAGE_NUMBER] = var.???;
                //if (this.Context.CurrentRead.IsEpi7ProjectRead && this.Context.CurrentProject.Views.Exists(this.Context.CurrentRead.Identifier) && this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier].Fields.Exists(var.Name))
                //{
                //    Epi.Fields.Field field = this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier].Fields[var.Name];
                //    if (field is FieldWithSeparatePrompt)
                //    {
                //        row[ColumnNames.PROMPT] = ((FieldWithSeparatePrompt)field).PromptText;
                //    }
                //    else
                //    {
                //        row[ColumnNames.PROMPT] = var.PromptText;
                //    }
                //}
                //else
                //{
                //    row[ColumnNames.PROMPT] = var.PromptText;
                //}

                //if (this.Context.DataSet.Tables.Contains("output"))
                //{
                //    row[ColumnNames.FIELDTYPE] = this.Context.DataSet.Tables["output"].Columns[var.Name].DataType.ToString();
                //}
                //else
                //{
                //    row[ColumnNames.FIELDTYPE] = var.DataType.ToString();
                //}
                //row[ColumnNames.VARIABLE] = var.Name;
                ////row[ColumnNames.VARIABLE_VALUE] = var.???;
                //row[ColumnNames.FORMATVALUE] = pattern;
                //row[ColumnNames.SPECIALINFO] = var.VarType.ToString();
                //row[ColumnNames.TABLE] = tableText;

                //table.Rows.Add(row);

            }
            return BuildMarkupFromTable(table, string.Format("{0} ASC, {1} ASC", ColumnNames.TABLE, ColumnNames.VARIABLE));
        }




        private DataRow GetDataTable(DataColumn dataColumn, string tableText, string pattern, Dictionary<string, string> formatStrings, DataRow[] rows, DataTable table)
        {

            pattern = string.Empty;

            IVariable var = (IVariable)this.Context.GetVariable(dataColumn.ColumnName);
            if (var != null)
            {
                if (var.VarType == VariableType.DataSource || var.VarType == VariableType.DataSourceRedefined)
                {
                    formatStrings.TryGetValue(var.Name, out pattern);
                }
                else
                {
                    tableText = "Defined";
                }
            }
            else
            {
                //tableText = "Defined";
                var = new DataSourceVariable(dataColumn.ColumnName, DataType.Unknown);
            }

            DataRow row = table.NewRow();


            if (
                this.Context.CurrentRead != null &&
                this.Context.CurrentRead.IsEpi7ProjectRead &&
                this.Context.CurrentProject.Views.Exists(this.Context.CurrentRead.Identifier) &&
                this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier].Fields.Exists(var.Name)
                )
            {
                Epi.Fields.Field field = this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier].Fields[var.Name];
                if (field is FieldWithSeparatePrompt)
                {
                    row[ColumnNames.PROMPT] = ((FieldWithSeparatePrompt)field).PromptText;
                }
                else
                {
                    row[ColumnNames.PROMPT] = var.PromptText;
                }
                //Fiexes for Issue: 943
                if (field.FieldType.ToString() == MetaFieldType.Checkbox.ToString())
                {
                    row[ColumnNames.FIELDTYPE] = "Checkbox";
                }
                else
                {
                    row[ColumnNames.FIELDTYPE] = field.FieldType.ToString();
                }



            }
            else
            {
                row[ColumnNames.PROMPT] = var.PromptText;
                if (var.VarType == VariableType.Permanent)
                {
                    row[ColumnNames.FIELDTYPE] = var.DataType.ToString();
                }
                else
                {

                    if (this.Context.DataSet.Tables.Contains("output"))
                    {                       
                        row[ColumnNames.FIELDTYPE] = GetVariableType(this.Context.DataSet.Tables["output"].Columns[var.Name].DataType.ToString());
                    }
                    else
                    {
                        row[ColumnNames.FIELDTYPE] = var.DataType.ToString();
                    }
                }

            }

            row[ColumnNames.VARIABLE] = var.Name;

            row[ColumnNames.FORMATVALUE] = pattern;
            row[ColumnNames.SPECIALINFO] = var.VarType.ToString();
            row[ColumnNames.TABLE] = tableText;


            //  table.Rows.Add(row);

            return row;
        }


        private string GetVariableType(string vartype)
        {
            switch (vartype)
            {
                case "System.String" :
                    return "Text";                   
                case  "System.Byte" :
                    return "YesNo";                  
                case "System.Int16" :
                case "System.Int32":
                case "System.Int64":
                case "System.Double":
                case "System.Decimal":
                    return "Number";                 
                case "System.Boolean":
                    return "Checkbox";                   
                case "System.DateTime" :
                        return "DateTime";                    
                default :
                        return "Text";                  
            }
        }



        /// <summary>
        /// GetViewMarkup gets both the markup used in the HTMLRESULTS section and a DataTable
        /// with a row for each DBVIEW and its properties.
        /// </summary>
        /// <param name="table">An instance of a DataTable containing properties of DBVIEW.</param>
        /// <returns>Returns the markup used in the HTMLRESULTS section.</returns>
        private string GetViewMarkup(out DataTable table)
        {
            StringBuilder sb = new StringBuilder();

            table = new DataTable();
            table.Columns.Add(ColumnNames.DATA_TABLE_NAME, typeof(string));
            table.Columns.Add(ColumnNames.TYPE, typeof(string));
            table.Columns.Add(ColumnNames.LINK, typeof(string));
            table.Columns.Add(ColumnNames.PARENT_VIEW, typeof(string));
            table.Columns.Add(ColumnNames.CHILD_TYPE, typeof(string));
            table.Columns.Add(ColumnNames.CHILD_TABLES, typeof(string));

            try
            {
                IDbDriver driver = null;

                if (this.Context.CurrentRead != null)
                {
                    driver = Epi.Data.DBReadExecute.GetDataDriver(this.Context.CurrentRead.File);
                }
                if (!string.IsNullOrEmpty(dbViewsOpt))
                {
                    driver = Epi.Data.DBReadExecute.GetDataDriver(dbViewsOpt);
                }

                List<string> tableNames = driver.GetTableNames();
                int colCount;




                /////////////////////////////////

                //ViewCollection Views = metadata.GetViews();
                //////Views parent list

                //List<string> pViewsList = new List<string>();

                //foreach (var view in Views)
                //{

                //    pViewsList.Add(metadata.GetAvailDataTableName(((Epi.View)(view)).Name));
                //}
                //////Views list

                //List<string> ViewsList = new List<string>();

                //foreach (var view in Views)
                //{

                //    ViewsList.Add(((Epi.View)(view)).Name);
                //}


                //////Code table list 
                //List<string> CodeTableList = new List<string>();
                //DataTable CodeTables = metadata.GetCodeTableList();
                //foreach (DataRow CodeTable in CodeTables.Rows)
                //{

                //    CodeTableList.Add(((Epi.DataSets.TableSchema.TablesRow)(CodeTable)).TABLE_NAME);
                //}

                //////Data table list 

                //List<string> DataTableList = new List<string>();
                //DataTableList = metadata.GetDataTableList();


                ////////////////////////////////



                foreach (string name in tableNames)
                {
                    StringBuilder primaryKeys = new StringBuilder();
                    DataTable currentTable = driver.GetTableData(name);

                    colCount = table.Columns.Count;


                    DataRow row = table.NewRow();
                    row[ColumnNames.DATA_TABLE_NAME] = name;
                    //issue 769 start
                    //if (this.Context.CurrentRead != null)
                    //{
                    //    if (this.Context.CurrentProject != null && this.Context.CurrentProject.Views.Exists(this.Context.CurrentRead.Identifier))
                    //    {
                    //        row[ColumnNames.TYPE] = "View";
                    //    }
                    //    else
                    //    {
                    //        row[ColumnNames.TYPE] = "Data";
                    //    }
                    //}
                    //else
                    //{
                    //    row[ColumnNames.TYPE] = "Data";
                    //}

                    row[ColumnNames.TYPE] = GetTableTypeName(name, colCount);
                    //issue 769 end
                    row[ColumnNames.LINK] = string.Empty;
                    row[ColumnNames.PARENT_VIEW] = string.Empty;
                    row[ColumnNames.CHILD_TYPE] = string.Empty;
                    row[ColumnNames.CHILD_TABLES] = string.Empty;




                    table.Rows.Add(row);
                }
            }
            catch (NullReferenceException)
            {
                throw new GeneralException(SharedStrings.NO_DATA_SOURCE);
            }

            return BuildMarkupFromTable(table, string.Empty);
        }

        /// <summary>
        /// GetVariableMarkup gets both the markup used in the HTMLRESULTS section and a DataTable
        /// with a row for each TABLES and its properties.
        /// </summary>
        /// <param name="table">An instance of a DataTable containing properties of TABLES.</param>
        /// <returns>Returns the markup used in the HTMLRESULTS section.</returns>
        private string GetTableMarkup(out DataTable table)
        {
            StringBuilder sb = new StringBuilder();

            table = new DataTable();
            table.Columns.Add(ColumnNames.DATA_TABLE_NAME, typeof(string));
            table.Columns.Add(ColumnNames.TYPE, typeof(string));
            table.Columns.Add(ColumnNames.NUMBER_OF_FIELDS, typeof(Int16));
            table.Columns.Add(ColumnNames.LINK, typeof(string));
            table.Columns.Add(ColumnNames.PRIMARY_KEY, typeof(string));
            table.Columns.Add(ColumnNames.DESCRIPTION, typeof(string));

            try
            {
                IDbDriver driver = null;

                if (this.Context.CurrentRead != null)
                {
                    driver = Epi.Data.DBReadExecute.GetDataDriver(this.Context.CurrentRead.File);
                }
                if (!string.IsNullOrEmpty(dbViewsOpt))
                {
                    driver = Epi.Data.DBReadExecute.GetDataDriver(dbViewsOpt);
                }

                List<string> tableNames = driver.GetTableNames();
                int colCount;

                foreach (string name in tableNames)
                {
                    StringBuilder primaryKeys = new StringBuilder();
                    colCount = driver.GetTableData(name).Columns.Count;

                    foreach (DataColumn key in driver.GetTableData(name).PrimaryKey)
                    {
                        primaryKeys.Append(string.Format("{0} ", key.ColumnName));
                    }

                    DataRow row = table.NewRow();
                    row[ColumnNames.DATA_TABLE_NAME] = name;
                    row[ColumnNames.TYPE] = GetTableTypeName(name, colCount);
                    row[ColumnNames.NUMBER_OF_FIELDS] = colCount;
                    row[ColumnNames.LINK] = string.Empty;
                    row[ColumnNames.PRIMARY_KEY] = primaryKeys.ToString();
                    row[ColumnNames.DESCRIPTION] = string.Empty;
                    table.Rows.Add(row);
                }
            }
            catch (NullReferenceException)
            {
                throw new GeneralException(SharedStrings.NO_DATA_SOURCE);
            }

            return BuildMarkupFromTable(table, string.Empty);
        }

        #region Protected Methods

        /// <summary>
        /// BuildMarkupFromTable takes the DataTable from any one of the Get...Markup methods in
        /// this class and builds a table in HTML.
        /// <para>
        /// It does a select on the table to get an arry of DataRows, prints a table header then
        /// prints each row.
        /// </para>
        /// </summary>
        /// <param name="table">A DataTable containing the information to be displayed.</param>
        /// <param name="sortOrder">The sort string used in the select statement used on the table
        /// for each DataTable</param>
        /// <returns>The HTML markup of the DataTable</returns>
        private string BuildMarkupFromTable(DataTable table, string sortOrder)
        {
            StringBuilder builder = new StringBuilder();
            DataRow[] rows = table.Select("", sortOrder);

            builder.Append("<table>");
            PrintHeaderRow(table, builder);
            for (int i = 0; i < rows.Length; i++)
            {
                PrintRow(table, rows[i], builder);
            }
            builder.Append("</table>");

            return builder.ToString();
        }

        /// <summary>
        /// PrintRow appends HTML markup to a StringBuilder object from the given
        /// DataRow using the column names from the given DataTable
        /// </summary>
        /// <param name="table">The DataTable used to provide.</param>
        /// <param name="row">The DataRow to be displayed in the markup table.</param>
        /// <param name="builder">The StringBuilder that contains the HTML table markup.</param>
        private void PrintRow(DataTable table, DataRow row, StringBuilder builder)
        {
            builder.Append("<tr>");

            object[] Items = row.ItemArray;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn col = table.Columns[i];
                builder.Append("<td>");
                if (this.Context.Recodes.ContainsKey(col.ColumnName))
                {
                    builder.Append(this.Context.Recodes[col.ColumnName].GetRecode(row[this.Context.Recodes[col.ColumnName].SourceName]));
                }
                else
                {
                    builder.Append(row[col.ColumnName].ToString());
                }
                builder.Append("</td>");
            }

            builder.Append("</tr>");
        }

        /// <summary>
        /// PrintHeaderRow just adds the header (column names) to the HTML markup
        /// table. It gets the column names from the given DataTable.
        /// </summary>
        /// <param name="table">The DataTable that will be written to the HTML table.</param>
        /// <param name="builder">The StringBuilder that contains the HTML table markup.</param>
        private void PrintHeaderRow(DataTable table, StringBuilder builder)
        {
            builder.Append("<tr>");
            for (int i = 0; i < table.Columns.Count; i++)
            {
                DataColumn col = table.Columns[i];
                builder.Append("<th>");
                builder.Append(col.ColumnName);
                builder.Append("</th>");
            }
            builder.Append("</tr>");
        }

        /// <summary>
        /// GetVariables returns a NamedObjectCollection generic for the given variable
        /// names in the string array varList. The option string is used to set the type
        /// of variables returned.
        /// </summary>
        /// <param name="varList">varList is a string array containing the variable names.</param>
        /// <param name="opt"></param>
        /// <returns>VariableCollection is a Epi.Collections.NamedObjectCollection.</returns>
        private VariableCollection GetVariables(string[] varList, string option)
        {
            VariableType scopeCombination = 0;
            VariableCollection vars = null;
            switch (option)
            {
                case "DEFINE":
                    scopeCombination = VariableType.Global | VariableType.Permanent | VariableType.Standard;
                    break;
                case "FIELDVAR":
                    scopeCombination = VariableType.DataSource | VariableType.DataSourceRedefined;
                    break;
                case "LIST":
                    scopeCombination = VariableType.Global | VariableType.Permanent | VariableType.Standard |
                                     VariableType.DataSource | VariableType.DataSourceRedefined;
                    break;
                default:
                    scopeCombination = VariableType.Global | VariableType.Permanent | VariableType.Standard |
                                     VariableType.DataSource | VariableType.DataSourceRedefined;
                    break;
            }
            if (varList != null)
            {
                vars = new VariableCollection();
                foreach (string varname in varList)
                {
                    IVariable var = null;
                    if (this.Context.MemoryRegion.TryGetVariable(varname, out var))
                    {
                        vars.Add(var);
                    }
                }
            }
            else
            {
                vars = this.Context.MemoryRegion.GetVariablesInScope(scopeCombination);
            }

            return vars;
        }

        /// <summary>
        /// GetTableTypeName gets the type of a table as a string value based on
        /// the name of the table of the number of columns in the table.
        /// </summary>
        /// <param name="tableName">tableName is the name of the table.</param>
        /// <param name="numOfColumns">numOfColumn in the table.</param>
        /// <returns></returns>
        private string GetTableTypeName(string tableName, int numOfColumns)
        {
            string firstFourChars = tableName.Substring(0, 4).ToLowerInvariant();
            //issue 769 start
            if (firstFourChars == "view")
            {
                firstFourChars = "View";
            }
            else if (firstFourChars == "code")
            {
                firstFourChars = "Code";
            }
            else if (firstFourChars == "meta")
            {
                firstFourChars = "Meta";
            }
            else
            {
                firstFourChars = "Data";
            }
            //issue 769 end
            return firstFourChars;
        }

        /// <summary>
        /// WriteToOutTable deletes the table named in the outTableName param
        /// then creates and inserts and new table with the name given. 
        /// </summary>
        /// <param name="table">The DataTable that will be copied to the database.</param>
        /// <param name="outTableName">The name of the new table to be persised</param>
        private void WriteToOutTable(DataTable table, string outTableName)
        {
            if (!string.IsNullOrEmpty(outTableName))
            {
                if (this.Context.CurrentRead != null)
                {
                    if (DBReadExecute.CheckDatabaseTableExistance(this.Context.CurrentRead.File, outTableName))
                    {
                        DBReadExecute.ExecuteSQL(this.Context.CurrentRead.File, "Delete From " + outTableName);
                        DBReadExecute.ExecuteSQL(this.Context.CurrentRead.File, "Drop Table " + outTableName);
                    }
                    DBReadExecute.ExecuteSQL(this.Context.CurrentRead.File, DBReadExecute.GetCreateFromDataTableSQL(outTableName, table));
                    DBReadExecute.InsertData(this.Context.CurrentRead.File, "Select * from " + outTableName, table.CreateDataReader());
                }
            }
        }
        #endregion Protected Methods
    }
}


