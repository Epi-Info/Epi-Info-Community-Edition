#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;
using Epi.Fields;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class ClearDialog : CheckCodeDesignDialog
    {
        #region Constructors       
        
        /// <summary>
		/// Default Constructor - Design mode only
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public ClearDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public ClearDialog(MainForm frm) : base(frm)
        {
            InitializeComponent();
        }

        #endregion  //Constructors

        #region Private Event Handlers        
        
        /// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

        /// <summary>
        /// Handled the index selection change of fields in listbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void lbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (lbxFields.SelectedItem != null);
        }

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET suppied event parameters</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Output = CommandNames.CLEAR + StringLiterals.SPACE;            
//          Output = "CLEAR ";
            
            for (int i = 0; i <= lbxFields.SelectedItems.Count - 1; i++)
            {
                Output += lbxFields.SelectedItems[i].ToString() + StringLiterals.SPACE;
            }

            Output = Output.Trim();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/check-commands-CLEAR.html");
        }

        #endregion  //Private Event Handlers

        #region Public Properties        
        
        /// <summary>
        /// Sets the View for the dialog
        /// </summary>
        public override View View
        {
            set 
            {
                foreach (Field field in value.Fields)
                {
                    if (field is Fields.RenderableField && !(field is Fields.LabelField))
                    {
                        lbxFields.Items.Add(field.Name);
                    }
                }
            }
        }
        #endregion  //Public Properties
    }
}

