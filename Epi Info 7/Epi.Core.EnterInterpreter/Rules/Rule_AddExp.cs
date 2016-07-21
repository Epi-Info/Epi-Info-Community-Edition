using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_AddExp: EnterRule
    {

        EnterRule MultExp = null;
        string operation = null;
        EnterRule AddExp = null;

        public Rule_AddExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*::= <Mult Exp> '+' <Add Exp>
            							| <Mult Exp> '-' <Add Exp>
            							| <Mult Exp>*/

            this.MultExp = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);

            if (pToken.Tokens.Length > 1)
            {
                operation = pToken.Tokens[1].ToString();
                this.AddExp = EnterRule.BuildStatments(pContext, pToken.Tokens[2]);
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
                result = this.MultExp.Execute();
            }
            else
            {
                object LHSO = this.MultExp.Execute();
                object RHSO = this.AddExp.Execute();


                if (this.NumericTypeList.Contains(LHSO.GetType().Name.ToUpperInvariant()) && this.NumericTypeList.Contains(RHSO.GetType().Name.ToUpperInvariant()))
                {
                    LHSO = Convert.ToDouble(LHSO);
                    RHSO = Convert.ToDouble(RHSO);
                }


                switch (operation)
                {
                    case "+":
                        if ((LHSO is DateTime) && (RHSO is DateTime))
                        {
                        }
                        else if ((LHSO is DateTime) && (RHSO is TimeSpan))
                        {
                            result = ((DateTime)LHSO).Add((TimeSpan)RHSO);
                        }
                        else if ((LHSO is TimeSpan) && (RHSO is DateTime))
                        {
                            result = ((DateTime)RHSO).Add((TimeSpan)LHSO);
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
