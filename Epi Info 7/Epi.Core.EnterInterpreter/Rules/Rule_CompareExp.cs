using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_CompareExp : EnterRule
    {
        EnterRule ConcatExp = null;
        string op = null;
        EnterRule CompareExp = null;
        string STRING = null;
        
        public Rule_CompareExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // <Concat Exp> LIKE String
            // <Concat Exp> '=' <Compare Exp>
            // <Concat Exp> '<>' <Compare Exp>
            // <Concat Exp> '>' <Compare Exp>
            // <Concat Exp> '>=' <Compare Exp>
            // <Concat Exp> '<' <Compare Exp>
            // <Concat Exp> '<=' <Compare Exp>
            // <Concat Exp>
            
            this.ConcatExp = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            if (pToken.Tokens.Length > 1)
            {
                op = pToken.Tokens[1].ToString();

                if (pToken.Tokens[1].ToString().Equals("LIKE", StringComparison.OrdinalIgnoreCase))
                {
                    this.STRING = pToken.Tokens[2].ToString().Trim(new char[] { '"' });
                }
                else
                {
                    this.CompareExp = EnterRule.BuildStatments(pContext, pToken.Tokens[2]);
                }
            }
        }


        /// <summary>
        /// perfoms comparison operations on expression ie (=, <=, >=, Like, >, <, and <)) returns a boolean
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (op == null)
            {
                result = this.ConcatExp.Execute();
            }
            else
            {
                object LHSO = this.ConcatExp.Execute();
                object RHSO;

                if (this.CompareExp != null)
                {
                    RHSO = this.CompareExp.Execute(); // this.CompareExp.Execute();
                }
                else
                {
                    RHSO = this.STRING;
                }


                double TryValue = 0.0;
                int i;

                if (Util.IsEmpty(LHSO) && Util.IsEmpty(RHSO) && op.Equals("="))
                {
                    result = true;
                }
                else if (Util.IsEmpty(LHSO) && Util.IsEmpty(RHSO) && op.Equals("<>"))
                {
                    return false;
                }
                else if ((Util.IsEmpty(LHSO) || Util.IsEmpty(RHSO)))
                {
                    if (op.Equals("<>"))
                    {
                        return !(Util.IsEmpty(LHSO) && Util.IsEmpty(RHSO));
                    }
                    else
                    {
                        result = false;
                    }
                }
                else if (op.Equals("LIKE", StringComparison.OrdinalIgnoreCase))
                {
                    //string testValue = "^" + RHSO.ToString().Replace("*", "(\\s|\\w)*") + "$";
                    string testValue = "^" + RHSO.ToString().Replace("*", ".*") + "$";
                    System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(testValue, System.Text.RegularExpressions.RegexOptions.IgnoreCase);


                    if (re.IsMatch(LHSO.ToString()))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {

                    if (this.NumericTypeList.Contains(LHSO.GetType().Name.ToUpper()) && this.NumericTypeList.Contains(RHSO.GetType().Name.ToUpper()))
                    {
                        LHSO = Convert.ToDouble(LHSO);
                        RHSO = Convert.ToDouble(RHSO);
                    }

                    if (!LHSO.GetType().Equals(RHSO.GetType()))
                    {
                        if (RHSO is Boolean && op.Equals(StringLiterals.EQUAL))
                        {
                            result = (RHSO.Equals(!Util.IsEmpty(LHSO)));
                        }
                        else if (LHSO is string && this.NumericTypeList.Contains(RHSO.GetType().Name.ToUpper()) && double.TryParse(LHSO.ToString(), out TryValue))
                        {
                            i = TryValue.CompareTo(RHSO);

                            switch (op)
                            {
                                case "=":
                                    result = (i == 0);
                                    break;
                                case "<>":
                                    result = (i != 0);
                                    break;
                                case "<":
                                    result = (i < 0);
                                    break;
                                case ">":
                                    result = (i > 0);
                                    break;
                                case ">=":
                                    result = (i >= 0);
                                    break;
                                case "<=":
                                    result = (i <= 0);
                                    break;
                            }

                        }
                        else if (RHSO is string && this.NumericTypeList.Contains(LHSO.GetType().Name.ToUpper()) && double.TryParse(RHSO.ToString(), out TryValue))
                        {
                            i = TryValue.CompareTo(LHSO);

                            switch (op)
                            {
                                case "=":
                                    result = (i == 0);
                                    break;
                                case "<>":
                                    result = (i != 0);
                                    break;
                                case "<":
                                    result = (i < 0);
                                    break;
                                case ">":
                                    result = (i > 0);
                                    break;
                                case ">=":
                                    result = (i >= 0);
                                    break;
                                case "<=":
                                    result = (i <= 0);
                                    break;
                            }
                        }
                        else if (op.Equals(StringLiterals.EQUAL) && (LHSO is Boolean || RHSO is Boolean))
                        {
                            if (LHSO is Boolean && RHSO is Boolean)
                            {
                                result = LHSO == RHSO;   
                            }
                            else if (LHSO is Boolean)
                            {
                                result = (Boolean)LHSO == (Boolean) this.ConvertStringToBoolean(RHSO.ToString());
                            }
                            else
                            {
                                result = (Boolean)this.ConvertStringToBoolean(LHSO.ToString()) == (Boolean)RHSO;
                            }
                        }
                        else
                        {


                            i = StringComparer.CurrentCultureIgnoreCase.Compare(LHSO.ToString(), RHSO.ToString());

                            switch (op)
                            {
                                case "=":
                                    result = (i == 0);
                                    break;
                                case "<>":
                                    result = (i != 0);
                                    break;
                                case "<":
                                    result = (i < 0);
                                    break;
                                case ">":
                                    result = (i > 0);
                                    break;
                                case ">=":
                                    result = (i >= 0);
                                    break;
                                case "<=":
                                    result = (i <= 0);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        i = 0;

                        if (LHSO.GetType().Name.ToUpper() == "STRING" && RHSO.GetType().Name.ToUpper() == "STRING")
                        {
                            i = StringComparer.CurrentCultureIgnoreCase.Compare(LHSO.ToString().Trim(), RHSO.ToString().Trim());
                        }
                        else if (LHSO is IComparable && RHSO is IComparable)
                        {
                            i = ((IComparable)LHSO).CompareTo((IComparable)RHSO);
                        }

                        switch (op)
                        {
                            case "=":
                                result = (i == 0);
                                break;
                            case "<>":
                                result = (i != 0);
                                break;
                            case "<":
                                result = (i < 0);
                                break;
                            case ">":
                                result = (i > 0);
                                break;
                            case ">=":
                                result = (i >= 0);
                                break;
                            case "<=":
                                result = (i <= 0);
                                break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
