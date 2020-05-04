using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using Epi;
using Epi.Resources;
using Epi.Windows;

using Epi.Windows.Dialogs;
using Epi.Windows.Menu.Dialogs;

namespace Epi.Windows.Menu
{
    /// <summary>
    /// Main form for Epi Menu
    /// </summary>
    public partial class MenuMainForm : MainForm
    {
        #region Private Attributes
        private Array menuData = null;
        #endregion Private Attributes

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public MenuMainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mod"></param>
        public MenuMainForm(MenuWindowsModule mod) : base(mod)
        {
            InitializeComponent();
            Construct();
        }

        ///// <summary>
        ///// Constructor - loads a file
        ///// </summary>
        ///// <param name="mod"></param>
        ///// <param name="file"></param>
        //public MenuMainForm(MenuWindowsModule mod, string file) : base(mod)
        //{
        //    InitializeComponent();
        //    Construct();
        //    menuFile = file;
        //}

        private void Construct()
        {
            this.Text = "Epi Info™ 7 - Menu";// SharedStrings.MENU;
        }

        private void ChangeAllButtonStyles(bool useDefault)
        {
            ChangeButtonStyle(btnMakeView, useDefault);
            ChangeButtonStyle(btnWebsite, useDefault);
            ChangeButtonStyle(btnEnterData, useDefault);
            ChangeButtonStyle(btnExit, useDefault);
            ChangeButtonStyle(btnWebsite, useDefault);
            ChangeButtonStyle(btnCreateReports, useDefault);
            ChangeButtonStyle(btnCreateMaps, useDefault);
            ChangeButtonStyle(btnAnalyze, useDefault);
            ChangeButtonStyle(btnDashboard, useDefault);
        }

        private void ChangeButtonStyle(Button b, bool useDefault)
        {
            if (useDefault)
            {
                b.ForeColor = SystemColors.ControlText;
                b.BackColor = SystemColors.Control;
                b.FlatStyle = FlatStyle.Standard;
            }
            else
            {
                b.ForeColor = Color.FromArgb(0, 18, 100);
                b.BackColor = Color.White;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderColor = Color.White;
                b.FlatAppearance.MouseDownBackColor = Color.White;
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(206, 228, 241);
            }
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Redefined Module property
        /// </summary>
        public new MenuWindowsModule Module
        {
            get
            {
                return (MenuWindowsModule) base.Module;
            }
        }

        /// <summary>
        /// Main menu strip
        /// </summary>
        public MenuStrip MenuStrip
        {
            get
            {
                return this.mnuMainMenu;
            }
        }

        #endregion Public Properties        

        #region Event Handlers

        private void MenuMainForm_Load(object sender, System.EventArgs e)
        {
            string retValue = String.Empty;
            //HideButtons();
            DisplayBackgroundImage();
            // ParseMnuFile(menuFile);
            //if (String.IsNullOrEmpty(retValue) == true)
            //{
            //    MenuDocument currentMenu = new MenuDocument(menuFile);
            //    menuData = currentMenu.MenuItems;
            //    CreateMenuItem(new ToolStripMenuItem());
            //    //CreateButtons(currentMenu.Buttons);
            //    //menuButtons = new MenuButtons();
            //    BuildScreen();
            //    //this.btnCreateReports.Enabled = false;
            //    //this.btnCreateMaps.Enabled = false;
            //}
            this.Show();
            CheckForUpdates();
            CheckForAlerts();
        }

        private void CheckForUpdates()
        {
            BackgroundWorker updateChecker = new BackgroundWorker();
            updateChecker.DoWork += new DoWorkEventHandler(updateChecker_DoWork);
            updateChecker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(updateChecker_Completed);
            updateChecker.RunWorkerAsync();
        }

        private void CheckForAlerts()
        {
            BackgroundWorker alertChecker = new BackgroundWorker();
            alertChecker.DoWork += new DoWorkEventHandler(alertChecker_DoWork);
            alertChecker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(alertChecker_Completed);
            alertChecker.RunWorkerAsync();
        }

        void alertChecker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                string message = (string)e.Result;
                if (!string.IsNullOrEmpty(message))
                {
                    MessageBox.Show(message, "Alert Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Configuration config = Configuration.GetNewInstance();
                    config.Settings.LastAlertDate = DateTime.Now;
                    Configuration.Save(config);
                }
            }
        }

