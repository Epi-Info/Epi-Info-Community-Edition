using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Fields;

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// Field group definition dialog
    /// </summary>
    public partial class GroupDefinition : Form
    {
        #region Fields
        private GroupField groupField;
        #endregion //Fields

		#region Constructors
		/// <summary>
		/// Constructor for the exclusive use by the designer.
		/// </summary>
		public GroupDefinition()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public GroupDefinition(MainForm frm, GroupField groupField)
        {
            InitializeComponent();
            this.groupField = groupField;
            LoadFormData();
        }        
        
        /// <summary>
        /// Constructor for the class
        /// </summary>
        public GroupDefinition(GroupField groupField)
        {
            InitializeComponent();
            this.groupField = groupField;
            LoadFormData();
        }
		#endregion //Constructors

        #region Private Methods
        private void LoadFormData()
        {
            txtTitle.Text = groupField.PromptText;
            pnlColor.BackColor = groupField.BackgroundColor;
        }
        #endregion //Private Methods

        #region Public Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected void SetFieldProperties()
        {
            groupField.PromptText = txtTitle.Text;
            groupField.Name = groupField.Page.GetView().ComposeFieldNameFromPromptText(txtTitle.Text);
            groupField.BackgroundColor = pnlColor.BackColor;
        }

        /// <summary>
        /// Gets the field defined by this field definition dialog
        /// </summary>
        public GroupField Group
        {
            get
            {
                return groupField;
            }
        }
        #endregion	//Public Methods

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.AllowFullOpen = true;
            dialog.CustomColors = new int[]{6916092, 15195440, 16107657, 1836924,
   3758726, 12566463, 7526079, 7405793, 6945974, 241502, 2296476, 5130294,
   3102017, 7324121, 14993507, 11730944,};
            dialog.AnyColor = false;
            dialog.Color = groupField.BackgroundColor;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                pnlColor.BackColor = dialog.Color;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SetFieldProperties();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }
    }
}