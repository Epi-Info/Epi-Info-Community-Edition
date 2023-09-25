#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Windows;
using Epi.Windows.Analysis;
using Epi.Analysis;
using Epi.Windows.Analysis.Dialogs;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Windows.Docking;
using Epi.Data.Services;
using Epi.Core.AnalysisInterpreter;


#endregion

namespace Epi.Windows.Analysis.Forms
{

    #region Delegates
    public enum CommandProcessingMode
    {
        Save_And_Execute,
        Save_Only
    }


    /// <summary>
    /// Delegate for command generation
    /// </summary>
    /// <param name="commandSyntax">The syntax generated</param>
    /// <param name="processingMode">The command's processing mode</param>
    public delegate void CommandGenerationEventHandler(string commandSyntax, CommandProcessingMode processingMode);

    #endregion
    
    /// <summary>
    /// The Analysis Command Explorer
    /// </summary>
    public partial class CommandExplorer : DockWindow
    {

        #region Private Members

        private AnalysisWindowsModule module;
        new private Epi.Windows.Analysis.Forms.AnalysisMainForm mainForm;
        private System.ComponentModel.ComponentResourceManager resources;
        private System.ComponentModel.ComponentResourceManager commandResources;
        
        #endregion

        #region Events

        /// <summary>
        /// Occurs when a command generation dialog generates a command syntax
        /// </summary>
        public event CommandGenerationEventHandler CommandGenerated;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CommandExplorer()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainForm"></param>
        public CommandExplorer(AnalysisMainForm mainForm)
        {
            this.mainForm = mainForm;
            this.module = (AnalysisWindowsModule) mainForm.Module;
            InitializeComponent();
            Construct();

            
        }

        #endregion

        #region Private Methods

        private void Construct()
        {
            BuildCommandTree();
        }

        /// <summary>
        /// Redefined Module property
        /// </summary>
        private AnalysisWindowsModule Module
        {
            get
            {
                return this.module;
            }
        }

        /// <summary>
        /// Builds the command tree dynamically
        /// </summary>
        private void BuildCommandTree()
        {
            TreeValueNode node = new TreeValueNode();
            SuspendLayout();
            node.Name = "commandNode";
            node.Text = SharedStrings.ANALYSIS_COMMANDS;
            resources.ApplyResources(node, "commandNode");

            int commandGroupId = 0;
            TreeValueNode commandGroupNode = new TreeValueNode();
            commandGroupNode.Name = "commandGroupNode";
            commandResources.ApplyResources(commandGroupNode, "commandGroupNode");

            node.Value = 0;
            node.ImageIndex = 72;
            node.SelectedImageIndex = 72;
            tvCommands.Nodes.Add(node);

            DataTable commandGroups = AppData.Instance.CommandGroupsDataTable;//appDataManager.GetCommandGroups() ;
            DataTable commands = AppData.Instance.CommandsDataTable;//appDataManager.GetCommands();

            foreach (DataRow commandGroup in commandGroups.Rows)
            {
                commandGroupId = System.Convert.ToInt16((commandGroup[ColumnNames.ID]));
                commandGroupNode = AddCommandGroupsToTreeView(commandGroup[ColumnNames.NAME].ToString(), commandGroupId);
                string comGroup = commandGroupId.ToString();
                DataRow[] commandDataRows = commands.Select("CommandGroups_Id = " + comGroup, "CommandGroups_Id");
                foreach (DataRow command in commandDataRows)
                {
                    AddCommandsToTreeView(command[ColumnNames.NAME].ToString(), System.Convert.ToInt16(command[ColumnNames.ID]), commandGroupNode);
                }
            }
            ResumeLayout();
            tvCommands.ExpandAll();
        }

