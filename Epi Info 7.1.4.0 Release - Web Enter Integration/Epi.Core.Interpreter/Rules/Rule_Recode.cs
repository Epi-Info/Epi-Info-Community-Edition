using System;
using System.Collections.Generic;
using System.Text;

using com.calitha.goldparser;

using Epi.Core.AnalysisInterpreter;
using Epi.Data;

namespace Epi.Core.AnalysisInterpreter.Rules
{

    public class Rule_Recode : AnalysisRule
    {
        string Identifier1 = null;
        string Identifier2 = null;
        new AnalysisRule RecodeList = null;
        private RecodeList _recodeList = null;
        public Rule_Recode(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            /*
            <Recode_Statement>                      ::= RECODE Identifier TO Identifier  <RecodeList> END
            <RecodeList> ::= <Recode> | <RecodeList> <Recode> 
            <RecodeValue> ::= <Literal> | Boolean | Identifier | LOVALUE | HIVALUE
            <Recoded_Value> ::= <Literal> | Boolean | Identifier
            <Recode> ::=      <RecodeValue>  '-' <RecodeValue>  '=' <Recoded_Value> 
                | <RecodeValue>  '=' <RecodeValue> 
                | ELSE '=' <Recoded_Value> 
             */

            // RECODE Identifier TO Identifier <RecodeList> END
            this.Identifier1 = this.GetCommandElement(pToken.Tokens, 1).Trim(new char[] { '[',']' });
            this.Identifier2 = this.GetCommandElement(pToken.Tokens, 3).Trim(new char[] { '[', ']' });
            this._recodeList = new RecodeList(this.Identifier2, this.Identifier1);
            this.SetRecodeList((NonterminalToken) pToken.Tokens[4]);
            //RecodeRules = this.GetCommandElement(pToken.Tokens, 5).Split(new char[] {'\r','\n' }, StringSplitOptions.RemoveEmptyEntries);
            //this.RecodeList = new Rule_RecodeList(pContext, (NonterminalToken)pToken.Tokens[4]);
        }


        private void SetRecodeList(NonterminalToken pToken)
        {
            //<RecodeList> ::= <Recode> | <RecodeList> <Recode> 

            if (pToken is NonterminalToken)
            {
                NonterminalToken NT = (NonterminalToken)pToken;
                switch (NT.Symbol.ToString())
                {
                    case "<Recode>":
                        this.SetRecode(NT);
                        break;
                    case "<RecodeList>":
                        this.SetRecodeList((NonterminalToken)NT.Tokens[0]);
                        this.SetRecode((NonterminalToken)NT.Tokens[1]);
                        break;
                }
            }
        }

        private void SetRecode(NonterminalToken pToken)
        {
            object fromValue = null;
            object rangeEnd = null;
            object equalValue = null;
            string recodeType = null;
            
            /*
            <Recode> ::=      <Value> '-' <Value> '=' <Value>
                | <Value> '–' HIVALUE '=' <Value>
                | LOVALUE '-' <Value> '=' <Value>
                | <Value> '=' <Value>
                | ELSE '=' <Value>
             */
            switch (pToken.Tokens.Length)
            {
                    /*
                case 3:
                    fromValue = GetRecodeValue(pToken.Tokens[0]);
                    equalValue =  GetRecodeValue(pToken.Tokens[2]);
                    recodeType = ((TerminalToken)pToken.Tokens[1]).Text;

                    RecodeRange recodeRange = new RecodeRange(recodeType, fromValue, rangeEnd, equalValue);
                    _recodeList.Ranges.Add(recodeRange);
                    break;

                default:
                    fromValue = GetRecodeValue(pToken.Tokens[0]);
                    rangeEnd = GetRecodeValue(pToken.Tokens[2]);
                    equalValue = GetRecodeValue(pToken.Tokens[4]);
                    recodeType = ((TerminalToken)pToken.Tokens[1]).Text;
                    _recodeList.Ranges.Add(new RecodeRange(recodeType, fromValue, rangeEnd, equalValue));
                    break;*/

                case 3:
                    fromValue = new Rule_Value(this.Context, pToken.Tokens[0]);
                    equalValue = new Rule_Value(this.Context, pToken.Tokens[2]);
                    recodeType = ((TerminalToken)pToken.Tokens[1]).Text;

                    RecodeRange recodeRange = new RecodeRange(recodeType, fromValue, rangeEnd, equalValue);
                    _recodeList.Ranges.Add(recodeRange);
                    break;

                default:
                    fromValue = new Rule_Value(this.Context, pToken.Tokens[0]);
                    rangeEnd = new Rule_Value(this.Context, pToken.Tokens[2]);
                    equalValue = new Rule_Value(this.Context, pToken.Tokens[4]);
                    recodeType = ((TerminalToken)pToken.Tokens[1]).Text;
                    _recodeList.Ranges.Add(new RecodeRange(recodeType, fromValue, rangeEnd, equalValue));
                    break;

            }
        }

