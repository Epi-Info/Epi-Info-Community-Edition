using System;
using System.Collections.Generic;
using System.Text;

using com.calitha.goldparser;

using Epi.Core.EnterInterpreter;

namespace Epi.Core.EnterInterpreter.Rules
{
    /*
<FuncName1> ::= ABS
		|COS
		|DAY|DAYS
		|ENVIRON|EXISTS|EXP
		|FILEDATE|FINDTEXT|FORMAT
		|HOUR|HOURS
		|LN|LOG
		|MINUTES|Month|MONTHS
		|NUMTODATE|NUMTOTIME
		|RECORDCOUNT|RND|ROUND
		|SECOND|SECONDS|STEP|SUBSTRING|SIN
		|TRUNC|TXTTODATE|TXTTONUM|TAN
		|UPPERCASE
		|YEAR|YEARS

<FuncName2> ::= SYSTEMTIME|SYSTEMDATE

<FunctionCall> ::= <FuncName1> '(' <FunctionParameterList> ')'
			| <FuncName1> '(' <FunctionCall> ')' 
			| <FuncName2>
<FunctionParameterList> ::= <EmptyFunctionParameterList> | <NonEmptyFunctionParameterList>
<NonEmptyFunctionParameterList> ::= <MultipleFunctionParameterList> | <SingleFunctionParameterList>
<MultipleFunctionParameterList> ::= <NonEmptyFunctionParameterList> ',' <Expression>
<SingleFunctionParameterList> ::= <Expression>
<EmptyFunctionParameterList> ::=
     */

    /// <summary>
    /// Class for executing FunctionCall reductions.
    /// </summary>
    public partial class Rule_FunctionCall : EnterRule
    {
        private string functionName = null;
        private EnterRule functionCall = null;

        private string ClassName = null;
        private string MethodName = null;
        private List<EnterRule> ParameterList = new List<EnterRule>();

        #region Constructors

