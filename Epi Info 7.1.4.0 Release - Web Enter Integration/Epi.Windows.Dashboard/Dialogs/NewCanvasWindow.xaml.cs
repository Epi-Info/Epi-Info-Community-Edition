using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Epi;
using Epi.Data;

namespace Epi.WPF.Dashboard.Dialogs
{
    /// <summary>
    /// Interaction logic for NewCanvasWindow.xaml
    /// </summary>
    public partial class NewCanvasWindow : Window
    {
        #region Private Nested Class
        private class ComboBoxItem
        {
            #region Implementation
            private string key;
            public string Key
            {
                get { return key; }
                set { key = value; }
            }

            private object value;
            public object Value
            {
                get { return this.value; }
                set { this.value = value; }
            }

            private string text;
            public string Text
            {
                get { return text; }
                set { text = value; }
            }

            public ComboBoxItem(string key, string text, object value)
            {
                this.key = key;
                this.value = value;
                this.text = text;
            }

            public override string ToString()
            {
                return text.ToString();
            }
            #endregion
        }

        #endregion Private Nested Class
        
        #region Private Attributes
        private string selectedDataProvider;
        private string mruSelectedDatabaseName;        
        private Project selectedProject;
        private object selectedDataSource;
        private bool ignoreDataFormatIndexChange = false;
        private string savedConnectionStringDescription = string.Empty;
        private string sqlQuery = string.Empty;
        private Configuration config;
        #endregion Private Attributes

        public NewCanvasWindow()
        {
            InitializeComponent();
            Construct();
        }

        public NewCanvasWindow(Project project)
        {
            InitializeComponent();
            Construct();

            LoadFormsFromProject(project);
        }

        private void Construct()
        {
        }

        public object SelectedDataSource
        {
            get
            {
                return this.selectedDataSource;
            }
        }

        public string SelectedDataProvider
        {
            get
            {
                return this.selectedDataProvider;
            }
        }

        public Project SelectedProject
        {
            get
            {
                return this.selectedProject;
            }
        }

        public string SelectedDataMember
        {
            get
            {
                //if (cmbDatabaseType.SelectedIndex == 0)
                //{
                //    DataRowView rowView = lbxTables.SelectedItems[0] as DataRowView;
                //    DataRow row = rowView.Row;
                //    return row[0].ToString();
                //}
                //else
                //{
                    return lbxTables.SelectedItems[0].ToString();
                //}
            }
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAdvanced_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (lbxTables.SelectedItems == null || lbxTables.SelectedItems.Count == 0)
            {
                return;
            }

            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Load()
        {
            config = Configuration.GetNewInstance();
            PopulateDataSourcePlugIns();
            PopulateRecentDataSources();
            //PopulateAvailableProjects();
        }

        /// <summary>
        /// Attach the DataFormats combobox with supported data formats
        /// </summary>
        private void PopulateDataSourcePlugIns()
        {
            try
            {
                ignoreDataFormatIndexChange = true;
                if (cmbDatabaseType.Items.Count == 0)
                {
                    cmbDatabaseType.Items.Clear();

                    cmbDatabaseType.Items.Add(new ComboBoxItem(null, "Epi Info 7 Project", null));

                    foreach (Epi.DataSets.Config.DataDriverRow row in config.DataDrivers)
                    {
                        cmbDatabaseType.Items.Add(new ComboBoxItem(row.Type, row.DisplayName, null));
                    }
                }
                cmbDatabaseType.SelectedIndex = 0;
            }
            finally
            {
                ignoreDataFormatIndexChange = false;
            }
        }

        /// <summary>
        /// Attach the Recent data sources combobox with the list of recent sources
        /// </summary>
        private void PopulateRecentDataSources()
        {
            if (grdRecentDataSources.RowDefinitions.Count > 0)
            {
                grdRecentDataSources.RowDefinitions.Clear();
                grdRecentDataSources.Children.Clear();
            }

            try
            {
                //ignoreDataFormatIndexChange = true;
                //if (cmbRecentSources.Items.Count == 0)
                //{
                //    cmbRecentSources.Items.Clear();
                //    cmbRecentSources.Items.Add(string.Empty);

                    foreach (Epi.DataSets.Config.RecentDataSourceRow row in config.RecentDataSources)
                    {
                        string desc = string.Empty;
                        foreach (Epi.DataSets.Config.DataDriverRow drow in config.DataDrivers)
                        {
                            //cmbDatabaseType.Items.Add(new ComboBoxItem(drow.Type, drow.DisplayName, null));
                            if (row.DataProvider == drow.Type)
                            {
                                desc = drow.DisplayName;
                                break;
                            }
                        }
                        //cmbRecentSources.Items.Add(new ComboBoxItem(row.DataProvider, row.Name, row.ConnectionString));
                        PopulateRecentDataSource(desc, row.DataProvider, row.Name, row.ConnectionString);
                    }
                //}
                //cmbRecentSources.SelectedIndex = 0;
            }
            finally
            {
                //ignoreDataFormatIndexChange = false;
            }
        }

        void highlightDataSourceBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border border = sender as Border;
                if (border.Tag is RecentDataSourceInfo)
                {
                    RecentDataSourceInfo info = (RecentDataSourceInfo)border.Tag;
                    string conn = Configuration.Decrypt(info.ConnStr);
                    if (conn.EndsWith(".prj") || conn.EndsWith(".prj7"))
                    {
                        Project project = new Project(conn);
                        LoadFormsFromProject(project);
                    }
                    else
                    {
                        LoadTablesFromDatabase(info);
                    }
                }
            }
        }

