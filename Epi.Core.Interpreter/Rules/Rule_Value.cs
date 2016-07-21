using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_Value : AnalysisRule
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
                    DateTime dateTime_temp;
                    switch (T.Rule.Rhs[0].ToString())
                    {
                        case "<Qualified ID>":
                            this.Id = this.SetQualifiedId(T.Tokens[0]);
                            break;
                        case "Identifier":
                            this.Id = this.GetCommandElement(T.Tokens, 0).Trim(new char[] {'[',']'});
                            break;
                        case "(":
                        case "<FunctionCall>":
                            this.value = new Rule_FunctionCall(pContext, (NonterminalToken)T.Tokens[1]);
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
                            if (string_temp == "(+)" || string_temp.ToLowerInvariant() == "true" || string_temp.ToLowerInvariant() == "yes")
                            {
                                this.value = true;
                            }
                            else if (string_temp == "(-)" || string_temp.ToLowerInvariant() == "false" || string_temp.ToLowerInvariant() == "no")
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
                            if (double.TryParse(this.GetCommandElement(T.Tokens, 0), out Decimal_temp))
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
                            this.value = this.GetCommandElement(T.Tokens, 0).Trim('"');;
                            break;
                    }
                }
                else
                {
                    //this.value = new Rule_ExprList(pContext, (NonterminalToken)T.Tokens[1]);
                    //this.value = AnalysisRule.BuildStatments(pContext, T.Tokens[1]);
                    if (T.Tokens.Length == 0)
                    {
                        this.value = AnalysisRule.BuildStatments(pContext, T);

                    }
                    else
                    {
                        this.value = AnalysisRule.BuildStatments(pContext, T.Tokens[1]);
                    }
                }
            }
            else
            {
                TerminalToken TT = (TerminalToken)pToken;
                DateTime dateTime_temp;
                switch (TT.Symbol.ToString())
                {
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
                        if (string_temp == "(+)" || string_temp.ToLowerInvariant() == "true" || string_temp.ToLowerInvariant() == "yes")
                        {
                            this.value = true;
                        }
                        else if (string_temp == "(-)" || string_temp.ToLowerInvariant() == "false" || string_temp.ToLowerInvariant() == "no")
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
                        this.value = TT.Text.Trim('"');;
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

                if (this.Id != null)
                {
                    IVariable var;
                    DataType dataType = DataType.Unknown;
                    string dataValue = string.Empty;

                    if (this.Context.MemoryRegion.TryGetVariable(this.Id, out var))
                    {
                        if (var.VarType == VariableType.Standard || var.VarType == VariableType.DataSource)
                        {
                            if (this.Context.CurrentDataRow != null)
                            {
                                if (var.VarType == VariableType.Standard)
                                {
                                    if (this.Context.Recodes.ContainsKey(var.Name))
                                    {
                                        Epi.Core.AnalysisInterpreter.Rules.RecodeList RL = this.Context.Recodes[var.Name];
                                        result = RL.GetRecode(this.Context.CurrentDataRow[RL.SourceName]);
                                    }
                                    else
                                    {
                                        //result = ParseDataStrings(this.Context.CurrentDataRow[this.Id].ToString());
                                        result = this.Context.CurrentDataRow[this.Id];
                                    }
                                }
                                else
                                {
                                    //result = ParseDataStrings(this.Context.CurrentDataRow[this.Id].ToString());
                                    result = this.Context.CurrentDataRow[this.Id];
                                }

                                if (result is System.DBNull)
                                {
                                    result = "";
                                }
                            }
                            else // there is NO current row go through datarows
                            {
                                this.Context.GetOutput(GetResultFromDataTable);

                                result = ReturnResult;
                            }
                        }
                        else
                        {
                            dataType = var.DataType;
                            dataValue = var.Expression;
                            result = ConvertEpiDataTypeToSystemObject(dataType, dataValue);
                        }
                    }
                    else if (this.Context.CurrentDataRow != null)
                    {

                        if (this.Context.Recodes.ContainsKey(this.Id))
                        {
                            Epi.Core.AnalysisInterpreter.Rules.RecodeList RL = this.Context.Recodes[this.Id];
                            result = RL.GetRecode(this.Context.CurrentDataRow[RL.SourceName]);
                        }
                        else
                        {
                            result = this.Context.CurrentDataRow[this.Id];
                            if (result is System.DBNull)
                            {
                                result = null;
                            }
                        }
                    }
                }
                else
                {
                    if (value is AnalysisRule)
                    {
                        result = ((AnalysisRule)value).Execute();
                    }
                    else
                    {
                        result = value;
                    }

                }
            
            
            return result;
        }

        private object ConvertEpiDataTypeToSystemObject(DataType dataType, string dataValue)
        {
            object result = null;
            if (dataValue != null)
            {
                DateTime dateTime;
                switch (dataType)
                {
                    case DataType.Boolean:
                    case DataType.YesNo:
                        result = new Boolean();
                        if (dataValue == "(+)" || dataValue.ToLowerInvariant() == "true" || dataValue == "1")
                            result = true;
                        else if (dataValue == "(-)" || dataValue.ToLowerInvariant() == "false" || dataValue == "0")
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
                        
                    case DataType.DateTime:
                    case DataType.Time:

                        if (DateTime.TryParse(dataValue, out dateTime))
                        {
                            result = dateTime;
                        }
                        else
                        {
                            result = null;
                        }
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
                //if ((String)subject == "1" || (String)subject == "(+)" || ((String)subject).ToLowerInvariant() == "true")
                if ((String)subject == "(+)" || ((String)subject).ToLowerInvariant() == "true")
                {
                    result = new Boolean();
                    result = true;
                }
                //else if ((String)subject == "0" || (String)subject == "(-)" || ((String)subject).ToLowerInvariant() == "false")
                else if ((String)subject == "(-)" || ((String)subject).ToLowerInvariant() == "false")
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

        private void GetResultFromDataTable()
        {
            object result = null;
            
            if (this.Context.Recodes.ContainsKey(this.Id))
            {
                Epi.Core.AnalysisInterpreter.Rules.RecodeList RL = this.Context.Recodes[this.Id];
                result = RL.GetRecode(this.Context.CurrentDataRow[RL.SourceName]);
            }
            else
            {
                result = ParseDataStrings(this.Context.CurrentDataRow[this.Id].ToString());
            }
            
            if (result is System.DBNull)
            {
                result = null;
            }

            ReturnResult = result;
        }
    }
}
