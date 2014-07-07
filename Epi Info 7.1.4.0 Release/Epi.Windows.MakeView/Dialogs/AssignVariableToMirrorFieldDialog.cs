using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi;
using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Dialogs
{
    public partial class AssignVariableToMirrorFieldDialog : DialogBase
	{
		#region Private Class Members
		private DataTable mirrorableFields;
		private int sourceFieldId;
		private FormMode mode;
		#endregion //Private Class Members

		#region Constructor

        /// <summary>
        /// Default constructor - Design mode only
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public AssignVariableToMirrorFieldDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor of the Assign Variable to Mirror Field dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="fields">Data table containing fields</param>
        /// <param name="fieldName">The field name</param>
        /// <param name="formMode">The form mode</param>
		public AssignVariableToMirrorFieldDialog(MainForm frm, DataTable fields, string fieldName, FormMode formMode) : base(frm)
		{
			InitializeComponent();
			mirrorableFields = fields;
			lblFieldName.Text = fieldName;
			mode = formMode; 
		}
		#endregion Constructors

		#region Public Properties

        /// <summary>
		/// Gets the name of the mirrored field
		/// </summary>
		public int SourceFieldId
		{
			get
			{
				return (sourceFieldId);
			}
		}
		#endregion //Public Properties

		#region Event Handlers
		private void AssignVariableToMirrorField_Load(object sender, System.EventArgs e)
		{
			LoadFields();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Hide();
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			sourceFieldId = int.Parse(lbxFields.SelectedValue.ToString());
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}
		#endregion //Event Handlers

		#region Private Methods
		private void LoadFields()
		{
			System.Data.DataView dv = mirrorableFields.DefaultView;
			dv.Sort = ColumnNames.NAME;
			lbxFields.DataSource = dv;
			lbxFields.DisplayMember = ColumnNames.NAME;
			lbxFields.ValueMember = ColumnNames.ID;
		}
		#endregion //Private Methods
	}
}
