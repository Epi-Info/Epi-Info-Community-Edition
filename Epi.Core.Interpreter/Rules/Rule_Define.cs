using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using Epi;
using Epi.Data;
using Epi.Data.Services;
using Epi.DataSets;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_Define : AnalysisRule
    {
        string Identifier = null;
        //strings named to match grammar
        string Variable_Scope = null;
        string VariableTypeIndicator = null;
        string Define_Prompt = null;
        AnalysisRule Expression = null;


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
        }

        public Rule_Define(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //DEFINE Identifier <Variable_Scope> <VariableTypeIndicator> <Define_Prompt>
            //DEFINE Identifier '=' <Expression>

            Identifier = GetCommandElement(pToken.Tokens, 1);
            Context.DefineVarList.Add(Identifier.ToUpperInvariant());
            if (GetCommandElement(pToken.Tokens, 2) == "=")
            {
                this.Expression = new Rule_Expression(pContext, (NonterminalToken)pToken.Tokens[3]);
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
                IVariable var = null;

                #region Preconditions
                //zack check reserved word /11/16/09
                AppData appdata = new AppData();
                /*
                if (appdata.IsReservedWord(Identifier))
                {
                    throw new GeneralException(string.Format(SharedStrings.RESERVED_WORD, Identifier.ToUpperInvariant()));
                }*/

                if (this.Context.MemoryRegion.IsVariableInScope(Identifier))
                {

                    if (this.Context.MemoryRegion.TryGetVariable(this.Identifier, out var))
                    {
                        if (var.VarType != VariableType.Permanent)
                        {
                            this.Context.AnalysisCheckCodeInterface.Dialog(SharedStrings.DUPLICATE_VARIABLE_DEFINITION + StringLiterals.COLON + Identifier, CommandNames.DEFINE);
                        }

                    }
                    else
                    {
                        this.Context.AnalysisCheckCodeInterface.Dialog(SharedStrings.DUPLICATE_VARIABLE_DEFINITION + StringLiterals.COLON + Identifier, CommandNames.DEFINE);
                    }
                    
                }
                else if(this.Context.GroupVariableList.ContainsKey(this.Identifier))
                {
                    this.Context.AnalysisCheckCodeInterface.Dialog(SharedStrings.DUPLICATE_VARIABLE_DEFINITION + StringLiterals.COLON + Identifier, CommandNames.DEFINE);
                }
                #endregion Preconditions

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

                if (var.VarType == VariableType.Standard || var.VarType == VariableType.Global)
                {
                    if (this.Context.VariableValueList.ContainsKey(var.Name.ToUpperInvariant()))
                    {
                        this.Context.VariableValueList.Remove(var.Name.ToUpperInvariant());
                    }
                    
                        
                    
                    DataTable dataTable;

                    if(!this.Context.DataSet.Tables.Contains("variables"))
                    {
                        this.Context.DataSet.Tables.Add(new DataTable("variables"));
                    }

                    dataTable = this.Context.DataSet.Tables["variables"];
                    DataColumn C = new DataColumn(var.Name);
                    
                    switch (var.DataType)
                    {
                        case DataType.Boolean:
                        case DataType.YesNo:
                            C.DataType = typeof(bool);
                            this.Context.VariableValueList.Add(var.Name.ToUpperInvariant(), false);
                            break;
                        case DataType.Date:
                        case DataType.DateTime:
                            C.DataType = typeof(DateTime);
                            this.Context.VariableValueList.Add(var.Name.ToUpperInvariant(), DateTime.Now);
                            break;
                        case DataType.Number:
                            C.DataType = typeof(double);
                            this.Context.VariableValueList.Add(var.Name.ToUpperInvariant(), 0.0);
                            break;
                        case DataType.Time:
                            C.DataType = typeof(System.TimeSpan);
                            this.Context.VariableValueList.Add(var.Name.ToUpperInvariant(), new TimeSpan());
                            break;

                        case DataType.PhoneNumber:
                        case DataType.Text:
                        case DataType.Unknown:
                        case DataType.Object:
                        default:
                            C.DataType = typeof(string);
                            this.Context.VariableValueList.Add(var.Name.ToUpperInvariant(), "");
                            break;
                    }

                    if (dataTable.Columns.Contains(C.ColumnName))
                    {
                        dataTable.Columns.Remove(C.ColumnName);
                    }
                    dataTable.Columns.Add(C);

                    this.Context.SyncVariableAndOutputTable();

                    if (this.Expression != null)
                    {
                        object vresult = null;
                        if (this.Context.VariableExpressionList.ContainsKey(this.Identifier.ToUpperInvariant()))
                        {
                            this.Context.VariableExpressionList[this.Identifier.ToUpperInvariant()] = this.Expression;
                        }
                        else
                        {
                            this.Context.VariableExpressionList.Add(this.Identifier.ToUpperInvariant(), this.Expression);
                        }

                        if (this.Context.CurrentDataRow != null)
                        {
                            vresult = this.Expression.Execute();
                            if (vresult == null)
                            {
                                this.Context.CurrentDataRow[var.Name] = DBNull.Value;
                            }
                            else
                            {
                                this.Context.CurrentDataRow[var.Name] = vresult;
                            }
                        }
                        else
                        {
                            
                            dataTable = this.Context.DataSet.Tables["output"];
                            if (dataTable.Rows.Count == 0)
                            {
                                DataRow R = dataTable.NewRow();
                                dataTable.Rows.Add(R);
                            }

                            this.Context.GetOutput(DefineMapDataFunction);

                            /*
                            vresult = null;
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                this.Context.CurrentDataRow = dataTable.Rows[i];
                                vresult = this.Expression.Execute();
                                if (vresult == null)
                                {
                                    this.Context.CurrentDataRow[var.Name] = DBNull.Value;
                                }
                                else
                                {
                                    this.Context.CurrentDataRow[var.Name] = vresult;
                                }
                            }
                            this.Context.CurrentDataRow = null;*/
                        }
                    }
                }
                Context.DefineVarList.Clear();

                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", CommandNames.DEFINE);
                this.Context.AnalysisCheckCodeInterface.Display(args);

                return results;
            }
            catch (Exception ex)
            {
                Epi.Diagnostics.Debugger.Break();
                Epi.Diagnostics.Debugger.LogException(ex);
                throw ex;
            }
        }

        private void DefineMapDataFunction()
        {
            object vresult = null;
            vresult = this.Expression.Execute();
            if (vresult == null)
            {
                this.Context.CurrentDataRow[this.Identifier] = DBNull.Value;
            }
            else
            {
                this.Context.CurrentDataRow[this.Identifier] = vresult;
            }
        }
    }
}