        void updateChecker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<System.Xml.XmlElement> elements = (List<System.Xml.XmlElement>)e.Result;
                if (elements.Count > 0)
                {
                    UpdateDetailDialog updateDetails = new UpdateDetailDialog(elements);
                    updateDetails.ShowDialog();
                }
            }
        }

        void alertChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ApplicationIdentity appId = new ApplicationIdentity(typeof(Configuration).Assembly);
                Version thisVersion = appId.VersionObject;

                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load("http://ftp.cdc.gov/pub/software/epi_info/alert.xml");
                List<System.Xml.XmlElement> elements = new List<System.Xml.XmlElement>();
                bool doAlert = false;
                string message = string.Empty;
                foreach (System.Xml.XmlElement element in doc.DocumentElement.ChildNodes)
                {
                    if (element.Name.Equals("alertDateTime"))
                    {
                        DateTime alertDateTime = DateTime.Parse(element.InnerText, null, System.Globalization.DateTimeStyles.AssumeUniversal);
                        if (Configuration.GetNewInstance().Settings.LastAlertDate < alertDateTime)
                        {
                            doAlert = true; 
                        }
                    }
                    if (element.Name.Equals("message"))
                    {
                        message = element.InnerText;
                    }
                }
                if (doAlert)
                {
                    e.Result = message;
                }
            }
            catch (Exception ex)
            {
                //
            }
        }

        void updateChecker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Configuration config = Configuration.GetNewInstance();
                if (config.Settings.CheckForUpdates == true)
                {

                    ApplicationIdentity appId = new ApplicationIdentity(typeof(Configuration).Assembly);
                    Version thisVersion = appId.VersionObject;

                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load("http://ftp.cdc.gov/pub/software/epi_info/versions.xml");
                    List<System.Xml.XmlElement> elements = new List<System.Xml.XmlElement>();
                    foreach (System.Xml.XmlElement element in doc.DocumentElement.ChildNodes)
                    {
                        if (element.Name.Equals("version"))
                        {
                            string versionNumber = element.Attributes["number"].Value;
                            string[] splits = versionNumber.Split('.');
                            int majorVersion = int.Parse(splits[0]);
                            int minorVersion = int.Parse(splits[1]);
                            int build = int.Parse(splits[2]);
                            int revision = int.Parse(splits[3]);

                            if (thisVersion.Major < majorVersion)
                            {
                                elements.Add(element);
                            }
                            else if (thisVersion.Minor < minorVersion)
                            {
                                elements.Add(element);
                            }
                            else if (thisVersion.Build < build)
                            {
                                elements.Add(element);
                            }
                            else if (thisVersion.Revision < revision)
                            {
                                elements.Add(element);
                            }
                        }
                    }
                    e.Result = elements;
                }
            }
            catch (Exception ex)
            {
                //
            }            
        }

        private void MakeView_Activate(object sender, System.EventArgs e)
        {
            LoadModule("MAKEVIEW");
        }
        private void EnterData_Activate(object sender, System.EventArgs e)
        {
            LoadModule("ENTER");
        }
        private void AnalyzeData_Activate(object sender, System.EventArgs e)
        {
            LoadModule("ANALYSIS");
        }
        private void CreateMaps_Activate(object sender, System.EventArgs e)
        {
            LoadModule("EPIMAP");
        }
        private void CreateReports_Activate(object sender, System.EventArgs e)
        {
            LoadModule("EPIREPORT");
        }
        private void mnuButtons_Activate(object sender, System.EventArgs e)
        {
            //btnAnalyze.Visible = mnuButtons.Checked;
            //btnCreateMaps.Visible = mnuButtons.Checked;
            //btnCreateReports.Visible = mnuButtons.Checked;
            //btnEnterData.Visible = mnuButtons.Checked;
            //btnExit.Visible = mnuButtons.Checked;
            //btnMakeView.Visible = mnuButtons.Checked;
            //btnWebsite.Visible = mnuButtons.Checked;
        }
        //		/// <summary>
        //		/// Display the About Epi Info dialog box
        //		/// </summary>
        //		/// <param name="sender">Object that fired the event</param>
        //		/// <param name="e">.NET supplied event parameters</param>
        //		private void mnuAbout_Activate(object sender, System.EventArgs e)
        //		{
        //			ShowAboutDialog();
        //		}

        /// <summary>
        /// Display Epi Info website
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void Website_Activate(object sender, System.EventArgs e)
        {
            OpenEpiInfoWebsite();
        }

        /// <summary>
        /// Handles code when Apply Changes event is fired from the Options dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="args">.NET supplied event args</param>
        protected override void OnApplyChanges(object sender, EventArgs args)
        {
            OnOptionsChanged();
        }

        private void mnuExtractStrings_Activate(object sender, System.EventArgs e)
        {
            //Maintenance.StringExtractor stringExtractor = new Epi.Windows.Menu.Maintenance.StringExtractor();
            //stringExtractor.ShowDialog();
        }


        private void Contents_Activate(object sender, System.EventArgs e)
        {
            //base.DisplayFeatureNotImplementedMessage();
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/support/userguide.html");
        }

        private void Exit_Activate(object sender, EventArgs e)
        {
            IModuleManager manager = this.GetService(typeof(IModuleManager)) as IModuleManager;
            if (manager != null)
            {
                manager.UnloadAll();
            }
        }

        private void AboutEpiInfo_Activate(object sender, EventArgs e)
        {
            OnAboutClicked();
        }

        private void Options_Activate(object sender, EventArgs e)
        {
            OnOptionsClicked();
        }

        /// <summary>
        /// Executes Menu command block
        /// </summary>
        protected void SelectMenu(object sender, System.EventArgs e)
        {
            ToolStripMenuItem itemClicked = new ToolStripMenuItem();
            itemClicked = (ToolStripMenuItem)sender;

            for (int i = 0; i <= menuData.GetUpperBound(0); i++)
            {
                if (string.Compare(menuData.GetValue(i, 2).ToString(), itemClicked.Text, true) == 0)
                {
                    MessageBox.Show("Execute Command Block: " + menuData.GetValue(i, 3).ToString(), "Menu"); // TODO: Non-standard string concatenation. Use ShareStrings resouirce
                } 
            }
            return;
        }

        private void toolStripMenuItemStatusBar_Click(object sender, EventArgs e)
        {
            OnViewStatusBarClicked(sender as ToolStripMenuItem);
        }

        private void epiInfoLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logFilePath = Logger.GetLogFilePath();
            //WinUtil.OpenTextFile(Logger.GetLogFilePath());

            if (File.Exists(logFilePath))
            {
                string containingFolder = Path.GetDirectoryName(logFilePath);
                System.Diagnostics.Process.Start(containingFolder);
            }
            else if (Directory.Exists(logFilePath))
            {
                System.Diagnostics.Process.Start(logFilePath);
            }
        }

        #endregion Event Handlers

        #region Public Methods

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Called when Options settings are changed by the user.
        /// </summary>
        protected override void OnOptionsChanged()
        {
            DisplayBackgroundImage();
            base.OnOptionsChanged();
        }
        

        #endregion Protected Methods

        #region Private Methods
        /// <summary>
        /// Loads Epi Info module specified by enumeration
        /// </summary>
        /// <param name="moduleType"></param>
        private void LoadModule(string moduleType)
        {
            try
            {
                BeginBusy(SharedStrings.LOADING_MODULE);

                string commandText = null;

                switch (moduleType/*.ToUpperInvariant()*/) // to fix a problem for Turkish users where ToUpperInvariant doesn't do what it's supposed to do
                {
                    case "ENTER":
                    commandText = Environment.CurrentDirectory + "\\Enter.exe";
                    break;
                    case "MAKEVIEW":
                    commandText = Environment.CurrentDirectory + "\\MakeView.exe";
                    break;
                    case "ANALYSIS":
                    commandText = Environment.CurrentDirectory + "\\Analysis.exe";
                    break;
                    case "DASHBOARD":
                    commandText = Environment.CurrentDirectory + "\\AnalysisDashboard.exe";
                    break;
                    case "EPIMAP":
                    commandText = Environment.CurrentDirectory + "\\Mapping.exe";
                    break;
                    case "EPIREPORT":
                    commandText = Environment.CurrentDirectory + "\\EpiReport.exe";
                    break;
                }
                
                if (!string.IsNullOrEmpty(commandText))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = commandText;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }

                /*
                // service locator pattern
                IModuleManager manager = this.Module.GetService(typeof(IModuleManager)) as IModuleManager;
                if (manager == null)
                {
                    MessageBox.Show("Module manager service not registered."); // TODO: Hard coded string
                    return;
                }

                IModule module = manager.CreateModuleInstance(moduleType);
                module.Load(manager, null);
                 */
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
            }
            finally
            {
                EndBusy();
            }
        }

        /// <summary>
        /// OpenEpiInfoWebsite opens the default browser and displays the Epi Info home page
        /// </summary>
        private void OpenEpiInfoWebsite()
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo");

        }
        /// <summary>
        /// Attach background image 
        /// </summary>
        private void DisplayBackgroundImage()
        {
            try
            {
                Configuration config = Configuration.GetNewInstance();
                string imageFile = config.Settings.BackgroundImage;

                if (string.IsNullOrEmpty(imageFile))
                {
                    DisplayDefaultBackgroundImage();
                    ChangeAllButtonStyles(false);
                }
                else
                {
                    //pbxBackground.Image = Image.FromFile(imageFile);
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                    this.BackgroundImage = Image.FromFile(imageFile);
                    ChangeAllButtonStyles(true);
                }
            }
            catch (FileNotFoundException ex)
            {
                MsgBox.ShowException(ex);
                //throw;
            }
        }

        /// <summary>
        /// Loads the default background image
        /// </summary>
        private void DisplayDefaultBackgroundImage()
        {
            try
            {
                //string imageFile = ImageFiles.DEFAULT_BACKGROUND_IMAGE;
                //pbxBackground.Image = ResourceLoader.GetImage(ResourceLoader.IMAGES_BACKGROUND); //Image.FromFile(imageFile);
                this.BackgroundImageLayout = ImageLayout.Stretch;
                this.BackgroundImage = ResourceLoader.GetImage(ResourceLoader.IMAGES_BACKGROUND); //Image.FromFile(imageFile);

                //Configuration.Current.Settings.BackgroundImage = imageFile;
                //Configuration.Current.Save();
            }
            catch (FileNotFoundException ex)
            {
                MsgBox.ShowException(ex);
                //throw;
            }
        }
        
        private void HideButtons()
        {
            this.btnMakeView.Visible = false;
            this.btnEnterData.Visible = false;
            this.btnAnalyze.Visible = false;
            this.btnDashboard.Visible = false;
            this.btnCreateMaps.Visible = false;
            this.btnCreateReports.Visible = false;
            this.btnWebsite.Visible = false;
            this.btnExit.Visible = false;
        }
        #endregion Private Methods

        private void populationSurveyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new StatCalc(StatCalc.Calculators.PopulationSurvey).Show();
        }

        private void cohortOrCrossToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Cohort).Show();
        }

        private void unmatchedCasecontrolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new StatCalc(StatCalc.Calculators.UnmatchedCaseControl).Show();
        }

        private void tables2x2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new StatCalc(StatCalc.Calculators.TwoByTwo).Show(); 
        }

        private void chiSquareForTrendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new StatCalc(StatCalc.Calculators.ChiSquareForTrend).Show();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            LoadModule("DASHBOARD");
        }

        private void videosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load("http://ftp.cdc.gov/pub/software/epi_info/sites.xml");
                List<System.Xml.XmlElement> elements = new List<System.Xml.XmlElement>();
                foreach (System.Xml.XmlElement element in doc.DocumentElement.ChildNodes)
                {
                    if (element.Name.Equals("videos"))
                    {
                        string url = element.InnerText;
                        System.Diagnostics.Process.Start(url);
                    }
                }
            }
            catch (Exception ex)
            {
                //
            }
        }

        private void communityMessageBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://epiinfo.atlassian.net/wiki/questions");
        }

        private void contactHelpdeskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Epi.Windows.MsgBox.ShowInformation(SharedStrings.HELPDESK_LOGIN);
            System.Diagnostics.Process.Start("https://epiinfo.atlassian.net/servicedesk/customer/portal/1");
        }

        private void activEpicomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://activepi.com");
        }
        
        private void openEpicomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://openepi.com");
        }

        private void mnuPoisson_Click(object sender, EventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Poisson).Show();
        }

        private void mnuBinomial_Click(object sender, EventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Binomial).Show();
        }

        private void mnuMatchedPairCaseControl_Click(object sender, EventArgs e)
        {
            new StatCalc(StatCalc.Calculators.MatchedPairCaseControl).Show();
        }

        private void pbxBackground_Click(object sender, EventArgs e)
        {

        }

        private void sampleSizeAndPowerToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void mnuMainMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }        
    }
}