using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;


namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_CloseOut : AnalysisRule
    {
        bool HasRun = false;

        public Rule_CloseOut(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // <Close_Out_Statement>    ::= CLOSEOUT
        }

        /// <summary>
        /// performs the CloseOut command via the AnalysisCheckCodeInterface.ChangeOutput method
        /// </summary>
        /// <returns></returns>
        public override object Execute()
        {
            if (!this.HasRun)
            {
                this.Context.AnalysisCheckCodeInterface.ChangeOutput(String.Empty, false, false);
                this.HasRun = true;
            }
            return null;
        }
    }
}
