using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Help : EnterRule
    {
        string FileName = null;
        string AnchorString = null;
        public Rule_Help(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            //<Help_Statement> ::= HELP file String
            // file = filepath
            // String = Anchor

            this.FileName = this.GetCommandElement(pToken.Tokens,1);
            this.AnchorString = this.GetCommandElement(pToken.Tokens, 2);
        }

        /// <summary>
        /// executes the GOTO command via the EnterCheckCodeInterface.GoTo method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            if (!string.IsNullOrEmpty(this.FileName))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = this.FileName;
                proc.StartInfo.UseShellExecute = true;
                // to do get the anchor string working
                //proc.StartInfo.Arguments = string.Format("/project:\"{0}\" /view:\"{1}\"", projectFilePath, viewName);
                proc.Start();
            }

            return null;
        }
    }
}
