using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using EpiInfo.Plugin;
using Epi.Data;
using Epi.Data.Services;
using Epi.DataSets;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Define : EnterRule
    {
        string Identifier = null;
        //strings named to match grammar
        string Variable_Scope = null;
        string VariableTypeIndicator = null;
        string Define_Prompt = null;
        EnterRule Expression = null;


        private EpiInfo.Plugin.VariableScope GetVariableScopeIdByName(string name)
        {
            string Query = "Name='" + name + "'";
            DataRow[] rows = AppData.Instance.VariableScopesDataTable.Select(Query);
            if (rows.GetUpperBound(0) >= 0)
            {
                return (EpiInfo.Plugin.VariableScope)int.Parse(rows[0]["Id"].ToString());
            }
            else
            {
                return 0;       // Unknown
            }
        }

        public Rule_Define(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //DEFINE Identifier <Variable_Scope> <VariableTypeIndicator> <Define_Prompt>
            //DEFINE Identifier '=' <Expression>

            Identifier = GetCommandElement(pToken.Tokens, 1);
            if (pContext.IsVariableValidationEnable)
            {
                if (!string.IsNullOrEmpty(Identifier))
                {

                    if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLowerInvariant()))
                    {
                        this.Context.CommandVariableCheck.Add(Identifier, "define");
                    }

                }
            }
            if (GetCommandElement(pToken.Tokens, 2) == "=")
            {
                this.Expression = EnterRule.BuildStatments(pContext, pToken.Tokens[3]);
                // set some defaults
                Variable_Scope = "STANDARD";
                VariableTypeIndicator  =  "";
                Define_Prompt = "";
            }
            else
            {
                Variable_Scope = GetCommandElement(pToken.Tokens, 2);//STANDARD | GLOBAL | PERMANENT |!NULL

                VariableTypeIndicator = GetCommandElement(pToken.Tokens, 3);
                Define_Prompt = GetCommandElement(pToken.Tokens, 4);
            }

        }

        /// <summary>
        /// peforms the Define rule uses the MemoryRegion and this.Context.DataSet to hold variable definitions
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            try
            {
                EpiInfo.Plugin.IVariable var = null;

                var = Context.CurrentScope.Resolve(Identifier);
                if (var != null)
                {
                    if (var.VariableScope != VariableScope.Permanent && var.VariableScope != VariableScope.Global)
                    {
                        this.Context.EnterCheckCodeInterface.Dialog("Duplicate variable: " + Identifier, "Define");
                        return null;
                    }


                    if (var.VariableScope == VariableScope.Permanent)
                    {
                        return var;
                    }
                }
                

                string dataTypeName = VariableTypeIndicator.Trim().ToUpperInvariant();
                EpiInfo.Plugin.DataType type = GetDataType(dataTypeName);
                string variableScope = Variable_Scope.Trim().ToUpperInvariant();
                EpiInfo.Plugin.VariableScope vt = EpiInfo.Plugin.VariableScope.Standard;
                
               //if(variableScope.Equals("PERMANENT", StringComparison.OrdinalIgnoreCase))
               //{
               //    vt = EpiInfo.Plugin.VariableScope.Permanent;
               //}
               //else if(variableScope.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase))
               //{
               //     vt = EpiInfo.Plugin.VariableScope.Global;
               //}
               //else
               //{
               //     vt = EpiInfo.Plugin.VariableScope.Standard;
               //}

                
                if (!string.IsNullOrEmpty(variableScope))
                {
                    vt = this.GetVariableScopeIdByName(variableScope);
                }

                
                string promptString = Define_Prompt.Trim().Replace("\"", string.Empty);
                if (!string.IsNullOrEmpty(promptString))
                {
                    promptString = promptString.Replace("(", string.Empty).Replace(")", string.Empty);
                    promptString.Replace("\"", string.Empty);
                }

                var = new PluginVariable(Identifier, type, vt, null, promptString);
                //var.PromptText = promptString;
                //this.Context.MemoryRegion.DefineVariable(var);
                EpiInfo.Plugin.IVariable temp = (EpiInfo.Plugin.IVariable)var;
                this.Context.CurrentScope.Define(temp);

                return var;
            }
            catch (Exception ex)
            {
                Epi.Diagnostics.Debugger.Break();
                Epi.Diagnostics.Debugger.LogException(ex);
                throw ex;
            }
        }
    }
}
