using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_IsUnique reduction.
    /// </summary>
    public partial class Rule_IsUnique : EnterRule
    {
        //<Is_Unique_Statement> ::= ISUNIQUE <IdentifierList> 

        string[] IdentifierList = null;

        public Rule_IsUnique(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            if (pToken.Tokens.Length >= 1)
            {
                int max = pToken.Tokens.GetUpperBound(0);
                string tokensInTree = "";
                for (int i = pToken.Tokens.GetLowerBound(0); i <= max; i++)
                {
                    if (pToken.Tokens[i] is NonterminalToken)
                    {
                        tokensInTree += ExtractTokens(((NonterminalToken)pToken.Tokens[i]).Tokens).Trim();
                    }
                    else
                    {
                        tokensInTree += pToken.Tokens[i].ToString();
                    }
                }
                this.IdentifierList = tokensInTree.Split(',');
                //if (pContext.IsVariableValidationEnable)
                //{
                //    if (IdentifierList.Length > 0)
                //    {
                //        foreach (var item in IdentifierList)
                //        {

                //            if (!string.IsNullOrEmpty(item) && !this.Context.CommandVariableCheck.ContainsKey(item.ToLowerInvariant()))
                //            {
                //                this.Context.CommandVariableCheck.Add(item.ToLowerInvariant(), "isunique");
                //            }
                //        }
                //    }
                //}
            }
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns true when the value is unique.</returns>
        public override object Execute()
        {
            object result = this.Context.EnterCheckCodeInterface.IsUnique(this.IdentifierList); 

            return result;
        }       
    }
}
