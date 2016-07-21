using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Epi;
using Epi.Windows;

namespace Epi.CommandLine.Config
{
    public delegate void VariableDialogResultHandler(string result);

    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            LoadConfiguration(args);
            if (args.Length > 1)
            {
                if (args[0].ToLowerInvariant().Equals("set"))
                {
                    SetVariable(args);
                }
                if (args[0].ToLowerInvariant().Equals("get"))
                {
                    GetVariable(args);
                }
                if (args[0].ToLowerInvariant().Equals("dialog"))
                {
                    SetVariableViaDialog(args);
                }
            }
        }

        private static void SetVariableViaDialog(string[] args)
        {
            if (args.Length > 2)
            {
                string variableName = args[1];
                string prompt = args[2];
                Epi.PermanentVariable perm;
                DataType dataType;
                if (args.Length == 3)
                {
                    dataType = DataType.Text;
                }
                else
                {
                    dataType = GetDataType(args[3]);
                }
                perm = new PermanentVariable(variableName, dataType);
                try
                {
                    new MemoryRegion().DefineVariable(perm);
                }
                catch (Exception ex)
                {
                    //
                }
                VariableInputDialog dialog = new VariableInputDialog(dataType, prompt);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    perm.Expression = dialog.Result.ToString();
                }
            }
        }

        /// <summary>
        /// SYNTAX: Config get myVar
        /// </summary>
        /// <param name="args">Command line paramters</param>
        private static void GetVariable(string[] args)
        {
            if (args.Length == 2)
            {
                string variableName = args[1];
                try
                {
                    Epi.PermanentVariable perm = (PermanentVariable)new MemoryRegion().GetVariable(variableName);
                    MessageBox.Show(variableName + " = " + perm.Expression, "Epi Info 7", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Empty);
                }
            }
        }

        /// <summary>
        /// SYNTAX: Config set myVar "string value"
        /// </summary>
        /// <param name="args">Command line parameters</param>
        private static void SetVariable(string[] args)
        {
            if (args.Length > 2)
            {
                string variableName = args[1];
                string variableValue = args[2];
                Epi.PermanentVariable perm;
                if (args.Length == 3)
                {
                    perm = new PermanentVariable(variableName, DataType.Text);
                }
                else
                {
                    perm = new PermanentVariable(variableName, GetDataType(args[3]));
                }
                try
                {
                    new MemoryRegion().DefineVariable(perm);
                }
                catch (Exception ex)
                {
                    //
                }
                perm.Expression = variableValue;
            }
        }

        private static DataType GetDataType(string arg)
        {
            if (arg.ToLowerInvariant().Equals("textinput"))
            {
                return DataType.Text;
            }
            else if (arg.ToLowerInvariant().Equals("numeric"))
            {
                return DataType.Number;
            }
            else if (arg.ToLowerInvariant().Equals("dateformat"))
            {
                return DataType.DateTime;
            }
            else if (arg.ToLowerInvariant().Equals("yn"))
            {
                return DataType.Boolean;
            }
            else
            {
                return DataType.Text;
            }
        }

        private static bool LoadConfiguration(string[] args)
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
