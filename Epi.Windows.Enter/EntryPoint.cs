using System;
using Epi.Windows;
using System.IO;
using System.Windows.Forms;
using System.Device.Location;

namespace Epi.Windows.Enter
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

                string titleArgument = commandLine.GetArgument("title");
                string projectPath = commandLine.GetArgument("project");
                string viewName = commandLine.GetArgument("view");
                string recordId = commandLine.GetArgument("record");

                try
                {
                    Epi.Windows.Enter.EnterMainForm mainForm = new Epi.Windows.Enter.EnterMainForm();


                    if (!mainForm.IsDisposed)
                    {
                        mainForm.Show();
                        if (mainForm.WindowState == FormWindowState.Minimized)
                        {
                            mainForm.WindowState = FormWindowState.Normal;
                        }

                        mainForm.Activate();

                        mainForm.Text = string.IsNullOrEmpty(titleArgument) ? mainForm.Text : titleArgument;

                        if (!string.IsNullOrEmpty(projectPath))
                        {
                            Project project = new Project(projectPath);

                            mainForm.CurrentProject = project;

                            if (string.IsNullOrEmpty(recordId))
                            {
                                mainForm.FireOpenViewEvent(project.Views[viewName]);
                            }
                            else
                            {
                                mainForm.FireOpenViewEvent(project.Views[viewName], recordId);
                            }
                        }
                        //--2225
                        Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                        //--

                        GeoCoordinateWatcher locationWatcher;
                        locationWatcher = new GeoCoordinateWatcher();

                        locationWatcher.PositionChanged += (sender, e) =>
                        {
                            GeoCoordinate coordinate = e.Position.Location;
                            mainForm.LastPosition = e.Position;
                            string locationText = "";
                            if(e.Position.Location.IsUnknown == false)
                            {
                                locationText = string.Format("{0:0.0000000},{1:0.0000000},{2:h:mm:ss tt}", coordinate.Latitude, coordinate.Longitude, e.Position.Timestamp.DateTime);
                            }
                                
                            mainForm.UpdateAppSpecificInfo(locationText);
                        };

                        locationWatcher.StatusChanged += (sender, e) =>
                        {
                            if (e.Status == GeoPositionStatus.NoData)
                            {
                                mainForm.LastPosition = null;
                            }
                        };

                        locationWatcher.Start();

                        System.Windows.Forms.Application.Run(mainForm);

                    }

                    mainForm = null;
                }
                catch (Exception baseException)
                {
                    MsgBox.ShowError(string.Format("Error: \n {0}", baseException.ToString()));
                }
            }


        }
        //---2225
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            // Handle exception
            return;
        }
        //---
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