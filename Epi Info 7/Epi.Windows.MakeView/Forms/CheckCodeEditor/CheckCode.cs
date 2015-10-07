#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi.Windows.Docking;
using Epi;
using Epi.Fields;
using EpiInfo.Plugin;
using Epi.Windows.Dialogs;
using Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs;
using Epi.Windows.MakeView.Dialogs;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Forms
{

    /// <summary>
    /// The check code editor form
    /// </summary>
    public partial class CheckCode : Form
    {
        #region Private Data Members

        private bool isDirty = false;

        private List<string> keywords;
        private List<string> operators;

        private bool isDefineCommand;
        private View view;

        private string findString = string.Empty;
        private int lineNo = 0;
        private Font printFont = new Font("Courier New", 10, FontStyle.Regular);

        private string currentSearchString = string.Empty;
        private RichTextBoxFinds currentSearchOpts;

        // drag and drop
        private bool validData = false;
        private TreeNode SelectedDragNode = null;
        private int lastX, lastY;

        #endregion  //Private Data Members

        #region Public Data Members

        public delegate void CheckCodeCommandDesignHandler(string command);
        public delegate void SaveEventHandler(object sender, EventArgs e);
        public event CheckCodeCommandDesignHandler CheckCodeCommandDesigned;
        public MakeViewMainForm mainForm;

        public View View
        {
            get { return this.view; }
        }

        public IEnterInterpreter EpiInterpreter
        {
            get { return this.mainForm.EpiInterpreter; }
        }

        #endregion  //Public Data Members

        #region Constructors

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public CheckCode(MakeViewMainForm frm)
        {
            this.mainForm = frm;
            Construct();
        }

        /// <summary>
        /// Compiles check code
        /// </summary>
        /// <param name="checkCode">The check code to be compiled</param>
        /// <returns>Determines whether check code was compiled</returns>
        public bool CompileCheckCode(String checkCode)
        {
            bool isValidCommand = false;
            try
            {
                this.mainForm.EpiInterpreter.Context.ClearState();
                this.mainForm.EpiInterpreter.Parse(checkCode);
                isValidCommand = true;
                this.view.CheckCode = checkCode;
                codeText.Clear();
                PopulateTextArea();
            }
            catch (ParseException ex)
            {
                int LineNumber = ex.UnexpectedToken.Location.LineNr;
                int ColumnNumber = ex.UnexpectedToken.Location.ColumnNr;
                LineNumber++;
                this.AddStatusInformationMessage(string.Format(SharedStrings.ERROR_ON_LINE_COLUMN + "\n{2}", LineNumber, ColumnNumber, ex.Message));
                int start = codeText.GetFirstCharIndexFromLine(ex.UnexpectedToken.Location.LineNr);
                int length = codeText.Lines[ex.UnexpectedToken.Location.LineNr].Length;
                codeText.Select(start, length);
                codeText.SelectionColor = Color.Red;
                codeText.Select(start, 0);
            }
            catch (Exception ex)
            {
                this.AddStatusInformationMessage(string.Format(SharedStrings.ERROR + ":\n{0}", ex.Message));
            }
            return isValidCommand;
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="view">The View to load check code for</param>
        /// <param name="frm">The main form</param>
        public CheckCode(View currentview, MakeViewMainForm frm)
        {
            mainForm = frm;
            if(currentview!=null)
            currentview.MustRefreshFieldCollection = true;
            view = currentview;
            Construct();
            EpiInfo.Plugin.IEnterInterpreter MR = mainForm.EpiInterpreter;
            MR.Context.RemoveVariablesInScope(VariableScope.Standard);

            if (!string.IsNullOrEmpty(view.CheckCode))
            {
                try
                {
                    mainForm.EpiInterpreter.Execute(view.CheckCode);
                }
                catch
                {

                }
            }

            foreach (Field field in view.Fields)
            {
                if (field is IDataField)
                {
                    EpiInfo.Plugin.IVariable definedVar = (EpiInfo.Plugin.IVariable)field;
                    MR.Context.DefineVariable(definedVar);
                }
                else if (field is Epi.Fields.LabelField)
                {
                    PluginVariable p = new PluginVariable();
                    p.Name = field.Name;
                    p.Prompt = ((Epi.Fields.LabelField)field).PromptText;
                    p.VariableScope = EpiInfo.Plugin.VariableScope.DataSource;
                    p.DataType = EpiInfo.Plugin.DataType.Text;
                    MR.Context.DefineVariable(p);
                }
                else
                {
                }
            }

            BuildComboBox();

            this.codeText.SelectionStart = 0;
            this.codeText.SelectionLength = 0;
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="page">The Page to load check code for</param>
        /// <param name="frm">The main form</param>
        public CheckCode(Page page, MakeViewMainForm frm,View currentview)
        {
            mainForm = frm;
            if (currentview != null)
            currentview.MustRefreshFieldCollection = true;         
            view = currentview;    
            Construct();                            
            BuildComboBox();
            string Identifier;
            if (page.Name.Trim().IndexOf(' ') > -1)
            {
                Identifier = "[" + page.Name + "]";
            }
            else
            {
                Identifier = page.Name;
            }
            this.GotoLine("page", "", Identifier);
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="field">The Field to load check code for</param>
        /// <param name="frm">The main form</param>
        public CheckCode(Field field, MakeViewMainForm frm,View currentview)
        {
            mainForm = frm;
            if (currentview != null)
            currentview.MustRefreshFieldCollection = true;
            view = currentview;  
            Construct();                                
            BuildComboBox();
            string Identifier;
            if (field.Name.Trim().IndexOf(' ') > -1)
            {
                Identifier = "[" + field.Name + "]";
            }
            else
            {
                Identifier = field.Name;
            }
            if (field is Epi.Fields.CommandButtonField)
            {
                this.GotoLine("field", "click", Identifier, true);
            }
            else
            {
                this.GotoLine("field", "after", Identifier, true);

                int lineIndex = this.codeText.GetLineFromCharIndex(this.codeText.SelectionStart) - 1;
                string addCodeHere = "//add code here";
                if (this.codeText.Lines[lineIndex].Contains(addCodeHere))
                {
                    this.codeText.SelectionStart = this.codeText.GetFirstCharIndexFromLine(lineIndex);
                    this.codeText.SelectionStart += this.codeText.Lines[lineIndex].IndexOf(addCodeHere);
                    this.codeText.SelectionLength = addCodeHere.Length;
                }
            }
        }

        private void Construct()
        {
            InitializeComponent();
            DockManager.FastMoveDraw = false;
            DockManager.Style = DockVisualStyle.VS2005;

            if (view == null)
            {
                view = mainForm.CurrentView;
            }

            if (string.IsNullOrEmpty(this.view.CheckCode.Trim()))
            {
                this.view.CheckCode = View.InitialCheckCode;
            }

            this.Text = "Check Code Editor - [ " + this.view.Name + " ]";

            this.CheckCodeCommandDesigned += new CheckCodeCommandDesignHandler(commandExplorer_CheckCodeCommandDesigned);
            this.codeText.MouseWheel += new MouseEventHandler(codeText_MouseWheel);
            this.tvCodeBlocks.AfterSelect += new TreeViewEventHandler(tvCodeBlocks_SelectHandler);
            this.tvCodeBlocks.MouseDoubleClick += new MouseEventHandler(tvCodeBlocks_MouseDoubleClickHandler);

            this.btnAddBlock.Enabled = false;
            this.keywords = new List<string>();
            this.operators = new List<string>();

            this.keywords.Add("after");
            this.keywords.Add("always");
            this.keywords.Add("assign");
            this.keywords.Add("autosearch");

            this.keywords.Add("before");

            this.keywords.Add("call");
            this.keywords.Add("clear");
            this.keywords.Add("click");

            this.keywords.Add("define");
            this.keywords.Add("definevariables");
            this.keywords.Add("dialog");
            this.keywords.Add("disable");

            this.keywords.Add("else");
            this.keywords.Add("enable");
            this.keywords.Add("end-definevariables");

            this.keywords.Add("end");
            this.keywords.Add("end-view");
            this.keywords.Add("end-record");
            this.keywords.Add("end-page");
            this.keywords.Add("end-before");
            this.keywords.Add("end-after");
            this.keywords.Add("end-click");
            this.keywords.Add("end-field");
            this.keywords.Add("end-if");
            this.keywords.Add("end-sub");
            this.keywords.Add("execute");

            this.keywords.Add("field");
            this.keywords.Add("form");
            this.keywords.Add("format");

            this.keywords.Add("geocode");
            this.keywords.Add("goto");
            //--gotoform
            this.keywords.Add("gotoform");
            //---
            this.keywords.Add("help");
            this.keywords.Add("hide");
            this.keywords.Add("highlight");

            this.keywords.Add("if");

            this.keywords.Add("newrecord");
            this.keywords.Add("page");
            this.keywords.Add("record");

            this.keywords.Add("sub");

            this.keywords.Add("then");
            this.keywords.Add("titletext");
            this.keywords.Add("unhide");
            this.keywords.Add("unhighlight");

            this.keywords.Add("view");

            this.keywords.Add("quit");
            this.keywords.Add("save");


            this.operators.Add("(.)");
            this.operators.Add("(+)");
            this.operators.Add("(-)");
            this.operators.Add("=");
            this.operators.Add("+");
            this.operators.Add("-");
            this.operators.Add(">");
            this.operators.Add("<");
            this.operators.Add(">=");
            this.operators.Add("<=");
            this.operators.Add("<>");
            this.operators.Add("^");
            this.operators.Add("&");
            this.operators.Add("*");
            this.operators.Add("%");
            this.operators.Add("mod");
            this.operators.Add("(.)");
            this.operators.Add("not");
            this.operators.Add("or");
            this.operators.Add("and");
            this.operators.Add("xor");

            PopulateTextArea(); // need this for findblock to work correctly

            // change view ... end-view to form ... end-form
            int viewStart = this.findBlock("view", "", "");
            string PreviousCheckCode = this.view.CheckCode;
            try
            {
                codeText.Text = ""; // clear out text area so duplicate won't be added

                if (viewStart > -1)
                {
                    string PreText = this.view.CheckCode.Substring(0, viewStart);
                    string PostText = this.view.CheckCode.Substring(viewStart + 4, this.view.CheckCode.Length - (viewStart + 4));
                    string temp = PreText + "Form" + PostText;

                    viewStart = temp.IndexOf("end-view", StringComparison.OrdinalIgnoreCase);
                    PreText = temp.Substring(0, viewStart);
                    PostText = temp.Substring(viewStart + 9, temp.Length - (viewStart + 9));
                    temp = PreText + "End-Form" + PostText;

                    this.view.CheckCode = temp;
                    GetPublishedViewKeys();
                    this.view.GetMetadata().UpdateView(this.view);
                }
            }
            catch (Exception ex)
            {
                this.view.CheckCode = PreviousCheckCode;
            }

            PopulateTextArea();

            this.isDirty = false;
            this.WindowState = FormWindowState.Maximized;
        }

        #endregion

        #region Events

        /// <summary>
        /// Save Check Code Handler
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void SaveHandler(object sender, EventArgs e)
        {
            string checkCodeValue = this.codeText.Text;

            bool couldCompile = this.CompileCheckCode(checkCodeValue);
            bool goAhead = true;

            if (couldCompile == false)
            {
                DialogResult result = MessageBox.Show("The check code does not compile. Do you want to save your changes anyway?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.No || result == DialogResult.Cancel)
                {
                    goAhead = false;
                }
            }

            if (goAhead)
            {
                this.view.CheckCode = checkCodeValue;
                this.mainForm.mediator.Project.Views[view.Name].CheckCode = checkCodeValue;
                GetPublishedViewKeys();
                this.view.GetMetadata().UpdateView(this.view);

                BuildComboBox();

                AddStatusInformationMessage(string.Format("{0}: Your Check Code has been saved.", DateTime.Now));
                this.isDirty = false;
            }
        }

        /// <summary>
        /// Click event for Cancel Button
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void editor_CancelButtonClicked(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Check Code Command Designed event for the command explorer
        /// </summary>
        /// <param name="command">Command string passed from the dialog.</param>
        private void commandExplorer_CheckCodeCommandDesigned(string command)
        {
            this.ParseText("\t\t" + command + "\n");
        }

        /// <summary>
        /// Mouse Down event handler for Code Blocks tree view
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied Mouse Event args.</param>
        private void tvCodeBlocks_MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (tvCodeBlocks.SelectedNode != null)
            {
                if (tvCodeBlocks.SelectedNode == this.SelectedDragNode)
                {
                    tvCodeBlocks.DoDragDrop(this.SelectedDragNode, DragDropEffects.Copy);
                }
            }
        }

        /// <summary>
        /// Mouse Double Click event handler for Code Blocks tree view
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied Mouse Event args.</param>
        private void tvCodeBlocks_MouseDoubleClickHandler(object sender, MouseEventArgs e)
        {
            TreeNode Node = this.SelectedDragNode;

            if (Node != null)
            {
                string[] Parameters = this.ConvertParameters(Node.Tag.ToString());

                if (this.findBlock(Parameters[0], Parameters[1], Parameters[2]) == -1)
                {
                    this.AddBlockHandler();
                    UpdateBlockButtonText();
                    this.CalculateStatus();
                    this.codeText.Focus();
                }
            }
        }

        /// <summary>
        /// Before Select handler for Commands tree view
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied Tree View Cancel Event args.</param>        
        private void tvCommands_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Select handler for Code Blocks tree view
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied Tree View Event args.</param>
        private void tvCodeBlocks_SelectHandler(object sender, TreeViewEventArgs e)
        {
            string[] Parameters = this.ConvertParameters(e.Node.Tag.ToString());

            switch (Parameters[0].ToLower())
            {
                case "view":
                case "form":
                    this.SelectedDragNode = e.Node;
                    if (this.GotoLine(Parameters[0], Parameters[1], Parameters[2]))
                    {
                        ActiveControl = this.codeText;
                        this.codeText.Select();
                        SendKeys.Send("+{TAB}+{TAB}+{TAB}");
                    }
                    else
                    {

                    }
                    UpdateBlockButtonText();
                    break;
                case "definevariables":
                    this.SelectedDragNode = e.Node;
                    if (this.GotoLine(Parameters[0], Parameters[1], Parameters[2]))
                    {
                        ActiveControl = this.codeText;
                        this.codeText.Select();
                        SendKeys.Send("+{TAB}+{TAB}+{TAB}");
                    }
                    else
                    {

                    }
                    UpdateBlockButtonText();
                    break;
                case "record":
                    this.SelectedDragNode = e.Node;
                    if (this.GotoLine(Parameters[0], Parameters[1], Parameters[2]))
                    {
                        ActiveControl = this.codeText;
                        this.codeText.Select();
                        SendKeys.Send("+{TAB}+{TAB}+{TAB}");
                    }
                    else
                    {

                    }
                    UpdateBlockButtonText();
                    break;
                case "field":
                    this.SelectedDragNode = e.Node;
                    if (this.GotoLine(Parameters[0], Parameters[1], Parameters[2]))
                    {
                        ActiveControl = this.codeText;
                        this.codeText.Select();
                        SendKeys.Send("+{TAB}+{TAB}+{TAB}");
                    }
                    else
                    {

                    }
                    UpdateBlockButtonText();
                    break;
                case "page":
                    this.SelectedDragNode = e.Node;
                    if (this.GotoLine(Parameters[0], Parameters[1], Parameters[2]))
                    {
                        ActiveControl = this.codeText;
                        this.codeText.Select();
                        SendKeys.Send("+{TAB}+{TAB}+{TAB}");
                    }
                    else
                    {

                    }
                    UpdateBlockButtonText();
                    break;
                case "sub":
                    this.SelectedDragNode = e.Node;
                    if (this.GotoLine(Parameters[0], Parameters[1], Parameters[2]))
                    {
                        ActiveControl = this.codeText;
                        this.codeText.Select();
                        SendKeys.Send("+{TAB}+{TAB}+{TAB}");
                    }
                    UpdateBlockButtonText();
                    break;
                default:
                    this.SelectedDragNode = e.Node;
                    UpdateBlockButtonText();
                    break;
            }
        }

        /// <summary>
        /// Mouse Down event handler for Text Area
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied mouse event args.</param>
        private void codeText_MouseDown(object sender, MouseEventArgs e)
        {
            CalculateStatus();
        }

        /// <summary>
        /// ResizeEnd Event handler for Check Code form
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied mouse event args.</param>
        private void CheckCode_ResizeEnd(object sender, EventArgs e)
        {
            //BuildComboBox();
        }

        /// <summary>
        /// Handles the Click event of the Validate Check Code button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied Event Arguments</param>
        private void ValidateCodeToolStripButton_Click(object sender, EventArgs e)
        {
            if (CompileCheckCode(this.codeText.Text))
            {
                this.AddStatusInformationMessage(string.Format("{0}: Check Code is Valid.", DateTime.Now));
            }
        }

        /// <summary>
        /// Handles the Click event of the Add Block button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied Event Arguments</param>
        private void btnAddBlock_Click(object sender, EventArgs e)
        {
            btnAddBlock.Enabled = false;
            this.AddBlockHandler();
            this.CalculateStatus();
            this.codeText.Focus();

        }

        /// <summary>
        /// Handles the Close event of the Cancel or Close buttons
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied Event Arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Drag Drop event of the Text Area
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied Drag Event Arguments</param>
        private void codeText_DragDrop(object sender, DragEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        /// <summary>
        /// Handles the Drag Enter event of the Text Area
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied Drag Event Arguments</param>
        private void codeText_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        /// <summary>
        /// Handles the On Drag Over event of the Text Area
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied Drag Event Arguments</param>
        private void codeText_OnDragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            System.Console.WriteLine("OnDragOver");
            if (validData)
            {
                if ((e.X != lastX) || (e.Y != lastY))
                {
                    SetThumbnailLocation(this.PointToClient(new Point(e.X, e.Y)));
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Print button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void btnPrint_Click(object sender, EventArgs e)
        {
            using (PrintDialog pd = new PrintDialog())
            {
                if (pd.ShowDialog() == DialogResult.OK)
                {
                    using (PrintDocument doc = new PrintDocument())
                    {
                        printFont = codeText.Font;
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

        /// <summary>
        /// Handles the Print Page event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="ev">.NET supplied print page event arguments</param>
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
            while (count < linesPerPage && (lineNo < codeText.Lines.GetUpperBound(0)))
            {
                line = codeText.Lines[lineNo];
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black, leftMargin, yPos, new StringFormat());
                count++;
                lineNo++;
            }

            // If more lines exist, print another page.
            ev.HasMorePages = (lineNo < codeText.Lines.GetUpperBound(0));
        }

        /// <summary>
        /// Handles the MouseWheel event of the Text Area textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void codeText_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Update the drawing based upon the mouse wheel scrolling.

            //int CurrentLine = this.codeText.GetLineFromCharIndex(this.codeText.SelectionStart);
            //int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            //CurrentLine += numberOfTextLinesToMove * -1;
            //if (CurrentLine > -1)
            //{
            //    int NewIndex = this.codeText.GetFirstCharIndexFromLine(CurrentLine);
            //    if (NewIndex > -1)
            //    {
            //        this.codeText.SelectionStart = NewIndex;
            //        this.codeText.SelectionLength = 0;
            //    }
            //    else
            //    {
            //        this.codeText.SelectionStart = this.codeText.Text.Length -1;
            //        this.codeText.SelectionLength = 0;
            //    }

            //}
            //else
            //{
            //    this.codeText.SelectionStart = 0;
            //    this.codeText.SelectionLength = 0;
            //}

            //CalculateStatus();
        }

        /// <summary>
        /// Handles the FormClosing event of the Check Code form.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied Form Closing Event Arguments</param>
        private void CheckCode_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (this.isDirty)
                {
                    if (MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        string checkCodeValue = this.codeText.Text;
                        bool couldCompile = this.CompileCheckCode(checkCodeValue);
                        bool goAhead = true;

                        if (couldCompile == false)
                        {
                            DialogResult result = MessageBox.Show("The check code does not compile. Do you want to save your changes anyway?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                            if (result == DialogResult.No || result == DialogResult.Cancel)
                            {
                                goAhead = false;
                            }
                        }

                        if (goAhead)
                        {
                            this.view.CheckCode = checkCodeValue;
                            GetPublishedViewKeys();
                            this.view.GetMetadata().UpdateView(this.view);
                        }
                        else
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Text Changed event of the Text Area textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void codeText_TextChanged(object sender, EventArgs e)
        {
            this.isDirty = true;
        }

        /// <summary>
        /// Handles the Click event of the Cut context menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.codeText.Cut();
        }

        /// <summary>
        /// Handles the Click event of the Copy context menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.codeText.Copy();
        }

        /// <summary>
        /// Handles the Click event of the Paste context menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.codeText.Paste();
        }

        /// <summary>
        /// Handles the Click event of the Delete context menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.codeText.SelectedText = "";
        }

        /// <summary>
        /// Handles the Shown event of Check Code form.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void CheckCode_Shown(object sender, EventArgs e)
        {
            //this.codeText.SelectionStart = 0;
            this.CalculateStatus();
            this.codeText.Focus();
        }

        /// <summary>
        /// Handles the KeyUp event of the Text Area
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied Key Event Arguments</param>
        private void codeText_KeyUp(object sender, KeyEventArgs e)
        {
            CalculateStatus();
        }

        /// <summary>
        /// Handles the Click event of the Edit Find menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuEditFind_Click(object sender, EventArgs e)
        {
            StatusTextBox.Clear();
            if (codeText.TextLength > 0)
            {
                SearchDialog dlg = new SearchDialog(codeText.SelectedText);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    currentSearchString = dlg.SearchString;
                    int currentPos = codeText.SelectionStart;
                    currentSearchOpts = (RichTextBoxFinds)0;
                    if (dlg.SearchDirection == SearchDialog.Direction.Beginning)
                    {
                        codeText.SelectionStart = 0;
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
                    if (codeText.TextLength > codeText.SelectionStart)
                    {
                        location = codeText.Find(dlg.SearchString, codeText.SelectionStart, currentSearchOpts);
                    }
                    if (location > 0)
                    {
                        codeText.SelectionStart = location;
                        codeText.SelectionLength = currentSearchString.Length;
                    }
                    else
                    {
                        StatusTextBox.Text = SharedStrings.EOF;
                        codeText.SelectionStart = currentPos;
                        codeText.SelectionLength = 0;
                    }
                }
            }
            else
            {
                StatusTextBox.Text = SharedStrings.NO_SEARCH_TEXT;
            }
        }

        /// <summary>
        /// Handles the Click event of the Edit Find Next menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuEditFindNext_Click(object sender, EventArgs e)
        {
            StatusTextBox.Clear();
            int currentPos = 0;
            if (codeText.TextLength > 0)
            {
                currentPos = codeText.SelectionStart;
                if (currentPos == codeText.TextLength)
                {
                    currentPos = 0;
                    codeText.SelectionLength = 0;
                }
                if (codeText.SelectionLength > 0)
                {
                    currentPos += codeText.SelectionLength;
                }
                int location = 0;

                if (currentPos < codeText.TextLength)
                {
                    location = codeText.Find(currentSearchString, currentPos, currentSearchOpts);
                }
                if (location > 0)
                {
                    codeText.SelectionStart = location;
                    codeText.SelectionLength = currentSearchString.Length;
                }
                else
                {
                    StatusTextBox.Text = SharedStrings.EOF;
                    codeText.SelectionStart = codeText.TextLength;
                    codeText.SelectionLength = 0;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Edit Replace menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuEditReplace_Click(object sender, EventArgs e)
        {
            if (codeText.TextLength > 0)
            {
                int location = codeText.SelectionStart;
                string text = codeText.SelectedText;
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
                        location = codeText.Find(currentSearchString, currentSearchOpts);
                        
                        if (location > 0)
                        {
                            codeText.SelectionStart = location;
                            codeText.SelectionLength = currentSearchString.Length;
                            codeText.SelectedText = dlg.ReplacementString;
                        }
                    } 
                    while (location > 0 && dlg.ReplaceAll);
                }
            }
            else
            {
                StatusTextBox.Text = SharedStrings.NO_SEARCH_TEXT;
            }
        }

        /// <summary>
        /// Handles the Click event of the Edit Undo menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuEditUndo_Click(object sender, EventArgs e)
        {
            this.codeText.Undo();
        }

        /// <summary>
        /// Handles the Click event of the Edit Redo menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuEditRedo_Click(object sender, EventArgs e)
        {
            this.codeText.Redo();
        }

        /// <summary>
        /// Handles the Click event of the Edit Cut menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuEditCut_Click(object sender, EventArgs e)
        {
            this.codeText.Cut();
        }

        /// <summary>
        /// Handles the Click event of the Edit Copy menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuEditCopy_Click(object sender, EventArgs e)
        {
            this.codeText.Copy();
        }

        /// <summary>
        /// Handles the Click event of the Edit Past  menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuEditPaste_Click(object sender, EventArgs e)
        {
            this.codeText.Paste();
        }

        /// <summary>
        /// Handles the Click event of the Select All menu item.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuSelectAll_Click(object sender, EventArgs e)
        {
            this.codeText.Select(0, codeText.TextLength);
        }

        /// <summary>
        /// Handles the Click event of the Set Editor Fonts menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuFontsSetEditorFont_Click(object sender, EventArgs e)
        {
            CheckCodeFontDialog.ShowDialog(this);
            printFont = CheckCodeFontDialog.Font;
            codeText.Font = printFont;
        }

        /// <summary>
        /// Handles the Click event of the File Save As Text menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Check Code files (*.chk)|*.chk|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.OverwritePrompt = true;
            DialogResult result = saveFileDialog.ShowDialog();
            
            if (result == DialogResult.OK)
            {
                System.IO.TextWriter file = new System.IO.StreamWriter(saveFileDialog.FileName);
                file.Write(codeText.Text);
                file.Close();
            }
        }

        private void GetPublishedViewKeys()
        {
            DataTable table = this.mainForm.mediator.Project.Metadata.GetPublishedViewKeys(this.view.Id);
            DataRow ViewRow = table.Rows[0];
            string EWEFormId = ViewRow.ItemArray[3].ToString();
            this.view.EWEFormId = EWEFormId;
            string OrganizationKey = ViewRow.ItemArray[2].ToString();
            this.view.EWEOrganizationKey = OrganizationKey;           
        }

        #endregion  //Event Handlers

        #region Public Methods

        /// <summary>
        /// Go To Line
        /// </summary>
        /// <param name="pLevel">The level of the check code block (eg. View; Page; Record; Field)</param>
        /// <param name="pEvent">The event of the block (eg. before; after)</param>
        /// <param name="pIdentifier">The name of the control (eg. Page name; field name).</param>
        /// <returns>True if the Check Code block is found.</returns>
        public bool GotoLine(string pLevel, string pEvent, string pIdentifier, bool createIfNotThere = false)
        {
            bool result = false;
            this.codeText.SelectionStart = 0;
            this.codeText.SelectionLength = 0;

            int index = findBlock(pLevel, pEvent, pIdentifier);

            if (index > -1)
            {
                int Findline = codeText.GetLineFromCharIndex(index);
                
                if (codeText.Text[index] == '\n')
                {
                    Findline++;
                }

                int length = codeText.Lines[Findline].Length;
                int start = codeText.GetFirstCharIndexFromLine(Findline) + length + 1;
                this.codeText.SelectionStart = start;
                this.codeText.SelectionLength = 0;

                CalculateStatus();

                result = true;
            }
            else
            {
                if (createIfNotThere)
                {
                    this.AddBlock(pLevel, pEvent, pIdentifier);
                    this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint(pLevel, pEvent, pIdentifier);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Parses the text from the dialog and inserts it into the Text Area.
        /// </summary>
        public void PopulateTextArea()
        {
            this.ParseText(this.view.CheckCode);
        }

        /// <summary>
        /// Parses the text from the dialog and inserts it into the Text Area.
        /// </summary>
        public void ParseText(string pText)
        {
            Regex r = new Regex("\\n");
            String[] lines = r.Split(pText);
            int linecount = lines.GetUpperBound(0);
            int linenum = 0;
            foreach (string l in lines)
            {
                ParseLine(l);
                linenum++;
                // add line return after each line except last
                if (linenum <= linecount) codeText.SelectedText = "\n";
            }
        }

        /// <summary>
        /// Parses the text from the dialog and inserts it into the Text Area.
        /// </summary>
        public void ParseLine(string line)
        {
            Regex r = new Regex("(\".*\")|(\\([.+-]\\))|([ \\t{}():;=+-/%^&])");
            String[] tokens = r.Split(line);

            foreach (string token in tokens)
            {
                if (this.keywords.Contains(token.Trim().ToLower()))
                {
                    codeText.SelectionColor = Color.Blue;
                    codeText.SelectionFont = printFont;
                }
                else if (this.operators.Contains(token.Trim().ToLower()))
                {
                    codeText.SelectionColor = Color.Brown;
                    codeText.SelectionFont = printFont;
                }
                else
                {
                    codeText.SelectionColor = Color.Black;
                    codeText.SelectionFont = printFont;
                }

                codeText.SelectedText = token;
            }
        }

        /// <summary>
        /// Input box for adding a Sub-routine to the Check Code
        /// </summary>
        /// <param name="title">Title of the input box.</param>
        /// <param name="promptText">Question for the input box.</param>
        /// <param name="value">Text of the question to ask of the user.</param>
        /// <returns>Returns the dialog result.</returns>
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 40, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        #endregion //Public Methods

        #region Private Methods

        /// <summary>
        /// Calculate Status method
        /// </summary>
        /// <remarks>
        /// Calculates the line number, column, and overall position of the insertion point.
        /// </remarks>
        private void CalculateStatus()
        {
            int Findline, Findcol, Findindex;

            Findindex = codeText.SelectionStart;
            Findline = codeText.GetLineFromCharIndex(Findindex);
            int TabCount;
            string Diff = null;
            if (codeText.SelectionStart > 0)
            {
                Findcol = codeText.Text.LastIndexOf("\n", codeText.SelectionStart - 1);
            }
            else
            {
                Findcol = codeText.Text.LastIndexOf("\n", 0);
            }

            if (Findcol > -1)
            {
                Diff = codeText.Text.Substring(Findcol, codeText.SelectionStart - Findcol);
                TabCount = Regex.Matches(Diff, "\t").Count;
                Findcol = codeText.SelectionStart - Findcol + (TabCount * 5);
            }
            else
            {
                Diff = codeText.Text.Substring(0, codeText.SelectionStart);
                TabCount = Regex.Matches(Diff, "\t").Count;
                Findcol = codeText.SelectionStart + 1 + (TabCount * 5);
            }

            Findline++;
            LineNumberLabel.Text = Findline.ToString();
            ColumnNumberLabel.Text = Findcol.ToString();
            PositionNumberLabel.Text = codeText.SelectionStart.ToString();
        }

        /// <summary>
        /// Handles the Field Command
        /// </summary>
        /// <param name="command">Enums.FieldCommands</param>
        private void DesignFieldCommand(Enums.FieldCommands command)
        {
            switch (command)
            {
                case Enums.FieldCommands.Clear:
                    if (this.PreValidateCommand(" Clear Address "))
                    {
                        DesignStatement(new ClearDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Clear command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.GoTo:
                    if (this.PreValidateCommand(" Goto 1 "))
                    {
                        DesignStatement(new GoToDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Goto command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                //---GotoForm
                case Enums.FieldCommands.GoToForm:
                    if (this.PreValidateCommand(" Gotoform relatedform "))
                    {
                        DesignStatement(new GoToFormDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Gotoform command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                //
                case Enums.FieldCommands.Hide:
                    if (this.PreValidateCommand(" Hide Address "))
                    {
                        DesignStatement(new HideDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Hide command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.Unhide:
                    if (this.PreValidateCommand(" UnHide Address "))
                    {
                        DesignStatement(new UnhideDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("UnHide command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.Disable:
                    if (this.PreValidateCommand(" Hide Address "))
                    {
                        DesignStatement(new DisableDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Disable command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.Enable:
                    if (this.PreValidateCommand(" UnHide Address "))
                    {
                        DesignStatement(new EnableDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Enable command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.Highlight:
                    if (this.PreValidateCommand(" Highlight Address "))
                    {
                        DesignStatement(new HighlightDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Highlight command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.Unhighlight:
                    if (this.PreValidateCommand(" Unhighlight Address "))
                    {
                        DesignStatement(new UnhighlightDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Unhighlight command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.Geocode:
                    if (this.PreValidateCommand("Geocode Address, Latitude, Longitude"))
                    {
                        DesignStatement(new GeocodeDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Geocode command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.Set_Required:
                    if (this.PreValidateCommand("Set-Required Address"))
                    {
                        DesignStatement(new SetRequiredDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Set-Required command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.FieldCommands.Set_Not_Required:
                    if (this.PreValidateCommand("Set-Not-Required Address"))
                    {
                        DesignStatement(new SetNotRequiredDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Set-Not-Required command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the program command
        /// </summary>
        /// <param name="command">Enums.ProgramCommands</param>
        private void DesignProgramCommand(Enums.ProgramCommands command)
        {
            switch (command)
            {
                case Enums.ProgramCommands.Call:
                    if (this.PreValidateCommand(" Call SubroutineName "))
                    {
                        DesignStatement(new CallDialog(this));
                    }
                    else
                    {
                        AddStatusErrorMessage("Execute command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.ProgramCommands.Execute:
                    if (this.PreValidateCommand(" EXECUTE 'c:\\text.txt' "))
                    {
                        DesignStatement(new ExecuteDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Execute command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.ProgramCommands.Quit:
                    if (this.PreValidateCommand(" Quit "))
                    {
                        DesignStatement(new QuitDialog(mainForm, true));
                    }
                    else
                    {
                        AddStatusErrorMessage("Quit command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the record command
        /// </summary>
        /// <param name="command">Enums.RecordCommands</param>
        private void DesignRecordCommand(Enums.RecordCommands command)
        {
            switch (command)
            {
                case Enums.RecordCommands.AutoSearch:
                    if (this.PreValidateCommand(" AUTOSEARCH vaccinated "))
                    {
                        DesignStatement(new AutoSearchDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("Autosearch command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.RecordCommands.If:
                    if (this.PreValidateCommand(" If " + Constants.VARIABLE_NAME_TEST_TOKEN + " > 10 Then NewRecord End-If"))
                    {
                        DesignStatement(new IfClauseDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("If command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.RecordCommands.NewRecord:
                    if (this.PreValidateCommand(" NewRecord "))
                    {
                        DesignStatement(new NewRecordDialog(mainForm));
                    }
                    else
                    {
                        AddStatusErrorMessage("NewRecord command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the UIC command
        /// </summary>
        /// <param name="command">Enums.UserInteractionCommands</param>
        private void DesignUICommand(Enums.UserInteractionCommands command)
        {
            switch (command)
            {
                case Enums.UserInteractionCommands.Dialog:
                    if (this.PreValidateCommand("Dialog \"\" TitleText=\"\""))
                    {
                        DesignStatement(new DialogDialog(mainForm, this.view.GetProject()));
                    }
                    else
                    {
                        AddStatusErrorMessage("Dialog command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                case Enums.UserInteractionCommands.Help:
                    if (this.PreValidateCommand(" HELP 'C:\\Epi_Info\\English\\Help\\EIHelp.chm' \"\" "))
                    {
                        DesignHelpStatement();
                    }
                    else
                    {
                        AddStatusErrorMessage("Help command: invalid cursor location.\nPlease place cursor inside a code block.");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the Variable command
        /// </summary>
        /// <param name="command">Enums.VariableCommands</param>
        private void DesignVariableCommand(Enums.VariableCommands command)
        {
            switch (command)
            {
                case Enums.VariableCommands.Assign:

                    try
                    {
                        if (this.PreValidateCommand(" Assign " + Constants.VARIABLE_NAME_TEST_TOKEN + " = 10 "))
                        {
                            DesignStatement(new AssignDialog(mainForm, true));
                        }
                        else
                        {
                            AddStatusErrorMessage("Assign  command: Please place cursor inside a code block.");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Source == Constants.VARIABLE_NAME_TEST_TOKEN)
                        {
                            DesignStatement(new AssignDialog(mainForm, true));
                        }
                        else
                        {
                            AddStatusErrorMessage("Assign  command: Please place cursor inside a code block.");
                        }
                    }
                    break;

                case Enums.VariableCommands.Define:
                
                    if (EpiInterpreter.Context.Scope.Resolve(Constants.VARIABLE_NAME_TEST_TOKEN) != null)
                    {
                        EpiInterpreter.Context.Scope.Undefine(Constants.VARIABLE_NAME_TEST_TOKEN);
                    }
                    
                    if (this.PreValidateCommand(" Define " + Constants.VARIABLE_NAME_TEST_TOKEN + " Numeric "))
                    {
                        isDefineCommand = true;
                        DesignStatement(new DefineVariableDialog(mainForm, true));
                    }
                    else
                    {
                        AddStatusErrorMessage("Define command: Please place cursor inside the DefineVariables code block.");
                    }
                    
                    break;
                
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the Check Code commands
        /// </summary>
        /// <param name="dialog">The check code command dialog to be displayed </param>
        private void DesignStatement(CheckCodeDesignDialog dialog)
        {
            dialog.View = view;
            DialogResult result = ((Form)dialog).ShowDialog();
            if (result == DialogResult.OK)
            {
                if (CheckCodeCommandDesigned != null)
                {
                    CheckCodeCommandDesigned(dialog.Output);
                }
                ((Form)dialog).Close();
            }
        }

        /// <summary>
        /// Handles the Check Code commands
        /// </summary>
        /// <param name="dialog"></param>
        private void DesignStatement(Epi.Windows.MakeView.Dialogs.CommandDesignDialog dialog)
        {
            DialogResult result = ((Form)dialog).ShowDialog();
            if (result == DialogResult.OK)
            {
                if (CheckCodeCommandDesigned != null)
                {
                    if (!isDefineCommand)
                    {
                        //Add check code to the editor
                        CheckCodeCommandDesigned(dialog.CommandText);
                    }
                    //Execute check code
                    //                    CommandProcessorResults results = ((MakeViewWindowsModule)dialog.Module).CommandProcessor.RunCommands(dialog.CommandText);

                    if (isDefineCommand)
                    {
                        //MessageBox.Show(dialog.CommandText + StringLiterals.COLON + StringLiterals.SPACE + SharedStrings.DEFINED_VARIABLES_SECTION);
                        ((MakeViewMainForm)mainForm).EpiInterpreter.Parse("DefineVariables " + dialog.CommandText + " End-DefineVariables");
                        CheckCodeCommandDesigned(dialog.CommandText);
                        isDefineCommand = false;
                    }

                    //results = null;
                }
                ((Form)dialog).Close();
            }
            else
            {
                if (isDefineCommand == true) { isDefineCommand = false; }
            }
        }

        /// <summary>
        /// Shows the Help Config dialog
        /// </summary>
        private void DesignHelpStatement()
        {
            DesignStatement(new HelpDialog(mainForm));
        }

        private void tvCommands_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (e.Node.Parent != null && e.Node.Parent.Parent != null)
                {
                    switch ((Enums.CommandGroups)e.Node.Parent.Index)
                    {
                        case Enums.CommandGroups.Fields:
                            DesignFieldCommand((Enums.FieldCommands)e.Node.Index);
                            break;
                        case Enums.CommandGroups.Programs:
                            DesignProgramCommand((Enums.ProgramCommands)e.Node.Index);
                            break;
                        case Enums.CommandGroups.Records:
                            DesignRecordCommand((Enums.RecordCommands)e.Node.Index);
                            break;
                        case Enums.CommandGroups.UserInteraction:
                            DesignUICommand((Enums.UserInteractionCommands)e.Node.Index);
                            break;
                        case Enums.CommandGroups.Variables:
                            DesignVariableCommand((Enums.VariableCommands)e.Node.Index);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MsgBox.ShowException(ex);
            }
            finally
            {
            }
        }

        private void Commands_SelectedIndexChanged(object sender, EventArgs e)
        {
            string commandName = ((ListControl)sender).Text;
            AddCommand(commandName);
        }

        private void AddCommand(string commandName)
        {
            try
            {
                switch (commandName.ToLower())
                {
                    case "clear":
                        DesignFieldCommand(Enums.FieldCommands.Clear);
                        break;
                    case "goto":
                        DesignFieldCommand(Enums.FieldCommands.GoTo);
                        break;
                    case "gotoform":
                        DesignFieldCommand(Enums.FieldCommands.GoToForm);
                        break;
                    case "hide":
                        DesignFieldCommand(Enums.FieldCommands.Hide);
                        break;
                    case "unhide":
                        DesignFieldCommand(Enums.FieldCommands.Unhide);
                        break;
                    case "disable":
                        DesignFieldCommand(Enums.FieldCommands.Disable);
                        break;
                    case "enable":
                        DesignFieldCommand(Enums.FieldCommands.Enable);
                        break;
                    case "highlight":
                        DesignFieldCommand(Enums.FieldCommands.Highlight);
                        break;
                    case "unhighlight":
                        DesignFieldCommand(Enums.FieldCommands.Unhighlight);
                        break;
                    case "geocode":
                        DesignFieldCommand(Enums.FieldCommands.Geocode);
                        break;
                    case "set-required":
                        DesignFieldCommand(Enums.FieldCommands.Set_Required);
                        break;
                    case "set-not-required":
                        DesignFieldCommand(Enums.FieldCommands.Set_Not_Required);
                        break;
                    case "call":
                        DesignProgramCommand(Enums.ProgramCommands.Call);
                        break;
                    case "execute":
                        DesignProgramCommand(Enums.ProgramCommands.Execute);
                        break;
                    case "quit":
                        DesignProgramCommand(Enums.ProgramCommands.Quit);
                        break;
                    case "autosearch":
                        DesignRecordCommand(Enums.RecordCommands.AutoSearch);
                        break;
                    case "if":
                        DesignRecordCommand(Enums.RecordCommands.If);
                        break;
                    case "newrecord":
                        DesignRecordCommand(Enums.RecordCommands.NewRecord);
                        break;
                    case "dialog":
                        DesignUICommand(Enums.UserInteractionCommands.Dialog);
                        break;
                    case "help":
                        DesignUICommand(Enums.UserInteractionCommands.Help);
                        break;
                    case "assign":
                        DesignVariableCommand(Enums.VariableCommands.Assign);
                        break;
                    case "define":
                        DesignVariableCommand(Enums.VariableCommands.Define);
                        break;

                    default:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                MsgBox.ShowException(ex);
            }
        }

        private bool PreValidateCommand(string pCommandText)
        {
            bool result = false;
            string start = null;
            string finish = null;

            start = codeText.Text.Substring(0, codeText.SelectionStart);
            finish = codeText.Text.Substring(codeText.SelectionStart, codeText.Text.Length - codeText.SelectionStart);

            try
            {
                this.mainForm.EpiInterpreter.Parse(string.Format("{0} {1} {2} ", start, pCommandText, finish));
                result = true;
            }
            catch (ParseException ex)
            {
                /*
                int LineNumber = ex.UnexpectedToken.Location.LineNr;
                int ColumnNumber = ex.UnexpectedToken.Location.ColumnNr;
                LineNumber++;

                StatusTextBox.ForeColor = Color.Black;
                StatusTextBox.SelectedText = string.Format("Error on line number: {0} Column number {1}", LineNumber, ColumnNumber);
                StatusTextBox.SelectedText = "\n" + ex.Message;
                int start = codeText.GetFirstCharIndexFromLine(ex.UnexpectedToken.Location.LineNr);
                int length = codeText.Lines[ex.UnexpectedToken.Location.LineNr].Length;
                codeText.Select(start, length);
                codeText.SelectionColor = Color.Red;
                codeText.Select(start, 0);*/
            }
            return result;
        }

        private void AddStatusErrorMessage(string pText)
        {
            int start = 0;
            int length = pText.Length;
            StatusTextBox.Text = pText;
            StatusTextBox.Select(start, length);
            StatusTextBox.SelectionColor = Color.Red;
            StatusTextBox.Select(start, 0);
        }

        private void AddStatusInformationMessage(string pText)
        {
            int start = 0;
            int length = pText.Length;
            StatusTextBox.Text = pText;
            StatusTextBox.Select(start, length);
            StatusTextBox.SelectionColor = Color.Black;
            StatusTextBox.Select(start, 0);
        }

        public bool AddBlock(string pLevel, string pEvent, string pIdentifier)
        {
            bool result = false;
            bool isBlockAlreadyAdded = false;

            string Identifier = null;

            StringBuilder CheckCodeBlock = new StringBuilder();
            ICommand command;
            ICommandContext commandContext = this.EpiInterpreter.Context;

            if (this.codeText.Text.Length > 0)
            {
                this.codeText.SelectionStart = this.codeText.Text.Length;
            }
            else
            {
                this.codeText.SelectionStart = 0;
            }

            CheckCodeBlock.Append("\n");

            if (!string.IsNullOrEmpty(pIdentifier))
            {
                if (pIdentifier.Trim().IndexOf(' ') > -1 && pIdentifier.IndexOf('[') < 0)
                {
                    Identifier = "[" + pIdentifier.Trim() + "]";
                }
                else
                {
                    Identifier = pIdentifier.Trim();
                }
            }

            string caseKey = pLevel.ToLower() + "_" + pEvent.ToLower();

            switch (caseKey)
            {

                case "definevariables_":
                    if (this.findBlock("definevariables", "", "") > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=definevariables&event=&identifier=");
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        CheckCodeBlock.Append("DefineVariables\n\n\t\t//add code here\n\nEnd-DefineVariables\n");
                        codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, pIdentifier);
                        this.ParseText(CheckCodeBlock.ToString());
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("DefineVariables block: This check code block already exists.");
                    }
                    break;
                case "view_before":
                    if (this.findBlock("view", "before", "") > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=view&event=before&identifier=");
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        if (this.findBlock("view", "", "") > -1)
                        {
                            CheckCodeBlock.Append("\tBefore\n\t\t//add code here\n\n\tEnd-Before\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, pIdentifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("View\n\tBefore\n\t\t//add code here\n\n\tEnd-Before\nEnd-View\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("View before: This check code block already exists.");
                    }
                    break;
                case "form_before":
                    if (this.findBlock("form", "before", "") > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=form&event=before&identifier=");
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        if (this.findBlock("form", "", "") > -1)
                        {
                            CheckCodeBlock.Append("\tBefore\n\t\t//add code here\n\n\tEnd-Before\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, pIdentifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Form\n\tBefore\n\t\t//add code here\n\n\tEnd-Before\nEnd-Form\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("View before: This check code block already exists.");
                    }
                    break;
                case "view_after":
                    if (this.findBlock("view", "after", "") > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=view&event=after&identifier=");
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        if (this.findBlock("form", "", "") > -1)
                        {
                            CheckCodeBlock.Append("\tAfter\n\t\t//add code here\n\n\tEnd-After\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, pIdentifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("View\n\tAfter\n\t\t//add code here\n\n\tEnd-After\nEnd-View\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("View after: This check code block already exists.");
                    }
                    break;
                case "form_after":
                    if (this.findBlock("form", "after", "") > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=form&event=after&identifier=");
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        if (this.findBlock("form", "", "") > -1)
                        {
                            CheckCodeBlock.Append("\tAfter\n\t\t//add code here\n\n\tEnd-After\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, pIdentifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Form\n\tAfter\n\t\t//add code here\n\n\tEnd-After\nEnd-Form\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("View after: This check code block already exists.");
                    }
                    break;
                case "record_before":
                    if (this.findBlock("record", "before", "") > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=record&event=before&identifier=");
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {

                        if (this.findBlock("record", "", "") > -1)
                        {
                            CheckCodeBlock.Append("\tBefore\n\t\t//add code here\n\n\tEnd-Before\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, pIdentifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Record\n\tBefore\n\t\t//add code here\n\n\tEnd-Before\nEnd-Record\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("Record before: This check code block already exists.");
                    }
                    break;
                case "record_after":
                    if (this.findBlock("record", "after", "") > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=record&event=after&identifier=");
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {

                        if (this.findBlock("record", "", "") > -1)
                        {
                            CheckCodeBlock.Append("\tAfter\n\t\t//add code here\n\n\tEnd-After\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, pIdentifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Record\n\tAfter\n\t\t//add code here\n\n\tEnd-After\nEnd-Record\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("Record after: This check code block already exists.");
                    }
                    break;
                case "page_before":
                    if (this.findBlock("page", "before", Identifier) > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=page&event=before&identifier=" + Identifier);
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {

                        if (this.findBlock("page", "", Identifier) > -1)
                        {
                            CheckCodeBlock.Append("\tBefore\n\t\t//add code here\n\n\tEnd-Before\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, Identifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Page ");
                            CheckCodeBlock.Append(Identifier);
                            CheckCodeBlock.Append("\n\tBefore\n\t\t//add code here\n\n\tEnd-Before\nEnd-Page\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("Page " + Identifier + " before: This check code block already exists.");
                    }
                    break;
                case "page_after":
                    if (this.findBlock("page", "after", Identifier) > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=page&event=after&identifier=" + Identifier);
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {

                        if (this.findBlock("page", "", Identifier) > -1)
                        {
                            CheckCodeBlock.Append("\tAfter\n\t\t//add code here\n\n\tEnd-After\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, Identifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Page ");
                            CheckCodeBlock.Append(Identifier);
                            CheckCodeBlock.Append("\n\tAfter\n\t\t//add code here\n\n\tEnd-After\nEnd-Page\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("Page " + Identifier + " after: This check code block already exists.");
                    }
                    break;
                case "field_before":
                    if (this.findBlock("field", "before", Identifier) > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=field&event=before&identifier=" + Identifier);
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {



                        if (this.findBlock("field", "", Identifier) > -1)
                        {
                            CheckCodeBlock.Append("\tBefore\n\t\t//add code here\n\n\tEnd-Before\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, Identifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Field ");
                            CheckCodeBlock.Append(Identifier);
                            CheckCodeBlock.Append("\n\tBefore\n\t\t//add code here\n\n\tEnd-Before\nEnd-Field\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("Field " + Identifier + " before: This check code block already exists.");
                    }
                    break;
                case "field_after":
                case "field_":

                    if (this.findBlock("field", "after", Identifier) > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=field&event=after&identifier=" + Identifier);
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        if (this.findBlock("field", "", Identifier) > -1)
                        {
                            CheckCodeBlock.Append("\tAfter\n\t\t//add code here\n\n\tEnd-After\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, Identifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Field ");
                            CheckCodeBlock.Append(Identifier);
                            CheckCodeBlock.Append("\n\tAfter\n\t\t//add code here\n\n\tEnd-After\nEnd-Field\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("Field " + Identifier + " after: This check code block already exists.");
                    }

                    break;

                case "field_click":
                    if (this.findBlock("field", "click", Identifier) > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=field&event=click&identifier=" + Identifier);
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {

                        if (this.findBlock("field", "", Identifier) > -1)
                        {
                            CheckCodeBlock.Append("\tClick\n\t\t//add code here\n\n\tEnd-Click\n");
                            codeText.SelectionStart = this.SetInsertionPoint(pLevel, pEvent, Identifier);
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        else
                        {
                            CheckCodeBlock.Append("Field ");
                            CheckCodeBlock.Append(Identifier);
                            CheckCodeBlock.Append("\n\tClick\n\t\t//add code here\n\n\tEnd-Click\nEnd-Field\n");
                            this.ParseText(CheckCodeBlock.ToString());
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("Field " + Identifier + " click: This check code block already exists.");
                    }
                    break;
                case "sub_":
                    if (this.findBlock("sub", "", Identifier) > -1)
                    {
                        isBlockAlreadyAdded = true;
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        command = commandContext.GetCommand("level=sub&event=&identifier=" + Identifier);
                        if (command == null || command.IsNull())
                        {
                            result = true;
                        }
                        else
                        {
                            isBlockAlreadyAdded = true;
                        }
                    }

                    if (!isBlockAlreadyAdded)
                    {
                        CheckCodeBlock.Append("Sub ");
                        CheckCodeBlock.Append(Identifier);
                        CheckCodeBlock.Append("\n\t//add code here\n\t\n\nEnd-Sub\n");
                        this.ParseText(CheckCodeBlock.ToString());
                        for (int i = 0; i < tvCodeBlocks.Nodes.Count; i++)
                        {
                            if (tvCodeBlocks.Nodes[i].Text.Trim().ToLower() == "subroutine(s)")
                            {
                                TreeNode treeNode = new TreeNode(Identifier);
                                treeNode.Tag = "level=sub&event=&identifier=" + Identifier;
                                this.tvCodeBlocks.Nodes[i].Nodes.Add(treeNode);
                            }
                        }
                        result = true;
                    }
                    else
                    {
                        AddStatusErrorMessage("Can NOT add Sub " + Identifier + " : This check code block already exists.");
                    }

                    break;
            }
            if (!isBlockAlreadyAdded)
            {
                //this.EpiInterpreter.Execute(CheckCodeBlock.ToString());

                //this.BuildComboBox();
            }
            return result;
        }

        protected void SetThumbnailLocation(Point p)
        {
            /*
          if (thumbnail.Image==null)
          {
            thumbnail.Visible=false;
          }
          else
          {
            p.X-=thumbnail.Width/2;
            p.Y-=thumbnail.Height/2;
            thumbnail.Location=p;
            thumbnail.Visible=true;
          }*/
        }

        private void UpdateBlockButtonText()
        {
            string[] Parameters = this.ConvertParameters(this.SelectedDragNode.Tag.ToString());

            btnAddBlock.Text = SharedStrings.ADD_BLOCK + ": ";
            btnAddBlock.Enabled = false;

            if (this.SelectedDragNode == null)
            {
                return;
            }

            Parameters = this.ConvertParameters(this.SelectedDragNode.Tag.ToString());

            if (findBlock(Parameters[0], Parameters[1], Parameters[2]) > -1)
            {
                return;
            }

            if (string.IsNullOrEmpty(Parameters[1]))
            {
                if (
                    Parameters[0].ToLower() != "definevariables"
                    && Parameters[0].ToLower() != "sub"
                )
                {

                    return;
                }

                if (Parameters[0].ToLower() == "sub" && string.IsNullOrEmpty(Parameters[2]))
                {
                    return;
                }
            }

            if (string.IsNullOrEmpty(Parameters[2]))
            {
                btnAddBlock.Text = string.Format(SharedStrings.ADD_BLOCK + ": {0} {1}", Parameters[0], Parameters[1], Parameters[2]);
            }
            else
            {
                btnAddBlock.Text = string.Format(SharedStrings.ADD_BLOCK + ": {2} {1}", Parameters[0], Parameters[1], Parameters[2]);
            }
            btnAddBlock.Enabled = true;

        }

        public int findBlock(string pLevel, string pEvent, string pIdentifier)
        {
            int result = -1;
            System.Text.RegularExpressions.Regex re = null;
            MatchCollection MC;
            int index1, index2;

            if (string.IsNullOrEmpty(pIdentifier))
            {                
                if (string.IsNullOrEmpty(pEvent))
                {
                    re = new Regex(string.Format("\\b{0}\\b[\\w\\W]*?\\bend-{0}\\b", pLevel), RegexOptions.IgnoreCase);
                    MC = re.Matches(this.codeText.Text);
                    if (MC.Count > 0)
                    {
                        result = MC[0].Index;
                    }
                }
                else
                {
                    re = new Regex(string.Format("\\b{0}\\b[\\w\\W]*\\b{1}\\b[\\w\\W]*\\bend-{0}\\b", pLevel, pEvent), RegexOptions.IgnoreCase);
                    MC = re.Matches(this.codeText.Text);
                    if (MC.Count > 0)
                    {
                        index2 = this.codeText.Text.ToLower().IndexOf(pEvent, MC[0].Index);
                        if (index2 > -1)
                        {
                            result = index2;
                        }
                        else
                        {
                            result = MC[0].Index;
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(pEvent))
                {
                    re = new Regex(string.Format("\\b{0}\\s*\\[?{1}\\]?\\s[\\w\\W]*?", pLevel, pIdentifier.Replace("[", "").Replace("]", "")), RegexOptions.IgnoreCase);
                    MC = re.Matches(this.codeText.Text);
                    if (MC.Count > 0)
                    {
                        if (re.ToString().Contains("bpage"))
                        {
                            try
                            {
                                string s1 = this.codeText.Text.Substring(this.codeText.Text.IndexOf("Page " + pIdentifier)).Replace("\r\n\t", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty);
                                if (s1.Substring(0, pIdentifier.Length + 5) == "Page " + pIdentifier)
                                {
                                    for (int i = 0; i < MC.Count; i++)
                                    {
                                        if (MC[i].ToString().IndexOf('[') > 0 && pIdentifier.IndexOf(']') > 0)
                                            result = MC[i].Index;
                                        else if (MC[i].ToString().IndexOf('[') < 0 && pIdentifier.IndexOf(']') < 0)
                                            result = MC[i].Index;
                                    }
                                }
                                else
                                    result = MC[0].Index;
                            }
                            catch (Exception)
                            {
                                result = -1;
                                return result;
                            }
                        }
                        else
                            result = MC[0].Index;
                    }
                }
                else
                {
                    re = new Regex(string.Format("\\b{0}\\s*\\[?{1}\\]?\\s[\\w\\W]*?", pLevel, pIdentifier.Replace("[", "").Replace("]", ""), pEvent), RegexOptions.IgnoreCase);
                    MC = re.Matches(this.codeText.Text);
                    if (MC.Count > 0)
                    {
                        for (int i = 0; i < MC.Count; i++)
                        {
                            if (MC[i].ToString().IndexOf('[') > 0 && pIdentifier.IndexOf(']') > 0)
                            {
                                index1 = MC[i].Index;
                                index2 = this.codeText.Text.ToLower().IndexOf("end-" + pLevel, index1);
                                re = new Regex(string.Format("\\b{0}\\b", pEvent), RegexOptions.IgnoreCase);
                                MC = re.Matches(this.codeText.Text.Substring(index1, index2 - index1));
                                if (MC.Count > 0)
                                    return result = index1 + MC[0].Index;
                            }
                            else if (MC[i].ToString().IndexOf('[') < 0 && pIdentifier.IndexOf(']') < 0)
                            {
                                index1 = MC[i].Index;
                                index2 = this.codeText.Text.ToLower().IndexOf("end-" + pLevel, index1);
                                re = new Regex(string.Format("\\b{0}\\b", pEvent), RegexOptions.IgnoreCase);
                                MC = re.Matches(this.codeText.Text.Substring(index1, index2 - index1));
                                if (MC.Count > 0)
                                    return result = index1 + MC[0].Index;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public string[] ConvertParameters(string pQueryString)
        {
            string[] result = new string[3];
            string[] NameValues = pQueryString.Split('&');
            result[0] = NameValues[0].Split('=')[1]; // level
            result[1] = NameValues[1].Split('=')[1];  // event
            result[2] = NameValues[2].Split('=')[1];  // field

            return result;
        }

        public int SetInsertionPoint(string pLevel, string pEvent, string pIdentifier)
        {
            int start = findBlock(pLevel, "", pIdentifier);
            int OldStart;
            if (start == -1)
            {
                start = this.codeText.Text.Length;
                return start;
            }


            switch (pLevel.ToLower())
            {
                /*
            case "definevariables":
            case "sub":
                // this should never happen ie there should only be 1 block
                break;*/

                case "view":
                    switch (pEvent.ToLower())
                    {
                        case "":
                            break;
                        case "before":
                            start = this.codeText.Text.ToLower().IndexOf("after", start) - 2;
                            break;
                        case "after":
                            start = this.codeText.Text.ToLower().IndexOf("end-view", start);
                            break;
                    }
                    break;
                case "form":
                    switch (pEvent.ToLower())
                    {
                        case "":
                            break;
                        case "before":
                            start = this.codeText.Text.ToLower().IndexOf("after", start) - 2;
                            break;
                        case "after":
                            start = this.codeText.Text.ToLower().IndexOf("end-form", start);
                            break;
                    }
                    break;
                case "record":
                    switch (pEvent.ToLower())
                    {
                        case "":
                            break;
                        case "before":
                            start = this.codeText.Text.ToLower().IndexOf("after", start) - 2;
                            break;
                        case "after":
                            start = this.codeText.Text.ToLower().IndexOf("end-record", start);
                            break;
                    }
                    break;


                case "page":
                    switch (pEvent.ToLower())
                    {
                        case "":
                            break;
                        case "before":
                            //suggest upping this to -2
                            OldStart = start;
                            start = this.codeText.Text.ToLower().IndexOf("after", start) - 2;
                            if (start < 0 || start > this.codeText.Text.ToLower().IndexOf("end-page", OldStart))
                            {
                                start = this.codeText.Text.IndexOf(pIdentifier, OldStart) + pIdentifier.Length;
                            }
                            break;
                        case "after":
                            OldStart = start;
                            start = this.codeText.Text.ToLower().IndexOf("end-page", start);
                            break;
                    }
                    break;
                case "field":
                    switch (pEvent.ToLower())
                    {
                        case "before":
                            OldStart = start;
                            //start = this.codeText.Text.ToLower().IndexOf("after", start) - 2;
                            start = this.codeText.Text.ToLower().IndexOf("after", start);
                            if (start < 0 || start > this.codeText.Text.ToLower().IndexOf("end-field", OldStart))
                            {
                                start = this.codeText.Text.ToLower().IndexOf("click", OldStart) - 2;
                            }

                            if (start < 0 || start > this.codeText.Text.ToLower().IndexOf("end-field", OldStart))
                            {
                                start = this.codeText.Text.IndexOf(pIdentifier, OldStart) + pIdentifier.Length;
                            }

                            break;
                        case "after":
                            OldStart = start;
                            start = this.codeText.Text.ToLower().IndexOf("end-before", start);
                            if (start < 0 || start > this.codeText.Text.ToLower().IndexOf("end-field", OldStart))
                            {
                                start = this.codeText.Text.ToLower().IndexOf("click", OldStart) - 2;

                                if (start < 0 || start > this.codeText.Text.ToLower().IndexOf("end-field", OldStart))
                                {
                                    start = this.codeText.Text.IndexOf(pIdentifier, OldStart) + pIdentifier.Length;
                                }
                            }
                            else // a before block exists
                            {
                                start += "end-before".Length;
                            }
                            break;
                        case "click":
                            start = this.codeText.Text.ToLower().IndexOf("end-field", start);
                            break;
                    }
                    break;
            }

            return start;
        }

        /// <summary>
        /// Loads the items in the Tree View
        /// </summary>
        private void BuildComboBox()
        {
            TreeView TV = this.tvCodeBlocks;
            TreeNode CurrentNode, ParentNode;
            //ICommand CheckCodeCommand = null;
            List<string> sortedFieldNames = new List<string>();
            string trail = "     ";
            TV.Nodes.Clear();

            CurrentNode = new TreeNode("DefineVariables" + trail);
            CurrentNode.Tag = "level=definevariables&event=&identifier=";
            CurrentNode.ImageIndex = 2;
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            TV.Nodes.Add(CurrentNode);


            CurrentNode = new TreeNode("Form : " + view.Name + trail);
            CurrentNode.Tag = "level=form&event=&identifier=";
            CurrentNode.ImageIndex = 2;
            ParentNode = CurrentNode;
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            TV.Nodes.Add(CurrentNode);

            CurrentNode = new TreeNode("     before");
            CurrentNode.Tag = "level=form&event=before&identifier=";
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            ParentNode.Nodes.Add(CurrentNode);
            CurrentNode = new TreeNode("     after");
            CurrentNode.Tag = "level=form&event=after&identifier=";
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            ParentNode.Nodes.Add(CurrentNode);


            CurrentNode = new TreeNode("Record" + trail);
            CurrentNode.Tag = "level=record&event=&identifier=";
            CurrentNode.ImageIndex = 2;
            ParentNode = CurrentNode;
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            TV.Nodes.Add(CurrentNode);

            CurrentNode = new TreeNode("     before");
            CurrentNode.Tag = "level=record&event=before&identifier=";
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            ParentNode.Nodes.Add(CurrentNode);
            CurrentNode = new TreeNode("     after");
            CurrentNode.Tag = "level=record&event=after&identifier=";
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            ParentNode.Nodes.Add(CurrentNode);

            CurrentNode = new TreeNode("Subroutine(s)" + trail);
            CurrentNode.Tag = "level=sub&event=&identifier=";
            CurrentNode.ImageIndex = 2;
            ParentNode = CurrentNode;
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            TV.Nodes.Add(CurrentNode);

            CurrentNode = new TreeNode("     add new" + trail);
            CurrentNode.Tag = "level=sub&event=new&identifier=";
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            ParentNode.Nodes.Add(CurrentNode);

            this.AddSubroutinesToTreeView(ParentNode);

            foreach (Page page in view.Pages)
            {
                string PageName = null;

                if (page.Name.IndexOf(' ') > -1)
                {
                    PageName = "[" + page.Name + "]";
                }
                else
                {
                    PageName = page.Name;
                }

                CurrentNode = new TreeNode(PageName + " : Page" + trail);
                CurrentNode.Tag = "level=page&event=&identifier=" + PageName;
                CurrentNode.ImageIndex = 2;
                ParentNode = CurrentNode;
                CurrentNode.NodeFont = GetNodeFont(CurrentNode);
                TV.Nodes.Add(CurrentNode);

                CurrentNode = new TreeNode("     before");
                CurrentNode.Tag = "level=page&event=before&identifier=" + PageName;
                CurrentNode.NodeFont = GetNodeFont(CurrentNode);
                ParentNode.Nodes.Add(CurrentNode);
                CurrentNode = new TreeNode("     after");
                CurrentNode.Tag = "level=page&event=after&identifier=" + PageName;
                CurrentNode.NodeFont = GetNodeFont(CurrentNode);
                ParentNode.Nodes.Add(CurrentNode);

                foreach (Epi.Fields.Field field in page.Fields)
                {
                    sortedFieldNames.Add(field.Name);
                }
                sortedFieldNames.Sort();

                foreach (string name in sortedFieldNames)
                {
                    foreach (Epi.Fields.Field field in page.Fields)
                    {
                        if (field.Name == name)
                        {
                            if (field is IFieldWithCheckCodeAfter || field is IFieldWithCheckCodeBefore || field is IFieldWithCheckCodeClick)
                            {
                                AddEditorComboBoxItem(ParentNode, field);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the font for the node; returns bold font style if block exists.
        /// </summary>
        /// <param name="CurrentNode">The tree node being checked.</param>
        private Font GetNodeFont(TreeNode CurrentNode)
        {
            string[] Parameters = this.ConvertParameters(CurrentNode.Tag.ToString());
            if (this.findBlock(Parameters[0], Parameters[1], Parameters[2]) > -1)
            {
                return new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
            }
            else
            {
                return new Font("Microsoft Sans Serif", 8, FontStyle.Regular);
            }
        }


        /// <summary>
        /// Add Editor Combo Box Item
        /// </summary>
        /// <param name="pParentNode">The parent of the item in the tree.</param>
        /// <param name="field">The field being added.</param>
        private void AddEditorComboBoxItem(TreeNode pParentNode, Epi.Fields.Field field)
        {
            TreeView TV = this.tvCodeBlocks;
            TreeNode CurrentNode, FieldNode;

            string lead = "     ";


            string FieldName = null;

            if (field.Name.IndexOf(' ') > -1)
            {
                FieldName = "[" + field.Name + "]";
            }
            else
            {
                FieldName = field.Name;
            }

            CurrentNode = new TreeNode(lead + FieldName + " : " + field.FieldType + lead);
            CurrentNode.Tag = "level=field&event=&identifier=" + FieldName;
            CurrentNode.ImageIndex = 2;
            FieldNode = CurrentNode;
            CurrentNode.NodeFont = GetNodeFont(CurrentNode);
            pParentNode.Nodes.Add(CurrentNode);

            if (field is IFieldWithCheckCodeBefore || field is IFieldWithCheckCodeAfter || field is IFieldWithCheckCodeClick)
            {
                if (field is IFieldWithCheckCodeBefore)
                {
                    CurrentNode = new TreeNode(lead + "     before");
                    CurrentNode.Tag = "level=field&event=before&identifier=" + FieldName;
                    CurrentNode.NodeFont = GetNodeFont(CurrentNode);
                    FieldNode.Nodes.Add(CurrentNode);
                }

                if (field is IFieldWithCheckCodeAfter)
                {
                    CurrentNode = new TreeNode(lead + "     after");
                    CurrentNode.Tag = "level=field&event=after&identifier=" + FieldName;
                    CurrentNode.NodeFont = GetNodeFont(CurrentNode);
                    FieldNode.Nodes.Add(CurrentNode);
                }

                if (field is IFieldWithCheckCodeClick)
                {
                    CurrentNode = new TreeNode(lead + "     click");
                    CurrentNode.Tag = "level=field&event=click&identifier=" + FieldName;
                    CurrentNode.NodeFont = GetNodeFont(CurrentNode);
                    FieldNode.Nodes.Add(CurrentNode);
                }
            }
        }

        /// <summary>
        /// Handler for adding new Check Code blocks.
        /// </summary>
        private void AddBlockHandler()
        {
            TreeNode Node = this.SelectedDragNode;
            string Identifier = null;
            string[] Parameters = this.ConvertParameters(Node.Tag.ToString());

            if (this.findBlock(Parameters[0], Parameters[1], Parameters[2]) > -1)
            {
                if (string.IsNullOrEmpty(Parameters[2]))
                {
                    AddStatusErrorMessage(string.Format(SharedStrings.BLOCK_ALREADY_EXISTS, Parameters[2], Parameters[1]));
                }
                else
                {
                    AddStatusErrorMessage(string.Format(SharedStrings.BLOCK_ALREADY_EXISTS, Parameters[0], Parameters[1]));
                }

                return;
            }

            string BlockType = Parameters[0].ToLower();
            switch (BlockType)
            {
                case "view":
                case "form":
                case "record":
                case "page":
                case "field":
                case "subroutine":
                // do nothing for these nodes
                //this.AddStatusInformationMessage(string.Format("{0} : {1} - DoubleClick\nyou must select the 'before' or 'after'  block in order to add a new code block.", DateTime.Now, BlockType));
                //break;
                default:
                    if (!string.IsNullOrEmpty(Parameters[2]))
                    {
                        if (Parameters[2].Trim().IndexOf(' ') > -1 && Parameters[2].IndexOf('[') < 0)
                        {
                            Identifier = "[" + Parameters[2].Trim() + "]";
                        }
                        else
                        {
                            Identifier = Parameters[2].Trim();
                        }
                    }
                    else if (Parameters[0].ToLower() == "sub")
                    {
                        Identifier = "NewSubroutine";

                        if (CheckCode.InputBox("New Subroutine", "Enter subroutine name:", ref Identifier) == DialogResult.OK)
                        {
                            if (Identifier.Trim().IndexOf(' ') > -1 && Identifier.IndexOf('[') < 0)
                            {
                                Identifier = "[" + Identifier.Trim() + "]";
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    string[] NameValues = BlockType.Split('&');

                    string BlockText = Parameters[0] + "_" + Parameters[1];
                    switch (BlockText)
                    {
                        case "definevariables_":
                            this.AddBlock("definevariables", "", "");
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("definevariables", "", "");
                            break;
                        case "view_before":
                            this.AddBlock("view", "before", "");
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("view", "before", "");
                            break;
                        case "view_after":
                            this.AddBlock("view", "after", "");
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("view", "after", "");
                            break;
                        case "form_before":
                            this.AddBlock("form", "before", "");
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("form", "before", "");
                            break;
                        case "form_after":
                            this.AddBlock("form", "after", "");
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("form", "after", "");
                            break;
                        case "record_before":
                            this.AddBlock("record", "before", "");
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("record", "before", "");
                            break;
                        case "record_after":
                            this.AddBlock("record", "after", "");
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("record", "after", "");
                            break;
                        case "page_before":
                            this.AddBlock("page", "before", Identifier);
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("page", "before", Identifier);
                            break;
                        case "page_after":
                            this.AddBlock("page", "after", Identifier);
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("page", "after", Identifier);
                            break;
                        case "field_before":
                            this.AddBlock("field", "before", Identifier);
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("field", "before", Identifier);
                            break;
                        case "field_after":
                            this.AddBlock("field", "after", Identifier);
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("field", "after", Identifier);
                            break;
                        case "field_click":
                            this.AddBlock("field", "click", Identifier);
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("field", "click", Identifier);
                            break;
                        case "sub_new":
                        case "sub_":
                            this.AddBlock("sub", "", Identifier);
                            this.codeText.SelectionStart = this.SetBlockCodeInsertionPoint("sub", "", Identifier);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handler for adding Soubroutine node to the Tree View.
        /// </summary>
        /// <param name="pSubroutineNode">Node of the subroutine being added.</param>
        private void AddSubroutinesToTreeView(TreeNode pSubroutineNode)
        {
            System.Text.RegularExpressions.Regex re = null;
            re = new Regex("\\ssub\\s+[\\w_\\[\\]\\.]*\\b|^sub\\s+[\\w_\\[\\]\\.]*\\b", RegexOptions.IgnoreCase);
            Dictionary<string, string> IdList = new Dictionary<string, string>();
            MatchCollection MC = re.Matches(this.codeText.Text.ToLower());


            for (int i = 0; i < MC.Count; i++)
            {

                int index2 = this.codeText.Text.ToLower().IndexOf(' ', MC[i].Index);

                if (index2 > -1)
                {
                    int index3 = this.codeText.Text.ToLower().IndexOf('\n', index2);
                    if (index3 > -1)
                    {
                        string id = this.codeText.Text.Substring(index2, index3 - index2).Trim();
                        if (!IdList.ContainsKey(id.ToUpper()))
                        {
                            IdList.Add(id.ToUpper(), id);
                        }
                    }
                }

            }

            foreach (System.Collections.Generic.KeyValuePair<string, string> Id in IdList)
            {
                TreeNode treeNode = new TreeNode("     " + Id.Value + "    ");
                treeNode.Tag = "level=sub&event=&identifier=" + Id.Value;
                pSubroutineNode.Nodes.Add(treeNode);
            }

        }

        /// <summary>
        /// Sets the insertion point in the Text Area prior to adding a new code blocks.
        /// </summary>
        /// <param name="pLevel">The level of the check code block (eg. View; Page; Record; Field)</param>
        /// <param name="pEvent">The event of the block (eg. before; after)</param>
        /// <param name="pIdentifier">The name of the control (eg. Page name; field name).</param>
        /// <returns></returns>
        public int SetBlockCodeInsertionPoint(string pLevel, string pEvent, string pIdentifier)
        {
            int start = findBlock(pLevel, "", pIdentifier);
            int SearchStart = start;

            if (start == -1)
            {
                start = this.codeText.Text.Length;
                return start;
            }

            switch (pLevel.ToLower())
            {

                case "definevariables":
                    start = this.codeText.Text.ToLower().IndexOf("end-definevariables", start) - 1;
                    break;
                case "sub":
                    start = this.codeText.Text.ToLower().IndexOf("end-sub", start) - 2;
                    break;

                case "view":
                case "form":
                    switch (pEvent.ToLower())
                    {
                        case "":
                            break;
                        case "before":
                            start = this.codeText.Text.ToLower().IndexOf("end-before", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-view", SearchStart) - 2;
                            }
                            break;
                        case "after":
                            start = this.codeText.Text.ToLower().IndexOf("end-after", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-view", SearchStart) - 2;
                            }
                            break;
                    }
                    break;
                case "record":
                    switch (pEvent.ToLower())
                    {
                        case "":
                            break;
                        case "before":
                            start = this.codeText.Text.ToLower().IndexOf("end-before", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-record", SearchStart) - 2;
                            }
                            break;
                        case "after":
                            start = this.codeText.Text.ToLower().IndexOf("end-after", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-record", SearchStart) - 2;
                            }
                            break;
                    }
                    break;


                case "page":
                    switch (pEvent.ToLower())
                    {
                        case "":
                            break;
                        case "before":
                            start = this.codeText.Text.ToLower().IndexOf("end-before", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-page", SearchStart) - 2;
                            }
                            break;
                        case "after":
                            start = this.codeText.Text.ToLower().IndexOf("end-after", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-page", SearchStart) - 2;
                            }
                            break;
                    }
                    break;
                case "field":
                    switch (pEvent.ToLower())
                    {
                        case "before":
                            start = this.codeText.Text.ToLower().IndexOf("end-before", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-field", SearchStart) - 2;
                            }

                            break;
                        case "after":
                            start = this.codeText.Text.ToLower().IndexOf("end-after", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-field", SearchStart) - 2;
                            }
                            break;
                        case "click":
                            start = this.codeText.Text.ToLower().IndexOf("end-click", start) - 2;
                            if (start < 0)
                            {
                                start = this.codeText.Text.ToLower().IndexOf("end-field", SearchStart) - 2;
                            }
                            break;
                    }
                    break;
            }

            if (start < 0)
            {
                start = SearchStart;
            }

            return start;
        }
        #endregion //Private Methods
    }
}