using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;


namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Quit : AnalysisRule
    {

        public Rule_Quit(Rule_Context pContext, NonterminalToken pToken) : base(pContext){}
        

        /// <summary>
        /// performs the Quit command via the EnterCheckCodeInterface.Quit or AnalysisCheckCodeInterface.Quit methods
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.AnalysisCheckCodeInterface.Quit();

            return null;
        }
    }
}
