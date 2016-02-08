using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Format reduction.
    /// </summary>
    public partial class Rule_Format : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Format(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
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
                            this.Context.CommandVariableCheck.Add(id.ToLower(), "format");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// formats a string based on the format parameter
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            if (ParameterList.Count > 1)
            {
                object LHSO = ParameterList[0].Execute();
                object RHSO = ParameterList[1].Execute();

                if (LHSO != null && !string.IsNullOrEmpty(LHSO.ToString().Trim()) && RHSO != null)
                {

                    Type ToDataType = LHSO.GetType();

                    if (ToDataType is IConvertible)
                    {
                        result = Microsoft.VisualBasic.Strings.Format(LHSO, RHSO.ToString());
                    }
                    else
                    {
                        if (ToDataType.ToString().Equals("System.TimeSpan", StringComparison.OrdinalIgnoreCase))
                        {
                            
                            LHSO = new DateTime(((TimeSpan)LHSO).Ticks);
                            result = Microsoft.VisualBasic.Strings.Format(LHSO, RHSO.ToString());
                        }
                        else
                        {
                            result = Microsoft.VisualBasic.Strings.Format(LHSO, RHSO.ToString());
                        }
                    }


                }
            }
            else
            {
                result = ParameterList[0].Execute().ToString();
            }

            return result;
        }
    }


}
