using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using Epi.Data;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Assign : EnterRule
    {
        public string QualifiedId;
        private string Namespace = null;
        //private string functionCall;
        //private string Identifier;
        EnterRule value = null;
        //Epi.View View = null;
        int Index0 = -1;
        object Index1 = null;

        //object ReturnResult = null;

        public Rule_Assign(Rule_Context pContext, NonterminalToken pTokens) : base(pContext)
        {
            //ASSIGN <Qualified ID> '=' <Expression>
            //<Let_Statement> ::= LET Identifier '=' <Expression> 
            //<Simple_Assign_Statement> ::= Identifier '=' <Expression>
            //<Assign_DLL_Statement> ::= ASSIGN <Qualified ID> '=' identifier'!'<FunctionCall>
            string[] temp;
            NonterminalToken T;
            switch(pTokens.Rule.Lhs.ToString())
            {
                  
                case "<Assign_Statement>":
                    T = (NonterminalToken)pTokens.Tokens[1];
                    if (T.Symbol.ToString() == "<Fully_Qualified_Id>")
                    {
                        temp = this.ExtractTokens(T.Tokens).Split(' ');
                        this.Namespace = T.Tokens[0].ToString();
                        this.QualifiedId = this.GetCommandElement(T.Tokens,2);
                    }
                    else
                    {
                        this.QualifiedId = this.GetCommandElement(T.Tokens, 0);
                    }
                    this.value = EnterRule.BuildStatments(pContext, pTokens.Tokens[3]);
                    if (!this.Context.AssignVariableCheck.ContainsKey(this.QualifiedId.ToLower()))
                    {
                        this.Context.AssignVariableCheck.Add(this.QualifiedId.ToLower(), this.QualifiedId.ToLower());
                    }
                    break;
                case "<Let_Statement>":
                    T = (NonterminalToken)pTokens.Tokens[1];
                    if (T.Symbol.ToString() == "<Fully_Qualified_Id>")
                    {
                        temp = this.ExtractTokens(T.Tokens).Split(' ');
                        this.Namespace = T.Tokens[0].ToString();
                        this.QualifiedId = this.GetCommandElement(T.Tokens, 2);
                    }
                    else
                    {
                        this.QualifiedId = this.GetCommandElement(T.Tokens, 0);
                    }

                    
                    this.value = EnterRule.BuildStatments(pContext, pTokens.Tokens[3]);

                    if (!this.Context.AssignVariableCheck.ContainsKey(this.QualifiedId.ToLower()))
                    {
                        this.Context.AssignVariableCheck.Add(this.QualifiedId.ToLower(), this.QualifiedId.ToLower());
                    }
                    break;
                case "<Simple_Assign_Statement>":
                    //Identifier '=' <Expression>
                    //T = (NonterminalToken)pTokens.Tokens[1];
                    T = (NonterminalToken)pTokens.Tokens[0];
                    if (T.Symbol.ToString() == "<Fully_Qualified_Id>")
                    {
                        temp = this.ExtractTokens(T.Tokens).Split(' ');
                        this.Namespace = T.Tokens[0].ToString();
                        this.QualifiedId = this.GetCommandElement(T.Tokens, 2);
                    }
                    else if (T.Symbol.ToString() == "<GridFieldId>")
                    {
                            /*<GridFieldId> ::= Identifier'[' <Number>',' Identifier ']'  
                            | Identifier '[' <Number> ',' <Number> ']' 
                            | Identifier '[' <Number> ',' <Literal_String> ']'*/
                            this.QualifiedId = this.GetCommandElement(T.Tokens, 0);
                            int Int_temp;
                            if (int.TryParse(this.GetCommandElement(T.Tokens, 2), out Int_temp))
                            {
                                this.Index0 = Int_temp;
                            }


                            if (int.TryParse(this.GetCommandElement(T.Tokens, 4), out Int_temp))
                            {
                                this.Index1 = Int_temp;
                            }
                            else
                            {
                                
                                //this.Index1 = this.GetCommandElement(T.Tokens, 4);
                                //this.Index1 = this.ExtractTokens(((NonterminalToken)T.Tokens[4]).Tokens);
                                this.Index1 = new Rule_Value(this.Context, T.Tokens[4]);
                            }
                            this.value = EnterRule.BuildStatments(pContext, pTokens.Tokens[2]);
                            break;
                    }
                    else
                    {
                        this.QualifiedId = this.GetCommandElement(T.Tokens, 0);
                    }

                    
                    this.value = EnterRule.BuildStatments(pContext, pTokens.Tokens[2]);
                    if (!this.Context.AssignVariableCheck.ContainsKey(this.QualifiedId.ToLower()))
                    {
                        this.Context.AssignVariableCheck.Add(this.QualifiedId.ToLower(), this.QualifiedId.ToLower());
                    }
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
            if (this.Index1 is Rule_Value)
            {
                this.Index1 = ((Rule_Value)this.Index1).Execute();
            }

            EpiInfo.Plugin.IVariable var;
            string dataValue = string.Empty;
            var =  this.Context.CurrentScope.Resolve(this.QualifiedId, this.Namespace);

            if (var != null)
            {
                if (var.VariableScope == EpiInfo.Plugin.VariableScope.DataSource)
                {
                    /*if (var is Epi.Fields.InputFieldWithoutSeparatePrompt)
                    {
                        var.Expression = result.ToString();
                    }
                    else
                    {
                        var.Expression = result.ToString();
                    }
                    try
                    {
                        var.Expression = result.ToString();
                    }
                    catch (Exception ex)
                    {*/
                    if (this.Index0 == -1)
                    {
                        this.Context.EnterCheckCodeInterface.Assign(this.QualifiedId, result);
                    }
                    else
                    {
                        this.Context.EnterCheckCodeInterface.AssignGrid(this.QualifiedId, result, this.Index0, this.Index1);
                    }
                    //}
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.Namespace))
                    {
                        result = var.Expression;
                    }
                    else if (result != null)
                    {
                        var.Expression = result.ToString();
                    }
                    else
                    {
                        var.Expression = "Null";
                    }

                    if (var.VariableScope == EpiInfo.Plugin.VariableScope.Permanent)
                    {
                        Rule_Context.UpdatePermanentVariable(var);
                    }
                }
            }
            else
            {
                if (this.Index0 == -1)
                {
                    this.Context.EnterCheckCodeInterface.Assign(this.QualifiedId, result);
                }
                else
                {
                    this.Context.EnterCheckCodeInterface.AssignGrid(this.QualifiedId, result, this.Index0, this.Index1);
                }
            }
            
            return result;
        }


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
        }


    }
}
