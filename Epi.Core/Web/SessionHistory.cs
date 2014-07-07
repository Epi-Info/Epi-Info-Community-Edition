#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
#endregion

namespace Epi.Web
{
    /// <summary>
    /// Keeps a records of URL navigation history during Analysis runtime session.
    /// </summary>
    public class SessionHistory
    {
        #region Public Interface
        #region Constructors
        /// <summary>
        /// Default constructor for SessionHistory()
        /// </summary>
        public SessionHistory()
        {
            ResetOutput();
        }
        #endregion Constructors

        #region Public Enums and Constants
        //
        #endregion Public Enums and Constants

        #region Public Properties
        /// <summary>
        /// Command History
        /// </summary>
        public List<string> CommandHistory
        {
            get
            {
                return commandHistory;
            }
        }
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Append a row with bookmark title, URL, and document title to the session history table.
        /// </summary>
        /// <param name="Time">Bookmark Title</param>
        /// <param name="Command">Uniform Resource Identifier</param>
        /// <param name="DocumentTitle">URL header title</param>
        public void Add(string Time, Uri Command, string DocumentTitle)
        {
            if (File.Exists(HistoryFile))
            {
                string fileName = Command.AbsolutePath;
                if (Command.IsFile)
                {
                    FileInfo file = new FileInfo(Command.AbsolutePath);
                    fileName = file.Name;
                }
                File.AppendAllText(historyFile, HTML.TableRow(HTML.TableCell(Time, "nowrap", "nowrap") +
                    HTML.TableCell(HTML.HyperLink(DocumentTitle, Command.OriginalString)) + HTML.TableCell(fileName)));
                commandHistory.Add(Command.AbsoluteUri);
            }
        }

        /// <summary>
        /// Append a row with time, URL, and document title to the session history table.
        /// </summary>
        /// <param name="Time">Bookmark Time</param>
        /// <param name="Command">Uniform Resource Identifier</param>
        /// <param name="DocumentTitle">URL header title</param>
        public void Add(DateTime Time, Uri Command, string DocumentTitle)
        {
            if (File.Exists(HistoryFile))
            {
                string fileName = Command.AbsolutePath;
                if (Command.IsFile)
                {
                    FileInfo file = new FileInfo(Command.AbsolutePath);
                    fileName = file.Name;
                }
                File.AppendAllText(historyFile, HTML.TableRow(HTML.TableCell(Time.ToShortTimeString(), "nowrap", "nowrap") +
                    HTML.TableCell(HTML.HyperLink(DocumentTitle, Command.OriginalString)) + HTML.TableCell(fileName)));
                commandHistory.Add(Command.AbsoluteUri);
            }
        }

        /// <summary>
        /// Delete Session History webpage.
        /// </summary>
        public void ResetOutput()
        {
            File.Delete(HistoryFile);
            this.historyFile = string.Empty;
        }
        #endregion Public Methods
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
        private const string historyFileName = "IHistory.html";
        #endregion Private Enums and Constants

        #region Private Attributes
        private string historyFile;
        private List<string> commandHistory = new List<string>();
        #endregion Private Attributes

        #region Private Properties
        /// <summary>
        /// Gets the analysis output file
        /// </summary>
        public string HistoryFile
        {
            get
            {
                Configuration config = Configuration.GetNewInstance();
                string OutputPath = config.Directories.Output;
                if (string.IsNullOrEmpty(this.historyFile))
                {
                    if (!Directory.Exists(OutputPath))
                    {
                        Directory.CreateDirectory(OutputPath);
                    }
                    if (!File.Exists(Path.Combine(OutputPath, historyFileName)))
                    {
                        // Get fully-qualified file path and name. Uses same path as Output Window.
                        historyFile = Path.Combine(OutputPath, historyFileName);
                        string historyFileContents = Epi.Resources.ResourceLoader.GetAnalysisSessionHistoryTemplate().Replace("{0}", DateTime.Today.ToLongDateString());
                        // Write new IHistory webpage with text from assembly manifest resource.
                        File.WriteAllText(historyFile, historyFileContents);

                        File.SetAttributes(historyFile, FileAttributes.Normal);
                    }
                    else
                    {

                        if (historyFile == null)
                        {
                            // Get fully-qualified file path and name. Uses same path as Output Window.
                            historyFile = Path.Combine(OutputPath, historyFileName);
                        }
                    }
                }
                return historyFile;
            }
        }
        #endregion Private Properties

        #region Private Methods
        //
        #endregion Private Methods

        #region Private Events
        //
        #endregion Private Events
        #endregion Private Members
    }
}
