using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public partial class Rule_TxtToDate : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_TxtToDate(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "txttodate");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLower(), "txttodate");
            //            }
            //        }
            //    }
            //}
        }
        /// <summary>
        /// returns a date based on a text string
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            result = ParameterList[0].Execute();

            DateTime dateValue;

            if (!Util.IsEmpty(result))
            {
                if (DateTime.TryParse(result.ToString(), out dateValue))
                {
                    result = dateValue;
                }
            }

            return result;
        }
    }
}
