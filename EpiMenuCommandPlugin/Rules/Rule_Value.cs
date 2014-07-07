using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace EpiMenu.CommandPlugin
{
    public class Rule_Value : Rule
    {
        string Id = null;
        object value = null;
        object ReturnResult = null;

        public Rule_Value(Rule_Context pContext, Token pToken) : base(pContext)
        {
            /* ::= Identifier	| <Literal> | Boolean | '(' <Expr List> ')' */

            if (pToken is NonterminalToken)
            {
                NonterminalToken T = (NonterminalToken)pToken;
                if (T.Tokens.Length == 1)
                {
                    switch (T.Rule.Rhs[0].ToString())
                    {
                        case "Identifier":
                            this.Id = this.GetCommandElement(T.Tokens, 0);
                            break;
                        case "<FunctionCall>":
                            //this.value = new Rule_FunctionCall(pContext, (NonterminalToken)T.Tokens[0]);
                            break;
                        case "<Literal>":
                        case "Boolean":
                        default:
                            this.value = this.GetCommandElement(T.Tokens, 0);
                            break;
                    }
                }
                else
                {
                    //this.value = new Rule_ExprList(pContext, (NonterminalToken)T.Tokens[1]);
                }
            }
            else
            {
                TerminalToken TT = (TerminalToken)pToken;
                switch (TT.Symbol.ToString())
                {
                    case "Boolean":
                        this.value = TT.Text;
                        break;
                    default:
                        this.value = TT.Text;
                        break;
                }
            }

            if (this.Id == null && this.value == null)
            {

            }
        }

        public Rule_Value(string pValue)
        {
            this.value = pValue;
        }


        /// <summary>
        /// performs execution of retrieving the value of a variable or expression
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            /*
            if (this.Id != null)
            {
                IVariable var;
                DataType dataType = DataType.Unknown;
                string dataValue = string.Empty;

                if (this.Context.MemoryRegion.TryGetVariable(this.Id, out var))
                {
                    dataType = var.DataType;
                    dataValue = var.Expression;
                }
                else
                {
                    if (this.Context.EnterCheckCodeInterface != null)
                    {
                        this.Context.EnterCheckCodeInterface.TryGetFieldInfo(this.Id, out dataType, out dataValue);
                    }
                }
                result = ConvertEpiDataTypeToSystemObject(dataType, dataValue);
            }
            else
            {
                if (value is Rule_FunctionCall)
                {
                    result = ((Rule_FunctionCall)value).Execute();
                }
                else if (value is Rule_ExprList)
                {
                    result = ((Rule_ExprList)value).Execute();
                }
                else
                {
                    result = ParseDataStrings(((String)value));
                }
            }*/
            return result;
        }

        /*
        private object ConvertEpiDataTypeToSystemObject(DataType dataType, string dataValue)
        {
            object result = null;
            if (dataValue != null)
            {
                switch (dataType)
                {
                    case DataType.Boolean:
                    case DataType.YesNo:
                        result = new Boolean();
                        if (dataValue == "(+)" || dataValue.ToLower() == "true" || dataValue == "1")
                            result = true;
                        else if (dataValue == "(-)" || dataValue.ToLower() == "false" || dataValue == "0")
                            result = false;
                        else
                            result = null;
                        break;

                    case DataType.Number:
                        double num;
                        if (double.TryParse(dataValue, out num))
                            result = num;
                        else
                            result = null;
                        break;

                    case DataType.Date:
                    case DataType.DateTime:
                    case DataType.Time:
                        DateTime dateTime;
                        if (DateTime.TryParse(dataValue, out dateTime))
                            result = dateTime;
                        else
                            result = null;
                        break;
                    case DataType.PhoneNumber:
                    case DataType.GUID:
                    case DataType.Text:
                        if (dataValue != null)
                            result = dataValue.Trim().Trim('\"');
                        else
                            result = null;
                        break;
                    case DataType.Unknown:
                    default:
                        double double_compare;
                        DateTime DateTime_compare;
                        bool bool_compare;

                        if (double.TryParse(dataValue, out double_compare))
                        {
                            result = double_compare;
                        }
                        else
                            if (DateTime.TryParse(dataValue, out DateTime_compare))
                            {
                                result = DateTime_compare;
                            }
                            else
                                if (bool.TryParse(dataValue, out bool_compare))
                                {
                                    result = bool_compare;
                                }
                                else { result = dataValue; }
                        break;
                }
            }
            return result;
        }

        private object ParseDataStrings(object subject)
        {
            object result = null;
            if (subject is Rule_ExprList)
            {
                result = ((Rule_ExprList)subject).Execute();
            }
            else if (subject is Rule_FunctionCall)
            {
                result = ((Rule_FunctionCall)subject).Execute();
            }
            else if (subject is String)
            {
                Double number;
                DateTime dateTime;

                result = ((String)subject).Trim('\"');
                //removing the "1" and "0" conditions here because an expression like 1 + 0 was evaluating as two booleans
                //if ((String)subject == "1" || (String)subject == "(+)" || ((String)subject).ToLower() == "true")
                if ((String)subject == "(+)" || ((String)subject).ToLower() == "true")
                {
                    result = new Boolean();
                    result = true;
                }
                //else if ((String)subject == "0" || (String)subject == "(-)" || ((String)subject).ToLower() == "false")
                else if ((String)subject == "(-)" || ((String)subject).ToLower() == "false")
                {
                    result = new Boolean();
                    result = false;
                }
                else if ((String)subject == "(.)")
                {
                    result = null;
                }
                else if (Double.TryParse(result.ToString(), out number))
                {
                    result = number;
                }
                else if (DateTime.TryParse(result.ToString(), out dateTime))
                {
                    result = dateTime;
                }
            }
            return result;
        }*/


    }
}
