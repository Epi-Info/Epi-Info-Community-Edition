using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Dialog for the HEADEROUT Command inserts text, either a string or the
    /// contents of a file, into the output.
    /// <remarks>Typical uses might include comments or boilerplate.</remarks>
    /// <example>HEADEROUT 'My Fancy Logo.htm'</example>
    /// </summary>
    public partial class HeaderoutDialog : CommandDesignDialog
    {
        #region Constructor

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public HeaderoutDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// TablesDialog constructor
        /// </summary>
        /// <param name="frm">The main form</param>
        public HeaderoutDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }
        #endregion Constructor

        #region Public Methods
        /// <summary>
        /// Sets enabled property of OK and Save Only
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }
        #endregion Public Methods

        #region Protected Methods
        /// <summary>
        /// Generates the command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder(CommandNames.HEADER);
            sb.Append(StringLiterals.SPACE);
            sb.Append(1.ToString());
            sb.Append(StringLiterals.SPACE);

            if (true)
            {
                sb.Append(Util.InsertInDoubleQuotes(txtTitle.Text));

                WordBuilder wb = new WordBuilder(StringLiterals.COMMA);
                if (boldToolStripButton.Checked || italicToolStripButton.Checked || underlineToolStripButton.Checked)
                {
                    sb.Append(StringLiterals.SPACE);
                    if (boldToolStripButton.Checked) wb.Append("BOLD");
                    if (italicToolStripButton.Checked) wb.Append("ITALIC");
                    if (underlineToolStripButton.Checked) wb.Append("UNDERLINE");

                    sb.Append(Util.InsertInParantheses(wb.ToString()));
                }

                if (!string.IsNullOrEmpty(textFontColorName) && !string.IsNullOrEmpty(fontSizeToolStripDDL.Text))
                {
                    sb.Append(" TEXTFONT ");

                    if (!string.IsNullOrEmpty(textFontColorName))
                    {
                        sb.Append(textFontColorName.ToUpperInvariant());
                    }

                    if (!string.IsNullOrEmpty(fontSizeToolStripDDL.Text))
                    {
                        sb.Append(StringLiterals.SPACE);
                        sb.Append(fontSizeToolStripDDL.Text);
                    }
                }
            }

            CommandText = sb.ToString();
        }
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if there is no error; else false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                ErrorMessages.Add(SharedStrings.NO_TEXT_VALUE);
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/classic-analysis/how-to-manage-output-header.html");
        }

        #endregion Protected Methods

        #region Private Members
        #region Private Enums and Constants
        //
        #endregion Private Enums and Constants

        #region Private Attributes
        //
        #endregion Private Attributes

        #region Private Properties
        //
        string textFontColorName = string.Empty;
        #endregion Private Properties

        #region Private Methods
        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
                this.btnHelp.Click += new EventHandler(this.btnHelp_Click);
                this.fontSizeToolStripDDL.SelectedIndexChanged += new EventHandler(fontSizeToolStripDDL_SelectedIndexChanged);
                this.defaultToolStripMenuItem.CheckedChanged += new EventHandler(defaultToolStripMenuItem_CheckedChanged);
            }
        }

        private void defaultToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (fontSizeToolStripDDL.SelectedIndex > -1)
            {
                blackToolStripMenuItem.Select();
            }
        }

        private void fontSizeToolStripDDL_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fontSizeToolStripDDL.SelectedIndex > -1 && defaultToolStripMenuItem.Selected)
            {
                blackToolStripMenuItem.Select();
            }
        }

        private void EnableControls()
        {
            boldToolStripButton.Enabled = true;
            italicToolStripButton.Enabled = true;
            underlineToolStripButton.Enabled = true;
            fontSizeToolStripDDL.Enabled = true;
            fontColorToolStripDDL.Enabled = true;
        }

        private void DisableAndClearControls()
        {
            fontSizeToolStripDDL.Text = String.Empty;
            fontSizeToolStripDDL.Enabled = false;
            boldToolStripButton.Checked = false;
            boldToolStripButton.Enabled = false;
            italicToolStripButton.Checked = false;
            italicToolStripButton.Enabled = false;
            underlineToolStripButton.Checked = false;
            underlineToolStripButton.Enabled = false;
            fontColorToolStripDDL.Text = String.Empty;
            fontColorToolStripDDL.Enabled = false;
        }

        #endregion Private Methods

        #region Private Event
        private void HeaderoutDialog_Load(object sender, EventArgs e)
        {
            boldToolStripButton.Image = baseImageList.Images[36];
            italicToolStripButton.Image = baseImageList.Images[49];
            underlineToolStripButton.Image = baseImageList.Images[64];
            fontColorToolStripDDL.Image = baseImageList.Images[10];
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            txtTitle.Text = String.Empty;
            fontColorToolStripDDL.Text = String.Empty;
            fontSizeToolStripDDL.Text = String.Empty;
            boldToolStripButton.Checked = false;
            italicToolStripButton.Checked = false;
            underlineToolStripButton.Checked = false;
        }


        private void fontColorToolStripDDL_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.BackColor.Name.Equals(SystemColors.Control.Name))
                textFontColorName = string.Empty;
            else
                textFontColorName = e.ClickedItem.BackColor.Name;

            fontColorToolStripDDL.Text = textFontColorName;
            fontColorToolStripDDL.ToolTipText = "Font Color " + textFontColorName;
            fontColorToolStripDDL.ForeColor = e.ClickedItem.BackColor;
        }
        #endregion Private Event


        #endregion Private Members
    }
}