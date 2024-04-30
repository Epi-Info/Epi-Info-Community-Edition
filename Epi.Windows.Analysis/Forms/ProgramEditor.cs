#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Epi.Analysis;
using Epi.Diagnostics;
using Epi.Windows;
using Epi.Windows.Docking;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis.Dialogs;

#endregion

namespace Epi.Windows.Analysis.Forms
{

    #region Delegates

    /// <summary>
    ///  Represents the method that handles the RunCommand event
    /// </summary>
    public delegate void RunCommandEventHandler(string command);

    /// <summary>
    ///  Represents the method that will be used to process the results from a command
    /// </summary>
    public delegate void ProcessResultsHandler( CommandProcessorResults results );

    /// <summary>
    ///  Represents the method that handles the RunPGM event
    /// </summary>
    public delegate void RunPGMEventHandler(string command);

    ///// <summary>
    /////  Represents the method that handles the RunPGM event
    ///// </summary>
    //public delegate void RunPGMEventHandler( string command, ProcessResultsHandler resultshandler );


    #endregion
    
    /// <summary>
    /// The Analysis program editor
    /// </summary>
    public partial class ProgramEditor : DockWindow
    {

        #region Private Attributes
        private string currentPgmName = string.Empty;
        private string findString = string.Empty;
        private int lineNo = 0;
//        private bool dirty = false;
        private Font printFont = new Font("Arial", 10);
        private static Char[] noiseChars = { '\r', '\n', ' ', '\t' };
        private string currentSearchString = string.Empty;
        private RichTextBoxFinds currentSearchOpts;
        new private Epi.Windows.Analysis.Forms.AnalysisMainForm mainForm;
        #endregion Private Attributes


        #region Public Events

        /// <summary>
        /// Occurs when RunThisCommand is clicked
        /// </summary>
        public event RunCommandEventHandler RunCommand;

