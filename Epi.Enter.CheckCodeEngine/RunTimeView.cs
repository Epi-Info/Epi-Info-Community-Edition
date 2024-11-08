using System;
using System.Collections.Generic;
using System.Text;
//using Epi.Core.Interpreter;
using Epi.Core;
using Epi.Fields;
using System.Text.RegularExpressions;
using System.Data;
using Epi.Data;
using EpiInfo.Plugin;
using System.Reflection;

namespace Epi.EnterCheckCodeEngine
{
    public class RunTimeView 
    {
        private IEnterInterpreter mEpiInterpreter;
        private IEnterInterpreterHost EnterClientInterface;
        private Epi.View mView;
        private Epi.Page mCurrentPage;
        private Epi.Fields.Field mCurrentField;
        private Dictionary<int,int> RecordNumberMap;
        private int recordCount;
        private int parentViewRecordId;
        private string parentViewGlobalRecordId;

        Stack<KeyValuePair<EventActionEnum,StackCommand>> AfterStack;

        public RunTimeView(IEnterInterpreterHost pEnterClientInterface, Epi.View pView)
        {

            // to do set the enter interpreter
            // application domain.
            Assembly a = Assembly.Load(pView.Project.EnterMakeviewIntepreter);
            // Get the type to use.
            Type myType = a.GetType(pView.Project.EnterMakeviewIntepreter + ".EpiInterpreterParser");

            // Create an instance.
            this.mEpiInterpreter = (IEnterInterpreter)Activator.CreateInstance(myType, new object[] { pEnterClientInterface });
            this.mEpiInterpreter.Host = pEnterClientInterface;

            //this.mEpiInterpreter = new EpiInterpreterParser(Epi.Resources.ResourceLoader.GetEnterCompiledGrammarTable(), pEnterClientInterface, Rule_Context.eRunMode.Enter);
            this.EnterClientInterface = pEnterClientInterface;
            this.AfterStack = new Stack<KeyValuePair<EventActionEnum, StackCommand>>();
            this.RecordNumberMap = new Dictionary<int, int>();
            this.mView = pView;
            
        }


        public RunTimeView(IEnterInterpreterHost pEnterClientInterface, Epi.View pView, int pParentRecordId, string pParentGlobalRecordId, IScope pScope = null)
        {
            // to do set the enter interpreter
            // application domain.
            Assembly a = Assembly.Load(pView.Project.EnterMakeviewIntepreter);
            // Get the type to use.
            Type myType = a.GetType(pView.Project.EnterMakeviewIntepreter + ".EpiInterpreterParser");


            // Create an instance.
            if (pScope == null)
            {
                this.mEpiInterpreter = (IEnterInterpreter)Activator.CreateInstance(myType, new object[] { pEnterClientInterface });
            }
            else
            {
                this.mEpiInterpreter = (IEnterInterpreter)Activator.CreateInstance(myType, new object[] { pEnterClientInterface, pScope });
            }
            this.mEpiInterpreter.Context.Scope.Name = pView.Name;
            this.mEpiInterpreter.Host = pEnterClientInterface;


            this.EnterClientInterface = pEnterClientInterface;
            this.AfterStack = new Stack<KeyValuePair<EventActionEnum, StackCommand>>();
            this.parentViewRecordId = pParentRecordId;
            this.parentViewGlobalRecordId = pParentGlobalRecordId;
            this.RecordNumberMap = new Dictionary<int, int>();
            this.mView = pView;            
            this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;            
        }

        public Epi.IMemoryRegion MemoryRegion
        {
            get 
            { 
                return null;
            }
        }

        public Epi.View View
        {
            get {return this.mView;}
        }

        public Epi.Page CurrentPage
        {
            get { return this.mCurrentPage; }
        }

        public Epi.Fields.Field CurrentField
        {
            get { return this.mCurrentField; }
        }

        public int ParentRecordId
        {
            get { return this.parentViewRecordId; }
            set { this.parentViewRecordId = value; }
        }

        public string ParentGlobalRecordId
        {
            get { return this.parentViewGlobalRecordId; }
            set { this.parentViewGlobalRecordId = value; }
        }
        public IEnterInterpreter EpiInterpreter
        {
            get { return this.mEpiInterpreter; }
        }

