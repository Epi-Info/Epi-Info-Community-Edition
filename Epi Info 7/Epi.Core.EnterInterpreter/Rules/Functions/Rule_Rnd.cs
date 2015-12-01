using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Rnd reduction.
    /// </summary>
    public partial class Rule_Rnd : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();
        private static Random random = new Random(DateTime.Now.Millisecond);

        public Rule_Rnd(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            if (ParameterList.Count > 0)
            {
                foreach (var item in ParameterList)
                {
                    var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
                    if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
                    {
                        this.Context.CommandVariableCheck.Add(id.ToLower(), "rnd");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns a random number.</returns>
        public override object Execute()
        {
            object result = null;
            string min = string.Empty;
            string max = string.Empty;
            int param1;
            int param2;

            if (this.ParameterList.Count == 2)
            {
                min = this.ParameterList[0].Execute().ToString();
                max = this.ParameterList[1].Execute().ToString();

                if (int.TryParse(min, out param1) && int.TryParse(max, out param2))
                {
                    result = random.Next(param1, param2);
                }
            }
            else
            {
                max = this.ParameterList[0].Execute().ToString();
                if (int.TryParse(max, out param1))
                {
                    result = random.Next(param1);
                }
            }

            return result;
        }
    }
}
