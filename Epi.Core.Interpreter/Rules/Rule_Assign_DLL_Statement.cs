using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Assign_DLL_Statement : AnalysisRule
    {
        string QualifiedId = null;
        string ClassName = null;
        string MethodName = null;
        Rule_FunctionParameterList ParameterList = null;

        public Rule_Assign_DLL_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            //<Assign_DLL_Statement>  ::= ASSIGN <Qualified ID> '=' identifier'!'Identifier '(' <FunctionParameterList> ')'

            this.QualifiedId = this.GetCommandElement(pToken.Tokens, 1);
            this.ClassName = this.GetCommandElement(pToken.Tokens, 3);
            this.MethodName  = this.GetCommandElement(pToken.Tokens, 5);
            this.ParameterList = new Rule_FunctionParameterList(pContext, (NonterminalToken)pToken.Tokens[7]);
            if (!this.Context.AssignVariableCheck.ContainsKey(this.QualifiedId.ToLowerInvariant()))
            {
                this.Context.AssignVariableCheck.Add(this.QualifiedId.ToLowerInvariant(), this.QualifiedId.ToLowerInvariant());
            }
        }

        /// <summary>
        /// peforms an assign rule by assigning an expression to a variable.  return the variable that was assigned
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            object[] args = null;

            if (!string.IsNullOrEmpty(this.ClassName))
            {
                // this is a dll call
                // and is NOT implemented as of 11/05/2010
                if (this.Context.DLLClassList.ContainsKey(this.ClassName.ToLowerInvariant()))
                {
                    if (this.ParameterList.paramList != null)
                    {
                        args = new object[this.ParameterList.paramList.Count];
                        for (int i = this.ParameterList.paramList.Count -1; i > -1; i--)
                        {
                            args[i] = this.ParameterList.paramList.Pop().Execute();
                        }
                    }

                    else
                    {
                        args = new object[0];
                    }
                    

                    object DLLObject = this.Context.DLLClassList[this.ClassName.ToLowerInvariant()];

                    result = Microsoft.VisualBasic.Interaction.CallByName(DLLObject, this.MethodName, Microsoft.VisualBasic.CallType.Method, args);
                }
            }


            IVariable var;
            //DataType dataType = DataType.Unknown;
            string dataValue = string.Empty;

            if (this.Context.MemoryRegion.TryGetVariable(this.QualifiedId, out var))
            {
                if (var.VarType == VariableType.DataSource)
                {
                    IVariable fieldVar = new DataSourceVariableRedefined(var.Name, var.DataType);
                    fieldVar.PromptText = var.PromptText;
                    fieldVar.Expression = result.ToString();
                    this.Context.MemoryRegion.UndefineVariable(var.Name);
                    this.Context.MemoryRegion.DefineVariable(fieldVar);
                }
                else
                {
                    if (result != null)
                    {
                        var.Expression = result.ToString();
                    }
                    else
                    {
                        var.Expression = "Null";
                    }
                }
            }
            else
            {
                /*
                if (result != null)
                {
                    this.Context.AnalysisCheckCodeInterface.Assign(this.QualifiedId, result);
                }*/
            }

            return result;
        }
    }
}