        /// <summary>
        /// Adds command groups to the treeview
        /// </summary>
        /// <param name="commandGroupName">Name of the command group</param>
        /// <param name="commandGroupId">Id of the command group</param>
        /// <returns>TreeValueNode</returns>
        private TreeValueNode AddCommandGroupsToTreeView(string commandGroupName, int commandGroupId)
        {
            TreeValueNode node = new TreeValueNode();

            commandResources.ApplyResources(node, commandGroupName);
            

            node.Value = commandGroupId;
           // if (((commandGroupId == (int)CommandGroups.AdvancedStatistics)) || ((commandGroupId == (int)CommandGroups.UserDefined)))
           // {
           //     node.ImageIndex = 73;
           //     node.SelectedImageIndex = 73;
           // }
           // else
           // {
                node.ImageIndex = 72;
                node.SelectedImageIndex = 72;
           // }
            tvCommands.Nodes[0].Nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Adds commands to the treeview
        /// </summary>
        /// <param name="commandName">Name of the command</param>
        /// <param name="commandId">Id of the command</param>
        /// <param name="commandGroupNode">TreeNode of the group the comamnd belongs to</param>
        /// <returns>TreeValueNode</returns>
        private TreeValueNode AddCommandsToTreeView(string commandName, int commandId, System.Windows.Forms.TreeNode commandGroupNode)
        {
            TreeValueNode node = new TreeValueNode();

            resources.ApplyResources(node, commandName);

            node.Name = commandName;
            node.Value = commandId;

            ////*********************************************************
            ////******* Only put in for the partner demo... remove after!
            ////*********************************************************
            //if (commandName.Equals("Read (Import)") || commandName.Equals("List") || commandName.Equals("Frequencies") || commandName.Equals("Display") || commandName.Equals("Define") || commandName.Equals("Undefine"))
            //{
            node.ImageIndex = 71;
            node.SelectedImageIndex = 71;
            //}
            //else
            //{
            //node.ImageIndex = 42;
            //node.SelectedImageIndex = 42;
            //}
            commandGroupNode.Nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Displays a message that the data source has not been read.
        /// </summary>
        private void DisplayNoDataSourceMessage()
        {
            this.mainForm.ProgramEditor.ShowErrorMessage(SharedStrings.NO_DATA_SOURCE);
            //MsgBox.ShowError(SharedStrings.NO_DATA_SOURCE);
        }

        

        /// <summary>
        /// Method for Design variable commands
        /// </summary>
        /// <param name="command">Enum for VariableCommands</param>
        private void DesignVariableCommand(VariableCommands command)
        {
            //CommandDesignDialog dlg = null;
            try
            {
                //				if (!(Enum.IsDefined(typeof(Enums.VariableCommands),command)))
                //				{
                //					throw new ArgumentException(Localization.LocalizeString(SharedStrings.NOT_VALID_ENUM_TYPE) + command.GetType().Name.ToString() );
                //				}

                switch (command)
                {
                    case VariableCommands.Assign:
                        DesignAndProcessCommand(new AssignDialog(mainForm, true));
                        break;
                    case VariableCommands.Define:
                        DesignAndProcessCommand(new DefineVariableDialog(mainForm, true));
                        break;
                    case VariableCommands.DefineGroup:
                        DesignAndProcessCommand(new DefineGroupDialog(mainForm));
                        break;
                    case VariableCommands.Display:
                        DesignAndProcessCommand(new DisplayDialog(mainForm));
                        break;
                    case VariableCommands.Recode:
                        DesignAndProcessCommand(new RecodeDialog(mainForm));
                        break;
                    case VariableCommands.Undefine:
                        //DisplayFeatureNotImplementedMessage(); 
                        DesignAndProcessCommand(new UndefineVariableDialog(mainForm));
                        break;
                    default:
                        break;
                } //switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
            //			}
        }

        /// <summary>
        /// Method for User Interaction commands
        /// </summary>
        /// <param name="command">Enum for UserInteractionCommands</param>
        private void DesignUserInteractionCommand(UserInteractionCommands command)
        {
            try
            {
                switch (command)
                {
                    case UserInteractionCommands.Beep:
                        DesignAndProcessCommand(new BeepDialog(mainForm));
                        break;
                    case UserInteractionCommands.Dialog:
                        DesignAndProcessCommand(new DialogDialog(mainForm, true));
                        break;
                    case UserInteractionCommands.Help:
                        DisplayFeatureNotImplementedMessage(); 
                        //DesignAndProcessCommand(new HelpDialog(mainForm, true));
                        break;
                    case UserInteractionCommands.Quit:
                        DesignAndProcessCommand(new QuitDialog(mainForm));
                        break;
                    default:
                        break;
                }//switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }

        /// <summary>
        /// Method for Statistics commands
        /// </summary>
        /// <param name="command">Enum for StatisticsCommands</param>
        private void DesignStatisticsCommand(StatisticsCommands command)
        {
            try
            {
                switch (command)
                {
                    case StatisticsCommands.Frequencies:
                        DesignAndProcessCommand(new FrequencyDialog(mainForm));
                        break;
                    case StatisticsCommands.Graph:
                        //DisplayFeatureNotImplementedMessage();
                        DesignAndProcessCommand(new GraphDialog(mainForm));
                        break;
                    case StatisticsCommands.List:
                        DesignAndProcessCommand(new ListDialog(mainForm));
                        break;
                    case StatisticsCommands.Map:
                        DisplayFeatureNotImplementedMessage();
                        //DesignAndProcessCommand(new Map());
                        break;
                    case StatisticsCommands.Match:
                        DisplayFeatureNotImplementedMessage();
                        //DesignAndProcessCommand(new MatchDialog(mainForm));
                        break;
                    case StatisticsCommands.Means:
                        DesignAndProcessCommand(new MeansDialog(mainForm));
                        break;
                    case StatisticsCommands.Summarize:
                        //DisplayFeatureNotImplementedMessage();
                        DesignAndProcessCommand(new SummarizeDialog(mainForm));
                        break;
                    case StatisticsCommands.Tables:
                        //DisplayFeatureNotImplementedMessage();
                        DesignAndProcessCommand(new TablesDialog(mainForm));
                        break;                    
                    default:
                        break;
                }//switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }


        /// <summary>
        /// Method for Statistics commands
        /// </summary>
        /// <param name="command">Enum for StatisticsCommands</param>
        private void DesignAdvancedStatisticsCommand(AdvancedStatisticsCommands command)
        {
            try
            {
                Rule_Context Context = this.mainForm.EpiInterpreter.Context;
                switch (command)
                {
                    case AdvancedStatisticsCommands.CoxProportionalHazards:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new CoxProportionalHazardsDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case AdvancedStatisticsCommands.KaplanMeierSurvival:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new KaplanMeierSurvivalDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case AdvancedStatisticsCommands.LinearRegression:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new LinearRegressionDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case AdvancedStatisticsCommands.LogisticRegression:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new LogisticRegressionDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }                        
                        break;
                    case AdvancedStatisticsCommands.ComplexSampleTables:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new ComplexSampleTablesDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case AdvancedStatisticsCommands.ComplexSampleMeans:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new ComplexSampleMeansDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case AdvancedStatisticsCommands.ComplexSampleFrequencies:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new ComplexSampleFrequencyDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    default:
                        break;
                }//switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }
        
        
        /// <summary>
        /// Method for Select/If commands
        /// </summary>
        /// <param name="command">Enum for SelectIfCommands</param>
        private void DesignSelectIfCommand(SelectIfCommands command)
        {

            try
            {
                Rule_Context Context = this.mainForm.EpiInterpreter.Context;
                switch (command)
                {
                    case SelectIfCommands.CancelSelect:
                        //Fix defect 225
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new CancelSelect(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case SelectIfCommands.CancelSort:
                        DesignAndProcessCommand(new CancelSort(mainForm));
                        break;
                    case SelectIfCommands.If:
                        //dpb DisplayFeatureNotImplementedMessage();
                        DesignAndProcessCommand(new IfDialog(mainForm));
                        break;
                    case SelectIfCommands.Select:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new SelectDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case SelectIfCommands.Sort:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new SortDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    default:
                        break;
                } //switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }
        
        /// <summary>
        /// Method for Output commands
        /// </summary>
        /// <param name="command">Enum for OutputCommands</param>
        private void DesignOutputCommand(OutputCommands command)
        {
            try
            {
                switch (command)
                {
                    case OutputCommands.Closeout:
                        DesignAndProcessCommand(new CloseoutDialog(mainForm));
                        break;
                    case OutputCommands.Header:
                        DesignAndProcessCommand(new HeaderoutDialog(mainForm));
                        break;
                    case OutputCommands.Printout:
                        DesignAndProcessCommand(new PrintoutDialog(mainForm));
                        break;
                    case OutputCommands.Reports:
                        DesignAndProcessCommand(new ReportDialog(mainForm));
                        break;
                    case OutputCommands.Routeout:
                        DesignAndProcessCommand(new RouteoutDialog(mainForm));
                        break;
                    case OutputCommands.StoreOutput:
                        DesignAndProcessCommand(new StoringOutputDialog(mainForm));
                        break;
                    case OutputCommands.Type:
                        DesignAndProcessCommand(new TypeoutDialog(mainForm));
                        break;
                    default:
                        break;
                }//switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }
        
        /// <summary>
        /// Method for Data commands
        /// </summary>
        /// <param name="command">Enum for DataCommands</param>
        private void DesignDataCommand(DataCommands command)
        {

            try
            {

                Rule_Context Context = this.mainForm.EpiInterpreter.Context;
                this.mainForm.ProgramEditor.ShowErrorMessage("");
                switch (command)
                {
                    case DataCommands.DeleteFile:
                        //DisplayFeatureNotImplementedMessage();
                        DesignAndProcessCommand(new DeleteFileTableDialog(mainForm));
                        break;
                    case DataCommands.DeleteRecord:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new DeleteRecordsDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case DataCommands.Merge:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new MergeDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case DataCommands.Read:
                        //DesignAndProcessCommand(new ReadDialog(mainForm));
                        DesignAndProcessCommand(new ReadDataSourceDialog(mainForm));
                        break;
                    case DataCommands.Relate:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new RelateDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case DataCommands.UndeleteRecord:

                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new UndeleteRecordsDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    case DataCommands.Write:
                        if (Context.CurrentRead != null)
                        {
                            DesignAndProcessCommand(new WriteDialog(mainForm));
                        }
                        else
                        {
                            DisplayNoDataSourceMessage();
                        }
                        break;
                    default:
                        break;
                }//switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }
        
        /// <summary>
        /// Method for Option Commands
        /// </summary>
        /// <param name="command">Enum for OptionCommand</param>
        private void DesignOptionCommand(OptionCommands command)
        {
            try
            {
                switch (command)
                {
                    case OptionCommands.Set:
                        DesignAndProcessCommand(new SetDialog(mainForm));
                        break;
                    default:
                        break;
                }//switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }
        


        /// <summary>
        /// Method for calling process command
        /// </summary>
        /// <param name="dlg">Dialog to design and process command</param>
        private void DesignAndProcessCommand(Epi.Windows.Analysis.Dialogs.CommandDesignDialog dlg)
        {
            #region Input Validation
            if (dlg == null)
            {
                throw new System.ArgumentNullException("dlg");
            }
            #endregion  Input Validation

            try
            {
                DialogResult result = dlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    //Some dialogs may not generate a command. No need to process command if command text is empty.
                    if (!string.IsNullOrEmpty(dlg.CommandText))
                    {
                        if (dlg.CommandText.ToUpperInvariant().StartsWith("RELATE") && dlg.CommandText.Contains("FMT=JSON"))
                        {
                            dlg.CommandText = dlg.CommandText.Replace(".json", "#json").Replace(".JSON", "#JSON").Replace(".txt", "#txt").Replace(".TXT", "#TXT");
                            int fmtindex = dlg.CommandText.IndexOf("FMT=JSON") + 11;
                            int filelength = dlg.CommandText.Substring(fmtindex).IndexOf(' ');
                            string nameoffile = dlg.CommandText.Substring(fmtindex, filelength);
                            if (!(nameoffile.StartsWith("[") && nameoffile.EndsWith("]")))
                                dlg.CommandText = dlg.CommandText.Replace(nameoffile, "[" + nameoffile + "]");
                        }
                        String csvFilePath = GetFilePathGivenCommand(dlg.CommandText);

                        if (System.IO.File.Exists(csvFilePath))
                        {
                            byte[] buffer = new byte[5];
                            System.IO.FileStream file = new System.IO.FileStream(csvFilePath, System.IO.FileMode.Open);
                            file.Read(buffer, 0, 5);
                            file.Close();

                            System.IO.StreamReader reader = new System.IO.StreamReader(csvFilePath);
                            string line = reader.ReadLine();
                            reader.Close();                            

                            if(line.ToUpper().Contains("SEP="))
                            {
                                String message = String.Format(resources.GetString("DELIMITER_HEADER_WARNING"), line);
                                MessageBox.Show(message);
                            }

                            string command = dlg.CommandText;

                            if(buffer[0] == 255 && buffer[1] == 254)
                            {
                                command = command.Insert(command.IndexOf("HDR="), "CharacterSet=UNICODE;");
                            }

                            dlg.CommandText = command;
                        }

                        if (CommandGenerated != null)
                        {
                            Epi.Windows.Analysis.Forms.CommandProcessingMode Mode = (Epi.Windows.Analysis.Forms.CommandProcessingMode) dlg.ProcessingMode;
                            CommandGenerated(dlg.CommandText, Mode);
                        }
                    }
                    dlg.Close();
                }
            } //try
            finally
            {
                //fix defect 279
                if (this.tvCommands.SelectedNode.Parent != null)
                {
                    tvCommands.SelectedNode = this.tvCommands.SelectedNode.Parent;
                }
                else
                {
                    throw new System.ArgumentNullException("Parent node is null");
                }

                // dcs0 TODO - I don't think this should be here
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }



        /// <summary>
        /// READ {Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\\cdc.gov\private\M131\ita3\_EI7\177;Extended Properties="text;HDR=Yes;FMT=Delimited"}:[VHF_Contacts_Export_Sans_Sep#csv] 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string GetFilePathGivenCommand(string command)
        {
            string directory = string.Empty;
            string fileName = string.Empty;
            List<string> segments = new List<string>(command.Split(';'));
            try
            {
                foreach (string segment in segments)
                {
                    if(segment.Contains("Data Source"))
                    {
                        directory = segment.Replace("Data Source=", "");
                    }

                    if (segment.Contains("}:["))
                    {
                        fileName = segment.Substring(segment.IndexOf("["));
                        fileName = fileName.Replace("[", "");
                        fileName = fileName.Replace("#", ".");
                        fileName = fileName.Replace("]", "");
                    }
                }

                string fullFilePath = System.IO.Path.Combine(directory, fileName);

                if(System.IO.File.Exists(fullFilePath))
                {
                    return fullFilePath;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {

            }
                return string.Empty;
          
        }
        
        private void DesignUserDefinedCommand(UserDefinedCommands command)
        {
            try
            {
                switch (command)
                {
                    case UserDefinedCommands.RunSavedProgram:
                        DesignAndProcessCommand(new RunSavedPGMDialog(mainForm));
                        break;
                    case UserDefinedCommands.DefineCommand:
                        DesignAndProcessCommand(new DefineUserCommandDialog(mainForm));
                        break;
                    case UserDefinedCommands.ExecuteFile:
                        DesignAndProcessCommand(new ExecuteDialog(mainForm));
                        //DisplayFeatureNotImplementedMessage();
                        break;
                    case UserDefinedCommands.UserCommand:
                        DesignAndProcessCommand(new UserCommandDialog(mainForm));
                        break;
                    default:
                        break;
                }//switch
            } //try
            finally
            {
                //programEditor.SavePGM(Files.LastPgm);
            }//finally
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles AfterSelect for the treeview control
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void tvCommands_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            try
            {
                Rule_Context Context = this.mainForm.EpiInterpreter.Context;
                if (Context.DataSet.Tables["output"] != null)
                {
                    Context.ReadDataSource(Context.DataSet.Tables["output"]);
                }
                if (e.Node.Parent != null)
                {
                    if (tvCommands.Nodes[0] == e.Node.Parent)
                    {
                        
                        switch ((CommandGroups)((TreeValueNode)e.Node).Value)
                        {
                            case CommandGroups.UserDefined:
                                DesignUserDefinedCommand((UserDefinedCommands)((TreeValueNode)e.Node).Value);
                                break;
                            default:
                                break;
                        }
                    }                    
                    else
                    {
                        // MessageBox.Show("tvCommands.Nodes[0] != e.Node.Parent", "trace");
                    }
                    if (e.Node.Parent.Parent != null)
                    {
                        
                        switch ((CommandGroups)((TreeValueNode)e.Node.Parent).Value)
                        {
                            case CommandGroups.AdvancedStatistics:
                                DesignAdvancedStatisticsCommand((AdvancedStatisticsCommands)((TreeValueNode)e.Node).Value);
                            	break;
                            case CommandGroups.Data:
                                DesignDataCommand((DataCommands)((TreeValueNode)e.Node).Value);
                                break;
                            case CommandGroups.Options:
                                DesignOptionCommand((OptionCommands)((TreeValueNode)e.Node).Value);
                                break;
                            case CommandGroups.Output:
                                DesignOutputCommand((OutputCommands)((TreeValueNode)e.Node).Value);
                                break;
                            case CommandGroups.SelectIf:
                                DesignSelectIfCommand((SelectIfCommands)((TreeValueNode)e.Node).Value);
                                break;
                            case CommandGroups.Statistics:
                                if (Context.CurrentRead != null)
                                {
                                    DesignStatisticsCommand((StatisticsCommands)((TreeValueNode)e.Node).Value);
                                }
                                else
                                {
                                    DisplayNoDataSourceMessage();
                                }
                                break;
                            case CommandGroups.UserDefined:
                                DesignUserDefinedCommand((UserDefinedCommands)((TreeValueNode)e.Node).Value);
                                break;
                            case CommandGroups.UserInteraction:
                                DesignUserInteractionCommand((UserInteractionCommands)((TreeValueNode)e.Node).Value);
                                break;
                            case CommandGroups.Variables:
                                DesignVariableCommand((VariableCommands)((TreeValueNode)e.Node).Value);
                                break;
                            default:
                                break;
                        }
                    }
                }                
            }
            catch (Exception)
            {
                //KKM4-1                MessageBox.Show(ex.Message + ".......... Exception occured. Giving up!", "trace");
            }
        }

        /// <summary>
        /// Handles the click event of tvCommands
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void tvCommands_Click(object sender, System.EventArgs e)
        {
            // KKM4-1            MessageBox.Show("tvCommands_Click", "trace");
            //Added this code to fire AfterSelect whenever the same node is clicked more than once
            //if (tvCommands.SelectedNode != null)
            //{
            //    tvCommands.SelectedNode = null;
            //}

        }

        /// <summary>
        /// Prevent expand of Advanced Stat and User Defined Commands.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void tvCommands_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                //switch ((CommandGroups)((TreeValueNode)e.Node).Value)
                //{
                    //case CommandGroups.AdvancedStatistics:
                    //    e.Cancel = true;
                    //    break;

                    //case CommandGroups.UserDefined:
                    //    e.Cancel = true;
                    //    break;
                //}
            }
        }

        /// <summary>
        /// Sets the nodes image
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void tvCommands_AfterExpand(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 72;
            e.Node.SelectedImageIndex = 72;
        }

        /// <summary>
        /// Set the node's immage to Closed Folder
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void tvCommands_AfterCollapse(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 73;
            e.Node.SelectedImageIndex = 73;
        }

        #endregion
        
    }
}
