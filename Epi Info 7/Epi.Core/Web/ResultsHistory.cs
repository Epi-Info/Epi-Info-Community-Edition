using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Epi.Web
{
    /// <summary>
    /// Results History
    /// </summary>
    public class ResultsHistory
    {
        private Configuration config;
        private string historyFile;
        private const string HISTORY_FILE_NAME = "IResults.html";

        /// <summary>
        /// Results History
        /// </summary>
        public ResultsHistory()
        {
            config = Configuration.GetNewInstance();
            if (!File.Exists(this.HistoryFile))
            {
                string historyFileContents = Epi.Resources.ResourceLoader.GetAnalysisResultsHistoryTemplate().Replace("{0}", DateTime.Today.ToLongDateString());
                File.WriteAllText(this.HistoryFile, historyFileContents);
            }
        }

        #region Public Methods
        /// <summary>
        /// Append a row with bookmark title, URL, and document title to the results history table.
        /// </summary>
        /// <param name="Time">Bookmark Title</param>
        /// <param name="Command">Uniform Resource Identifier</param>
        /// <param name="DocumentTitle">URL header title</param>
        public void Add(string Time, Uri Command, string DocumentTitle)
        {
            if (File.Exists(this.HistoryFile))
            {
                string fileName = Command.AbsolutePath;
                if (Command.IsFile)
                {
                    FileInfo file = new FileInfo(Command.AbsolutePath);
                    fileName = file.Name;
                }
                File.AppendAllText(historyFile, HTML.TableRow(HTML.TableCell(Time, "nowrap", "nowrap") +
                    HTML.TableCell(HTML.HyperLink(DocumentTitle, Command.OriginalString)) + HTML.TableCell(fileName)));
            }
        }

        /// <summary>
        /// Append a row with time, URL, and document title to the results history table.
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
            }
        }

        /// <summary>
        /// Delete Results History webpage.
        /// </summary>
        public void ResetOutput()
        {
            File.Delete(this.HistoryFile);
            this.historyFile = String.Empty;
        }
        #endregion Public Methods

        /// <summary>
        /// History File
        /// </summary>
        public string HistoryFile
        {
            get
            {
                return Path.Combine(config.Directories.Output, HISTORY_FILE_NAME);
            }
        }
    }
}