        /// <summary>
        /// Constructor for Rule_FunctionCall
        /// </summary>
        /// <param name="pToken">The token to build the reduction with.</param>
        public Rule_FunctionCall(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*
            <FunctionCall> ::= <FuncName1> '(' <FunctionParameterList> ')'
			    | <FuncName1> '(' <FunctionCall> ')' 
			    | <FuncName2>
             */
       
            NonterminalToken T;
            if (pToken.Tokens.Length == 1)
            {
                if (pToken.Rule.ToString().Equals("<FunctionCall>"))
                {
                    T = (NonterminalToken)pToken.Tokens[0];
                }
                else
                {
                    T = pToken;
                }
            }
            else
            {
                T = (NonterminalToken)pToken.Tokens[2];
            }


            string temp = null;
            string[] temp2 = null;

            if (pToken.Tokens[0] is NonterminalToken)
            {
                temp = this.ExtractTokens(((NonterminalToken)pToken.Tokens[0]).Tokens).Replace(" . ", ".");
                temp2 = temp.Split('.');

            }
            else
            {
                temp = ((TerminalToken)pToken.Tokens[0]).Text.Replace(" . ", ".");
            }

            if(temp2 != null && temp2.Length > 1)
            {
                this.ClassName = temp2[0].Trim();
                this.MethodName = temp2[1].Trim();

                this.ParameterList = EnterRule.GetFunctionParameters(pContext, (NonterminalToken)pToken.Tokens[2]);
            }
            else
            {
                functionName = this.GetCommandElement(pToken.Tokens, 0).ToString();

                switch (functionName.ToUpperInvariant())
                {
                    case "ABS":
                        functionCall = new Rule_Abs(pContext, T);
                        break;
                    case "COS":
                        functionCall = new Rule_Cos(pContext, T);
                        break;
                    case "CURRENTUSER":
                        functionCall = new Rule_CurrentUser(pContext, T);
                        break;
                    case "DAY":
                        functionCall = new Rule_Day(pContext, T);
                        break;
                    case "DAYS":
                        functionCall = new Rule_Days(pContext, T);
                        break;
                    case "FORMAT":
                        functionCall = new Rule_Format(pContext, T);
                        break;
                    case "HOUR":
                        functionCall = new Rule_Hour(pContext, T);
                        break;
                    case "HOURS":
                        functionCall = new Rule_Hours(pContext, T);
                        break;
                    case "ISUNIQUE":
                        functionCall = new Rule_IsUnique(pContext, T);
                        break;
                    case "LINEBREAK":
                        functionCall = new Rule_LineBreak(pContext, T);
                        break;
                    case "MINUTE":
                        functionCall = new Rule_Minute(pContext, T);
                        break;
                    case "MINUTES":
                        functionCall = new Rule_Minutes(pContext, T);
                        break;
                    case "MONTH":
                        functionCall = new Rule_Month(pContext, T);
                        break;
                    case "MONTHS":
                        functionCall = new Rule_Months(pContext, T);
                        break;
                    case "NUMTODATE":
                        functionCall = new Rule_NumToDate(pContext, T);
                        break;
                    case "NUMTOTIME":
                        functionCall = new Rule_NumToTime(pContext, T);
                        break;
                    case "RECORDCOUNT":
                        functionCall = new Rule_RecordCount(pContext, T);
                        break;
                    case "SECOND":
                        functionCall = new Rule_Second(pContext, T);
                        break;
                    case "SECONDS":
                        functionCall = new Rule_Seconds(pContext, T);
                        break;
                    case "SQRT":
                        functionCall = new Rule_SQRT_Func(pContext, T);
                        break;
                    case "SYSTEMDATE":
                        functionCall = new Rule_SystemDate(pContext, T);
                        break;
                    case "SYSTEMTIME":
                        functionCall = new Rule_SystemTime(pContext, T);
                        break;
                    case "TXTTODATE":
                        functionCall = new Rule_TxtToDate(pContext, T);
                        break;
                    case "TXTTONUM":
                        functionCall = new Rule_TxtToNum(pContext, T);
                        break;
                    case "YEAR":
                        functionCall = new Rule_Year(pContext, T);
                        break;
                    case "YEARS":
                        functionCall = new Rule_Years(pContext, T);
                        break;
                    case "SUBSTRING":
                        functionCall = new Rule_Substring(pContext, T);
                        break;
                    case "RND":
                        functionCall = new Rule_Rnd(pContext, T);
                        break;
                    case "EXP":
                        functionCall = new Rule_Exp_Func(pContext, T);
                        break;
                    case "LN":
                        functionCall = new Rule_LN_Func(pContext, T);
                        break;
                    case "ROUND":
                        functionCall = new Rule_Round(pContext, T);
                        break;
                    case "LOG":
                        functionCall = new Rule_LOG_Func(pContext, T);
                        break;
                    case "SIN":
                        functionCall = new Rule_Sin(pContext, T);
                        break;
                    case "TAN":
                        functionCall = new Rule_Tan(pContext, T);
                        break;
                    case "TRUNC":
                        functionCall = new Rule_TRUNC(pContext, T);
                        break;
                    case "STEP":
                        functionCall = new Rule_Step(pContext, T);
                        break;
                    case "UPPERCASE":
                        functionCall = new Rule_UpperCase(pContext, T);
                        break;
                    case "FINDTEXT":
                        functionCall = new Rule_FindText(pContext, T);
                        break;
                    case "ENVIRON":
                        functionCall = new Rule_FindText(pContext, T);
                        break;
                    case "EXISTS":
                        functionCall = new Rule_Exists(pContext, T);
                        break;
                    case "FILEDATE":
                        functionCall = new Rule_FileDate(pContext, T);
                        break;
                    case "ZSCORE":
                        functionCall = new Rule_ZSCORE(pContext, T);
                        break;
                    case "PFROMZ":
                        functionCall = new Rule_PFROMZ(pContext, T);
                        break;
                    case "EPIWEEK":
                        functionCall = new Rule_EPIWEEK(pContext, T);
                        break;
                    case "STRLEN":
                        functionCall = new Rule_STRLEN(pContext, T);
                        break;
                    case "GETCOORDINATES":
                        functionCall = new Rule_GetCoordinates(pContext, T);
                        break;
                    case "SENDSMS":
                        functionCall = new Rule_SendSMS(pContext, T);
                        break;
                    default:
                        throw new Exception("Function name " + functionName.ToUpperInvariant() + " is not a recognized function.");
                }
            }

        }

        

        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the result of executing the reduction.</returns>
        public override object Execute()
        {
            object result = null;
            if (string.IsNullOrEmpty(this.functionName))
            {

                if (this.Context.DLLClassList.ContainsKey(this.ClassName.ToLowerInvariant()))
                {
                    object[] args = this.ParameterList.ToArray();
                    if (this.ParameterList.Count > 0)
                    {
                        args = new object[this.ParameterList.Count];
                        for (int i = 0; i < this.ParameterList.Count; i++)
                        {
                            args[i] = this.ParameterList[i].Execute();
                        }
                    }
                    else
                    {
                        args = new object[0];
                    }

                    result = this.Context.DLLClassList[this.ClassName].Execute(this.MethodName, args);
                }
            }
            else
            {
                if (this.functionCall != null)
                {
                    result = this.functionCall.Execute();
                }
            }
            return result;
        }

        

