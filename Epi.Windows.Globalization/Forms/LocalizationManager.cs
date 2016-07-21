using System.Globalization;
using System.Collections;
using System.IO;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Epi.Windows.Globalization.Translators;

namespace Epi.Windows.Globalization.Forms
{
    /// <summary>
    /// class LocalizationManager
    /// </summary>
    public partial class LocalizationManager : Form
    {
        /// <summary>
        /// Default constructor for LocalizationManager
        /// </summary>
        public LocalizationManager()
        {
            InitializeComponent();
        }

        #region Implementation
        private delegate void VoidDelegate();


        private void AppendToLog(string format, params object[] args)
        {
            this.AppendToLog(string.Format(format, args));
        }
        private void AppendToLog(string message)
        {

            if (this.statusStrip.InvokeRequired)
            {
                AppendLogCallback del = new AppendLogCallback(this.AppendToLog);
                this.statusStrip.Invoke(del, message);
            }
            else
            {
                this.toolStripStatusLabel.Text = message;
                Application.DoEvents();
            }
        }

        private void DeleteSelectedCulture()
        {
            string path = (this.lstInstalledLanguages.SelectedItem as DropDownListItem).Value as string;

            // if null then default language
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("The default language cannot be uninstalled.", "Unable to perform", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (DialogResult.OK == MessageBox.Show(SharedStrings.CONFIRM_LANGUAGE_DELETION, SharedStrings.WARNING, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation))
            {
                Directory.Delete(path, true);
                this.AppendToLog(SharedStrings.LANGUAGE_DELETION_CONFIRM, path);
            }

            this.LoadInstalledLanguages(this.folderBrowserDialog.SelectedPath);
        }
        
        private void DisableForm()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate del = new VoidDelegate(this.DisableForm);
                this.Invoke(del);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                this.panelMain.Enabled = false;
            }
        }

        private void EnableForm()
        {
            if (this.InvokeRequired)
            {
                VoidDelegate del = new VoidDelegate(this.EnableForm);
                this.Invoke(del);
            }
            else
            {
                this.Cursor = Cursors.Default;
                this.panelMain.Enabled = true;
                this.LoadInstalledLanguages(this.folderBrowserDialog.SelectedPath);
            }
        }
     
        private void SetupForm()
        {
            this.folderBrowserDialog.SelectedPath = Path.GetDirectoryName(Application.ExecutablePath);
            this.LoadInstalledLanguages(this.folderBrowserDialog.SelectedPath);
            
        }

        private void RefreshForm()
        {

          
            if (lstInstalledLanguages.SelectedIndices.Count > 0)
            {
                this.btnExport.Enabled = true;
                this.btnRemove.Enabled = true;
                this.groupBoxExport.Enabled = true;

                string sourceCulture = ((DropDownListItem)this.lstInstalledLanguages.SelectedItem).Key;
                BindTranslationCultures(sourceCulture);

                rbCreateAutoTranslate.Enabled  = ddlCultures.Items.Count > 0;
            }
            else
            {
                this.btnExport.Enabled = false;
                this.btnRemove.Enabled = false;
                this.groupBoxExport.Enabled = false;
            }

            if (rbCreateAutoTranslate.Checked && rbCreateAutoTranslate.Enabled == false)
            {
                rbCreateCopy.Checked = true;
            }

            if (rbCreateAutoTranslate.Checked)
            {
                this.ddlCultures.Enabled = true;
            }
            else
            {
                this.ddlCultures.Enabled = false;
            }

            if (rbCreateLegacyEpiDatabase.Checked)
            {
                this.txtLanguageDatabase.Enabled = true;
            }
            else
            {
                this.txtLanguageDatabase.Enabled = false;
            }


            if (chkExportOptions.Checked)
            {
                this.Size = new Size(622, 593);
                this.groupBoxExport.Visible = true;
            }
            else
            {
                this.Size = new Size(622, 384);
                this.groupBoxExport.Visible = false;
            }
        }

