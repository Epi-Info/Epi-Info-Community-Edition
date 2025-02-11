using Epi.Windows.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;




namespace Epi.Windows.Menu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Controls.UserControl
    {
        MainForm mainform = null;
        public event EventHandler<RoutedEventArgs> Close;
        /// <summary>
        /// Redefined Module property
        /// </summary>      

        # region Constructor

        public MainWindow()
        {
            InitializeComponent();
            ApplicationIdentity appId = new ApplicationIdentity(typeof(Configuration).Assembly);
            this.tsslLocale.Text = Thread.CurrentThread.CurrentUICulture.Name;
            this.tsslVersion.Text = appId.Version;

            if (Configuration.IsRelease)
            {
                versionFooter.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 59, 106));
                testingOnly.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                versionFooter.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(106, 21, 18));
                testingOnly.Visibility = System.Windows.Visibility.Visible;
            }

            mainform = new MainForm();

            #region Translation

            txtlblCreateForms.Text = MenuSharedStrings.MENU_CREATE_FORMS;
            txtlblCreateFormsDescp.Text = MenuSharedStrings.MENU_CREATE_FORMS_DESCP;
            txtlblEnterData.Text = MenuSharedStrings.MENU_ENTER_DATA;
            txtlblEnterDataDescp.Text = MenuSharedStrings.MENU_ENTER_DATA_DESCP;
            txtlblCreateMaps.Text = MenuSharedStrings.MENU_CREATE_MAPS;
            txtlblCreateMapsDescp.Text = MenuSharedStrings.MENU_CREATE_MAPS_DESCP;
            txtlblStatCalc.Text = MenuSharedStrings.MENU_STATCALC;
            txtlblStatCalcDescp.Text = MenuSharedStrings.MENU_STATCALC_DESCP;
            txtlblAnalyzeData.Text = MenuSharedStrings.MENU_ANALYZE_DATA;
            txtlblClassic.Text = MenuSharedStrings.MENU_CLASSIC_ANALYZE;
            txtlblClassicDescp.Text = MenuSharedStrings.MENU_CLASSIC_ANALYZEDSCP;
            txtlblVisualDashboard.Text = MenuSharedStrings.MENU_VISUALDASHBOARD;
            txtlblVisualDashboardDscp.Text = MenuSharedStrings.MENU_VISUALDASHBOARD_DESCP;
            txtLanguage.Text = MenuSharedStrings.MENU_LANGUAGE;
            txtContents.Text = MenuSharedStrings.MENU_CONTENT;
            txtHowToVideos.Text = MenuSharedStrings.MENU_HOW_TO_VIDEOS;
            txtEpiInfoQA.Text = MenuSharedStrings.MENU_EPI_INFO_QA;
            txtContactHelpDesk.Text = MenuSharedStrings.MENU_CONTACT_HELP_DESK;
            txtOtherEpiResources.Text = MenuSharedStrings.MENU_OTHER_RESOURCES;
            txtEpiInfoLogs.Text = MenuSharedStrings.MENU_EPI_INFO_LOGS;
            txtOptions.Text = MenuSharedStrings.MENU_OPTIONS;
            txtExit.Text = MenuSharedStrings.MENU_EXIT;
            txtVersion.Text = MenuSharedStrings.MENU_VERSION_TEXT;
            epiInfoWebsite.Text = MenuSharedStrings.MENU_FOOTER_EPIINFOWEBSITE;
            aboutEpiInfo.Text = MenuSharedStrings.MENU_FOOTER_ABOUTEPIINFO;



            #endregion 

        }



        #endregion

        private void button_MouseOver(object sender, EventArgs e)
        {
        }

        private void enterData_Click(object sender, EventArgs e)
        {
            LoadModule("ENTER");
        }

        private void createMaps_Click(object sender, EventArgs e)
        {
            LoadModule("EPIMAP");
        }

        private void statCalc_Click(object sender, EventArgs e)
        {
            LoadModule("StatCalc");
        }

        private void classicAnalyze_Click(object sender, EventArgs e)
        {
            LoadModule("ANALYSIS");
        }

        private void visualAnalyze_Click(object sender, EventArgs e)
        {
            LoadModule("DASHBOARD");
        }

        private void createForms_Click(object sender, EventArgs e)
        {

            LoadModule("MAKEVIEW");



        }



        private void createForms_hover(object sender, EventArgs e)
        {

            createFormsBox.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f3f3f3"));

        }

        private void createForms_Leave(object sender, EventArgs e)
        {
            createFormsBox.Background = Brushes.White;

        }

        private void enterData_Leave(object sender, EventArgs e)
        {
            enterDataBox.Background = Brushes.White;
        }

        private void enterData_hover(object sender, EventArgs e)
        {

            enterDataBox.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f3f3f3"));

        }

        private void createMaps_Leave(object sender, EventArgs e)
        {
            createMapsBox.Background = Brushes.White;
        }

        private void createMaps_hover(object sender, EventArgs e)
        {

            createMapsBox.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f3f3f3"));

        }

        private void statCalc_Leave(object sender, EventArgs e)
        {
            statCalcBox.Background = Brushes.White;
        }

        private void statCalc_hover(object sender, EventArgs e)
        {

            statCalcBox.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f3f3f3"));

        }

        private void classic_hover(object sender, EventArgs e)
        {
            classicBox.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f3f3f3"));
        }

        private void classic_Leave(object sender, EventArgs e)
        {
            classicBox.Background = Brushes.White;
        }


        private void visualDashboard_hover(object sender, EventArgs e)
        {
            visualDashboardBox.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f3f3f3"));
        }

        private void visualDashboard_Leave(object sender, EventArgs e)
        {
            visualDashboardBox.Background = Brushes.White;
        }


        //Link for Epi Info Website - located in footer
        //private void Hyperlink_epiInfoWebsite(object sender, RequestNavigateEventArgs e)
        //{
        //    HideStoryBoard();
        //    System.Diagnostics.Process.Start(e.Uri.ToString());
        //}

        private void contents_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/support/userguide.html");
        }

        private void epiInfoWebsite_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/");
        }

        private void howToVideo_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
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

        private void SyncFile2CSV_Click(object sender, EventArgs e)
        {
            SyncFile2CSV();
        }

        private void SyncFile2CSV()
        {
            HideStoryBoard();

            try
            {
                string commandText = null;

                commandText = Environment.CurrentDirectory + "\\SyncFile2CSV.exe";

                if (!string.IsNullOrEmpty(commandText))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = commandText;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
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

        private void epiInfoQA_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            System.Diagnostics.Process.Start("https://epiinfo.atlassian.net/wiki/questions");
        }

        private void contactHelpDesk_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            Epi.Windows.MsgBox.ShowInformation(SharedStrings.HELPDESK_LOGIN);
            System.Diagnostics.Process.Start("https://epiinfo.atlassian.net/servicedesk/customer/portal/1");
        }

        private void activEpi_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            System.Diagnostics.Process.Start("http://activepi.com");
        }

        private void openEpi_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            System.Diagnostics.Process.Start("http://openepi.com");
        }

        private void microbeTrace_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            System.Diagnostics.Process.Start("https://github.com/CDCgov/MicrobeTrace/wiki");
        }

        private void epi_InfoLogs_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            string logFilePath = Logger.GetLogFilePath();
            //WinUtil.OpenTextFile(Logger.GetLogFilePath());

            if (File.Exists(logFilePath))
            {
                string containingFolder = System.IO.Path.GetDirectoryName(logFilePath);
                System.Diagnostics.Process.Start(containingFolder);
            }
            else if (Directory.Exists(logFilePath))
            {
                System.Diagnostics.Process.Start(logFilePath);
            }
        }

        private void options_Click(object sender, EventArgs e)
        {
            HideStoryBoard();
            OnOptionsClicked(OptionsDialog.OptionsTabIndex.General);
        }

        /// <summary>
        /// Handles the Click event of the About Epi Info menu item
        /// </summary>
        protected void OnAboutClicked()
        {
            HideStoryBoard();
            if (mainform == null)
                mainform = new MainForm();
            AboutEpiInfoDialog dlg = new AboutEpiInfoDialog(mainform);
            dlg.ShowDialog();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            /*if (mainform == null)
                mainform = new MainForm();
            IModuleManager manager = mainform.Module.GetService(typeof(IModuleManager)) as IModuleManager;
            if (manager != null)
            {
                manager.UnloadAll();
            }*/
            //((System.Windows.Threading.DispatcherObject)(this.Parent)).Dispatcher.InvokeShutdown();   
            if (Close != null)
                Close(this, new RoutedEventArgs());
        }


        private void LoadModule(string moduleType)
        {
            HideStoryBoard();
            try
            {
                // BeginBusy(SharedStrings.LOADING_MODULE);

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
                    case "StatCalc":
                        commandText = Environment.CurrentDirectory + "\\StatCalc.exe";
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



        public void BeginBusy(string str)
        {
            UpdateStatus(str, false);
            this.Cursor = Cursors.Wait;
        }
        /// <summary>
        /// Sets the cursor to normal and all status messages to default
        /// </summary>
        public void EndBusy()
        {
            this.Cursor = Cursors.None;
            UpdateStatus(SharedStrings.READY, false);
        }

        public void UpdateStatus(string str, bool shouldAddLogEntry, bool doEvents = false)
        {
            // this.tsslMessage.Text = str;
            if (shouldAddLogEntry)
            {
                Logger.Log(DateTime.Now + ":  " + str);
            }
            if (doEvents)
            {
                // Application.do();
            }
        }


        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnOptionsClicked(OptionsDialog.OptionsTabIndex.General);
        }

        private void btnLocaleClick(object sender, RoutedEventArgs e)
        {
            HideStoryBoard();
            OnOptionsClicked(OptionsDialog.OptionsTabIndex.Language);
        }

        /// <summary>
        /// Handles the Click event of the Options menu item
        /// </summary>
        /// <param name="tabIndex">The index of the options dialog tab </param>
        protected void OnOptionsClicked(OptionsDialog.OptionsTabIndex tabIndex)
        {
            if (mainform == null)
                mainform = new MainForm();
            OptionsDialog dlg = new OptionsDialog(mainform, tabIndex);
            try
            {
                dlg.ApplyChanges += new OptionsDialog.ApplyChangesHandler(OnApplyChanges);
                System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    OnOptionsChanged();
                }
            }
            finally
            {
                dlg.Close();
                dlg.Dispose();
            }
        }

        /// <summary>
        /// On Apply Changes
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="args">Arguments</param>
        protected virtual void OnApplyChanges(object sender, EventArgs args)
        {
            OnOptionsChanged();
        }

        /// <summary>
        /// Called when Option settings are changed by the user.
        /// </summary>
        protected virtual void OnOptionsChanged()
        {
        }

        private void aboutEpiInfo_click(object sender, EventArgs e)
        {
            HideStoryBoard();
            OnAboutClicked();
        }

        private void btnRightMenuHide_Click(object sender, RoutedEventArgs e)
        {

            var pnlRightMenuHeight = pnlRightMenu.Height.ToString();

            if (pnlRightMenuHeight == "0")
            {
                this.BeginStoryboard(FindResource("sbShowRightMenu") as Storyboard);


            }
            else if (pnlRightMenuHeight == "200")
            {
                this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);

            }
            else if (pnlRightMenuHeight == "220")
            {
                this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);

            }

            //MessageBox.Show("Hide");
        }

        private void HideStoryBoard()
        {
            var pnlRightMenuHeight = pnlRightMenu.Height.ToString();

            if (pnlRightMenuHeight == "200")
            {
                this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);
            }
            else if (pnlRightMenuHeight == "220")
            {
                this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);
            }
        }

        private void tabEvent()
        {



        }

        private void Other_EpiResource_Click(object sender, EventArgs e)
        {
            var submenuEpiResoucesHeight = submenuEpiResouces.Height.ToString();

            if (submenuEpiResoucesHeight == "0")
            {
                this.BeginStoryboard(FindResource("showEpiResource") as Storyboard);
            }
            else if (submenuEpiResoucesHeight == "80")
            {
                this.BeginStoryboard(FindResource("hideEpiResource") as Storyboard);
            }
        }

        private void click_leftDown(object sender, MouseButtonEventArgs e)
        {
            var pnlRightMenuHeight = pnlRightMenu.Height.ToString();

            if (pnlRightMenuHeight == "200")
            {
                this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);
            }
            else if (pnlRightMenuHeight == "220")
            {
                this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);
            }
        }


        private void click_Test(object sender, MouseButtonEventArgs e)
        {
            System.Windows.MessageBox.Show("Hi");
        }

        private void createForms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                LoadModule("MAKEVIEW");
            }
        }

        private void enterData_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                LoadModule("ENTER");
            }
        }

        private void createMaps_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                LoadModule("EPIMAP");
            }
        }

        private void statCalc_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                LoadModule("StatCalc");
            }
        }

        private void classic_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                LoadModule("ANALYSIS");
            }
        }

        private void visualDashboard_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                LoadModule("DASHBOARD");
            }
        }

        private void epiInfoWebsite_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/");
            }
        }

        private void aboutEpiInfo_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                OnAboutClicked();
            }
        }

        private void language_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                OnOptionsClicked(OptionsDialog.OptionsTabIndex.Language);
            }
        }

        private void rightMenu_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var pnlRightMenuHeight = pnlRightMenu.Height.ToString();

                if (pnlRightMenuHeight == "0")
                {
                    this.BeginStoryboard(FindResource("sbShowRightMenu") as Storyboard);
                }
                else if (pnlRightMenuHeight == "200")
                {
                    this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);
                }
                else if (pnlRightMenuHeight == "220")
                {
                    this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);
                }
            }
        }

        private void options_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                OnOptionsClicked(OptionsDialog.OptionsTabIndex.General);
            }
        }

        private void contents_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/support/userguide.html");
            }
        }

        private void howToVideo_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
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
        }
        private void SyncFile2CSV_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                SyncFile2CSV();
            }
        }

        private void epiInfoQA_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                System.Diagnostics.Process.Start("https://epiinfo.atlassian.net/wiki/questions");
            }
        }

        private void contactHelpDesk_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                Epi.Windows.MsgBox.ShowInformation(SharedStrings.HELPDESK_LOGIN);
                System.Diagnostics.Process.Start("https://epiinfo.atlassian.net/servicedesk/customer/portal/1");
            }
        }

        private void otherEpiResource_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var submenuEpiResoucesHeight = submenuEpiResouces.Height.ToString();

                if (submenuEpiResoucesHeight == "0")
                {
                    this.BeginStoryboard(FindResource("showEpiResource") as Storyboard);


                }
                else if (submenuEpiResoucesHeight == "80")
                {
                    this.BeginStoryboard(FindResource("hideEpiResource") as Storyboard);



                }
            }
        }

        private void activeEpi_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                System.Diagnostics.Process.Start("http://activepi.com");
            }
        }

        private void openEpi_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                System.Diagnostics.Process.Start("http://openepi.com");
            }
        }
        private void microbeTrace_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                System.Diagnostics.Process.Start("https://microbetrace.cdc.gov/MicrobeTrace");
            }
        }

        private void epiInfoLogs_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                HideStoryBoard();
                string logFilePath = Logger.GetLogFilePath();
                //WinUtil.OpenTextFile(Logger.GetLogFilePath());

                if (File.Exists(logFilePath))
                {
                    string containingFolder = System.IO.Path.GetDirectoryName(logFilePath);
                    System.Diagnostics.Process.Start(containingFolder);
                }
                else if (Directory.Exists(logFilePath))
                {
                    System.Diagnostics.Process.Start(logFilePath);
                }
            }
        }























        //private void helpandother_leave(object sender, MouseEventArgs e)
        //{
        //    this.BeginStoryboard(FindResource("sbHideRightMenu") as Storyboard);
        //}


    }
}