        void highlightProjectBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border border = sender as Border;
                if (border.Tag is FileInfo)
                {
                    //if (CanCloseDashboard())
                    //{
                    //    FileInfo fi = (border.Tag as FileInfo);
                    //    OpenCanvas(fi.FullName);
                    //}
                }
            }
        }

        private void PopulateRecentDataSource(string dataProviderDesc, string dataProvider, string name, string connString)
        {
            int rowDefStart = grdRecentDataSources.RowDefinitions.Count;

            try
            {
                string conn = Configuration.Decrypt(connString);

                RowDefinition rowDef1 = new RowDefinition();
                rowDef1.Height = GridLength.Auto;

                RowDefinition rowDef2 = new RowDefinition();
                rowDef2.Height = GridLength.Auto;

                grdRecentDataSources.RowDefinitions.Add(rowDef1);
                grdRecentDataSources.RowDefinitions.Add(rowDef2);

                RecentDataSourceInfo recentDsInfo = new RecentDataSourceInfo();
                recentDsInfo.ConnStr = connString;
                recentDsInfo.Name = name;
                recentDsInfo.Provider = dataProvider;
                recentDsInfo.ProviderDesc = dataProviderDesc;

                Border highlightBorder = new Border();
                highlightBorder.Tag = recentDsInfo;
                highlightBorder.Style = this.Resources["recentDataBlueHighlightBorderStyle"] as Style;
                highlightBorder.MouseLeftButtonUp += new MouseButtonEventHandler(highlightDataSourceBorder_MouseLeftButtonUp);
                Grid.SetRow(highlightBorder, rowDefStart);
                Grid.SetRowSpan(highlightBorder, 2);
                Grid.SetColumn(highlightBorder, 0);
                Grid.SetColumnSpan(highlightBorder, 2);
                grdRecentDataSources.Children.Add(highlightBorder);

                TextBlock txt = new TextBlock();
                txt.Text = name;
                txt.IsHitTestVisible = false;
                txt.FontSize = 15;
                txt.Foreground = Brushes.White;
                txt.Margin = new Thickness(2, 8, 2, 2);
                Grid.SetRow(txt, rowDefStart);
                Grid.SetColumn(txt, 1);
                grdRecentDataSources.Children.Add(txt);

                txt = new TextBlock();
                txt.Text = dataProviderDesc;
                txt.IsHitTestVisible = false;
                txt.FontSize = 12;
                txt.Foreground = new SolidColorBrush(Color.FromRgb(191, 221, 242));
                txt.Margin = new Thickness(2, 2, 2, 8);
                Grid.SetRow(txt, rowDefStart + 1);
                Grid.SetColumn(txt, 1);
                grdRecentDataSources.Children.Add(txt);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
            }
        }

        //private void PopulateAvailableProject(FileInfo fi)
        //{
        //    int rowDefStart = grdAvailableProjects.RowDefinitions.Count;

        //    if (rowDefStart >= 10)
        //    {
        //        // limit to 10
        //        return;
        //    }

        //    RowDefinition rowDef1 = new RowDefinition();
        //    rowDef1.Height = GridLength.Auto;

        //    grdAvailableProjects.RowDefinitions.Add(rowDef1);

        //    Border highlightBorder = new Border();
        //    highlightBorder.Tag = fi;
        //    highlightBorder.Style = this.Resources["recentDataGrayHighlightBorderStyle"] as Style;
        //    highlightBorder.MouseLeftButtonUp += new MouseButtonEventHandler(highlightProjectBorder_MouseLeftButtonUp);
        //    Grid.SetRow(highlightBorder, rowDefStart);
        //    Grid.SetColumn(highlightBorder, 0);
        //    Grid.SetColumnSpan(highlightBorder, 2);
        //    grdAvailableProjects.Children.Add(highlightBorder);

        //    Project project = new Project(fi.FullName);

        //    TextBlock txt = new TextBlock();
        //    txt.Text = project.Name;
        //    txt.IsHitTestVisible = false;
        //    txt.FontSize = 14;
        //    txt.Foreground = Brushes.Gray;
        //    txt.Margin = new Thickness(2, 8, 2, 8);
        //    Grid.SetRow(txt, rowDefStart);
        //    Grid.SetColumn(txt, 1);
        //    grdAvailableProjects.Children.Add(txt);
        //}

        //private void PopulateAvailableProjects()
        //{
        //    if (config == null)
        //    {
        //        config = Configuration.GetNewInstance();
        //    }

        //    if (grdAvailableProjects.RowDefinitions.Count > 0)
        //    {
        //        grdAvailableProjects.RowDefinitions.Clear();
        //        grdAvailableProjects.Children.Clear();
        //    }

        //    DirectoryInfo di = new DirectoryInfo(config.Directories.Project);

        //    int count = 0;

        //    List<string> fileNames = new List<string>();

        //    foreach (DirectoryInfo projectDi in di.GetDirectories())
        //    {
        //        FileInfo[] fi = projectDi.GetFiles("*.prj");
        //        foreach (FileInfo canvasFi in fi)
        //        {
        //            fileNames.Add(canvasFi.FullName);
        //            count++;
        //        }
        //    }

        //    var sort = from fn in fileNames.ToArray()
        //               orderby new FileInfo(fn).LastWriteTime descending
        //               select fn;

        //    foreach (string n in sort)
        //    {
        //        FileInfo prjFi = new FileInfo(n);
        //        PopulateAvailableProject(prjFi);
        //    }
        //}

        private void txtConnectionInformation_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void LoadFormsFromProject(Project project)
        {
            //this.Cursor = Cursors.Wait;
            cmbDatabaseType.SelectedIndex = 0;
            lbxTables.ItemsSource = null;
            lbxTables.Items.Clear();
            lbxTables.ItemTemplate = null;

            this.selectedProject = project;
            this.selectedDataSource = project;
            txtConnectionInformation.Text = project.FilePath;

            //DataTable dt = new DataTable();
            //dt.Columns.Add(new DataColumn("Name", typeof(string)));
            //dt.Columns.Add(new DataColumn("Fields", typeof(int)));
            //dt.Columns.Add(new DataColumn("Pages", typeof(int)));

            //foreach (View view in project.Views)
            //{
            //    if (!string.IsNullOrEmpty(view.TableName))
            //    {
            //        dt.Rows.Add(view.Name, view.Fields.Count, view.Pages.Count);
            //    }
            //}

            //if (dt.Rows.Count > 0)
            //{
            //    lbxTables.ItemTemplate = this.Resources["listBoxEpiInfoForms"] as DataTemplate;
            //    lbxTables.DataContext = dt;
            //    lbxTables.ItemsSource = dt.DefaultView;

            //    selectedDataProvider = project.CollectedDataDriver.ToString();
            //}

            //this.Cursor = Cursors.Arrow;
            this.Cursor = Cursors.Wait;
            foreach (View view in project.Views)
            {
                if (!string.IsNullOrEmpty(view.TableName))
                {
                    lbxTables.Items.Add(view.Name);
                }
            }
            selectedDataProvider = project.CollectedDataDriver.ToString();
            this.Cursor = Cursors.Arrow;
        }

        private void LoadTablesFromDatabase(RecentDataSourceInfo info)
        {
            string connectionString = Configuration.Decrypt(info.ConnStr);
            string name = info.Name;
            string provider = info.Provider;

            mruSelectedDatabaseName = name;
            selectedDataProvider = provider;            
            
            IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(provider);
            if (dbFactory.ArePrerequisitesMet())
            {
                DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
                IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);
                db.ConnectionString = connectionString;

                try
                {
                    db.TestConnection();

                    cmbDatabaseType.Text = info.ProviderDesc;

                    LoadTablesFromDatabase(db);
                }
                catch (Exception ex)
                {
                    Dialogs.MsgBox.ShowException(ex);
                    return;
                }

                this.selectedDataSource = db;
            }            
        }

        private void LoadTablesFromDatabase(IDbDriver db)
        {
            lbxTables.ItemsSource = null;            
            lbxTables.Items.Clear();
            lbxTables.ItemTemplate = null;
            txtConnectionInformation.Text = db.ConnectionString;

            List<string> tableNames = db.GetTableNames();
            foreach (string tableName in tableNames)
            {
                if (!string.IsNullOrEmpty(tableName.Trim()))
                {
                    lbxTables.Items.Add(tableName);
                }
            }
        }

        private void btnConnectionBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDatabaseType.SelectedIndex >= 0)
            {
                ComboBoxItem item = cmbDatabaseType.SelectedItem as ComboBoxItem;
                switch (item.Text)
                {
                    case "Epi Info 7 Project":
                        System.Windows.Forms.OpenFileDialog projectDialog = new System.Windows.Forms.OpenFileDialog();
                        projectDialog.InitialDirectory = config.Directories.Project;
                        projectDialog.Filter = "Epi Info 7 Project File|*.prj";

                        System.Windows.Forms.DialogResult projectDialogResult = projectDialog.ShowDialog();

                        if (projectDialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            try
                            {
                                Project project = new Project(projectDialog.FileName);
                                LoadFormsFromProject(project);
                            }
                            catch (System.Security.Cryptography.CryptographicException ex)
                            {
                                Dialogs.MsgBox.ShowError(string.Format(SharedStrings.ERROR_CRYPTO_KEYS, ex));
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                        break;
                    default:
                        IDbDriverFactory dbFactory = null;

                        selectedDataProvider = item.Key;

                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(item.Key);
                        if (dbFactory.ArePrerequisitesMet())
                        {
                            DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
                            IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);

                            IConnectionStringGui dialog = dbFactory.GetConnectionStringGuiForExistingDb();

                            if (!string.IsNullOrEmpty(savedConnectionStringDescription))
                            {
                                int splitIndex = savedConnectionStringDescription.IndexOf("::");
                                if (splitIndex > -1)
                                {
                                    string serverName = savedConnectionStringDescription.Substring(0, splitIndex);
                                    string databaseName = savedConnectionStringDescription.Substring(splitIndex + 2, savedConnectionStringDescription.Length - splitIndex - 2);
                                    dialog.SetDatabaseName(databaseName);
                                    dialog.SetServerName(serverName);
                                }
                            }

                            System.Windows.Forms.DialogResult result = ((System.Windows.Forms.Form)dialog).ShowDialog();

                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                this.savedConnectionStringDescription = dialog.ConnectionStringDescription;
                                bool success = false;
                                db.ConnectionString = dialog.DbConnectionStringBuilder.ToString();

                                try
                                {
                                    success = db.TestConnection();
                                }
                                catch
                                {
                                    success = false;
                                    MessageBox.Show("Could not connect to selected data source.");
                                }

                                if (success)
                                {
                                    this.selectedDataSource = db;
                                    LoadTablesFromDatabase(db);
                                    //formNeedsRefresh = true;
                                }
                                else
                                {
                                    this.selectedDataSource = null;
                                }                                
                            }
                            else
                            {
                                this.selectedDataSource = null;
                            }                            
                        }
                        else
                        {
                            MessageBox.Show(dbFactory.PrerequisiteMessage, "Prerequisites not found");
                        }
                        break;
                }
            }
        }

        private struct RecentDataSourceInfo
        {
            public string ConnStr;
            public string Provider;
            public string ProviderDesc;
            public string Name;
        }
    }    
}
