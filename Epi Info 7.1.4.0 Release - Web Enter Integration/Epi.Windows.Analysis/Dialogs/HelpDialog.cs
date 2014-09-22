#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using mshtml;
using System.Net;

#endregion  //Namespaces

namespace Epi.Windows.Analysis.Dialogs
{
    public partial class HelpDialog : CommandDesignDialog
    {
        #region Private Attributes
        private bool showSaveOnly = false;
        #endregion Private Attributes

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public HelpDialog()
        {
            InitializeComponent();
        }
                /// <summary>
        /// Constructor for AssignDialog
        /// </summary>
        /// <param name="frm">The main form</param>
        public HelpDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor for AssignDialog.  if showSave, enable the SaveOnly button
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="showSave">True or False to show Save Only button</param>
        public HelpDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm, bool showSave)
            : base(frm)
        {
            InitializeComponent();
            if (showSave)
            {
                showSaveOnly = true;
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
                Construct();
            }
        }
        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Checks if input is sufficient and Enables control buttons accordingly
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }
        #endregion Public Methods               

        #region Protected methods
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if ErrorMessages.Count is 0; otherwise false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            if (string.IsNullOrEmpty(this.txtFilename.Text))
            {
                ErrorMessages.Add(SharedStrings.NO_FILENAME);
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Generates command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder command = new StringBuilder();
            command.Append(CommandNames.HELP).Append(StringLiterals.SPACE);
            command.Append(StringLiterals.SINGLEQUOTES).Append(txtFilename.Text.Trim()).Append(StringLiterals.SINGLEQUOTES);
            command.Append(StringLiterals.SPACE);
            command.Append(StringLiterals.DOUBLEQUOTES).Append(this.cmbAnchor.Text.Trim()).Append(StringLiterals.DOUBLEQUOTES);
            CommandText = command.ToString();
        }

        #endregion Protected methods

        #region Private Methods
        /// <summary>
        /// Repositions buttons on dialog
        /// </summary>
        private void RepositionButtons()
        {
            int x = this.btnClear.Left;
            int y = btnClear.Top;
            btnClear.Location = new Point(btnCancel.Left, y);
            btnCancel.Location = new Point(btnOK.Left, y);
            btnOK.Location = new Point(btnSaveOnly.Left, y);
            btnSaveOnly.Location = new Point(x, y);
        }

        /// <summary>
        /// Construct the dialog
        /// </summary>
        private void Construct()
        {
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        }

        /// <summary>
        /// Loads the anchors for a specified html file
        /// </summary>
        /// <param name="fileName">The file name in which anchors are to be retrieved from</param>
        private void LoadAnchors(string fileName)
        {
            WebClient client = new WebClient();
            byte[] data = client.DownloadData(fileName);
            mshtml.HTMLDocumentClass ms = new mshtml.HTMLDocumentClass();
            string strHTML = Encoding.ASCII.GetString(data);
            mshtml.IHTMLDocument2 mydoc = (mshtml.IHTMLDocument2)ms;            
            //mydoc.write(!--saved from url=(0014)about:internet -->);
            mydoc.write(strHTML);

            mshtml.IHTMLElementCollection ec = (mshtml.IHTMLElementCollection)mydoc.all.tags("a");
            if (ec != null)
            {
                for (int i = 0; i < ec.length - 1; i++)
                {
                    mshtml.HTMLAnchorElementClass anchor;
                    anchor = (mshtml.HTMLAnchorElementClass)ec.item(i, 0);

                    if (!string.IsNullOrEmpty(anchor.name))
                    {
                        cmbAnchor.Items.Add(anchor.name);
                    }
                }
            }
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Loads the dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void HelpDialog_Load(object sender, EventArgs e)
        {
            btnSaveOnly.Visible = showSaveOnly;
            if (showSaveOnly)
            {
                RepositionButtons();
            }
        }

        /// <summary>
        /// Handles the Text Changed event of the filename textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtFilename_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the Click event of the Browse button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "HTML files (*.htm)|*.htm*|CHM Help Files (*.chm)|*.chm";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                this.txtFilename.Text = dlg.FileName;
                LoadAnchors(this.txtFilename.Text);
            }            
            dlg.Dispose();
        }
        
        #endregion Event Handlers
    }
}

