using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Beep : AnalysisRule
    {
        bool hasExecuted = false;

        public Rule_Beep(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {

        }

        /// <summary>
        /// sends a "beep" command to the system
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            if (!hasExecuted)
            {
                Console.Beep();
                this.hasExecuted = true;
            }

            return null;
        }
    }
}
