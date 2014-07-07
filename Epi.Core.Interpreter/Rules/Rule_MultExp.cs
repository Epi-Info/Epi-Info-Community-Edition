using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_MultExp : AnalysisRule
    {
        //AnalysisRule PowExp = null;
        string op = null;
        //AnalysisRule MultExp = null;
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_MultExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*::= <Pow Exp> '*' <Mult Exp>
								| <Pow Exp> '/' <Mult Exp>
								| <Pow Exp> MOD <Mult Exp>
								| <Pow Exp> '%' <Mult Exp>
            							| <Pow Exp>*/

            //this.PowExp = new Rule_PowExp(pContext, (NonterminalToken)pToken.Tokens[0]);
            this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[0]));
            if(pToken.Tokens.Length > 1)
            {
                this.op = pToken.Tokens[1].ToString();

                //this.MultExp = new Rule_MultExp(pContext, (NonterminalToken)pToken.Tokens[2]);
                this.ParameterList.Add(AnalysisRule.BuildStatments(pContext, pToken.Tokens[2]));
            }
        }

        /// <summary>
        /// performs execution of the (/, MOD and %) operators
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (op == null)
            {
                //result = this.PowExp.Execute();
                result = this.ParameterList[0].Execute();
            }
            else
            {
                object LHSO = this.ParameterList[0].Execute(); // this.PowExp.Execute();
                object RHSO = this.ParameterList[1].Execute(); // this.MultExp.Execute();

                if (LHSO == null || RHSO == null)
                {
                    return null;
                }


                if (this.NumericTypeList.Contains(LHSO.GetType().Name.ToUpper()))
                {
                    LHSO = Convert.ToDouble(LHSO);
                }

                if (this.NumericTypeList.Contains(RHSO.GetType().Name.ToUpper()))
                {
                    RHSO = Convert.ToDouble(RHSO);
                }


                switch (op)
                {
                    case "*":
                        if ((LHSO is TimeSpan) && (RHSO is TimeSpan))
                        {
                            result = ((TimeSpan)LHSO).TotalDays * ((TimeSpan)RHSO).TotalDays;
                        }
                        else if ((LHSO is TimeSpan) && (RHSO is Double))
                        {
                            result = ((TimeSpan)LHSO).TotalDays * (Double)RHSO;
                        }
                        else if ((LHSO is Double) && (RHSO is TimeSpan))
                        {
                            result = (Double)LHSO * ((TimeSpan)RHSO).TotalDays;
                        }
                        else if ((LHSO is Double) && (RHSO is Double))
                        {
                            result = (Double)LHSO * (Double)RHSO;
                        }
                        break;
                    case "/":
                        if ((LHSO is TimeSpan) && (RHSO is TimeSpan))
                        {
                            result = ((TimeSpan)LHSO).TotalDays / ((TimeSpan)RHSO).TotalDays;
                        }
                        else if ((LHSO is TimeSpan) && (RHSO is Double))
                        {
                            result = ((TimeSpan)LHSO).TotalDays / (Double)RHSO;
                        }
                        else if ((LHSO is Double) && (RHSO is TimeSpan))
                        {
                            result = (Double)LHSO / ((TimeSpan)RHSO).TotalDays;
                        }
                        else if ((LHSO is Double) && (RHSO is Double))
                        {
                            result = (Double)LHSO / (Double)RHSO;
                        }
                        break;
                    case "MOD":
                    case "%":
                        if ((LHSO is TimeSpan) && (RHSO is TimeSpan))
                        {
                            result = ((TimeSpan)LHSO).TotalDays % ((TimeSpan)RHSO).TotalDays;
                        }
                        else if ((LHSO is TimeSpan) && (RHSO is Double))
                        {
                            result = ((TimeSpan)LHSO).TotalDays % (Double)RHSO;
                        }
                        else if ((LHSO is Double) && (RHSO is TimeSpan))
                        {
                            result = (Double)LHSO % ((TimeSpan)RHSO).TotalDays;
                        }
                        else if ((LHSO is Double) && (RHSO is Double))
                        {
                            result = (Double)LHSO % (Double)RHSO;
                        }

                        break;
                }
            }

            return result;
        }
    }
}
