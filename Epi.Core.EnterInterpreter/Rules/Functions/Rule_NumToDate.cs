using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to convert number values to a date.
    /// </summary>
    public partial class Rule_NumToDate : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_NumToDate(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "numtodate");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLowerInvariant()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLowerInvariant(), "numtodate");
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the value converted to a date.</returns>
        public override object Execute()
        {
            object result = null;

            int year;
            int month;
            int day;

            if (Int32.TryParse(ParameterList[0].Execute().ToString(), out year))
            {
                if (Int32.TryParse(ParameterList[1].Execute().ToString(), out month))
                {
                    if (Int32.TryParse(ParameterList[2].Execute().ToString(), out day))
                    {
                        result = new DateTime(year, month, day);
                    }
                }
            }

            return result;
        }
    }
}
