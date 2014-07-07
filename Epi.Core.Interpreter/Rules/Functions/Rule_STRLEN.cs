using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_STRLEN : AnalysisRule
    {
        object _result = null;
        object _fullString = null;
        object _startIndex = 0;
        object _length = 0;

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_STRLEN(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
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