        /// <summary>
        /// Occurs when Run is clicked
        /// </summary>
        public event RunPGMEventHandler RunPGM;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ProgramEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainForm">The main form</param>
        public ProgramEditor(AnalysisMainForm mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the generated command to the text area
        /// </summary>
        /// <param name="commandText">Command Text</param>
        public void AddCommand(string commandText)
        {
            if (string.IsNullOrEmpty(commandText.Trim()))
            {
                throw new ArgumentNullException("commandText");
            }

            if (!(txtTextArea.Text.EndsWith(Environment.NewLine) || txtTextArea.Text.EndsWith("\n") || txtTextArea.Text == ""))
            {
                commandText = Environment.NewLine + commandText;
            }

            commandText += Environment.NewLine;

            if (mnuEditInsertCommandAtCursor.Checked)
            {
                int line = txtTextArea.GetLineFromCharIndex( txtTextArea.SelectionStart );
                txtTextArea.SelectionStart = txtTextArea.GetFirstCharIndexFromLine( line );
                txtTextArea.SelectionLength = 0;
                txtTextArea.SelectedText = commandText;
            }
            else
            {
                txtTextArea.AppendText( commandText );
            }
        }

        /// <summary>
        /// Clears out the program editor's text area.
        /// </summary>
        public virtual void Clear()
        {
            txtTextArea.Text = string.Empty;
        }

        /// <summary>
        /// Clears all the text in the editor
        /// </summary>
        public void OnNew()
        {
            if (!string.IsNullOrEmpty(this.txtTextArea.Text))
            {
                DialogResult result = MsgBox.ShowQuestion(SharedStrings.CREATE_NEW_PGM);
                if (result == DialogResult.Yes)
                {
                    Clear();
                    ShowNewPgm();
//                    dirty = false;
                    currentPgmName = string.Empty;
                }
            }
            else
            {
                ShowNewPgm();
            }
        }

        /// <summary>
        /// Loads pgm from command line text file and executes.
        /// </summary>
        /// <param name="programPath">Path to program</param>
        public void LoadProgramFromCommandLine(string programPath)
        {
            if (File.Exists(programPath))
            {
                txtTextArea.Text = File.ReadAllText(programPath);
                RunPGM(txtTextArea.Text);
            }
        }

        /// <summary>
        /// Set Cursor
        /// </summary>
        /// <param name="index">Index of the cursor</param>
        public void SetCursor(int index)
        {
            txtTextArea.SelectionStart = index;
            
        }
        /// <summary>
        /// Set Line Focus
        /// </summary>
        /// <param  > </param>
        public void SetLineFocus()
        {
             
            int lineNumber = txtTextArea.GetLineFromCharIndex(txtTextArea.SelectionStart);
            int start = txtTextArea.GetFirstCharIndexFromLine(lineNumber);
            txtTextArea.SelectionStart = start;
            txtTextArea.SelectionLength = 0;
            txtTextArea.Focus();
            
        }
        /// <summary>
        /// Set Cursor
        /// </summary>
        /// <param name="args">Parse Exception</param>
        public void SetCursor(ParseException args)
        {
            int lineNumber = args.UnexpectedToken.Location.LineNr;
            if (lineNumber == 0)
            {
                string compare = string.Empty;
                if ( txtTextArea.Lines[lineNumber].Length > (args.UnexpectedToken.Location.Position - 1 + args.UnexpectedToken.Text.Length))
                {
                    compare = txtTextArea.Lines[lineNumber].Substring(args.UnexpectedToken.Location.Position - 1, args.UnexpectedToken.Text.Length);
                }
                if (compare != args.UnexpectedToken.Text)
                {
                    lineNumber = txtTextArea.GetLineFromCharIndex(txtTextArea.SelectionStart);
                }
            }
            txtTextArea.Text = txtTextArea.Text.TrimStart(new char[]{'\n','\r',' '});
            int start = txtTextArea.GetFirstCharIndexFromLine(lineNumber);
            int length = txtTextArea.Lines[lineNumber].Length;
            txtTextArea.Select(start, length);
            txtTextArea.SelectionColor = Color.Red;
            txtTextArea.Select(start, 0);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Show a blank Pgm
        /// </summary>
        private void ShowNewPgm()
        {
            this.Text = SharedStrings.PROGRAM_EDITOR + " - " + SharedStrings.NEW_PROGRAM;
        }

        private void SetTitle()
        {
            this.Text = SharedStrings.PROGRAM_EDITOR;
            if (!string.IsNullOrEmpty(this.currentPgmName)) this.Text += " - " + this.currentPgmName;
        }

        /// <summary>
        /// Saves the current program
        /// </summary>
        /// <param name="content">Content of th Pgm</param>

        private void SaveCurrentPgm(string content)
        {
            PgmDialog dialog = new PgmDialog(mainForm, currentPgmName, content, PgmDialog.PgmDialogMode.SaveProgram);
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
            }
        }

        /// <summary>
        /// Raises the RunPGM event.
        /// </summary>
        /// <param name="command">Command that is to be executed</param>
        private void OnRunPGM(string command)
        {
            //Issue 889: making sure command text is not null or empty string
            if (RunPGM != null && !string.IsNullOrEmpty(command) && command.Trim().Length> 0)
            {
                OutputTextBox.Clear();
                btnCancelRunningCommands.Enabled = true;
                btnRun.Enabled = false;
                RunPGM(command);      // dcs90 7/7/2008 because resultlts must be handled for each command

            }
        }

        ///// <summary>
        ///// change the font color of the selected Line.
        ///// </summary>
        ///// <param name="lineNo">number of line to be changed</param>
        ///// <param name="color">the color to which to change</param>

        //private void ChangeColor(int lineNo, Color color)
        //{
        //    int startPos = TextArea.SelectionStart;
        //    int currLine = txtTextArea.GetLineFromCharIndex(txtTextArea.SelectionStart);
        //    int lineLen = txtTextArea
        //}
        /// <summary>
        /// Raises the RunCommand event.
        /// </summary>
        /// <param name="command">Command that is to be executed</param>
        private void OnRunCommand(string command)
        {
            //Issue 889: making sure command text is not null or empty string
            if (RunCommand != null && !string.IsNullOrEmpty(command) && command.Trim().Length > 0)
            {
                OutputTextBox.Clear();
                btnCancelRunningCommands.Enabled = false;
                btnRun.Enabled = false;
                
                RunCommand(command);
                
                btnCancelRunningCommands.Enabled = true;
                btnRun.Enabled = true;
            }
        }

        #endregion Private Methods

        #region Event Handlers

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            mainForm.Close();
        }

        private void ProgramEditor_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtTextArea_TextChanged(object sender, EventArgs e)
        {
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            OnNew();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            PgmDialog dialog = new PgmDialog(mainForm, string.Empty, string.Empty, PgmDialog.PgmDialogMode.OpenProgram);
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                currentPgmName = dialog.ProgramName;
                txtTextArea.Text = dialog.Content;
                SetTitle();
            }
            if (result != DialogResult.None)
            {
                dialog.Close();
            }
        }

