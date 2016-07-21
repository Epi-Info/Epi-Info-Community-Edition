using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_AddExp: AnalysisRule
    {

        //AnalysisRule MultExp = null;
        string operation = null;
        //AnalysisRule AddExp = null;
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_AddExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*::= <Mult Exp> '+' <Add Exp>
            							| <Mult Exp> '-' <Add Exp>
            							| <Mult Exp>*/

            //this.MultExp = new Rule_MultExp(pContext, (NonterminalToken)pToken.Tokens[0]);
            this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[0]));

            if (pToken.Tokens.Length > 1)
            {
                operation = pToken.Tokens[1].ToString();
                //this.AddExp = new Rule_AddExp(pContext, (NonterminalToken)pToken.Tokens[2]);
                this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[2]));
            }
        }
        /// <summary>
        /// performs an addition / subtraction or pass thru rule
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (operation == null)
            {
                //result = this.MultExp.Execute();
                result = this.ParameterList[0].Execute();
            }
            else
            {
                object LHSO = this.ParameterList[0].Execute(); // this.MultExp.Execute();
                object RHSO = this.ParameterList[1].Execute(); // this.AddExp.Execute();


                if (LHSO!=null && this.NumericTypeList.Contains(LHSO.GetType().Name.ToUpperInvariant()))
                {
                    LHSO = Convert.ToDouble(LHSO);
                }

                if (RHSO!=null && this.NumericTypeList.Contains(RHSO.GetType().Name.ToUpperInvariant()))
                {
                    RHSO = Convert.ToDouble(RHSO);
                }



                switch (operation)
                {
                    case "+":
                        if ((LHSO is DateTime) && (RHSO is DateTime))
                        {
                        }
                        else if ((LHSO is DateTime) && (RHSO is Double))
                        {
                            result = ((DateTime)LHSO).AddDays((Double)RHSO);
                        }
                        else if ((LHSO is Double) && (RHSO is DateTime))
                        {
                        }
                        else if ((LHSO is Double) && (RHSO is Double))
                        {
                            result = (Double)LHSO + (Double)RHSO;
                        }
                        break;

                    case "-":
                        if ((LHSO is DateTime) && (RHSO is DateTime))
                        {

                            result = (DateTime)LHSO - (DateTime)RHSO;
                            result = ((TimeSpan)result).TotalDays;
                        }
                        else if ((LHSO is DateTime) && (RHSO is Double))
                        {
                            result = ((DateTime)LHSO).AddDays(-1 * (Double)RHSO);
                        }
                        else if ((LHSO is Double) && (RHSO is DateTime))
                        {
                        }
                        else if ((LHSO is Double) && (RHSO is Double))
                        {
                            result = (Double)LHSO - (Double)RHSO;
                        }
                        break;
                }
            }

            return result;
        }

    }
}
