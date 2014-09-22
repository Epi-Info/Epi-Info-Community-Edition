using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using System.IO;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Data.Services;
using Epi.DataSets;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    class Rule_Printout:AnalysisRule
    {
        //<Simple_Print_Out_Statement> 					::= PRINTOUT 
        //<File_Print_Out_Statement> 					::= PRINTOUT File

        bool HasRun = false;

        string file = string.Empty;

        public Rule_Printout(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            if (pToken.Tokens.Length > 1)
            {
                file = this.GetCommandElement(pToken.Tokens, 1);
            }
        }


        /// <summary>
        /// performs the PrintOut command via the AnalysisCheckCodeInterface.Printout method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            if (!this.HasRun)
            {
                this.Context.AnalysisCheckCodeInterface.Printout(file);
                this.HasRun = true;
            }
            return result;
        }
    }
}
