using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Beep : EnterRule
    {
        public Rule_Beep(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {

        }

        /// <summary>
        /// sends a "beep" command to the system
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            Console.Beep();

            return null;
        }
    }
}
