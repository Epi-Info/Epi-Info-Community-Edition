using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Epi.Data;

namespace Epi
{
    /// <summary>
    /// Logs application events
    /// </summary>
    public static class Logger
    {
        #region Fields
        private static string logFilePath = string.Empty;
        #endregion Fields

        #region Public Methods

        /// <summary>
        ///  Logs an application message
        /// </summary>
        /// <param name="msg">Application message.</param>
        public static void Log(string msg)
        {
            try
            {
                if (EnsureLogFileExists())
                {
                    TextWriter tw = new StreamWriter(logFilePath, true);
                    tw.WriteLine(msg);
                    tw.Close();
                }
            }
            catch (Exception ex)
            {
                //absorb exception
            }
        }

        /// <summary>
        /// Formats a string and calls Log(...) if in DEBUG mode.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="timeSpan"></param>
        public static void LogBenchmark(string description, TimeSpan timeSpan)
        {
#if (DEBUG)
            string msg = DateTime.Now.ToString() + " | " + description;
            Log(msg + " | " + timeSpan.ToString());
#endif
        }

        /// <summary>
        /// Logs a database query
        /// </summary>
        /// <param name="query"></param>
        public static void Log(Query query)
        {
            // Log the sql statement ...
            Log(query.SqlStatement);
            string paramsString = string.Empty;
            // Log parameters ...
            foreach (QueryParameter param in query.Parameters)
            {
                paramsString += param.ParameterName + "=" + (Util.IsEmpty(param.Value) ? String.Empty:param.Value.ToString()) + ";   ";
            }
            Log(paramsString);
        }

        /// <summary>
        /// Returns the current log file path.
        /// </summary>
        /// <returns></returns>
        public static string GetLogFilePath()
        {
            EnsureLogFileExists();
            return logFilePath;
        }
        
        #endregion Public Methods

        #region Private Methods
        private static bool EnsureLogFileExists()
        {
            Configuration config = Configuration.GetNewInstance();
            
            try
            {
                // If the log file name is not availabe, create it.
                if (string.IsNullOrEmpty(logFilePath))
                {
                    string dateStamp = DateTime.Now.ToString();
                    dateStamp = dateStamp.Replace(StringLiterals.FORWARD_SLASH, StringLiterals.UNDER_SCORE);
                    dateStamp = dateStamp.Replace(StringLiterals.BACKWARD_SLASH, StringLiterals.UNDER_SCORE);
                    dateStamp = dateStamp.Replace(StringLiterals.COLON, StringLiterals.UNDER_SCORE);
                    dateStamp = dateStamp.Replace(StringLiterals.SPACE, StringLiterals.UNDER_SCORE);
                    string logFileName = "EpiInfo_Log_" + dateStamp + ".txt";
                    
                    config = Configuration.GetNewInstance();
                    logFilePath = Path.Combine(config.Directories.LogDir, logFileName);
                }

                if (Directory.Exists(config.Directories.LogDir) == false)
                {
                    Directory.CreateDirectory(config.Directories.LogDir);
                }
                
                // if log file doesn't exist, create the file.
                if (!File.Exists(logFilePath))
                {
                    FileStream stream = File.Create(logFilePath);
                    stream.Close();
                }
                return true;
            }
            catch (Exception)
            {
                // Ignore any exceptions for now. TODO
                return false;
            }
        }
        #endregion Private Methods
    }
}
