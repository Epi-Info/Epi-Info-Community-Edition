using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using Epi.Data;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_Combine : AnalysisRule
    {
        public string QualifiedId;
        AnalysisRule value = null;
        Epi.View View = null;

        object ReturnResult = null;

        public Rule_Combine(Rule_Context pContext, NonterminalToken pTokens) : base(pContext)
        {
            //ASSIGN <Qualified ID> '=' <Expression>
            //<Let_Statement> ::= LET Identifier '=' <Expression> 
            //<Simple_Assign_Statement> ::= Identifier '=' <Expression>
            
            switch(pTokens.Rule.Lhs.ToString())
            {
                  
                case "<Combine_Statement>":
                    //NonterminalToken T = (NonterminalToken)pTokens.Tokens[1];
                    //this.QualifiedId = T.Tokens[0].ToString();
                    this.QualifiedId = this.SetQualifiedId(pTokens.Tokens[3]);
                    //this.value = new Rule_Expression(pContext, (NonterminalToken)pTokens.Tokens[3]);
                    this.value = AnalysisRule.BuildStatments(pContext, pTokens.Tokens[4]);
                    
                    break;
            }
        }

        public AnalysisRule getValue()
        {
            return this.value;
        }
        /// <summary>
        /// peforms an assign rule by assigning an expression to a variable.  return the variable that was assigned
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            IVariable var;
            string dataValue = string.Empty;

            if (this.Context.CurrentDataRow != null)
            {
                if (this.Context.MemoryRegion.TryGetVariable(this.QualifiedId, out var))
                {
                    if (var.VarType != VariableType.Standard && var.VarType != VariableType.DataSource)
                    {
                        result = this.value.Execute();
                        ReturnResult = result;
                        if (!Util.IsEmpty(result))
                        {
                            var.Expression = result.ToString();
                        }
                        else
                        {
                            var.Expression = "Null";
                        }
                    }

                        AssignNonProjectDataFunction();
                }
                else
                {
                        AssignNonProjectDataFunction();
                }
            }
            else
            {
                
                if (this.Context.MemoryRegion.TryGetVariable(this.QualifiedId, out var))
                {
                    if (var.VarType != VariableType.Standard && var.VarType != VariableType.DataSource)
                    {
                        result = this.value.Execute();
                        ReturnResult = result;
                        if (!Util.IsEmpty(result))
                        {
                            //result = this.value.Execute();
                            var.Expression = result.ToString();
                        }
                        else
                        {
                            var.Expression = "Null";
                        }

                        this.Context.GetOutput(this.AssignMapDataFunction());
                    }
                    else
                    {

                        if (this.Context.DataSet.Tables.Contains("Output"))
                        {
                            if (this.Context.DataSet.Tables["Output"].Rows.Count == 0)
                            {
                                System.Data.DataRow R = this.Context.DataSet.Tables["Output"].NewRow();
                                this.Context.DataSet.Tables["Output"].Rows.Add(R);
                                
                            }
                        }

                        this.Context.GetOutput(this.AssignMapDataFunction());
                    }
                }
                else
                {
                    if (this.Context.DataSet.Tables.Contains("Output"))
                    {
                        if (this.Context.DataSet.Tables["Output"].Rows.Count == 0)
                        {
                            System.Data.DataRow R = this.Context.DataSet.Tables["Output"].NewRow();
                            this.Context.DataSet.Tables["Output"].Rows.Add(R);
                        }
                    }


                    this.Context.GetOutput(this.AssignMapDataFunction());

                }
            }

            return ReturnResult;
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


        private Rule_Context.MapDataDelegate AssignMapDataFunction()
        {
            //object result = this.value.Execute();
            if (this.Context.CurrentRead == null)
            {
                return AssignNonProjectDataFunction;
            }
            else
            {
                if(this.View == null)
                {
                    return AssignNonProjectDataFunction;
                }
                else
                {
                    return AssignProjectDataFunction;
                }
            }
        }

        private void AssignNonProjectDataFunction()
        {
            if (value is Rule_FunctionCall)
            {
                if (((Rule_FunctionCall)value).getFunctionCall() is Rule_GroupRowIndex)
                {
                    ((Rule_GroupRowIndex)((Rule_FunctionCall)value).getFunctionCall()).setQualifiedId(this.QualifiedId);
                }
            }
            object result = this.value.Execute();
            if (Util.IsEmpty(result))
            {
                this.Context.CurrentDataRow[this.QualifiedId] = DBNull.Value;
            }
            else
            {
                
                try
                {
                    Type ToDataType = this.Context.DataSet.Tables["output"].Columns[this.QualifiedId].DataType;
                    IVariable var;
                    DateTime dateTime;
                    if (ToDataType is IConvertible && result is IConvertible)
                    {
                        this.Context.CurrentDataRow[this.QualifiedId] = result;
                    }
                    else
                    {
                        if (ToDataType.ToString().Equals("System.DateTime" , StringComparison.OrdinalIgnoreCase) && result is TimeSpan)
                        {
                            TimeSpan LHS = (TimeSpan) result;
                            this.Context.CurrentDataRow[this.QualifiedId] = new DateTime(1900,1,1,LHS.Hours,LHS.Minutes, LHS.Seconds);
                        }
                        else if (ToDataType.ToString().Equals("System.TimeSpan", StringComparison.OrdinalIgnoreCase) && result is DateTime)
                        {
                            DateTime LHS = (DateTime)result;
                            this.Context.CurrentDataRow[this.QualifiedId] = new TimeSpan(LHS.Ticks);
                        }
                        else if (ToDataType.ToString().Equals("System.DateTime", StringComparison.OrdinalIgnoreCase) && this.Context.VariableValueList.ContainsKey(this.QualifiedId) && this.Context.MemoryRegion.TryGetVariable(this.QualifiedId, out var) && result is DateTime)
                        {
                            if (var.DataType == DataType.Date)
                            {
                                dateTime = (DateTime)result;
                                DateTime dt = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
                                this.Context.CurrentDataRow[this.QualifiedId] = dt;
                            }
                            else
                            {
                                this.Context.CurrentDataRow[this.QualifiedId] = result;
                            }

                        }
                        else 
                        {
                            this.Context.CurrentDataRow[this.QualifiedId] = result;
                        }
                    }
                }
                catch(System.Exception ex)
                {
                    if (ex != null)
                    {
                        throw new System.Exception(string.Format("You have attempted to assign an incompatible value ({0}) to {1}", result, this.QualifiedId));
                    }
                }
            }

            this.ReturnResult = result;
        }

        private void AssignProjectDataFunction()
        {
            object result = this.value.Execute();
            

            if (Util.IsEmpty(result))
            {
                this.Context.CurrentDataRow[this.QualifiedId] = DBNull.Value;
            }
            else
            {
                if (this.View.Fields.Contains(this.QualifiedId))
                {
                    Epi.Fields.Field F = this.View.Fields[this.QualifiedId];
                    switch (F.FieldType)
                    {
                        case MetaFieldType.YesNo:
                            int temp_int = 0;
                            if (int.TryParse(result.ToString(), out temp_int))
                            {
                                if (temp_int == 0 || temp_int == 1)
                                {
                                    this.Context.CurrentDataRow[this.QualifiedId] = result;
                                }
                                else
                                {
                                    throw new System.Exception(string.Format("You have attempted to assign an incompatible value ({0}) to {1}", result, this.QualifiedId));
                                }
                            }
                            else
                            {
                                throw new System.Exception(string.Format("You have attempted to assign an incompatible value ({0}) to {1}", result, this.QualifiedId));
                            }
                            break;
                        default:
                            try
                            {
                                this.Context.CurrentDataRow[this.QualifiedId] = result;
                            }
                            catch (System.Exception ex)
                            {
                                if (ex != null)
                                {
                                    throw new System.Exception(string.Format("You have attempted to assign an incompatible value ({0}) to {1}", result, this.QualifiedId));
                                }
                            }
                            break;
                    }
                }
                else
                {
                    try
                    {
                        this.Context.CurrentDataRow[this.QualifiedId] = result;
                    }
                    catch (System.Exception ex)
                    {
                        if (ex != null)
                        {
                            throw new System.Exception(string.Format("You have attempted to assign an incompatible value ({0}) to {1}", result, this.QualifiedId));
                        }
                    }
                }
            }

            this.ReturnResult = result;
        }
    }
}
