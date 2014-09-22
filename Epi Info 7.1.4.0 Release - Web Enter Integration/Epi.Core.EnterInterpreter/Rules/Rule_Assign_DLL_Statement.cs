using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Assign_DLL_Statement : EnterRule
    {
        string QualifiedId = null;
        string ClassName = null;
        string MethodName = null;
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Assign_DLL_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            //<Assign_DLL_Statement>  ::= ASSIGN <Qualified ID> '=' identifier'!'Identifier '(' <FunctionParameterList> ')'

            this.QualifiedId = this.GetCommandElement(pToken.Tokens, 1);
            this.ClassName = this.GetCommandElement(pToken.Tokens, 3);
            this.MethodName  = this.GetCommandElement(pToken.Tokens, 5);
            
            //this.ParameterList = new Rule_FunctionParameterList(pContext, (NonterminalToken)pToken.Tokens[7]);
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, (NonterminalToken)pToken.Tokens[7]);
            if (!this.Context.AssignVariableCheck.ContainsKey(this.QualifiedId.ToLower()))
            {
                this.Context.AssignVariableCheck.Add(this.QualifiedId.ToLower(), this.QualifiedId.ToLower());
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
                if (this.Context.DLLClassList.ContainsKey(this.ClassName.ToLower()))
                {
                    if (this.ParameterList.Count > 0)
                    {
                        args = new object[this.ParameterList.Count];
                        for (int i = 0; i < this.ParameterList.Count; i++)
                        {
                            args[i] = this.ParameterList[i].Execute();
                        }
                    }

                    else
                    {
                        args = new object[0];
                    }


                    IDLLClass DLLObject = this.Context.DLLClassList[this.ClassName.ToLower()];

                    result = DLLObject.Execute (this.MethodName, args);
                }
            }


            IVariable var;
            //DataType dataType = DataType.Unknown;
            string dataValue = string.Empty;
            var = (IVariable) this.Context.CurrentScope.Resolve(this.QualifiedId);
            if (var != null)
            {
                if (var.VarType == VariableType.DataSource)
                {
                    IVariable fieldVar = new DataSourceVariableRedefined(var.Name, var.DataType);
                    fieldVar.PromptText = var.PromptText;
                    fieldVar.Expression = result.ToString();
                    this.Context.CurrentScope.Undefine(var.Name);
                    this.Context.CurrentScope.Define((EpiInfo.Plugin.IVariable)fieldVar);
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
                if (result != null)
                {
                    this.Context.EnterCheckCodeInterface.Assign(this.QualifiedId, result);
                }
            }

            return result;
        }
    }
}
