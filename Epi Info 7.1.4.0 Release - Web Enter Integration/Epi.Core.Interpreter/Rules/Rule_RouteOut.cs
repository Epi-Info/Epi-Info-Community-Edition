using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using System.IO;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_RouteOut : AnalysisRule
    {
        bool HasRun = false;
        /// <summary>
        /// The name of the file that will receive the ROUTEOUT.
        /// </summary>
        string fileName = string.Empty;
        /// <summary>
        /// Replace as opposed to append.
        /// </summary>
        bool isReplace = false;

        /// <summary>
        /// Constructor - Rule_Routeout
        /// <para>Assign local variables from NonterminalToken.</para>
        /// </summary>
        /// <param name="pToken"></param>
        public Rule_RouteOut(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // <Simple_Routeout_Statement>  ::= ROUTEOUT File
            // <Replace_Routeout_Statement> ::= ROUTEOUT File REPLACE
            // <Append_Routeout_Statement>  ::= ROUTEOUT File APPEND
            
            fileName = GetCommandElement(pToken.Tokens, 1).Trim('\"');

            if( pToken.Tokens.Length >= 3)
            {
                if( GetCommandElement(pToken.Tokens, 2).ToUpper().Equals("REPLACE"))
                {
                    isReplace = true;    
                }
            }
        }


       /// <summary>
        /// performs execution of the RouteOut command via the AnalysisCheckCodeInterface.ChangeOutput method
       /// </summary>
       /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            if (!this.HasRun)
            {
                this.Context.AnalysisCheckCodeInterface.ChangeOutput(fileName, isReplace, true);
                this.HasRun = true;
            }
            return result;
        }
    }
}