        private object GetEqualsValue(Token pToken)
        {
            object result = null;

            if (pToken is NonterminalToken)
            {
                NonterminalToken NT = (NonterminalToken)pToken;

                //<RecodeValue> ::= <Literal> | Boolean | Identifier
                
                string dataTypeIdentifier = NT.Rule.Rhs[0].ToString();

                switch (dataTypeIdentifier)
                {
                    case "<Literal>":
                        result = this.GetCommandElement(NT.Tokens, 0).Replace("\"", "").Trim();
                        break;
                    case "DecLiteral":
                        result = this.GetCommandElement(NT.Tokens, 0).Replace("\"", "").Trim();
                        break;

                    default:
                        result = this.ExtractTokens(NT.Tokens).Replace("\"", "").Trim();
                        break;
                }
            }
            else
            {
                TerminalToken TT = (TerminalToken)pToken;
                result = TT.ToString();
            }

            return result;
        }

        
        private object GetRecodeValue(Token token)
        {
            object result = null;

            if (token is NonterminalToken)
            {
                NonterminalToken nonterm = (NonterminalToken)token;
                string os = Environment.OSVersion.VersionString; 

                //<RecodeValue> ::= <Literal> | Boolean | Identifier

                string name = nonterm.Tokens[0].ToString();
                string symbolName = nonterm.Symbol.Name;

                switch (symbolName)
                {
                    case "Real_Number":
                    case "Decimal_Number":
                        string commandElementString = this.GetCommandElement(nonterm.Tokens, 0).Replace("\"", "").Trim();
                        double commandElementDouble = double.NaN;
                        System.Double.TryParse(commandElementString, out commandElementDouble);
                        if (commandElementDouble != double.NaN)
                        {
                            result = commandElementDouble;
                        }
                        break;
                    case "Literal_Date":
                        result = DateTime.Parse(name);
                        break;
                    case "Literal_String":
                    case "RecodeValue":
                    case "Recoded_Value":
                        result = this.GetCommandElement(nonterm.Tokens, 0).Replace("\"", "").Trim();
                        break;
                }
            }
            else
            {
                TerminalToken TT = (TerminalToken)token;
                result = TT.ToString();
            }

            return result;
        }

        /// <summary>
        /// builds a RecodeList based on the supplied values and saves the List in the this.Context.Recodes property
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            string KeyName = _recodeList.VariableName.ToUpper();

            if (this.Context.Recodes.ContainsKey(KeyName))
            {
                this.Context.Recodes[KeyName] = _recodeList;
            }
            else
            {
                this.Context.Recodes.Add(KeyName, _recodeList);
            }

            

            if (this.Context.DataSet.Tables.Contains("Output"))
            {
                this.Context.GetOutput(RecodeMapFunction);
            }
            
            return null;
        }

