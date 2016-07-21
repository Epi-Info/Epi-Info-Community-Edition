using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_SetRequired : EnterRule
    {
        string[] IdentifierList = null;

        public Rule_SetRequired(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            this.IdentifierList = this.IdentifierList = this.GetCommandElement(pToken.Tokens, 1).ToString().Split(' ');
            if (pContext.IsVariableValidationEnable)
            {
                if (IdentifierList.Length > 0)
                {
                    foreach (var item in IdentifierList)
                    {
                        if (!string.IsNullOrEmpty(item) && !this.Context.CommandVariableCheck.ContainsKey(item.ToLowerInvariant()))
                        {
                            this.Context.CommandVariableCheck.Add(item, "SetRequired");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes the reduction
        /// </summary>
        /// <returns>null</returns>
        public override object Execute()
        {
            if (this.IdentifierList.Length == 1 && this.IdentifierList[0] == "*")
            {
                List<EpiInfo.Plugin.IVariable> variableList = this.Context.CurrentScope.FindVariables(EpiInfo.Plugin.VariableScope.DataSource);
                this.IdentifierList = new string[variableList.Count];
                int i = 0;
                foreach (EpiInfo.Plugin.IVariable field in variableList)
                {
                    this.IdentifierList[i] = field.Name.ToLowerInvariant();
                    i++;
                }
               
            }

            this.Context.EnterCheckCodeInterface.SetRequired(this.IdentifierList);
            return null;
        }
    }
}
