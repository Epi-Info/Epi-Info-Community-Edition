using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi.Data.Services;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Enter;
using Epi.Windows.Enter.Dialogs;
using Epi.Windows.Enter.PresentationLogic;
using Epi.Collections;
//using Epi.Core.Interpreter;
using Epi.EnterCheckCodeEngine;
using System.Threading;
using System.ComponentModel;
using System.Runtime;

namespace Epi.Windows.Enter.PresentationLogic
{
    /// <summary>
    /// Mediator for Make View's GUI
    /// </summary>
    public partial class GuiMediator
    {
        #region Public Functions

        /// <summary>
        /// Utilized to write a bitmap image to a control's window
        /// </summary>
        /// <param name="hdcDest">hDC of object to receive the bitmap</param>
        /// <param name="nxDest">x-coordinate of the destination rectangle</param>
        /// <param name="nyDest">y-coordinate of the destination rectangle</param>
        /// <param name="nwidth">width of the destination rectangle and source bitmap</param>
        /// <param name="nHeight">height of the destination rectangle and source bitmap</param>
        /// <param name="hdcSrc">hDC of source object that contains the bitmap</param>
        /// <param name="nXSrc">x-coordinate source bitmap</param>
        /// <param name="nYSrc">y-coordinate source bitmap</param>
        /// <param name="dwRop">specifies the raster operation to be performed</param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern long BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nwidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        #endregion //Public Functions

        public int NumberOfRecords;
        public int CurrentRecordId;

        #region Private Members

        private static GuiMediator mediator;
        private static Object classLock = typeof(GuiMediator);
        private Epi.Windows.Enter.ViewExplorer viewExplorer;
        private Epi.Windows.Enter.Canvas canvas;
        private Project project;
        private View view;

        private Page currentPage;
        private int currentRecord;
        private EnterMainForm mainForm;
        private LinkedRecordsViewer linkedRecordsViewer;

        private Panel _fieldPanel;
        private Bitmap memoryImage;
        private ArrayList pageImageList;
        private int currentPrintPage;
        private System.Drawing.Printing.PrintDocument printDocument;
        private List<Control> controlsList;
        private Dictionary<Field, Control> fieldControls;
        private bool autoSearchMessageDisplayed;
        private bool dirty;
        private bool isNewRecord;
        private MemoryRegion memoryRegion;
        public EnterCheckCodeEngine.CheckCodeEngine EnterCheckCodeEngine;
        private Configuration config;
        BackgroundWorker worker = null;
        SplashScreenForm sf = null;

        #endregion  //Private Members

        #region Constructors

        /// <summary>
        /// Constructor of GuiMediator
        /// </summary>
        private GuiMediator()
        {
            controlsList = new List<Control>();
            fieldControls = new Dictionary<Field, Control>();
            EnterCheckCodeEngine = new CheckCodeEngine();
            IsDirty = false;
            config = Configuration.GetNewInstance();
        }

        /// <summary>
        /// Gets an instance of the mediator
        /// </summary>
        public static GuiMediator Instance
        {
            get
            {
                lock (classLock)
                {
                    if (mediator == null)
                    {
                        mediator = new GuiMediator();
                    }
                    return mediator;
                }
            }
        }

        #endregion  //Constructors

        #region Public Events Handlers

        /// <summary>
        /// Occurs when there is a request to close all the Enter forms
        /// </summary>
        public event EventHandler CloseFormsRequested;

        /// <summary>
        /// Occurs when there is a request to the input format for masked text boxes in the status bar.
        /// </summary>
        public event EventHandler DisplayFormat;

        public event EventHandler RecordSaved;

        #endregion  //Public Events

        #region Private Event Handlers


        /// <summary>
        /// Handles the Print Page event of the document
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(((Bitmap)pageImageList[currentPrintPage++]), 0, 0);

