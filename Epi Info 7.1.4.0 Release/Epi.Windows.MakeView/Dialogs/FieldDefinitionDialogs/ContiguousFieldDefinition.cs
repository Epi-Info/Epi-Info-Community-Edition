using System;


namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Contiguous Field Definition dialog
    /// </summary>
    public partial class ContiguousFieldDefinition : PatternableTextFieldDefinition 
	{	
		#region Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public ContiguousFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Contiguous Field Definition dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public ContiguousFieldDefinition(MainForm frm) : base(frm)
		{
			InitializeComponent();
		}
		#endregion Constructors

		#region Event Handlers
        /// <summary>
        /// Check Changed event for cbxRange
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
		protected virtual void cbxRange_CheckedChanged(object sender, System.EventArgs e)
		{
			lblLower.Enabled = cbxRange.Checked;
			lblUpper.Enabled = cbxRange.Checked;
			txtLower.Enabled = cbxRange.Checked;
			txtUpper.Enabled = cbxRange.Checked;
            
		}
		#endregion	//Event Handlers
	
	}
}
