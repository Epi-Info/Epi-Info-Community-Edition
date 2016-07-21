// Using Parameters with a DataAdapter 
// http://msdn.microsoft.com/en-us/library/bbw6zyha(v=vs.80).aspx

// http://msdn.microsoft.com/en-us/library/ms971491.aspx weaning use of command builder article


using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;

using Epi.Data;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Merge : AnalysisRule
    {
        bool HasRun = false;

        string FileName = null;
        string MergeType = null;
        string Identifier = null;
        string ReadOption = null;
        string LinkName = null;
        //string[] KeyDef = null;
        string KeyString = null;
        //Reduction FileSpec = null;


        public Rule_Merge(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            //MERGE File ':' Identifier <KeyDef> <MergeOpt>

            for (int i = 1; i < pToken.Tokens.Length; i++)
            {
                Token T = pToken.Tokens[i];
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<KeyExprIdentifier>":
                        case "<KeyDef>":
                            this.SetKeyDef(NT);
                            break;
                        case "<MergeOpt>":
                            this.SetMergeOpt(NT);
                            break;
                        case "<ReadOpt>":
                            this.SetReadOpt(NT);
                            break;
                        case "<FileSpec>":
                            this.SetFileSpec(NT);
                            break;
                    }
                }
                else
                {
                    TerminalToken TT = (TerminalToken)T;
                    switch (TT.Symbol.ToString())
                    {
                        case "Identifier":
                            this.Identifier = this.GetCommandElement(pToken.Tokens, i).Trim(new char[] { '{', '}' });
                            break;
                        case "File":
                        case "BraceString":
                            this.FileName = this.GetCommandElement(pToken.Tokens, i).Trim(new char[] { '{', '}' });
                            break;
                        case "APPEND":
                        case "UPDATE":
                        case "RELATE":
                            this.MergeType = this.GetCommandElement(pToken.Tokens, i);
                            break;
                    }
                }
            }
        }



        private void SetKeyDef(NonterminalToken pToken)
        {
            /*
            <KeyExpr> 							::= <KeyExprSimple> 
                                        | <KeyExprIdentifier>

        <KeyExprSimple> 						::= <Expression> '::' <UndefExpression>
        <KeyExprIdentifier> 						::= Identifier '::' <UndefExpression>

        <KeyDef> 							::= <KeyExpr>
                                        | <KeyDef> AND <KeyExpr>
        <KeyVarList> 							::= KEYVARS '=' <IdentifierList>
                                        | !Null
             */
            if (pToken.Tokens.Length == 1)
            {
                this.KeyString = this.GetCommandElement(pToken.Tokens, 0);
            }
            else
            {
                this.KeyString = this.ExtractTokens(pToken.Tokens);
            }
        }
        /*
        private void SetIdentifier(NonterminalToken pToken)
        {

        }
        private void SetFile(NonterminalToken pToken)
        {

        }

        private void SetBraceString(NonterminalToken pToken)
        {

        }*/

        private void SetReadOpt(NonterminalToken pToken)
        {
            //<ReadOpt>                           ::= String 
            this.ReadOption = this.GetCommandElement(pToken.Tokens, 1);
        }

        private void SetMergeOpt(NonterminalToken pToken)
        {
            /*
              <MergeOpt>                          ::= APPEND
                                | UPDATE
                                | RELATE*/
            this.MergeType = this.GetCommandElement(pToken.Tokens, 1);
        }

        private void SetLinkName(NonterminalToken pToken)
        {
            //<LinkName> ::= LINKNAME '=' Identifier
            this.LinkName = this.GetCommandElement(pToken.Tokens, 1);
        }

        private void SetFileSpec(NonterminalToken pToken)
        {
            /*
            !****** FileSpec                         *********!
<FileSpec>                          ::= <FilespecKey>
                                | <FileSpecField>
                                | <FileSpecKeyAndField>
                                

<FileSpecKey>                           ::= FILESPEC <FileSpecKeyList> END  
<FileSpecField>                         ::= <FileSpecFieldListEnd>
                                | <FileSpecFieldLiteral>

<FileSpecFieldListEnd>                      ::= FILESPEC  <FileSpecFieldList> END
<FileSpecFieldLiteral>                      ::= Identifier DecLiteral '-' DecLiteral Identifier

<FileSpecKeyAndField>                       ::= FILESPEC <FileSpecKeyList> <FileSpecFieldList> END

<FileSpecKeyList>                       ::= <FileSpecKeyDef>
                                | <FileSpecKeyDef> <FileSpecKeyList>

<FileSpecKeyDef>                        ::= <FileSpecNumericKey>
                                | <FileSpecStringKey>

<FileSpecNumericKey>                        ::= Identifier '=' <Number>
<FileSpecStringKey>                         ::= Identifier '=' <StringList>

<FileSpecFieldSingle>                       ::= Identifier DecLiteral Identifier

<FileSpecFieldDef>                      ::= <FileSpecField>
                                | <FileSpecFieldSingle>

<FileSpecFieldList>                         ::= <FileSpecFieldDef>
                                | <FileSpecFieldDef> <FileSpecFieldList> 

!***            End FileSpec            ***!
             * */
        }

        /*
        !***			Merge Statement 			***!
        <Merge_Table_Statement>         ::= MERGE Identifier <KeyDef> <MergeOpt>
        <Merge_Db_Table_Statement>     ::= MERGE <ReadOpt> File ':' Identifier <LinkName> <KeyDef> <MergeOpt>  <FileSpec>
                                                    | MERGE <ReadOpt> BraceString ':' Identifier <LinkName> <KeyDef> <MergeOpt>  <FileSpec>
                                                    | MERGE File ':' Identifier <KeyDef> <MergeOpt>
                                                    | MERGE BraceString ':' Identifier <KeyDef> <MergeOpt>
        <Merge_File_Statement>          ::= MERGE <ReadOpt> File <LinkName> <KeyDef> <MergeOpt> <FileSpec>
        <Merge_Excel_File_Statement>    ::= MERGE <ReadOpt> File ExcelRange <LinkName> <KeyDef> <MergeOpt> <FileSpec>
        <MergeOpt>                      ::= APPEND | UPDATE | RELATE | !Null
        !***			End					***!
         */
        /*

        <UndefExpression>  ::= Literal
                                        | Identifier
                                        | '-' '(' <UndefExpression> ')'
                                        | '-' Identifier
                                        | '(' <UndefExpression> ')'


        <EffectLeft> 							::= Identifier
                                        | <EffectLeft> ',' Identifier
        <EffectList> 							::= '(' <EffectLeft> ')'
                                            | !Null

        <TitleFontPart> 						::= TEXTFONT <Color> DecLiteral
                                        | TEXTFONT <Color>
                                        | TEXTFONT DecLiteral

        <Color> 							::= Identifier
                                            | HexLiteral

        <KeyExpr> 							::= <KeyExprSimple> 
                                        | <KeyExprIdentifier>

        <KeyExprSimple> 						::= <Expression> '::' <UndefExpression>
        <KeyExprIdentifier> 						::= Identifier '::' <UndefExpression>

        <KeyDef> 							::= <KeyExpr>
                                        | <KeyDef> AND <KeyExpr>
        <KeyVarList> 							::= KEYVARS '=' <IdentifierList>
                                        | !Null
        */


        /*
        this.WriteMode = this.GetCommandElement(pToken.Tokens, 1);
        this.FileDataFormat = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '"' });
        this.OutTarget = this.GetCommandElement(pToken.Tokens, 3);
        if (pToken.Tokens.Length <= 5)
        {
            this.IdentifierList = this.GetCommandElement(pToken.Tokens, 4).Split(' ');
        }
        else
        {
            this.IdentifierList = this.GetCommandElement(pToken.Tokens, 6).Split(' ');
            this.IsExceptionList = true;
        }*/

        /// <summary>
        /// performs execution of the MERGE command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {

            if (!this.HasRun)
            {

                int NumberInserted = 0;
                int NumberUpdated = 0;

                System.Data.Common.DbDataReader DbReader = null;
                System.Data.IDataAdapter DestinationAdapter = null;
                System.Data.DataTable DestinationTable = null;
                System.Data.DataTable SourceTable = null;
                // for each row in the source check for a match in the destination 
                //      if match then update
                //      if NOT match then insert
                Rule_Read CurrentRead = (Rule_Read)this.Context.CurrentRead;



                string[] KeySet = this.KeyString.ToUpperInvariant().Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);

                Dictionary<string, string> KeyCheck = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (string key in KeySet)
                {
                    string[] temp = key.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                    if (!KeyCheck.ContainsKey(temp[0]))
                    {
                        KeyCheck.Add(temp[0].Trim(), temp[0].Trim());
                    }

                }


                StringBuilder KeyMatch = new StringBuilder();
                StringBuilder UpdateSQL = new StringBuilder();


                string DestinationFile = CurrentRead.File;

                List<string> DestinationTableNameList = new List<string>();
                string SourceTableName = this.Identifier;
                bool SourceIsEpi7View = false;
                Project SourceProject = null;
                DBReadExecute.ParseConnectionString(this.FileName);
                if (DBReadExecute.ProjectFileName != "")
                {
                    SourceProject = new Project(DBReadExecute.ProjectFileName);
                    if (SourceProject.Views.Exists(this.Identifier))
                    {
                        SourceIsEpi7View = true;
                    }
                }

                if (this.Context.CurrentRead.IsEpi7ProjectRead && this.Context.CurrentProject.Views.Exists(CurrentRead.Identifier))
                {
                    if (SourceIsEpi7View)
                    {
                        SourceTableName = this.Context.CurrentProject.Views[CurrentRead.Identifier].TableName;
                    }
                    else
                    {
                        return MergeNonEpi7WithEpi7(this.Context.CurrentProject.Views[CurrentRead.Identifier]);
                    }

                    DestinationTableNameList.Add(this.Context.CurrentProject.Views[CurrentRead.Identifier].TableName);
                    foreach (Page p in this.Context.CurrentProject.Views[CurrentRead.Identifier].Pages)
                    {
                        DestinationTableNameList.Add(p.TableName);
                    }
                }
                else
                {
                    DestinationTableNameList.Add(CurrentRead.Identifier);
                }

                foreach (string DestinationTableName in DestinationTableNameList)
                {
                    StringBuilder SelectSQL = new StringBuilder();

                    //
                    NumberInserted = 0;
                    NumberUpdated = 0;

                    int columnIndex = 0;
                    string updateHeader = string.Empty;
                    StringBuilder whereClause = new StringBuilder();
                    List<DbCommand> CommandList = new List<DbCommand>();
                    List<String> fieldParameterList = new List<String>();


                    // compare data tables
                    DestinationTable = DBReadExecute.GetDataTable(DestinationFile, "Select * From [" + DestinationTableName + "] Where 1 = 0");

                    SourceTable = DBReadExecute.GetDataTable(this.FileName, "Select * From [" + SourceTableName + "] Where 1 = 0");

                    List<string> UsableColumns = new List<string>();


                    SelectSQL.Append("Select ");
                    foreach (System.Data.DataColumn C in DestinationTable.Columns)
                    {
                        if (SourceTable.Columns.Contains(C.ColumnName))
                        {
                            if (SourceTable.Columns[C.ColumnName].DataType != C.DataType)
                            {

                                SourceTable.Columns[C.ColumnName].DataType = C.DataType;
                            }
                            else
                            {
                                UsableColumns.Add(C.ColumnName);
                                SelectSQL.Append(C.ColumnName);
                                SelectSQL.Append(",");
                            }
                        }

                    }
                    SelectSQL.Length = SelectSQL.Length - 1;
                    SelectSQL.Append(" From ");
                    SelectSQL.Append(DestinationTableName);

                    DestinationTable = DBReadExecute.GetDataTable(DestinationFile, "Select * From [" + DestinationTableName + "]");

                    if (this.Context.CurrentRead.IsEpi7ProjectRead && SourceIsEpi7View)
                    {
                        // source and destination names should match
                        SourceTable = DBReadExecute.GetDataTable(this.FileName, "Select * From [" + DestinationTableName + "]");
                    }
                    else
                    {
                        SourceTable = DBReadExecute.GetDataTable(this.FileName, "Select * From [" + SourceTableName + "]");
                    }

                    System.Data.Common.DbCommand Command = DBReadExecute.GetCommand(CurrentRead.File, this.KeyString, DestinationTable);

                    // Begin - Build CommandText

                    //Build update header
                    UpdateSQL.Append("UPDATE [");
                    UpdateSQL.Append(DestinationTableName);
                    UpdateSQL.Append("] Set ");
                    updateHeader = UpdateSQL.ToString();

                    UpdateSQL.Remove(0, UpdateSQL.ToString().Length);

                    //Build Where Clause
                    whereClause.Append(" Where ");
                    for (int i = 0; i < KeySet.Length; i++)
                    {
                        string[] temp = KeySet[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                        if (i != 0)
                        {
                            whereClause.Append(" And ");
                        }
                        whereClause.Append(temp[0].Trim());
                        whereClause.Append(" = @");
                        whereClause.Append(temp[1].Trim());
                        whereClause.Append(",");

                    }

                    if (whereClause.Length > 0)
                    {
                        whereClause.Length = whereClause.Length - 1;
                    }


                    foreach (string C in UsableColumns)
                    {
                        columnIndex += 1;

                        if (!KeyCheck.ContainsKey(C.ToUpperInvariant()))
                        {
                            UpdateSQL.Append(C);
                            UpdateSQL.Append("=@");
                            UpdateSQL.Append(C);
                            UpdateSQL.Append(",");
                        }

                        fieldParameterList.Add(C);
                    }

                    DbParameter[] paramList = new DbParameter[Command.Parameters.Count];

                    Command.Parameters.CopyTo(paramList, 0);
                    foreach (DbParameter p in paramList)
                    {
                        if (!fieldParameterList.Contains(p.ParameterName))
                        {
                            Command.Parameters.Remove(p);
                        }
                    }
                    Command.CommandText = updateHeader + UpdateSQL.ToString().Trim(StringLiterals.COMMA.ToCharArray()) + whereClause.ToString();
                    CommandList.Add(Command);

                    fieldParameterList.Clear();
                    columnIndex = 0;

                    string USQL = Command.CommandText;

                    UpdateSQL.Remove(0, UpdateSQL.ToString().Length);
                    //UpdateSQL.Length = UpdateSQL.Length - 1;

                    // End - Build CommandText
                    //Command.CommandText = UpdateSQL.ToString();
                    int StatusCount = 0;

                    DbReader = SourceTable.CreateDataReader();
                    while (DbReader.Read())
                    {
                        StatusCount++;
                        if (StatusCount % 25 == 0)
                        {
                            Dictionary<string, string> args2 = new Dictionary<string, string>();
                            args2.Add("status", string.Format("merge in-progress: {0} of {1} records processed...", StatusCount, SourceTable.Rows.Count));
                            this.Context.AnalysisCheckCodeInterface.DisplayStatusMessage(args2);
                        }
                        KeyMatch.Length = 0;
                        for (int i = 0; i < KeySet.Length; i++)
                        {
                            string[] temp = KeySet[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                            if (i != 0)
                            {
                                KeyMatch.Append(" And ");
                            }


                            KeyMatch.Append("[");
                            KeyMatch.Append(temp[0].Trim());
                            KeyMatch.Append("]");
                            switch (DbReader[temp[1].Trim()].GetType().ToString())
                            {
                                case "System.Int32":
                                case "System.Decimal":
                                case "System.Boolean":
                                case "System.Double":
                                case "System.Single":
                                    KeyMatch.Append(" = ");
                                    KeyMatch.Append(DbReader[temp[1].Trim()]);
                                    break;
                                default:
                                    KeyMatch.Append(" = '");
                                    KeyMatch.Append(DbReader[temp[1].Trim()]);
                                    KeyMatch.Append("'");
                                    break;
                            }
                        }

                        if (this.Context.CurrentRead.IsEpi7ProjectRead && this.Context.CurrentProject.Views.Exists(this.Context.CurrentRead.Identifier) && !SourceIsEpi7View)
                        {
                            DestinationTable = DBReadExecute.GetDataTable(DestinationFile, "Select Count(*) " + this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier].FromViewSQL + " Where " + KeyMatch.ToString());
                        }
                        else
                        {
                            DestinationTable = DBReadExecute.GetDataTable(DestinationFile, "Select Count(*) From [" + DestinationTableName + "] Where " + KeyMatch.ToString());
                        }
                        switch (DestinationTable.Rows[0][0].ToString())
                        {
                            case "1":
                                //DBReadExecute.Update_1_Row(DestinationFile, SelectSQL, DestinationTableName, DbReader);
                                foreach (DbCommand uCommand in CommandList)
                                {
                                    foreach (System.Data.Common.DbParameter param in uCommand.Parameters)
                                    {
                                        param.Value = DbReader[param.ParameterName];
                                    }
                                    //DBReadExecute.Update_1_Row(DestinationFile, SelectSQL.ToString(), uCommand);
                                    //DBReadExecute.Update_1_Row(DestinationFile, SelectSQL.ToString(), this.KeyString, DbReader);
                                    Rule_Merge.Update_1_Row(DestinationFile, "Select * From [" + DestinationTableName + "]", KeyMatch.ToString(), DbReader);
                                }
                                NumberUpdated++;
                                break;
                            case "0":
                                Rule_Merge.Insert_1_Row(DestinationFile, "Select * From [" + DestinationTableName + "]", DbReader);
                                NumberInserted++;
                                break;
                            default:
                                throw new Exception("The key used is NOT unique: " + this.KeyString);

                        }
                    }
                }

                //result = string.Format("number of records Merged {0}", Output.Rows.Count);
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", CommandNames.MERGE);
                args.Add("DESTINATIONFILENAME", DestinationFile);
                args.Add("DESTINATIONTABLENAME", CurrentRead.Identifier);
                args.Add("FROMFILENAME", this.FileName);
                args.Add("FROMTABLENAME", this.Identifier);
                args.Add("NUMBERINSERTED", NumberInserted.ToString());
                args.Add("NUMBERUPDATED", NumberUpdated.ToString());

                this.Context.AnalysisCheckCodeInterface.Display(args);
                this.HasRun = true;
            }

            return null;
        }



        private void Merge_Test1()
        {
            string connetionString = null;
            OleDbConnection connection;
            OleDbDataAdapter oledbAdapter;
            DataSet ds1 = new DataSet();
            DataSet ds2 = new DataSet();
            DataTable dt;
            string firstSql = null;
            string secondSql = null;
            int i = 0;
            connetionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Your mdb filename;";
            firstSql = "Your First SQL Statement Here";
            secondSql = "Your Second SQL Statement Here";
            connection = new OleDbConnection(connetionString);
            try
            {
                connection.Open();
                oledbAdapter = new OleDbDataAdapter(firstSql, connection);
                oledbAdapter.Fill(ds1, "First Table");
                oledbAdapter.SelectCommand.CommandText = secondSql;
                oledbAdapter.Fill(ds2, "Second Table");
                oledbAdapter.Dispose();
                connection.Close();

                ds1.Tables[0].Merge(ds2.Tables[0]);
                dt = ds1.Tables[0];

                for (i = 0; i <= dt.Rows.Count - 1; i++)
                {
                    System.Console.Write(dt.Rows[i].ItemArray[0] + " -- " + dt.Rows[i].ItemArray[0]);

                }
            }
            catch (Exception ex)
            {
                System.Console.Write("Can not open connection ! ");
            }
        }


        #region MergeSample

        public static void DisplayDataTable(DataTable myDataTable)
        {

            // display the columns for each row in the DataTable,
            // using a DataRow object to access each row in the DataTable
            foreach (DataRow myDataRow in myDataTable.Rows)
            {
                Console.WriteLine("CustomerID = " + myDataRow["CustomerID"]);
                Console.WriteLine("CompanyName = " + myDataRow["CompanyName"]);
                Console.WriteLine("ContactName = " + myDataRow["ContactName"]);
                Console.WriteLine("Address = " + myDataRow["Address"]);
            }

        }

        public static void AddRow(
          DataTable myDataTable
        )
        {

            Console.WriteLine("\nAdding a new row with CustomerID of 'T1COM'");

            // step 1: use the NewRow() method of the DataRow object to create
            // a new row in the DataTable
            DataRow myNewDataRow = myDataTable.NewRow();

            // step 2: set the values for the columns of the new row
            myNewDataRow["CustomerID"] = "T1COM";
            myNewDataRow["CompanyName"] = "T1 Company";
            myNewDataRow["ContactName"] = "Jason Price";
            myNewDataRow["Address"] = "1 Main Street";

            // step 3: use the Add() method through the Rows property to add
            // the new DataRow to the DataTable
            myDataTable.Rows.Add(myNewDataRow);

            // step 4: use the AcceptChanges() method of the DataTable to commit
            // the changes
            myDataTable.AcceptChanges();

        }

        public static void ModifyRow(
          DataTable myDataTable
        )
        {

            Console.WriteLine("\nModifying the new row");

            // step 1: set the PrimaryKey property for the DataTable object
            DataColumn[] myPrimaryKey = new DataColumn[1];
            myPrimaryKey[0] = myDataTable.Columns["CustomerID"];
            myDataTable.PrimaryKey = myPrimaryKey;

            // step 2: use the Find() method to locate the DataRow
            // in the DataTable using the primary key value
            DataRow myEditDataRow = myDataTable.Rows.Find("T1COM");

            // step 3: change the column values
            myEditDataRow["CompanyName"] = "Widgets Inc.";
            myEditDataRow["ContactName"] = "John Smith";
            myEditDataRow["Address"] = "1 Any Street";

            // step 4: use the AcceptChanges() method of the DataTable to commit
            // the changes
            myDataTable.AcceptChanges();
            Console.WriteLine("myEditDataRow.RowState = " + myEditDataRow.RowState);

        }

        public static void RemoveRow(
          DataTable myDataTable
        )
        {

            Console.WriteLine("\nRemoving the new row");

            // step 1: set the PrimaryKey property for the DataTable object
            DataColumn[] myPrimaryKey = new DataColumn[1];
            myPrimaryKey[0] = myDataTable.Columns["CustomerID"];
            myDataTable.PrimaryKey = myPrimaryKey;

            // step 2: use the Find() method to locate the DataRow
            DataRow myRemoveDataRow = myDataTable.Rows.Find("T1COM");

            // step 3: use the Delete() method to remove the DataRow
            myRemoveDataRow.Delete();

            // step 4: use the AcceptChanges() method of the DataTable to commit
            // the changes
            myDataTable.AcceptChanges();

        }

        public static void Merge_Main()
        {

            // formulate a string containing the details of the
            // database connection
            string connectionString =
              "server=localhost;database=Northwind;uid=sa;pwd=sa";

            // create a SqlConnection object to connect to the
            // database, passing the connection string to the constructor
            OleDbConnection mySqlConnection =
              new OleDbConnection(connectionString);

            // formulate a SELECT statement to retrieve the
            // CustomerID, CompanyName, ContactName, and Address
            // columns for the first row from the Customers table
            string selectString =
              "SELECT CustomerID, CompanyName, ContactName, Address " +
              "FROM Customers " +
              "WHERE CustomerID = 'ALFKI'";

            // create a SqlCommand object to hold the SELECT statement
            OleDbCommand mySqlCommand = mySqlConnection.CreateCommand();

            // set the CommandText property of the SqlCommand object to
            // the SELECT string
            mySqlCommand.CommandText = selectString;

            // create a SqlDataAdapter object
            OleDbDataAdapter mySqlDataAdapter = new OleDbDataAdapter();

            // set the SelectCommand property of the SqlAdapter object
            // to the SqlCommand object
            mySqlDataAdapter.SelectCommand = mySqlCommand;

            // create a DataSet object to store the results of
            // the SELECT statement
            DataSet myDataSet = new DataSet();

            // open the database connection using the
            // Open() method of the SqlConnection object
            mySqlConnection.Open();

            // use the Fill() method of the SqlDataAdapter object to
            // retrieve the rows from the table, storing the rows locally
            // in a DataTable of the DataSet object
            Console.WriteLine("Retrieving a row from the Customers table");
            mySqlDataAdapter.Fill(myDataSet, "Customers");

            // get the DataTable object from the DataSet object
            DataTable myDataTable = myDataSet.Tables["Customers"];

            // display the rows in the DataTable object
            DisplayDataTable(myDataTable);

            // add a new row
            AddRow(myDataTable);
            DisplayDataTable(myDataTable);

            // modify a row
            ModifyRow(myDataTable);
            DisplayDataTable(myDataTable);

            // remove a row
            RemoveRow(myDataTable);
            DisplayDataTable(myDataTable);

            // use the Fill() method of the SqlDataAdapter object
            // to synchronize the changes with the database
            mySqlDataAdapter.Fill(myDataSet, "Customers");

            // close the database connection using the Close() method
            // of the SqlConnection object
            mySqlConnection.Close();

        }



        #endregion

        /// <summary>
        /// Update one row
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pUpdateSQL">Update SQL Statement</param>
        /// <param name="pCommand">Command</param>
        /// <returns>bool</returns>
        public static bool Update_1_Row(string pFileString, string pSelectSQL, string pKeyString, System.Data.Common.DbDataReader pDataReader)
        {
            bool result = false;

            System.Data.SqlClient.SqlConnection Conn = null;
            System.Data.OleDb.OleDbConnection ConnOle = null;
            System.Data.SqlClient.SqlDataAdapter Adapter = null;
            System.Data.OleDb.OleDbDataAdapter AdapterOle = null;
            System.Data.SqlClient.SqlCommandBuilder builderSQL = null;
            System.Data.OleDb.OleDbCommandBuilder builderOLE = null;
            System.Data.SqlClient.SqlCommand cmdSqL = null;
            System.Data.Common.DbCommand cmdOle = null;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            string ConnectionString = DBReadExecute.ParseConnectionString(pFileString);
            StringBuilder UpdateSQL = new StringBuilder();

            try
            {
            Type SQLServerType = Type.GetType("Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer");
            if (DBReadExecute.DataSource.GetType().AssemblyQualifiedName == SQLServerType.AssemblyQualifiedName)
            {


                //case DBReadExecute.enumDataSouce.SQLServer:
                Conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
                Adapter = new System.Data.SqlClient.SqlDataAdapter(pSelectSQL, Conn);
                //Adapter.FillSchema(dataSet, SchemaType.Source, pDestinationTableName);
                Adapter.FillSchema(dataSet, SchemaType.Source);
                builderSQL = new System.Data.SqlClient.SqlCommandBuilder(Adapter);
                Conn.Open();

                cmdSqL = Conn.CreateCommand();
                cmdSqL = builderSQL.GetInsertCommand();
                cmdSqL.CommandTimeout = 1500;

                UpdateSQL.Append("Update ");
                UpdateSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                UpdateSQL.Append(" Set ");
                foreach (System.Data.SqlClient.SqlParameter param in cmdSqL.Parameters)
                {
                    //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                    string FieldName = param.SourceColumn;
                    try
                    {

                        StringBuilder TUpdateSQL = new StringBuilder();

                        if (pDataReader[FieldName] != DBNull.Value && !string.IsNullOrEmpty(pDataReader[FieldName].ToString()))
                        {
                            TUpdateSQL.Append("[");
                            TUpdateSQL.Append(FieldName);
                            TUpdateSQL.Append("]=");

                            switch (pDataReader[FieldName].GetType().ToString())
                            {
                                case "System.Boolean":
                                    if (Convert.ToBoolean(pDataReader[FieldName]) == false)
                                    {
                                        TUpdateSQL.Append("0");
                                    }
                                    else
                                    {
                                        TUpdateSQL.Append("1");
                                    }
                                    break;
                                case "System.Int32":
                                case "System.Decimal":
                                case "System.Double":
                                case "System.Single":
                                case "System.Byte":
                                    TUpdateSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                    break;
                                default:
                                    TUpdateSQL.Append("'");
                                    TUpdateSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                    TUpdateSQL.Append("'");
                                    break;
                            }
                            TUpdateSQL.Append(",");
                        }

                        UpdateSQL.Append(TUpdateSQL);
                    }
                    catch (Exception ex)
                    {
                        // do nothing
                    }


                }
                UpdateSQL.Length = UpdateSQL.Length - 1;
                UpdateSQL.Append(" Where ");
                UpdateSQL.Append(pKeyString);
                //builderOLE = null;
                cmdSqL = null;
                cmdSqL = Conn.CreateCommand();
                cmdSqL.CommandText = UpdateSQL.ToString();
                cmdSqL.ExecuteNonQuery();
                //break;
            }
            else
            {
               

                
                    //case DBReadExecute.enumDataSouce.MSAccess:
                    //case DBReadExecute.enumDataSouce.MSAccess2007:
                    //case DBReadExecute.enumDataSouce.MSExcel:
                    //case DBReadExecute.enumDataSouce.MSExcel2007:
                        ConnOle = new System.Data.OleDb.OleDbConnection(ConnectionString);
                        AdapterOle = new System.Data.OleDb.OleDbDataAdapter(pSelectSQL, ConnOle);
                        //Adapter.FillSchema(dataSet, SchemaType.Source, pDestinationTableName);
                        AdapterOle.FillSchema(dataSet, SchemaType.Source);
                        AdapterOle.Fill(Temp);
                        builderOLE = new System.Data.OleDb.OleDbCommandBuilder();
                        builderOLE.DataAdapter = AdapterOle;

                        ConnOle.Open();
                        cmdOle = ConnOle.CreateCommand();
                        cmdOle = builderOLE.GetInsertCommand();
                        cmdOle.CommandTimeout = 1500;



                        UpdateSQL.Append("Update ");
                        UpdateSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                        UpdateSQL.Append(" Set ");
                        foreach (System.Data.OleDb.OleDbParameter param in cmdOle.Parameters)
                        {
                            //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                            string FieldName = param.SourceColumn;
                            try
                            {

                                StringBuilder TUpdateSQL = new StringBuilder();

                                if (pDataReader[FieldName] != DBNull.Value && !string.IsNullOrEmpty(pDataReader[FieldName].ToString()))
                                {
                                    TUpdateSQL.Append("[");
                                    TUpdateSQL.Append(FieldName);
                                    TUpdateSQL.Append("]=");

                                    switch (pDataReader[FieldName].GetType().ToString())
                                    {

                                        case "System.Int32":
                                        case "System.Decimal":
                                        case "System.Boolean":
                                        case "System.Double":
                                        case "System.Single":
                                        case "System.Byte":
                                            TUpdateSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                            break;
                                        default:
                                            TUpdateSQL.Append("'");
                                            TUpdateSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                            TUpdateSQL.Append("'");
                                            break;
                                    }
                                    TUpdateSQL.Append(",");
                                }

                                UpdateSQL.Append(TUpdateSQL);

                            }
                            catch (Exception ex)
                            {
                                // do nothing 
                            }
                        }
                        UpdateSQL.Length = UpdateSQL.Length - 1;
                        UpdateSQL.Append(" Where ");
                        UpdateSQL.Append(pKeyString);
                        builderOLE = null;
                        cmdOle = null;
                        cmdOle = ConnOle.CreateCommand();
                        cmdOle.CommandText = UpdateSQL.ToString();

                        //DBReadExecute.ExecuteSQL(pFileString, InsertSQL.ToString());

                        cmdOle.ExecuteNonQuery();
                        //break;
                }

           
            }
            catch (System.Exception ex)
            {
                Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            finally
            {
                if (Conn != null)
                {
                    Conn.Close();
                }

                if (ConnOle != null)
                {
                    ConnOle.Close();
                }

            }

            result = true;
            return result;
        }

        /// <summary>
        /// Insert one row
        /// </summary>
        /// <param name="pFileString">File String</param>
        /// <param name="pSelectSQL">Select Statement</param>
        /// <param name="pDataReader">DataReader</param>
        /// <returns>bool</returns>
        public static bool Insert_1_Row(string pFileString, string pSelectSQL, System.Data.Common.DbDataReader pDataReader)
        {
            bool result = false;

            System.Data.SqlClient.SqlConnection Conn = null;
            System.Data.OleDb.OleDbConnection ConnOle = null;
            System.Data.SqlClient.SqlDataAdapter Adapter = null;
            System.Data.OleDb.OleDbDataAdapter AdapterOle = null;
            System.Data.SqlClient.SqlCommandBuilder builderSQL = null;
            System.Data.OleDb.OleDbCommandBuilder builderOLE = null;
            System.Data.Common.DbCommand cmdSqL = null;
            System.Data.Common.DbCommand cmdOle = null;

            DataSet dataSet = new DataSet();
            DataTable Temp = new DataTable();

            string ConnectionString = DBReadExecute.ParseConnectionString(pFileString);

            try
            {
                StringBuilder InsertSQL = new StringBuilder();
                StringBuilder ValueSQL = new StringBuilder();

                //switch (DBReadExecute.DataSource)
                Type SQLServerType = Type.GetType("Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer");
                if (DBReadExecute.DataSource.GetType().AssemblyQualifiedName == SQLServerType.AssemblyQualifiedName)
                {

                    //case DBReadExecute.enumDataSouce.SQLServer:
                        Conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
                        Adapter = new System.Data.SqlClient.SqlDataAdapter(pSelectSQL, Conn);
                        //Adapter.FillSchema(dataSet, SchemaType.Source, pDestinationTableName);
                        Adapter.FillSchema(dataSet, SchemaType.Source);
                        builderSQL = new System.Data.SqlClient.SqlCommandBuilder(Adapter);
                        Conn.Open();
                        cmdSqL = Conn.CreateCommand();
                        cmdSqL = builderSQL.GetInsertCommand(true);
                        cmdSqL.CommandTimeout = 1500;


                        InsertSQL.Append("Insert Into ");
                        InsertSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                        InsertSQL.Append(" (");
                        ValueSQL.Append(" values (");
                        foreach (System.Data.SqlClient.SqlParameter param in cmdSqL.Parameters)
                        {
                            //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                            string FieldName = param.SourceColumn;

                            try
                            {

                                StringBuilder TInsertSQL = new StringBuilder();
                                StringBuilder TValueSQL = new StringBuilder();

                                if (pDataReader[FieldName] != DBNull.Value && !string.IsNullOrEmpty(pDataReader[FieldName].ToString()))
                                {


                                    TInsertSQL.Append("[");
                                    TInsertSQL.Append(FieldName);
                                    TInsertSQL.Append("],");

                                    if (pDataReader[FieldName] == DBNull.Value)
                                    {
                                        TValueSQL.Append("null");
                                    }
                                    else
                                    {
                                        switch (pDataReader[FieldName].GetType().ToString())
                                        {

                                            case "System.Boolean":
                                                if (Convert.ToBoolean(pDataReader[FieldName]) == false)
                                                {
                                                    TValueSQL.Append("0");
                                                }
                                                else
                                                {
                                                    TValueSQL.Append("1");
                                                }
                                                break;
                                            case "System.Int32":
                                            case "System.Decimal":
                                            case "System.Double":
                                            case "System.Single":
                                            case "System.Byte":
                                                TValueSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                                break;
                                            default:
                                                TValueSQL.Append("'");
                                                TValueSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                                TValueSQL.Append("'");
                                                break;
                                        }
                                    }

                                    TValueSQL.Append(",");

                                }

                                InsertSQL.Append(TInsertSQL);
                                ValueSQL.Append(TValueSQL);
                            }
                            catch (Exception ex)
                            {
                                // do nothing for now
                            }
                        }


                        InsertSQL.Length = InsertSQL.Length - 1;
                        ValueSQL.Length = ValueSQL.Length - 1;
                        InsertSQL.Append(")");
                        ValueSQL.Append(")");
                        InsertSQL.Append(ValueSQL);
                        builderSQL = null;
                        cmdSqL = null;
                        cmdSqL = Conn.CreateCommand();
                        cmdSqL.CommandText = InsertSQL.ToString();

                        //DBReadExecute.ExecuteSQL(pFileString, InsertSQL.ToString());
                        cmdSqL.ExecuteNonQuery();
                }
                else
                {
                    //    break;
                    //case DBReadExecute.enumDataSouce.MSAccess:
                    //case DBReadExecute.enumDataSouce.MSAccess2007:
                    //case DBReadExecute.enumDataSouce.MSExcel:
                    //case DBReadExecute.enumDataSouce.MSExcel2007:
                        ConnOle = new System.Data.OleDb.OleDbConnection(ConnectionString);
                        AdapterOle = new System.Data.OleDb.OleDbDataAdapter(pSelectSQL, ConnOle);
                        //Adapter.FillSchema(dataSet, SchemaType.Source, pDestinationTableName);
                        AdapterOle.FillSchema(dataSet, SchemaType.Source);
                        AdapterOle.Fill(Temp);
                        builderOLE = new System.Data.OleDb.OleDbCommandBuilder();
                        builderOLE.DataAdapter = AdapterOle;

                        ConnOle.Open();
                        cmdOle = ConnOle.CreateCommand();
                        cmdOle = builderOLE.GetInsertCommand();
                        cmdOle.CommandTimeout = 1500;


                        InsertSQL.Append("Insert Into ");
                        InsertSQL.Append(pSelectSQL.Replace("Select * From ", ""));
                        InsertSQL.Append(" (");
                        ValueSQL.Append(" values (");
                        foreach (System.Data.OleDb.OleDbParameter param in cmdOle.Parameters)
                        {
                            //string FieldName = param.ParameterName.TrimStart(new char[] { '@' });
                            try
                            {

                                StringBuilder TInsertSQL = new StringBuilder();
                                StringBuilder TValueSQL = new StringBuilder();

                                string FieldName = param.SourceColumn;
                                for (int i = 0; i < pDataReader.FieldCount; i++)
                                {
                                    if (pDataReader.GetName(i).Equals(FieldName, StringComparison.OrdinalIgnoreCase))
                                    {

                                        TInsertSQL.Append("[");
                                        TInsertSQL.Append(FieldName);
                                        TInsertSQL.Append("],");

                                        if (pDataReader[FieldName] == DBNull.Value)
                                        {
                                            TValueSQL.Append("null");
                                        }
                                        else
                                        {
                                            switch (pDataReader[FieldName].GetType().ToString())
                                            {

                                                case "System.Int32":
                                                case "System.Decimal":
                                                case "System.Boolean":
                                                case "System.Double":
                                                case "System.Single":
                                                case "System.Byte":
                                                    TValueSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                                    break;
                                                default:
                                                    TValueSQL.Append("'");
                                                    TValueSQL.Append(pDataReader[FieldName].ToString().Replace("'", "''"));
                                                    TValueSQL.Append("'");
                                                    break;
                                            }
                                        }


                                        TValueSQL.Append(",");

                                        InsertSQL.Append(TInsertSQL);
                                        ValueSQL.Append(TValueSQL);

                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // do nothing for now
                            }

                        }
                        InsertSQL.Length = InsertSQL.Length - 1;
                        ValueSQL.Length = ValueSQL.Length - 1;
                        InsertSQL.Append(")");
                        ValueSQL.Append(")");
                        InsertSQL.Append(ValueSQL);
                        builderOLE = null;
                        cmdOle = null;
                        cmdOle = ConnOle.CreateCommand();
                        cmdOle.CommandText = InsertSQL.ToString();

                        //DBReadExecute.ExecuteSQL(pFileString, InsertSQL.ToString());

                        cmdOle.ExecuteNonQuery();
                        //break;
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log(DateTime.Now + ":  " + ex.Message);
            }
            finally
            {
                if (Conn != null)
                {
                    Conn.Close();
                }

                if (ConnOle != null)
                {
                    ConnOle.Close();
                }

            }

            result = true;
            return result;
        }



        private object MergeNonEpi7WithEpi7(View pView)
        {
            object result = null;

            string[] KeySet = this.KeyString.ToUpperInvariant().Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder KeyMatch = new StringBuilder();


            int NumberInserted = 0;
            int NumberUpdated = 0;
            int StatusCount = 0;

            DataTable SourceTable = DBReadExecute.GetDataTable(this.FileName, "Select * From [" + this.Identifier + "]");
            foreach (DataRow row in SourceTable.Rows)
            {

                    StatusCount++;
                    if (StatusCount % 25 == 0)
                    {
                        Dictionary<string, string> args2 = new Dictionary<string, string>();
                        args2.Add("status", string.Format("merge in-progress: {0} of {1} records processed...", StatusCount, SourceTable.Rows.Count));
                        this.Context.AnalysisCheckCodeInterface.DisplayStatusMessage(args2);
                    }

                    KeyMatch.Length = 0;
                    for (int i = 0; i < KeySet.Length; i++)
                    {
                        string[] temp = KeySet[i].Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                        if (i != 0)
                        {
                            KeyMatch.Append(" And ");
                        }

                        switch (temp[0].Trim().ToUpperInvariant())
                        {
                            case "GLOBALRECORDID":
                            case "RECSTATUS":
                            case "FKEY":
                            case "FIRSTSAVETIME":
                            case "LASTSAVETIME":
                            case "FIRSTSAVELOGONNAME":
                            case "LASTSAVELOGONNAME":
                                KeyMatch.Append("t.[");
                                break;
                            default:
                                KeyMatch.Append("[");
                                break;
                        }
                        KeyMatch.Append(temp[0].Trim());
                        KeyMatch.Append("]");
                        switch (row[temp[1].Trim()].GetType().ToString())
                        {
                            case "System.Int32":
                            case "System.Decimal":
                            case "System.Boolean":
                            case "System.Double":
                            case "System.Single":
                                KeyMatch.Append(" = ");
                                KeyMatch.Append(row[temp[1].Trim()]);
                                break;
                            default:
                                KeyMatch.Append(" = '");
                                KeyMatch.Append(row[temp[1].Trim()]);
                                KeyMatch.Append("'");
                                break;
                        }
                    }

                    DataTable DestinationTable = DBReadExecute.GetDataTable(this.Context.CurrentRead.File, "Select t.UniqueKey " + pView.FromViewSQL + " Where " + KeyMatch.ToString());
                    if (DestinationTable.Rows.Count == 1)
                    {
                        int CurrentRecord = int.Parse(DestinationTable.Rows[0]["UniqueKey"].ToString());
                        pView.LoadRecord(CurrentRecord);
                        foreach (DataColumn column in SourceTable.Columns)
                        {
                            if (pView.Fields.Exists(column.ColumnName))
                            {
                                Epi.Fields.IDataField dataField = (Epi.Fields.IDataField) pView.Fields[column.ColumnName];

                                dataField.CurrentRecordValueObject = row[column.ColumnName];
                            }
                        }
                        pView.SaveRecord(CurrentRecord);
                        NumberUpdated++;
                        
                    }
                    else if (DestinationTable.Rows.Count == 0)
                    {
                        pView.CurrentGlobalRecordId = Guid.NewGuid().ToString();

                        foreach (Epi.Fields.IDataField dataField in pView.Fields.DataFields)
                        {
                            if (dataField is Epi.Fields.GlobalRecordIdField)
                            {
                                ((Epi.Fields.GlobalRecordIdField)dataField).NewValue();
                            }
                            else if (!(dataField is Epi.Fields.ForeignKeyField))
                            {
                                dataField.CurrentRecordValueObject = null;
                            }
                        }

                        
                        foreach (DataColumn column in SourceTable.Columns)
                        {
                            if (pView.Fields.Exists(column.ColumnName))
                            {
                                Epi.Fields.IDataField dataField = (Epi.Fields.IDataField)pView.Fields[column.ColumnName];

                                dataField.CurrentRecordValueObject = row[column.ColumnName];
                            }
                        }
                        pView.SaveRecord();
                        NumberInserted++;
                    }
            }

            this.Context.DataTableRefreshNeeded = true;

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", CommandNames.MERGE);
            args.Add("DESTINATIONFILENAME", this.Context.CurrentRead.File);
            args.Add("DESTINATIONTABLENAME", this.Context.CurrentRead.Identifier);
            args.Add("FROMFILENAME", this.FileName);
            args.Add("FROMTABLENAME", this.Identifier);
            args.Add("NUMBERINSERTED", NumberInserted.ToString());
            args.Add("NUMBERUPDATED", NumberUpdated.ToString());

            this.Context.AnalysisCheckCodeInterface.Display(args);



            result = true;
            return result;
        }

    }
}
/*
!***			Merge Statement 			***!
<Merge_Table_Statement> 						::= MERGE Identifier <KeyDef> <MergeOpt>
!<Merge_Db_Table_Statement> 					::= MERGE <ReadOpt> File <LinkName> <KeyDef> <MergeOpt> <FileSpec>
<Merge_Db_Table_Statement> 					::= MERGE <ReadOpt> File ':' Identifier <LinkName> <KeyDef> <MergeOpt> <FileSpec>
<Merge_File_Statement> 						::= MERGE <ReadOpt> File <LinkName> <KeyDef> <MergeOpt> <FileSpec>
<Merge_Excel_File_Statement> 					::= MERGE <ReadOpt> File ExcelRange <LinkName> <KeyDef> <MergeOpt> <FileSpec>

<MergeOpt> 							::= APPEND
		 						| UPDATE
		 						| RELATE
		 						| !Null

!***			End					***!
 */