        public void OpenViewHandler(object sender, OpenViewEventArgs e)
        {
            if (this.mView.GetProject().CollectedData.TableExists(mView.TableName) == false)
            {
                this.mView.GetProject().CollectedData.CreateDataTableForView(mView, 1);
            }

            this.mView.MustRefreshFieldCollection = true;

            if (this.mView.IsRelatedView)
            {                
                this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;                
            }
            
            this.mCurrentPage = this.mView.Pages[0];

            // access fields to be sure available  in the checkcode
            Epi.Collections.FieldCollectionMaster Fields =  this.mView.Fields;

            // push after_view onto stack
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseView, new StackCommand(this.EpiInterpreter, "view","after","")));

            // execute before_view
            //this.RunCheckCodeCommands(this.mView.CheckCodeVariableDefinitions);

            // will run the variable definitions and setup compiled context
            foreach (Field field in this.View.Fields)
            {
                if (field is IDataField)
                {
                    EpiInfo.Plugin.IVariable definedVar = (EpiInfo.Plugin.IVariable)field;
                    this.mEpiInterpreter.Context.DefineVariable(definedVar);
                }
            }

            this.RunCheckCodeCommands(this.mView.CheckCode);

            this.ExecuteCheckCode("view","before","");
            this.recordCount = this.View.GetRecordCount();
        }

        public void CheckCodeHandler(object sender, RunCheckCodeEventArgs e)
        {
            switch (e.EventType)
            {
                /*case EventActionEnum.OpenView:
                    this.OpenView(sender, e);
                    break;*/
                case EventActionEnum.CloseView:
                    this.CloseView(sender, e);
                    break;
                case EventActionEnum.OpenRecord:
                    switch (e.Parameter)
                    {
                        case "+":
                        case Constants.Plus:
                            this.NewRecord(sender, e);
                            break;
                        case "<<":
                            this.FirstRecord(sender, e);
                            break;
                        case "<":
                            this.PreviousRecord(sender, e);
                            break;
                        case ">":
                            this.NextRecord(sender, e);
                            break;
                        case ">>":
                            this.LastRecord(sender, e);
                            break;
                        default:
                            this.GotoRecord(sender, e);
                            break;
                    }
                    break;
                case EventActionEnum.CloseRecord:
                    this.CloseRecord(sender, e);
                    break;
                case EventActionEnum.OpenPage:
                    this.OpenPage(sender, e);
                    break;
                case EventActionEnum.ClosePage:
                    this.ClosePage(sender, e);
                    break;
                case EventActionEnum.OpenField:
                    this.OpenField(sender, e);
                    break;
                case EventActionEnum.CloseField:
                    this.CloseField(sender, e);
                    break;
                case EventActionEnum.ClickField:
                    this.ClickField(sender, e);
                    break;
                case EventActionEnum.FirstRecord:
                    this.FirstRecord(sender, e);
                    break;
                case EventActionEnum.NextRecord:
                    this.NextRecord(sender, e);
                    break;
                case EventActionEnum.PreviousRecord:
                    this.PreviousRecord(sender, e);
                    break;
                case EventActionEnum.LastRecord:
                    this.LastRecord(sender, e);
                    break;
                case EventActionEnum.GotoRecord:
                    this.GotoRecord(sender, e);
                    break;
                case EventActionEnum.NewRecord:
                    this.NewRecord(sender, e);
                    break;
                default:
                    break;
            }
        }
        /*private void OpenView(object sender, RunCheckCodeEventArgs e)
        {

        }*/
        public void CloseView(object sender, RunCheckCodeEventArgs e)
        {
            //execute after stack until current level = view
            while (this.AfterStack.Count > 0)
            {
                    this.AfterStack.Pop().Value.Execute();
            }

            this.mCurrentField = null;
            this.mCurrentPage = null;
            this.mView = null;

            // then execute current level after_view
            //this.RunCheckCodeCommands(this.mView.CheckCodeAfter);
        }

        private void OpenRecord(object sender, RunCheckCodeEventArgs e)
        {
            this.UnRollRecord();
            // push after_record onto stack
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseRecord, new StackCommand(this.EpiInterpreter,"record","after","")));
            this.ExecuteCheckCode("record", "before", "");
            //this.RunCheckCodeCommands(this.mView.RecordCheckCodeBefore);
        }
        private void CloseRecord(object sender, RunCheckCodeEventArgs e)
        {
            EventActionEnum Level = this.AfterStack.Peek().Key;

            switch (Level)
            {
                case EventActionEnum.CloseRecord:
                    this.AfterStack.Pop().Value.Execute();
                    break;
                case EventActionEnum.ClosePage:
                    this.AfterStack.Pop().Value.Execute();
                    this.AfterStack.Pop().Value.Execute();
                    break;
                case EventActionEnum.CloseField:
                    this.AfterStack.Pop().Value.Execute();
                    this.AfterStack.Pop().Value.Execute();
                    this.AfterStack.Pop().Value.Execute();
                    break;
                default:
                    break;
            }

            if (View.IsEmptyNewRecord() == false)
            {
                this.View.SaveRecord(this.View.CurrentRecordId);
            }
        }

        private void OpenPage(object sender, RunCheckCodeEventArgs e)
        {
            this.UnRollPage();

            // push after_page onto stack
            if (this.AfterStack.Peek().Key == EventActionEnum.CloseView)
            {
                this.OpenRecord(sender, new RunCheckCodeEventArgs(EventActionEnum.OpenRecord,"<<"));
            }

            Epi.Page page = this.mView.Pages.Find(x => x.Name.ToUpperInvariant() == e.Parameter.ToUpperInvariant());
            this.mCurrentPage = page;
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.ClosePage, new StackCommand(this.EpiInterpreter, "page", "after", page.Name)));
            this.ExecuteCheckCode("page", "before", page.Name);
        }
        
        private void ClosePage(object sender, RunCheckCodeEventArgs e)
        {
            EventActionEnum Level = this.AfterStack.Peek().Key;

            switch (Level)
            {
                case EventActionEnum.ClosePage:
                    this.AfterStack.Pop().Value.Execute();
                    break;
                case EventActionEnum.CloseField:
                    this.AfterStack.Pop().Value.Execute();
                    this.AfterStack.Pop().Value.Execute();
                    break;
                default:
                    break;
            }
        }

        private void OpenField(object sender, RunCheckCodeEventArgs e)
        {
            this.mCurrentField = this.mView.Fields[e.Parameter];

            this.UnRollField();

            // push after_field onto stack

            if (this.mCurrentField is IFieldWithCheckCodeAfter)
            {
                this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseField, new StackCommand(this.EpiInterpreter, "field", "after", this.mCurrentField.Name)));
            }

            if (this.mCurrentField is IFieldWithCheckCodeBefore)
            {
                this.ExecuteCheckCode("field", "before", this.mCurrentField.Name);
            }
        }

        private void CloseField(object sender, RunCheckCodeEventArgs e)
        {
            if (this.AfterStack.Peek().Key == EventActionEnum.CloseField)
            {
                this.AfterStack.Pop().Value.Execute();
            }
        }

        private void ClickField(object sender, RunCheckCodeEventArgs e)
        {
            if(this.mView.Fields.Contains(e.Parameter))
            {
                this.mCurrentField = this.mView.Fields[e.Parameter];
            }

            if (this.mCurrentField is IFieldWithCheckCodeClick)
            {
                this.ExecuteCheckCode("field", "click", this.mCurrentField.Name);
            }
        }

        private void FirstRecord(object sender, RunCheckCodeEventArgs e)
        {
            this.UnRollRecord();
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseRecord, new StackCommand(this.EpiInterpreter, "record", "after", "")));
            this.ResetFields();
            
            if (this.mView.IsRelatedView)
            {
                this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;                
            }            
            
            this.mView.GlobalRecordIdField.NewValue();
            this.mView.LoadFirstRecord();
            this.UpdateCurrentRecordNumber();
            this.ExecuteCheckCode("record", "before", "");
            
        }
        private void NextRecord(object sender, RunCheckCodeEventArgs e)
        {
            this.UnRollRecord();
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseRecord, new StackCommand(this.EpiInterpreter, "record", "after", "")));
            this.ResetFields();

            if (this.mView.IsRelatedView)
            {                
                this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;                
            }
            
            this.mView.GlobalRecordIdField.NewValue();
            this.mView.LoadNextRecord(this.mView.CurrentRecordId);
            this.UpdateCurrentRecordNumber();
            this.ExecuteCheckCode("record", "before", "");
        }
        private void PreviousRecord(object sender, RunCheckCodeEventArgs e)
        {
            this.UnRollRecord();
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseRecord, new StackCommand(this.EpiInterpreter, "record", "after", "")));
            this.ResetFields();
            

            if (this.mView.IsRelatedView)
            {
                this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;                
            }
            
            this.mView.GlobalRecordIdField.NewValue();
            this.mView.LoadPreviousRecord(this.mView.CurrentRecordId);
            this.UpdateCurrentRecordNumber();
            this.ExecuteCheckCode("record", "before", "");
        }
        private void LastRecord(object sender, RunCheckCodeEventArgs e)
        {
            this.UnRollRecord();
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseRecord, new StackCommand(this.EpiInterpreter, "record", "after", "")));
            this.ResetFields();
            

            if (this.mView.IsRelatedView)
            {                
                this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;                
            }
            
            this.mView.GlobalRecordIdField.NewValue();
            this.mView.LoadLastRecord();
            this.UpdateCurrentRecordNumber();
            this.ExecuteCheckCode("record", "before", "");
        }

        private void NewRecord(object sender, RunCheckCodeEventArgs e)
        {
            //----2101
            this.ClearPageEventForNewRecord(e.Parameter);
            //--
            this.UnRollRecord();
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseRecord, new StackCommand(this.EpiInterpreter, "record", "after", "")));
            this.ResetFields();
            

            foreach (IDataField dataField in this.mView.Fields.DataFields)
            {
                if (dataField is IInputField && ((IInputField)dataField).ShouldRepeatLast)
                {
                    this.mView.IsDirty = true;
                }
                else
                {
                    dataField.CurrentRecordValueObject = null;
                }
            }

            if (this.mView.IsRelatedView)
            {
                this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;                
            }
            
            this.mView.GlobalRecordIdField.NewValue();
            this.ExecuteCheckCode("record", "before", "");
            
        }

        //--2101
        private void ClearPageEventForNewRecord(string param)
        {
            //clearing page event for New
            if (this.AfterStack.Peek().Key == EventActionEnum.CloseField && param == Constants.Plus)
            {
                    this.AfterStack.Pop();
                    this.AfterStack.Pop();
                    this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseField, new StackCommand(this.EpiInterpreter, "field", "after", this.mCurrentField.Name)));
            } 
            else if (this.AfterStack.Peek().Key == EventActionEnum.ClosePage && param == Constants.Plus)
            {
                 this.AfterStack.Pop();
            }
         }
        //---
        private void GotoRecord(object sender, RunCheckCodeEventArgs e)
        {
            this.UnRollRecord();
            this.AfterStack.Push(new KeyValuePair<EventActionEnum, StackCommand>(EventActionEnum.CloseRecord, new StackCommand(this.EpiInterpreter, "record", "after", "")));
            this.ResetFields();
            
            int RecordId = 0;

            if (int.TryParse(e.Parameter, out RecordId))
            {
                if (this.mView.IsRelatedView)
                {
                    this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;
                }
                
                this.mView.GlobalRecordIdField.NewValue();
                this.mView.LoadRecord(RecordId);
                this.UpdateCurrentRecordNumber();

                this.ExecuteCheckCode("record", "before", "");
            }
        }

        /// <summary>
        /// Runs check code commands
        /// </summary>
        /// <param name="checkCode">The check code to be executed</param>
        public void RunCheckCodeCommands(string checkCode)
        {
            System.Text.RegularExpressions.Regex re = new Regex("\n");
            checkCode = checkCode.Trim().Trim(System.Environment.NewLine.ToCharArray());
            if (!string.IsNullOrEmpty(checkCode))
            {
                this.mEpiInterpreter.Execute(checkCode.Trim());
            }

        }

        public void ExecuteCheckCode(string pLevel, string pEvent, string pIdentifier)
        {
            ICommand command = null;
            string QueryString = string.Format("level={0}&event={1}&identifier={2}", pLevel.ToLowerInvariant(), pEvent.ToLowerInvariant(), pIdentifier.ToLowerInvariant());

            command = this.EpiInterpreter.Context.GetCommand(QueryString);
            if (command != null)
            {
                command.Execute();
            }
        }

        /*
        private void UnRollView()
        {

            while (this.AfterStack.Count > 0)
            {
                this.AfterStack.Pop().Value.Execute();
            }
        }*/

        private void UnRollRecord()
        {
            while (this.AfterStack.Peek().Key != EventActionEnum.CloseView)
            {
                StackCommand command = this.AfterStack.Peek().Value;
                command.Execute();
                this.AfterStack.Pop();
            }

            if (View.IsEmptyNewRecord() == false)
            {
                if (this.View.CurrentRecordId != 0 && this.View.IsDirty)
                {
                    this.View.SaveRecord(this.View.CurrentRecordId);
                }
            }
        }

        private void UnRollPage()
        {
            EventActionEnum CurrentState = this.AfterStack.Peek().Key;
            switch (CurrentState)
            {
                case EventActionEnum.CloseField:
                        this.AfterStack.Pop().Value.Execute();
                        this.AfterStack.Pop().Value.Execute();
                    break;
                case EventActionEnum.ClosePage:
                        this.AfterStack.Pop().Value.Execute();
                    break;
                default:
                case EventActionEnum.CloseRecord:
                    break;
            }
        }

        private void UnRollField()
        {
            EventActionEnum CurrentState = this.AfterStack.Peek().Key;
            switch (CurrentState)
            {
                case EventActionEnum.CloseField:
                    StackCommand stackCommand = this.AfterStack.Pop().Value;
                    if ((stackCommand.Identifier == CurrentField.Name && stackCommand.Event == "after") == false)
                    {
                        stackCommand.Execute();
                    }
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Saves the current record
        /// </summary>
        public int SaveRecord()
        {
            int result = -1;

            if (View.IsEmptyNewRecord() == false || View.IsParent())
            {
                if (this.mView.IsRelatedView)
                {
                    this.mView.ForeignKeyField.CurrentRecordValueString = this.parentViewGlobalRecordId.ToString();
                    if (this.mView.GlobalRecordIdField.CurrentRecordValueString == string.Empty)
                    {
                        this.mView.GlobalRecordIdField.CurrentRecordValueString = Guid.NewGuid().ToString();
                    }
                }

                if (!string.IsNullOrEmpty(this.mView.UniqueKeyField.CurrentRecordValueString) && this.mView.UniqueKeyField.CurrentRecordValue != 0)
                {
                    result = this.mView.SaveRecord(this.mView.UniqueKeyField.CurrentRecordValue);
                }
                else
                {
                    this.mView.CurrentGlobalRecordId = Guid.NewGuid().ToString();
                    result = this.mView.SaveRecord();
                    this.mView.UniqueKeyField.CurrentRecordValue = result;
                    this.mView.RecStatusField.CurrentRecordValue = 1;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.mView.UniqueKeyField.CurrentRecordValueString))
                {
                    if (this.mView.IsRelatedView)
                    {
                        this.mView.ForeignKeyField.CurrentRecordValueString = this.ParentGlobalRecordId;
                    }
                    this.mView.GlobalRecordIdField.NewValue();
                }
                else
                {
                    result = this.mView.SaveRecord();
                    this.mView.UniqueKeyField.CurrentRecordValue = result;
                    this.mView.RecStatusField.CurrentRecordValue = 1;
                }
            }
           
            return result;
        }

        public int CurrentRecordNumber
        {
            get
            {
                if (this.mView.CurrentRecordId > 0)
                {
                    if (this.RecordNumberMap.ContainsKey(this.mView.CurrentRecordId))
                    {
                        return this.RecordNumberMap[this.mView.CurrentRecordId];
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return this.mView.CurrentRecordId;
                }
            }
        }
        public int RecordCount
        {
            get { return this.recordCount; }
        }

        public void UpdateCurrentRecordNumber()
        {
            if (this.mView.CurrentRecordId > 0)
            {
                if (!this.RecordNumberMap.ContainsKey(this.mView.CurrentRecordId))
                {
                    string relatedViewFilter = string.Empty;
                    if (this.mView.IsRelatedView)
                    {
                        relatedViewFilter = " and ";
                        relatedViewFilter += ColumnNames.FOREIGN_KEY;
                        relatedViewFilter += StringLiterals.EQUAL + "'" + mView.ForeignKeyField.CurrentRecordValueString + "'";
                    }

                    DataTable DT = DBReadExecute.GetDataTable(this.mView.Project.FilePath,"Select Count(*) From [" + this.mView.TableName + "] Where UniqueKey <= " + this.mView.CurrentRecordId.ToString() + relatedViewFilter);
                    if(DT.Rows.Count > 0)
                    {
                        this.RecordNumberMap.Add(this.mView.CurrentRecordId,int.Parse(DT.Rows[0][0].ToString()));
                    }
                }
            }
        }

        private void ResetFields()
        {
            foreach (Field field in this.View.Fields)
            {
                field.IsVisible = true;
                field.IsHighlighted = false;
                
                if (field is InputFieldWithoutSeparatePrompt)
                {
                    if (((InputFieldWithoutSeparatePrompt)field).IsReadOnly)
                    {
                        field.IsEnabled = false;
                    }
                    else
                    {
                        field.IsEnabled = true;
                    }
                }
                else if (field is InputFieldWithSeparatePrompt)
                {
                    if (((InputFieldWithSeparatePrompt)field).IsReadOnly)
                    {
                        field.IsEnabled = false;
                    }
                    else
                    {
                        field.IsEnabled = true;
                    }
                }
                else
                {
                    field.IsEnabled = true;
                }

                if (field is MirrorField)
                {
                    field.IsEnabled = false;
                }
            }
        }

    }
}
