using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_FileDate reduction.
    /// </summary>
    public partial class Rule_FileDate : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_FileDate(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "filedate");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLowerInvariant()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLowerInvariant(), "filedate");
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns></returns>
        public override object Execute()
        {
            object result = null;

            string fileName = this.ParameterList[0].Execute().ToString();

            if (System.IO.File.Exists(fileName))
            {
                result = System.IO.File.GetLastWriteTime(fileName);
            }

            return result;
        }
        
    }
}
