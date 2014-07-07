using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Quit : EnterRule
    {

        public Rule_Quit(Rule_Context pContext)
            : base(pContext)
        {

        }

        /// <summary>
        /// performs the Quit command via the EnterCheckCodeInterface.Quit or AnalysisCheckCodeInterface.Quit methods
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.Quit();
            return null;
        }
    }
}
