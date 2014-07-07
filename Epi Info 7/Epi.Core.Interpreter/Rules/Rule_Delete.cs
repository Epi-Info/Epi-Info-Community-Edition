using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using System.IO;
//using System.Text.RegularExpressions;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Data.Services;
using Epi.DataSets;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Delete : AnalysisRule
    {
        protected string filePath = string.Empty;
        protected string identifier = string.Empty;
        protected List<string> deleteOptions = new List<string>();
        protected bool runSilent = false;
        protected bool saveData = false;
        protected bool permanent = false;
        protected bool ignoreDeleted = true;
        protected string dbType = string.Empty;
        protected string commandText = string.Empty;

        public Rule_Delete(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            commandText = this.ExtractTokens(pToken.Tokens);
        }

        public override object Execute()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
    //!*** 			Delete Statement 		***!
    //<Delete_File_Statement>	 				::= DELETE File <DeleteOpts>

    //<Delete_Table_Statement>	 				::= DELETE TABLES Identifier <DeleteOpts>
    //<Delete_Table_Short_Statement> 			::= DELETE TABLES File <DeleteOpts>
    //<Delete_Table_Long_Statement> 			::= DELETE TABLES File ':' Identifier <DeleteOpts>

    //<Delete_Records_All_Statement> 				::= DELETE '*' <DeleteOpts>
    //<Delete_Records_Selected_Statement> 			::= DELETE <Compare Exp> <DeleteOpts>

    //<DeleteOpts> 							    ::= <DeleteOpts> <DeleteOpt>
    //                                            | <DeleteOpt>
    //                                            | !Null

    //<DeleteOpt>							    ::= PERMANENT
    //                                            | SAVEDATA
    //                                            | RUNSILENT



    /// <summary>
    /// Delete_Records_All_Statement 				::= DELETE '*' DeleteOpts
    /// </summary>
    public class Rule_Delete_Records_All_Statement : Rule_Delete
    {
        public Rule_Delete_Records_All_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken)
        {
            if (!Util.IsEmpty(this.Context.CurrentRead))
            {
                this.filePath = this.Context.CurrentRead.File;
            }
            if (pToken.Tokens.Length > 2)
            {
                if (pToken.Tokens.Length > 2)
                {
                    this.deleteOptions.AddRange(this.GetCommandElement(pToken.Tokens, 2).ToUpper().Split(StringLiterals.SPACE.ToCharArray()));
                    this.runSilent = deleteOptions.Contains("RUNSILENT");
                    this.saveData = deleteOptions.Contains("SAVEDATA");
                    this.permanent = deleteOptions.Contains("PERMANENT");
                }
            }
        }

        private string BuildSQLExpression(string expression)
        {
            string retval = expression.Replace("(+)", "true");
            retval = retval.Replace("(-)", "0");
            retval = retval.Replace("<> (.)", "is not null");
            retval = retval.Replace("<>(.)", "is not null");
            retval = retval.Replace("= (.)", "is null");
            retval = retval.Replace("=(.)", "is null");

            if (dbType == "Epi.Data.Office.AccessDatabase")
            {
                string[] expParts = expression.Split(' ');
                //string DatePattern = @"^(0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])[- /.](19|20)\d\d$";
                //Regex r = new Regex(DatePattern);
                foreach (string i in expParts)
                {
                    string ii = i.Trim(new char[]{'(', ')'});
                    DateTime date1;
                    if (DateTime.TryParse(ii, out date1))
                    {
                        retval = retval.Replace(ii, Util.InsertIn(ii, StringLiterals.HASH));
                    }
                }
            }
            return retval;
        }

        /// <summary>
        /// performs the Delete All records statments
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object results = null;
            
            int rowCount;
            string whereClause = string.Empty;
            StringBuilder selectExpression = new StringBuilder(this.Context.SelectString.ToString());
            bool confirmDelete = true;
            object delResult = false;
            string prompt = string.Empty;
            string FromClause = string.Empty;
            StringBuilder GlobalRecordIdList = new StringBuilder();

            bool isEpiView = this.Context.CurrentRead.IsEpi7ProjectRead && this.Context.CurrentProject != null && this.Context.CurrentProject.Views.Exists(this.Context.CurrentRead.Identifier);

            IDbDriver dbDriver = DBReadExecute.GetDataDriver(this.Context.CurrentRead.File);
            dbType = dbDriver.GetType().ToString();

           if (!Util.IsEmpty(selectExpression))
           {
               whereClause = " " + BuildSQLExpression(selectExpression.ToString());
           }

           if (isEpiView)
           {
               //whereClause = this.Context.DataInfo.GetSqlStatementPartWhere();
               if (whereClause.Length > 0)
               {
                   whereClause += string.Format(" And t.{0} = 1 ", ColumnNames.REC_STATUS);
               }
               else
               {
                   whereClause = string.Format(" t.{0} = 1 ", ColumnNames.REC_STATUS);
               }

               FromClause = this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier].FromViewSQL;
               FromClause = FromClause.Remove(0, 5);
           }
           else
           {
               FromClause = this.Context.CurrentRead.Identifier;
           }
            

            Query qr;
            if (isEpiView)
            {
                if (string.IsNullOrEmpty(whereClause))
                {
                    qr = dbDriver.CreateQuery(string.Format("Select count(*) from {0}", FromClause));
                }
                else
                {
                    qr = dbDriver.CreateQuery(string.Format("Select count(*) from {0} Where {1} ", FromClause, whereClause));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(whereClause))
                {
                    qr = dbDriver.CreateQuery(string.Format("Select count(*) from [{0}]", FromClause));
                }
                else
                {
                    qr = dbDriver.CreateQuery(string.Format("Select count(*) from [{0}] Where {1} ", FromClause, whereClause));
                }
            }
            rowCount = (int)dbDriver.ExecuteScalar(qr);
            prompt = String.Format((permanent ? "Permanently " : string.Empty) + SharedStrings.CONFIRM_DELETE_RECORDS, rowCount);


            if (rowCount > 0)
            {
                if (!runSilent)
                {
                    this.Context.AnalysisCheckCodeInterface.Dialog(prompt, ref delResult, "YesNo", SharedStrings.ANALYSIS);
                    if (!(bool)delResult)
                    {
                        confirmDelete = false;
                    }
                }
            }
            else
            {
                confirmDelete = false;
            }

            if (confirmDelete)
            {
                if (permanent)
                {
                    if (isEpiView)
                    {

                        if (string.IsNullOrEmpty(whereClause))
                        {
                            qr = dbDriver.CreateQuery(string.Format("Select t.UniqueKey as UniqueKey from {0} ", FromClause));
                        }
                        else
                        {
                            qr = dbDriver.CreateQuery(string.Format("Select t.UniqueKey as UniqueKey from {0} Where {1} ", FromClause, whereClause));
                        }
                        IDataReader dr = dbDriver.ExecuteReader(qr);
                        while (dr.Read())
                        {
                            rowCount++;
                            this.CascadePermanentDelete((int)dr["UniqueKey"]);
                        }

/*
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            qr = dbDriver.CreateQuery(string.Format("Select t.UniqueKey, t.GlobalRecordId As GlobalRecordId from {0} ", FromClause));
                        }
                        else
                        {
                            qr = dbDriver.CreateQuery(string.Format("Select t.UniqueKey, T.GlobalRecordId As GlobalRecordId  from {0} Where {1} ", FromClause, whereClause));
                        }
                        IDataReader dr = dbDriver.ExecuteReader(qr);
                        while (dr.Read())
                        {
                            IDList.Add((int)dr["GlobalRecordId"]);

                            GlobalRecordIdList.Append("'");
                            GlobalRecordIdList.Append(dr["GlobalRecordId"].ToString());
                            GlobalRecordIdList.Append("',");
                        }
                        if (GlobalRecordIdList.Length > 0)
                        {
                            GlobalRecordIdList.Length = GlobalRecordIdList.Length - 1;
                        }

                       

                        
                        foreach (Epi.Page page in this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier].Pages)
                        {
                            if (string.IsNullOrEmpty(whereClause))
                            {
                                qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] ", page.TableName));
                            }
                            else
                            {
                                qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] Where GlobalRecordId in ({2}) ", page.TableName, whereClause, GlobalRecordIdList.ToString()));
                            }
                            dbDriver.ExecuteNonQuery(qr);
                        }


                        if (string.IsNullOrEmpty(whereClause))
                        {
                            qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] ", this.Context.CurrentRead.Identifier));
                        }
                        else
                        {
                            qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] Where GlobalRecordId in ({1}) ", this.Context.CurrentRead.Identifier, GlobalRecordIdList.ToString()));
                        }
                        rowCount = dbDriver.ExecuteNonQuery(qr);*/
                        dbDriver.Dispose();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] ", this.Context.CurrentRead.Identifier));
                        }
                        else
                        {
                            qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] Where {1} ", this.Context.CurrentRead.Identifier, whereClause));
                        }
                        rowCount = dbDriver.ExecuteNonQuery(qr);
                        dbDriver.Dispose();
                    }
                    
                    
                    Rule_Read.RemoveVariables = false;
                    this.Context.CurrentRead.Execute();
                }
                else if (isEpiView)
                {
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        qr = dbDriver.CreateQuery(string.Format("Select t.UniqueKey as UniqueKey from {0} ", FromClause));
                    }
                    else
                    {
                        qr = dbDriver.CreateQuery(string.Format("Select t.UniqueKey as UniqueKey from {0} Where {1} ", FromClause, whereClause));
                    }
                    IDataReader dr = dbDriver.ExecuteReader(qr);
                    List<int> IDList = new List<int>();
                    while (dr.Read())
                    {
                        rowCount++;
                        this.MarkRelatedRecoredsAsDeleted((int)dr["UniqueKey"]);
                    }
                    /*
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        qr = dbDriver.CreateQuery(string.Format("Select t.GlobalRecordId As GlobalRecordId from {0} ", FromClause));
                    }
                    else
                    {
                        qr = dbDriver.CreateQuery(string.Format("Select T.GlobalRecordId As GlobalRecordId  from {0} Where {1} ", FromClause, whereClause));
                    }
                    IDataReader dr = dbDriver.ExecuteReader(qr);
                    while (dr.Read())
                    {
                        GlobalRecordIdList.Append("'");
                        GlobalRecordIdList.Append(dr["GlobalRecordId"].ToString());
                        GlobalRecordIdList.Append("',");
                    }
                    if (GlobalRecordIdList.Length > 0)
                    {
                        GlobalRecordIdList.Length = GlobalRecordIdList.Length - 1;
                    }

                    if (string.IsNullOrEmpty(whereClause))
                    {
                        qr = dbDriver.CreateQuery(string.Format("Update [{0}] Set {1} = 0 ", this.Context.CurrentRead.Identifier, ColumnNames.REC_STATUS));
                    }
                    else
                    {
                        qr = dbDriver.CreateQuery(string.Format("Update [{0}] Set {1} = 0 Where GlobalRecordId in ({2}) ", this.Context.CurrentRead.Identifier, ColumnNames.REC_STATUS, GlobalRecordIdList));

                    }
                    rowCount = dbDriver.ExecuteNonQuery(qr);
                    dbDriver.Dispose();*/
                    Rule_Read.RemoveVariables = false;
                    this.Context.CurrentRead.Execute();
                }
                else
                {

                    rowCount = 0;
                    DataRow[] Rows = this.Context.DataSet.Tables["output"].Select(whereClause);
                    foreach (DataRow row in Rows)
                    {
                        rowCount++;
                        row.Delete();
                    }

                    this.Context.DataSet.AcceptChanges();
                }

            }
            else
            {
                rowCount = 0;
            }

            if (permanent && rowCount > 0)
            {
                this.Context.DataTableRefreshNeeded = true;
            }

            string resultcount = string.Format(SharedStrings.NUMBER_RECORDS_DELETED, rowCount);
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", CommandNames.DELETE);
            args.Add("COMMANDTEXT", this.commandText.Trim());
            args.Add("TABLENAME", this.Context.CurrentRead.Identifier);
            args.Add("HTMLRESULTS", resultcount);
            this.Context.AnalysisCheckCodeInterface.Display(args);

            return results;
        }


        private void MarkRelatedRecoredsAsDeleted(Epi.Data.Services.CollectedDataProvider OutputDriver, View pView, List<int> pIDList)
        {
            Dictionary<string, bool> VisitedViews = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string SQL = null;
            StringBuilder InSQL = new StringBuilder();
            InSQL.Append(" Where FKey In (");
            foreach (int ID in pIDList)
            {
                pView.LoadRecord(ID);
                pView.RecStatusField.CurrentRecordValue = 0;
                pView.SaveRecord(ID);
                InSQL.Append("'");
                InSQL.Append(pView.CurrentGlobalRecordId);
                InSQL.Append("',");
            }
            InSQL.Length = InSQL.Length - 1;
            InSQL.Append(")");

            foreach (Epi.Fields.Field field in pView.Fields)
            {
                if (field is Epi.Fields.RelatedViewField)
                {
                    Epi.Fields.RelatedViewField rvf = field as Epi.Fields.RelatedViewField;
                    if (!VisitedViews.ContainsKey(rvf.ChildView.Name))
                    {
                        SQL = "Select UniqueKey From [" + rvf.ChildView.TableName + "] " + InSQL.ToString();
                        IDataReader reader = OutputDriver.ExecuteReader(OutputDriver.CreateQuery(SQL));
                        List<int> NewIdList = new List<int>();
                        while (reader.Read())
                        {
                            if (reader["UniqueKey"] != DBNull.Value)
                            {
                                NewIdList.Add((int)reader["UniqueKey"]);
                            }
                        }
                        VisitedViews.Add(rvf.ChildView.Name, true);
                        MarkRelatedRecoredsAsDeleted(OutputDriver, rvf.ChildView, NewIdList);
                    }
                }
            }
        }

        private void MarkRelatedRecoredsAsDeleted(int pUniqueKey)
        {
            List<int> IDList = new List<int>();
            IDList.Add(pUniqueKey);
            Epi.Data.Services.CollectedDataProvider OutputDriver = this.Context.CurrentProject.CollectedData;// Epi.Data.DBReadExecute.GetDataDriver(this.project.CollectedDataConnectionString, true);
            MarkRelatedRecoredsAsDeleted(OutputDriver, this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier], IDList);
        }


        private void UnMarkRelatedRecoredsAsDeleted(Epi.Data.Services.CollectedDataProvider OutputDriver, View pView, List<int> pIDList)
        {
            Dictionary<string, bool> VisitedViews = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string SQL = null;
            StringBuilder InSQL = new StringBuilder();
            InSQL.Append(" Where FKey In (");
            foreach (int ID in pIDList)
            {
                pView.LoadRecord(ID);
                pView.RecStatusField.CurrentRecordValue = 1;
                pView.SaveRecord(ID);
                InSQL.Append("'");
                InSQL.Append(pView.CurrentGlobalRecordId);
                InSQL.Append("',");
            }
            InSQL.Length = InSQL.Length - 1;
            InSQL.Append(")");

            foreach (Epi.Fields.Field field in pView.Fields)
            {
                if (field is Epi.Fields.RelatedViewField)
                {
                    Epi.Fields.RelatedViewField rvf = field as Epi.Fields.RelatedViewField;
                    if (!VisitedViews.ContainsKey(rvf.ChildView.Name))
                    {
                        SQL = "Select UniqueKey From [" + rvf.ChildView.TableName + "] " + InSQL.ToString();
                        IDataReader reader = OutputDriver.ExecuteReader(OutputDriver.CreateQuery(SQL));
                        List<int> NewIdList = new List<int>();
                        while (reader.Read())
                        {
                            if (reader["UniqueKey"] != DBNull.Value)
                            {
                                NewIdList.Add((int)reader["UniqueKey"]);
                            }
                        }
                        VisitedViews.Add(rvf.ChildView.Name, true);
                        UnMarkRelatedRecoredsAsDeleted(OutputDriver, rvf.ChildView, NewIdList);
                    }
                }
            }
        }

        private void UnMarkRelatedRecoredsAsDeleted(int pUniqueKey)
        {
            List<int> IDList = new List<int>();
            IDList.Add(pUniqueKey);
            Epi.Data.Services.CollectedDataProvider OutputDriver = this.Context.CurrentProject.CollectedData;// Epi.Data.DBReadExecute.GetDataDriver(this.project.CollectedDataConnectionString, true);
            UnMarkRelatedRecoredsAsDeleted(OutputDriver, this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier], IDList);
        }

        private void CascadePermanentDelete(Epi.Data.Services.CollectedDataProvider OutputDriver, View pView, List<int> pIDList)
        {
            Dictionary<string, bool> VisitedViews = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string SQL = null;
            StringBuilder InSQL = new StringBuilder();
            InSQL.Append(" (");
            foreach (int ID in pIDList)
            {
                pView.LoadRecord(ID);
                InSQL.Append("'");
                InSQL.Append(pView.CurrentGlobalRecordId);
                InSQL.Append("',");
            }
            InSQL.Length = InSQL.Length - 1;
            InSQL.Append(")");

            foreach (Epi.Fields.Field field in pView.Fields)
            {
                if (field is Epi.Fields.RelatedViewField)
                {
                    Epi.Fields.RelatedViewField rvf = field as Epi.Fields.RelatedViewField;
                    if (!VisitedViews.ContainsKey(rvf.ChildView.Name))
                    {
                        SQL = "Select UniqueKey From [" + rvf.ChildView.TableName + "] Where FKey In " + InSQL.ToString();
                        IDataReader reader = OutputDriver.ExecuteReader(OutputDriver.CreateQuery(SQL));
                        List<int> NewIdList = new List<int>();
                        while (reader.Read())
                        {
                            if (reader["UniqueKey"] != DBNull.Value)
                            {
                                NewIdList.Add((int)reader["UniqueKey"]);
                            }
                        }
                        VisitedViews.Add(rvf.ChildView.Name, true);
                        CascadePermanentDelete(OutputDriver, rvf.ChildView, NewIdList);
                    }
                }
                else if(field is Epi.Fields.GridField)
                {

                    Epi.Fields.GridField gf = field as Epi.Fields.GridField;

                    SQL = "Delete from  [" + gf.TableName + "]  Where FKey In " + InSQL.ToString();
                    OutputDriver.ExecuteNonQuery(OutputDriver.CreateQuery(SQL));

                }
            }

            foreach (Epi.Page page in pView.Pages)
            {
                SQL = "Delete from  [" + page.TableName + "]  Where GlobalRecordId In " + InSQL.ToString();
                OutputDriver.ExecuteNonQuery(OutputDriver.CreateQuery(SQL));
            }

            SQL = "Delete from  [" + pView.TableName + "]  Where GlobalRecordId In " + InSQL.ToString();
            OutputDriver.ExecuteNonQuery(OutputDriver.CreateQuery(SQL));
        }

        private void CascadePermanentDelete(int pUniqueKey)
        {
            List<int> IDList = new List<int>();
            IDList.Add(pUniqueKey);
            Epi.Data.Services.CollectedDataProvider OutputDriver = this.Context.CurrentProject.CollectedData;// Epi.Data.DBReadExecute.GetDataDriver(this.project.CollectedDataConnectionString, true);
            CascadePermanentDelete(OutputDriver, this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier], IDList);
        }

    }

    /// <summary>
    /// Delete_Records_Selected_Statement 			::= DELETE CompareExp DeleteOpts
    /// </summary>
    public class Rule_Delete_Records_Selected_Statement : Rule_Delete
    {
        private string compareExp = string.Empty;

        public Rule_Delete_Records_Selected_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken)
        {
            if (!Util.IsEmpty(this.Context.CurrentRead))
            {
                this.filePath = this.Context.CurrentRead.File;
            }

            for(int i = 1; i < pToken.Tokens.Length; i++)
            {
                if (pToken.Tokens[i] is NonterminalToken)
                {
                    NonterminalToken T = (NonterminalToken)pToken.Tokens[i];

                    switch (T.Symbol.ToString())
                    {

                        case "<Value>":


                        case "<Compare Exp>":
                        
                            this.compareExp = this.GetCommandElement(pToken.Tokens, i);
                            break;
                        case "<DeleteOpts>":
                            this.deleteOptions.AddRange(this.GetCommandElement(pToken.Tokens, i).ToUpper().Split(StringLiterals.SPACE.ToCharArray()));
                            this.runSilent = deleteOptions.Contains("RUNSILENT");
                            this.saveData = deleteOptions.Contains("SAVEDATA");
                            this.permanent = deleteOptions.Contains("PERMANENT");
                            break;
                        case "<DeleteOpt>":
                            switch (this.GetCommandElement(T.Tokens, 0))
                            {
                                case "RUNSILENT":
                                    this.runSilent = true;
                                    break;
                                case "SAVEDATA":
                                    this.saveData = true;
                                    break;
                                case "PERMANENT":
                                    this.permanent = true;
                                    break;
                            }
                            break;
                    }
                }
                else
                {

                }
            }
        }


        /// <summary>
        /// performs the delete command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object results = null;

            int rowCount;
            StringBuilder selectExpression = new StringBuilder(this.Context.SelectString.ToString());
            //StringBuilder whereClause = new StringBuilder();
            string whereClause = string.Empty;
            bool confirmDelete = true;
            object delResult = false;
            string prompt = string.Empty;
            string FromClause = string.Empty;
            StringBuilder GlobalRecordIdList = new StringBuilder();

            IDbDriver dbDriver = DBReadExecute.GetDataDriver(this.Context.CurrentRead.File);
            dbType = dbDriver.GetType().ToString();

            bool isEpiView = this.Context.CurrentRead.IsEpi7ProjectRead && this.Context.CurrentProject != null && this.Context.CurrentProject.Views.Exists(this.Context.CurrentRead.Identifier);
            View EpiView = null;
            if (isEpiView)
            {
                EpiView = this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier];
            }

            if (!Util.IsEmpty(selectExpression))
            {
                whereClause = BuildSQLExpression(selectExpression.ToString());

                if (compareExp.Length > 0)
                {
                    whereClause += " AND " + BuildSQLExpression(compareExp);
                }
            }
            else
            {
                

                if (compareExp.Length > 0)
                {
                    whereClause = " ";

                    whereClause += BuildSQLExpression(compareExp);
                }

            }


            if (isEpiView)
            {
                //whereClause = this.Context.DataInfo.GetSqlStatementPartWhere();
                if (!whereClause.ToUpper().Contains("[RECSTATUS]"))
                {
                    if (whereClause.Length > 0)
                    {
                        whereClause += string.Format(" AND t.{0} = 1 ", ColumnNames.REC_STATUS);
                    }
                    else
                    {
                        whereClause = string.Format(" t.{0} = 1 ", ColumnNames.REC_STATUS);
                    }
                }

                FromClause = EpiView.FromViewSQL;
                FromClause = FromClause.Remove(0, 5);
            }
            else
            {
                FromClause = this.Context.CurrentRead.Identifier;
            }
            
            Query qr;
            if (isEpiView)
            {
                if (string.IsNullOrEmpty(whereClause))
                {
                    qr = dbDriver.CreateQuery(string.Format("Select count(*) from {0}", FromClause));
                }
                else
                {
                    qr = dbDriver.CreateQuery(string.Format("Select count(*) from {0} Where {1} ", FromClause, whereClause));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(whereClause))
                {
                    qr = dbDriver.CreateQuery(string.Format("Select count(*) from [{0}]", FromClause));
                }
                else
                {
                    qr = dbDriver.CreateQuery(string.Format("Select count(*) from {0} Where {1} ", FromClause, whereClause));
                }
            }
            rowCount = (int)dbDriver.ExecuteScalar(qr);
            prompt = String.Format((permanent ? "Permanently ":string.Empty) + SharedStrings.CONFIRM_DELETE_RECORDS, rowCount);

            if (rowCount > 0)
            {
                if (!runSilent)
                {
                    this.Context.AnalysisCheckCodeInterface.Dialog(prompt, ref delResult, "YesNo", SharedStrings.ANALYSIS);
                    if (!(bool)delResult)
                    {
                        confirmDelete = false;
                    }
                }
            }
            else
            {
                confirmDelete = false;
            }


            rowCount = 0;

            if (confirmDelete)
            {
                if (permanent)
                {
                    if (isEpiView)
                    {

                        if (string.IsNullOrEmpty(whereClause))
                        {
                            qr = dbDriver.CreateQuery(string.Format("Select t.UniqueKey As UniqueKey from {0} ", FromClause));
                        }
                        else
                        {
                            qr = dbDriver.CreateQuery(string.Format("Select T.UniqueKey As UniqueKey  from {0} Where {1} ", FromClause, whereClause));
                        }
                        IDataReader dr = dbDriver.ExecuteReader(qr);
                        while (dr.Read())
                        {
                            rowCount++;
                            this.CascadePermanentDelete((int)dr["UniqueKey"]);
                        }
                        /*
                        IDataReader dr = dbDriver.ExecuteReader(qr);
                        while (dr.Read())
                        {
                            GlobalRecordIdList.Append("'");
                            GlobalRecordIdList.Append(dr["GlobalRecordId"].ToString());
                            GlobalRecordIdList.Append("',");
                        }
                        if (GlobalRecordIdList.Length > 0)
                        {
                            GlobalRecordIdList.Length = GlobalRecordIdList.Length - 1;
                        }

                        foreach (Epi.Page page in EpiView.Pages)
                        {
                            if (string.IsNullOrEmpty(whereClause))
                            {
                                qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] ", page.TableName));
                            }
                            else
                            {
                                qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] Where GlobalRecordId in ({2}) ", page.TableName, whereClause, GlobalRecordIdList.ToString()));
                            }
                            dbDriver.ExecuteNonQuery(qr);
                        }


                        if (string.IsNullOrEmpty(whereClause))
                        {
                            qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] ", EpiView.TableName));
                        }
                        else
                        {
                            qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] Where GlobalRecordId in ({1}) ", EpiView.TableName, GlobalRecordIdList.ToString()));
                        }
                        rowCount = dbDriver.ExecuteNonQuery(qr);
                        dbDriver.Dispose();*/
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] ", this.Context.CurrentRead.Identifier));
                        }
                        else
                        {
                            qr = dbDriver.CreateQuery(string.Format("Delete from [{0}] Where {1} ", this.Context.CurrentRead.Identifier, whereClause));
                        }
                        rowCount = dbDriver.ExecuteNonQuery(qr);
                        dbDriver.Dispose();
                    }
                    Rule_Read.RemoveVariables = false;
                    this.Context.CurrentRead.Execute();
                }
                else if (isEpiView)
                {
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        qr = dbDriver.CreateQuery(string.Format("Select t.UniqueKey As UniqueKey from {0} ", FromClause));
                    }
                    else
                    {
                        qr = dbDriver.CreateQuery(string.Format("Select T.UniqueKey As UniqueKey  from {0} Where {1} ", FromClause, whereClause));
                    }
                    
                    IDataReader dr = dbDriver.ExecuteReader(qr);
                    List<int> IDList = new List<int>();
                    while (dr.Read())
                    {
                        rowCount++;
                        this.MarkRelatedRecoredsAsDeleted((int)dr["UniqueKey"]);
                    }

                    /*
                    IDataReader dr = dbDriver.ExecuteReader(qr);
                    while (dr.Read())
                    {
                        GlobalRecordIdList.Append("'");
                        GlobalRecordIdList.Append(dr["GlobalRecordId"].ToString());
                        GlobalRecordIdList.Append("',");
                    }
                    if (GlobalRecordIdList.Length > 0)
                    {
                        GlobalRecordIdList.Length = GlobalRecordIdList.Length - 1;
                    }


                    if (string.IsNullOrEmpty(whereClause))
                    {
                        qr = dbDriver.CreateQuery(string.Format("Update [{0}] Set {1} = 0 ", EpiView.TableName, ColumnNames.REC_STATUS));
                    }
                    else
                    {
                        qr = dbDriver.CreateQuery(string.Format("Update [{0}] Set {1} = 0 Where GlobalRecordId in ({2}) ", EpiView.TableName, ColumnNames.REC_STATUS, GlobalRecordIdList));

                    }
                    rowCount = dbDriver.ExecuteNonQuery(qr);
                    dbDriver.Dispose();*/
                    Rule_Read.RemoveVariables = false;
                    this.Context.CurrentRead.Execute();
                }
                else
                {

                    rowCount = 0;
                    DataRow[] Rows = this.Context.DataSet.Tables["output"].Select(whereClause);
                    foreach (DataRow row in Rows)
                    {
                        rowCount++;
                        row.Delete();
                    }

                    this.Context.DataSet.AcceptChanges();
                }

            }
            else
            {
                rowCount = 0;
            }

            if (permanent && rowCount > 0)
            {
                this.Context.DataTableRefreshNeeded = true;
            }

            

            string resultcount = string.Format(SharedStrings.NUMBER_RECORDS_DELETED, rowCount);
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", CommandNames.DELETE);
            args.Add("COMMANDTEXT", this.commandText.Trim());
            args.Add("TABLENAME", this.Context.CurrentRead.Identifier);
            args.Add("HTMLRESULTS", resultcount);
            this.Context.AnalysisCheckCodeInterface.Display(args);

            return results;
        }

        private string BuildSQLExpression(string expression)
        {           
            string retval = expression.Replace("(+)", "true");
            retval = retval.Replace("(-)", "0");
            retval = retval.Replace("<> (.)", "is not null");
            retval = retval.Replace("<>(.)", "is not null");
            retval = retval.Replace("= (.)", "is null");
            retval = retval.Replace("=(.)", "is null");

            if (dbType == "Epi.Data.Office.AccessDatabase")
            {
                string[] expParts = expression.Split(' ');
                foreach (string i in expParts)
                {
                    string ii = i.Trim(new char[] { '(', ')' });
                    DateTime date1;
                    if (DateTime.TryParse(ii, out date1))
                    {
                        retval = retval.Replace(ii, Util.InsertIn(ii, StringLiterals.HASH));
                    }
                }
            }
            return retval;
        }

        private void MarkRelatedRecoredsAsDeleted(Epi.Data.Services.CollectedDataProvider OutputDriver, View pView, List<int> pIDList)
        {
            Dictionary<string, bool> VisitedViews = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string SQL = null;
            StringBuilder InSQL = new StringBuilder();
            InSQL.Append(" Where FKey In (");
            foreach (int ID in pIDList)
            {
                pView.LoadRecord(ID);
                pView.RecStatusField.CurrentRecordValue = 0;
                pView.SaveRecord(ID);
                InSQL.Append("'");
                InSQL.Append(pView.CurrentGlobalRecordId);
                InSQL.Append("',");
            }
            InSQL.Length = InSQL.Length - 1;
            InSQL.Append(")");

            foreach (Epi.Fields.Field field in pView.Fields)
            {
                if (field is Epi.Fields.RelatedViewField)
                {
                    Epi.Fields.RelatedViewField rvf = field as Epi.Fields.RelatedViewField;
                    if (!VisitedViews.ContainsKey(rvf.ChildView.Name))
                    {
                        SQL = "Select UniqueKey From [" + rvf.ChildView.TableName + "] " + InSQL.ToString();
                        IDataReader reader = OutputDriver.ExecuteReader(OutputDriver.CreateQuery(SQL));
                        List<int> NewIdList = new List<int>();
                        while (reader.Read())
                        {
                            if (reader["UniqueKey"] != DBNull.Value)
                            {
                                NewIdList.Add((int)reader["UniqueKey"]);
                            }
                        }
                        VisitedViews.Add(rvf.ChildView.Name, true);
                        MarkRelatedRecoredsAsDeleted(OutputDriver, rvf.ChildView, NewIdList);
                    }
                }
            }
        }

        private void MarkRelatedRecoredsAsDeleted(int pUniqueKey)
        {
            List<int> IDList = new List<int>();
            IDList.Add(pUniqueKey);
            Epi.Data.Services.CollectedDataProvider OutputDriver = this.Context.CurrentProject.CollectedData;// Epi.Data.DBReadExecute.GetDataDriver(this.project.CollectedDataConnectionString, true);
            MarkRelatedRecoredsAsDeleted(OutputDriver, this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier], IDList);
        }


        private void UnMarkRelatedRecoredsAsDeleted(Epi.Data.Services.CollectedDataProvider OutputDriver, View pView, List<int> pIDList)
        {
            Dictionary<string, bool> VisitedViews = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string SQL = null;
            StringBuilder InSQL = new StringBuilder();
            InSQL.Append(" Where FKey In (");
            foreach (int ID in pIDList)
            {
                pView.LoadRecord(ID);
                pView.RecStatusField.CurrentRecordValue = 1;
                pView.SaveRecord(ID);
                InSQL.Append("'");
                InSQL.Append(pView.CurrentGlobalRecordId);
                InSQL.Append("',");
            }
            InSQL.Length = InSQL.Length - 1;
            InSQL.Append(")");

            foreach (Epi.Fields.Field field in pView.Fields)
            {
                if (field is Epi.Fields.RelatedViewField)
                {
                    Epi.Fields.RelatedViewField rvf = field as Epi.Fields.RelatedViewField;
                    if (!VisitedViews.ContainsKey(rvf.ChildView.Name))
                    {
                        SQL = "Select UniqueKey From [" + rvf.ChildView.TableName + "] " + InSQL.ToString();
                        IDataReader reader = OutputDriver.ExecuteReader(OutputDriver.CreateQuery(SQL));
                        List<int> NewIdList = new List<int>();
                        while (reader.Read())
                        {
                            if (reader["UniqueKey"] != DBNull.Value)
                            {
                                NewIdList.Add((int)reader["UniqueKey"]);
                            }
                        }
                        VisitedViews.Add(rvf.ChildView.Name, true);
                        UnMarkRelatedRecoredsAsDeleted(OutputDriver, rvf.ChildView, NewIdList);
                    }
                }
            }
        }

        private void UnMarkRelatedRecoredsAsDeleted(int pUniqueKey)
        {
            List<int> IDList = new List<int>();
            IDList.Add(pUniqueKey);
            Epi.Data.Services.CollectedDataProvider OutputDriver = this.Context.CurrentProject.CollectedData;// Epi.Data.DBReadExecute.GetDataDriver(this.project.CollectedDataConnectionString, true);
            UnMarkRelatedRecoredsAsDeleted(OutputDriver, this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier], IDList);
        }

        private void CascadePermanentDelete(Epi.Data.Services.CollectedDataProvider OutputDriver, View pView, List<int> pIDList)
        {
            Dictionary<string, bool> VisitedViews = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            string SQL = null;
            StringBuilder InSQL = new StringBuilder();
            InSQL.Append(" (");
            foreach (int ID in pIDList)
            {
                pView.LoadRecord(ID);
                InSQL.Append("'");
                InSQL.Append(pView.CurrentGlobalRecordId);
                InSQL.Append("',");
            }
            InSQL.Length = InSQL.Length - 1;
            InSQL.Append(")");

            foreach (Epi.Fields.Field field in pView.Fields)
            {
                if (field is Epi.Fields.RelatedViewField)
                {
                    Epi.Fields.RelatedViewField rvf = field as Epi.Fields.RelatedViewField;
                    if (!VisitedViews.ContainsKey(rvf.ChildView.Name))
                    {
                        SQL = "Select UniqueKey From [" + rvf.ChildView.TableName + "] Where FKey In " + InSQL.ToString();
                        IDataReader reader = OutputDriver.ExecuteReader(OutputDriver.CreateQuery(SQL));
                        List<int> NewIdList = new List<int>();
                        while (reader.Read())
                        {
                            if (reader["UniqueKey"] != DBNull.Value)
                            {
                                NewIdList.Add((int)reader["UniqueKey"]);
                            }
                        }
                        VisitedViews.Add(rvf.ChildView.Name, true);
                        CascadePermanentDelete(OutputDriver, rvf.ChildView, NewIdList);
                    }
                }
                else if (field is Epi.Fields.GridField)
                {

                    Epi.Fields.GridField gf = field as Epi.Fields.GridField;

                    SQL = "Delete from  [" + gf.TableName + "]  Where FKey In " + InSQL.ToString();
                    OutputDriver.ExecuteNonQuery(OutputDriver.CreateQuery(SQL));

                }
            }

            foreach (Epi.Page page in pView.Pages)
            {
                SQL = "Delete from  [" + page.TableName + "]  Where GlobalRecordId In " + InSQL.ToString();
                OutputDriver.ExecuteNonQuery(OutputDriver.CreateQuery(SQL));
            }

            SQL = "Delete from  [" + pView.TableName + "]  Where GlobalRecordId In " + InSQL.ToString();
            OutputDriver.ExecuteNonQuery(OutputDriver.CreateQuery(SQL));
        }

        private void CascadePermanentDelete(int pUniqueKey)
        {
            List<int> IDList = new List<int>();
            IDList.Add(pUniqueKey);
            Epi.Data.Services.CollectedDataProvider OutputDriver = this.Context.CurrentProject.CollectedData;// Epi.Data.DBReadExecute.GetDataDriver(this.project.CollectedDataConnectionString, true);
            CascadePermanentDelete(OutputDriver, this.Context.CurrentProject.Views[this.Context.CurrentRead.Identifier], IDList);
        }

    }

    /// <summary>
    /// DELETE File DeleteOpts
    /// </summary>
    public class Rule_Delete_File : Rule_Delete
    {
        public Rule_Delete_File(Rule_Context pContext, NonterminalToken pToken) : base(pContext, pToken)
        {
            string trimChars = "'";
            filePath = GetCommandElement(pToken.Tokens, 1).Trim(trimChars.ToCharArray());
            if (pToken.Tokens.Length > 2)
            {
                deleteOptions.AddRange(GetCommandElement(pToken.Tokens, 2).ToUpper().Split(StringLiterals.SPACE.ToCharArray()));
                runSilent = deleteOptions.Contains("RUNSILENT");
            }
        }

        public override object Execute()
        {
            object result = null;
            object delResult = false;
            bool confirmDelete = true;
            string path = string.Empty;
            string searchPattern = string.Empty;
            int dirEnd = filePath.LastIndexOf("\\");
            string[] files;

            path = filePath.Substring(1, dirEnd);
            searchPattern = filePath.Substring(dirEnd + 1, filePath.Length - dirEnd - 2);
            files = Directory.GetFiles(path, searchPattern);
            string prompt = string.Format(SharedStrings.CONFIRM_FILE_DELETE, files.Length);
            if (files.Length > 0)
            {
                if (!runSilent)
                {
                    this.Context.AnalysisCheckCodeInterface.Dialog(prompt, ref delResult, "YesNo", SharedStrings.ANALYSIS);
                    if (!(bool)delResult)
                    {
                        confirmDelete = false;
                    }
                }

                if (confirmDelete)
                {
                    foreach (string file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            Logger.Log(DateTime.Now + ":  " + ex.Message);
                            if (!runSilent)
                            {
                                this.Context.AnalysisCheckCodeInterface.Dialog(SharedStrings.FILE_NOT_FOUND + ":" + filePath, SharedStrings.ANALYSIS);
                            }
                            continue;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Logger.Log(DateTime.Now + ":  " + ex.Message);
                            if (!runSilent)
                            {
                                this.Context.AnalysisCheckCodeInterface.Dialog(SharedStrings.CANNOT_DELETE_FILE + ":" + filePath, SharedStrings.ANALYSIS);
                            }
                            continue;
                        }
                    }
                }
            }
            else
            {
                if (!runSilent)
                {
                    this.Context.AnalysisCheckCodeInterface.Dialog(SharedStrings.FILE_NOT_FOUND + ":" + filePath, SharedStrings.ANALYSIS);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// DELETE TABLES Identifier DeleteOpts
    /// </summary>
    public class Rule_Delete_Table : Rule_Delete
    {
        private IMetadataProvider md = null;

        public Rule_Delete_Table(Rule_Context pContext, NonterminalToken pToken) : base(pContext, pToken)
        {

            md = this.Context.CurrentProject.Metadata;
            identifier = GetCommandElement(pToken.Tokens, 2);
            if (pToken.Tokens.Length > 3)
            {
                deleteOptions.AddRange(GetCommandElement(pToken.Tokens, 3).ToUpper().Split(StringLiterals.SPACE.ToCharArray()));
                runSilent = deleteOptions.Contains("RUNSILENT");
                saveData = deleteOptions.Contains("SAVEDATA");
            }
        }

        public override object Execute()
        {
            object results = null;
            object delResult = false;
            bool confirmDelete = true;

            if (!runSilent)
            {
                this.Context.AnalysisCheckCodeInterface.Dialog("Delete tables?", ref delResult, "YesNo", SharedStrings.ANALYSIS);
                if (!((bool)delResult))
                {
                    confirmDelete = false;
                }
            }

            if (confirmDelete)
            {
                if (this.Context.CurrentProject.CollectedData.TableExists(identifier))
                {
                    this.Context.CurrentProject.CollectedData.DeleteTable(identifier);
                }
            }
            return results;

        }

        private bool IsView(string name)
        {
            name = ":" + name;
            return (md.GetViewByFullName(name) != null);
        }
    }

    /// <summary>
    /// DELETE TABLES File DeleteOpts
    /// </summary>
    public class Rule_Delete_Table_Short : Rule_Delete
    {
        public Rule_Delete_Table_Short(Rule_Context pContext, NonterminalToken pToken) : base(pContext, pToken)
        {
            filePath = GetCommandElement(pToken.Tokens, 2);

            if (pToken.Tokens.Length > 3)
            {
                deleteOptions.AddRange(GetCommandElement(pToken.Tokens, 3).ToUpper().Split(StringLiterals.SPACE.ToCharArray()));
            }
        }

        public override object Execute()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    /// <summary>
    /// DELETE TABLES File ':' Identifier DeleteOpts
    /// </summary>
    public class Rule_Delete_Table_Long : Rule_Delete
    {

        public Rule_Delete_Table_Long(Rule_Context pContext, NonterminalToken pToken) : base(pContext, pToken)
        {
            filePath = GetCommandElement(pToken.Tokens, 2).Trim( new char[] {'{','}', '\'', '"'});
            identifier = GetCommandElement(pToken.Tokens, 4).Trim(new char[] { '[',']'});
            if (pToken.Tokens.Length > 5)
            {
                deleteOptions.AddRange(GetCommandElement(pToken.Tokens, 5).ToUpper().Split(StringLiterals.SPACE.ToCharArray()));
                runSilent = deleteOptions.Contains("RUNSILENT");
            }
        }

        public override object Execute()
        {
            //throw new Exception("The method or operation is not implemented.");
            object result = null;
            bool confirmDelete = true;
            object delResult = true;

            if (!runSilent)
            {
                this.Context.AnalysisCheckCodeInterface.Dialog("Delete tables?", ref delResult, "YesNo", SharedStrings.ANALYSIS);
                if (!((bool)delResult))
                {
                    confirmDelete = false;
                }
            }

            if (confirmDelete)
            {

                IDbDriver db = DBReadExecute.GetDataDriver(filePath);

                if (db.CheckDatabaseExistance(filePath, identifier))
                {
                    db.DeleteTable(identifier);

                }
            }

            return result;
        }
    }
}