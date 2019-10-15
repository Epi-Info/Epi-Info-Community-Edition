#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Windows.Dialogs;

#endregion  Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// Command generator for Iocode
	/// </summary>
    public partial class IocodeDialog : CheckCodeDesignDialog
    {
        #region Constructors
        
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public IocodeDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        public IocodeDialog(MainForm frm) : base(frm)
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
        /// Handles the Click event of the Ok button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Output = "IOCODE " 
                + cbxIndustry.SelectedItem.ToString() 
                + ", " + cbxOccupation.SelectedItem.ToString()
                + ", " + cbxICode.SelectedItem.ToString()
                + ", " + cbxOCode.SelectedItem.ToString()
                + ", " + cbxITitle.SelectedItem.ToString()
                + ", " + cbxOTitle.SelectedItem.ToString()
                + ", " + cbxScheme.SelectedItem.ToString();
            this.DialogResult = DialogResult.OK;
            this.Hide();
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
                foreach (Fields.Field field in value.Fields)
                {
                    if (field is Fields.TextField)
                    {
                        cbxIndustry.Items.Add(field.Name);
                        cbxOccupation.Items.Add(field.Name);
                        cbxICode.Items.Add(field.Name);
                        cbxOCode.Items.Add(field.Name);
                        cbxITitle.Items.Add(field.Name);
                        cbxOTitle.Items.Add(field.Name);
                        cbxScheme.Items.Add(field.Name);
                    }
                    if (field is Fields.NumberField)
                    {
                        //cbxOccupation.Items.Add(field.Name);
                        //cbxICode.Items.Add(field.Name);
                    }
                }
            }
        }
        #endregion  //Public Properties

        private void cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (cbxIndustry.SelectedItem != null)
                && (cbxOccupation.SelectedItem != null)
                && (cbxICode.SelectedItem != null)
                && (cbxOCode.SelectedItem != null)
                && (cbxITitle.SelectedItem != null)
                && (cbxOTitle.SelectedItem != null)
                && (cbxScheme.SelectedItem != null);
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/check-commands-iocode.html");
        }
    }
}

