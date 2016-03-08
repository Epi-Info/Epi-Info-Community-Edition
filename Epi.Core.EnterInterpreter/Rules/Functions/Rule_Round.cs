

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Round reduction.
    /// </summary>
    public partial class Rule_Round : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Round(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "round");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLower(), "round");
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the rounded value of a number.</returns>
        public override object Execute()
        {
            object result = null;
            object p1 = this.ParameterList[0].Execute().ToString(); // the number to round
            object p2 = 0; 

            int param2 = 0;

            if (this.ParameterList.Count == 2) // if provided, use this value to specify the number of decimal places to round to; otherwise round to a whole number
            {
                p2 = this.ParameterList[1].Execute().ToString();
                Int32.TryParse(p2.ToString(), out param2);
            }

            double param1;

            if (Double.TryParse(p1.ToString(), out param1))
            {
                result = Math.Round(param1, param2, MidpointRounding.AwayFromZero);
            }

            return result;
        }
        
    }
}
