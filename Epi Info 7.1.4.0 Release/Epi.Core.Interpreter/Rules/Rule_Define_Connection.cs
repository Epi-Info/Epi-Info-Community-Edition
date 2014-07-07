using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using EpiInfo.Plugin;
using Epi.Data;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Define_Connection : AnalysisRule
    {

        private string Identifier;
        private string ConnectionString;
        public Rule_Define_Connection(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<Define_Connection_Command> ::= Connection Identifier'=' BraceString 
            // Connection BraceString
            if (pToken.Tokens.Length > 3)
            {
                this.Identifier = pToken.Tokens[1].ToString();
                this.ConnectionString = pToken.Tokens[3].ToString().Trim(new[] { '{', '}' });
            }
            else
            {
                this.Identifier = "_DB";
                this.ConnectionString = pToken.Tokens[1].ToString().Trim(new[] { '{', '}' });
            }
        }


                /// <summary>
        /// performs concatenation of string via the '&' operator
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            //this.Context.isReadMode = false;
            this.Context.AddConnection(this.Identifier, this.ConnectionString);
            if(this.Identifier == "_DB")
            {
                this.Context.CurrrentConnection = this.ConnectionString;
            }
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "Define Connection");
            args.Add("Connection", ConnectionString);
            args.Add("COMMANDTEXT", "");
            this.Context.AnalysisCheckCodeInterface.Display(args);

            return result;
        }
    }
}
