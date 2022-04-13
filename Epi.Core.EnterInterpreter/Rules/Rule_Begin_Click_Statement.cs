using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Begin_Click_Statement : EnterRule
    {
        public EnterRule Statements = null;

        public Rule_Begin_Click_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<Begin_Click_statement> ::= Begin-Click <Statements> End  | Begin-Click End | !Null
            if (pToken.Tokens.Length > 2)
            {
                //NonterminalToken T = (NonterminalToken)pToken.Tokens[1];
                //this.Statements = new Rule_Statements(pContext, T);
                this.Statements = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
            }
        }

        /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (this.Statements != null && this.Context.EnterCheckCodeInterface.IsExecutionEnabled)
            {
                try
                {
                    result = this.Statements.Execute();
                }
                catch (Exception ex)
                {
                    if (this.Context.EnterCheckCodeInterface.IsSuppressErrorsEnabled)
                    {
                        Logger.Log(string.Format("{0} - EnterInterpreter Execute : source [{1}]\n message:\n{2}", DateTime.Now, ex.Source, ex.Message));
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// To String method
        /// </summary>
        /// <returns>object</returns>
        public override string ToString()
        {
            return base.ToString();
        }

        public override bool IsNull() { return this.Statements == null; } 
    }
}
