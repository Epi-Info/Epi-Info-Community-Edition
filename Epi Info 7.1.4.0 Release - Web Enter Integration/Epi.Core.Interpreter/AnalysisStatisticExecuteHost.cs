using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpiInfo.Plugin;
using Epi.Data;

namespace Epi.Core.AnalysisInterpreter
{
    public class AnalysisStatisticExecuteHost : IAnalysisStatisticContext 
    {
        private Dictionary<string, string> mSetProperties;
        private IDataSource mDataSource;
        private Dictionary<string, string> mInputVariableList;
        private string mCurrentSQLStatement;
        private IAnalysisInterpreterHost mAnalysisInterpreterHost;
        private Rule_Context mContext;
        private bool HasCalledGetOutput = false;
        private Dictionary<string, EpiInfo.Plugin.IVariable> mEpiViewVariableList = null;
        private bool isCachedEpiViewVariableList = false;

        public AnalysisStatisticExecuteHost
            (
                Rule_Context Context,
                Dictionary<string, string> SetProperties,
                IDataSource DataSource,
                Dictionary<string, string> InputVariableList,
                string CurrentSQLStatement,
                IAnalysisInterpreterHost AnalysisInterpreterHost
            )
        {
                this.mContext = Context;
                this.mSetProperties = SetProperties;
                this.mDataSource = DataSource;
                this.mInputVariableList = InputVariableList;
                this.mCurrentSQLStatement = CurrentSQLStatement;
                this.mAnalysisInterpreterHost = AnalysisInterpreterHost;
        }

        public Dictionary<string, string> SetProperties
        {
            get { return this.mSetProperties; }
        }

        public IDataSource DataSource
        {
            get { return this.mDataSource; }
        }

        public Dictionary<string, string> InputVariableList
        {
            get { return this.mInputVariableList; }
        }


        public void Display(Dictionary<string, string> pDisplayArgs)
        {
            this.mAnalysisInterpreterHost.Display(pDisplayArgs);
        }

        public System.Data.DataColumnCollection Columns
        {
            get 
            {
                if (! this.HasCalledGetOutput)
                {
                    this.mContext.GetOutput();
                    this.HasCalledGetOutput = true;
                }
                return this.mContext.DataSet.Tables["output"].Columns;  
            }
        }

        public List<System.Data.DataRow> GetDataRows(List<string> pVariableList = null)
        {
            List<System.Data.DataRow> result = this.mContext.GetOutput(pVariableList);
            this.HasCalledGetOutput = true;
            return result;
        }

        public bool OutTable(System.Data.DataTable pDataTable)
        {
            bool result = false;
            try
            {
                if (DBReadExecute.CheckDatabaseTableExistance(DBReadExecute.ParseConnectionString(this.mContext.CurrentRead.File), pDataTable.TableName))
                {
                    DBReadExecute.ExecuteSQL(this.mContext.CurrentRead.File, "Delete From [" + pDataTable.TableName + "]");
                    DBReadExecute.ExecuteSQL(this.mContext.CurrentRead.File, "Drop Table [" + pDataTable.TableName + "]");
                }
                DBReadExecute.ExecuteSQL(this.mContext.CurrentRead.File, DBReadExecute.GetCreateFromDataTableSQL(pDataTable.TableName, pDataTable));

                DBReadExecute.InsertData(this.mContext.CurrentRead.File, "Select * from [" + pDataTable.TableName + "]", pDataTable.CreateDataReader());

                result = true;
            }
            catch (Exception e)
            {
                // do nothing for now
            }
            return result;
        }