        private void btnSave_Click(object sender, EventArgs ev)
        {
            PgmDialog dialog = new PgmDialog(mainForm, currentPgmName,
                               this.txtTextArea.Text, PgmDialog.PgmDialogMode.SaveProgram);
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                currentPgmName = dialog.ProgramName;
                txtTextArea.Text = dialog.Content;
                SetTitle();
            }
            dialog.Close();
        }

        private void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            PgmDialog dialog = new PgmDialog(mainForm, currentPgmName, this.txtTextArea.Text, 
                PgmDialog.PgmDialogMode.SaveProgramAs);

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                currentPgmName = dialog.ProgramName;
                txtTextArea.Text = dialog.Content;
                SetTitle();
            }
            dialog.Close();
        }

        private void btnPrint_Click(object sender, EventArgs ev)
        {
            using (PrintDialog pd = new PrintDialog())
            {
                if (pd.ShowDialog() == DialogResult.OK)
                {
                    using (PrintDocument doc = new PrintDocument())
                    {
                        printFont = txtTextArea.Font;
                        lineNo = 0;
                        doc.PrintPage += new PrintPageEventHandler(doc_PrintPage);
                        try
                        {
                            doc.Print();
                        }
                        catch (Win32Exception ex)
                        {
                            MsgBox.Show(ex.Message, SharedStrings.PRINT);
                        }
                    }
                }
            }
        }

        // The PrintPage event is raised for each page to be printed.
        private void doc_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            String line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height / printFont.GetHeight(ev.Graphics);

            // Iterate over the file, printing each line.
            while (count < linesPerPage && (lineNo < txtTextArea.Lines.GetUpperBound(0))) 
            {
                line = txtTextArea.Lines[lineNo];
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black,leftMargin, yPos, new StringFormat());
                count++;
                lineNo++;
            }

            // If more lines exist, print another page.
            ev.HasMorePages = (lineNo < txtTextArea.Lines.GetUpperBound(0));
        }

        private void btnRun_Click(object sender, EventArgs e)
        {

            /*txtTextArea.SelectAll();
            txtTextArea.SelectionColor = Color.Black;
            txtTextArea.Select(0, 0);
            */

            if (txtTextArea.SelectedText.Length > 0)
            {
                OnRunCommand(txtTextArea.SelectedText);
            }
            else
            {

                OnRunPGM(txtTextArea.Text.Replace("\n", "\r\n"));
            }

        }
            

        private void btnCancelRunningCommands_Click(object sender, EventArgs e)
        {
            this.mainForm.CancelRunningCommand();
        }
        


        private void mnuEditUndo_Click(object sender, EventArgs e)
        {
            this.txtTextArea.Undo();
        }

        private void mnuEditRedo_Click(object sender, EventArgs e)
        {
            this.txtTextArea.Redo();
        }

        private void mnuEditCut_Click(object sender, EventArgs e)
        {
            this.txtTextArea.Cut();
        }

        private void mnuEditCopy_Click(object sender, EventArgs e)
        {
            this.txtTextArea.Copy();
        }

        private void mnuEditPaste_Click(object sender, EventArgs e)
        {
            this.txtTextArea.Paste();
        }

        private void mnuSelectAll_Click(object sender, EventArgs e)
        {
            //this.txtTextArea.SelectAll();        this doesn't work - par for M$
            this.txtTextArea.Select(0, txtTextArea.TextLength);
            //txtTextArea.SelectionStart = 0;
            //txtTextArea.SelectionLength = txtTextArea.TextLength;
        }

        private void mnuEditDeleteLine_Click(object sender, EventArgs e)
        {
            // TODO: dcs0 figure out how to delete a line
            //this.txtTextArea.SelectionStart = this.txtTextArea.GetFirstCharIndexOfCurrentLine();
            //this.txtTextArea.SelectionLength = txtTextArea.GetFirstCharIndexFromLine(this.txtTextArea.GetLineFromCharIndex(1));
        }

        private void mnuEditFind_Click(object sender, EventArgs e)
        {
            OutputTextBox.Clear();
            if (txtTextArea.TextLength > 0)
            {
                SearchDialog dlg = new SearchDialog(txtTextArea.SelectedText);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    currentSearchString = dlg.SearchString;
                    int currentPos = txtTextArea.SelectionStart;
                    currentSearchOpts = (RichTextBoxFinds)0;
                    if (dlg.SearchDirection == SearchDialog.Direction.Beginning)
                    {
                        txtTextArea.SelectionStart = 0;
                    }
                    else if (dlg.SearchDirection == SearchDialog.Direction.Backward)
                    {
                        currentSearchOpts |= RichTextBoxFinds.Reverse;
                    }
                    if (dlg.CaseSensitive)
                    {
                        currentSearchOpts |= RichTextBoxFinds.MatchCase;
                    }
                    currentSearchOpts |= (dlg.WholeWord) ? RichTextBoxFinds.WholeWord : RichTextBoxFinds.None;
                    int location = 0;
                    if (txtTextArea.TextLength > txtTextArea.SelectionStart)
                    {
                        location = txtTextArea.Find(dlg.SearchString, txtTextArea.SelectionStart, currentSearchOpts);
                    }
                    if (location > 0)
                    {
                        txtTextArea.SelectionStart = location;
                        txtTextArea.SelectionLength = currentSearchString.Length;
                    }
                    else
                    {
                        OutputTextBox.Text = SharedStrings.EOF;
                        txtTextArea.SelectionStart = currentPos;
                        txtTextArea.SelectionLength = 0;
                    }
                }
            }
            else
            {
                OutputTextBox.Text = SharedStrings.NO_SEARCH_TEXT;
            }
        }

        private void mnuEditFindNext_Click(object sender, EventArgs e)
        {
            OutputTextBox.Clear();
            int currentPos = 0;
            if (txtTextArea.TextLength > 0)
            {
                currentPos = txtTextArea.SelectionStart;
                if (currentPos == txtTextArea.TextLength)
                {
                    currentPos = 0;
                    txtTextArea.SelectionLength = 0;
                }
                if (txtTextArea.SelectionLength > 0)
                {
                    currentPos += txtTextArea.SelectionLength;
                }
                int location = 0;

                if (currentPos < txtTextArea.TextLength)
                {
                    location = txtTextArea.Find(currentSearchString, currentPos, currentSearchOpts);
                }
                if (location > 0)
                {
                    txtTextArea.SelectionStart = location;
                    txtTextArea.SelectionLength = currentSearchString.Length;
                }
                else
                {
                    OutputTextBox.Text = SharedStrings.EOF;
                    txtTextArea.SelectionStart = txtTextArea.TextLength;
                    txtTextArea.SelectionLength = 0;
                }
            }
        }

        private void mnuEditReplace_Click(object sender, EventArgs e)
        {
            if (txtTextArea.TextLength > 0)
            {
                int location = txtTextArea.SelectionStart;
                string text = txtTextArea.SelectedText;
                ReplaceDialog dlg = new ReplaceDialog(text);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    currentSearchOpts = (RichTextBoxFinds)0;
                    currentSearchString = dlg.ReplaceString;
                    if (dlg.CaseSensitive)
                    {
                        currentSearchOpts |= RichTextBoxFinds.MatchCase;
                    }
                    currentSearchOpts |= (dlg.WholeWord) ? RichTextBoxFinds.WholeWord : RichTextBoxFinds.None;
                    do
                    {
                        location = txtTextArea.Find(currentSearchString, currentSearchOpts);
                        if (location > 0)
                        {
                            txtTextArea.SelectionStart = location;
                            txtTextArea.SelectionLength = currentSearchString.Length;
                            txtTextArea.SelectedText = dlg.ReplacementString;
                        }
                    } while (location > 0 && dlg.ReplaceAll);
                }
            }
            else
            {
                OutputTextBox.Text = SharedStrings.NO_SEARCH_TEXT;
            }
        }

        private void mnuEditProgramBeginning_Click(object sender, EventArgs e)
        {
            this.txtTextArea.SelectionStart = 0;
        }

        private void mnuEditProgramEnd_Click(object sender, EventArgs e)
        {
            int line = txtTextArea.Lines.GetUpperBound(0);
            if (line >= 0)
            {
                txtTextArea.SelectionStart = txtTextArea.GetFirstCharIndexFromLine(line);
            }
        }

        private void mnuEditInsertCommandAtCursor_Click(object sender, EventArgs e)
        {
            mnuEditInsertCommandAtCursor.Checked = (!mnuEditInsertCommandAtCursor.Checked);
        }

        private void mnuFontsSetEditorFont_Click(object sender, EventArgs e)
        {
            fontDialog.ShowDialog(this);
            txtTextArea.Font = fontDialog.Font;
        }
        #endregion

        private void mnuOutputTextCopy_Click(object sender, EventArgs e)
        {
            this.OutputTextBox.Copy();
        }

        public void ShowErrorMessage(string pErrorMessage)
        {
            this.OutputTextBox.Text = pErrorMessage;
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtTextArea.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
                this.txtTextArea.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtTextArea.Paste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtTextArea.SelectedText = "";
        }

		private void txtTextArea_KeyPress(object sender, KeyPressEventArgs e)
		{

		}
	}
}

