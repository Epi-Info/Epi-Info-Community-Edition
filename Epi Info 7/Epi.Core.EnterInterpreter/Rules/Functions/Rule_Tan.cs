using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Tan reduction
    /// </summary>
    public partial class Rule_Tan : EnterRule
    {

        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Tan(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "tan");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLowerInvariant()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLowerInvariant(), "tan");
            //            }
            //        }
            //    }
            //}
        }
        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the Tan of a number.</returns>
        public override object Execute()
        {
            double result = 0.0;
            if (Double.TryParse(this.ParameterList[0].Execute().ToString(), out result))
            {

                return Math.Tan(result);
            }
            else
            {
                return null;
            }

        }
    }
}
