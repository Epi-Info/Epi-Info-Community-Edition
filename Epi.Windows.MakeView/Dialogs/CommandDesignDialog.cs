#region Namespaces

using System;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;

using Epi;
//using Epi.Analysis;
using EpiInfo.Plugin;
using Epi.Collections;
using Epi.Windows.Dialogs;

#endregion  Namespaces

using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// Command Design Dialog
    /// </summary>
    public partial class CommandDesignDialog : DialogBase
	{
		#region private Class members
        
        /// <summary>
        /// Command processing modes
        /// </summary>
        public enum CommandProcessingMode
        {
            Save_And_Execute,
            Save_Only
        }


		private CommandProcessingMode  processingMode = CommandProcessingMode.Save_And_Execute;
        public CommandProcessingMode ProcessingMode
        {
            get { return this.processingMode; }
        }

		private string commandText = string.Empty;
        

		#endregion private Class members		
        
        #region Protected Data Members

        /// <summary>
        ///  The EpiIterpreter object accessable by Inherited Dialogs
        /// </summary>
        protected EpiInfo.Plugin.IEnterInterpreter EpiInterpreter;


        /// <summary>
        /// The ICommand Processor for the dialog
        /// </summary>
        protected ICommandProcessor dialogCommandProcessor;

        #endregion //Protected Data Members

        #region Constructors
        /// <summary>
        /// Default Constructor for execlusive use by the designer
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public CommandDesignDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Command Design Dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public CommandDesignDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            this.EpiInterpreter = frm.EpiInterpreter;

            //dialogCommandProcessor = Module.GetService(typeof(ICommandProcessor)) as ICommandProcessor;
            //if (dialogCommandProcessor == null)
            //{
            //    throw new GeneralException("Command processor is required but not available.");
            //}
        }
		#endregion Constructors

		#region Public Properties

        /// <summary>
        /// Returns the current Module
        /// </summary>
        public new IWindowsModule Module
        {
            get
            {
                return base.Module as IWindowsModule;
            }
        }

		/// <summary>
		/// Accessor method for property commandText
		/// </summary>
		public string CommandText
		{
			get
			{
				return commandText;
			}
			set
			{
				commandText = value;	
			}
		}
		#endregion Public Properties

		#region Event Handlers
		/// <summary>
		/// Sets the processing mode and generates command 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		protected virtual void btnOK_Click(object sender, System.EventArgs e)
		{
			processingMode = CommandProcessingMode.Save_And_Execute;
			OnOK();
		}
		/// <summary>
		/// Sets the processing mode and generates command 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		protected void btnSaveOnly_Click(object sender, System.EventArgs e)
		{
			processingMode = CommandProcessingMode.Save_Only;
			OnOK();
		}
		/// <summary>
		/// Closes the dialog
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		protected void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		
		#endregion Event Handlers

		#region Protected Methods

         /// <summary>
        /// FillVariableListBox()
        /// ListBox and ComboBox both derive from ListControl
        /// According to Help file Items property of ListControl is Public.
        /// Contrary to helpfile there is no such property.
        /// So polymorphism could not be used as expected
        /// </summary>
        /// <param name="lbx">ListBox to be filled</param>
        /// <param name="scopeWord">The scope of the variable</param>
        protected void FillVariableListBox(ListBox lbx, VariableType scopeWord)
        {
            lbx.Items.Clear();
            List<EpiInfo.Plugin.IVariable> vars = this.EpiInterpreter.Context.GetVariablesInScope((VariableScope)scopeWord);
            lbx.BeginUpdate();
            foreach (EpiInfo.Plugin.IVariable var in vars)
            {
                if (!(var is Epi.Fields.PredefinedDataField))
                {
                    lbx.Items.Add(var.Name.ToString());
                }
            }
            lbx.EndUpdate();
            lbx.Sorted = true;
            lbx.Refresh();
        }


        /// <summary>
        /// FillvariableCombo()
        /// </summary>
        /// <param name="cmb">ComboBox to be filled</param>
        /// <param name="scopeWord">The scope of the variable</param>
        protected void FillVariableCombo(ComboBox cmb, VariableType scopeWord)
        {
            cmb.Items.Clear();
            List<EpiInfo.Plugin.IVariable> vars = this.EpiInterpreter.Context.GetVariablesInScope((VariableScope)scopeWord);
            cmb.BeginUpdate();
            foreach (EpiInfo.Plugin.IVariable var in vars)
            {
                if (!(var is Epi.Fields.PredefinedDataField))
                {
                    cmb.Items.Add(var.Name.ToString());
                }
                //--Ei-99
                if (cmb.Name == "cmbAvailVar")
                if ((var is Epi.Fields.FirstSaveTimeField) || (var is Epi.Fields.LastSaveTimeField)) 
                    { cmb.Items.Add(var.Name.ToString());}
                //--
            }
            cmb.EndUpdate();
            cmb.Sorted = true;
            cmb.Refresh();
        }

        /// <summary>
        /// FillvariableCombo()
        /// </summary>
        /// <param name="cmb">ComboBox to be filled</param>
        /// <param name="scopeWord">The scope of the variable</param>
        /// <param name="typ">Limit selection to (DataType)</param>
        protected void FillVariableCombo(ComboBox cmb, VariableType scopeWord, DataType typ)
        {
            cmb.Items.Clear();
            List<EpiInfo.Plugin.IVariable> vars = this.EpiInterpreter.Context.GetVariablesInScope((VariableScope)scopeWord);
            cmb.BeginUpdate();
            foreach (EpiInfo.Plugin.IVariable var in vars)
            {
                int VType = (int)var.DataType;
                int FType = (int)typ;
                if (!(var is Epi.Fields.PredefinedDataField) && ((VType & FType) == VType))
                {
                    cmb.Items.Add(var.Name.ToString());
                }
            }
            cmb.EndUpdate();
            cmb.Sorted = true;
            cmb.Refresh();
        }



		/// <summary>
		/// Method for generating Commands
		/// </summary>
		protected virtual void GenerateCommand()
		{			
		}

		/// <summary>
		/// Method for preprocessing commands
		/// </summary>
		protected virtual void PreProcess()
		{
		}

		#endregion Protected Methods

        #region Private Methods
        /// <summary>
		/// If validation passes command is generated
		/// </summary>
		protected void OnOK()
		{
			if (ValidateInput() == true)
			{
				GenerateCommand();
				PreProcess();				
				this.DialogResult = DialogResult.OK;
				this.Hide();
			}
			else
			{
                this.DialogResult = DialogResult.None;
				ShowErrorMessages();
			}
		}
		#endregion Private Methods
	} // Class
}
