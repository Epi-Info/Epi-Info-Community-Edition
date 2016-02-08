using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_DateDiff reduction.
    /// This is the base class for:
    /// Rule_Years
    /// Rule_Months
    /// Rule_Days
    /// Rule_Hours
    /// Rule_Minutes
    /// Rule_Seconds
    /// </summary>
    public abstract partial class Rule_DateDiff : EnterRule
    {
        #region Private Variables

        private List<EnterRule> ParameterList = new List<EnterRule>();

        private FunctionUtils.DateInterval currentInterval;
        #endregion

        #region Constructors
        /// <summary>
        /// Reduction to calculate the difference between two dates.
        /// </summary>
        /// <param name="pToken">The token to use to build the reduction.</param>
        /// <param name="interval">The date interval to use for calculating the difference (seconds, hours, days, months, years)</param>
        public Rule_DateDiff(Rule_Context pContext, NonterminalToken pToken, FunctionUtils.DateInterval interval)
            : base(pContext)
        {
            currentInterval = interval;
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            if (ParameterList.Count > 0)
            {
                foreach (var item in ParameterList)
                {
                    if (item is Rule_Value)
                    {
                        var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
                        if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
                        {
                            this.Context.CommandVariableCheck.Add(id.ToLower(), "datediff");
                        }
                    }
                }
            }
            /*
            NonterminalToken T = (NonterminalToken)pToken.Tokens[0];
            string type = pToken.Rule.Lhs.ToString();
            switch (type)
            {
                case "<FunctionParameterList>":
                    this.ParamList = new Rule_FunctionParameterList(pContext, pToken);
                    break;
                case "<FunctionCall>":
                    this.functionCall = new Rule_FunctionCall(pContext, T);
                    break;
                default:
                    break;
            }*/
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the difference between two dates.</returns>
        public override object Execute()
        {
            List<string> test = new List<string>();
            object result = null;

            if (this.ParameterList.Count == 1)
            {
                result = this.ParameterList[0].Execute();
            }
            else
            {

                DateTime date1, date2;
                if (this.ParameterList[0].Execute() != null)
                {
                    if (DateTime.TryParse(this.ParameterList[0].Execute().ToString(), out date1))
                    {
                        if (this.ParameterList[1].Execute() != null)
                        {
                            if (DateTime.TryParse(this.ParameterList[1].Execute().ToString(), out date2))
                            {
                                result = FunctionUtils.GetDateDiff(currentInterval, date1, date2);
                            }
                        }
                    }
                }
            }

            // To prevent a null value in the param list from returning a zero
            //if (result == null)
            //{
            //    result = 0;
            //}

            return result;
        }
        #endregion
    }
}
