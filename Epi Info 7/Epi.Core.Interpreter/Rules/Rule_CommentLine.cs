using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_CommentLine :AnalysisRule
    {

        public Rule_CommentLine(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //do nothing
        }


        /// <summary>
        /// comment line it doesn't perform any function
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            //do nothing
            return null;
        }
    }
}
