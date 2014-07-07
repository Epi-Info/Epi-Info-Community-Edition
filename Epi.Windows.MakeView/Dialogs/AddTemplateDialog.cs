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
	/// Dialog for setting background color and background image
	/// </summary>
    public partial class AddTemplateDialog : DialogBase
    {
        #region Private Variables
        private string templateName = string.Empty;
        private MakeViewMainForm mainForm;
        #endregion

        #region Public Variables
        public event EventHandler SettingsSaved;

        public string TemplateName
        {
            get
            {
                return templateName;
            }
            set
            {
                templateName = value;
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
		/// Default Constructor - Design mode only
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public AddTemplateDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public AddTemplateDialog(MainForm main)
            : base(main)
        {
            InitializeComponent();
            mainForm = main as MakeViewMainForm;
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
            if (ValidateTemplateName(mainForm.TemplateNode, textBoxTemplateName.Text))
            {
                DialogResult = DialogResult.OK;
                TemplateName = textBoxTemplateName.Text;
                this.Close();
            }
        }

        private bool ValidateTemplateName(string node, string name)
        {
            Configuration config = Configuration.GetNewInstance();
            string execPath = config.Directories.Templates + node;
            if (System.IO.Directory.Exists(execPath))
            {
                string[] templateNames = System.IO.Directory.GetFiles(execPath, "*.xml");

                foreach (string s in templateNames)
                {
                    if (s.Substring(execPath.Length + 1) == name + ".xml")
                    {
                        if (MessageBox.Show("The template with the name already exists.Please click OK to continue or Cancel to return.", "Template Name", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                            return true;
                        else
                            textBoxTemplateName.Focus();
                            return false;
                    }
                }
            }
            return true;
        }

        #endregion
    }
}