        private void RecodeMapFunction()
        {
            if (this.Context.DataSet.Tables["Output"].Columns.Contains(_recodeList.SourceName))
            {
                if (this.Context.CurrentDataRow[_recodeList.SourceName] == DBNull.Value)
                {
                    this.Context.CurrentDataRow[_recodeList.VariableName] = DBNull.Value;
                }
                else
                {
                    object sourceValue = _recodeList.GetRecode(Context.CurrentDataRow[_recodeList.SourceName]);

                    if (sourceValue == null || (sourceValue is string && string.IsNullOrEmpty((string)sourceValue)))
                    {
                        this.Context.CurrentDataRow[_recodeList.VariableName] = DBNull.Value;
                    }
                    else
                    {
                        this.Context.CurrentDataRow[_recodeList.VariableName] = _recodeList.GetRecode(this.Context.CurrentDataRow[_recodeList.SourceName]);
                    }
                }
            }
        }
    }

    public class RecodeRange
    {
        protected static List<string> NumericTypeList = new List<string>(new string[] { "INT", "FLOAT", "INT16", "INT32", "INT64", "SINGLE", "DOUBLE", "BYTE", "DECIMAL" });

        private string recodeType = null;
        private object fromValue = null;
        private object rangeEnd = null;
        private object equalValue = null;

        public object EqualValue
        {
            get
            {
                if (equalValue is Rule_Value)
                {
                    return ((Rule_Value)equalValue).Execute();
                }
                else
                {
                    return equalValue;
                }
            }
            set { equalValue = value; }
        }

        public object RangeEnd
        {
            get 
            {
                if (rangeEnd is Rule_Value)
                {
                    return ((Rule_Value)rangeEnd).Execute();
                }
                else
                {
                    return rangeEnd;
                }
            }
            set { rangeEnd = value; }
        }

        public object FromValue
        {
            get 
            {
                if (fromValue is Rule_Value)
                {
                    return ((Rule_Value)fromValue).Execute();
                }
                else
                {

                    return fromValue;
                }
            
            }
            set { fromValue = value; }
        }

        public string RecodeType
        {
            get { return recodeType; }
            set { recodeType = value; }
        }

        public RecodeRange() { }

        public RecodeRange(string pRecodeType, object pValue1, object pValue2, object pEqualValue)
        {
            RecodeType = pRecodeType;
            fromValue = pValue1;
            rangeEnd = pValue2;
            EqualValue = pEqualValue;
        }

        /// <summary>
        /// checks if a value is within a recode range
        /// </summary>
        /// <param name="setValue"></param>
        /// <returns>bool</returns>
        public bool ValueIsInRange(object pValue)
        {
            bool result = false;
            bool check1 = false;
            bool check2 = false;

            double double_compare;
            DateTime DateTime_compare;
            bool bool_compare;

            int i = 0;

            object LHSO = null;
            object RHSO = null;

            object fromValue = null;
            if (this.fromValue is Rule_Value)
            {
                fromValue = ((Rule_Value)this.fromValue).Execute();
            }
            else
            {
                fromValue = this.fromValue;
            }

            object EqualValue = null;
            if (this.EqualValue is Rule_Value)
            {
                EqualValue = ((Rule_Value)this.EqualValue).Execute();
            }
            else
            {
                EqualValue = this.EqualValue;
            }


            object rangeEnd = null;
            if (this.rangeEnd is Rule_Value)
            {
                rangeEnd = ((Rule_Value)this.rangeEnd).Execute();
            }
            else
            {
                fromValue = this.fromValue;
            }




            if (fromValue is string && ((string)fromValue).ToUpper() == "ELSE")
            {
                return true;
            }
            else
            {
                if (fromValue.ToString().ToUpper() == "LOVALUE")
                {
                    if (double.TryParse(pValue.ToString(), out double_compare))
                    {
                        LHSO = double.MinValue;
                    }
                    else
                    {
                        if (bool.TryParse(pValue.ToString(), out bool_compare))
                        {
                            LHSO = bool.FalseString;
                        }
                        else
                        {
                            LHSO = string.Empty;
                        }
                    }
                }
                else
                {
                    if (fromValue is string)
                    {
                        string from = (string)fromValue;

                        if (bool.TryParse(from, out bool_compare))
                        {
                            LHSO = bool_compare;
                        }
                        else if (fromValue.Equals("(+)") || fromValue.Equals("(-)") || fromValue.Equals("(.)"))
                        {
                            switch (from)
                            {
                                case "(+)":
                                    LHSO = true;
                                    break;
                                case "(-)":
                                    LHSO = false;
                                    break;
                                case "(.)":
                                    LHSO = null;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            LHSO = from.ToUpper();
                        }
                    }
                    else 
                    {
                        LHSO = fromValue;
                    }
                }
            }
            
            if (double.TryParse(pValue.ToString(), out double_compare))
            {
                RHSO = double_compare;
            }
            else
            {
                if (bool.TryParse(pValue.ToString(), out bool_compare))
                {
                    RHSO = bool_compare;
                }
                else if (pValue.ToString().Equals("(+)") || pValue.ToString().Equals("(-)") || pValue.ToString().Equals("(.)"))
                {
                    switch (pValue.ToString())
                    {
                        case "(+)":
                            RHSO = true;
                            break;
                        case "(-)":
                            RHSO = false;
                            break;
                        case "(.)":
                            RHSO = null;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (pValue != null)
                    {
                        if (pValue is DateTime)
                        {
                            RHSO = pValue;
                        }
                        else
                        {
                            RHSO = pValue.ToString().ToUpper();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(RHSO.ToString()))
            {
                if (LHSO is IComparable && RHSO is IComparable)
                {
                    if (LHSO is DateTime && !(RHSO is DateTime))
                    {
                        DateTime dt;
                        if (DateTime.TryParse(RHSO.ToString(), out dt))
                            i = ((IComparable)LHSO).CompareTo((IComparable)dt);
                        else i = 0;
                    }
                    else
                        i = ((IComparable)LHSO).CompareTo((IComparable)RHSO);
                }

                if (this.recodeType.ToUpper() == "=")
                {
                    check1 = i == 0;
                }
                else
                {
                    check1 = i <= 0;
                }
            }
            else if (this.recodeType.ToUpper() == "RECODEC")
            {
                if (Util.IsEmpty(LHSO))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (double.TryParse(pValue.ToString(), out double_compare))
            {
                LHSO = double_compare;
            }
            else
            {
                if (DateTime.TryParse(pValue.ToString(), out DateTime_compare))
                {
                    LHSO = DateTime_compare;
                }
                else
                {
                    if (bool.TryParse(pValue.ToString(), out bool_compare))
                    {
                        LHSO = bool_compare;
                    }
                    else
                    {
                        if (pValue != null)
                        {
                            LHSO = pValue.ToString().ToUpper();
                        }
                    }
                }
            }

            if (rangeEnd.ToString().ToUpper() == "HIVALUE")
            {
                if (double.TryParse(pValue.ToString(), out double_compare))
                {
                    RHSO = double.MaxValue;
                }
                else
                {
                    if (DateTime.TryParse(pValue.ToString(), out DateTime_compare))
                    {
                        RHSO = DateTime.MaxValue;
                    }
                    else
                    {
                        if (bool.TryParse(pValue.ToString(), out bool_compare))
                        {
                            RHSO = bool.TrueString;
                        }
                        else
                        { 
                            RHSO = string.Empty; 
                        }
                    }
                }
            }
            else
            {
                if (rangeEnd is string)
                {
                    RHSO = ((string)rangeEnd).ToUpper();
                }
                else
                {
                    RHSO = rangeEnd;
                }
            }

            if (!string.IsNullOrEmpty(LHSO.ToString()))
            {
                if (LHSO.GetType() == typeof(DateTime) && RHSO.GetType() != typeof(DateTime))
                {
                    if (LHSO is IComparable && (Convert.ToDateTime(RHSO) is IComparable))
                    {
                        i = ((IComparable)LHSO).CompareTo(Convert.ToDateTime(RHSO));
                    }
                }
                else
                {
                    if (LHSO is IComparable && RHSO is IComparable)
                    {
                        i = ((IComparable)LHSO).CompareTo((IComparable)RHSO);
                    }

            }

                if (this.recodeType.ToUpper() == "=")
                {
                    check2 = i == 0;
                }
                else
                {
                    check2 = (i <= 0);
                }
            }
            
            result = check1 && check2;
            return result;
        }



        public bool IsValue(object pValue)
        {
            bool result = false;

            double CheckNumber = 0.0;
            double CheckNumber2 = 0.0;

            object fromValue = null;
            if (this.fromValue is Rule_Value)
            {
                fromValue = ((Rule_Value)this.fromValue).Execute();
            }
            else
            {
                    fromValue = this.fromValue;
            }

            if (fromValue.Equals("ELSE"))
            {
                return true;
            }
            else if (fromValue is string && pValue is string)
            {
                if (fromValue.ToString().ToUpper() == pValue.ToString().ToUpper())
                {
                    return true;
                }
            }
            else 
            {
                if ((pValue.GetType().Name.ToUpper() != fromValue.GetType().Name.ToUpper()) && RecodeRange.NumericTypeList.Contains(fromValue.GetType().Name.ToUpper()) && RecodeRange.NumericTypeList.Contains(pValue.GetType().Name.ToUpper()))
                {
                    CheckNumber = Convert.ToDouble(fromValue);
                    CheckNumber2 = Convert.ToDouble(pValue);
                    return CheckNumber == CheckNumber2;

                }
                else if (fromValue.Equals(pValue))
                {
                    return true;
                }
            }

            return result;
        }
    }

    public class RecodeList
    {
        public List<RecodeRange> Ranges = new List<RecodeRange>();
        public string VariableName;
        public string SourceName;

        public RecodeList(string pVariableName, string pSourceName)
        {
            this.VariableName = pVariableName;
            this.SourceName = pSourceName;
        }

        public object GetRecode(object pValue)
        {
            object result = null;


            foreach (RecodeRange range in this.Ranges)
            {
                object FromValue = range.FromValue;
                object EqualValue = range.EqualValue;
                object RangeEnd = range.RangeEnd;

                if (RangeEnd == null || (RangeEnd is string && string.IsNullOrEmpty((string)RangeEnd)))
                {
                    if (range.IsValue(pValue))
                    {
                        if (EqualValue is string)
                        {
                            string val = EqualValue.ToString();
                            if (val == "(+)")
                            {
                                result = true;
                            }
                            else if (val == "(-)")
                            {
                                result = false;
                            }
                            else if (val == "(.)")
                            {

                                result = null;
                            }
                            else
                            {
                                result = val;
                            }
                        }
                        else
                        {
                            result = EqualValue;
                        }
                        break;
                    }
                }
                else
                { 
                    if (range.ValueIsInRange(pValue))
                    {
                        if (EqualValue is string)
                        {
                            string val = EqualValue.ToString();
                            if (val == "(+)")
                            {
                                result = true;
                            }
                            else if (val == "(-)")
                            {
                                result = false;
                            }
                            else if (val == "(.)")
                            {

                                result = null;
                            }
                            else
                            {
                                result = val;
                            }
                        }
                        else
                        {
                            result = EqualValue;
                        }
                        break;
                    }
                }
            }

            return result;
        }
    }

    public class Rule_RecodeList : AnalysisRule
    {
        AnalysisRule Recode = null;
        new AnalysisRule RecodeList;
        public Rule_RecodeList(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<Recode> 
            //<RecodeList> <Recode> 
            NonterminalToken RecodeToken = (NonterminalToken)pToken.Tokens[0];

            if (pToken.Tokens.Length > 1)
            {
                this.RecodeList = new Rule_RecodeList(pContext, (NonterminalToken)pToken.Tokens[0]);
                RecodeToken = (NonterminalToken)pToken.Tokens[1];
            }

            //"<Recode_F> | <Recode_G> | <Recode_H> | <Recode_I> | <Recode_J> | <Recode_K> | <Recode_L> | <Recode_M> | <Recode_N>"
            switch (RecodeToken.Rule.Rhs[0].ToString())
            {
                case "<Recode_A>":
                    this.Recode = new Rule_Recode_A(pContext, RecodeToken);
                    break;
                case "<Recode_B>":
                    this.Recode = new Rule_Recode_B(pContext, RecodeToken);
                    break;
                case "<Recode_C>":
                    this.Recode = new Rule_Recode_C(pContext, RecodeToken);
                    break;
                case "<Recode_D>":
                    this.Recode = new Rule_Recode_D(pContext, RecodeToken);
                    break;
                case "<Recode_E>":
                    this.Recode = new Rule_Recode_E(pContext, RecodeToken);
                    break;
                case "<Recode_F>":
                    this.Recode = new Rule_Recode_F(pContext, RecodeToken);
                    break;
                case "<Recode_G>":
                    this.Recode = new Rule_Recode_G(pContext, RecodeToken);
                    break;
                case "<Recode_H>":
                    this.Recode = new Rule_Recode_H(pContext, RecodeToken);
                    break;
                case "<Recode_I>":
                    this.Recode = new Rule_Recode_I(pContext, RecodeToken);
                    break;
                case "<Recode_J>":
                    this.Recode = new Rule_Recode_J(pContext, RecodeToken);
                    break;
                case "<Recode_K>":
                    this.Recode = new Rule_Recode_K(pContext, RecodeToken);
                    break;
                case "<Recode_L>":
                    this.Recode = new Rule_Recode_L(pContext, RecodeToken);
                    break;
                case "<Recode_M>":
                    this.Recode = new Rule_Recode_M(pContext, RecodeToken);
                    break;
                case "<Recode_N>":
                    this.Recode = new Rule_Recode_N(pContext, RecodeToken);
                    break;
                default:
                    //Should not be here
                    break;
            }
        }

        public override object Execute()
        {
            object result = null;
            if (this.RecodeList != null)
            {
                this.RecodeList.Execute();
            }

            result = this.Recode.Execute();

            return result;
        }
    }

    //<Recode_A> 	::= <Literal> '-' <Literal> '=' <Literal>
    class Rule_Recode_A : AnalysisRule
    {
        string recodeType = "RecodeA";
        string value1 = null;
        string value2 = null;
        string equalValue = null;

        public Rule_Recode_A(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];

            //this.value1 = this.CreateNegateRecode(T.Tokens[0]);
            //this.value2 = this.CreateNegateRecode(T.Tokens[2]);

            this.value1 = this.GetCommandElement(T.Tokens, 0);
            this.value2 = this.ExtractTokens(((NonterminalToken)T.Tokens[2]).Tokens);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 4).Replace("\"", "");

        }
        public override object Execute()
        {
            //AddMe to the RecodeList
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, this.value2, this.equalValue)));
            return null;
        }
    }
    //<Recode_B> ::= <Literal> '=' <Literal>
    class Rule_Recode_B : AnalysisRule
    {
        string recodeType = "RecodeB";
        string value1 = null;
        string equalValue = null;

        public Rule_Recode_B(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //this.value1 = this.CreateNegateRecode(pToken.Tokens[0]);
            this.value1 = this.GetCommandElement(pToken.Tokens, 0).Replace("\"", "");
            this.equalValue = this.GetCommandElement(pToken.Tokens, 2).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, null, this.equalValue)));
            return null;
        }
    }
    //<Recode_C>							::= Boolean '=' <Literal> 	
    class Rule_Recode_C : AnalysisRule
    {
        string recodeType = "RecodeC";
        string value1 = null;
        string equalValue = null;

        public Rule_Recode_C(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            this.value1 = this.GetCommandElement(pToken.Tokens, 0).Replace("\"", "");
            this.equalValue = this.GetCommandElement(pToken.Tokens, 2).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, null, this.equalValue)));
            return null;
        }
    }
    //<Recode_D>							::= LOVALUE '-' <Literal> '=' <Literal>
    class Rule_Recode_D : AnalysisRule
    {
        string recodeType = "RecodeD";
        string value1 = "LOVALUE";
        string value2 = null;
        string equalValue = null;

        public Rule_Recode_D(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];
            //this.value2 = this.CreateNegateRecode(T.Tokens[2]);
            this.value2 = this.GetCommandElement(T.Tokens,2);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 4).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, this.value2, this.equalValue)));
            return null;
        }

    }
    //<Recode_E>							::= <Literal> '-' HIVALUE '=' <Literal>
    class Rule_Recode_E : AnalysisRule
    {
        string recodeType = "RecodeE";
        string value1 = null;
        string value2 = "HIVALUE";
        string equalValue = null;

        public Rule_Recode_E(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];

            //this.value1 = this.CreateNegateRecode(T.Tokens[0]);
            this.value1 = this.GetCommandElement(T.Tokens,0);
            //this.value2 = this.GetCommandElement(pToken.Tokens, 2);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 4).Replace("\"", "");

        }
        public override object Execute()
        {
            //AddMe to the RecodeList
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, this.value2, this.equalValue)));
            return null;
        }
    }
    //<Recode_F>							::= LOVALUE '-' HIVALUE '=' <Literal>
    class Rule_Recode_F : AnalysisRule
    {
        string recodeType = "RecodeF";
        string value1 = "LOVALUE";
        string value2 = "HIVALUE";
        string equalValue = null;

        public Rule_Recode_F(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //this.value1 = this.GetCommandElement(pToken.Tokens, 0);
            //this.value2 = this.GetCommandElement(pToken.Tokens, 2);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 4).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, this.value2, this.equalValue)));
            return null;
        }
    }
    //<Recode_G>							::= ELSE '=' <Literal>
    class Rule_Recode_G : AnalysisRule
    {
        string recodeType = "RecodeG";
        string value1 = "ELSE";
        string equalValue = null;

        public Rule_Recode_G(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //this.value1 = this.GetCommandElement(pToken.Tokens, 0);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 2).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, null, this.equalValue)));
            return null;
        }
    }
    //<Recode_H>							::= <Literal> '-' <Literal> '=' Boolean
    class Rule_Recode_H : AnalysisRule
    {
        string recodeType = "RecodeH";
        string value1 = null;
        string value2 = null;
        string equalValue = null;

        public Rule_Recode_H(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];

            //this.value1 = this.CreateNegateRecode(T.Tokens[0]);
            //this.value2 = this.CreateNegateRecode(T.Tokens[2]);
            this.value1 = this.GetCommandElement(T.Tokens,0);
            this.value2 = this.GetCommandElement(T.Tokens,2);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 4).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, this.value2, this.equalValue)));
            return null;
        }

    }
    //<Recode_I>							::= <Literal> '=' Boolean
    class Rule_Recode_I : AnalysisRule
    {
        string recodeType = "RecodeI";
        string value1 = null;
        string equalValue = null;

        public Rule_Recode_I(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //this.value1 = this.CreateNegateRecode(pToken.Tokens[0]);
            this.value1 = this.GetCommandElement(pToken.Tokens,0);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 2).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, null, this.equalValue)));
            return null;
        }
    }
    //<Recode_J>							::= Boolean '=' Boolean
    class Rule_Recode_J : AnalysisRule
    {
        string recodeType = "RecodeJ";
        string value1 = null;
        string equalValue = null;

        public Rule_Recode_J(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            this.value1 = this.GetCommandElement(pToken.Tokens, 0).Replace("\"", "");
            this.equalValue = this.GetCommandElement(pToken.Tokens, 2).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, null, this.equalValue)));
            return null;
        }
    }
    //<Recode_K>							::= LOVALUE '-' <Literal> '=' Boolean
    class Rule_Recode_K : AnalysisRule
    {
        string recodeType = "RecodeK";
        string value1 = "LOVALUE";
        string value2 = null;
        string equalValue = null;

        public Rule_Recode_K(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];

            //this.value2 = this.CreateNegateRecode(T.Tokens[2]);
            this.value2 = this.GetCommandElement(T.Tokens,2);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 4).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, this.value2, this.equalValue)));
            return null;
        }

    }
    //<Recode_L>							::= <Literal> '-' HIVALUE '=' Boolean
    class Rule_Recode_L : AnalysisRule
    {
        string recodeType = "RecodeL";
        string value1 = null;
        string value2 = "HIVALUE";
        string equalValue = null;

        public Rule_Recode_L(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];

            //this.value1 = this.CreateNegateRecode(T.Tokens[0]);
            this.value1 = this.GetCommandElement(T.Tokens,0);

            this.equalValue = this.GetCommandElement(pToken.Tokens, 4).Replace("\"", "");

        }
        public override object Execute()
        {
            //AddMe to the RecodeList
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, this.value2, this.equalValue)));
            return null;
        }
    }
    //<Recode_M>							::= LOVALUE '-' HIVALUE '=' Boolean
    class Rule_Recode_M : AnalysisRule
    {
        string recodeType = "RecodeM";
        string value1 = "LOVALUE";
        string value2 = "HIVALUE";
        string equalValue = null;

        public Rule_Recode_M(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //this.value1 = this.GetCommandElement(pToken.Tokens, 0);
            //this.value2 = this.GetCommandElement(pToken.Tokens, 2);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 4).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, this.value2, this.equalValue)));
            return null;
        }
    }
    //<Recode_N>							::= ELSE '=' Boolean
    class Rule_Recode_N : AnalysisRule
    {
        string recodeType = "RecodeN";
        string value1 = "ELSE";
        string equalValue = null;

        public Rule_Recode_N(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //this.value1 = this.GetCommandElement(pToken.Tokens, 0);
            this.equalValue = this.GetCommandElement(pToken.Tokens, 2).Replace("\"", "");

        }
        public override object Execute()
        {
            this.Context.RecodeList.Add(this.Context.RecodeList.Count, new System.Collections.DictionaryEntry(this.recodeType, new RecodeRange(this.recodeType, this.value1, null, this.equalValue)));
            return null;
        }
    }
}
