using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Always : AnalysisRule
    {
        AnalysisRule statements = null;

        public Rule_Always(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // ALWAYS <Statements> END
            this.statements = new Rule_Statements(pContext, (NonterminalToken)pToken.Tokens[1]);
        }

        /// <summary>
        /// executes the enclosed expressions
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object results = null;

            if (this.statements != null)
            {
                results = statements.Execute();
            }

            return results;
        }
    }
}