        #endregion
    }

    /// <summary>
    /// Class for the FunctionParameterList reduction
    /// </summary>
    public partial class Rule_FunctionParameterList : EnterRule
    {
        public Stack<EnterRule> paramList = null;

        #region Constructors

        /// <summary>
        /// Constructor for Rule_FunctionParameterList
        /// </summary>
        /// <param name="pToken">The token to build the reduction with.</param>
        public Rule_FunctionParameterList(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<FunctionParameterList> ::= <EmptyFunctionParameterList>
            //<FunctionParameterList> ::= <NonEmptyFunctionParameterList>

            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];

            switch (T.Rule.Lhs.ToString())
            {
                case "<NonEmptyFunctionParameterList>":
                    this.paramList = new Stack<EnterRule>();

                    //this.paramList.Push(new Rule_NonEmptyFunctionParameterList(T, this.paramList));
                    new Rule_NonEmptyFunctionParameterList(pContext, T, this.paramList);
                    break;
                case "<SingleFunctionParameterList>":
                    this.paramList = new Stack<EnterRule>();
                    new Rule_SingleFunctionParameterList(pContext, T, this.paramList);
                    break;
                case "<EmptyFunctionParameterList>":
                    //this.paramList = new Rule_EmptyFunctionParameterList(T);
                    // do nothing the parameterlist is empty
                    break;
                case "<MultipleFunctionParameterList>":
                    this.paramList = new Stack<EnterRule>();
                    //this.MultipleParameterList = new Rule_MultipleFunctionParameterList(pToken);
                    new Rule_MultipleFunctionParameterList(pContext, T, this.paramList);
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// rule to build zero or more funtion parameters builds parameters and allows the associated function to call the parameters when needed
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            return result;
        }

        #endregion


    }

    /// <summary>
    /// Class for the Rule_EmptyFunctionParameterList reduction
    /// </summary>
    public partial class Rule_EmptyFunctionParameterList : EnterRule
    {
        #region Constructors
        public Rule_EmptyFunctionParameterList(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<EmptyFunctionParameterList> ::=
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// rule to return an empty parameter
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            return String.Empty;
        }
        #endregion
    }

    /// <summary>
    /// Class for the Rule_NonEmptyFunctionParameterList reduction.
    /// </summary>
    public partial class Rule_NonEmptyFunctionParameterList : EnterRule
    {
        protected Stack<EnterRule> MultipleParameterList = null;
        //private Reduction SingleParameterList = null;

