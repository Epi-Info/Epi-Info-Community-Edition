using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;

namespace EpiMenu.CommandPlugin
{
    public class Rule_Define : Rule
    {
        string Identifier = null;
        //strings named to match grammar
        string Variable_Scope = null;
        string VariableTypeIndicator = null;
        string Define_Prompt = null;
        Rule Expression = null;


        /*
        private VariableType GetVariableScopeIdByName(string name)
        {
            string Query = "Name='" + name + "'";
            DataRow[] rows = AppData.Instance.VariableScopesDataTable.Select(Query);
            if (rows.GetUpperBound(0) >= 0)
            {
                return (VariableType)int.Parse(rows[0]["Id"].ToString());
            }
            else
            {
                return 0;       // Unknown
            }
        }*/

        public Rule_Define(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //DEFINE Identifier <Variable_Scope> <VariableTypeIndicator> <Define_Prompt>
            //DEFINE Identifier '=' <Expression>

            Identifier = GetCommandElement(pToken.Tokens, 1);
            if (GetCommandElement(pToken.Tokens, 2) == "=")
            {
                //this.Expression = new Rule_Expression(pContext, (NonterminalToken)pToken.Tokens[3]);
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
            object result = null;
            try
            {

                /*
                IVariable var = null;

                CommandProcessorResults results = new CommandProcessorResults();
                string dataTypeName = VariableTypeIndicator.Trim().ToUpperInvariant();
                DataType type = GetDataType(dataTypeName);
                string variableScope = Variable_Scope.Trim().ToUpperInvariant();
                VariableType vt = VariableType.Standard;
                if (!string.IsNullOrEmpty(variableScope))
                {
                    vt = this.GetVariableScopeIdByName(variableScope);
                }

                var = new Variable(Identifier, type, vt);
                string promptString = Define_Prompt.Trim().Replace("\"", string.Empty);
                if (!string.IsNullOrEmpty(promptString))
                {
                    promptString = promptString.Replace("(", string.Empty).Replace(")", string.Empty);
                    promptString.Replace("\"", string.Empty);
                }
                var.PromptText = promptString;
                this.Context.MemoryRegion.DefineVariable(var);
                */

            }
            catch (Exception ex)
            {
                //Epi.Diagnostics.Debugger.Break();
                //Epi.Diagnostics.Debugger.LogException(ex);
                throw ex;
            }

            return result;
        }
    }
}
