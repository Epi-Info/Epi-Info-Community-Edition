using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Epi.Windows.Dialogs;
using Epi.Windows.MakeView.Forms;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// </summary>
    public partial class RenameTemplate : DialogBase
    {
        #region Private Variables
        private string _templateName = string.Empty;
        private string _copiedFolderName = string.Empty;
        private string _copiedExtension = string.Empty;

        private MakeViewMainForm mainForm;
        #endregion

        #region Public Variables
        public event EventHandler SettingsSaved;

        public string TemplateName
        {
            get
            {
                return _templateName;
            }
            set
            {
                _templateName = value;
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
		/// Default Constructor - Design mode only
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public RenameTemplate()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public RenameTemplate(MainForm main, string copiedFolderName, string copiedExtension)
            : base(main)
        {
            InitializeComponent();
            mainForm = main as MakeViewMainForm;

            _copiedFolderName = copiedFolderName;
            _copiedExtension = copiedExtension;

            InitSettings();
        }

        #endregion

        #region Private Methods

        private void InitSettings()
        {
        }
	
		/// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            TemplateName = textBoxTemplateName.Text;
            this.Close();
        }

        #endregion

        private void textBoxTemplateName_TextChanged(object sender, EventArgs e)
        {
            string fullPathCanditate = System.IO.Path.Combine(_copiedFolderName, this.textBoxTemplateName.Text) + _copiedExtension;
            this.btnOK.Enabled = System.IO.File.Exists(fullPathCanditate) ? false : true;
        }
    }
}

