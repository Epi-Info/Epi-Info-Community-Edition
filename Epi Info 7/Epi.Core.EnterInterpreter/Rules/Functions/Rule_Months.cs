using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Months reduction.
    /// </summary>
    public partial class Rule_Months : Rule_DateDiff
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Months(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken, FunctionUtils.DateInterval.Year)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "months");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLower(), "months");
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the date difference in years between two dates.</returns>
        public override object Execute()
        {
            object result = null;

            object p1 = this.ParameterList[0].Execute();
            object p2 = this.ParameterList[1].Execute();

            if (p1 is DateTime && p2 is DateTime)
            {
                DateTime param1 = (DateTime)p1;
                DateTime param2 = (DateTime)p2;

                int monthsApart = 12 * (param2.Year - param1.Year) + param2.Month - param1.Month;

                if (param2.Day < param1.Day)
                {
                    monthsApart--;
                }

                result = monthsApart;
                //int age = param2.Year - param1.Year;
                //if
                //(
                //    param2.Month < param1.Month ||
                //    (param2.Month == param1.Month && param2.Day < param1.Day)
                //)
                //{
                //    age--;

                //}

                //age *= 12;

                //int months;
                //if (param2.Month > param1.Month)
                //{
                //    months = param2.Month - param1.Month;

                //    if (param2.Day < param1.Day)
                //    {
                //        months--;
                //    }
                //}
                //else if (param2.Month == param1.Month && param2.Day < param1.Day)
                //{
                //    months = 11;
                //}
                //else
                //{
                //    months = 12 - param1.Month;
                //    months = months + param2.Month;

                //    if (param2.Day < param1.Day)
                //    {
                //        months--;
                //    }

                //}

                //result = age + months;
            }

            return result;
        }
    }
}