            if (currentPrintPage < pageImageList.Count)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
                currentPrintPage = 0;
            }
        }

        #endregion  // Private Event Handlers

        #region Public Properties

        /// <summary>
        /// Gets/sets whether the mediator should enforce entering a new record only (thereby disallowing
        /// entering any subsequent records or navigating to other records already saved).
        /// </summary>
        public bool AllowOneRecordOnly { get; set; }

        /// <summary>
        /// Gets or sets the dirty flag (clean = what is in memory matches what is in the database)
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (this.View != null)
                {
                    return this.View.IsDirty;
                }
                else
                {
                    return false;
                }
                // return dirty;
            }
            set
            {
                if (this.View != null)
                {
                    this.View.IsDirty = value;
                    EnableDisableSaveButton(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current view explorer
        /// </summary>
        public Epi.Windows.Enter.ViewExplorer ViewExplorer
        {
            get
            {
                return viewExplorer;
            }
            set
            {
                viewExplorer = value;
            }
        }

        /// <summary>
        /// Gets or sets the current Enter canvas
        /// </summary>
        public Epi.Windows.Enter.Canvas Canvas
        {
            get
            {
                return canvas;
            }
            set
            {
                canvas = value;
            }
        }

        /// <summary>
        /// Gets or sets the current project
        /// </summary>
        public Project Project
        {
            get
            {
                return project;
            }
            set
            {
                project = value;
            }
        }

        /// <summary>
        /// Gets or sets the main form
        /// </summary>
        public EnterMainForm MainForm
        {
            get
            {
                return mainForm;
            }
            set
            {
                mainForm = value;
            }
        }

        /// <summary>
        /// Gets or sets the current view
        /// </summary>
        public View View
        {
            get
            {
                return view;
            }
            set
            {
                view = value;
            }
        }

        public GeoPosition<GeoCoordinate> LastPosition
        {
            get { return _lastPosition; }
            set { _lastPosition = value; }
        }
        private GeoPosition<GeoCoordinate> _lastPosition;

        public bool IsComboTextAssign { get; set; }
        #endregion  //Public Properties

        #region Public Methods

        public void FocusOnCanvasDesigner()
        {
            Canvas.DesignerFocus();
        }

        /// <summary>
        /// Resets the mediator to its original state
        /// </summary>
        public void Reset()
        {
            mainForm.Reset();
            canvas.Reset();
            viewExplorer.Reset();
            this.ResetVariables();
        }

        /// <summary>
        /// Reset the variables used in the mediator
        /// </summary>
        public void ResetVariables()
        {
            this.autoSearchMessageDisplayed = false;
            this.canvas.EnableTabToNextControl = false;
        }

        /// <summary>
        /// Enables/Disables current record
        /// </summary>
        /// <param name="view">The current view</param>
        /// <param name="enable">Boolean to indicate whether to enable current record</param>
        public void EnableDisableCurrentRecord(View view, bool enable)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            #endregion  //Input Validation

            if (!string.IsNullOrEmpty(this.view.UniqueKeyField.CurrentRecordValueString))
            {
                mainForm.EnableDisableCurrentRecord(enable);
            }
            else
            {
                mainForm.DisableCurrentRecord();
            }
        }

        public void EnableDisableSaveButton(bool enable)
        {
            if (mainForm != null)
            { 
                mainForm.EnableDisableSaveButton(enable);
            }
        }

        /// <summary>
        /// Saves the current record
        /// </summary>
        public bool SaveRecord()
        {
            if (this.view != null)
            {
                this.SetFieldData();
                if (View.IsEmptyNewRecord() == false)
                {
                    if (this.CheckViewRequiredFields())
                    {
                        if (this.IsDirty)
                        {
                            this.EnterCheckCodeEngine.SaveRecord();
                        }
                        
                        this.IsDirty = false;
                        if (RecordSaved != null) RecordSaved(this, new EventArgs());
                        return true;
                    }
                    else
                    {                        
                        return false;
                    }
                }
                this.IsDirty = false;
                if (RecordSaved != null) RecordSaved(this, new EventArgs());
                return true;
            }
            return false;
        }
      
        /// <summary>
        /// Loads current view for edit in MakeView
        /// </summary>
        public void LoadMakeView()
        {
            string projectFilePath = currentPage.GetProject().FilePath;
            string viewName = currentPage.GetView().Name;

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string commandText = System.IO.Path.GetDirectoryName(a.Location) + "\\MakeView.exe";

            if (!string.IsNullOrEmpty(commandText))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = commandText;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Arguments = string.Format("/project:\"{0}\" /view:\"{1}\"", projectFilePath, viewName);

                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new DoWorkEventHandler(worker_DoWork);             
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                worker.RunWorkerAsync();

                proc.Start();
              
            }

            this.CloseFormEventHandler(this, new EventArgs());
        }
      
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (sf == null)
            {
                sf = new SplashScreenForm();
                sf.ShowDialog();
            }
            if (worker.CancellationPending == true)
                e.Cancel = true;           
        }
        private void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (sf != null)
            {
                sf.Close();
                sf.Dispose();
                worker.DoWork -= new DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            }
        }

        public void Print(int recordStart, int recordEnd, int pageStart, int pageEnd)
        {
            printDocument = null;
            if (ProcessPrintRequest(recordStart, recordEnd, pageStart, pageEnd))
            {
                printDocument.Print();
            }
            else
            {
                MessageBox.Show("The selected range and number of pages is too much to print at one time.");
            }

            this.printDocument.Dispose();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        public void PrintPreview(int recordStart, int recordEnd, int pageNumberStart, int pageNumberEnd)
        {
            DataRow row = currentPage.GetMetadata().GetPageSetupData(view);
            bool isLandscape = row["Orientation"].ToString().ToLowerInvariant() == "landscape";

            if (ProcessPrintRequest(recordStart, recordEnd, pageNumberStart, pageNumberEnd))
            {
                Epi.Windows.Enter.Dialogs.EnterPrintPreviewDialog printPreview = new Epi.Windows.Enter.Dialogs.EnterPrintPreviewDialog();
                printPreview.Document = printDocument;
                printPreview.ShowIcon = false;
                printDocument.DefaultPageSettings.Landscape = isLandscape;
                printPreview.ShowDialog();
            }
            else
            {
                MessageBox.Show("The selected range and number of pages is too much to print at one time.");
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
        }

        private bool ProcessPrintRequest(int recordStart, int recordEnd, int pageNumberStart, int pageNumberEnd)
        {
            pageImageList = new ArrayList();
            currentPrintPage = 0;
            bool continueProcess = true;

            if (recordStart == -1)
            {
                ProcessPrintRequest(pageNumberStart, pageNumberEnd, false);
            }
            else if (recordStart == recordEnd)
            {
                mediator.view.LoadRecord(recordStart);
                continueProcess = ProcessPrintRequest(pageNumberStart, pageNumberEnd, true);
            }
            else
            {
                for (int i = recordStart; i <= recordEnd; i++)
                {
                    if (i == 1)
                    {
                        mediator.view.LoadFirstRecord();

                        if (i >= recordStart)
                        {
                            continueProcess = ProcessPrintRequest(pageNumberStart, pageNumberEnd, true);

                            if (continueProcess == false)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if(i == recordStart)
                        {
                            mediator.view.LoadRecord(recordStart);
                        }

                        mediator.view.LoadNextRecord(mediator.view.CurrentRecordId);

                        continueProcess = ProcessPrintRequest(pageNumberStart, pageNumberEnd, true);

                        if (continueProcess == false)
                        {
                            break;
                        }
                    }
                }
            }
            
            return continueProcess;
        }

        private bool ProcessPrintRequest(int pageNumberStart, int pageNumberEnd, bool showData)
        {
            bool isLandscape = false;
            bool exitPrint = false;

            try
            {
                List<Page> allPages = view.GetMetadata().GetViewPages(view);
                List<Page> pages = new List<Page>();
                pages = allPages.GetRange(pageNumberStart - 1, 1 + pageNumberEnd - pageNumberStart);

                foreach (Page page in pages)
                {
                    printDocument = new System.Drawing.Printing.PrintDocument();
                    printDocument.PrintPage += new PrintPageEventHandler(printDocument_PrintPage);
                    DataRow row = page.GetMetadata().GetPageSetupData(view);
                    isLandscape = row["Orientation"].ToString().ToLowerInvariant() == "landscape";
                    Panel printPanel = new Panel();

                    float dpiX;
                    Graphics graphics = printPanel.CreateGraphics();
                    dpiX = graphics.DpiX;

                    int height = (int)row["Height"];
                    int width = (int)row["Width"];

                    if (dpiX != 96)
                    {
                        float scaleFactor = (dpiX * 1.041666666f) / 100;
                        height = Convert.ToInt32(((float)height) * (float)scaleFactor);
                        width = Convert.ToInt32(((float)width) * (float)scaleFactor);
                    }
                    
                    if (isLandscape)
                    {
                        printPanel.Size = new System.Drawing.Size(height, width);
                        printDocument.DefaultPageSettings.Landscape = true;
                    }
                    else
                    {
                        printPanel.Size = new System.Drawing.Size(width, height);
                    }

                    ControlFactory factory = ControlFactory.Instance;
                    List<Control> pageControls = factory.GetPageControls(page, printPanel.Size);

                    SortedList<FieldGroupBox, System.Drawing.Point> groupsOnPage = new SortedList<FieldGroupBox, System.Drawing.Point>(new GroupZeeOrderComparer());
                    List<Control> childrenOnPage = new List<Control>();
                    List<Control> orphansOnPage = new List<Control>();
                    ArrayList namesOfChildenOnPage = new ArrayList();

                    Panel panel = _fieldPanel;

                    foreach (Control control in pageControls)
                    {
                        Control printControl = control;
                        Field field = factory.GetAssociatedField(control);
                        field = this.view.Fields[field.Name];

                        if (control is DragableGroupBox == false)
                        {
                            try
                            {
                                if (control is RichTextBox)
                                {
                                    printControl = new TextBox();

                                    ((TextBoxBase)printControl).BorderStyle = ((TextBoxBase)control).BorderStyle;
                                    ((TextBoxBase)printControl).Multiline = true;

                                    printControl.Left = control.Left;
                                    printControl.Top = control.Top;
                                    printControl.Height = control.Height;
                                    printControl.Width = control.Width;
                                    printControl.Font = control.Font;
                                }

                                var memFailPoint = new MemoryFailPoint(8);
                                printPanel.Controls.Add(printControl);
                            }
                            catch(System.InsufficientMemoryException)
                            {
                                printPanel.Dispose();
                                return false;
                            }
                            catch (System.OutOfMemoryException)
                            {
                                printPanel.Dispose();
                                return false;
                            }
                            catch
                            {
                                printPanel.Dispose();
                                return false;
                            }
                        }

                        if (printControl is Label) continue;
                        if (printControl is FieldGroupBox) continue;

                        if (showData)
                        {
                            if (field is ImageField)
                            {
                                this.canvas.GetFieldDataIntoControl(field as ImageField, printControl as PictureBox);
                            }
                            else if (field is YesNoField)
                            {
                                this.canvas.GetFieldDataIntoControl(field as YesNoField, printControl as ComboBox);
                                if (((ComboBox)printControl).SelectedValue == null)
                                {
                                    foreach (Control panelControl in panel.Controls)
                                    {
                                        if (panelControl is Label) continue;
                                        if (panelControl is FieldGroupBox) continue;

                                        Field panelfield = factory.GetAssociatedField(panelControl);
                                        panelfield = this.view.Fields[field.Name];

                                        if (panelfield.Name == field.Name)
                                        {
                                            string holdit = ((YesNoField)panelfield).CurrentRecordValueString.ToString();
                                            string yes = "1";
                                            string no = "0";

                                            if (holdit.Contains(yes))
                                            {
                                                printControl.Text = "Yes";
                                            }
                                            else if (holdit.Contains(no))
                                            {
                                                printControl.Text = "No";
                                            }
                                        }
                                    }
                                }
                            }
                            else if (printControl is TextBox || printControl is ComboBox || printControl is CheckBox || printControl is MaskedTextBox || printControl is DataGridView || printControl is GroupBox || printControl is DateTimePicker)
                            {
                                if (field is MirrorField)
                                {
                                    this.canvas.GetMirrorData(field as MirrorField, printControl);
                                }
                                else if (field is IDataField || printControl is DataGridView)
                                {
                                    if (printControl is TextBox)
                                    {
                                        this.canvas.GetTextData(field, printControl);
                                    }
                                    else if (printControl is RichTextBox)
                                    {
                                        this.canvas.GetTextData(field, printControl);
                                    }
                                    else if (field is DateField)
                                    {
                                        this.canvas.GetDateData(field as DateField, printControl);
                                    }
                                    else if (field is TimeField)
                                    {
                                        this.canvas.GetTimeData(field as TimeField, printControl);
                                    }
                                    else if (field is DateTimeField)
                                    {
                                        this.canvas.GetDateTimeData(field as DateTimeField, printControl);
                                    }
                                    else if (printControl is MaskedTextBox)
                                    {
                                        if (((IDataField)field).CurrentRecordValueObject != null)
                                        {
                                            if (((MaskedTextBox)printControl).Mask != null)
                                            {
                                                if (field is NumberField && !Util.IsEmpty(((IDataField)field).CurrentRecordValueString))
                                                {
                                                    printControl.Text = FormatNumberInput(((NumberField)field).CurrentRecordValueString, ((NumberField)field).Pattern);
                                                }
                                                else
                                                {
                                                    printControl.Text = ((IDataField)field).CurrentRecordValueString;
                                                }
                                            }
                                            else
                                            {
                                                printControl.Text = ((IDataField)field).CurrentRecordValueString;
                                            }
                                        }
                                        else
                                        {
                                            printControl.Text = string.Empty;
                                        }
                                    }
                                    else if (printControl is ComboBox)
                                    {
                                        this.canvas.GetComboboxData(field, printControl);
                                    }
                                    else if (printControl is CheckBox)
                                    {
                                        this.canvas.GetCheckBoxData(field, printControl);
                                    }
                                    else if (printControl is DataGridView)
                                    {
                                        this.canvas.GetDataGridViewData(field, printControl);
                                    }
                                    else if (field is OptionField)
                                    {
                                        this.canvas.GetOptionData((OptionField)field, printControl);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (printControl is TextBox || printControl is ComboBox)
                            {
                                if (field is IDataField || printControl is DataGridView)
                                {
                                    if (printControl is ComboBox)
                                    {
                                        ((ComboBox)printControl).Text = "";
                                    }
                                }

                                if (printControl is TextBox)
                                {
                                    ((TextBox)printControl).Text = "";
                                    ((TextBox)printControl).Enabled = true;
                                }
                            }
                        }
                    }

                    if (exitPrint)
                    {
                        break;
                    }

                    int colorValue;
                    DataTable backgroundTable = page.GetMetadata().GetPageBackgroundData(page);
                    DataRow[] rows = backgroundTable.Select("BackgroundId=" + page.BackgroundId);
                    string imageLayout = string.Empty;
                    if (rows.Length > 0)
                    {
                        imageLayout = Convert.ToString(rows[0]["ImageLayout"]);
                    }
                    Color color;
                    Image image;
                    Bitmap bufferBitmap = null;

                    if (page != null)
                    {
                        if (rows.Length > 0)
                        {
                            colorValue = Convert.ToInt32(rows[0]["Color"]);

                            if (rows[0]["Image"] != System.DBNull.Value)
                            {
                                try
                                {
                                    byte[] imageBytes = ((byte[])rows[0]["Image"]);

                                    using (MemoryStream memStream = new MemoryStream(imageBytes.Length))
                                    {
                                        memStream.Seek(0, SeekOrigin.Begin);
                                        memStream.Write(imageBytes, 0, imageBytes.Length);
                                        memStream.Seek(0, SeekOrigin.Begin);

                                        image = Image.FromStream(((Stream)memStream));
                                    }
                                }
                                catch
                                {
                                    image = null;
                                }
                            }
                            else
                            {
                                image = null;
                            }

                            if (colorValue == 0)
                            {
                                color = SystemColors.Window;
                            }
                            else
                            {
                                color = Color.FromArgb(colorValue);
                            }
                        }
                        else
                        {
                            image = null;
                            imageLayout = "None";
                            color = SystemColors.Window;
                        }
                    }
                    else
                    {
                        image = null;
                        imageLayout = "None";
                        color = SystemColors.Window;
                    }

                    if ((image == null) && (color.Equals(Color.Empty)))
                    {
                        printPanel.BackColor = Color.White;
                    }
                    else
                    {
                      /* if (showtab)
                        {
                            foreach (Control control in pageControls)
                            {
                                Field field = factory.GetAssociatedField(control);                              
                                if (control.TabStop == true)
                                {
                                    bool isInputField = ((Control)control) is PairedLabel == false && field.FieldType != MetaFieldType.Group;
                                    bool isLabelField = field.FieldType == MetaFieldType.LabelTitle;
                                    if (isInputField || isLabelField)
                                    {
                                        Label lbTabSquare = new Label();
                                        lbTabSquare.BackColor = control.TabStop ? Color.Black : Color.Firebrick;
                                        lbTabSquare.Padding = new Padding(2);
                                        lbTabSquare.ForeColor = Color.White;
                                        lbTabSquare.BorderStyle = BorderStyle.None;
                                        lbTabSquare.Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
                                        lbTabSquare.Text = control.TabIndex.ToString() + "  " + field.Name;
                                        lbTabSquare.Location = new Point(control.Location.X, control.Location.Y);
                                        lbTabSquare.Size = TextRenderer.MeasureText(lbTabSquare.Text, lbTabSquare.Font);
                                        lbTabSquare.Size = new Size(lbTabSquare.Size.Width + lbTabSquare.Padding.Size.Width, lbTabSquare.Size.Height + lbTabSquare.Padding.Size.Height);
                                        lbTabSquare.Tag = "showtab";
                                        lbTabSquare.BringToFront();
                                        printPanel.Controls.Add(lbTabSquare);
                                    }
                                }
                            }
                        }*/
                        printPanel.BackgroundImageLayout = ImageLayout.None;
                        if (printPanel.Size.Width > 0 && printPanel.Size.Height > 0)
                        {
                            try
                            {
                                Bitmap b = new Bitmap(printPanel.Size.Width, printPanel.Size.Height);
                                Graphics bufferGraphics = Graphics.FromImage(b);

                                if (!(color.Equals(Color.Empty)))
                                {
                                    printPanel.BackColor = color;
                                }

                                bufferGraphics.Clear(printPanel.BackColor);

                                if (image != null)
                                {
                                    Image img = image;
                                    switch (imageLayout.ToUpperInvariant())
                                    {
                                        case "TILE":
                                            TextureBrush tileBrush = new TextureBrush(img, System.Drawing.Drawing2D.WrapMode.Tile);
                                            bufferGraphics.FillRectangle(tileBrush, 0, 0, printPanel.Size.Width, printPanel.Size.Height);
                                            tileBrush.Dispose();
                                            break;

                                        case "STRETCH":
                                            bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                            bufferGraphics.DrawImage(img, 0, 0, printPanel.Size.Width, printPanel.Size.Height);
                                            bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                                            break;

                                        case "CENTER":
                                            int centerX = (printPanel.Size.Width / 2) - (img.Size.Width / 2);
                                            int centerY = (printPanel.Size.Height / 2) - (img.Size.Height / 2);
                                            bufferGraphics.DrawImage(img, centerX, centerY);
                                            break;

                                        default:
                                            bufferGraphics.DrawImage(img, 0, 0);
                                            break;
                                    }
                                }

                                foreach (Control control in pageControls)
                                {
                                    if (control is DragableGroupBox)
                                    {
                                        Pen pen = new Pen(Color.Black);
                                        Point ul = control.Location;
                                        Point ur = new Point(control.Location.X + control.Width, control.Location.Y);
                                        Point ll = new Point(control.Location.X, control.Location.Y + control.Height);
                                        Point lr = new Point(control.Location.X + control.Width, control.Location.Y + control.Height);
                                        bufferGraphics.DrawLine(pen, ul, ur);
                                        bufferGraphics.DrawLine(pen, ur, lr);
                                        bufferGraphics.DrawLine(pen, lr, ll);
                                        bufferGraphics.DrawLine(pen, ll, ul);
                                    }
                                }

                                bufferGraphics.DrawImage(b, 0, 0);
                                bufferBitmap = b;
                                printPanel.BackgroundImage = b;
                            }
                            catch
                            {
                                bufferBitmap.Dispose();
                                graphics.Dispose();
                                printPanel.Dispose();
                                pageImageList = null;
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                return false;
                            }
                        }
                    }

                    foreach (Control control in pageControls)
                    {
                        if (control is DragableGroupBox)
                        {
                            Label label = new Label();
                            label.Width = control.Width;
                            label.Text = control.Text;
                            Size textSize = TextRenderer.MeasureText(graphics, label.Text, label.Font);
                            label.Left = control.Left + 12;
                            label.Top = control.Top - textSize.Height / 2;
                            label.Width = textSize.Width + 12;
                            label.Font = control.Font;
                            label.AutoSize = true;
                            printPanel.Controls.Add(label);
                        }
                    }

                    if (bufferBitmap != null)
                    {
                        graphics.DrawImage(bufferBitmap, 0, 0);
                    }

                    try
                    {
                        memoryImage = new Bitmap(printPanel.Width, printPanel.Height, graphics);
                        printPanel.DrawToBitmap(memoryImage, new Rectangle(0, 0, printPanel.Width, printPanel.Height));
                        printPanel.Dispose();
                        pageImageList.Add(memoryImage.Clone());
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                    catch
                    {
                        bufferBitmap.Dispose();
                        graphics.Dispose();
                        printPanel.Dispose();                      
                        pageImageList = null;
                        memoryImage.Dispose();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        return false;
                    }
                }
            }
            catch
            {
                memoryImage.Dispose();
                pageImageList = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return false;
            };

            return true;
        }

        /// <summary>
        /// Calls the method to set the focus to the first control on a view's page
        /// </summary>
        public void SetFocusToFirstControl(Page currentPage, Panel currentPanel)
        {
            canvas.SetFocusToFirstControl(currentPage, currentPanel);
        }

        /// <summary>
        /// Navigates to the next page of the view
        /// </summary>
        public void GoToNextPage()
        {
            viewExplorer.GoToNextPage();
        }

        /// <summary>
        /// Navigates to the previous page of the view
        /// </summary>
        public void GoToPreviousPage()
        {
            viewExplorer.GoToPreviousPage();
        }

        /// <summary>
        /// Navigates to a specific page of the view
        /// </summary>
        /// <param name="pagePosition">The page position</param>
        public void GoToSpecificPage(String pagePosition)
        {
            viewExplorer.GoToSpecificPage(pagePosition);
        }

        /// <summary>
        /// Formats a number string to the proper mask format.
        /// </summary>
        /// <param name="numberInput">Number to format</param>
        /// <param name="mask">Number format mask</param>
        /// <returns></returns>
        public string FormatNumberInput(string numberInput, string mask)
        {
            if (string.IsNullOrEmpty(mask))
            {
                return numberInput;
            }
            else
            {
                if (mask.Contains("."))
                {
                    mask = "{0:" + mask.Replace("#", "0") + "}";
                    Match spaceMatch = Regex.Match(numberInput, @"\.*[\s][0-9]");
                    if (spaceMatch.Success)
                    {
                        numberInput = Regex.Replace(numberInput, @"[\s]", "0");
                    }
                    else
                    {
                        numberInput = Regex.Replace(numberInput, @"[\s]", "");
                    }
                    if (numberInput.Contains("-"))
                    {
                        mask = mask.Remove(3, 1);
                    }
                    numberInput = String.Format(mask, double.Parse(numberInput));
                }
                else
                {
                    double parsedValue = Math.Round(double.Parse(numberInput), 0);
                    string format = mask.Replace("#", "0");
                    numberInput = parsedValue.ToString(format);
                }
                return numberInput;
            }
        }

        #endregion  //Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the current panel
        /// </summary>
        private void LoadPanel(Page page)
        {
                       
            DataRow row = page.GetMetadata().GetPageSetupData(page.GetView());
            if (_fieldPanel != null)
            {
                foreach (Control control in _fieldPanel.Controls)
                {
                    control.Font = null;       //GDI Memory leak           
                }
            }
            _fieldPanel = new Panel();

            float dpiX;
            Graphics graphics = _fieldPanel.CreateGraphics();
            dpiX = graphics.DpiX;
             try
            {
            int height = (int)row["Height"];
            int width = (int)row["Width"];

            if (dpiX != 96)
            {
                float scaleFactor = (dpiX * 1.041666666f) / 100;
                height = Convert.ToInt32(((float)height) * (float)scaleFactor);
                width = Convert.ToInt32(((float)width) * (float)scaleFactor);
            }

            if (row["Orientation"].ToString() == "Landscape")
            {
                _fieldPanel.Size = new System.Drawing.Size(height, width);
            }
            else
            {
                _fieldPanel.Size = new System.Drawing.Size(width, height);
            }

            canvas.Size = _fieldPanel.Size;
            canvas.SetPanelProperties(_fieldPanel);

            currentPage = page;

            ControlFactory factory = ControlFactory.Instance;
            canvas.canvasPanel.Size = new Size(_fieldPanel.Size.Width, _fieldPanel.Size.Height);

            List<Control> controls = factory.GetPageControls(page, canvas.canvasPanel.Size);
            canvas.AddControlsToPanel(controls, _fieldPanel);

            SetZeeOrderOfGroups(_fieldPanel);

            _fieldPanel.Visible = false;
            _fieldPanel.SendToBack();

            foreach (Control controlOnPanel in _fieldPanel.Controls)
            {
                if (controlOnPanel is DataGridView)
                {
                    ((DataGridView)controlOnPanel).DataSource = null;
                }

            }
            while (canvas.canvasPanel.Controls.Count > 0)
                canvas.canvasPanel.Controls[0].Dispose();//User Handles Memory leak
            // canvas.canvasPanel.Controls.Clear();
            canvas.canvasPanel.Controls.Add(_fieldPanel);
            }
             finally
             {
             }
        }

        private void DisposePanel()
        {
            if (_fieldPanel != null)
            {
                _fieldPanel.Hide();

                foreach (Control controlOnPanel in _fieldPanel.Controls)
                {
                    if (controlOnPanel is DataGridView)
                    {
                        ((DataGridView)controlOnPanel).DataSource = null;
                    }

                    controlOnPanel.Dispose();
                }

                _fieldPanel.Dispose();
            }

            this.canvas.ResetControlFactoryFields();
            canvas.canvasPanel.Controls.Clear();
            GC.Collect();
        }

        /// <summary>
        /// Selects the image
        /// </summary>
        /// <param name="imageField">Image to select</param>
        /// <param name="pictureBox">Picture box</param>
        private void SelectImage(ImageField imageField, PictureBox pictureBox)
        {
            if (imageField.CurrentRecordValueObject != null)
            {
                DialogResult clear = MsgBox.ShowQuestion("Would you like to clear the current image?", MessageBoxButtons.YesNoCancel);

                if (clear == DialogResult.Yes)
                {
                    imageField.CurrentRecordValueObject = null;
                    GetFieldDataIntoControl(imageField, pictureBox);
                    return;
                }
            }
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.bmp;*.jpg;*.gif)|*.bmp;*.jpg;*.gif | *.bmp | *.bmp | *.jpg | *.jpg | *.gif | *.gif"; // TODO: hard coded string
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            DialogResult result = openFileDialog.ShowDialog();
            
            if (result == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName.Trim();
                byte[] imageAsBytes = Util.GetByteArrayFromImagePath(filePath);
                imageField.CurrentRecordValue = imageAsBytes;
                this.IsDirty = true;

                GetFieldDataIntoControl(imageField, pictureBox);
            }
        }

        /// <summary>
        /// Copy Data Row
        /// </summary>
        /// <param name="sourceRow">Source data row</param>
        /// <param name="targetRow">Target data row</param>
        private void CopyDataRow(DataRow sourceRow, DataRow targetRow)
        {
            int iIndex = 0;

            foreach (object item in sourceRow.ItemArray)
            {
                targetRow[iIndex] = item;
                iIndex++;
            }
        }

        /// <summary>
        /// Get the image Field Data into the control
        /// </summary>
        /// <param name="field">Image field</param>
        /// <param name="control">Picture Box</param>
        private void GetFieldDataIntoControl(ImageField field, PictureBox control)
        {
            if (field.CurrentRecordValueObject is byte[])
            {
                MemoryStream memStream = new MemoryStream();
                byte[] imageAsBytes = (byte[])field.CurrentRecordValueObject;
                memStream.Write(imageAsBytes, 0, imageAsBytes.Length);
                ((PictureBox)control).Image = Image.FromStream(memStream);
            }
            else
            {
                ((PictureBox)control).Image = null;
            }
        }

        /// <summary>
        /// Get the YesNo Field Data into the control
        /// </summary>
        /// <param name="field">YesNo Field</param>
        /// <param name="control">ComboBox</param>
        private void GetFieldDataIntoControl(YesNoField field, ComboBox control)
        {
            //if (field.CurrentRecordValueObject == null || (control.Items.Contains(field.CurrentRecordValueObject) == false))
            if (field.CurrentRecordValueObject == null)
            {
                control.SelectedIndex = -1;
            }
            else
            {
                control.SelectedValue = field.CurrentRecordValueObject;
            }
        }

        /// <summary>
        /// Gets the field data value
        /// </summary>        
        //private void GetFieldData(Control control)
        public void ShowFieldDataOnForm()
        {
            ControlFactory factory = ControlFactory.Instance;
            foreach (Panel panel in canvas.Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    // Skip prompts and field group boxes. They don't have any data.
                    if (control is Label) continue;
                    if (control is FieldGroupBox) continue;

                    Field field = factory.GetAssociatedField(control);

                    if (field is ImageField)
                    {
                        GetFieldDataIntoControl(field as ImageField, control as PictureBox);
                    }
                    else if (field is YesNoField)
                    {
                        GetFieldDataIntoControl(field as YesNoField, control as ComboBox);
                    }
                    else if (control is TextBox || control is RichTextBox || control is ComboBox || control is CheckBox || control is MaskedTextBox || control is DataGridView || control is GroupBox)
                    {
                        if (field is MirrorField)
                        {
                            GetMirrorData(field as MirrorField, control);
                        }
                        else if (field is IDataField || control is DataGridView)
                        {
                            if (control is TextBox || control is RichTextBox)
                            {
                                GetTextData(field, control);
                            }
                            else if (control is MaskedTextBox)
                            {
                                if (((IDataField)field).CurrentRecordValueObject != null)
                                {
                                    if (((MaskedTextBox)control).Mask != null)
                                    {
                                        if (field is DateField)
                                        {
                                            GetDateData(field as DateField, control);
                                        }
                                        else if (field is TimeField)
                                        {
                                            GetTimeData(field as TimeField, control);
                                        }
                                        else if (field is DateTimeField)
                                        {
                                            GetDateTimeData(field as DateTimeField, control);
                                        }
                                        else if (field is NumberField && !Util.IsEmpty(((IDataField)field).CurrentRecordValueString))
                                        {
                                            control.Text = FormatNumberInput(((NumberField)field).CurrentRecordValueString, ((NumberField)field).Pattern);
                                        }
                                        else
                                        {
                                            control.Text = ((IDataField)field).CurrentRecordValueString;
                                        }
                                    }
                                    else
                                    {
                                        control.Text = ((IDataField)field).CurrentRecordValueString;
                                    }
                                }
                                else
                                {
                                    control.Text = string.Empty;
                                }
                            }
                            else if (control is ComboBox)
                            {
                                GetComboboxData(field, control);
                            }
                            else if (control is CheckBox)
                            {
                                GetCheckBoxData(field, control);
                            }
                            else if (control is DataGridView)
                            {
                                GetDataGridViewData(field, control);
                            }
                            else if (field is OptionField)
                            {
                                GetOptionData((OptionField)field, control);
                            }
                        }
                    }
                }
            }
        }

        private void DataBind(ComboBox comboBox, DataTable sourceData, string displayMember, string valueMember)
        {
            comboBox.DisplayMember = displayMember;
            comboBox.ValueMember = valueMember;
            comboBox.DataSource = sourceData;
            if (sourceData.Rows.Count > 0)
            {
                comboBox.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <param name="control">The control to be validated</param>
        /// <returns>Returns true or false is the input is valid</returns>
        public bool IsValidData(Control control)
        {
            #region Input Validation
            if (control == null)
            {
                throw new ArgumentNullException("Control");
            }
            #endregion  //Input Validation

            // Prevents the 'Invalid data' warning from showing up if the user selects 'No' to saving the record
            if (mainForm == null || mainForm.FormClosed)
            {
                return true;
            }

            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField(control);
            bool isValid = true;            

            if (field is NumberField)
            {
                ((MaskedTextBox)control).TextMaskFormat = MaskFormat.IncludeLiterals;

                string decimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                if (!String.IsNullOrEmpty(control.Text) && !control.Text.Trim().Equals(decimalSeparator))
                {
                    if (field is IPatternable)
                    { 
                        string pattern = ((IPatternable)field).Pattern;
                        if (control.Text.Length > 39 && pattern == "None")
                        {
                            MsgBox.ShowWarning(SharedStrings.MAX_NONE_MASK_DIGITS_EXCEEDED);
                            control.Select();
                            isValid = false;
                        }
                        else
                        { 
                            try
                            {
                                // Test what is in the box. If we catch a format exception then the
                                // user entered something bad, like -- or just a decimal point. They
                                // may also have entered some text if the field doesn't have a pattern.                               // doesn't have a pattern.
                                string TestNumber = control.Text.Replace(" ", "");

                                //!float.TryParse(TestNumber, out temp)

                                float temp;
                                if (float.TryParse(TestNumber, out temp))
                                {
                                    temp = float.Parse(TestNumber, System.Globalization.NumberStyles.Any);
                                    ((MaskedTextBox)control).Text = FormatNumberInput(TestNumber, ((MaskedTextBox)control).Mask);
                                    ((MaskedTextBox)control).TextMaskFormat = MaskFormat.IncludeLiterals;
                                    string format = AppData.Instance.DataPatternsDataTable.GetExpressionByMask(((MaskedTextBox)(control)).Mask.ToString(), ((IPatternable)field).Pattern);
                                    bool result;
                                    double doubleNumber;
                                    result = Double.TryParse(((MaskedTextBox)control).Text, out doubleNumber);
                                    if (result)
                                    {
                                        NumberField checkedField = (NumberField)field;
                                        if (!string.IsNullOrEmpty(checkedField.Lower) && !string.IsNullOrEmpty(checkedField.Upper))
                                        {
                                            if ((double.Parse(checkedField.Lower) > doubleNumber) || (doubleNumber > double.Parse(checkedField.Upper)))
                                            {
                                                //FIX for DEFECT 1080
                                                MsgBox.ShowWarning(String.Format(SharedStrings.VALUE_NOT_IN_RANGE, checkedField.Lower, checkedField.Upper));
                                                control.Select();
                                                isValid = false;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    MsgBox.ShowInformation(SharedStrings.ENTER_VALID_NUMBER);
                                    control.Select();
                                    isValid = false;
                                }
                            }
                            catch (FormatException)
                            {
                                MsgBox.ShowWarning(SharedStrings.ENTER_VALID_NUMBER);
                                control.Text = string.Empty;
                                control.Select();
                                isValid = false;
                            }
                        }
                    }
                }
            }
            else if (field is DateField)
            {
                DateTime dateTime;
                bool canParse = DateTime.TryParse(control.Text, out dateTime);

                if (string.IsNullOrEmpty(control.Text)) return true;

                DateTimeFormatInfo formatInfo = DateTimeFormatInfo.CurrentInfo;
                string pattern = formatInfo.ShortDatePattern.ToUpperInvariant();

                if (control.Text.Equals(pattern)) return true;

                if (canParse == false)
                {
                    ShowValidationError(control);
                    control.Select();
                    return false;
                }

                if (dateTime < new DateTime(1800, 1, 1))
                {
                    ShowValidationError(control);
                  control.Select();
                    return false;
                }

                if (((DateField)field).IsInRange(dateTime) == false)
                {
                    control.Select();
                    isValid = false;
                }
            }
            else
            {
                //Ensure text entered in combobox text exists within the combobox's items
                if (control is ComboBox)
                {
                    if (!string.IsNullOrEmpty(control.Text.Trim()))
                    {
                        int index = ((ComboBox)control).FindStringExact(control.Text.Trim(), -1);
                        bool bdoit = false;
                        if (index == -1)
                        {
                            if (field is YesNoField)
                            {
                                
                                MsgBox.ShowInformation(String.Format(SharedStrings.INVALID_YES_NO_VALUE, config.Settings.RepresentationOfYes, config.Settings.RepresentationOfNo));
                                //EpiMessages msgs;
                                //msgs.Caption = String.Format(SharedStrings.INVALID_YES_NO_VALUE_CAPTION, config.Settings.RepresentationOfYes, config.Settings.RepresentationOfNo);
                                //msgs.Message = String.Format(SharedStrings.INVALID_YES_NO_VALUE, config.Settings.RepresentationOfYes, config.Settings.RepresentationOfNo);
                                //bdoit = this.MainForm.Module.Processor.onCommunicateUI(msgs, MessageType.OkOnly);

                                //if (bdoit)
                                {
                                    ((ComboBox)control).SelectedIndex = -1;
                                    control.Text = String.Empty;
                                    control.Focus();
                                    isValid = false;
                                }
                            }
                            else
                            {
                                if (((ComboBox)control).FindString(control.Text) != -1)
                                {
                                    ((ComboBox)control).SelectedIndex = ((ComboBox)control).FindString(((ComboBox)control).Text);
                                    isValid = true;
                                }
                                else
                                {
                                    MsgBox.ShowInformation(SharedStrings.INVALID_DROPDOWN_VALUE);
                                    //EpiMessages msgs;
                                    //msgs.Caption = SharedStrings.INVALID_DROPDOWN_VALUE_CAPTION;
                                    //msgs.Message = SharedStrings.INVALID_DROPDOWN_VALUE;
                                    //bdoit = this.MainForm.Module.Processor.onCommunicateUI(msgs, MessageType.OkOnly);

                                    //if (bdoit)
                                    {
                                        if (!String.IsNullOrEmpty(((IDataField)field).CurrentRecordValueString))
                                        {
                                            ((ComboBox)control).SelectedIndex = ((ComboBox)control).FindString(((IDataField)field).CurrentRecordValueString);
                                        }
                                        else
                                        {
                                            ((ComboBox)control).SelectedIndex = -1;
                                            control.Text = String.Empty;
                                        }
                                        control.Focus();
                                        isValid = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (mainForm != null)
            {
                ((EnterMainForm)mainForm).ContinueChangeRecord = isValid;
            }
            
            return isValid;
        }

        ///// <summary>
        ///// Displays error message for masked input textboxes, if validation failed
        ///// </summary>
        ///// <param name="control">The control that failed validation</param>
        private void ShowValidationError(Control control)
        {
            #region Input Validation
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            #endregion  //Input Validation

            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField(control);
            if (field is DateTimeField)
            {
                if (field is DateField)
                {
                    MsgBox.ShowWarning(SharedStrings.ENTER_VALID_DATE);
                }
                else if (field is TimeField)
                {
                    MsgBox.ShowWarning(SharedStrings.ENTER_VALID_TIME);
                }
                else
                {
                    MsgBox.ShowWarning(SharedStrings.ENTER_VALID_DATE_AND_TIME);
                }
            }

            control.Text = string.Empty;
            control.Select();
        }

        /// <summary>
        /// Gets associated controls for fields in list
        /// </summary>
        /// <param name="checkCodeList">A list of fields</param>        
        private List<Control> GetAssociatedControls(Collection<string> checkCodeList)
        {
            controlsList.Clear();
            ControlFactory factory = ControlFactory.Instance;

            for (int i = 0; i <= checkCodeList.Count - 1; i++)
            {
                try
                {
                    string[] args = checkCodeList[i].Split(StringLiterals.SPACE.ToCharArray());
                    
                    for (int j = 0; j < args.Length; j++)
                    {
                        //loop through args for one command
                        // For example: HIDE firstName lastName 

                        if (!(String.IsNullOrEmpty(args[j].ToString())))
                        {
                            DataRow dataRow = this.view.GetMetadata().GetFieldIdByNameAsDataRow(this.view.Name, args[j].ToString());
                            Field field = this.view.GetFieldById(int.Parse(dataRow["FieldId"].ToString()));
                            if (view.Fields.Contains(field))
                            {
                                controlsList.AddRange(factory.GetAssociatedControls(field));
                            }
                        }
                    }

                }
                catch (ApplicationException ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }
            return controlsList;
        }

        /// <summary>
        /// Gets associated controls for a particular field
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>A list of associated controls</returns>
        private List<Control> GetAssociatedControls(String fieldName)
        {
            controlsList.Clear();
            try
            {
                ControlFactory factory = ControlFactory.Instance;
                DataRow dataRow = view.GetMetadata().GetFieldIdByNameAsDataRow(this.view.Name, fieldName);
                Field field = this.view.GetFieldById(int.Parse(dataRow["FieldId"].ToString()));

                if (view.Fields.Contains(field))
                {
                    controlsList.AddRange(factory.GetAssociatedControls(field));
                }
            }
            catch (Exception ex)
            {
                System.Console.Write(ex);
            }

            return controlsList;
        }

        /// <summary>
        /// Processes the control's data and navigates to next control
        /// </summary>
        /// <param name="sender">The control to be processed</param>
        //private void ProcessControl(object sender)
        //{
        //    this.mainForm.TabToNextControl = true;
        //    if (!IsValidData((Control)sender)) return;
        //    SetFieldData();
            

        //    if (IsLastControlOnPage((Control)sender) && !IsLastControlOnView((Control)sender))
        //    {
        //        if (!CheckRequiredFields((Control)sender)) return;
        //        if (HasCheckCode((Control)sender)) { if (!ProceedAfterCheckCodeExecution((Control)sender)) return; }
        //        GoToNextPage();
        //        SetFocusToFirstControl(this.currentPage, this.currentPanel);
        //    }
        //    else if (IsLastControlOnView((Control)sender))
        //    {
        //        if (!CheckRequiredFields((Control)sender)) return;
        //        //Set the focus to the first control on the page since the record is empty
        //        if (this.view.IsViewRecordEmpty())
        //        {
        //            viewExplorer.GoToFirstPage();
        //        }
        //        else
        //        {
        //            SaveRecord();
        //            this.EnterCheckCodeEngine.CurrentView.CheckCodeHandler(this, new RunCheckCodeEventArgs(EventActionEnum.CloseRecord, ""));
        //            this.GoToRecordHandler(this, new GoToRecordEventArgs("+"));
        //        }
        //        SetFocusToFirstControl(this.currentPage, this.currentPanel);
        //    }
        //    else
        //    {
        //        this.canvas.GoToNextControl(currentPage, this.view, (Control)sender);
        //    }
        //}


        /// <summary>
        /// Checks page to make sure data exists for required fields
        /// </summary>
        /// <param name="control">The last control to be processed on the page</param>
        /// <returns>boolean: true-required field have data, false-one is empty</returns>
        private bool CheckRequiredFields(Control control)
        {
            #region Input Validation
            if (control == null)
            {
                throw new ArgumentNullException("Control");
            }
            #endregion  //Input Validation

            ControlFactory factory = ControlFactory.Instance;

            //check required fields just for this page - before going to next page

            foreach (IField pageField in currentPage.GetView().GetFieldsOnPage(currentPage))
            {
                try
                {
                    if (pageField is InputFieldWithSeparatePrompt)
                    {
                        if (((InputFieldWithSeparatePrompt)pageField).IsRequired)
                        {
                            if (String.IsNullOrEmpty(((InputFieldWithSeparatePrompt)pageField).CurrentRecordValueString))
                            {                                
                                MsgBox.ShowInformation(string.Format(SharedStrings.FIELD_IS_REQUIRED, ((InputFieldWithSeparatePrompt)pageField).Name));
                                factory.GetAssociatedControls((InputFieldWithSeparatePrompt)pageField)[1].Focus();
                                control.Focus();
                                return false;
                            }
                        }
                    }
                }
                catch (InvalidCastException ice)
                {
                    //if cast fails, skip viewField
                    //Logger.Log(ice.Message.ToString());
                }
                catch
                {
                    throw new ApplicationException("Unable to check required fields.");
                }
            }

            return true;
        }



        /// <summary>
        /// Sets the data for text fields
        /// </summary>        
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void SetTextData(Field field, Control control)
        {
            if (control is TextBox || control is RichTextBox)
            {
                if (field is Epi.Fields.TextField)
                {
                    Epi.Fields.TextField textField = (Epi.Fields.TextField)field;

                    if ((control.Text.Trim().Length <= textField.MaxLength || textField.MaxLength <= 0) || (textField.IsReadOnly))
                    {
                        textField.CurrentRecordValueObject = control.Text;
                    }
                    else
                    {
                        string value = control.Text;

                        if (textField.CurrentRecordValue != null)
                        {
                            if (textField.CurrentRecordValue.Length <= textField.MaxLength)
                            {
                                control.Text = textField.CurrentRecordValue;
                                throw new System.Exception(string.Format("Value {0}: exceeds maximum Length of {1} for field [{2}].", value, textField.MaxLength, textField.Name));
                            }
                            else
                            {
                                textField.CurrentRecordValue = textField.CurrentRecordValue.Substring(0, textField.MaxLength - 1);
                            }
                        }
                        else
                        {
                            control.Text = "";
                            throw new System.Exception(string.Format("Value {0}: exceeds maximum Length of {1} for field [{2}].", value, textField.MaxLength, textField.Name));
                        }
                    }
                }
                else
                {
                    IDataField dataField = (IDataField)field;
                    dataField.CurrentRecordValueObject = control.Text.Trim();
                }
            }
        }

        /// <summary>
        /// Sets the data for text fields
        /// </summary>        
        /// <param name="textBoxControl">The control associated with the field text you want to set</param>
        /// <param name="control">The control associated with the Codes field</param>
        /// <param name="value">The associated value in Codes field</param>
        private void SetTextData(Control textBoxControl, Control control, string value)
        {
            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField(textBoxControl);
            ((IDataField)field).CurrentRecordValueObject = value;
            textBoxControl.Enabled = false;
        }

        /// <summary>
        /// Sets the data for number and phone number fields
        /// </summary>        
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void SetNumberData(Field field, Control control)
        {
            MaskedTextBox tempControl = new MaskedTextBox();
            tempControl.TextMaskFormat = ((MaskedTextBox)control).TextMaskFormat;
            tempControl.Mask = ((MaskedTextBox)control).Mask;
            tempControl.Text = ((MaskedTextBox)control).Text;

            tempControl.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;

            if (!string.IsNullOrEmpty(tempControl.Text))
            {
                tempControl.TextMaskFormat = MaskFormat.IncludeLiterals;
                ((IDataField)field).CurrentRecordValueObject = tempControl.Text;
            }
            else
            {
                ((IDataField)field).CurrentRecordValueObject = null;
            }
        }

        /// <summary>
        /// Sets the data for date, datetime and time fields
        /// </summary>        
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void SetDateTimeData(DateTimeField field, Control control)
        {
            field.CurrentRecordValueString = control.Text;
        }

        /// <summary>
        /// Sets the data for other masked text boxes that are not dates or times
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void SetOtherMaskedData(Field field, Control control)
        {
            // This assigning of the control's data to a temp control is done to stop the 'textchanged' event
            // from firing on the change to the TextMaskFormat property, which in turn was causing a dirty
            // event to be thrown and then initiated saves when it shouldn't have. This assignment should
            // not cause the record to become dirty. 
            //
            // However, maybe it would be better to just unsubscripe the event here and then re-subscribe after?
            MaskedTextBox tempControl = new MaskedTextBox();
            tempControl.TextMaskFormat = ((MaskedTextBox)control).TextMaskFormat;
            tempControl.Mask = ((MaskedTextBox)control).Mask;
            tempControl.Text = ((MaskedTextBox)control).Text;

            tempControl.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            ((IDataField)field).CurrentRecordValueObject = tempControl.Text;
        }

        /// <summary>
        /// Sets the data for combobox items
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void SetComboBoxData(Field field, Control control)
        {
            
            if (((ComboBox)control).SelectedIndex != -1)
            {
                if (field is YesNoField)
                {
                    if (((ComboBox)control).SelectedItem.Equals(config.Settings.RepresentationOfYes))
                    {
                        ((IDataField)field).CurrentRecordValueObject = "1";
                    }
                    else if (((ComboBox)control).SelectedItem.Equals(config.Settings.RepresentationOfNo))
                    {
                        ((IDataField)field).CurrentRecordValueObject = "0";
                    }
                }
                else if (field is DDLFieldOfLegalValues)
                {
                    ((IDataField)field).CurrentRecordValueObject = ((ComboBox)control).SelectedItem;
                }
                else if (field is DDLFieldOfCommentLegal)
                {
                    ((DDLFieldOfCommentLegal)field).CurrentRecordValueString = ((ComboBox)control).SelectedItem.ToString();
                }
                else if (field is DDLFieldOfCodes)
                {
                    ((IDataField)field).CurrentRecordValueObject = ((ComboBox)control).Text;
                }
                else if (field is DDListField)
                {
                    ((IDataField)field).CurrentRecordValueObject = ((ComboBox)control).SelectedValue;
                }
                else
                {
                    ((IDataField)field).CurrentRecordValueObject = ((ComboBox)control).SelectedValue;
                }
            }
            else
            {
                ((IDataField)field).CurrentRecordValueObject = null;
            }
        }

        /// <summary>
        /// Sets the data for checkbox items
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void SetCheckBoxData(Field field, Control control)
        {
            ((IDataField)field).CurrentRecordValueObject = ((CheckBox)control).Checked;
            ((IDataField)field).CurrentRecordValueString = (((CheckBox)control).Checked ? "1" : "0");
        }

        /// <summary>
        /// Gets the data for text fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void GetTextData(Field field, Control control)
        {
            if (((IDataField)field).CurrentRecordValueString.Equals(string.Empty) && field is GUIDField)
            {
                control.Text = ((GUIDField)field).NewGuid().ToString();
                SetTextData(field, control);
            }
            else
            {
                control.Text = ((IDataField)field).CurrentRecordValueString;
            }
        }

        /// <summary>
        /// Gets data for option fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void GetOptionData(OptionField field, Control control)
        {
            if (Util.IsEmpty(field.CurrentRecordValueObject))
            {   //no data, so clear all radiobuttons
                foreach (var item in control.Controls)
                {
                    ((RadioButton)item).Checked = false;
                }
            }
            else
            {
                if (!Util.IsEmpty(field.CurrentRecordValue))
                {
                    ((RadioButton)(control.Controls[Convert.ToInt16(field.CurrentRecordValue)])).Checked = true;
                }
            }
        }

        /// <summary>
        /// Gets the data for Mirror fields.
        /// </summary>
        /// <param name="mirrorField">The field that mirrors data of another field.</param>
        /// <param name="control">The control associated with the field</param>
        private void GetMirrorData(MirrorField mirrorField, Control control)
        {
            if (mirrorField.SourceField == null)
            {
                control.Text = string.Empty;
            }
            else
            {
                control.Text = ((IMirrorable)(mirrorField.SourceField)).GetReflectedValue();
            }
        }


        /// <summary>
        /// Gets the data for date fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void GetDateData(DateField field, Control control)
        {
            control.Text = field.CurrentRecordValueString;
        }
        
        /// <summary>
        /// Gets the data for time fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void GetTimeData(TimeField field, Control control)
        {
            control.Text = field.CurrentRecordValueString;
        }

        /// <summary>
        /// Gets the data for date time fields
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void GetDateTimeData(DateTimeField field, Control control)
        {
            control.Text = field.CurrentRecordValueString;
        }

        /// <summary>
        /// Gets the data for combobox items
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void GetComboboxData(Field field, Control control)
        {
            if (string.IsNullOrEmpty(((IDataField)field).CurrentRecordValueString))
            {
                ((ComboBox)control).SelectedIndex = -1;
                ((ComboBox)control).Text = string.Empty;
            }
            else
            {
                if (field is YesNoField)
                {
                    int num;
                    bool isNum = int.TryParse(((IDataField)field).CurrentRecordValueString, out num);
                    if (isNum)
                    {
                        if (int.Parse(((IDataField)field).CurrentRecordValueString) == 1)    //Yes                                        
                        {
                            ((ComboBox)control).Text = config.Settings.RepresentationOfYes;                            
                        }
                        else //No
                        {
                            ((ComboBox)control).Text = config.Settings.RepresentationOfNo;                            
                        }
                    }
                    else
                    {
                        if (((IDataField)field).CurrentRecordValueString.Equals(config.Settings.RepresentationOfYes))
                        {
                            ((ComboBox)control).Text = config.Settings.RepresentationOfYes;                            
                        }
                        else if (((IDataField)field).CurrentRecordValueString.Equals(config.Settings.RepresentationOfNo))
                        {
                            ((ComboBox)control).Text = config.Settings.RepresentationOfNo;                            
                        }
                    }
                }
                else
                {
                    string findString = ((IDataField)field).CurrentRecordValueString;

                    if (field is TableBasedDropDownField)
                    {
                        DataTable table = ((TableBasedDropDownField)field).CodeTable;
                        string codeName = ((TableBasedDropDownField)field).CodeColumnName;
                        string textColumnName = ((TableBasedDropDownField)field).TextColumnName;

                        ((ComboBox)control).BeginUpdate();

                        int indexCombo = ((ComboBox)control).FindString(findString);

                        if (indexCombo == -1)
                        { 
                            if (field is DDLFieldOfCommentLegal)
                            {
                                string numeric, trimmed;
                            
                                System.Windows.Forms.ComboBox.ObjectCollection items = ((ComboBox)control).Items;
                                foreach (var item in items)
                                {
                                    numeric = ((string)item).Substring(0, ((string)item).IndexOf('-'));
                                    trimmed = numeric.TrimStart(new char[] { '0' });
                                    if (trimmed == findString)
                                    {
                                        indexCombo = ((ComboBox)control).FindString(numeric);
                                        break;
                                    }
                                }
                            }
                        }

                        IsComboTextAssign = true;
                        
                        if (indexCombo == -1)
                        {
                            ((System.Windows.Forms.ComboBox)control).Text = string.Empty;
                            ((System.Windows.Forms.ComboBox)control).SelectedText = string.Empty;
                        }
                        else
                        {
                            ((System.Windows.Forms.ComboBox)control).SelectedIndex = indexCombo;
                        }

                        ((ComboBox)control).EndUpdate();
                        IsComboTextAssign = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the data for combo box items
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void GetCheckBoxData(Field field, Control control)
        {
            if (((IDataField)field).CurrentRecordValueObject == null)
            {
                ((CheckBox)control).Checked = false;
            }
            else
            {
                bool checkedValue = false;
                bool.TryParse(((IDataField)field).CurrentRecordValueObject.ToString(), out checkedValue);
                ((CheckBox)control).Checked = checkedValue;
            }
        }

        /// <summary>
        /// Gets the data for data grid items
        /// </summary>
        /// <param name="field">The field whose data is to be stored</param>
        /// <param name="control">The control associated with the field</param>
        private void GetDataGridViewData(Field field, Control control)
        {
            if (field is GridField)
            {
                if (Util.IsEmpty(((GridField)field).DataSource))
                {
                    DataTable dataTable = view.GetProject().CollectedData.GetGridTableData(view, (GridField)field);
                    dataTable.TableName = view.Name + field.Name;
                    ((GridField)field).DataSource = dataTable;
                }
                ((DataGridView)control).DataSource = ((GridField)field).DataSource;
            }
        }

        /// <summary>
        /// Check to see if a field has check code
        /// </summary>
        /// <param name="control">The control containing the check code, if any</param>
        private bool HasCheckCode(Control control)
        {
            bool hasCheckCode = false;
            ControlFactory factory = ControlFactory.Instance;
            Field field = factory.GetAssociatedField(control);

            if (field is IFieldWithCheckCodeBefore)
            {
                if (!String.IsNullOrEmpty(((IFieldWithCheckCodeBefore)field).CheckCodeBefore))
                {
                    hasCheckCode = true;
                }
            }

            if (field is IFieldWithCheckCodeAfter)
            {
                if (!String.IsNullOrEmpty(((IFieldWithCheckCodeAfter)field).CheckCodeAfter))
                {
                    hasCheckCode = true;
                }
            }

            if (field is IFieldWithCheckCodeClick)
            {
                if (!String.IsNullOrEmpty(((IFieldWithCheckCodeClick)field).CheckCodeClick))
                {
                    hasCheckCode = true;
                }
            }

            return hasCheckCode;
        }

        private bool GetConditionalIfValid(int trueConditionsCount, string[] conditionalIfStatement, string conditionalOperator)
        {
            bool isConditionalIfValid;

            if (conditionalOperator.Equals("and"))
            {
                //if conditional has n statements, the number of true conditions should equal n, 
                //   as the final true condition happens right before the action is processed

                double numberExpectedTrueStatements = Convert.ToDouble(conditionalIfStatement.Length) / 2;

                if (trueConditionsCount >= numberExpectedTrueStatements)
                {
                    isConditionalIfValid = true;
                }
                else
                {
                    isConditionalIfValid = false;
                }
            }
            else if (conditionalOperator.Equals("or"))  // conditionalOperator is "or" and at least one true condition exists
            {
                if (trueConditionsCount >= 1)
                {
                    isConditionalIfValid = true;
                }
                else
                {
                    isConditionalIfValid = false;
                }
            }
            else
            {
                isConditionalIfValid = false;
            }
            return isConditionalIfValid;
        }


        /// <summary>
        /// Method to avoid compiler warnings when a method has not been implemented and is called in a case statement
        /// </summary>
        private void ThrowMethodApplicationException()
        {
            throw new ApplicationException("The method or operation is not implemented.");
        }


        #endregion  //Private Methods


    }
}