        private void LoadInstalledLanguages(string installPath)
        {
            // TODO: installed languages must be read from configuration

            string searchPath = installPath.Trim().ToLowerInvariant();

            if (searchPath[searchPath.Length - 1] != Path.DirectorySeparatorChar)
                searchPath += Path.DirectorySeparatorChar;

            if (Directory.Exists(searchPath))
            {
                this.AppendToLog(SharedStrings.SEARCHING_LANGUAGES, searchPath);

                List<string> files = new List<string>();
                string[] subdirs = Directory.GetDirectories(searchPath);
                foreach (string dir in subdirs)
                {
                    files.AddRange(Directory.GetFiles(dir, "*.Resources.dll", SearchOption.TopDirectoryOnly));
                }


                List<string> index = new List<string>();
                foreach (string file in files)
                {
                    string path = Path.GetDirectoryName(file).ToLowerInvariant();
                    if (!index.Contains(path))
                    {
                        index.Add(path);
                    }
                }

                index.Sort();

                this.lstInstalledLanguages.Items.Clear();

                foreach (string key in index)
                {
                    if (key == searchPath) continue;

                    string cultureName = key.Substring(key.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    
                    CultureInfo ci;
                    try
                    {
                         ci = new CultureInfo(cultureName);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                    int i = this.lstInstalledLanguages.Items.Add(new DropDownListItem(cultureName, ci.EnglishName, key));

                    this.AppendToLog(SharedStrings.FOUND_LANGUAGE, cultureName);
                }


                

                this.lstInstalledLanguages.Items.Insert(0,
                    new DropDownListItem(
                    "",
                    "English (default)",
                    null)
                );

                this.AppendToLog("Ready");

                 this.lstInstalledLanguages.SelectedIndex = 0;
            }
            else
            { 
              MessageBox.Show(SharedStrings.INVALID_BUILD_FOLDER);
            }
            RefreshForm();
        }

      
     
        private void ImportLanguage()
        {
            Import.ShowImportDialog(this.folderBrowserDialog.SelectedPath);
            this.LoadInstalledLanguages(this.folderBrowserDialog.SelectedPath);
        
        }

        private void ExportSelectedLanguage()
        {
            if (exportSaveFileDialog.ShowDialog() != DialogResult.OK) return;

            string exportFilePath = exportSaveFileDialog.FileName;

            string sourceCulture = ((DropDownListItem)this.lstInstalledLanguages.SelectedItem).Key;

            string targetCulture = null;

            if (rbCreateAutoTranslate.Checked)
            {
                targetCulture = (this.ddlCultures.SelectedItem as DropDownListItem).Key.ToString();
            }


            ExtractionMode mode = 
                this.rbCreateAutoTranslate.Checked ? ExtractionMode.WebTranslation
                : this.rbCreateReverse.Checked ? ExtractionMode.Reverse
                : this.rbCreateCopy.Checked ? ExtractionMode.Copy
                : this.rbCreateLegacyEpiDatabase.Checked ? ExtractionMode.DatabaseTranslation
                : this.rbCreateExpansion.Checked ? ExtractionMode.Double
                : mode = ExtractionMode.None;

            string installPath = this.folderBrowserDialog.SelectedPath;

            string languageDbPath = this.txtLanguageDatabase.Text;

            System.Threading.WaitCallback callback = new System.Threading.WaitCallback(this.QueueExctractionWorkerThread);

            object[] args = new object[6];
            args[0] = installPath;
            args[1] = sourceCulture;
            args[2] = targetCulture;
            args[3] = mode;
            args[4] = exportFilePath;
            args[5] = languageDbPath;

            System.Threading.ThreadPool.QueueUserWorkItem(callback, (object)args);
        }

        private void Debug()
        {
            if (this.lstInstalledLanguages.SelectedItem != null)
            {
                string sourceCulture = ((DropDownListItem)this.lstInstalledLanguages.SelectedItem).Key;
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(sourceCulture);

                IModuleManager manager = GetService(typeof(IModuleManager)) as IModuleManager;
                if (manager != null)
                {
                    IModule module = manager.CreateModuleInstance(typeof(LocalizationWindowsModule).AssemblyQualifiedName);
                    module.Load(manager, null);
                }
            }
        }

        private void QueueExctractionWorkerThread(object state)
        {
            object[] args = state as object[];
            string installPath = (string)args[0];
            string sourceCulture = (string)args[1];
            string targetCulture = (string)args[2] ?? ResourceTool.DEFAULT_CULTURE;
            ExtractionMode mode = (ExtractionMode)args[3];
            string exportFilePath = (string)args[4];
            string languageDbPath = (string)args[5];

            try
            {
                this.DisableForm();

                ITranslator translator;
                switch (mode)
                { 
                    case ExtractionMode.DatabaseTranslation:
                        translator = new LegacyLanguageDbTranslator();
                        ((LegacyLanguageDbTranslator)translator).ReadDatabase(languageDbPath);
                        break;
                    case ExtractionMode.Double:
                        translator = new ExpandedStringTranslator();
                        break;
                    case ExtractionMode.Reverse:
                        translator = new ReverseStringTranslator();
                        break;
                    case ExtractionMode.WebTranslation:
                        translator = new WebServiceTranslator();
                        break;
                    default:
                        translator = new NormalizedStringTranslator();
                        break;
                }
                ResourceTool.ExportLanguage(installPath, sourceCulture, targetCulture, translator, exportFilePath, new AppendLogCallback(this.AppendToLog));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                this.EnableForm();
            }


        }

        private void BindTranslationCultures(string sourceCultureName)
        {
            Dictionary<string, CultureInfo> cultureDictionary = new Dictionary<string, CultureInfo>();
            List<string> cultureList = new List<string>();

            List<string> supportedLanguages = WebServiceTranslator.GetSupportedTargetLanguages(sourceCultureName);

            // sort the list by english name

            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (ci.Name == "") continue;
                if (supportedLanguages.Contains(ci.Name))
                {
                    cultureDictionary.Add(ci.EnglishName, ci);
                    cultureList.Add(ci.EnglishName);
                }
            }

            cultureList.Sort();

            // add sorted list to combobox 

            ddlCultures.Items.Clear();

            foreach (string cultureEnglishName in cultureList)
            {
                CultureInfo ci = cultureDictionary[cultureEnglishName];
                int i = ddlCultures.Items.Add(new DropDownListItem(ci.Name, cultureEnglishName, ci.NativeName));
            }

            if (ddlCultures.Items.Count > 0)
            {
                this.ddlCultures.SelectedIndex = 0;
            }

        }

        #endregion

        #region Event handlers

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedCulture();
        }


        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportSelectedLanguage();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportLanguage();
        }

        private void chkExportOptions_CheckedChanged(object sender, EventArgs e)
        {
            RefreshForm();
        }
       
        private void lstInstalledLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            RefreshForm();
        }

        private void LocalizationManager_Load(object sender, EventArgs e)
        {
            SetupForm();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ImportLanguage();
        }

        private void rbCreateAutoTranslate_CheckedChanged(object sender, EventArgs e)
        {
            RefreshForm();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            Debug();
        }

        #endregion

        private void btnBrowseEpiDatabases_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Epi Info 3.x language database (*.mdb)|*.mdb";
            dialog.Multiselect = false;
            if (DialogResult.OK == dialog.ShowDialog())
            {
                this.txtLanguageDatabase.Text = dialog.FileName;
            }
        }

        private void rbCreateLegacyEpiDatabase_CheckedChanged(object sender, EventArgs e)
        {
            RefreshForm();
        }


    }
  
    /// <summary>
    /// class DropDownListItem
    /// </summary>
    public class DropDownListItem
    {
        #region Implementation
        private string key;

        /// <summary>
        /// Return the Key
        /// </summary>
        public string Key
        {
            get { return key; }
            set { key = value; }
        }
        
        private object value;

        /// <summary>
        /// Return the Value
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        private string text;

        /// <summary>
        /// Text Property
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// DropDownListItem constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        /// <param name="value"></param>
        public DropDownListItem(string key, string text, object value)
        {
            this.key = key;
            this.value = value;
            this.text = text;
        }

        /// <summary>
        /// Returns string representation of this
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return text.ToString();
        }
        #endregion
    }


}