        #region Constructors
        public Rule_NonEmptyFunctionParameterList(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<NonEmptyFunctionParameterList> ::= <MultipleFunctionParameterList>
            //<NonEmptyFunctionParameterList> ::= <SingleFunctionParameterList>
            NonterminalToken T = (NonterminalToken) pToken.Tokens[0];

            switch (T.Rule.Lhs.ToString())
            {
                case "<MultipleFunctionParameterList>":
                    this.MultipleParameterList = new Stack<EnterRule>();
                    //this.MultipleParameterList = new Rule_MultipleFunctionParameterList(pToken);
                    new Rule_MultipleFunctionParameterList(pContext, T, this.MultipleParameterList);
                    break;
                case "<SingleFunctionParameterList>":
                    //this.SingleParameterList = new Rule_SingleFunctionParameterList(pToken);
                    new Rule_SingleFunctionParameterList(pContext, T, this.MultipleParameterList);
                    break;
            }
        }

        public Rule_NonEmptyFunctionParameterList(Rule_Context pContext, NonterminalToken pToken, Stack<EnterRule> pList) : base(pContext)
        {
            //<NonEmptyFunctionParameterList> ::= <MultipleFunctionParameterList>
            //<NonEmptyFunctionParameterList> ::= <SingleFunctionParameterList>
            NonterminalToken T = (NonterminalToken) pToken.Tokens[0];

            switch (T.Rule.Lhs.ToString())
            {
                case "<MultipleFunctionParameterList>":
                    new Rule_MultipleFunctionParameterList(pContext, T, pList);

                    break;
                case "<SingleFunctionParameterList>":
                    new Rule_SingleFunctionParameterList(pContext, T, pList);
                    break;
                default:

                    break;
            }

            if (pToken.Tokens.Length > 2)
            {
                Rule_Expression Expression = new Rule_Expression(pContext, (NonterminalToken)pToken.Tokens[2]);
                pList.Push(Expression);
            }
        }
        #endregion
        #region Public Methods

