#region Namespaces

using System;
using System.Drawing;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Epi.Fields;
using Epi.Windows.Dialogs;


#endregion

namespace Epi.Windows.MakeView.Dialogs
{
    /// <summary>
    /// The Option Field Items dialog
    /// </summary>
    public partial class OptionFieldItemsDialogs : DialogBase
	{

		#region Private Members

		private OptionField optionField;
		private DataTable optionItems;

		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public OptionFieldItemsDialogs()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Option Field Items dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public OptionFieldItemsDialogs(MainForm frm) : base(frm)
        {
            InitializeComponent();
        }

		#endregion Constructors

		#region Public Properties

		/// <summary>
		/// Gets or sets the option field the option items will belong to
		/// </summary>
		public OptionField OptionField
		{
			get
			{
				return this.optionField;
			}
			set
			{
				this.optionField = value;
			}
		}

		/// <summary>
		/// Gets or sets a data table containing all the option items
		/// </summary>
		public DataTable OptionItems
		{
			get
			{
				return this.optionItems;
			}
			set
			{
				this.optionItems = value;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the load event of the options dialog
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void Options_Load(object sender, System.EventArgs e)
		{
            this.OptionItems = new DataTable();
            OptionItems.Columns.Add("Text");
			if (this.OptionField != null)
			{
                foreach (string item in OptionField.Options)
                {
                    OptionItems.Rows.Add(new string[] {item});
                }
			}
			dgOptions.DataSource = this.OptionItems;
		}

		/// <summary>
		/// Handles the click event of the OK button
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}

		#endregion

	}
}
