using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;


namespace EpiMenu.CommandPlugin
{
    public class Rule_Assign : Rule
    {
        public string QualifiedId;
        Rule value = null;

        object ReturnResult = null;

        public Rule_Assign(Rule_Context pContext, NonterminalToken pTokens) : base(pContext)
        {
            //ASSIGN <Qualified ID> '=' <Expression>
            //<Let_Statement> ::= LET Identifier '=' <Expression> 
            //<Simple_Assign_Statement> ::= Identifier '=' <Expression>
            
            switch(pTokens.Rule.Lhs.ToString())
            {
                  
                case "<Assign_Statement>":
                    NonterminalToken T = (NonterminalToken)pTokens.Tokens[1];
                    this.QualifiedId = T.Tokens[0].ToString();
                    //this.value = new Rule_Expression(pContext, (NonterminalToken)pTokens.Tokens[3]);
                    break;
                case "<Let_Statement>":
                    this.QualifiedId = pTokens.Tokens[1].ToString();
                    //this.value = new Rule_Expression(pContext, (NonterminalToken)pTokens.Tokens[3]);
                    break;
                case "<Simple_Assign_Statement>":
                    //Identifier '=' <Expression>
                    //T = (NonterminalToken)pTokens.Tokens[1];
                    this.QualifiedId = this.GetCommandElement(pTokens.Tokens, 0);
                    //this.value = new Rule_Expression(pContext, (NonterminalToken)pTokens.Tokens[2]);
                    break;
            }
        }
        /// <summary>
        /// peforms an assign rule by assigning an expression to a variable.  return the variable that was assigned
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = this.value.Execute();

            /*
            IVariable var;
            //DataType dataType = DataType.Unknown;
            string dataValue = string.Empty;

            if (this.Context.MemoryRegion.TryGetVariable(this.QualifiedId, out var))
            {
                if (this.Context.RunMode == Rule_Context.eRunMode.Enter)
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
                    string expression = result.ToString();
                    if (var.VarType == VariableType.DataSource)
                    {
                        IVariable fieldVar = new DataSourceVariableRedefined(var.Name, var.DataType);
                        fieldVar.PromptText = var.PromptText;
                        fieldVar.Expression = expression;

                        this.Context.MemoryRegion.UndefineVariable(var.Name);
                        this.Context.MemoryRegion.DefineVariable(fieldVar);
                    }
                    else if (var.VarType == VariableType.DataSourceRedefined)
                    {
                        IVariable fieldVarRedefine = this.Context.MemoryRegion.GetVariable(var.Name);

                        this.Context.MemoryRegion.UndefineVariable(fieldVarRedefine.Name);
                        this.Context.MemoryRegion.DefineVariable(fieldVarRedefine);
                    }
                    else
                    {
                        var.Expression = expression;
                        if (var.DataType == DataType.Unknown)
                        {
                            var.DataType = GuessDataTypeFromExpression(expression);
                        }
                    }
                }
            }
            else
            {
                if (result != null)
                {
                    this.Context.EnterCheckCodeInterface.Assign(this.QualifiedId, result);
                }
            }*/
            
            return result;
        }

        /*
        private Epi.DataType GuessDataTypeFromExpression(string expression)
        {
            double d = 0.0;
            DateTime dt;
            if (double.TryParse(expression, out d))
            {
                return DataType.Number;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(expression, "([+,-])"))
            {
                return DataType.Boolean;
            }
            if (DateTime.TryParse(expression, out dt))
            {
                return DataType.Date;
            }
            return DataType.Unknown;
        }*/

    }
}
