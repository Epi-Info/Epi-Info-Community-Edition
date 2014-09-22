using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;
using Epi;
using Epi.Windows;
using Epi.Analysis;
using Epi.Windows.Analysis;
using Epi.Windows.Dialogs;

namespace Epi.Windows.Analysis.Dialogs
{

    public partial class PgmDialog : DialogBase
    {

        #region Private Attributes
        private DataTable programs;
        private string content = string.Empty;
        private Project currentProject;
        private string programName = string.Empty;
        private PgmDialogMode pgmDialogMode;
        private DataRow programRow;
        private Pgm currentPgm;
        private bool isProjectBased;
        private Epi.Core.AnalysisInterpreter.EpiInterpreterParser EpiInterpreter;
        #endregion Private Attributes

        #region Public Interface
        #region Constructors

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", false)]
        protected PgmDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// theContent will be empty for Attach
        /// </summary>
        public PgmDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm theParentForm, string name, string theContent, PgmDialogMode mode)
            : base(theParentForm)
        {
            //
            // Required for Windows Form Designer support
            // 
            InitializeComponent();
            this.content = theContent;
            this.pgmDialogMode = mode;

            this.EpiInterpreter = theParentForm.EpiInterpreter;

            Project project = this.EpiInterpreter.Context.CurrentProject;

            if (project != null)
            {
                this.currentProject = project;
            }

            if (mode == PgmDialogMode.SaveProgram)
            {
                cmbPrograms.DropDownStyle = ComboBoxStyle.DropDown;
                this.Text = "Save Program";         // form title
            }
            else if (mode == PgmDialogMode.SaveProgramAs)
            {
                cmbPrograms.DropDownStyle = ComboBoxStyle.DropDown;
                this.Text = "Save Program As";      // form title
            }
            else 
            {
                cmbPrograms.DropDownStyle = ComboBoxStyle.DropDownList;
                this.Text = "Open Program";         // form title
            }
            programName = name;
            this.isProjectBased = true;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Property that returns the program name
        /// </summary>
        public string ProgramName
        {
            get
            {
                if (isProjectBased)
                {
                    if (this.programRow == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return (this.programRow[ColumnNames.PGM_NAME].ToString());
                    }
                }
                else
                {
                    return programName;
                }
            }
        }

        /// <summary>
        /// Returns the content of the PGM
        /// </summary>
        public string Content
        {
            get
            {
                //DEFECT: 231 protect against Null Exception when programRow is null.
                if (isProjectBased)
                {
                    if (this.programRow == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return this.programRow[ColumnNames.PGM_CONTENT].ToString();
                    }
                }
                else
                {
                    return txtComment.Text;
                }
            }
        }
        #endregion Public Properties         


        #region Public Enumerations
        /// <summary>
        /// Enumeration used to instantiate PgmDialogs
        /// </summary>
        public enum PgmDialogMode
        {
            /// <summary>
            /// true when Open button was clicked in program editor
            /// </summary>
            OpenProgram,
            /// <summary>
            /// true when save button was clicked in program editor
            /// </summary>
            SaveProgram,
            /// <summary>
            /// true when save as button was clicked in program editor
            /// </summary>
            SaveProgramAs
        }
        #endregion Public Enumerations
        #endregion Public Interface

        #region Protected Interface
        #region Protected Methods
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon validation</returns>
        protected override bool ValidateInput()
        {
            bool isInputValid = true;
            if (this.pgmDialogMode == PgmDialogMode.OpenProgram)
            {
                if (string.IsNullOrEmpty(txtProject.Text))
                {
                    isInputValid = false;
                }

                /*
                if (currentPgm is ProjectBasedPgm)
                {
                    if (string.IsNullOrEmpty(cmbPrograms.Text))
                    {
                        isInputValid = false;
                    }
                }*/
                return (isInputValid);
            }
            return (isInputValid);
        }
        #endregion Protected Methods
        #endregion Protected Interface

        #region Private Methods
        private DataRow CreateProgramRow(Project proj)
        {
            DataTable tbl = proj.GetPgms();
            DataTable clone = tbl.Clone();
            DataRow row = clone.NewRow();
            clone.Dispose();
            tbl.Dispose();
            return row;
        }

        private void LoadPgms()
        {
            cmbPrograms.DataSource = programs;
            cmbPrograms.DisplayMember = ColumnNames.NAME;
            cmbPrograms.ValueMember = ColumnNames.PROGRAM_ID;
            cmbPrograms.Text = string.Empty;
            cmbPrograms.SelectedIndex = -1;
            this.cmbPrograms.SelectedIndexChanged += new System.EventHandler( this.cmbPrograms_SelectedIndexChanged );
        }

        private void Clear()
        {
            txtAuthor.Text = string.Empty;
            txtComment.Text = string.Empty;
            txtDateCreated.Text = string.Empty;
            txtDateUpdated.Text = string.Empty;
        }

        private void ToggleControls()
        {
            lblProject.Visible = isProjectBased;
            lblProgramFile.Visible = isProjectBased;
            cmbPrograms.Enabled = isProjectBased;
            lblComments.Visible = isProjectBased;
            lblContent.Visible = !isProjectBased;
            txtAuthor.Enabled = isProjectBased;
            lblProject.Visible = false;
            lblProject.Text = ((isProjectBased) ? SharedStrings.PROJECT_FILE : SharedStrings.PROGRAM_FILE);
            lblProject.Visible = true;
            btnOK.Enabled = (!isProjectBased || !string.IsNullOrEmpty(cmbPrograms.Text));
            lblProject.Refresh();
        }

        private void LoadProgramInfo()
        {
            txtAuthor.Text = programRow[ColumnNames.PGM_AUTHOR].ToString();
            txtComment.Text = programRow[ColumnNames.PGM_COMMENT].ToString();
            cmbPrograms.Text = programRow[ColumnNames.PGM_NAME].ToString();
            txtDateCreated.Text = ((DateTime)programRow[ColumnNames.PGM_CREATE_DATE]).ToShortDateString();
            txtDateUpdated.Text = ((DateTime)programRow[ColumnNames.PGM_MODIFY_DATE]).ToShortDateString();
            content = programRow[ColumnNames.PGM_CONTENT].ToString();
        }

        #endregion Private Methods

        #region Event Handlers
        private void SaveProgramDialog_Load(object sender, System.EventArgs e)
		{
			if (currentProject != null)
			{
                programs = currentProject.GetPgms();
                LoadPgms();
                if (programName != string.Empty)
                {
                    cmbPrograms.Text = programName;
                }
                txtProject.Text = currentProject.FilePath;
			}
            
            if (currentPgm != null)
            {
                LoadProgramInfo();
            }
            else if (this.pgmDialogMode == PgmDialogMode.SaveProgram)
            {
                txtDateUpdated.Text = DateTime.Now.ToString();
            }
            else if (this.pgmDialogMode == PgmDialogMode.SaveProgramAs)
            {
                txtDateCreated.Text = DateTime.Now.ToString();
                txtDateUpdated.Text = DateTime.Now.ToString();
                cmbPrograms.SelectedIndex = -1;
                cmbPrograms.Text = string.Empty;
            }
        }

		private void btnOK_Click(object sender, System.EventArgs e)
		{
            if (currentProject != null)
            {
                if (!string.IsNullOrEmpty(content.Trim()))
                {
                    this.Hide();

                    if (isProjectBased)
                    {
                        currentPgm = new ProjectBasedPgm(currentProject, cmbPrograms.Text);
                    }
                    else
                    {
                        currentPgm = new FileBasedPgm(currentProject, this.txtProject.Text);
                    }

                    if (this.pgmDialogMode == PgmDialogMode.SaveProgram || this.pgmDialogMode == PgmDialogMode.SaveProgramAs)
                    {
                        this.cmbPrograms.SelectedIndexChanged -= new System.EventHandler(this.cmbPrograms_SelectedIndexChanged);
                        string today = DateTime.Now.ToShortDateString();

                        if (programRow == null)
                        {
                            programRow = this.CreateProgramRow(currentProject);
                        }
                        programRow[ColumnNames.PGM_CONTENT] = content;
                        programRow[ColumnNames.PGM_AUTHOR] = txtAuthor.Text.ToString();
                        programRow[ColumnNames.PGM_COMMENT] = txtComment.Text.ToString();
                        programRow[ColumnNames.PGM_NAME] = cmbPrograms.Text.ToString();
                        string createDate = txtDateCreated.Text.ToString();
                        DateTime tryDateTime;
                        if (DateTime.TryParse(createDate, out tryDateTime) == false)
                        {
                            createDate = DateTime.Now.ToString();
                        }
                        programRow[ColumnNames.PGM_CREATE_DATE] = createDate;
                        programRow[ColumnNames.PGM_MODIFY_DATE] = DateTime.Now.ToString();
                        cmbPrograms.Enabled = true;
                        currentPgm.PgmSave(programRow);
                        this.cmbPrograms.Refresh();
                        this.cmbPrograms.SelectedIndexChanged += new System.EventHandler(this.cmbPrograms_SelectedIndexChanged);
                    }
                    else
                    {
                        if (programRow == null)
                        {
                            //programRow = currentPgm.PgmLoad(this.txtProject.Text);
                        }

                    }
                }
                else
                {
                    // DEFECT: 231
                    if (this.pgmDialogMode == PgmDialogMode.SaveProgram || this.pgmDialogMode == PgmDialogMode.SaveProgramAs)
                    {
                        MsgBox.ShowWarning(SharedStrings.WARNING_CANNOT_SAVE_BLANK_PGM);
                    }
                    else
                    {
                        MsgBox.ShowWarning(SharedStrings.WARNING_CANNOT_OPEN_PROGRAM);
                    }
                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtComment.Text))
                {
                    DialogResult = DialogResult.None;
                }
                else
                {
                    if (this.pgmDialogMode == PgmDialogMode.SaveProgram || this.pgmDialogMode == PgmDialogMode.SaveProgramAs)
                    {
                        Epi.Analysis.FileBasedPgm.PgmSave(this.ProgramName.ToString(), txtComment.Text.ToString());
                    }
                }    
                    
               if (!isProjectBased)
                {
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    DialogResult = DialogResult.None;
                }
            }

		}

