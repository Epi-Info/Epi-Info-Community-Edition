#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Windows.Dialogs;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class AutoSearchDialog : CheckCodeDesignDialog
	{
        #region Constructors

        /// <summary>
        /// Default constructor - Design mode only
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public AutoSearchDialog()
        {
            InitializeComponent();
        }

        /// <summary>
		/// Constructor for the class
		/// </summary>
        public AutoSearchDialog(MainForm frm) : base(frm)
		{
			InitializeComponent();
        }
        #endregion Constructors

        #region Private Event Handling

        /// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object thate fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
        }

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            Output = CommandNames.AUTOSEARCH + StringLiterals.SPACE;
            //            Output = "AUTOSEARCH ";

            for (int i = 0; i <= lbxFields.SelectedItems.Count - 1; i++)
            {
                Output += lbxFields.SelectedItems[i].ToString() + StringLiterals.SPACE;
            }

            if (chkDisplayList.Checked)
            {
                Output += CommandNames.DISPLAYLIST + StringLiterals.SPACE;
                for (int i = 0; i <= lbxFields.SelectedItems.Count - 1; i++)
                {
                    Output += lbxFields.SelectedItems[i].ToString() + StringLiterals.SPACE;
                }
            }

            if (chkAlways.Checked)
            {
                Output += CommandNames.ALWAYS + StringLiterals.SPACE;
            }

            Output = Output.Trim();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void lbxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (lbxFields.SelectedItem != null);
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/check-commands-AUTOSEARCH.html");
        }

        #endregion  //Private Event Handling

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
                    if (field is Fields.RenderableField && !(field is Fields.LabelField) && !(field is Fields.GUIDField))
                    {                        
                        lbxFields.Items.Add(field.Name);
                    }
                }
            }
        }

        #endregion  //Public Properties

    }
}

