using System;
using System.Collections.Generic;
using System.Text;
//using Epi.Core.Interpreter;
using Epi.Core;
using EpiInfo.Plugin;
using System.Reflection;
using System.IO;

namespace Epi.EnterCheckCodeEngine
{
    public delegate void OpenViewHandler(object sender, OpenViewEventArgs e);
    public delegate void CheckCodeHandler(object sender, RunCheckCodeEventArgs e);
    public delegate void CheckCodeExecuter(string pCheckCode);
    public enum EventActionEnum
    {
        OpenView,
        CloseView,
        OpenRecord,
        CloseRecord,
        OpenPage,
        ClosePage,
        OpenField,
        CloseField,
        ClickField,
        FirstRecord,
        NextRecord,
        PreviousRecord,
        LastRecord,
        GotoRecord,
        NewRecord
    }
    
    public class CheckCodeEngine
    {
        private Stack<RunTimeView> mCurrentView;
        private Epi.Project mCurrentProject;

        public CheckCodeEngine()
        {
            this.Reset();
        }

        public Epi.IMemoryRegion MemoryRegion
        {
            get { return this.mCurrentView.Peek().MemoryRegion; }
        }

        public void OpenViewHandler(object sender, OpenViewEventArgs e)
        {
            Epi.View view = mCurrentProject.Views.GetViewById(e.ViewId);
            if (this.mCurrentView.Count == 0)
            {
                this.mCurrentView.Push(new RunTimeView(e.EnterCheckCodeInterface, view));
            }
            else
            {
                if (
                        this.mCurrentProject.FilePath != view.GetProject().FilePath
                        || view.ParentView == null
                        || view.ParentView.Name != this.mCurrentView.Peek().View.Name
                    )
                {
                    while (this.mCurrentView.Count > 0)
                    {
                        this.mCurrentView.Pop().CloseView(this, new RunCheckCodeEventArgs(EventActionEnum.CloseView, ""));
                    }

                    this.mCurrentView.Push(new RunTimeView(e.EnterCheckCodeInterface, view));
                }
                else
                {
                    RunTimeView runTimeView = this.mCurrentView.Peek();
                    int CurrentRecordId = runTimeView.View.CurrentRecordId;
                    string CurrentGlobalRecordId = runTimeView.View.CurrentGlobalRecordId;

                    EpiInfo.Plugin.IScope scope = runTimeView.EpiInterpreter.Context.GetNewScope(runTimeView.View.Name, runTimeView.EpiInterpreter.Context.Scope);

                    foreach (Epi.Fields.Field field in runTimeView.View.Fields)
                    {
                        if (field is EpiInfo.Plugin.IVariable)
                        {
                            PluginVariable pluginVariable = new PluginVariable();
                            pluginVariable.VariableScope = EpiInfo.Plugin.VariableScope.Standard;
                            pluginVariable.Name = field.Name;
                            pluginVariable.DataType = ((EpiInfo.Plugin.IVariable)field).DataType;

                            if (field is Epi.Fields.CheckBoxField || field is Epi.Fields.YesNoField)
                            {
                                if (((Epi.Fields.IDataField)field).CurrentRecordValueString == "1")
                                {
                                    pluginVariable.Expression = "true";
                                }
                                else if (((Epi.Fields.IDataField)field).CurrentRecordValueString == "0")
                                {
                                    pluginVariable.Expression = "false";
                                }
                                else
                                {
                                    pluginVariable.Expression = null;
                                }
                            }
                            else
                            {
                                pluginVariable.Expression = ((Epi.Fields.IDataField)field).CurrentRecordValueString;
                            }

                            scope.Define(pluginVariable);
                        }
                    }

                    this.mCurrentView.Push(new RunTimeView(e.EnterCheckCodeInterface, view, CurrentRecordId, CurrentGlobalRecordId, scope));
                }
            }
        
            this.mCurrentView.Peek().OpenViewHandler(sender, e);
        }

        public void CheckCodeHandler(object sender, RunCheckCodeEventArgs e)
        {
            if (this.mCurrentView.Count > 0)
            {
                switch (e.EventType)
                {
                    case EventActionEnum.CloseView:
                        this.mCurrentView.Peek().CheckCodeHandler(sender, e);
                        this.mCurrentView.Pop();
                        break;
                    default:
                        this.mCurrentView.Peek().CheckCodeHandler(sender, e);
                        break;
                }
            }
        }

        public RunTimeView CurrentView
        {
            get
            {
                if (this.mCurrentView.Count > 0)
                {
                    return this.mCurrentView.Peek();
                }
                else
                {
                    return null;
                }
            }
        }

        public Epi.Project Project
        {
            get { return this.mCurrentProject; }
            set 
            { 
                this.mCurrentProject = value; 
            }
        }

        public void Reset()
        {
            this.mCurrentView = new Stack<RunTimeView>();
            this.mCurrentProject = null;
        }

        /// <summary>
        /// Saves the current record
        /// </summary>
        public int SaveRecord()
        {
            int result = -1;
            if (this.mCurrentView.Count > 0)
            {
                result = this.mCurrentView.Peek().SaveRecord();
            }

            return result;
        }

        public void Close()
        {
            while (this.mCurrentView.Count > 0)
            {
                this.mCurrentView.Pop().CloseView(this, new RunCheckCodeEventArgs(EventActionEnum.CloseView,""));
            }
        }
    }
}
