using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Epi;
using Epi.Windows;
using Epi.ImportExport;
using Epi.Windows.ImportExport.Dialogs;

namespace Epi.Windows.ImportExport
{
    /// <summary>
    /// Entry point into ImportExport DLL. This is the only public class in this assembly.
    /// </summary>
    public abstract class UpgradeAssistant
    {
        /// <summary>
        /// Kicks off the Import process.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="parentForm"></param>
        /// <returns></returns>
        public static Project UpgradeEpi2000Project(string filePath, MainForm parentForm)
        {
            Epi.Epi2000.Project sourceProject = new Epi.Epi2000.Project(filePath);
            
            //// Validate the source project ...
            //parentForm.BeginBusy(SharedStrings.VALIDATING_SOURCE_PROJECT);
            //List<string> validationErrors = sourceProject.Validate();
            //parentForm.EndBusy();
            //if ( validationErrors.Count > 0)
            //{
            //    string errorMsg = SharedStrings.SOURCE_PROJECT_CONTAINS_ERRORS + Environment.NewLine + Environment.NewLine;
            //    errorMsg += Util.ToString(validationErrors, Environment.NewLine);
            //    MsgBox.ShowError(errorMsg);
            //    return null;
            //}

            UpgradeProblemDisplayDialog problemDialog = new UpgradeProblemDisplayDialog(sourceProject);
            DialogResult result = problemDialog.ShowDialog();            

            //List<KeyValuePair<ProjectUpgradeProblemType, string>> problems = CheckSourceProjectForProblems(sourceProject);
            if (result == DialogResult.Cancel)
            {
                return null;
            }

            sourceProject.LoadViews();
            
            ProjectUpgradeDialog dialog = new ProjectUpgradeDialog(sourceProject, parentForm);
            DialogResult ImportResult = dialog.ShowDialog();

            if (ImportResult == DialogResult.OK)
            {
                ProjectUpgradeManager upgradeManager;                

                Project targetProject = dialog.Project;

                try
                {
                    parentForm.BeginBusy(SharedStrings.UPGRADING_PROJECT); 
                    //parentForm.BeginBusy(SharedStrings.UPGRADING_PROJECT);

                    upgradeManager = new ProjectUpgradeManager(sourceProject, targetProject);

                    upgradeManager.ImportStarted += new ImportStartedEventHandler(parentForm.UpgradeProjectManager_ImportStarted);
                    upgradeManager.ImportStatus += new ImportStatusEventHandler(parentForm.UpgradeProjectManager_ImportStatus);
                    upgradeManager.ImportEnded += new SimpleEventHandler(parentForm.UpgradeProjectManager_ImportEnded);
                    upgradeManager.ArtifactImported += new EventHandler(parentForm.UpgradeProjectManager_ProgressReportUpdate);
                    //GuiMediator mediator = ((MakeViewMainForm)parentForm).GetMediator();
                    bool upgraded = upgradeManager.Import(/*mediator.Canvas*/);

                    if (upgraded)
                    {
                        return targetProject;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    MsgBox.ShowException(ex);
                }
                finally
                {
                    parentForm.EndBusy();
                }
            }
            return null;
        }
    }
}