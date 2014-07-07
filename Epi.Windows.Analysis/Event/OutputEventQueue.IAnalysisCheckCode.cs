using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Epi.Core.AnalysisInterpreter;
using Epi.Windows.Analysis.Forms;
using System.Data;
namespace Epi.Windows.Analysis.Forms
{
    public partial class OutputEventQueue : IAnalysisCheckCode
    {
        /// <summary>
        /// EventHandler delegate for CheckCode display processing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void OutputEventHandler(object sender, Epi.Windows.Analysis.Forms.OutputEventArg e);
        
        public delegate void ChangedEventHandler(object sender, EventArgs e);

        public delegate bool TryGetFileDialogDelgate(string filter, string caption, bool isReadOnly, out string filePath);
        public delegate bool DialogDelegate(string text, string caption, string mask, string modifier, ref object input);


        public TryGetFileDialogDelgate TryGetFileDialogMethod;
        public DialogDelegate DialogDelegateMethod;

        private Queue<Epi.Windows.Analysis.Forms.OutputEventArg> queue = new Queue<Windows.Analysis.Forms.OutputEventArg>();

        public event ChangedEventHandler Changed;

        protected virtual void OnChanged()
        {
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public virtual void Enqueue(OutputEventArg item)
        {
            queue.Enqueue(item);
            OnChanged();
        }
        public int Count { get { return queue.Count; } }

        public virtual OutputEventArg Dequeue()
        {
            OutputEventArg item = queue.Dequeue();
            OnChanged();
            return item;
        }

        void IAnalysisCheckCode.Dialog(string pTextPrompt, string pTitleText)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.TextPrompt = pTextPrompt;
            args.TitleText = pTitleText;
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.DialogSimple;
            queue.Enqueue(args);
            OnChanged();
         }

        void IAnalysisCheckCode.Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.TextPrompt = pTextPrompt;
            args.Variable = pVariable;
            args.ListType = pListType;
            args.TitleText = pTitleText;
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.DialogInputList;
            queue.Enqueue(args);
            OnChanged();
            pVariable = args.Variable;
        }

        bool IAnalysisCheckCode.Dialog(string text, string caption, string mask, string modifier, ref object input)
        {
            bool result = false;

            if (DialogDelegateMethod != null)
            {
                while (queue.Count > 0)
                {
                    // block until queue is empty
                }
                result = DialogDelegateMethod(text, caption, mask, modifier, ref input);
            }

            return result; 
        }

        void IAnalysisCheckCode.Display(Dictionary<string, string> pDisplayArgs)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.DisplayArgs = pDisplayArgs;
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.Display;
            queue.Enqueue(args);
            OnChanged();
        }

        void IAnalysisCheckCode.DisplayStatusMessage(Dictionary<string, string> pStatusArgs)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.StatusArgs = pStatusArgs;
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.DisplayStatusMessage;
            queue.Enqueue(args);
            OnChanged();
        }

        bool IAnalysisCheckCode.TryGetFileDialog(string filter, string caption, bool isReadOnly, out string filePath)
        {
            bool result = false;

            filePath = String.Empty;

            if (this.TryGetFileDialogMethod != null)
            {
                while (queue.Count > 0)
                {
                    // block until queue is empty
                }

                result = TryGetFileDialogMethod(filter, caption, isReadOnly, out filePath);
            }

            return result;
        }

        void IAnalysisCheckCode.SetOutputFile(string fileName)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.FileName = fileName;
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.SetOutputFile;
            queue.Enqueue(args);
            OnChanged();

        }

        void IAnalysisCheckCode.ChangeOutput(string fileName, bool isReplace, bool useRouteOut)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.FileName = fileName;
            args.IsReplace = isReplace;
            args.UseRouteOut = useRouteOut;
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.ChangeOutput;
            queue.Enqueue(args);
            OnChanged();

        }

        void IAnalysisCheckCode.Printout(string fileName)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.FileName = fileName;
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.Printout;
            queue.Enqueue(args);
            OnChanged();
        }

        void IAnalysisCheckCode.RunProgram(string command)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.Command = command;
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.RunProgram;
            queue.Enqueue(args);
            OnChanged();
        }

        void IAnalysisCheckCode.Quit()
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.Quit;
            queue.Enqueue(args);
            OnChanged();
        }

        void IAnalysisCheckCode.ShowWaitDialog(string message)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.ShowWaitDialog;
            queue.Enqueue(args);
            OnChanged();
        }

        void IAnalysisCheckCode.HideWaitDialog()
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.HideWaitDialog;
            queue.Enqueue(args);
            OnChanged();
        }

        void IAnalysisCheckCode.ReportIndeterminateTaskStarted(string message)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.Message = message;

            args.OutputEventType = OutputEventArg.OutputEventArgEnum.ReportIndeterminateTaskStarted;
            queue.Enqueue(args);
            OnChanged();
        }

        void IAnalysisCheckCode.ReportIndeterminateTaskEnded()
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            args.OutputEventType = OutputEventArg.OutputEventArgEnum.ReportIndeterminateTaskEnded;
            queue.Enqueue(args);
            OnChanged();
        }

                           
        void IAnalysisCheckCode.ShowGridTable( List<DataRow> table, List<string> identifierList, Epi.View epiView)
        {
            Epi.Windows.Analysis.Forms.OutputEventArg args = new OutputEventArg();
            
            args.Table = table;
            args.IdentifierList = identifierList;

            if (epiView != null)
            {
                args.EpiView = epiView;
            }

            args.OutputEventType = OutputEventArg.OutputEventArgEnum.ShowGridTable;
            queue.Enqueue(args);
            OnChanged();
        }

    }
}