       private void btnTextFile_Click( object sender, System.EventArgs e )
        {
            isProjectBased = true;
            DialogResult result;
            if (this.pgmDialogMode == PgmDialogMode.OpenProgram)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Program Files (*.pgm7) |*.pgm7";
                openFileDialog.CheckPathExists = true;
                openFileDialog.CheckFileExists = true;
                result = openFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (File.Exists(openFileDialog.FileName))
                    {
                        isProjectBased = false;
                        txtProject.Text = openFileDialog.FileName;
                        txtDateCreated.Text = File.GetCreationTime(openFileDialog.FileName).ToShortDateString();
                        txtDateUpdated.Text = File.GetLastWriteTime(openFileDialog.FileName).ToShortDateString();
                        content = File.ReadAllText(openFileDialog.FileName);
                        programName = txtProject.Text;
                        txtComment.Text = content;
                        ToggleControls();
                    }
                }
            }
            else
            {
                txtComment.Text = content;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Program Files (*.pgm7) |*.pgm7";
                saveFileDialog.CheckPathExists = true;
                saveFileDialog.OverwritePrompt = true;
                result = saveFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtProject.Text = saveFileDialog.FileName;
                    programName = saveFileDialog.FileName;
                    isProjectBased = false;
                    ToggleControls();
                }
            }
        }
        private void btnFindProject_Click(object sender, System.EventArgs e)
        {
            //isu6 - does not seem to be used except in section that was causing Defect #794.
            //Project oldProject = currentProject;
            Project newProject = null;
            this.cmbPrograms.SelectedIndexChanged -= new System.EventHandler(this.cmbPrograms_SelectedIndexChanged);
            newProject = SelectProject();
            if (newProject != null)
            {
                //isu6 - Defect #794 - code was setting the values of the project loaded in memory to null.
                //if (oldProject != null)
                //{
                //    oldProject.Dispose();
                //}

                
                this.mainForm.CurrentProject = new Project(newProject.FilePath);
                currentProject = this.mainForm.CurrentProject;
                txtProject.Text = currentProject.FilePath;
                programs = currentProject.GetPgms();
                LoadPgms();
                Clear();
            }
        }


		private void cmbPrograms_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            if (cmbPrograms.SelectedIndex != -1)
			{
                programRow = programs.Select("ProgramId = " + (int.Parse(cmbPrograms.SelectedValue.ToString())).ToString())[0];

				txtAuthor.Text = programRow[ColumnNames.PGM_AUTHOR].ToString();
				txtComment.Text = programRow[ColumnNames.PGM_COMMENT].ToString();
				txtDateCreated.Text = programRow[ColumnNames.PGM_CREATE_DATE].ToString();
				txtDateUpdated.Text = programRow[ColumnNames.PGM_MODIFY_DATE].ToString();
                if (this.pgmDialogMode != PgmDialogMode.SaveProgram && this.pgmDialogMode != PgmDialogMode.SaveProgramAs)
                {
                    content = programRow[ColumnNames.PGM_CONTENT].ToString();
                }
                programName = cmbPrograms.SelectedText;
			}
			
		}
		private void txtComment_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (this.pgmDialogMode == PgmDialogMode.OpenProgram)
			{
				//User cannot edit contents of comments text box in open mode. 
				//txtComments is not disabled to allow users to scroll
				e.Handled = true;
			}
			else
			{
				e.Handled = false;
			}
		}
		private void btnDelete_Click(object sender, System.EventArgs e)
		{
            string nameOrPath;
            Pgm pgm;

            if (isProjectBased)
            {
                pgm = new ProjectBasedPgm(currentProject, currentProject.FilePath);
                nameOrPath = cmbPrograms.Text;
            }
            else
            {
                pgm = new FileBasedPgm(currentProject, currentProject.FilePath);
                nameOrPath = txtProject.Text;
            }
            
            if (nameOrPath == string.Empty)
            {
                MsgBox.ShowError(SharedStrings.SELECT_PROGRAM);
            }
            else
            {
                DialogResult result = MessageBox.Show(SharedStrings.DELETE_PROGRAM, SharedStrings.CONFIRM_PROGRAM_DELETE, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.pgmDialogMode = PgmDialogMode.OpenProgram;
                    pgm.PgmDelete(nameOrPath);
				    Clear();
				    txtProject.Text = currentProject.FilePath;
					programs = currentProject.GetPgms();
                    if (programs.Rows.Count > 0)
                    {
                        LoadPgms();
                    }
                    else
                    {
                        this.programRow = null;
                    }
                    ToggleControls();
				}
			}
            DialogResult = DialogResult.OK;
		}

        private void cmbPrograms_SelectedValueChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (!string.IsNullOrEmpty(cmbPrograms.Text.Trim()));
            btnDelete.Enabled = (!string.IsNullOrEmpty(cmbPrograms.Text.Trim()));
        }

        private void cmbPrograms_KeyUp(object sender, KeyEventArgs e)
        {
            cmbPrograms.SelectionStart = cmbPrograms.Text.Length;
            btnOK.Enabled = (!string.IsNullOrEmpty(cmbPrograms.Text.Trim())); 
            btnDelete.Enabled = (!string.IsNullOrEmpty(cmbPrograms.Text.Trim()));
        }

        private void cmbPrograms_Leave(object sender, EventArgs e)
        {
            cmbPrograms.Text = cmbPrograms.Text.Trim();
        }

        #endregion Event Handlers

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (currentProject != null)
            {
                programs = currentProject.GetPgms();
                LoadPgms();
                if (programName != string.Empty)
                {
                    cmbPrograms.Text = programName;
                }
                txtProject.Text = currentProject.FilePath;
            }
            cmbPrograms.SelectedIndex = -1;
            txtAuthor.Text = string.Empty;
            txtComment.Text = string.Empty;
            txtProject.Focus();
        }
	}
}