#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using Epi.Windows.Docking;
using Epi.Web;
using System.Security.Permissions;
using Epi.Core.AnalysisInterpreter;
using Epi.Analysis;
using Epi.Data;
using Epi.Data.SqlServer;
using Epi.Data.Office;

#endregion Namespaces

namespace Epi.Windows.Analysis.Forms
{
    /// <summary>
    /// The main Output window
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public partial class OutputWindow : DockWindow
    {
        public OutputEventQueue eventQueue = null;


         #region Public Interface
        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public OutputWindow()
        {
            InitializeComponent();
            if (!this.DesignMode)           // designer throws an error
            {
                ResetOutput();
                Navigate(OutputFile);

                eventQueue = new OutputEventQueue();
                eventQueue.DialogDelegateMethod += this.Dialog;
                eventQueue.TryGetFileDialogMethod += this.TryGetFileDialog;
                eventQueue.Changed += new OutputEventQueue.ChangedEventHandler(delegate(object s, EventArgs ex)
                {
                    if (eventQueue.Count > 0)
                    {
                        OutputEventArg arg = eventQueue.Dequeue();

                        this.OutputEventChangedHandler(this,arg);
                    }
                });
            }
        }

        /// <summary>
        /// Tracking Id: 782 Invoked the AdjustDocumentTitle() method by delegate 
        /// Issue: System.InvalidOperationException: Cross-thread operation not valid: Control 'OutputWindow' accessed from a thread other than the thread it was created on. 
        /// </summary>
        public delegate void delegateAdjustDocTitle();

      
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainForm">Main Form</param>
        public OutputWindow(AnalysisMainForm mainForm)
            : this()
        {
            this.mainForm = mainForm;
        }
        #endregion


        /// <summary>
        /// Dictionary HeaderLevels
        /// </summary>
        #region Public Properties

        public Dictionary<int, HeaderLevel> HeaderLevels
        {
            get
            {
                if (headerLevels == null)
                {
                    headerLevels = new Dictionary<int, HeaderLevel>();

                    for (int i = 0; i < 2; i++)
                    {
                        headerLevels.Add(i, new HeaderLevel(i));
                    }
                }

                return headerLevels;
            }

            set
            {
                headerLevels = value;
            }
        }


        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Shows a web page based on URL
        /// </summary>
        /// <param name="address">A URL</param>
        public void Navigate(string address)
        {
            if (String.IsNullOrEmpty(address)) return;
            if (address.Equals("about:blank")) return;
            if (!address.StartsWith("file://"))
            {
                address = "file://" + address;
            } 
            try
            {
                if (webBrowser1 != null)
                {
                    webBrowser1.Refresh(WebBrowserRefreshOption.Completely);
                    webBrowser1.Navigate(new Uri(address));
                }
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }

        /// <summary>
        /// Set the header level
        /// </summary>
        /// <param name="header">HeaderLevel to be set</param>
        public void SetHeaderLevel(HeaderLevel header)
        {
            if (this.HeaderLevels.ContainsKey(header.LevelNumber))
            {
                this.HeaderLevels[header.LevelNumber] = header;
            }
            else
            {
                this.HeaderLevels.Add(header.LevelNumber, header);
            }
        }

        /// <summary>
        /// Appends HTML to the output file
        /// </summary>
        /// <param name="results">Command Processor Results</param>
        public void SendToOutput(CommandProcessorResults results)
        {
            if (results.ResetOutput)
            {
                ResetOutput();
            }
            if (!string.IsNullOrEmpty(results.HtmlOutput))
            {
                //New results' anchor name
                string currentAnchorName = "#Results" + currentAnchorNumber.ToString();
                //New anchor and up and down hyperlinks
                string hyperlinks = HTML.Anchor(currentAnchorName);
                currentAnchorNumber--;
                if (currentAnchorNumber >= 0)
                {
                    hyperlinks += HTML.HyperLink("Back", "#Results" + currentAnchorNumber);
                }
                currentAnchorNumber += 2;
                hyperlinks += "&nbsp;&nbsp;";
                hyperlinks += HTML.HyperLink("Forward", "#Results" + currentAnchorNumber);
                hyperlinks += HTML.Tag("br");

                results.HtmlOutput = hyperlinks + results.HtmlOutput;
                using (StreamWriter writer = File.AppendText(OutputFile))
                {
                    writer.WriteLine(results.HtmlOutput);
                    writer.Flush();
                    writer.Close();
                }
                Regex commandRegEx = new Regex(@"<h3>(?<cmd>\w+[\s*\w*]*\s*)</h3>");
                Match cmdMatch = commandRegEx.Match(results.HtmlOutput);

                string commandName = String.Empty;
                if (cmdMatch.Success)
                {
                    if (OutputFile.Equals(EpiHome))
                    {
                        SetOutputFile(string.Empty);
                        File.Copy(EpiHome, OutputFile, true);
                    }
                    commandName = cmdMatch.Result("${cmd}");
                }
                string address = "file://" + OutputFile + currentAnchorName;
                Uri URL = new Uri(address);
                if (URL.IsFile)
                {
                    if (!String.IsNullOrEmpty(commandName))
                    {
                        sessionHistory.Add(DateTime.Now.ToShortTimeString(), URL, commandName.Trim());
                    }
                }
                else
                {
                    sessionHistory.Add(DateTime.Now.ToShortTimeString(), URL, URL.Segments[URL.Segments.GetUpperBound(0)]);
                }
                Navigate(address);
                Focus();
                //ToggleButtons(true);
            }
        }

        /// <summary>
        /// Appends HTML to Output File
        /// </summary>
        /// <param name="command">Command being sent to Output file, string.empty if the command does not save</param>
        /// <param name="htmlOutput">HTML to append</param>
        /// <param name="resetOutput">Reset output to new file</param>
        public void SendToOutput(string command, string htmlOutput, bool resetOutput)
        {
            this.isReplaceRouteOut = resetOutput;
            if (resetOutput)
            {
                ResetOutput();
            }

            StreamWriter outputStream = null;

            try
            {
                string CurrentText = null;
                if (File.Exists(this.outputFile))
                {
                    StreamReader inputStream = File.OpenText(this.outputFile);
                    CurrentText = inputStream.ReadToEnd();
                    inputStream.Close();
                }

                outputStream = File.CreateText(this.OutputFile);
                outputStream.Write(CurrentText);

                if (!string.IsNullOrEmpty(htmlOutput))
                {
                    string hyperlinks = String.Empty;
                    string currentAnchorName = String.Empty;

                    if (!String.IsNullOrEmpty(command))
                    {
                        currentAnchorName = "#Results" + currentAnchorNumber.ToString();
                        hyperlinks = HTML.Anchor(currentAnchorName);
                        currentAnchorNumber++;
                    }

                    htmlOutput = hyperlinks + HTML.H3(command) + htmlOutput;
                    outputStream.WriteLine(htmlOutput);
                    outputStream.Flush();

                    if (!String.IsNullOrEmpty(routeoutFile))
                    {
                        CurrentText = string.Empty;
                        StreamWriter routeoutStream = null;

                        if (File.Exists(this.routeoutFile))
                        {
                            StreamReader inputStream = File.OpenText(this.routeoutFile);
                            CurrentText = inputStream.ReadToEnd();
                            inputStream.Close();

                            routeoutStream = File.CreateText(this.routeoutFile);
                            routeoutStream.Write(CurrentText);
                            routeoutStream.Write(htmlOutput);
                            routeoutStream.Flush();
                            routeoutStream.Close();
                        }
                        else
                        {
                            routeoutStream = File.CreateText(this.routeoutFile);
                            
                            if (!hasRouteOutHeader)
                            {
                                string outputTemplate = Epi.Resources.ResourceLoader.GetAnalysisOutputTemplate();
                                outputTemplate = InsertHeaderStyles(outputTemplate);
                                routeoutStream.Write(outputTemplate);
                                routeoutStream.Flush();
                                hasRouteOutHeader = true;
                            }
                            
                            routeoutStream.Write(htmlOutput);
                            routeoutStream.Flush();
                            routeoutStream.Close();
                        }
                    }

                    string address = "file://" + OutputFile + currentAnchorName;
                    Uri URL = new Uri(address);
                    
                    if (URL.IsFile)
                    {
                        if (!String.IsNullOrEmpty(command))
                        {
                            sessionHistory.Add(DateTime.Now.ToShortTimeString(), URL, command.Trim());
                        }
                    }
                    else
                    {
                        sessionHistory.Add(DateTime.Now.ToShortTimeString(), URL, URL.Segments[URL.Segments.GetUpperBound(0)]);
                    }
                
                    Navigate(address);
                    Focus();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if(outputStream != null)
                {
                    outputStream.Flush();
                    outputStream.Close();
                }
            }
        }

        /// <summary>
        /// Set a custom output file or resume Automated Output file names.
        /// </summary>
        /// <param name="fileName">Name of output file.</param>
        public void SetOutputFile(string fileName)
        {

            if (string.IsNullOrEmpty(fileName) || fileName.Equals(String.Empty))
            {
                this.outputFile = OutputFile;
            }
            else
            {
                this.outputFile = fileName;
            }
        }
        
        /// <summary>
        /// Prints the document currently displayed
        /// using the current print and page settings.
        /// </summary>
        public void Printout()
        {
            webBrowser1.Print();
        }
        
        /* Old Printout command
        /// <summary>
        /// Navigates to the filename in the urlString
        /// and prints the document
        /// using the current print and page settings
        /// and resets the current document back to the previous document.
        /// </summary>
        /// <param name="urlString"></param>
        public void Printout(string urlString)
        {
            Uri urlPrevious = webBrowser1.Url;
            webBrowser1.Navigate(new Uri(urlString));
            Printout();
            webBrowser1.Url = urlPrevious;
        }*/

        #endregion
        #endregion Public Interface

        #region Protected Interface
        //
        #region Protected Properties
        //
        #endregion Protected Properties

        #region Protected Methods
        //
        #endregion Protected Methods

        #region Protected Events
        //
        #endregion Protected Events
        #endregion Protected Interface

        #region Private Members
        #region Private Enums and Constants
        //
        #endregion Private Enums and Constants

        #region Private Attributes
        private string outputFile;
        private bool hasHeader = false;
        private bool hasRouteOutHeader = false;
        private Dictionary<int, HeaderLevel> headerLevels;
        private bool isReplaceRouteOut = false;
        private string epiHome;
        private int currentAnchorNumber = 0;
        private string displayedAnchor = String.Empty;
        private Uri currentUri;
        private SessionHistory sessionHistory = new SessionHistory();
        private ResultsHistory resultsHistory = new ResultsHistory();
        new private Epi.Windows.Analysis.Forms.AnalysisMainForm mainForm;

        private string routeoutFile = string.Empty;

        #endregion Private Attributes

        #region Private Properties
        /// <summary>
        /// Gets the analysis output file
        /// </summary>
        private string OutputFile
        {
            get
            {
                Configuration config = Configuration.GetNewInstance();
                StreamWriter outstream = null;

                // generate path\filename if not given
                if (string.IsNullOrEmpty(this.outputFile))
                {
                    int counter = 1;
                    while (true)
                    {
                        if (!File.Exists(Path.Combine(config.Directories.Output, "output" + counter + ".html")))
                        {
                            outputFile = Path.Combine(config.Directories.Output, "output" + counter + ".html");

                            string outputTemplate = Epi.Resources.ResourceLoader.GetAnalysisOutputTemplate();
                            try
                            {
                                outstream = File.CreateText(outputFile);
                                //File.WriteAllText(outputFile, outputTemplate);
                                outstream.Write(outputTemplate);
                                hasHeader = true;
                                //File.SetAttributes(outputFile, FileAttributes.Normal);
                            }
                            catch (Exception ex)
                            {
                                // do nothing for now
                            }
                            finally
                            {
                                if (outstream != null)
                                {
                                    outstream.Flush();
                                }
                            }
                            break;
                        }
                        counter++;
                    }



                    // add default path if no path is given
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(this.outputFile)))
                    {
                        outputFile = Path.Combine(config.Directories.Output, outputFile);
                    }

                    // add file extension if extension not given
                    if (!(this.outputFile.EndsWith(".html") || this.outputFile.EndsWith(".htm")))
                    {
                        outputFile = outputFile + ".html";
                    }

                    // write output template if creating new file
                    bool fileExists = File.Exists(this.outputFile);
                    if (!fileExists || (fileExists && isReplaceRouteOut))
                    {
                        if (outstream == null)
                        {
                            outstream = File.CreateText(outputFile);
                        }
                        //File.WriteAllText(outputFile, Epi.Resources.ResourceLoader.GetAnalysisOutputTemplate());
                        if(!hasHeader)
                        {
                        outstream.Write(Epi.Resources.ResourceLoader.GetAnalysisOutputTemplate());
                        hasHeader = true;
                        outstream.Flush();
                            }
                        File.SetAttributes(outputFile, FileAttributes.Normal);
                        /*
                        if (!string.IsNullOrEmpty(this.routeoutFile) && this.routeoutFile != "")
                        {
                            if (File.Exists(this.routeoutFile))
                            {
                                File.Delete(this.routeoutFile);
                            }

                        }
                        this.isReplaceRouteOut = false;*/
                    }

                    outstream.Flush();
                    outstream.Close();
                }
                return outputFile;
            }
        }

        /// <summary>
        /// Gets the default output file.
        /// </summary>
        private string EpiHome
        {
            get
            {
                if (String.IsNullOrEmpty(this.epiHome))
                {
                    Configuration config = Configuration.GetNewInstance();
                    this.epiHome = Path.Combine(config.Directories.Output, "EpiHome.html");
                    string outputTemplate = Epi.Resources.ResourceLoader.GetAnalysisOutputTemplate();
                    if (!hasHeader)
                    {
                        outputTemplate = InsertHeaderStyles(outputTemplate);
                        hasHeader = true;
                    }
                    StreamWriter outStream = File.CreateText(this.epiHome);
                    outStream.Write(outputTemplate);
                    outStream.Flush();
                    outStream.Close();

                    //File.WriteAllText(this.epiHome, outputTemplate);

                    File.SetAttributes(this.epiHome, FileAttributes.Normal);
                }
                return this.epiHome;
            }
        }

        private string InsertHeaderStyles(string outputTemplate)
        {
            int styleStart = outputTemplate.IndexOf("<style");
            int styleEnd = outputTemplate.IndexOf(">", styleStart + 1);
            foreach(HeaderLevel level in this.HeaderLevels.Values)
            {
                outputTemplate = outputTemplate.Insert(styleEnd + 1, level.CssText);
            }
            return outputTemplate;
        }
        #endregion Private Properties

        #region Private Methods
        private void ResetOutput()
        {
            Configuration config = Configuration.GetNewInstance();
            this.hasHeader = false;
            this.hasRouteOutHeader = false;
            //this.epiHome = Path.Combine(config.Directories.Output, "EpiHome.html");
            this.epiHome = string.Empty;
            /*
            if (!String.IsNullOrEmpty(routeoutFile))
            {
                if (File.Exists(this.routeoutFile))
                {
                    File.Delete(this.routeoutFile);
                }
            }*/
            this.outputFile = string.Empty;
           
            SetOutputFile(string.Empty);
            currentAnchorNumber = 0;
            ToggleButtons(false);
        }

        private void ToggleButtons(bool on)
        {
            tsbBookmark.Enabled = on;
            tsbGoBack.Enabled = on;
            tsbGoForward.Enabled = on;
            tsbHistory.Enabled = on;
            tsbPrint.Enabled = on;
        }
        #endregion Private Methods

        #region Private Events
        /// <summary>
        /// Previous opens the Analysis Output window to the results previously generated.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tsbGoBack_Click(object sender, EventArgs e)
        {
            int prevIndex = sessionHistory.CommandHistory.IndexOf(currentUri.AbsoluteUri);
            Navigate(sessionHistory.CommandHistory[prevIndex - 1]);
        }

        /// <summary>
        /// Next opens the Analysis Output window to the next set of results in the Analysis Output window.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tsbGoForward_Click(object sender, EventArgs e)
        {
            int prevIndex = sessionHistory.CommandHistory.IndexOf(currentUri.AbsoluteUri);
            Navigate(sessionHistory.CommandHistory[prevIndex + 1]);
        }

        /// <summary>
        /// Last opens the Analysis Output window to the last output in the session.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tsbGoToLast_Click(object sender, EventArgs e)
        {
            Navigate(sessionHistory.CommandHistory[sessionHistory.CommandHistory.Count - 1]);
        }
        /// <summary>
        /// Print sends the current output to the designated printer.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tsbPrint_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowPrintDialog();
            
            //webBrowser1.Print();
        }
        /// <summary>
        /// History opens the Session History window.
        /// All programs run in the current session are displayed and can be accessed using the hyperlink names.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tsbHistory_Click(object sender, EventArgs e)
        {
            Navigate(sessionHistory.HistoryFile);
        }
        /// <summary>
        /// Open displays the Browse window and allows previous output results to be opened in the Analysis Output window.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tsbOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            Configuration config = Configuration.GetNewInstance();
            // Initialize Open File Dialog.
           //openFileDlg.AutoUpgradeEnabled = true; //For Windows Vista & 7
            openFileDlg.Title = "Browse";
            if (Directory.Exists(config.Directories.Output))
                openFileDlg.InitialDirectory = config.Directories.Output;
            else
                openFileDlg.InitialDirectory = "c:\\";

