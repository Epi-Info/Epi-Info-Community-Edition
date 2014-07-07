using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Always : EnterRule
    {
        EnterRule statements = null;

        public Rule_Always(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // ALWAYS <Statements> END
            this.statements = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
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