        public Dictionary<string, EpiInfo.Plugin.IVariable> EpiViewVariableList 
        {
            get
            {
                if (! this.isCachedEpiViewVariableList)
                {
                    this.mEpiViewVariableList = new Dictionary<string, EpiInfo.Plugin.IVariable>(StringComparer.OrdinalIgnoreCase);

                    if (this.mContext.CurrentRead != null && this.mContext.CurrentRead.IsEpi7ProjectRead && this.mContext.CurrentProject.Views.Exists(this.mContext.CurrentRead.Identifier))
                    {
                        View view = this.mContext.CurrentProject.Views[this.mContext.CurrentRead.Identifier];

                        Epi.Collections.NamedObjectCollection<IVariable> varCollection = this.mContext.MemoryRegion.GetVariablesInScope();
                        foreach (IVariable var in varCollection)
                        {

                            EpiInfo.Plugin.IVariable newVar = new PluginVariable();


                            if (view.Fields.Exists(var.Name))
                            {

                                switch ((int)view.Fields[var.Name].FieldType)
                                {
                                            /// <summary>

                                    case 1:// Text field type
                                    case 2:// Label field type
                                    case 3:// Uppercase text field type
                                    case 4:// Multiline text field type
                                          newVar.DataType = EpiInfo.Plugin.DataType.Text;
                                        break;
                                    case 5:// Number field type
                                        newVar.DataType = EpiInfo.Plugin.DataType.Number;
                                        break;
                                    case 6:// Phone number field type
                                        newVar.DataType = EpiInfo.Plugin.DataType.PhoneNumber;
                                        break;

                                    case 7:// Date field type
                                        newVar.DataType = EpiInfo.Plugin.DataType.Date;
                                        break;

                                    case 8:// Time field type
                                        newVar.DataType = EpiInfo.Plugin.DataType.Time;
                                        break;

                                    case 9:// Date/time field type
                                        newVar.DataType = EpiInfo.Plugin.DataType.DateTime;
                                        break;

                                    case 10:// Check box field type
                                    case 11:// Yes/No field type
                                        newVar.DataType = EpiInfo.Plugin.DataType.Boolean;
                                        break;

                                    case 17:// Legal Values field type
                                    case 18:// Codes field type
                                    case 19:// Comment Legal field type
                                        newVar.DataType = EpiInfo.Plugin.DataType.Text;
                                        break;

                                    case 22:// Rec Status field type
                                    case 23:// Unique Key field type
                                    case 28:// UniqueRowId
                                    case 24:// Foreign Key of a Unique Key field type
                                        newVar.DataType = EpiInfo.Plugin.DataType.Number;
                                        break;
                                    case 25:// GUID field type
                                    case 26: // GlobalRecordId
                                        newVar.DataType = EpiInfo.Plugin.DataType.GUID;
                                        break;


                                    case 12:// Radio button field type
                                    case 13:// Button field type
                                    case 14:// Image field type
                                    case 15:// Mirror field type
                                    case 16:// Grid field type
                                    case 20:// Related view field type
                                    case 21:// Group box field type
                                    case 27:// List
                                    default:
                                        newVar.DataType = EpiInfo.Plugin.DataType.Unknown;
                                        break;
                                }
                            }
                            else
                            {
                                newVar.DataType = (EpiInfo.Plugin.DataType)var.DataType;
                            }
                            if (view.Fields.Contains(var.Name) && view.Fields[var.Name] is Epi.Fields.FieldWithSeparatePrompt)
                            {
                                newVar.Prompt = ((Epi.Fields.FieldWithSeparatePrompt)view.Fields[var.Name]).PromptText;
                            }
                            else if (view.Fields.Contains(var.Name) && view.Fields[var.Name] is Epi.Fields.FieldWithoutSeparatePrompt)
                            {
                                newVar.Prompt = ((Epi.Fields.FieldWithoutSeparatePrompt)view.Fields[var.Name]).PromptText;
                            }
                            else
                            {
                                newVar.Prompt = var.PromptText;
                            }
                            newVar.Expression = var.Expression;
                            newVar.VariableScope = (EpiInfo.Plugin.VariableScope) var.VarType;
                            newVar.Name = var.Name;

                            this.mEpiViewVariableList.Add(var.Name, newVar);
                        }
                    }

                    this.isCachedEpiViewVariableList = true;
                }
                return this.mEpiViewVariableList;
            }
        }
    }
}
