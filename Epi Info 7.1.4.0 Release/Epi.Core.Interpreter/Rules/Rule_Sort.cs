using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// The rule for the SORT command.
    /// </summary>
    public class Rule_Sort : AnalysisRule
    {
        List<string> SortList = null;
        List<string> SortOption = null;
        bool CancelSort = false;
        bool HasRun = false;

        public Rule_Sort(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            

            /*
            !***       		SortStatement  			***!
            <Sort_Statement> 						::= SORT <SortList>
                                            | SORT |CANCEL SORT

            <SortOpt> 							::=   ASCENDING
                                            | DESCENDING
                                            | !Null

            <Sort> 								::= Identifier <SortOpt>

            <SortList> 							::= <SortList> <Sort> 
                                            |<Sort>
            <Cancel_Sort_By_Sorting_Statement>					::= SORT
            <Cancel_Sort_Statement>						::= CANCEL SORT
            !***			End    				***!
 
            */

            this.SortList = new List<string>();
            this.SortOption = new List<string>();

            if (pToken.Tokens.Length == 1)
            {
                this.CancelSort = true;
            }
            else
            {
                if (pToken.Tokens[0].ToString().ToUpper() == "CANCEL")
                {
                    this.CancelSort = true;
                }
                else
                {
                    this.SetSortList((NonterminalToken)pToken.Tokens[1]); 
                }
            }

        }




        private void SetSortList(NonterminalToken pToken)
        {
            //<SortList> ::= <SortList> <Sort> |<Sort>

            switch (pToken.Symbol.ToString())
            {
                case "<SortList>":
                    this.SetSortList((NonterminalToken) pToken.Tokens[0]);
                    this.SetSort((NonterminalToken) pToken.Tokens[1]);
                    break;
                case "<Sort>":
                    this.SetSort(pToken);
                    break;
            }

        }


        private void SetSort(NonterminalToken pToken)
        {
            /*
            <Sort> ::= Identifier <SortOpt>
            <SortOpt> ::= ASCENDING | DESCENDING | !Null
            */
            if (pToken.Tokens.Length > 1)
            {
                this.SortList.Add(this.GetCommandElement(pToken.Tokens,0).Trim(new char[] { '[',']'}));
                switch(this.GetCommandElement(pToken.Tokens, 1).ToUpper())
                {
                    case "DESCENDING":
                    case "DESC":
                        this.SortOption.Add("DESC");
                        break;
                    default:
                        this.SortOption.Add("ASC");
                        break;
                }
            }
            else
            {
                this.SortList.Add(this.GetCommandElement(pToken.Tokens, 0).Trim(new char[] { '[', ']' }));
                this.SortOption.Add("ASC");
            }
        }

        /// <summary>
        /// Executes the Rule_Sort reduction
        /// </summary>
        /// <returns>Returns the result of executing the reduction.</returns>
        public override object Execute()
        {
            object result = null;

            if (!this.HasRun)
            {
                this.Context.NewSortNeeded = true;

                this.Context.SortExpression.Length = 0;

                if (!this.CancelSort)
                {
                    this.Context.SortExpression.Length = 0;

                    for (int i = 0; i < this.SortList.Count; i++)
                    {
                        if (this.Context.SortExpression.Length == 0)
                        {
                            this.Context.SortExpression.Append("[");
                            this.Context.SortExpression.Append(this.SortList[i]);
                            this.Context.SortExpression.Append("] ");
                            this.Context.SortExpression.Append(this.SortOption[i]);
                        }
                        else
                        {
                            this.Context.SortExpression.Append(", [");
                            this.Context.SortExpression.Append(this.SortList[i]);
                            this.Context.SortExpression.Append("] ");
                            this.Context.SortExpression.Append(this.SortOption[i]);
                        }
                    }
                }
                else
                {
                    // cancel the sort
                    this.Context.SortExpression.Length = 0;
                }

                this.Context.GetOutput(); // peform the sort before the display performs the sort to prevent error occuring in separate thread in display

                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", CommandNames.SORT);
                this.Context.AnalysisCheckCodeInterface.Display(args);

                this.HasRun = true;
            }

            return result;
        }

    }

    /// <summary>
    /// Class to hold one of the items involved in a SORT command
    /// </summary>
    public class Rule_SortItem : AnalysisRule
    {
        #region Constructors

        /// <summary>
        /// Rule_SortItem constructor
        /// </summary>
        /// <param name="pToken">Token to build the command with.</param>
        /// <param name="pList">List of fields to sort on</param>
        /// <param name="pOption">The sort option to use (ASC, DESC)</param>
        public Rule_SortItem(NonterminalToken pToken, List<string> pList, List<string> pOption)
        {
            //<SortList> 							::= <SortList> <Sort>
            //<Sort> 								::= Identifier <SortOpt> | Identifier
            switch(pToken.Rule.Lhs.ToString())
            {
                case "<Sort>":
                    pList.Add(this.GetCommandElement(pToken.Tokens, 0));
                    if (pToken.Tokens.Length > 1)
                    {
                        string Test = this.GetCommandElement(pToken.Tokens, 1);
                        switch (Test.ToUpper())
                        {
                            case "DESCENDING":
                                pOption.Add("DESC");
                                break;
                            default:
                                pOption.Add("ASC");
                                break;
                        }
                    }
                    else
                    {
                        pOption.Add("ASC");
                    }
                    break;
                case "<SortList>":
                    new Rule_SortItem((NonterminalToken) pToken.Tokens[0], pList, pOption);
                    if (pToken.Tokens.Length > 1)
                    {
                        new Rule_SortItem((NonterminalToken)pToken.Tokens[1], pList, pOption);
                    }
                    break;
            }

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the Rule_SortItem reduction
        /// </summary>
        /// <returns>Always returns null.</returns>
        public override object Execute()
        {
            return null;
        }

        #endregion
    }
}

