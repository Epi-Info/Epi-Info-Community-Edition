using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using System.Data;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_UnDelete : AnalysisRule
    {
        AnalysisRule CompareExpression;
        bool isRunSilent = false;
        string CommandText = null;

        public Rule_UnDelete(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            /*
             <Undelete_All_Statement> ::= UNDELETE '*' | UNDELETE '*' RUNSILENT
                                        | UNDELETE <Compare Exp> | UNDELETE <Compare Exp> RUNSILENT
             */
            CommandText = this.ExtractTokens(pToken.Tokens);

            if (pToken.Tokens[1] is NonterminalToken)
            {
                this.CompareExpression = AnalysisRule.BuildStatments(pContext, pToken.Tokens[1]);
            }
            else
            {
                this.CompareExpression = new Rule_Value("true");
            }


            if (pToken.Tokens.Length > 2)
            {
                this.isRunSilent = true;
            }
        }

        /// <summary>
        /// performs execution of the UNDELETE command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            int NumberOfUnDeletes = 0;
            StringBuilder HTMLString = new StringBuilder();

            if (this.Context.CurrentRead != null && this.Context.CurrentRead.IsEpi7ProjectRead)
            {
                if (this.Context.CurrentDataRow == null)
                {
                    Project currentProject = this.Context.CurrentProject;
                    Rule_Read currentRead = this.Context.CurrentRead;
                    View currentView = currentProject.Views[currentRead.Identifier.ToLower()];
                    string viewName = currentView.TableName;
                    
                    string pSQL = "Select * From [" + viewName + "]";
                    DataTable completeTable = Epi.Data.DBReadExecute.GetDataTable(currentRead.File, pSQL);

                    foreach (Page page in currentProject.Views[currentRead.Identifier].Pages)
                    {
                        completeTable = currentRead.JoinTables(completeTable, Epi.Data.DBReadExecute.GetDataTable(currentRead.File, "Select * From [" + page.TableName + "]"));
                    }

                    foreach (DataRow row in completeTable.Rows)
                    {
                        this.Context.CurrentDataRow = row;

                        string compareExpression = this.CompareExpression.Execute().ToString();


                        if (compareExpression.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                        {
                            NumberOfUnDeletes++;
                            row["RecStatus"] = 1;
                            Epi.Data.DBReadExecute.ExecuteSQL(this.Context.CurrentProject.FullName, "UPDATE " + this.Context.CurrentRead.Identifier + " SET RecStatus=1 Where GlobalRecordId='" + row["GlobalRecordId"] + "'");
                        }
                    }

                    this.Context.CurrentDataRow = null;
                    this.Context.CurrentRead.Execute();
                }
                HTMLString.Append("<p>");
                HTMLString.Append(string.Format(SharedStrings.NUMBER_RECORDS_UNDELETED, NumberOfUnDeletes.ToString()));
                HTMLString.Append("</p>");
            }
            else
            {
                HTMLString.Append("<p>The current Read must be an Epi7 project. 0 items marked as deleted.</p>");
            }

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "UNDELETE");
            args.Add("COMMANDTEXT", CommandText);
            args.Add("ROWCOUNT", NumberOfUnDeletes.ToString());
            args.Add("HTMLRESULTS", HTMLString.ToString());
            
            this.Context.AnalysisCheckCodeInterface.Display(args);

            return null;
        }


    }

 
}
