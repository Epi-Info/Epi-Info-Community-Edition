using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Define_Group : AnalysisRule
    {
        string Identifier = null;
        List<string> IdentifierList = new List<string>();

        public Rule_Define_Group(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<Define_Group_Statement> ::= DEFINE Identifier GROUPVAR <IdentifierList>
            this.Identifier = this.GetCommandElement(pToken.Tokens, 1);
            this.IdentifierList.AddRange(this.GetCommandElement(pToken.Tokens, 3).Split(' '));
        }

        /// <summary>
        /// peforms the Define GroupVAR
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {

            IVariable var = null;

            if (this.Context.MemoryRegion.IsVariableInScope(Identifier))
            {

                if (this.Context.MemoryRegion.TryGetVariable(this.Identifier, out var))
                {
                    if (var.VarType != VariableType.Permanent)
                    {
                        this.Context.AnalysisCheckCodeInterface.Dialog(SharedStrings.DUPLICATE_VARIABLE_DEFINITION + StringLiterals.COLON + Identifier, CommandNames.DEFINE);
                    }

                }
                else
                {
                    this.Context.AnalysisCheckCodeInterface.Dialog(SharedStrings.DUPLICATE_VARIABLE_DEFINITION + StringLiterals.COLON + Identifier, CommandNames.DEFINE);
                }

            }

            else
            {

                if (this.Context.GroupVariableList.ContainsKey(this.Identifier))
                {
                    this.Context.GroupVariableList[this.Identifier].Clear();
                }
                else
                {
                    this.Context.GroupVariableList.Add(this.Identifier, new List<string>());
                }

                foreach (string s in this.IdentifierList)
                {
                    this.Context.GroupVariableList[this.Identifier].Add(s);
                }
            }

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", CommandNames.DEFINE);
            this.Context.AnalysisCheckCodeInterface.Display(args);
            return null;
        }
    }
}
