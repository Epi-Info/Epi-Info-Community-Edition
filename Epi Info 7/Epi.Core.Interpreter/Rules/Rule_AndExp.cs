using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_AndExp : AnalysisRule 
    {
        //AnalysisRule NotExp = null;
        //AnalysisRule AndExp = null;

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_AndExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // <Not Exp> AND <And Exp> 
            // <Not Exp> 

            //this.NotExp = new Rule_NotExp(pContext, (NonterminalToken)pToken.Tokens[0]);
            this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[0]));
            if (pToken.Tokens.Length > 1)
            {
                //this.AndExp = new Rule_AndExp(pContext, (NonterminalToken)pToken.Tokens[2]);
                this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[2]));
            }
        }

        /// <summary>
        /// performs an "and" operation on boolean expresssions
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (this.ParameterList.Count == 1)
            {
                //result = this.NotExp.Execute();
                result = this.ParameterList[0].Execute();
            }
            else
            {
                // dpb: this needs to be fixed to work with more then just strings

                string LHS = this.ParameterList[0].Execute().ToString().ToLower(); // this.NotExp.Execute().ToString().ToLower();
                string RHS = this.ParameterList[1].Execute().ToString().ToLower(); //this.AndExp.Execute().ToString().ToLower();

                result = "true" == LHS && LHS == RHS;
            }

            return result;
        }
    }
}
