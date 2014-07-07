using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_Substring : AnalysisRule
    {
        object _result = null;
        object _fullString = null;
        object _startIndex = 0;
        object _length = 0;

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_Substring(Rule_Context pContext, NonterminalToken pToken)
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
            _result = null;
            _fullString = null;
            _startIndex = 0;
            _length = 0;

            _fullString = this.ParameterList[0].Execute();            

            if (!Util.IsEmpty(_fullString))
            {
                string fullString = _fullString.ToString();

                _startIndex = this.ParameterList[1].Execute();
                int start = int.Parse(_startIndex.ToString());
                

                if (this.ParameterList.Count > 2)
                {
                    _length = this.ParameterList[2].Execute();
                }
                else
                {
                    _length = fullString.Length;
                }

                int length = int.Parse(_length.ToString());

                if (start + length > fullString.Length)
                {
                    length = fullString.Length - start + 1;
                }
                if (start <= fullString.Length)
                {
                    _result = fullString.Substring(start - 1, length);
                }
                else
                {
                    _result = "";
                }
            }

            return _result;
        }
    }
}
