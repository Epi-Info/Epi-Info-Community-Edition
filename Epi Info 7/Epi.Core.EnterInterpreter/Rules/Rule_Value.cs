using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Value : EnterRule
    {
        public string Id = null;
        string Namespace = null;
        object value = null;
        int Index0 = -1;
        object Index1 = null;


        //object ReturnResult = null;

        public Rule_Value(Rule_Context pContext, Token pToken) : base(pContext)
        {
            /* ::= Identifier	| <Literal> | Boolean | '(' <Expr List> ')' */

            if (pToken is NonterminalToken)
            {
                NonterminalToken T = (NonterminalToken)pToken;
                if (T.Tokens.Length == 1)
                {
                    DateTime dateTime_temp;
                    switch (T.Symbol.ToString())
                    {
                        case "<GridFieldId>":
                            /*<GridFieldId> ::= Identifier'[' <Number>',' Identifier ']'  
                            | Identifier '[' <Number> ',' <Number> ']' 
                            | Identifier '[' <Number> ',' <Literal_String> ']'*/
                            this.Id = this.GetCommandElement(T.Tokens, 0);
                            int Int_temp;
                            if (int.TryParse(this.GetCommandElement(T.Tokens, 2), out Int_temp))
                            {
                                this.Index0 = Int_temp;
                            }


                            if (int.TryParse(this.GetCommandElement(T.Tokens, 4), out Int_temp))
                            {
                                this.Index1 = Int_temp;
                            }
                            else
                            {
                                //this.Index1 = this.GetCommandElement(T.Tokens, 4);
                                this.Index1 = new Rule_Value(this.Context, T.Tokens[4]);

                            }
                            break;
                        case "<Qualified ID>":
                        case "Identifier":
                            this.Id = this.GetCommandElement(T.Tokens, 0);
                            break;
                        case "<FunctionCall>":
                            this.value = EnterRule.BuildStatments(pContext, T.Tokens[0]);
                            break;
                        case "<Literal>":
                            this.value = this.GetCommandElement(T.Tokens, 0).Trim('"');
                            break;
                        case "<Literal_String>":
                        case "String":
                             this.value = this.GetCommandElement(T.Tokens, 0).Trim('"');
                            break;
                        case "<Literal_Char>":
                        case "CharLiteral":
                            this.value = this.GetCommandElement(T.Tokens, 0).Trim('\'');
                            break;
                        case "Boolean":
                            string string_temp = this.GetCommandElement(T.Tokens, 0);
                            if (string_temp == "(+)" || string_temp.ToLower() == "true" || string_temp.ToLower() == "yes")
                            {
                                this.value = true;
                            }
                            else if (string_temp == "(-)" || string_temp.ToLower() == "false" || string_temp.ToLower() == "no")
                            {
                                this.value = false;
                            }
                            else if (string_temp == "(.)")
                            {
                                this.value = null;
                            }
                            break;
                        case "<Decimal_Number>":
                        case "<Number>":
                        case "<Real_Number>":
                        case "<Hex_Number>":
                        case "DecLiteral":
                        case "RealLiteral":
                        case "HexLiteral":
                            double Decimal_temp;
                           // if (double.TryParse(this.GetCommandElement(T.Tokens, 0), out Decimal_temp))
                            if (double.TryParse(this.GetCommandElement(T.Tokens, 0), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out Decimal_temp))
                            {
                                this.value = Decimal_temp;
                            }
                            break;
                        case "<Literal_Date>":
                        case "Date":
                            if (DateTime.TryParse(this.GetCommandElement(T.Tokens, 0), out dateTime_temp))
                            {
                                DateTime dt = new DateTime(dateTime_temp.Year, dateTime_temp.Month, dateTime_temp.Day);
                                this.value = dt;
                            }
                            else
                            {
                                this.value = null;
                            }
                            break;
                        case "<Literal_Time>":
                        case "<Literal_Date_Time>":
                        case "Time":
                        case "Date Time":
                            if (DateTime.TryParse(this.GetCommandElement(T.Tokens, 0), out dateTime_temp))
                            {
                                this.value = dateTime_temp;
                            }
                            break;

                        case "<Value>":
                            this.value = new Rule_Value(this.Context, T.Tokens[0]);
                            break;
                        default:
                            this.value = this.GetCommandElement(T.Tokens, 0);
                            break;
                    }
                }
                else
                {
                    //this.value = new Rule_ExprList(pContext, (NonterminalToken)T.Tokens[1]);
                    if (T.Tokens.Length == 0)
                    {
                        this.value = EnterRule.BuildStatments(pContext, T);

                    }
                    else if (T.Symbol.ToString() == "<Fully_Qualified_Id>" || T.Symbol.ToString() == "<Qualified ID>")
                    {
                        string[] temp = this.ExtractTokens(T.Tokens).Split(' ');
                        this.Namespace = temp[0];
                        this.Id = temp[2];
                    }
                    else if (T.Symbol.ToString() == "<GridFieldId>")
                    {
                        /*<GridFieldId> ::= Identifier'[' <Number>',' Identifier ']'  
                        | Identifier '[' <Number> ',' <Number> ']' 
                        | Identifier '[' <Number> ',' <Literal_String> ']'*/
                        this.Id = this.GetCommandElement(T.Tokens, 0);
                        int Int_temp;
                        if (int.TryParse(this.GetCommandElement(T.Tokens, 2), out Int_temp))
                        {
                            this.Index0 = Int_temp;
                        }


                        if (int.TryParse(this.GetCommandElement(T.Tokens, 4), out Int_temp))
                        {
                            this.Index1 = Int_temp;
                        }
                        else
                        {
                            //this.Index1 = this.GetCommandElement(T.Tokens, 4);
                            this.Index1 = new Rule_Value(this.Context, T.Tokens[4]);
                        }
                    }
                    else
                    {
                        this.value = EnterRule.BuildStatments(pContext, T.Tokens[1]);
                    }
                }
            }
            else
            {
                TerminalToken TT = (TerminalToken)pToken;
                DateTime dateTime_temp;
                switch (TT.Symbol.ToString())
                {
                    case "<GridFieldId>":
                        ///*<GridFieldId> ::= Identifier'[' <Number>',' Identifier ']'  
                        //| Identifier '[' <Number> ',' <Number> ']' 
                        //| Identifier '[' <Number> ',' <Literal_String> ']'*/
                        //this.Id = this.GetCommandElement(T.Tokens, 0);
                        //int Int_temp;
                        //if (int.TryParse(this.GetCommandElement(T.Tokens, 2), out Int_temp))
                        //{
                        //    this.Index0 = Int_temp;
                        //}


                        //if (int.TryParse(this.GetCommandElement(T.Tokens, 4), out Int_temp))
                        //{
                        //    this.Index1 = Int_temp;
                        //}
                        //else
                        //{
                        //    this.Index1 = this.GetCommandElement(T.Tokens, 4);
                        //}
                        //break;
                    case "Identifier":
                        this.Id = TT.Text;
                        break;
                    case "<Literal>":
                        this.value = TT.Text.Trim('"');
                        break;
                    case "<Literal_String>":
                    case "String":
                        this.value = TT.Text.Trim('"');
                        break;
                    case "<Literal_Char>":
                    case "CharLiteral":
                        this.value = TT.Text.Trim('\'');
                        break;
                    case "Boolean":
                        string string_temp = TT.Text;
                        if (string_temp == "(+)" || string_temp.ToLower() == "true" || string_temp.ToLower() == "yes")
                        {
                            this.value = true;
                        }
                        else if (string_temp == "(-)" || string_temp.ToLower() == "false" || string_temp.ToLower() == "no")
                        {
                            this.value = false;
                        }
                        else if (string_temp == "(.)")
                        {
                            this.value = null;
                        }
                        break;
                    case "<Decimal_Number>":
                    case "<Number>":
                    case "<Real_Number>":
                    case "<Hex_Number>":
                    case "DecLiteral":
                    case "RealLiteral":
                    case "HexLiteral":
                        double Decimal_temp;
                        if (double.TryParse(TT.Text, out Decimal_temp))
                        {
                            this.value = Decimal_temp;
                        }
                        break;
                    case "<Literal_Date>":
                    case "Date":
                        if (DateTime.TryParse(TT.Text, out dateTime_temp))
                        {
                            DateTime dt = new DateTime(dateTime_temp.Year, dateTime_temp.Month, dateTime_temp.Day);
                            this.value = dt;
                        }
                        else
                        {
                            this.value = null;
                        }
                        break;
                    case "<Literal_Time>":
                    case "<Literal_Date_Time>":
                    case "Time":
                    case "Date Time":
                        if (DateTime.TryParse(TT.Text, out dateTime_temp))
                        {
                            this.value = dateTime_temp;
                        }
                        break;
                    default:
                        this.value = TT.Text;
                        break;
                }
            }

            if (this.Id == null && this.value == null)
            {

            }

            if (this.value != null && this.value.Equals("END-IF"))
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

            if (this.Id != null)
            {
                EpiInfo.Plugin.IVariable var;
                EpiInfo.Plugin.DataType dataType = EpiInfo.Plugin.DataType.Unknown;
                string dataValue = string.Empty;

                var = Context.CurrentScope.Resolve(this.Id, this.Namespace);

                if (var != null && !(var.VariableScope ==  EpiInfo.Plugin.VariableScope.DataSource))
                {
                    dataType = var.DataType;
                    dataValue = var.Expression;
                }
                else
                {
                    if (this.Context.EnterCheckCodeInterface != null)
                    {
                        EpiInfo.Plugin.DataType dt = EpiInfo.Plugin.DataType.Unknown;

                        if (this.Index0 == -1)
                        {
                            this.Context.EnterCheckCodeInterface.TryGetFieldInfo(this.Id, out dt, out dataValue);
                            dataType = (EpiInfo.Plugin.DataType)dt;
                        }
                        else
                        {
                            if (this.Index1 is Rule_Value)
                            {
                                this.Index1 = ((Rule_Value)this.Index1).Execute();
                            }

                            EpiInfo.Plugin.IVariable iv = this.Context.EnterCheckCodeInterface.GetGridValue(this.Id, this.Index0, this.Index1);
                            dataValue = iv.Expression;
                            dataType = iv.DataType;
                        }
                    }
                }
                result = ConvertEpiDataTypeToSystemObject(dataType, dataValue);
            }
            else
            {
                if (value is EnterRule)
                {
                    result = ((EnterRule)value).Execute();
                }
                else 
                {
                    
                    result = value;
                }
            }
            return result;
        }


        private object ConvertEpiDataTypeToSystemObject(EpiInfo.Plugin.DataType dataType, string dataValue)
        {
            object result = null;
            if (dataValue != null)
            {
                DateTime dateTime;
                switch (dataType)
                {
                    case EpiInfo.Plugin.DataType.Boolean:
                    case EpiInfo.Plugin.DataType.YesNo:
                        result = new Boolean();
                        if (dataValue == "(+)" || dataValue.ToLower() == "true" || dataValue == "1" || dataValue.ToLower() == "yes")
                            result = true;
                        else if (dataValue == "(-)" || dataValue.ToLower() == "false" || dataValue == "0" || dataValue.ToLower() == "no")
                            result = false;
                        else
                            result = null;
                        break;

                    case EpiInfo.Plugin.DataType.Number:
                        double num;
                        if (double.TryParse(dataValue, out num))
                            result = num;
                        else
                            result = null;
                        break;
                    case EpiInfo.Plugin.DataType.Date:
                        if (DateTime.TryParse(dataValue, out dateTime))
                        {
                            DateTime dt = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
                            result = dt;
                        }
                        else
                        {
                            result = null;
                        }
                        break;

                    case EpiInfo.Plugin.DataType.DateTime:
                    case EpiInfo.Plugin.DataType.Time:

                        if (DateTime.TryParse(dataValue, out dateTime))
                        {
                            result = dateTime;
                        }
                        else
                        {
                            result = null;
                        }
                        break;
                    case EpiInfo.Plugin.DataType.PhoneNumber:
                    case EpiInfo.Plugin.DataType.GUID:
                    case EpiInfo.Plugin.DataType.Text:
                        if (dataValue != null)
                            result = dataValue.Trim('\"');
                        else
                            result = null;
                        break;
                    case EpiInfo.Plugin.DataType.Unknown:
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
                if ((String)subject == "(+)" || ((String)subject).ToLower() == "true" || ((String)subject).ToLower() == "yes")
                {
                    result = new Boolean();
                    result = true;
                }
                //else if ((String)subject == "0" || (String)subject == "(-)" || ((String)subject).ToLower() == "false")
                else if ((String)subject == "(-)" || ((String)subject).ToLower() == "false" || ((String)subject).ToLower() == "no")
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
        }

    }
}
