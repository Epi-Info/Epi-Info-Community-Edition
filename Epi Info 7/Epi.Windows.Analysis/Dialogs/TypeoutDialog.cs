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
    /// Dialog for the TYPEOUT Command inserts text, either a string or the
    /// contents of a file, into the output.
    /// <remarks>Typical uses might include comments or boilerplate.</remarks>
    /// <example>TYPEOUT 'My Fancy Logo.htm'</example>
    /// </summary>
    public partial class TypeoutDialog : CommandDesignDialog
    {
        #region Public Interface
        #region Constructor

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public TypeoutDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// TypeoutDialog constructor
        /// </summary>
        /// <param name="frm">The main form</param>
        public TypeoutDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }
        #endregion Constructor

        #region Public Enums and Constants
        //
        #endregion Public Enums and Constants

        #region Public Properties
        //
        #endregion Public Properties

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
        #endregion Public Interface

        #region Protected Interface
        //
        #region Protected Properties
        //
        #endregion Protected Properties

        #region Protected Methods
        /// <summary>
        /// Generates the command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder(CommandNames.TYPEOUT);
            sb.Append(StringLiterals.SPACE);
            if (optRichText.Checked)
            {
                sb.Append(Util.InsertInDoubleQuotes(txtRichText.Text));
            }
            else
            {
                sb.Append(Util.InsertInSingleQuotes(txtFilename.Text));
            }

            WordBuilder wb = new WordBuilder(StringLiterals.COMMA);
            if (boldToolStripButton.Checked || italicToolStripButton.Checked || underlineToolStripButton.Checked)
            {
                sb.Append(StringLiterals.SPACE);
                if (boldToolStripButton.Checked) wb.Append("BOLD");
                if (italicToolStripButton.Checked) wb.Append("ITALIC");
                if (underlineToolStripButton.Checked) wb.Append("UNDERLINE");

                sb.Append(Util.InsertInParantheses(wb.ToString()));
            }

            if ((!string.IsNullOrEmpty(textFontColorName)) || (!string.IsNullOrEmpty(fontSizeToolStripDDL.Text)))
            {
                sb.Append(StringLiterals.SPACE).Append("TEXTFONT");
                if (!string.IsNullOrEmpty(textFontColorName))
                {
                    sb.Append(StringLiterals.SPACE).Append(textFontColorName.ToUpperInvariant());
                }

                if (!string.IsNullOrEmpty(fontSizeToolStripDDL.Text))
                {
                    sb.Append(StringLiterals.SPACE).Append(fontSizeToolStripDDL.Text);
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
            if (optRichText.Checked)
            {
                if (string.IsNullOrEmpty(txtRichText.Text))
                {
                    ErrorMessages.Add(SharedStrings.NO_TEXT_VALUE);
                }
            }
            else if (optFilename.Checked)
            {
                if (string.IsNullOrEmpty(txtFilename.Text))
                {
                    ErrorMessages.Add(SharedStrings.NO_FILENAME);
                }
            }

            return (ErrorMessages.Count == 0);
        }

        #endregion Protected Methods

        #region Protected Events
        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-typeout.html");
        }

        #endregion Protected Events
        #endregion Protected Interface

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
                this.helpToolStripButton.Click += new EventHandler(this.btnHelp_Click);
            }
        }
        #endregion Private Methods

        #region Private Event
        private void TypeoutDialog_Load(object sender, EventArgs e)
        {
            boldToolStripButton.Image = baseImageList.Images[36];
            italicToolStripButton.Image = baseImageList.Images[49];
            underlineToolStripButton.Image = baseImageList.Images[64];
            fontColorToolStripDDL.Image = baseImageList.Images[10];
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            TypeoutDialog_Load(this, null);
            txtRichText.Text = string.Empty;
            txtFilename.Text = string.Empty;
        }

        private void optRichText_CheckedChanged(object sender, EventArgs e)
        {
            txtRichText.Enabled = optRichText.Checked;
            txtFilename.Enabled = !optRichText.Checked;
            btnBrowse.Enabled = !optRichText.Checked;
        }

        private void optFilename_CheckedChanged(object sender, EventArgs e)
        {
            txtFilename.Enabled = optFilename.Checked;
            btnBrowse.Enabled = optFilename.Checked;
            txtRichText.Enabled = !optFilename.Checked;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            txtFilename.Text = openFileDialog1.FileName;
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