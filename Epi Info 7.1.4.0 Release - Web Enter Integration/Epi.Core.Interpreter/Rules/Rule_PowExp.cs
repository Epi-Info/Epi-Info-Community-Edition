using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_PowExp : AnalysisRule 
    {
        //AnalysisRule NegateExp1 = null;
        //AnalysisRule NegateExp2 = null;

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_PowExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /* 	::= <Negate Exp> '^' <Negate Exp>  | <Negate Exp> */

            //this.ParameterList[0] = new Rule_NegateExp(pContext, (NonterminalToken)pToken.Tokens[0]);
            this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[0]));
            if (pToken.Tokens.Length > 1)
            {
                //this.ParameterList[1] = new Rule_NegateExp(pContext, (NonterminalToken)pToken.Tokens[2]);
                this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[2]));
            }
            
        }


        /// <summary>
        /// raises a number to a power and returns the resulting number
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (this.ParameterList.Count == 1)
            {
                result = this.ParameterList[0].Execute();
            }
            else
            {

                string LHS = this.ParameterList[0].Execute().ToString();
                string RHS = this.ParameterList[1].Execute().ToString();

                result = System.Math.Pow(double.Parse(LHS),double.Parse(RHS));
            }

            return result;
        }
    }
}
