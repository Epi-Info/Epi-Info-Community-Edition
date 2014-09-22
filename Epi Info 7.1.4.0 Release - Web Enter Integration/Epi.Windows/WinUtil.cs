using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Design;
using System.Drawing;
using System.Drawing.Text;

namespace Epi.Windows
{
    /// <summary>
    /// Windows specific Utility methods
    /// </summary>
    public static class WinUtil
    {
        #region Private Data Members

        private static List<string> installedFontFamilies = new List<string>();

        #endregion //Private Data Members

        #region Public Properties

        /// <summary>
        /// Returns added font families
        /// </summary>
        public static List<string> InstalledFontFamilies
        {
            get
            {
                if (installedFontFamilies.Count == 0)
                {
                    InstalledFontCollection fonts = new InstalledFontCollection();
                    try
                    {
                        foreach (FontFamily font in fonts.Families)
                        {
                            installedFontFamilies.Add(font.Name);
                        }
                    }
                    finally
                    {
                        fonts.Dispose();
                        fonts = null;
                    }
                }
                return installedFontFamilies;
            }
        }

        #endregion  //Public Properties

        #region Public Methods

        /// <summary>
        /// Method for returning the selected radiobutton in groupbox
        /// </summary>
        /// <param name="gbx">A group box</param>
        /// <returns>The selected radio button</returns>
        public static RadioButton GetSelectedRadioButton(GroupBox gbx)
        {
            #region Input Validation
            if (gbx == null)
            {
                throw new ArgumentNullException("gbx");
            }
            #endregion Input Validation


            foreach (RadioButton rb in gbx.Controls)
            {
                if (rb.Checked)
                {
                    return rb;
                }
            }
            throw new ApplicationException(SharedStrings.NO_RADIO_BUTTON_SELECTED);
        }

        /// <summary>
        ///Sets Checked property of  radio buttons contained in a group box
        /// </summary>
        /// <param name="radiobuttonTag"></param>
        /// <param name="gbx"></param>
        public static void SetSelectedRadioButton(string radiobuttonTag, GroupBox gbx)
        {
            foreach (RadioButton rb in gbx.Controls)
            {
                if (rb.Tag != null)
                {
                    if (string.Compare((string)rb.Tag, radiobuttonTag, true) == 0)
                    {
                        rb.Checked = true;
                        return;
                    }
                }
            }
            // throw new GeneralException(StringsEx.NO_RADIO_BUTTON_SELECTED, gbx.Text);
        }

        /// <summary>
        /// Loads form
        /// </summary>
        /// <param name="assemblyFile">The assembly file</param>
        /// <param name="typeName">The type name</param>
        /// <param name="parameters">Command line parameters</param>
        /// <returns>The form to be loaded</returns>
        public static Form LoadForm(string assemblyFile, string typeName, object[] parameters)
        {
            return (Form)Util.LoadObject(assemblyFile, typeName, parameters);
        }

        /// <summary>
        /// Loads the form
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <param name="typeName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static void ShowForm(string assemblyFile, string typeName, object[] parameters)
        {
            Form form = LoadForm(assemblyFile, typeName, parameters);
            form.Show();
        }

        public static void FillMetadataDriversList(ComboBox cb)
        {
            Configuration config = Configuration.GetNewInstance();
            DataView dv = config.DataDrivers.DefaultView;
            dv.RowFilter = "MetadataProvider = true";
            dv.Sort = "DisplayName";
            cb.DataSource = dv;
            cb.DisplayMember = "DisplayName";
            cb.ValueMember = "Type";
        }

        public static void FillDataDriversList(ComboBox cb)
        {
            Configuration config = Configuration.GetNewInstance();
            DataView dv = config.DataDrivers.DefaultView;
            dv.Sort = "DisplayName";
            cb.DataSource = dv;
            cb.DisplayMember = "DisplayName";
            cb.ValueMember = "Type";
        }

        ///// <summary>
        ///// Fills the combo box with all data formats supported by READ command
        ///// </summary>
        ///// <param name="cb"></param>
        //public static void FillDataFormatsForRead(ComboBox cb)
        //{
        //    DataView dv = AppData.Instance.DataFormatsDataTable.DefaultView;
        //    dv.Sort = ColumnNames.NAME;
        //    cb.DataSource = dv;
        //    cb.DisplayMember = ColumnNames.NAME;
        //    cb.ValueMember = ColumnNames.ID;
        //    cb.SelectedValue = Configuration.Current.Settings.DefaultDataFormatForRead;
        //}

        /// <summary>
        /// Dispalys a Confirmation box and gets results.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool Confirm(string msg)
        {
            DialogResult result = MessageBox.Show(msg, SharedStrings.CONFIRMATION, MessageBoxButtons.YesNo);
            return (result == DialogResult.OK || result == DialogResult.Yes);
        }

        /// <summary>
        /// Opens a text file in Wordpad
        /// </summary>
        /// <param name="filePath"></param>
        public static void OpenTextFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);   
			ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("WordPad.exe");
            startInfo.WorkingDirectory = fileInfo.Directory.FullName;
            startInfo.Arguments = Util.InsertInDoubleQuotes(fileInfo.Name);
            startInfo.Verb = "Open";
			startInfo.WindowStyle = ProcessWindowStyle.Maximized;
			System.Diagnostics.Process.Start(startInfo);
        }

        /// <summary>
        /// Asks the user to provide a new file path when the file specified is not found.
        /// </summary>
        /// <param name="brokenFilePath"></param>
        /// <param name="newFilePath"></param>
        /// <returns>new fixed file path</returns>
        public static bool FixBrokenFilePath(string brokenFilePath, out string newFilePath)
        {
            newFilePath = string.Empty;
            
            if (File.Exists(brokenFilePath))
            {
                return false;
            }
            else
            {
                string msg = SharedStrings.FILE_NOT_FOUND + " : " + brokenFilePath + Environment.NewLine + Environment.NewLine;
                msg += SharedStrings.BROWSE_FOR_FILE;
                DialogResult dialogResult = MsgBox.ShowQuestion(msg, MessageBoxButtons.YesNo);
                
                if (dialogResult != DialogResult.Yes)
                {
                    return false;
                }
                
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;

                if (brokenFilePath.Contains("."))
                {
                    string fileExtension = "*" + brokenFilePath.Substring(brokenFilePath.LastIndexOf('.'));
                    openFileDialog.Filter = fileExtension + " | " + fileExtension;
                }
                
                dialogResult = openFileDialog.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    newFilePath = openFileDialog.FileName;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static int GetPromptTop(Epi.Fields.FieldWithSeparatePrompt field, int height)
        {
            return (int)Math.Ceiling(field.PromptTopPositionPercentage * height);
        }
        public static int GetPromptLeft(Epi.Fields.FieldWithSeparatePrompt field, int width)
        {
            return (int)Math.Ceiling(field.PromptLeftPositionPercentage * width);
        }
        public static int GetControlHeight(Epi.Fields.RenderableField field, int height)
        {
            return (int)Math.Ceiling(field.ControlHeightPercentage * height);
        }
        public static int GetControlWidth(Epi.Fields.RenderableField field, int width)
        {
            return (int)Math.Ceiling(field.ControlWidthPercentage * width);
        }
        public static int GetControlLeft(Epi.Fields.RenderableField field, int width)
        {
            return (int)Math.Ceiling(field.ControlLeftPositionPercentage * width);
        }
        public static int GetControlTop(Epi.Fields.RenderableField field, int height)
        {
            return (int)Math.Ceiling(field.ControlTopPositionPercentage * height);
        }

        #endregion  Public Methods
    }
}