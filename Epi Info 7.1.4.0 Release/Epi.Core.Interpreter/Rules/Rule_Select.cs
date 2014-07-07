using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;
namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Rule for the SELECT command
    /// </summary>
    public class Rule_Select : AnalysisRule
    {
        bool HasRun = false;

        AnalysisRule Expression = null;
        string SelectString = null;
        bool CancelExpresson = false;
        StringBuilder fieldnames = new StringBuilder(); 
        /// <summary>
        /// Constructor for Rule_Select
        /// </summary>
        /// <param name="pToken">The token used to build the reduction.</param>
        public Rule_Select(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<Select_Statement> ::= SELECT <Expression> |
            //                              |  CANCEL SELECT | Select
            if(pToken.Tokens.Length == 1)
            {
                this.CancelExpresson = true;
            }
            else
            {
                if(pToken.Tokens[0].ToString().ToUpper() == "CANCEL")
                {
                    this.CancelExpresson = true;
                }
                else
                {                   
                    fieldnames.Append(this.GetFieldnames(pToken.Tokens, 1));                   
                        this.Expression = AnalysisRule.BuildStatments(pContext, (NonterminalToken)pToken.Tokens[1]);
                        this.SelectString = this.GetCommandElement(pToken.Tokens, 1);                                     
                }                    
            }
        }


        protected string GetFieldnames(Token[] tokens, int index)
        {
            if (tokens[index] is NonterminalToken)
            {
                return ExtractToken(((NonterminalToken)tokens[index]).Tokens).Trim();
            }
            else
            {
                return (tokens[index]).ToString();
            }
        }

        protected string ExtractToken(Token[] tokens)
        {
            string tokensInTree = ""; int max = tokens.GetUpperBound(0);
            for (int i = tokens.GetLowerBound(0); i <= max; i++)
            {
                if (tokens[i] is NonterminalToken)
                {
                    if (tokens[i].ToString().Contains("<Qualified ID>"))
                        tokensInTree += ExtractToken(((NonterminalToken)tokens[i]).Tokens);
                }
                else
                {
                    if (((com.calitha.goldparser.TerminalToken)(tokens[i])).Symbol.ToString() == "Identifier")
                    {
                        if (tokens[i].ToString() != "false" & tokens[i].ToString() != "true")
                            tokensInTree += tokens[i].ToString() + " ";
                    }
                }
            }
            return tokensInTree;
        }

        public bool Checkvariablenames(StringBuilder fieldnames, out List<string> invalidfieldnames)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.Standard |
                                     VariableType.Global | VariableType.Permanent;
            VariableCollection vars = this.Context.MemoryRegion.GetVariablesInScope(scopeWord);
            string[] names = null; bool isvalid = false;
            fieldnames.ToString().Trim(new char[] { '[', ']' });
            names = fieldnames.ToString().Split(' '); invalidfieldnames = new List<string>();
            foreach (string name in names)
            {
                invalidfieldnames.Add(name);
                foreach (IVariable var in vars)
                {
                    if (!(var is Epi.Fields.PredefinedDataField))
                    {
                        if (var.Name.ToUpper() == name.ToUpper())
                        {
                            isvalid = true;
                            invalidfieldnames.Remove(name);
                            break;
                        }
                    }
                }
                if (invalidfieldnames.Count > 0)
                {
                    if (this.Context.VariableValueList.ContainsKey(name))
                    {
                        isvalid = true;
                        invalidfieldnames.Remove(name);
                        break;
                    }
                    else if (Context.SelectCommandList.Contains(name.ToUpper()))
                    {
                        isvalid = true;
                        invalidfieldnames.Remove(name);
                        break;
                    }
                }
            }
            return isvalid;
        }

        /// <summary>
        /// Method to execute the SELECT command
        /// </summary>
        /// <returns>Returns the result of executing the reduction.</returns>
        public override object Execute()
        {
            if (!this.HasRun)
            {

                if (this.CancelExpresson)
                {
                    //this.Context.DataTableRefreshNeeded = true;
                    this.Context.SelectExpression.Clear();
                    this.Context.SelectString.Length = 0;
                }
                else
                {
                    List<string> invalidfieldnames = new List<string>();
                    bool isValid = Checkvariablenames(fieldnames, out invalidfieldnames);
                    if (isValid & invalidfieldnames.Count == 0)
                    {
                    this.Context.SelectExpression.Add(this.Expression);
                    if (this.Context.SelectString.Length > 0)
                    {
                        if (!this.Context.SelectString.ToString().StartsWith("("))
                        {
                            this.Context.SelectString.Insert(0, '(');
                            this.Context.SelectString.Append(") AND (");

                        }
                        else
                        {
                            this.Context.SelectString.Append(" AND (");
                        }
                        this.Context.SelectString.Append(this.ConvertToSQL(this.SelectString));
                        this.Context.SelectString.Append(")");
                    }
                    else
                    {
                        this.Context.SelectString.Append(this.ConvertToSQL(this.SelectString));
                    }
                }
                    else
                    {
                        throw new Exception(string.Join(",", invalidfieldnames.ToArray()) + " does not exist ");
                    }
                }

                this.Context.GetOutput();

                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", CommandNames.SELECT);
                this.Context.AnalysisCheckCodeInterface.Display(args);

                this.HasRun = true;
            }
            return null;
        }



        private string ConvertToSQL(string pValue)
        {
            string result = pValue.Replace("\"", "\'").Replace("(.)", "NULL").Replace("(+)", "true").Replace("(-)", "false");

            return result;
        }

    }
}
