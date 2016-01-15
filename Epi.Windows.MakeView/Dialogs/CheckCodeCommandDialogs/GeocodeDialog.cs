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
	/// Command generator for Geocode
	/// </summary>
    public partial class GeocodeDialog : CheckCodeDesignDialog
    {
        #region Constructors
        
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public GeocodeDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="frm">The main form</param>
        public GeocodeDialog(MainForm frm) : base(frm)
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
            Output = "GEOCODE " + cbxAddress.SelectedItem.ToString() + ", " + cbxLatitude.SelectedItem.ToString() + ", " + cbxLongitude.SelectedItem.ToString();
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
                        cbxAddress.Items.Add(field.Name);
                    }
                    if (field is Fields.NumberField)
                    {
                        cbxLatitude.Items.Add(field.Name);
                        cbxLongitude.Items.Add(field.Name);
                    }
                }
            }
        }
        #endregion  //Public Properties

        private void cbxAddress_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (cbxAddress.SelectedItem != null) && (cbxLatitude.SelectedItem != null) && (cbxLongitude.SelectedItem != null);
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/check-commands-geocode.html");
        }
    }
}

