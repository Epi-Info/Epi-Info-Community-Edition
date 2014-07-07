﻿using System;
using System.IO;
using System.Windows.Forms;
using Epi.Windows;

namespace Epi.Windows.AnalysisDashboard
{
    class EntryPoint
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();

            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application
                {
                    ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown
                };
            }

            if (LoadConfiguration(args))
            {
                Configuration.Environment = ExecutionEnvironment.WindowsApplication;

                try
                {
                    //string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    //string executablePath = System.IO.Path.Combine(path, "EpiInfo.exe");
                    //string commandLine = "/l:Analysis " + string.Join(" ", args);
                    ICommandLine commandLine = new CommandLine(args);

                    DashboardMainForm M = new DashboardMainForm();
                    string canvasPath = string.Empty;
                    string htmlOutputPath = string.Empty;

                    bool minimized = false;
                    bool.TryParse(commandLine.GetArgument("minimized"), out minimized);

                    for (int i = 0; i < commandLine.ArgumentStrings.Length; i++)
                    {
                        if (commandLine.ArgumentStrings[i].IndexOf(".cvs7", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            canvasPath = commandLine.ArgumentStrings[i].Trim();                            
                        }
                        else if (commandLine.ArgumentStrings[i].IndexOf(".html", StringComparison.CurrentCultureIgnoreCase) >= 0 || commandLine.ArgumentStrings[i].IndexOf(".htm", StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            htmlOutputPath = commandLine.ArgumentStrings[i].Trim();                           
                        }
                    }

                    if (!string.IsNullOrEmpty(htmlOutputPath))
                    {
                        M.SetHTMLOutputPath(htmlOutputPath);
                    }
                    else
                    {
                        minimized = false; // don't hide the window unless HTML will be generated, even if this was explicity set
                    }

                    if (minimized)
                    {
                        M.WindowState = FormWindowState.Minimized;
                    }

                    if (!string.IsNullOrEmpty(canvasPath))
                    {
                        M.OpenCanvas(canvasPath);
                    }                    

                    System.Windows.Forms.Application.Run(M);
                    M = null;

                    /*

                    

                    string titleArgument = commandLine.GetArgument("title");
                    string projectPath = commandLine.GetArgument("project");
                    string viewName = commandLine.GetArgument("view");

                    try
                    {
                        Epi.Windows.Enter.EnterMainForm M = new Epi.Windows.Enter.EnterMainForm();

                        if (!string.IsNullOrEmpty(titleArgument))
                        {
                            M.Text = titleArgument;
                        }

                        if (!string.IsNullOrEmpty(projectPath))
                        {
                            Project p = new Project(projectPath);
                            M.FireOpenViewEvent(p.Views[viewName]);
                        }
                        else
                        {
                            if (!M.IsDisposed)
                            {
                                M.Show();
                                if (M.WindowState == FormWindowState.Minimized)
                                {
                                    M.WindowState = FormWindowState.Normal;
                                }
                                M.Activate();
                            }
                        }

                    
                        M = null;*/
                }
                catch (Exception ex)
                {
                    //MsgBox.ShowError(SharedStrings.WARNING_APPLICATION_RUNNING);
                    MsgBox.ShowError(string.Format("Error: \n {0}", ex.ToString()));
                }
                finally
                {
                }
            }
        }

        private static bool LoadConfiguration(string[] args)
        {
            // TODO: parse command line to load configuration if specified
            string configFilePath = Configuration.DefaultConfigurationPath;

            bool configurationOk = true;
            try
            {
                string directoryName = Path.GetDirectoryName(configFilePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                if (!File.Exists(configFilePath))
                {
                    // create a default configuration file and save it
                    Configuration defaultConfig = Configuration.CreateDefaultConfiguration();
                    Configuration.Save(defaultConfig);
                }

                Configuration.Load(configFilePath);
            }
            catch (ConfigurationException)
            {
                configurationOk = RecoverFromConfigurationError(configFilePath);
            }
            catch (Exception ex)
            {
                configurationOk = ex.Message == "";
            }

            return configurationOk;
        }

        /// <summary>
        /// Restores error-free configuration file
        /// </summary>
        /// <param name="configFilePath">The file path of the configuration file</param>
        /// <returns>Boolean indicating if the configuration file was restored</returns>
        private static bool RecoverFromConfigurationError(string configFilePath)
        {
            DialogResult result = MsgBox.ShowQuestion(SharedStrings.RESTORE_CONFIGURATION_Q, MessageBoxButtons.YesNo);
            if ((result == DialogResult.Yes) || (result == DialogResult.OK))
            {
                if (string.IsNullOrEmpty(configFilePath))
                {
                    throw new ArgumentNullException("configFilePath");
                }

                try
                {
                    if (File.Exists(configFilePath))
                    {
                        // Get the current time and format it to append it to the file name.
                        string timeStamp = DateTime.Now.ToString().Replace(StringLiterals.FORWARD_SLASH, StringLiterals.UNDER_SCORE);
                        timeStamp = timeStamp.Replace(StringLiterals.BACKWARD_SLASH, StringLiterals.UNDER_SCORE);
                        timeStamp = timeStamp.Replace(StringLiterals.COLON, StringLiterals.UNDER_SCORE);
                        timeStamp = timeStamp.Replace(StringLiterals.SPACE, StringLiterals.UNDER_SCORE);


                        string oldConfigFilePath = configFilePath + "." + timeStamp;
                        File.Copy(configFilePath, oldConfigFilePath);
                        File.Delete(configFilePath);
                    }

                    Configuration defaultConfig = Configuration.CreateDefaultConfiguration();
                    Configuration.Save(defaultConfig);
                    Configuration.Load(configFilePath);
                    return true;

                }
                catch (Exception)
                {
                    return false;
                }

            }
            return false;
        }

    }
}
