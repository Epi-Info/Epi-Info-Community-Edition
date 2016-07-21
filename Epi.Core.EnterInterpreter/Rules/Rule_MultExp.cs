using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_MultExp : EnterRule
    {
        EnterRule PowExp = null;
        string op = null;
        EnterRule MultExp = null;


        public Rule_MultExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*::= <Pow Exp> '*' <Mult Exp>
								| <Pow Exp> '/' <Mult Exp>
								| <Pow Exp> MOD <Mult Exp>
								| <Pow Exp> '%' <Mult Exp>
            							| <Pow Exp>*/

            this.PowExp = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            if(pToken.Tokens.Length > 1)
            {
                this.op = pToken.Tokens[1].ToString();

                this.MultExp = EnterRule.BuildStatments(pContext, pToken.Tokens[2]);
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
                result = this.PowExp.Execute();
            }
            else
            {
                object LHSO = this.PowExp.Execute();
                object RHSO = this.MultExp.Execute();

                if (this.NumericTypeList.Contains(LHSO.GetType().Name.ToUpperInvariant()))
                {
                    LHSO = Convert.ToDouble(LHSO);
                }

                if (this.NumericTypeList.Contains(RHSO.GetType().Name.ToUpperInvariant()))
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