            openFileDlg.Filter = "HTML files|*.htm;*.html|XML Files|*.xml|Picture Files|*.gif;*.jpeg;*.jpg;*.tif;*.bmp;*.png";
            openFileDlg.FilterIndex = 0;
            openFileDlg.RestoreDirectory = true;

            // Use Open File Dialog.
            if (openFileDlg.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(openFileDlg.FileName))
                    Navigate(openFileDlg.FileName);
            }
        }
        /// <summary>
        /// Bookmark displays the Bookmark dialog box,
        /// which allows you to enter the identification for the bookmark.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tsbBookmark_Click(object sender, EventArgs e)
        {
            Dialogs.BookmarkDialog bookmarkDlg = new Dialogs.BookmarkDialog((Epi.Windows.Analysis.Forms.AnalysisMainForm)MainForm);
            if (bookmarkDlg.ShowDialog(this) == DialogResult.OK)
                sessionHistory.Add("Bookmark", webBrowser1.Url, bookmarkDlg.Bookmark);
        }

        /// <summary>
        /// Document Title Changed event.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void webBrowser1_DocumentTitleChanged(object sender, System.EventArgs e)
        {
           // AdjustDocumentTitle();
            //Invoked the AdjustDocumentTitle() method by delegate 
            Invoke(new delegateAdjustDocTitle(AdjustDocumentTitle));
            
        }

        private void AdjustDocumentTitle()
        {
            if (webBrowser1.Url.IsFile)
            {
                FileInfo fileInfo = new FileInfo(webBrowser1.Url.LocalPath);
                this.Text = webBrowser1.DocumentTitle + StringLiterals.COLON + fileInfo.Name;
            }
            else
            {
                this.Text = "Analysis Output: " + webBrowser1.DocumentTitle;
            }

            ModifyTitleWithHeader();
        }

        private void ModifyTitleWithHeader()
        {
            if (this.HeaderLevels.ContainsKey(1))
            {
                HeaderLevel levelOne = this.HeaderLevels[1];
                if (!String.IsNullOrEmpty(levelOne.Text))
                {
                    if (levelOne.ShouldAppend)
                    {
                        this.Text += levelOne.Text;
                    }
                    else
                    {
                        this.Text = levelOne.Text;
                    }
                }
            }
        }

        /// <summary>
        /// Can Go Forward Changed event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void webBrowser1_CanGoForwardChanged(object sender, System.EventArgs e)
        {
            //tsbGoForward.Enabled = webBrowser1.CanGoForward;
        }

        /// <summary>
        /// Can Go Back Changed event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void webBrowser1_CanGoBackChanged(object sender, System.EventArgs e)
        {
            //tsbGoBack.Enabled = webBrowser1.CanGoBack;
        }

        /// <summary>
        /// Maximize/Restore window button click event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied WindowState event parameters</param>
        private void tsbWindowState_Click(object sender, EventArgs e)
        {
            bool Restore = tsbWindowState.Text.Equals("Restore");
            if (MainForm is AnalysisMainForm)
            {
                ((AnalysisMainForm)MainForm).ToggleToolWindows(Restore);
            }
            if (Restore)
            {
                tsbWindowState.Text = "Maximize";
            }
            else
            {
                tsbWindowState.Text = "Restore";
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            tsbHistory.Enabled = !(sessionHistory.CommandHistory.Count == 0);

            if (!String.IsNullOrEmpty(e.Url.Fragment))
            {
                currentUri = e.Url;
                tsbGoBack.Enabled = !(sessionHistory.CommandHistory.IndexOf(currentUri.AbsoluteUri) <= 0);
                tsbGoForward.Enabled = !(sessionHistory.CommandHistory.IndexOf(currentUri.AbsoluteUri) == (sessionHistory.CommandHistory.Count - 1));
                tsbGoToLast.Enabled = !(sessionHistory.CommandHistory.IndexOf(currentUri.AbsoluteUri) == (sessionHistory.CommandHistory.Count - 1));
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //webBrowser1.Refresh(WebBrowserRefreshOption.Completely);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.Equals(sessionHistory.HistoryFile))
            {
            }
            else
            {
            }
        }
        #endregion Private Events
        #endregion Private Members

        #region IAnalysisCheckCode Members

        /// <summary>
        /// Displays output from checkcode engine
        /// </summary>
        /// <param name="pDisplayArgs"></param>
        public void Display(Dictionary<string,string> pDisplayArgs)
        {
            Rule_Context Context = this.mainForm.EpiInterpreter.Context;
            StringBuilder htmlBuilder = new StringBuilder();
            string fileName = String.Empty;
            string tableName = String.Empty;
            string relateName = String.Empty;
            string htmlOutput = String.Empty;
            int rowCount = 0;
            DataTable table;
            if(pDisplayArgs.ContainsKey("COMMANDNAME"))
            {
                switch(pDisplayArgs["COMMANDNAME"])
                {
                    case CommandNames.DEFINE:
                        //table = Context.DataSet.Tables["datasource"];
                        //Context.ReadDataSource(table);
                        break;

                    case CommandNames.READ:

                        if (pDisplayArgs.ContainsKey("FILENAME"))
                        {
                            fileName = pDisplayArgs["FILENAME"];
                        }
                        if (pDisplayArgs.ContainsKey("TABLENAME"))
                        {
                            tableName = pDisplayArgs["TABLENAME"];
                        }
                        if (pDisplayArgs.ContainsKey("ROWCOUNT"))
                        {
                            rowCount = int.Parse(pDisplayArgs["ROWCOUNT"]);
                        }

                        htmlOutput = DisplayRenderer.RenderHtml(this.mainForm.EpiInterpreter.Context, CommandNames.READ, fileName, tableName, rowCount);
                        this.SendToOutput(String.Empty, htmlOutput, true);
                        //table = Context.DataSet.Tables["datasource"];
                        //Context.ReadDataSource(table);
                        break;

                    case "RecordSet":
                        if (pDisplayArgs.ContainsKey("SQL"))
                        {
                            tableName = pDisplayArgs["SQL"];
                        }
                        if (pDisplayArgs.ContainsKey("ROWCOUNT"))
                        {
                            rowCount = int.Parse(pDisplayArgs["ROWCOUNT"]);
                        }

                        htmlOutput = DisplayRenderer.RenderHtml(this.mainForm.EpiInterpreter.Context, CommandNames.READ, fileName, tableName, rowCount);
                        this.SendToOutput(String.Empty, htmlOutput, true);
                        //table = Context.DataSet.Tables["datasource"];
                        //Context.ReadDataSource(table);

                        break;

                    case "Define Connection":
                        htmlBuilder = new StringBuilder();
                        htmlBuilder.Append("<b>Connection:</b>&nbsp;");
                        htmlBuilder.Append(pDisplayArgs["Connection"]);
                        this.SendToOutput(pDisplayArgs["COMMANDTEXT"], htmlBuilder.ToString(), false);
                        break;
                    
                    case CommandNames.RELATE:
                        if (pDisplayArgs.ContainsKey("FILENAME"))
                        {
                            fileName = pDisplayArgs["FILENAME"];
                        }
                        if (pDisplayArgs.ContainsKey("TABLENAME"))
                        {
                            tableName = pDisplayArgs["TABLENAME"];
                        }
                        if (pDisplayArgs.ContainsKey("ROWCOUNT"))
                        {
                            rowCount = int.Parse(pDisplayArgs["ROWCOUNT"]);
                        }

                        htmlOutput = DisplayRenderer.RenderHtml(Context, CommandNames.RELATE, fileName, tableName, rowCount);
                        this.SendToOutput(string.Empty, htmlOutput, true);
                        break;

                    case CommandNames.GRAPH:
                        htmlBuilder = new StringBuilder();
                        htmlBuilder.Append(pDisplayArgs["DATA"]);
                        this.SendToOutput(pDisplayArgs["COMMANDTEXT"], htmlBuilder.ToString(), false);
                        break;

                    case CommandNames.LIST:
                        htmlBuilder = new StringBuilder();
                        htmlBuilder.Append(pDisplayArgs["DATA"]);
                        this.SendToOutput(pDisplayArgs["COMMANDTEXT"], htmlBuilder.ToString(), false);
                        
                        break;
                    case CommandNames.MERGE:
                        htmlBuilder = new StringBuilder();

                        htmlBuilder.Append("<hr/><table><tr><th colspan=2>MERGE</th></tr>");

                        foreach(System.Collections.Generic.KeyValuePair<string,string> E in pDisplayArgs)
                        {
                            htmlBuilder.Append(string.Format("<tr><td><b>{0}</b></td><td>{1}</td></tr>",E.Key,E.Value));
                        
                        }
                        htmlBuilder.Append("</table>");
                        this.SendToOutput(String.Empty, htmlBuilder.ToString(), false);
                        
                        break;

                    case CommandNames.WRITE:
                        htmlBuilder = new StringBuilder();

                        htmlBuilder.Append("<hr/><table><tr><th colspan=2>WRITE</th></tr>");

                        foreach (System.Collections.Generic.KeyValuePair<string, string> E in pDisplayArgs)
                        {
                            htmlBuilder.Append(string.Format("<tr><td><b>{0}</b></td><td>{1}</td></tr>", E.Key, E.Value));

                        }
                        htmlBuilder.Append("</table>");
                        this.SendToOutput(String.Empty, htmlBuilder.ToString(), false);
                        
                        break;
                    case CommandNames.FREQ:
                        htmlBuilder = new StringBuilder();
                        //htmlBuilder.Append("<p>&nbsp;</p><hr/> Freq ");
                        //htmlBuilder.Append(pDisplayArgs["IDENTIFIERLIST"]);
                        htmlBuilder.Append(pDisplayArgs["HTMLRESULTS"]);
                        this.SendToOutput(pDisplayArgs["COMMANDTEXT"], htmlBuilder.ToString(), false);
                        break;

                    case CommandNames.MEANS:
                        htmlBuilder = new StringBuilder();
                        //htmlBuilder.Append("<p>&nbsp;</p><hr/> Freq ");
                        //htmlBuilder.Append(pDisplayArgs["IDENTIFIERLIST"]);
                        htmlBuilder.Append(pDisplayArgs["HTMLRESULTS"]);
                        this.SendToOutput(pDisplayArgs["COMMANDTEXT"], htmlBuilder.ToString(), false);
                        break;
                    
                    case CommandNames.TABLES:
                        DataSet ds = Context.DataInfo.GetDataSet2x2(Context, pDisplayArgs["parameter1"], pDisplayArgs["parameter2"]);
                        if (ds != null && ds.Tables.Count>0)
                        {
                            htmlOutput = DisplayRenderer.TableDataHTML(pDisplayArgs["parameter1"], pDisplayArgs["parameter2"], ds.Tables[0], ds.Tables[0].Columns.Count);
                            this.SendToOutput(pDisplayArgs["COMMANDTEXT"], htmlOutput, false);
                            
                        }
                        break;
                    case CommandNames.SELECT:
                        if (Context.CurrentRead != null)
                        {
                            htmlOutput = DisplayRenderer.RenderHtml(Context, CommandNames.SELECT, Context.CurrentRead.File, Context.CurrentRead.Identifier, Context.GetOutput().Count);
                            this.SendToOutput(String.Empty, htmlOutput, false);
                            
                        }
                        break;
                    case CommandNames.SORT:
                        if (Context.CurrentRead != null)
                        {
                            htmlOutput = DisplayRenderer.RenderHtml(Context, CommandNames.SORT, Context.CurrentRead.File, Context.CurrentRead.Identifier, Context.GetOutput().Count);
                            this.SendToOutput(String.Empty, htmlOutput, false);
                            
                        }
                        break;

                    case CommandNames.TYPEOUT:
                        this.SendToOutput(pDisplayArgs["COMMANDTEXT"], pDisplayArgs["DATA"], false);
                        break;

                    case CommandNames.SUMMARIZE:
                    case CommandNames.DISPLAY:
                    case CommandNames.REGRESS:
                    case CommandNames.LOGISTIC:
                    case CommandNames.COXPH:
                    case CommandNames.KMSURVIVAL:
                    case CommandNames.DELETE:
                    case CommandNames.UNDELETE:
                    case CommandNames.MATCH:
                        htmlBuilder = new StringBuilder();
                        htmlBuilder.Append(pDisplayArgs["HTMLRESULTS"]);
                        this.SendToOutput(pDisplayArgs["COMMANDTEXT"], htmlBuilder.ToString(), false);
                        break;

                    case CommandNames.HEADER:
                        int levelNumber = Convert.ToInt32(pDisplayArgs["LEVELNUMBER"]);
                        HeaderLevel newLevel = new HeaderLevel(levelNumber);
                        bool result = false;

                        bool.TryParse(pDisplayArgs["SHOULDRESET"], out result);
                        if (result == true)
                        {
                            newLevel.Reset();
                        }
                        else
                        {
                            newLevel.Text = pDisplayArgs["TEXT"];
                            newLevel.Color = pDisplayArgs["COLOR"];
                            newLevel.Size = pDisplayArgs["SIZE"];

                            if (bool.TryParse(pDisplayArgs["SHOULDAPPEND"], out result))
                            {
                                newLevel.ShouldAppend = result;
                            }

                            if (bool.TryParse(pDisplayArgs["SHOULDBOLD"], out result))
                            {
                                newLevel.ShouldBold = result;
                            }

                            if (bool.TryParse(pDisplayArgs["SHOULDITALICIZE"], out result))
                            {
                                newLevel.ShouldItalicize = result;
                            }

                            if (bool.TryParse(pDisplayArgs["SHOULDUNDERLINE"], out result))
                            {
                                newLevel.ShouldUnderline = result;
                            }
                        }
                        
                        if (this.HeaderLevels.ContainsKey(levelNumber))
                        {
                            this.HeaderLevels[levelNumber] = newLevel;
                        }
                        else
                        {
                            this.HeaderLevels.Add(levelNumber, newLevel);
                        }

                        if (newLevel.LevelNumber == 1)
                        {
                            Invoke(new delegateAdjustDocTitle(AdjustDocumentTitle));
                        }

                        string displayText = pDisplayArgs["TEXT"];

                        if (newLevel.ShouldBold)
                        {
                            displayText = HTML.Bold(displayText);
                        }

                        if (newLevel.ShouldItalicize)
                        {
                            displayText = HTML.Italics(displayText);
                        }

                        if (newLevel.ShouldUnderline)
                        {
                            displayText = HTML.Underline(displayText);
                        }
                        int size = 0;

                        if (int.TryParse(newLevel.Size, out size))
                        {
                            displayText = HTML.Tag(displayText, "font", "color", newLevel.Color, "size", newLevel.Size);
                        }

                        this.SendToOutput("", displayText, false);
                        break;
                }
            }
        }

        /// <summary>
        /// Displays simple dialog box
        /// </summary>
        /// <param name="pTextPrompt"></param>
        /// <param name="pTitleText"></param>
        public void Dialog(string pTextPrompt, string pTitleText)
        {
            //MessageBox.Show(this, pTextPrompt, pTitleText);
            MsgBox.Show(pTextPrompt, pTitleText);
        }

        public void ShowGridTable(List<DataRow> dt, List<string> identifierList, Epi.View epiView)
        {
            Epi.Analysis.Dialogs.GridListDialog dialog = new Epi.Analysis.Dialogs.GridListDialog();

            if (epiView != null)
            {
                dialog.EpiView = epiView;
            }
            dialog.DataSource = dt;
            dialog.IdentifierList = identifierList;
            dialog.Validate();            
            dialog.ShowDialog();
        }

        /// <summary>
        /// Displays YesNo dialog box
        /// </summary>
        /// <param name="pTextPrompt">Text prompt</param>
        /// <param name="pVariable">Variable</param>
        /// <param name="pListType">List Type</param>
        /// <param name="pTitleText">Title Text</param>
        public void Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText)
        {
            DialogResult result = MsgBox.Show(pTextPrompt, pTitleText, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                pVariable = true;
            }
            else
            {
                pVariable = false;
            }
        }

        /// <summary>
        /// Dialog
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="mask"></param>
        /// <param name="modifier"></param>
        /// <param name="input"></param>
        public bool Dialog(string text, string caption, string mask, string modifier, ref object input)
        {
            bool ret = false;

            if (modifier.ToUpper().Equals("READ") || modifier.ToUpper().Equals("WRITE"))
            {
                string filePath;
                if (TryGetFileDialog((string)input, caption, modifier.ToUpper().Equals("READ"), out filePath))
                {
                    ret = true;
                    input = filePath;
                }
            }
            else
            {
                Epi.Windows.Dialogs.InputDialog dialog = new Epi.Windows.Dialogs.InputDialog(text, caption, mask, input);
                DialogResult result = dialog.ShowDialog();
                input = dialog.Input;
                ret = result == DialogResult.OK ? true : false;
            }
            return ret;
        }

        /// <summary>
        /// Displays open file dialog
        /// </summary>
        /// <param name="filter">The filter used by the dialog</param>
        /// <param name="caption">The caption of the dialog - aka title </param>
        /// <param name="isReadOnly">Sets the CheckFileExists property for the OpenFileDialog</param>
        /// <param name="filePath">The file path returned by the dialog</param>
        public bool TryGetFileDialog(string filter, string caption, bool isReadOnly, out string filePath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = caption;
            openFileDialog.ShowHelp = false;
            filter = filter.Replace("\"","");
            filter = filter.Trim();
            openFileDialog.Filter = filter == null ? string.Empty : filter;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = isReadOnly;
            DialogResult dialogResult = openFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                filePath = openFileDialog.FileName.Trim();
                return true;
            }
            else
            {
                filePath = string.Empty;
                return false;
            }
        }


        public void DisplayStatusMessage(Dictionary<string, string> pStatusArgs)
        {
            ((AnalysisMainForm)mainForm).UpdateStatus(pStatusArgs["status"]);
        }


        /// <summary>
        /// Quits program
        /// </summary>
        public void Quit()
        {
            this.mainForm.Close();
        }

        /// <summary>
        /// Executes checkcode
        /// </summary>
        /// <param name="command">String of a PGM</param>
        public void RunProgram(string command)
        {
            ((AnalysisMainForm)mainForm).RunCommand(command);
        }

        /// <summary>
        /// Sets or resets the output file for RouteOut and CloseOut
        /// </summary>
        /// <param name="fileName">File to send output to</param>
        /// <param name="isReplaceRouteOut"></param>
        /// <param name="useRouteOut">Use routeout or reset to default output file</param>
        public void ChangeOutput(string fileName, bool isReplaceRouteOut, bool useRouteOut)
        {
            this.isReplaceRouteOut = isReplaceRouteOut;


            if (isReplaceRouteOut)
            {
                if (!String.IsNullOrEmpty(fileName))
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
            }


            if (useRouteOut)
            {
                //SetOutputFile(fileName);
                routeoutFile = fileName;
            }
            else
            {
                //SetOutputFile(string.Empty);
                routeoutFile = string.Empty;
            }
        }

        /// <summary>
        /// Prints contents of output file or specified file
        /// </summary>
        /// <param name="fileName"></param>
        public void Printout(string fileName)
        {
            using (System.Diagnostics.Process proc = new System.Diagnostics.Process())
            {
                if (String.IsNullOrEmpty(fileName))
                {
                    proc.StartInfo.FileName = outputFile;
                }
                else
                {
                    proc.StartInfo.FileName = fileName.Trim('\'');
                }
                proc.StartInfo.Verb = "Print";
                proc.Start();
            }
        }

        public event BeginBusyEventHandler IndeterminateTaskStarted;
        public event EndBusyEventHandler IndeterminateTaskEnded;
        private Epi.Analysis.Dialogs.WaitDialog waitDialog;

        public void ShowWaitDialog(string message)
        {
            try
            {
                waitDialog = new Epi.Analysis.Dialogs.WaitDialog();
                waitDialog.Prompt = message;
                waitDialog.Show();
            }
            catch (Exception ex)
            {
                //
            }
        }

        public void HideWaitDialog()
        {
            try
            {
                if (waitDialog != null)
                {
                    waitDialog.Close();
                }
            }
            catch (Exception ex)
            {
                //
            }
        }

        public void ReportIndeterminateTaskStarted(string message)
        {
            if (IndeterminateTaskStarted != null)
            {
                IndeterminateTaskStarted(message);
            }
        }

        public void ReportIndeterminateTaskEnded()
        {
            if (IndeterminateTaskEnded != null)
            {
                IndeterminateTaskEnded();
            }
        }

        #endregion

        private void OutputEventChangedHandler(object s, OutputEventArg arg)
        {
            MethodInvoker invoker = new MethodInvoker(delegate()
            {
                switch (arg.OutputEventType)
                {
                    case OutputEventArg.OutputEventArgEnum.DialogSimple: //Dialog(string pTextPrompt, string pTitleText)
                        Dialog(arg.TextPrompt, arg.TitleText);
                        break;
                    case OutputEventArg.OutputEventArgEnum.DialogInputList: //Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText)
                        Dialog(arg.TextPrompt, ref arg.Variable, arg.ListType, arg.TitleText);
                        break;
                    case OutputEventArg.OutputEventArgEnum.DialogInput: //Dialog(string text, string caption, string mask, string modifier, ref object input)
                        arg.FunctionResult = Dialog(arg.Text, arg.Caption, arg.Mask, arg.Modifier, ref arg.Input);
                        break;
                    case OutputEventArg.OutputEventArgEnum.Display: //Display(Dictionary<string, string> pDisplayArgs)
                        Display(arg.DisplayArgs);
                        break;
                    case OutputEventArg.OutputEventArgEnum.DisplayStatusMessage: //DisplayStatusMessage(Dictionary<string, string> pStatusArgs)
                        DisplayStatusMessage(arg.StatusArgs);
                        break;
                    case OutputEventArg.OutputEventArgEnum.TryGetFileDialog: //TryGetFileDialog(string filter, string caption, bool isReadOnly, out string filePath)
                        arg.FunctionResult = TryGetFileDialog(arg.Filter, arg.Caption, arg.IsReadOnly, out arg.FilePath);
                        break;
                    case OutputEventArg.OutputEventArgEnum.SetOutputFile:  //SetOutputFile(string fileName)
                        SetOutputFile(arg.FileName);
                        break;
                    case OutputEventArg.OutputEventArgEnum.ChangeOutput:  //ChangeOutput(string fileName, bool isReplace, bool useRouteOut)
                        ChangeOutput(arg.FileName, arg.IsReplace, arg.UseRouteOut);
                        break;
                    case OutputEventArg.OutputEventArgEnum.Printout: //Printout(string fileName)
                        Printout(arg.FileName);
                        break;
                    case OutputEventArg.OutputEventArgEnum.RunProgram: //RunProgram(string command)
                        RunProgram(arg.Command);
                        break;
                    case OutputEventArg.OutputEventArgEnum.Quit: //Quit()
                        Quit();
                        break;
                    case OutputEventArg.OutputEventArgEnum.ShowWaitDialog: //ShowWaitDialog(string message)
                        ShowWaitDialog(arg.Message);
                        break;
                    case OutputEventArg.OutputEventArgEnum.HideWaitDialog: //HideWaitDialog()
                        HideWaitDialog();
                        break;
                    case OutputEventArg.OutputEventArgEnum.ReportIndeterminateTaskStarted: //ReportIndeterminateTaskStarted(string message);
                        ReportIndeterminateTaskStarted(arg.Message);
                        break;
                    case OutputEventArg.OutputEventArgEnum.ReportIndeterminateTaskEnded:  //ReportIndeterminateTaskEnded()
                        ReportIndeterminateTaskEnded();
                        break;
                    //case OutputEventArg.OutputEventArgEnum.ShowGridTable://ShowGridTable(List<System.Data.DataRow> table, List<string> identifierList)
                    //    ShowGridTable(arg.Table, arg.IdentifierList); 
                    //    break;
                    case OutputEventArg.OutputEventArgEnum.ShowGridTable://ShowGridTable(List<System.Data.DataRow> table, List<string> identifierList)
                        ShowGridTable(arg.Table, arg.IdentifierList, arg.EpiView);
                        break;

                }
            }
            );
            
            this.Invoke(invoker);

        }

    }

    /// <summary>
    /// Display Renderer class
    /// </summary>
    public static class DisplayRenderer
    {
        /// <summary>
        /// Renders the HTML
        /// </summary>
        /// <param name="pContext">Rule_Context</param>
        /// <param name="commandName">Command Name</param>
        /// <param name="fileName">File Name</param>
        /// <param name="tableName">Table Name</param>
        /// <param name="rowCount">Row Count</param>
        /// <returns></returns>
        public static string RenderHtml(Rule_Context pContext, string commandName, string fileName, string tableName, int rowCount)
        {

            StringBuilder sb = new StringBuilder();
            Epi.DataSets.Config.SettingsRow settings = Configuration.GetNewInstance().Settings;

            if (pContext.CurrentRead != null && pContext.CurrentRead.RelatedTables != null)
            {
                sb.Append(HTML.Italics(SharedStrings.CURRENT_VIEW + ":&nbsp;"));
                if (fileName.ToLower().Contains("password="))
                {
                    sb.Append(HTML.Bold(tableName));
                }
                else
                {
                    sb.Append(HTML.Bold(String.Format("{0}:{1}", fileName.Trim(new char[] { '\'' }), tableName)));
                }

                foreach (string table in pContext.CurrentRead.RelatedTables)
                {
                    sb.Append(HTML.Tag("br"));
                    sb.Append(HTML.Italics("&nbsp&nbsp&nbsp&nbspRelate:&nbsp;"));
                    sb.Append(HTML.Bold(table));
                }
            }
            else
            {
                sb.Append(HTML.Italics(SharedStrings.CURRENT_VIEW + ":&nbsp;"));
                if (pContext.CurrrentConnection.ToLower().Contains("password="))
                {
                    sb.Append(String.Format("RecordSet:&nbsp;&nbsp;<b>{0}</b>", tableName));
                }
                else
                {
                    sb.Append(String.Format("<b>{0}</b><br/>RecordSet:&nbsp;&nbsp;<b>{1}</b>", pContext.CurrrentConnection.Trim(new char[] { '\'' }), tableName));
                }
            }
            if (pContext.SelectString.ToString() != String.Empty)
            {
                sb.Append(HTML.Tag("br"));
                sb.Append(HTML.Italics("Selection:&nbsp;&nbsp;"));
                sb.Append("&nbsp;");
                sb.Append(HTML.Bold(EpiExpression(pContext, pContext.SelectString.ToString())));
            }
            //Defect 1228 fixed by this change to describe SortExpression
            if (pContext.SortExpression.ToString() != string.Empty)
            {
                sb.Append(HTML.Tag("br"));
                sb.Append(HTML.Italics("Sort By:&nbsp;&nbsp;"));
                sb.Append(HTML.Bold(EpiExpression(pContext, pContext.SortExpression.ToString())));
            }
            sb.Append(HTML.Tag("br"));
            sb.Append(HTML.Italics(SharedStrings.RECORD_COUNT + ":&nbsp;&nbsp;"));
            sb.Append(HTML.Bold(rowCount.ToString()));

            if (pContext.CurrentRead == null)
            {
                sb.Append("&nbsp;&nbsp;&nbsp;");
            }
            else
            {
                string scope = string.Empty;
                switch (settings.RecordProcessingScope)
                {
                    case 1:
                        scope = SharedStrings.DELETED_RECORDS_EXCLUDED;
                        break;
                    case 2:
                        scope = SharedStrings.DELETED_RECORDS_ONLY;
                        break;
                    default:
                        scope = SharedStrings.DELETED_RECORDS_INCLUDED;
                        break;
                }
                sb.Append("&nbsp;");
                sb.Append(HTML.Italics("(" + scope + ")&nbsp;&nbsp;&nbsp;"));
            }
            sb.Append(HTML.Italics("Date:"));
            sb.Append("&nbsp;&nbsp;");
            sb.Append(HTML.Bold(DateTime.Now.ToString()));
            sb.Append(HTML.Tag("br"));
            sb.Append(HTML.Tag("br"));


            return sb.ToString();
        }

        private static bool IsBoolean(IVariable var)
        {
            return (var.DataType == DataType.Boolean || var.DataType == DataType.YesNo);
        }

        private static bool IsBoolean(Rule_Context pContext, string name)
        {
            return IsBoolean(pContext.MemoryRegion.GetVariable(name));
        }

        private static string RepresentationOfValue(string val, bool isBoolean)
        {
            Configuration config = Configuration.GetNewInstance();

            if (isBoolean)

            {
                return (val == "0") ? config.Settings.RepresentationOfNo : config.Settings.RepresentationOfYes;
            }
            else
            {
                return val;
            }
        }

        /// <summary>
        /// Table Heading HTML
        /// </summary>
        /// <param name="pContext">Rule_Context</param>
        /// <param name="distinct">DataTable</param>
        /// <param name="outcome">Outcome variable</param>
        /// <param name="exposure">Exposure variable</param>
        /// <returns></returns>
        public static string TableHeadingHTML(Rule_Context pContext, DataTable distinct, string outcome, string exposure)
        {
            Configuration config = Configuration.GetNewInstance();
            StringBuilder sb = new StringBuilder();
            IMemoryRegion module = pContext.MemoryRegion;

            IVariable oVar = module.GetVariable(outcome);
            string outcomeWord = (config.Settings.ShowCompletePrompt) ?
                oVar.PromptText.ToString() : oVar.Name;
            IVariable eVar = module.GetVariable(exposure);
            string exposureWord = (config.Settings.ShowCompletePrompt) ?
                eVar.PromptText.ToString() : eVar.Name.ToString();

            sb.Append("<caption> <b>").Append(outcomeWord).Append("</b></caption>");

            sb.Append("<tr>");
            sb.Append("<th nowrap>").Append(exposureWord).Append("</th>");
            foreach (DataRow row in distinct.Rows)
            {
                foreach (DataColumn col in distinct.Columns)
                {
                    IVariable var = module.GetVariable(col.ColumnName);
                    DataType thisType = var.DataType;
                    bool isBoolean = (thisType == DataType.Boolean || thisType == DataType.YesNo);

                    sb.Append("<th>");
                    if (row[col.ColumnName] == null ||
                        string.IsNullOrEmpty(row[col.ColumnName].ToString()))
                    {
                        sb.Append(config.Settings.RepresentationOfMissing);
                    }
                    else
                    {
                        string val = RepresentationOfValue(row[col.ColumnName].ToString(), isBoolean);
                        sb.Append(val);
                    }
                    sb.Append("</th>");
                }
            }
            sb.Append("<th>TOTAL</th>");
            sb.Append("</tr>");

            return sb.ToString();
        }

        /// <summary>
        /// Table Row HTML
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="col">Column</param>
        /// <param name="rowCount">Row Count</param>
        /// <param name="colCount">Column Count</param>
        /// <param name="isBooleanField">Boolean isBooleanField for display</param>
        /// <returns></returns>
        public static string TableRowHTML(string row, string col, int rowCount, int colCount, bool isBooleanField)     // exposure is going to have to be the word
        {
            StringBuilder sb = new StringBuilder();
            string rowWord = row;
            if (isBooleanField)
            {
                rowWord = RepresentationOfValue(row, true);
            }
            sb.Append("<tr>");
            sb.Append("<td align=right><b>");
            sb.Append(row);
            sb.Append("</b><br>row %<br>col %</td>");

            //foreach()
            //{
            //    sb.Append("<td align=right>");
            //    sb.Append(count).Append("<br>");
            //    double rowPct = ((double)count / (double)rowTotal) * 100;
            //    sb.Append(rowPct.ToString("##.#")).Append("<br>");
            //    double colPct = ((double)count / count) * 100;
            //    sb.Append(colPct.ToString("##.#"));
            //    sb.Append("</td>");
            //}
            sb.Append("<td align=right>").Append(rowCount).Append("<br>100.0<br>100.0").Append("</td>");
            sb.Append("</tr>");
            return sb.ToString();
        }

        /// <summary>
        /// This is for building the HTML for the whole table.
        /// </summary>
        /// <param name="exposureField">Exposure field</param>
        /// <param name="outcomeField">Outcome field</param>
        /// <param name="table2x2">2x2 Table</param>
        /// <param name="colCount">Column Count</param>
        /// <returns>String of HTML</returns>
        public static string TableDataHTML(string exposureField, string outcomeField, DataTable table2x2, int colCount)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, Dictionary<string, int>> exposureList = new Dictionary<string, Dictionary<string, int>>();

            Dictionary<string, int> exposureTotals = new Dictionary<string, int>();
            Dictionary<string, int> outcomeTotals = new Dictionary<string, int>();

            int grandTotal = 0;
            //calulate totals
            foreach (DataRow r in table2x2.Rows)
            {
                string currExposure = r[ColumnNames.EXPOSURE].ToString();
                string currOutcome = r[ColumnNames.OUTCOME].ToString();
                int currCount = (int)r[ColumnNames.COUNT];
                grandTotal += currCount;

                bool isBoolOutcome = false;//(table2x2.Columns[colIndex].GetType() == typeof(bool));
                bool isBoolExposure = false;
                int outcomeCount = currCount;
                int exposureCount = currCount;

                if (isBoolExposure)
                {
                    if (exposureCount == 1)
                    {
                        currExposure += "#TRUE";
                        AddToTotal(exposureTotals, currExposure, exposureCount);
                    }
                    else
                    {
                        currExposure += "#FALSE";
                        exposureCount = 1;
                        AddToTotal(exposureTotals, currExposure, exposureCount);
                    }
                }
                else
                {
                    AddToTotal(exposureTotals, currExposure, exposureCount);
                }

                if (isBoolOutcome)
                {
                    if (outcomeCount == 1)
                    {
                        currOutcome += "#TRUE";
                        AddToTotal(outcomeTotals, currOutcome, outcomeCount);
                    }
                    else
                    {
                        currOutcome += "#FALSE";
                        outcomeCount = 1;
                        AddToTotal(outcomeTotals, currOutcome, outcomeCount);
                    }
                }
                else
                {
                    AddToTotal(outcomeTotals, currOutcome, outcomeCount);
                }
            }

            string[] keys = new string[outcomeTotals.Keys.Count];
            outcomeTotals.Keys.CopyTo(keys,0);

            sb.Append("<table>");
            sb.AppendFormat("<tr><td>&nbsp;</td><td align=center colspan={0}><b>{1}</b></td></tr>", keys.Length, outcomeField);
            sb.Append(HeaderHtml(keys, exposureField));

            StringBuilder rowBuilder = new StringBuilder();
            int exposureTotal=0;
            
            foreach (DataRow row in table2x2.Rows)
            {
                //create new html row
                string exposure = row[0].ToString();
                rowBuilder = new StringBuilder();
                exposureTotal = (int)row[1];
                rowBuilder.Append(RowHtml(exposure));

                for (int colIndex = 2; colIndex < table2x2.Columns.Count; colIndex++)
                {
                    string outcome = table2x2.Columns[colIndex].ColumnName;
                    int currCount = 0;
                    int.TryParse(row[colIndex].ToString(),out currCount);
                    bool isBoolField = (table2x2.Columns[colIndex].GetType() == typeof(bool));

                    if (isBoolField)
                    {
                        if (currCount == 1)
                        {
                            outcome += "#TRUE";
                            int outcomeTotal = outcomeTotals[outcome];
                            rowBuilder.Append(ColumnHtml(currCount, exposureTotal, outcomeTotal));
                        }
                        else
                        {
                            outcome += "#FALSE";
                            currCount = 1;
                            int outcomeTotal = outcomeTotals[outcome];
                            rowBuilder.Append(ColumnHtml(currCount, exposureTotal, outcomeTotal));
                        }
                    }
                    else
                    {
                        //add the next outcome
                        int outcomeTotal = outcomeTotals[outcome];
                        rowBuilder.Append(ColumnHtml(currCount, exposureTotal, outcomeTotal));
                    }
                }

                //add the last exposure info to the row
                rowBuilder.Append(ColumnHtml(exposureTotal, exposureTotal, grandTotal));
                rowBuilder.Append("</tr>");
                sb.Append(rowBuilder.ToString());
            }

            StringBuilder tr = new StringBuilder();
            tr.Append("<tr><td><b><i>Total</i></b><br/>Row %<br/>Col %</td>");
            for (int colIndex = 2; colIndex < table2x2.Columns.Count; colIndex++)
            {
                string currentOutcome = table2x2.Columns[colIndex].ColumnName;
                int colTotal = outcomeTotals[currentOutcome];
                tr.Append(ColumnHtml(colTotal, colTotal, grandTotal));
            }
            tr.Append(ColumnHtml(grandTotal, grandTotal, grandTotal));

            tr.Append("</tr>");

            sb.Append(tr.ToString());
            sb.Append("</table>");
            return sb.ToString();
        }

        private static void AddToTotal(Dictionary<string, int> totals, string key, int count)
        {
            if (totals.ContainsKey(key))
            {
                totals[key] += count;
            }
            else
            {
                totals.Add(key, count);
            }
        }

        /// <summary>
        /// Header HTML
        /// </summary>
        /// <param name="columns">string array of columns</param>
        /// <param name="exposureField">exposure field</param>
        /// <returns>HTML</returns>
        public static string HeaderHtml(string[] columns, string exposureField)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>");
            sb.AppendFormat("<td align=center><b>{0}</b></td>", exposureField);
            for(int i=0; i<columns.Length; i++)
            {
                sb.AppendFormat("<td align=center><b>{0}</b></td>", columns[i]);
            }
            sb.Append("<td align=center><b>Total</b></td>");
            sb.Append("</tr>");
            return sb.ToString();
        }

        /// <summary>
        /// Column HTML
        /// </summary>
        /// <param name="colCount">Column Count</param>
        /// <param name="rowTotal">Row Total</param>
        /// <param name="colTotal">Column Total</param>
        /// <returns>HTML</returns>
        public static string ColumnHtml(int colCount, int rowTotal, int colTotal)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<td align=right>");
            sb.Append(colCount).Append("<br>");
            double rowPct = ((double)colCount / (double)rowTotal) * 100;
            sb.Append(rowPct.ToString("#0.0")).Append("<br>");
            double colPct = ((double)colCount / colTotal) * 100;
            sb.Append(colPct.ToString("#0.0"));
            sb.Append("</td>");
            return sb.ToString();
        }
        
        /// <summary>
        /// Row HTML
        /// </summary>
        /// <param name="rowName">Row Name</param>
        /// <returns>HTML for a row</returns>
        public static string RowHtml(string rowName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr><td align=right><b>");
            sb.Append(rowName);
            sb.Append("</b><br/>");
            sb.Append("Row %");
            sb.Append("<br/>");
            sb.Append("Col %");
            sb.Append("</td>");
            return sb.ToString();
        }

        /// <summary>
        /// Totals HTML
        /// </summary>
        /// <param name="table2x2">2x2 DataTable</param>
        /// <returns>HTML for the totals</returns>
        public static string TotalsHTML(DataTable table2x2)
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }

        /// <summary>
        /// Totals HTML
        /// </summary>
        /// <param name="colCount">Column Count</param>
        /// <param name="rowTotal">Row Total</param>
        /// <returns>HTML</returns>
        public static string TotalsHTML(int colCount, ulong rowTotal)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>");
            sb.Append("<td align=right><b>").Append(SharedStrings.TOTAL);
            sb.Append("</b><br>Row %<br>Col %").Append("</td>");
            for (int i = 0; i < colCount; i++)
            {
                sb.Append("<td align=right>").Append("count").Append(i.ToString());
                sb.Append("<br>pct").Append(i.ToString());
                sb.Append("<br>").Append("100").Append("</td>");
            }
            sb.Append("<td align=right>").Append(rowTotal).Append("<br>100.0<br>100.0</td>");
            sb.Append("</tr>");
            return sb.ToString();
        }

        /// <summary>
        /// Table HTML
        /// </summary>
        /// <param name="pContext">Rule_Context</param>
        /// <param name="outcomeName">Outcome Name</param>
        /// <param name="exposureName">Exposure Name</param>
        /// <returns></returns>
        public static string TableHTML(Rule_Context pContext, string outcomeName, string exposureName)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // Generate the table
                DataSet ds = pContext.DataInfo.GetDataSet2x2(pContext, outcomeName, exposureName);
                sb.Append("<table border align=left>");
                sb.Append(TableHeadingHTML(pContext, ds.Tables["DistinctOutcomes"], outcomeName, exposureName));
                sb.Append(TableDataHTML(outcomeName, exposureName, ds.Tables["Table2x2"], ds.Tables["DistinctOutcomes"].Rows.Count));
                sb.Append("</table>");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new GeneralException(SharedStrings.DATA_TABLE_NOT_CREATED, ex);
            }
        }

        private static string EpiExpression(Rule_Context pContext, string whereClause)
        {
            string expression = string.Empty;
            if (!string.IsNullOrEmpty(whereClause))
            {
                string where = whereClause.Replace("is null", " = (.)").Replace("is not null", " <> (.)");
                bool isBoolean = false;
                int result = 0;
                string s = string.Empty;
                string[] expStrings = where.Split(new char[] { ' ' });

                for (int i = 0; i < expStrings.Length; i++)
                {
                    if (string.IsNullOrEmpty(expStrings[i]))
                    {
                        s = string.Empty;
                    }
                    else if (Regex.IsMatch(expStrings[i], @"[()=]"))
                    {
                        s = expStrings[i];
                    }
                    else if (expStrings[i] == "<>")
                    {
                        s = expStrings[i];
                    }
                    else if (expStrings[i].Equals("and", StringComparison.CurrentCultureIgnoreCase) || expStrings[i].Equals("or", StringComparison.CurrentCultureIgnoreCase))
                    {
                        s = expStrings[i];
                    }
                    else if (!int.TryParse(expStrings[i], out result))        // Maybe it's a variable?
                    {
                        s = expStrings[i].ToString();
                        /*
                        s = expStrings[i];
                        IVariable var = null;
                        try
                        {
                            var = pContext.MemoryRegion.GetVariable(s);
                        }
                        catch
                        {
                            var = null;
                        }
                        if (var != null)                    // It really is a variable
                        {
                            isBoolean = (var.DataType == DataType.Boolean || var.DataType == DataType.YesNo);
                        }
                        else
                        {
                            s = expStrings[i].ToString();
                        }*/
                    }
                    else if (isBoolean && int.TryParse(expStrings[i], out result))
                    {
                        if (result == 0)
                        {
                            s = "(-)";
                        }
                        if (result == 1)
                        {
                            s = "(+)";
                        }
                        isBoolean = false;
                    }
                    else
                    {
                        s = expStrings[i].ToString();
                    }
                    if (!string.IsNullOrEmpty(expression))
                    {
                        expression += " ";
                    }
                    expression += s;
                }

            }
            return expression;
        }




    }
}
