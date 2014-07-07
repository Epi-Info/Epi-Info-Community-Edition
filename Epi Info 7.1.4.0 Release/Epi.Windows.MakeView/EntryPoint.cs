using System;
using Epi.Windows;
using System.Windows.Forms;
using System.IO;

namespace Epi.Windows.MakeView
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

            if (LoadConfiguration())
            { 
                try
                {
                    Configuration.Environment = ExecutionEnvironment.WindowsApplication;
                    string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string executablePath = System.IO.Path.Combine(path, "EpiInfo.exe");
                    Epi.Windows.MakeView.Forms.MakeViewMainForm makeViewMainForm = new Epi.Windows.MakeView.Forms.MakeViewMainForm();
                    ICommandLine commandLine = new CommandLine(args);

                    string titleArgument = commandLine.GetArgument("title");
                    string projectPath = commandLine.GetArgument("project");
                    string viewName = commandLine.GetArgument("view");

                    if (titleArgument != null)
                    {
                        makeViewMainForm.Text = titleArgument;
                    }

                    if (! string.IsNullOrEmpty(projectPath))
                    {
                        makeViewMainForm.LoadViewFromCommandLine(projectPath, viewName);
                    }
                    else
                    {
                        if (!makeViewMainForm.IsDisposed)
                        {
                            makeViewMainForm.Show();
                            if (makeViewMainForm.WindowState == FormWindowState.Minimized)
                            {
                                makeViewMainForm.WindowState = FormWindowState.Normal;
                            }
                            makeViewMainForm.Activate();
                        }

                        if (args.Length > 0 && args[0] is string && System.IO.File.Exists(args[0]))
                        {
                            makeViewMainForm.GetTemplate(args[0]);
                        }
                    }

                    System.Windows.Forms.Application.Run(makeViewMainForm);
                    makeViewMainForm = null;
                }
                catch(Exception ex)
                {
                    MsgBox.ShowException(ex);
                }
            }
        }

        private static bool LoadConfiguration()
        {
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
