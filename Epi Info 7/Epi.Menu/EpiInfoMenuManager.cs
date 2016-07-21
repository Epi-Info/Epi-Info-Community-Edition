using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi.Windows;

namespace Epi.Menu
{
    /// <summary>
    /// A class designed to be used by 3rd party menu programs.
    /// </summary>
    /// <remarks>
    /// Good overloading of methods has been avoided in the interest of making the API as simple as
    /// possible for non-technical users, of which there may be many.
    /// </remarks>
    public static class EpiInfoMenuManager
    {
        /// <summary>
        /// Opens the Enter module.
        /// </summary>
        public static void OpenEnter() 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            //Enter.EnterMainForm enterMainForm = new Enter.EnterMainForm();
            //System.Windows.Forms.Application.Run(enterMainForm);

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Enter.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;                
                proc.Start();
            } 
        }

        /// <summary>
        /// Opens the Enter module with a specific form.
        /// </summary>
        /// <param name="projectPath">The project that contains the form. Use the path to the PRJ file.</param>
        /// <param name="formName">The name of the form within the project.</param>
        public static void OpenEnterWithForm(string projectPath, string formName) 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Enter.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("/project:\"{0}\" /view:\"{1}\"", projectPath, formName);
                proc.Start();
            } 
        }

        /// <summary>
        /// Opens the Enter module with a specific form and on a specific record by the record's globally unique identifier.
        /// </summary>
        /// <param name="projectPath">The project that contains the form. Use the path to the PRJ file.</param>
        /// <param name="formName">The name of the form within the project.</param>
        /// <param name="guid">The GlobalRecordId of the record to open.</param>
        public static void OpenEnterWithFormAndRecord(string projectPath, string formName, string guid) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens the Enter module with a specific form and on a specific record by the record's unique key value
        /// </summary>
        /// <param name="projectPath">The project that contains the form. Use the path to the PRJ file.</param>
        /// <param name="formName">The name of the form within the project.</param>
        /// <param name="uniquekey">The UniqueKey value of the record to open.</param>
        public static void OpenEnterWithFormAndRecord(string projectPath, string formName, int uniquekey) 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Enter.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("/project:\"{0}\" /view:\"{1}\" /record:\"{2}\"", projectPath, formName, uniquekey.ToString());
                proc.Start();
            } 
        }

        /// <summary>
        /// Opens the Enter module with a specific form and on a new record
        /// </summary>
        /// <param name="projectPath">The project that contains the form. Use the path to the PRJ file.</param>
        /// <param name="formName">The name of the form within the project.</param>
        public static void OpenEnterWithFormAndNewRecord(string projectPath, string formName)
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Enter.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("/project:\"{0}\" /view:\"{1}\" /record:\"{2}\"", projectPath, formName, "*");
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the form designer module.
        /// </summary>
        public static void OpenFormDesigner()
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\MakeView.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                //proc.StartInfo.Arguments = string.Format("/project:\"{0}\" /view:\"{1}\"", projectFilePath, viewName);
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the form designer module with a specific project.
        /// </summary>
        /// <param name="projectPath">The project to open. Use the path to the PRJ file.</param>
        public static void OpenFormDesignerWithProject(string projectPath) 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\MakeView.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("/project:\"{0}\"", projectPath);
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the Dashboard.
        /// </summary>
        public static void OpenDashboard() 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\AnalysisDashboard.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;                
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the Dashboard with a specific canvas file.
        /// </summary>
        /// <param name="canvasPath">The canvas file to open. Use the path to the canvas file.</param>
        public static void OpenDashboardWithCanvas(string canvasPath) 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\AnalysisDashboard.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("\"{0}\"", canvasPath);
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the Dashboard with a specific canvas file and produces output.
        /// </summary>
        /// <param name="canvasPath">The canvas file to open. Use the path to the canvas file.</param>
        /// <param name="htmlPath">The html file to produce.</param>
        public static void OpenDashboardWithCanvasAndCreateOutput(string canvasPath, string htmlPath, bool runMinimized = false)
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\AnalysisDashboard.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" /minimized:\"{2}\"", canvasPath, htmlPath, runMinimized);
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the Dashboard with a specific project set as the data source.
        /// </summary>
        /// <param name="projectPath">The project to open. Use the path to the PRJ file.</param>
        /// <param name="formName">The name of the form to open.</param>
        public static void OpenDashboardWithForm(string projectPath, string formName) 
        {
            Project project = new Project(projectPath);
            if (project.Views.Contains(formName))
            {
                View view = project.Views[formName];
                Epi.Windows.Enter.AnalyticsViewer analyticsViewer = new Epi.Windows.Enter.AnalyticsViewer(null);
                analyticsViewer.Render(view);
                analyticsViewer.Show();
            }
        }

        /// <summary>
        /// Opens the Maps module.
        /// </summary>
        public static void OpenMaps() 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Mapping.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;                
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the Maps module using a given MAP file.
        /// </summary>
        /// <param name="mapPath">The map file to open. Use the path to the MAP file.</param>
        public static void OpenMapsWithMap(string mapPath) 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Mapping.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("\"{0}\"", mapPath);
                proc.Start();
            }
        }

        public static void OpenDataPackager() 
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\DataPackager.exe";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
        }

        public static void OpenDataPackagerWithScript(string scriptPath) 
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\DataPackager.exe";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Arguments = string.Format("/script:\"{0}\"", scriptPath);
            proc.Start();
        }

        public static void OpenDataPackagerWithScript(string scriptPath, bool autoRun, bool smallSize) 
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\DataPackager.exe";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Arguments = string.Format("/script:\"{0}\" /autorun:\"{1}\" /smallsize:\"{2}\" ", scriptPath, autoRun, smallSize);
            proc.Start();
        }

        public static void OpenDataUnpackager()
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\DataUnpackager.exe";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;            
            proc.Start();
        }

        public static void OpenDataUnpackager(string destinationProjectPath, string destinationViewName) 
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\DataUnpackager.exe";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Arguments = string.Format("/project:\"{0}\" /view:\"{1}\"", destinationProjectPath, destinationViewName);
            proc.Start();
        }

        public static void OpenDataUnpackagerWithPackage(string destinationProjectPath, string destinationViewName, string packagePath) 
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\DataUnpackager.exe";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Arguments = string.Format("/package:\"{0}\" /project:\"{1}\" /view:\"{2}\"", packagePath, destinationProjectPath, destinationViewName);
            proc.Start();
        }

        public static void OpenDataUnpackagerWithPackage(string destinationProjectPath, string destinationViewName, string packagePath, bool autorun/*, MergeType mergeType*/) 
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\DataUnpackager.exe";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = commandText;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Arguments = string.Format("/package:\"{0}\" /project:\"{1}\" /view:\"{2}\" /autorun:\"{3}\"", packagePath, destinationProjectPath, destinationViewName, autorun);
            proc.Start();
        }

        /// <summary>
        /// Opens the Classic Analysis module.
        /// </summary>
        public static void OpenAnalysis() 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Analysis.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;                
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the Classic Analysis module and subsequently runs a given PGM.
        /// </summary>
        /// <param name="pgmPath">The pgm file to open. Use the path to the PGM file.</param>
        public static void OpenAnalysisWithPGM(string pgmPath) 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Analysis.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("\"{0}\"", pgmPath);
                proc.Start();
            }
        }

        /// <summary>
        /// Opens the default Epi Info 7 menu.
        /// </summary>
        public static void OpenMenu() 
        {
            // ***************************************************************************************
            // TODO: See about getting rid of shell execute. Shell execute just a placeholder for now.
            // ***************************************************************************************

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\Menu.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
            }
        }

        public static void CreatePermanentVariable(string variableName, int variableType) 
        {
            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.PermanentVariables.Select("Name='" + variableName + "'");
            string variableExpression = string.Empty;
            if (result.Length < 1)
            {
                config.PermanentVariables.AddPermanentVariableRow(
                   variableName,
                   variableExpression ?? "",
                    (int)variableType,
                   config.ParentRowPermanentVariables);
                Configuration.Save(config);
            }
            else if (result.Length == 1)
            {
                ((DataSets.Config.PermanentVariableRow)result[0]).DataValue = variableExpression ?? "";
                ((DataSets.Config.PermanentVariableRow)result[0]).DataType = (int)variableType;
                Configuration.Save(config);
            }
            else
            {
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.ContentsInvalid, "Duplicate permanent variable rows encountered.");
            }
        }

        public static void CreatePermanentVariableWithValue(string variableName, object variableType, object variableValue) { }

        /// <summary>
        /// Sets the value of a permanent variable
        /// </summary>
        /// <param name="variableName">The name of the permanent variable.</param>
        /// <param name="variableValue">The value to set.</param>
        public static void SetPermanentVariable(string variableName, object variableValue) 
        {
            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.PermanentVariables.Select("Name='" + variableName + "'");
            if (result.Length == 1)
            {
                ((DataSets.Config.PermanentVariableRow)result[0]).DataValue = variableValue.ToString() ?? "";
                Configuration.Save(config);
            }            
        }

        /// <summary>
        /// Gets the value of a given permanent variable.
        /// </summary>
        /// <param name="variableName">The name of the permanent variable.</param>
        /// <returns>The value of that variable.</returns>
        public static object GetPermanentVariableValue(string variableName) 
        {
            Configuration config = Configuration.GetNewInstance();

            foreach (Epi.DataSets.Config.PermanentVariableRow row in config.PermanentVariables)
            {
                if (row.Name.ToLowerInvariant().Equals(variableName.ToLowerInvariant()))
                {
                    string dataType = row.DataType.ToString();
                    string dataValue = row.DataValue.ToString();

                    if(string.IsNullOrEmpty(dataValue)) 
                    {
                        return string.Empty;
                    }

                    switch (dataType)
                    {
                        case "1": // numeric
                            double dResult = -1;
                            bool numberParseSuccess = double.TryParse(dataValue, out dResult);
                            if (numberParseSuccess)
                            {
                                return dResult;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case "2": // text
                            return dataValue;
                        case "3": // date/time
                            DateTime dtResult = DateTime.Now;
                            bool dateParseSuccess = DateTime.TryParse(dataValue, out dtResult);
                            if (dateParseSuccess)
                            {
                                return dtResult;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        case "6": // boolean
                            DateTime bResult = DateTime.Now;
                            bool boolParseSuccess = DateTime.TryParse(dataValue, out bResult);
                            if (boolParseSuccess)
                            {
                                return bResult;
                            }
                            else
                            {
                                return string.Empty;
                            }
                        default:
                            return dataValue;
                    }
                }
            }

            return null;
        }
    }
}
