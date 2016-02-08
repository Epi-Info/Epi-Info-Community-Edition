using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public partial class Rule_STRLEN : EnterRule
    {
        object _result = null;
        object _fullString = null;
        object _startIndex = 0;
        object _length = 0;

        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_STRLEN(Rule_Context pContext, NonterminalToken pToken)
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
                            this.Context.CommandVariableCheck.Add(id.ToLower(), "strlen");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// returns a substring index is 1 based ie 1 = first character
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            _fullString = null;
            _length = null;

            _fullString = this.ParameterList[0].Execute();

            if (_fullString is string)
            { 
                string fullString = _fullString.ToString();
                _length = fullString.Length;
            }

            return _length;
        }
    }
}
