using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using Epi.Data;
using System.Linq;
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
            int number;
            bool isNumeric;
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
                     


                    for (int i = 0; pTokens.Tokens.Length > i; i++)
                    {
                        var IdentifierList = this.GetCommandElement(pTokens.Tokens, i).ToString().Split(' ');
                        IdentifierList = RemoveOp(IdentifierList);
                        if (IdentifierList.Length > 0)
                        {
                            foreach (var Identifier in IdentifierList)
                             {
                                 isNumeric = int.TryParse(Identifier, out number);
                                 if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()) && !isNumeric)
                                    {
                                        this.Context.CommandVariableCheck.Add(Identifier, "assign");
                                    }
                             }
                         }
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

                    for (int i = 0; pTokens.Tokens.Length > i; i++)
                    {
                        var IdentifierList = this.GetCommandElement(pTokens.Tokens, i).ToString().Split(' ');
                        IdentifierList = RemoveOp(IdentifierList);
                        if (IdentifierList.Length > 0)
                        {
                            foreach (var Identifier in IdentifierList)
                            {
                                isNumeric = int.TryParse(Identifier, out number);
                                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()) && !isNumeric)
                                {
                                    this.Context.CommandVariableCheck.Add(Identifier, "assign");
                                }
                            }
                        }
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
                    if (!this.Context.CommandVariableCheck.ContainsKey(this.QualifiedId.ToLower()))
                    {
                        this.Context.CommandVariableCheck.Add(this.QualifiedId.ToLower(), this.QualifiedId.ToLower());
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

        private string[] RemoveOp(string[] IdentifierList)
        {
            List<string> NewList = new List<string>();
            List<string> OpList = new List<string>();



            OpList.Add("(");
            OpList.Add(")");
            OpList.Add("/");
            OpList.Add("(.)");
            OpList.Add("(+)");
            OpList.Add("(-)");
            OpList.Add("=");
            OpList.Add("+");
            OpList.Add("-");
            OpList.Add(">");
            OpList.Add("<");
            OpList.Add(">=");
            OpList.Add("<=");
            OpList.Add("<>");
            OpList.Add("^");
            OpList.Add("&");
            OpList.Add("*");
            OpList.Add("%");
            OpList.Add("mod");
            OpList.Add("(.)");
            OpList.Add("not");
            OpList.Add("or");
            OpList.Add("and");
            OpList.Add("xor");
            OpList.Add("Yes");
            OpList.Add("No");
            OpList.Add("Missing");
            OpList.Add("AND");
            OpList.Add("OR");
            OpList.Add("ASSIGN");
            
            var QuotationCount = IdentifierList.Where(x => x.Contains("\"")).Count();
        
            List<int> IndexList = new List<int>();
            for (int i = 0; IdentifierList.Count() > i;i++ )
            {
                if (IdentifierList[i].Contains("\""))
                {
                    IndexList.Add(i);
                }
            }
            int j = 0;
            List<string> RemoveList = new List<string> () ;
            for(int i=0 ; QuotationCount/2 > i; i++)
            {
                var List = IdentifierList.Skip(IndexList[j]).Take(IndexList[j + 1] + 1).ToList();
                foreach (var _item in List ){
                RemoveList.Add(_item);
                }
               
                j = j + 2;
            }

            foreach (var item in IdentifierList)
            {
                int number;
                bool isNumeric = int.TryParse(item, out number);
                DateTime date;
                bool IsDate = DateTime.TryParse(item, out date);

                if (!OpList.Contains(item) && !isNumeric && !RemoveList.Contains(item) && !IsDate)
                {

                    NewList.Add(item);
                }
            }

            return NewList.ToArray();
        }
    }
}
