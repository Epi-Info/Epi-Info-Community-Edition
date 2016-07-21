using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public partial class Rule_FindText : EnterRule
    {
        
        private List<EnterRule> ParameterList = new List<EnterRule>();


        public Rule_FindText(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SUBSTRING(fullString,startingIndex,length)
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "findtext");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLowerInvariant()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLowerInvariant(), "findtext");
            //            }
            //        }
            //    }
            //}
        }
        /// <summary>
        /// returns a substring index is 1 based ie 1 = first character
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            object p1 = this.ParameterList[0].Execute();
            object p2 = this.ParameterList[1].Execute();

            if (p1 != null && p2 != null)
            {
                result = p2.ToString().IndexOf(p1.ToString(), StringComparison.OrdinalIgnoreCase) + 1;
            }

            return result;
        }
    }
}
