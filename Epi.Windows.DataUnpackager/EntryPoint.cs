using System;
using Epi.Windows;
using System.IO;
using System.Windows.Forms;

namespace Epi.Windows.DataUnpackager
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

            if (LoadConfiguration(args))
            {
                Configuration.Environment = ExecutionEnvironment.WindowsApplication;

                ICommandLine commandLine = new CommandLine(args);

                //string titleArgument = commandLine.GetArgument("title");
                string packagePath = commandLine.GetArgument("package");
                string projectPath = commandLine.GetArgument("project");
                string viewName = commandLine.GetArgument("view");
                string autorun = commandLine.GetArgument("autorun");
                string merge = commandLine.GetArgument("merge");

                try
                {
                    Epi.Windows.ImportExport.Dialogs.ImportEncryptedDataPackageDialog M = null;
                    Project destinationProject = null;
                    View destinationView = null;

                    Epi.ImportExport.DataMergeType mergeType = Epi.ImportExport.DataMergeType.UpdateAndAppend;

                    if (!string.IsNullOrEmpty(merge))
                    {
                        switch (merge.ToLowerInvariant())
                        {                            
                            case "1":
                                mergeType = Epi.ImportExport.DataMergeType.UpdateOnly;
                                break;
                            case "2":
                                mergeType = Epi.ImportExport.DataMergeType.AppendOnly;
                                break;
                            default:
                                mergeType = Epi.ImportExport.DataMergeType.UpdateAndAppend;
                                break;
                        }
                    }

                    M = new Epi.Windows.ImportExport.Dialogs.ImportEncryptedDataPackageDialog();

                    if (!string.IsNullOrEmpty(projectPath) && !(string.IsNullOrEmpty(viewName)))
                    {
                        destinationProject = new Project(projectPath);
                        if (destinationProject.Views.Contains(viewName))
                        {
                            destinationView = destinationProject.Views[viewName];
                        }
                    }

                    if (!string.IsNullOrEmpty(packagePath) && destinationView != null)
                    {
                        M = new Epi.Windows.ImportExport.Dialogs.ImportEncryptedDataPackageDialog(destinationView, packagePath, mergeType);
                    }
                    else if(destinationView != null)
                    {
                        M = new Epi.Windows.ImportExport.Dialogs.ImportEncryptedDataPackageDialog(destinationView, mergeType);
                    }

                    if (!string.IsNullOrEmpty(autorun) && autorun.ToLowerInvariant().Equals("true") && M != null)
                    {
                        M.CloseOnFinish = true;
                        M.StartImportPackage();
                    }

                    //if (!string.IsNullOrEmpty(projectPath))
                    //{
                    //    Project p = new Project(projectPath);
                    //    M.FireOpenViewEvent(p.Views[viewName]);
                    //}
                    //else
                    //{
                    if (!M.IsDisposed)
                    {
                        M.Show();
                        if (M.WindowState == FormWindowState.Minimized)
                        {
                            M.WindowState = FormWindowState.Normal;
                        }
                        M.Activate();
                    }
                    //}

                    System.Windows.Forms.Application.Run(M);
                    M = null;
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