        /// <summary>
        /// builds a multi parameters list which is executed in the calling function's execute method.
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            return null;
        }
        #endregion
    }

    /// <summary>
    /// Class for the Rule_MultipleFunctionParameterList reduction.
    /// </summary>
    public partial class Rule_MultipleFunctionParameterList : EnterRule
    {
        private EnterRule Expression = null;
        private EnterRule nonEmptyList = null;
        #region Constructors
        public Rule_MultipleFunctionParameterList(Rule_Context pContext, NonterminalToken pToken, Stack<EnterRule> pList) : base(pContext)
        {
            //<MultipleFunctionParameterList> ::= <NonEmptyFunctionParameterList> ',' <Expression>

            NonterminalToken nonEmptyToken = (NonterminalToken)pToken.Tokens[0];
            NonterminalToken ExpressionToken = (NonterminalToken)pToken.Tokens[2];
           // nonEmptyList = new Rule_NonEmptyFunctionParameterList(pContext, nonEmptyToken, pList);
            //this.Expression = new Rule_Expression(pContext, ExpressionToken);

            pList.Push(EnterRule.BuildStatments(pContext, nonEmptyToken));
            pList.Push(EnterRule.BuildStatments(pContext, ExpressionToken));

            //pList.Push(this.Expression);
        }
        #endregion
        #region Public Methods

        /// <summary>
        /// assists in building a multi parameters list which is executed in the calling function's execute method.
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            //nonEmptyList.Execute();

            //result = Expression.Execute();

            return result;
        }
        #endregion
    }

    /// <summary>
    /// Class for the Rule_SingleFunctionParameterList reduction.
    /// </summary>
    public partial class Rule_SingleFunctionParameterList : EnterRule
    {
        private EnterRule Expression = null;

        #region Constructors
        public Rule_SingleFunctionParameterList(Rule_Context pContext, NonterminalToken pToken, Stack<EnterRule> pList) : base(pContext)
        {
            //<SingleFunctionParameterList> ::= <Expression>
            this.Expression = new Rule_Expression(pContext, (NonterminalToken)pToken.Tokens[0]);
            pList.Push(this.Expression);
        }
        #endregion
        #region Public Methods

        /// <summary>
        /// executes the parameter expression of a function.
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            result = this.Expression.Execute();
            return result;
        }
        #endregion
    }













    //****
    //**** Not implemented yet, but on the list of features
    //****
    //public partial class Rule_Uppercase : Reduction
    //{
    //    private Reduction functionCallOrParamList = null;
    //    private string type;
    //    private List<Reduction> reductions = new List<Reduction>();
    //    private List<object> reducedValues = null;
    //    private string fullString = null;

    //    public Rule_Uppercase(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
    //    {
    //        //UPPERCASE(fullString)

    //        NonterminalToken T = (NonterminalToken)pToken.Tokens[0];
    //        type = pToken.Rule.Lhs.ToString();
    //        switch (type)
    //        {
    //            case "<FunctionParameterList>":
    //                this.functionCallOrParamList = new Rule_FunctionParameterList(T);
    //                string tmp = this.GetCommandElement(pToken.Tokens, 0);
    //                reductions.Add(new Rule_Value(tmp));

    //                break;
    //            case "<FunctionCall>":
    //                this.functionCallOrParamList = new Rule_FunctionCall(T);
    //                break;
    //            default:
    //                break;
    //        }
    //    }

    //    public override object Execute()
    //    {
    //        object result = null;

    //        reducedValues = new List<object>();

    //        reducedValues.Add(FunctionUtils.StripQuotes(reductions[0].Execute().ToString()));
    //        fullString = (string)reducedValues[0];

    //        result = fullString.ToUpperInvariant();

    //        return result;
    //    }
    //}









    /// <summary>
    /// Utility class for helper methods for the Epi Functions.
    /// </summary>
    public static class FunctionUtils
    {
        public enum DateInterval
        {
            Second,
            Minute,
            Hour,
            Day,
            Month,
            Year
        }

        /// <summary>
        /// Gets the appropriate date value based on the date and interval.
        /// </summary>
        /// <param name="interval">The interval to retrieve from the date.</param>
        /// <param name="date">The date to get the value from.</param>
        /// <returns></returns>
        public static object GetDatePart(DateInterval interval, DateTime date)
        {
            object returnValue = null;
            switch (interval)
            {
                case DateInterval.Second:
                    returnValue = date.Second;
                    break;
                case DateInterval.Minute:
                    returnValue = date.Minute;
                    break;
                case DateInterval.Hour:
                    returnValue = date.Hour;
                    break;
                case DateInterval.Day:
                    returnValue = date.Day;
                    break;
                case DateInterval.Month:
                    returnValue = date.Month;
                    break;
                case DateInterval.Year:
                    returnValue = date.Year;
                    break;
            }

            return returnValue;
        }

        /// <summary>
        /// Gets the difference between two dates based on an interval.
        /// </summary>
        /// <param name="interval">The interval to use (seconds, minutes, hours, days, months, years)</param>
        /// <param name="date1">The date to use for comparison.</param>
        /// <param name="date2">The date to compare against the first date.</param>
        /// <returns></returns>
        public static object GetDateDiff(DateInterval interval, DateTime date1, DateTime date2)
        {
            object returnValue = null;
            TimeSpan t;

            double diff = 0;

            //returns negative value if date1 is more recent
            t = date2 - date1;

            switch (interval)
            {
                case DateInterval.Second:
                    diff = t.TotalSeconds;
                    break;
                case DateInterval.Minute:
                    diff = t.TotalMinutes;
                    break;
                case DateInterval.Hour:
                    diff = t.TotalHours;
                    break;
                case DateInterval.Day:
                    diff = t.TotalDays;
                    break;
                case DateInterval.Month:
                    diff = t.TotalDays / 365.25 * 12.0;
                    break;
                case DateInterval.Year:
                    diff = t.TotalDays / 365.25;
                    break;
            }

            returnValue = Convert.ToInt32(diff);
            return returnValue;
        }

        /// <summary>
        /// Removes all double quotes from a string.
        /// </summary>
        /// <param name="s">The string to remove quotes from.</param>
        /// <returns>Returns the modified string with no double quotes.</returns>
        public static string StripQuotes(string s)
        {
            return s.Trim(new char[] { '\"' });
        }
    }
}

