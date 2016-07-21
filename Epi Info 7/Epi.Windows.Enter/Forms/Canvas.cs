#region Namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Enter.PresentationLogic;


#endregion  //Namespaces

namespace Epi.Windows.Enter
{

    #region Public Delegates

    /// <summary>
    /// The event handler which fires when the panel is left
    /// </summary>
    /// <param name="panel">The panel that loses focus</param>
    public delegate void LeavePanelEventHandler(Panel panel);

    #endregion  //Public Delegates

    /// <summary>
    /// Enter's canvas
    /// </summary>
    public partial class Canvas : Epi.Windows.Docking.DockWindow
    {
        public event GuiMediator.GotoRecordEventHandler GotoRecordEvent;
        public event GuiMediator.OpenFieldEventHandler GotoFieldEvent;
        public event GuiMediator.CloseFieldEventHandler CloseFieldEvent;
        public event GuiMediator.FieldChangeEventHandler FieldChangeEvent;
        public event GuiMediator.ClickFieldEventHandler ClickFieldEvent;
        public event GuiMediator.DirtyFieldEventHandler DirtyFieldEvent;
        public event DataGridViewRowEventHandler DataGridRowAddedEvent;

        #region Private Members

        private Panel currentPanel;
        private View currentView;
        private List<Panel> panelsList = new List<Panel>();
        private Graphics bufferGraphics;
        private Bitmap bufferBitmap;
        private Configuration config;
        private EnterMainForm mainFrm;

        static public Color HighlightColor = Color.Yellow;
        static public Color UnHighlightColor = Color.White;

        #endregion  //Private Members

        #region Public Properties
        /// <summary>
        /// List of panels loaded for the view
        /// </summary>
        public List<Panel> Panels
        {
            get
            {
                if (this.canvasPanel.Controls.Count > 0)
                {
                    return new List<Panel>(){this.canvasPanel.Controls[0] as Panel};
                }
                else
                {
                    return new List<Panel>();
                }
            }
        }

        public void DisposePanels()
        {
            try
            {
                foreach (Panel panel in panelsList)
                {
                    panel.MouseDown -= new MouseEventHandler(panel_MouseDown);
                    panel.Paint -= new PaintEventHandler(DrawBorderRect);
                    panel.Dispose();
                }
            }
            catch (Exception ex)
            {
                //
            }
            finally
            {
                panelsList.Clear();
                GC.Collect();
            }
        }


        public View CurrentView
        {
            get { return this.currentView; }
            set { this.currentView = value; }
        }

        #endregion  //Public Properties

        #region Public Events
        /// <summary>
        /// Event to be raised when a panel loses focus
        /// </summary>
        public event LeavePanelEventHandler PanelLeft;
        #endregion  //Public Events

        #region Constructors
        /// <summary>
        /// Constructor for Canvas
        /// </summary>
        public Canvas(MainForm frm)
        {
            config = Configuration.GetNewInstance();
            mainFrm = frm as EnterMainForm;
            InitializeComponent();
            canvasPanel.Dock = DockStyle.None;
            canvasPanel.BorderStyle = BorderStyle.FixedSingle;
            canvasPanel.Visible = false;            
            RedrawCanvasBackground();
            
        }


        #endregion  //Constructors

        #region Public Methods

        /// <summary>
        /// Resets the canvas to its original state
        /// </summary>
        public void Reset()
        {
            foreach (Control control in canvasPanel.Controls)
            {
                if (control is Panel)
                {
                    Panel jPanel = control as Panel;

                    foreach (Control jControl in jPanel.Controls)
                    {
                        if (jControl is DataGridView)
                        {
                            DataGridView dgv = jControl as DataGridView;
                            dgv.DataSource = null;
                        }
                    }                    
                }
                canvasPanel.Controls.Remove(control);
                control.Dispose();
            }
            RedrawCanvasBackground();
            canvasPanel.Visible = false;
        }

        /// <summary>
        /// Sets the dirty flag
        /// </summary>        
        public void SetDirtyFlag()
        {
            DirtyFieldEvent(this, new EventArgs());            
        }

        /// <summary>
        /// Enable/Disable the current record
        /// </summary>
        /// <param name="view">The current view</param>
        /// <param name="enable">Boolean to indicate whether to enable or disable record</param>
        public void EnableDisableCurrentRecord(View view, bool enable)
        {
            #region Input Validation
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            #endregion  //Input Validation

            //When loading a new page or adding a new record, unselect
            //first entry for comboboxes and enable controls
            foreach (Panel panel in this.Panels)
            {
                Epi.Windows.Enter.PresentationLogic.ControlFactory factory = Epi.Windows.Enter.PresentationLogic.ControlFactory.Instance;

                foreach (Control control in panel.Controls)

                //foreach (Page page in view.Pages)
                //{
                //    foreach (Control control in currentPanel.Controls)
                {
                    Field field = factory.GetAssociatedField(control);

                    // Test for perf improvements on record nav
                    if (field is InputFieldWithSeparatePrompt && control.Enabled == enable)
                    {
                        continue;
                    }

                    if (control is ComboBox)
                    {
                        if (view.CurrentRecordId == 0)
                        {
                            ((ComboBox)control).SelectedIndex = -1;
                            ((ComboBox)control).Text = string.Empty;
                        }
                        else
                        {                            

                            if (string.IsNullOrEmpty(((IDataField)field).CurrentRecordValueString))
                            {
                                ((ComboBox)control).SelectedIndex = -1;
                                ((ComboBox)control).Text = string.Empty;
                            }
                        }
                    }                    

                    control.Visible = true;     //reset control to visible in case the Hide command was executed
                    control.Enabled = enable;
                   

                    // If we're enabling the record (typically what's done unless 'mark record for deletion' button is pressed)
                    if (enable)
                    {
                        if (field is InputFieldWithSeparatePrompt)
                        {
                            control.Enabled = !((InputFieldWithSeparatePrompt)field).IsReadOnly ? true : false;
                        }
                        else if (field is InputFieldWithoutSeparatePrompt)
                        {
                            control.Enabled = !((InputFieldWithoutSeparatePrompt)field).IsReadOnly ? true : false;
                        }

                        if (field is MirrorField)
                        {
                            control.Enabled = false;
                        }
                    }
                }
            }
            currentView = view;
            return;
        }

        /// <summary>
        /// Show a panel
        /// </summary>
        /// <param name="panel">The panel to show</param>
        public void ShowPanel(Panel panel)
        {
            panel.Visible = true;
            panel.BringToFront();
            currentPanel = panel;
        }

        /// <summary>
        /// Add a panel to the canvas
        /// </summary>
        /// <param name="panel">The panel to add</param>
        public void SetPanelProperties(Panel panel)
        {
            panel.Dock = DockStyle.Fill;
            panel.MouseDown += new MouseEventHandler(panel_MouseDown);
            panel.BackColor = Color.Transparent;            
            panel.AutoScroll = false;
            panel.Paint += new PaintEventHandler(DrawBorderRect);
            //panelsList.Clear();
            //panelsList.Add(panel);

            
        }

        /// <summary>
        /// Hides controls for check code execution
        /// </summary>                
        /// <param name="controlsList">The list of controls to be hidden</param>
        public void HideCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("controlsList");
            }
            #endregion  //Input Validation

            Control[] tmpControlList = new Control[controlsList.Count];
            controlsList.CopyTo(tmpControlList);

            foreach (Control control in tmpControlList)
            {
                controlsList[controlsList.IndexOf(control)].Visible = false;
            }
        }

        /// <summary>
        /// Hides all controls except those specified for check code execution
        /// </summary>
        /// <param name="controlsList">The list of controls to not be hidden</param>
        public void HideExceptCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("controlsList");
            }
            #endregion  //Input Validation

            foreach (Panel panel in Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    if (!controlsList.Contains(control))
                    {
                        control.Visible = false;
                    }
                    else
                    {
                        control.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Unhides controls for check code execution
        /// </summary>
        /// <param name="controlsList">The list of controls to be unhidden </param>
        public void UnhideCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("controlsList");
            }
            #endregion  //Input Validation

            foreach (Panel panel in Panels)
            {
                foreach (Control control in controlsList)
                {
                    if (panel.Controls.Contains(control))
                    {
                        control.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Unhides all controls except those specified for check code execution
        /// </summary>
        /// <param name="controlsList">The list of controls to be hidden </param>
        public void UnhideExceptCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("controlsList");
            }
            #endregion  //Input Validation

            foreach (Panel panel in Panels)
            {
                foreach (Control control in controlsList)
                {
                    if (panel.Controls.Contains(control))
                    {
                        control.Visible = false;
                    }
                }
            }
        }


        /// <summary>
        /// Highlights controls for check code execution
        /// </summary>                
        /// <param name="controlsList">The list of controls to be hidden</param>
        public void HighlightCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("controlsList");
            }
            #endregion  //Input Validation

            Control[] tmpControlList = new Control[controlsList.Count];
            controlsList.CopyTo(tmpControlList);

            foreach (Control control in tmpControlList)
            {
                controlsList[controlsList.IndexOf(control)].BackColor = HighlightColor;
            }
        }

        /// <summary>
        /// Highlights all controls except those specified for check code execution
        /// </summary>
        /// <param name="controlsList">The list of controls to not be hidden</param>
        public void HighlightExceptCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("controlsList");
            }
            #endregion  //Input Validation

            foreach (Panel panel in Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    if (!controlsList.Contains(control))
                    {
                        control.BackColor = HighlightColor;
                    }
                    else
                    {
                        control.BackColor = HighlightColor;
                    }
                }
            }
        }


        /// <summary>
        /// UnHighlights controls for check code execution
        /// </summary>                
        /// <param name="controlsList">The list of controls to be hidden</param>
        public void UnHighlightCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("controlsList");
            }
            #endregion  //Input Validation

            Control[] tmpControlList = new Control[controlsList.Count];
            controlsList.CopyTo(tmpControlList);

            foreach (Control control in tmpControlList)
            {
                controlsList[controlsList.IndexOf(control)].BackColor = UnHighlightColor;
            }
        }

        /// <summary>
        /// UnHighlights all controls except those specified for check code execution
        /// </summary>
        /// <param name="controlsList">The list of controls to not be hidden</param>
        public void UnHighlightExceptCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("controlsList");
            }
            #endregion  //Input Validation

            foreach (Panel panel in Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    if (!controlsList.Contains(control))
                    {
                        control.BackColor = UnHighlightColor;
                    }
                    else
                    {
                        control.BackColor = UnHighlightColor;
                    }
                }
            }
        }


        /// <summary>
        /// Highlights controls for check code execution
        /// </summary>                
        /// <param name="controlsList">The list of controls to be hidden</param>
        public void EnableCheckCodeItems(List<Control> pList)
        {
            #region Input Validation
            if (pList == null)
            {
                //throw new ArgumentNullException("controlsList");
                return;
            }
            #endregion  //Input Validation


            for (int i = 0; i < pList.Count; i++)
            {
                pList[i].Enabled = true;
            }

            /*
            Control[] tmpControlList = new Control[controlsList.Count];
            controlsList.CopyTo(tmpControlList);

            foreach (Control control in tmpControlList)
            {
                controlsList[controlsList.IndexOf(control)].Enabled = true;
            }*/
        }

        /// <summary>
        /// Highlights all controls except those specified for check code execution
        /// </summary>
        /// <param name="controlsList">The list of controls to not be hidden</param>
        public void EnableExceptCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                //throw new ArgumentNullException("controlsList");
                return;
            }
            #endregion  //Input Validation

            foreach (Panel panel in Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    if (!controlsList.Contains(control))
                    {
                        control.Enabled = true;
                    }
                    else
                    {
                        control.Enabled = true;
                    }
                }
            }
        }


        /// <summary>
        /// Highlights controls for check code execution
        /// </summary>                
        /// <param name="controlsList">The list of controls to be hidden</param>
        public void DisableCheckCodeItems(List<Control> pList)
        {
            #region Input Validation
            if (pList == null)
            {
                //throw new ArgumentNullException("controlsList");
                return;
            }
            #endregion  //Input Validation

            for (int i = 0; i < pList.Count; i++)
            {
                pList[i].Enabled = false;
            }
        }

        /// <summary>
        /// Highlights all controls except those specified for check code execution
        /// </summary>
        /// <param name="controlsList">The list of controls to not be hidden</param>
        public void DisableExceptCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                //throw new ArgumentNullException("controlsList");
                return;
            }
            #endregion  //Input Validation

            foreach (Panel panel in Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    if (!controlsList.Contains(control))
                    {
                        control.Enabled = false;
                    }
                    else
                    {
                        control.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Assigns controls to desired text
        /// </summary>
        /// <param name="controlsList">The list of controls to be assigned</param>
        public void AssignCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                //throw new ArgumentNullException("controlsList");
                return;
            }
            #endregion  //Input Validation

            foreach (Panel panel in Panels)
            {
                foreach (Control control in controlsList)
                {
                    if (!(control is Label))
                    {
                        if (panel.Controls.Contains(control))
                        {
                            control.Text = control.Text.Substring(1, control.Text.Length - 2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears content of controls for check code execution
        /// </summary>        
        /// <param name="controlsList">List of controls to be cleared</param>
        public void ClearCheckCodeItems(List<Control> controlsList)
        {
            #region Input Validation
            if (controlsList == null)
            {
                //throw new ArgumentNullException("controlsList");
                return;
            }
            #endregion  //Input Validation

            //foreach (Panel panel in Panels)
            //{
                foreach (Control control in controlsList)
                {
                    //if (panel.Controls.Contains(control))
                    //{
                        if (control is TextBoxBase)
                        {
                            control.Text = string.Empty;
                        }
                        else if (control is CheckBox)
                        {
                            ((CheckBox)control).Checked = false;
                        }
                        else if (control is ComboBox)
                        {
                            ((ComboBox)control).SelectedIndex = -1;
                            ((ComboBox)control).Text = string.Empty;
                        }/*
                        else
                        {
                            // do nothing for now
                        }*/
                    //}
                }
            //}
        }

        /// <summary>
        /// Hide all the panels
        /// </summary>
        public void HideAll()
        {
            foreach (Control control in canvasPanel.Controls)
            {
                control.Visible = false;
            }
        }

        public void DesignerFocus()
        {
            canvasPanel.Focus();
        }

        /// <summary>
        /// Sets the focus to a specific field
        /// </summary>
        /// <param name="controlsList">A list of controls to set the focus to</param>
        public void GoToField(List<Control> controlsList, Epi.Fields.Field field)
        {
            #region Input Validation
            if (controlsList == null)
            {
                throw new ArgumentNullException("ControlsList");
            }
            #endregion  //Input Validation
            
            for (int i = 0; i <= controlsList.Count - 1; i++)
            {
                if (!(controlsList[i] is Label))
                {
                    if (!currentPanel.Controls.Contains(controlsList[i]))
                    {
                        foreach (Panel panel in canvasPanel.Controls)
                        {
                            if (panel.Controls.Contains(controlsList[i]))
                            {
                                Control control = controlsList[i];
                                this.Invoke(new MethodInvoker(delegate() { SetFocusToControl(control, field); })); 
                            }
                        }
                    }
                    else
                    {
                        Control control = controlsList[i];
                        this.Invoke(new MethodInvoker(delegate() { SetFocusToControl(control, field); })); 
                    }
                }
            }
        }

        /// <summary>
        /// Adds the autosearch results grid to the current panel
        /// </summary>
        /// <param name="dataGrid1">Auto search results grid</param>
        public void AddGridToPanel(DataGrid dataGrid1)
        {
            currentPanel.Controls.Add(dataGrid1);
            dataGrid1.BringToFront();
        }

        /// <summary>
        /// Sets the focus to a given control.
        /// </summary>
        /// <param name="control">The control to focus on.</param>
        /// <param name="field">The field associated with the control.</param>
        public void SetFocusToControl(Control control, Field field)
        {
            #region Input Validation
            if (control == null)
            {
                throw new ArgumentNullException("Epi.Windows.Enter.SetFocusToControl control is null"); 
            }

            if (field == null)
            {
                throw new ArgumentNullException("Epi.Windows.Enter.SetFocusToControl field is null"); 
            }
            #endregion // Input Validation
            if (control.Focused && this.IsEventEnabled)
            {
                control_Enter(control, new EventArgs());
            }
            else
            {
                control.Focus();
            }

            if (control is MaskedTextBox)
            {
                ((MaskedTextBox)control).SelectAll();
            }
            else if (control is TextBoxBase)
            {
                if (control is RichTextBox)
                {
                    ((TextBoxBase)control).DeselectAll();
                    ((TextBoxBase)control).Select(control.Text.Length, 0);
                }
                else
                {
                    if (control.Text.Length > 0)
                    {
                        ((TextBoxBase)control).SelectAll();
                    }
                    else
                    {
                        ((TextBoxBase)control).SelectionStart = 0;
                    }
                }
            }
            else if (control is ComboBox)
            {
                if (string.IsNullOrEmpty(((InputFieldWithSeparatePrompt)field).CurrentRecordValueString))
                {
                    ((ComboBox)control).SelectedIndex = -1;
                }
                else if (field is YesNoField)
                {
                    if (((IDataField)field).CurrentRecordValueObject.ToString() == "1")
                    {
                        ((ComboBox)control).SelectedItem = config.Settings.RepresentationOfYes;
                        ((ComboBox)control).Text = config.Settings.RepresentationOfYes;
                    }
                    else if (((IDataField)field).CurrentRecordValueObject.ToString() == "0")
                    {
                        ((ComboBox)control).SelectedItem = config.Settings.RepresentationOfNo;
                        ((ComboBox)control).Text = config.Settings.RepresentationOfNo;
                    }
                }
                else
                {
                    string val = ((IDataField)field).CurrentRecordValueString;

                    foreach (object item in ((ComboBox)control).Items)
                    {
                        if (item is String)
                        {
                            string[] parts = ((string)item).Split('-');
                            if (val == parts[0])
                            {
                                ((ComboBox)control).Text = ((string)item);
                                ((ComboBox)control).SelectedIndex = ((ComboBox)control).FindString(((string)item));
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the focus to the first input control on the current panel
        /// </summary>
        /// <param name="currentPage">The current page</param>
        /// <param name="currentPanel">The current panel</param>
        public void SetFocusToFirstControl(Page currentPage, Panel currentPanel)
        {
            #region Input Validation

            if (currentPage == null)
            {
                throw new ArgumentNullException("Epi.Windows.Enter.SetFocusToFirstControl currentPage is null"); 
            }

            if (currentPanel == null)
            {
                throw new ArgumentNullException("Epi.Windows.Enter.SetFocusToFirstControl currentPanel is null"); 
            }
            #endregion  //Input Validation

            double minTabIndex = currentPage.GetMetadata().GetMinTabIndex(currentPage.Id, currentPage.GetView().Id);
            if (minTabIndex != -1)
            {
                foreach (Control control in currentPanel.Controls)
                {
                    if (control is TextBox || control is RichTextBox || control is CheckBox || control is ComboBox || control is MaskedTextBox || control is Button)
                    {
                        Epi.Windows.Enter.PresentationLogic.ControlFactory factory = Epi.Windows.Enter.PresentationLogic.ControlFactory.Instance;
                        Field field = factory.GetAssociatedField(control);
                        ((RenderableField)field).Tag = "";
                    }
                }

                foreach (Control control in currentPanel.Controls)
                {
                    if (control is TextBox || control is RichTextBox || control is CheckBox || control is ComboBox || control is MaskedTextBox || control is Button)
                    {
                        Epi.Windows.Enter.PresentationLogic.ControlFactory factory = Epi.Windows.Enter.PresentationLogic.ControlFactory.Instance;
                        Field field = factory.GetAssociatedField(control);

                        if (((RenderableField)field).TabIndex == minTabIndex)
                        {
                            if (control.Enabled)
                            {
                                // The problem with calling SetFocusToControl directly is that
                                // later on, other controls steal the focus in ways that we
                                // cannot control. This theft has unintended side-effects, e.g 
                                // running check code in ways that aren't desired, and/or the focus 
                                // doesn't actually get placed in the first field of the page in 
                                // some cases. By invoking the method, we pass it to the message queue
                                // and it is run after the other focus-stealing events are called.
                                // EK 12/7/2010
                                this.BeginInvoke(new MethodInvoker(delegate() { SetFocusToControl(control, field); }));
                                ((RenderableField)field).Tag = "tabbed";
                                return;
                            }
                            else
                            {
                                GoToNextControl(currentPage, currentView, control);
                            }
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Sets the focus of the next control depending upon the tab index
        /// </summary>
        /// <param name="currentPage">The current page</param>
        /// <param name="currentView">The current panel</param>
        /// <param name="currentControl">The current control that is losing focus</param>
        public void GoToNextControl(Page currentPage, View currentView, Control currentControl)
        {
            #region Input Validation

            if (currentPage == null)
            {
                throw new ArgumentNullException("Current Page");
            }

            if (currentPanel == null)
            {
                throw new ArgumentNullException("Current Panel");
            }

            if (currentControl == null)
            {
                throw new ArgumentException("Control");
            }

            #endregion  //Input Validation

            Epi.Windows.Enter.PresentationLogic.ControlFactory factory = Epi.Windows.Enter.PresentationLogic.ControlFactory.Instance;
            Field currentField = factory.GetAssociatedField(currentControl);

            if (currentField == null)
            {
                return;
            }

            double currentTabIndex = ((RenderableField)currentField).TabIndex;
            double nextTabIndex = currentPage.GetMetadata().GetNextTabIndex(currentPage, currentView, currentTabIndex);
            double maxTabIndex = currentPage.GetMetadata().GetMaxTabIndex(currentPage.Id, currentView.Id, false);
            foreach (Control control in currentPanel.Controls)
            {
                if (control is TextBox || control is RichTextBox || control is CheckBox || control is ComboBox || control is Button || control is MaskedTextBox || control is GroupBox || control is DateTimePicker)
                {
                    Field field = factory.GetAssociatedField(control);
                    if (((RenderableField)field).TabIndex == nextTabIndex || ((RenderableField)field).TabIndex == currentTabIndex & control != currentControl & ((RenderableField)field).Tag == "")
                    {
                        if (!control.Visible || !control.Enabled)  //if hidden by check code command or disabled due to read only property being set
                        {
                            if (control.TabIndex != maxTabIndex)
                            {
                                GoToNextControl(currentPage, currentView, control);
                            }
                        }

                        ((RenderableField)field).Tag ="tabbed";

                        this.EnableTabToNextControl = false;

                        if (control.Focused && this.IsEventEnabled)
                        {
                            control_Enter(control, new EventArgs());
                        }
                        else if((control is GroupBox) == false)
                        {
                                control.Focus();
                        }

                        if (control is TextBoxBase)
                        {
                            ((TextBoxBase)control).SelectAll();
                        }

                        if ((control is TextBox || control is RichTextBox || control is ComboBox || control is MaskedTextBox || control is DateTimePicker) &&
                            (field is InputFieldWithSeparatePrompt))
                        {
                            if (((InputFieldWithSeparatePrompt)field).IsRequired)
                            {
                                control.CausesValidation = true;
                            }
                            else
                            {
                                control.CausesValidation = false;
                            }
                            
                            if (field is DateField)
                            {
                                if (((DateField)field).Upper.Length + ((DateField)field).Lower.Length > 0)
                                {
                                    control.CausesValidation = true;
                                }
                            }
                        }
                        else if(control is GroupBox )
                        {
                            control.Controls[0].Focus();
                        }

                        this.EnableTabToNextControl = false;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Updates current view's controls with config settings
        /// </summary>
        public void UpdateSettings()
        {
            //TODO Add other option settings as they pertain to the Enter module

            
            foreach (Panel panel in this.Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    if (control is ComboBox)
                    {
                        Epi.Windows.Enter.PresentationLogic.ControlFactory factory = Epi.Windows.Enter.PresentationLogic.ControlFactory.Instance;
                        Field field = factory.GetAssociatedField(control);

                        if (field is YesNoField)
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("name", typeof(string));
                            dt.Columns.Add("value", typeof(int));
                            DataRow dr;
                            dr = dt.NewRow();
                            dr["name"] = config.Settings.RepresentationOfYes;
                            dr["value"] = Constants.YES;
                            dt.Rows.Add(dr);

                            dr = dt.NewRow();
                            dr["name"] = config.Settings.RepresentationOfNo;
                            dr["value"] = Constants.NO;
                            dt.Rows.Add(dr);

                            ((ComboBox)control).ValueMember = "value";
                            ((ComboBox)control).DisplayMember = "name";
                            ((ComboBox)control).DataSource = dt;
                        }
                    }
                }
            }
        }

        #endregion  //Public Methods

        #region Private Methods

        #region Background management
        private void pnlDesigner_Resize(object sender, EventArgs e)
        {
            RedrawCanvasBackground();
        }

        /// <summary>
        /// Handles the Mouse Down event of the panel
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int ix = ((Control)sender).Controls.Count - 1; ix >= 0; ix--)
                    if (((Control)sender).Controls[ix] is MonthCalendar)
                    {
                        ((Control)sender).Controls[ix].Dispose();
                        ControlFactory factory = ControlFactory.Instance;
                        factory.IsPopup = false;
                    }
                this.Focus(); // needed to grab back focus for scroll-wheel effects
            }
        }

        private void mainForm_PageChanged(object sender, EventArgs e)
        {
            RedrawCanvasBackground();
        }

        public void RedrawCanvasBackground()
        {
            
            SaveSnapshot();
            DrawPicture(bufferGraphics);
        }

        private void SaveSnapshot()
        {
            if ((mainFrm.CurrentBackgroundImage==null) && (mainFrm.CurrentBackgroundColor.Equals(Color.Empty)))
            {
                canvasPanel.BackColor = Color.White;
            }
            else
            {
                canvasPanel.BackgroundImageLayout = ImageLayout.None;
                if (canvasPanel.Size.Width > 0 && canvasPanel.Size.Height > 0)
                {
                    Bitmap b = new Bitmap(canvasPanel.Size.Width, canvasPanel.Size.Height);
                    this.bufferGraphics = Graphics.FromImage(b);
              
                    if(!(mainFrm.CurrentBackgroundColor.Equals(Color.Empty)))
                    {
                        canvasPanel.BackColor = mainFrm.CurrentBackgroundColor;
                    }
                    this.bufferGraphics.Clear(canvasPanel.BackColor);
                    if (mainFrm.CurrentBackgroundImage != null )
                    {
                        Image img = mainFrm.CurrentBackgroundImage;
                        switch (mainFrm.CurrentBackgroundImageLayout.ToUpperInvariant())
                        {
                            case "TILE":
                                TextureBrush tileBrush = new TextureBrush(img, System.Drawing.Drawing2D.WrapMode.Tile);
                                bufferGraphics.FillRectangle(tileBrush, 0, 0, canvasPanel.Size.Width, canvasPanel.Size.Height);
                                tileBrush.Dispose();
                                break;

                            case "STRETCH":
                                bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                bufferGraphics.DrawImage(img, 0, 0, canvasPanel.Size.Width, canvasPanel.Size.Height);
                                bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                                break;

                            case "CENTER":
                                int centerX = (canvasPanel.Size.Width / 2) - (img.Size.Width / 2);
                                int centerY = (canvasPanel.Size.Height / 2) - (img.Size.Height / 2);
                                bufferGraphics.DrawImage(img, centerX, centerY);
                                break;

                            default:
                                bufferGraphics.DrawImage(img, 0, 0);
                                break;
                        }
                    }

                    bufferGraphics.DrawImage(b, 0, 0);
                    bufferBitmap = b;
                    canvasPanel.BackgroundImage = b;
                }
            }
        }

        private void DrawGrid(Bitmap b)
        {
            for (int y = 0; y < b.Height; y += config.Settings.GridSize * 10)
            {
                for (int x = 0; x < b.Width; x += config.Settings.GridSize * 10)
                {
                    b.SetPixel(x, y, Color.Black);
                }
            }
        }

        private void DrawPicture(Graphics g)
        {
            if (bufferBitmap != null)
            {
                g.DrawImage(bufferBitmap, 0, 0);
            }
        }
        #endregion

        private Dictionary<int, object> repeatableFields;
        private Epi.Collections.NamedObjectCollection<IDataField> dataFields;
        public bool hasRepeatableFields = false;

        /// <summary>
        /// Clears all values in controls
        /// </summary>
        public void SetNewRecordValues()
        {
            foreach (IDataField dataField in this.currentView.Fields.DataFields)
            {
                if (dataField is GlobalRecordIdField)
                {
                    ((GlobalRecordIdField)dataField).NewValue();
                }
                else if (dataField is ForeignKeyField == false)
                {
                    dataField.SetNewRecordValue();
                }
            }

            foreach (GridField grid in currentView.Fields.GridFields)
            {
                grid.SetNewRecordValue();
            }


            SetControlDefaults();
        }
        
        /// <summary>
        /// Sets the default values for the controls on the panel
        /// </summary>
        private void SetControlDefaults()
        {
            foreach (Panel panel in this.Panels)
            {
                foreach (Control control in panel.Controls)
                {
                    if (control is PictureBox)
                    {
                        (control as PictureBox).ImageLocation = string.Empty;
                    }
                    if (control is TextBox || control is RichTextBox || control is ComboBox || control is CheckBox || control is MaskedTextBox || control is DataGridView || control is GroupBox)
                    {
                        ControlFactory factory = ControlFactory.Instance;
                        Field field = factory.GetAssociatedField(control);

                        if (field is GUIDField)
                        {
                            control.Text = ((GUIDField)field).NewGuid().ToString();
                            SetTextData(field, control);
                        }
                        else if (field is IDataField || field is MirrorField || field is OptionField)
                        {
                            if (control is CheckBox)
                            {
                                ((CheckBox)control).Checked = false;
                            }
                            else if (control is ComboBox)
                            {
                                ((ComboBox)control).Text = string.Empty;
                                ((ComboBox)control).SelectedIndex = -1;
                                ((ComboBox)control).SelectedIndex = -1; // necessary to prevent an odd occurrence of values being pre-populated into new records with Yes/No fields. See defect #221. TODO: Re-investigate later why this is the case and why two of these commands must be issued.
                            }
                            else if (control is GroupBox)
                            {
                                foreach (Control ofGroupBox in control.Controls)
                                {
                                    if (ofGroupBox is RadioButton)
                                    {
                                        ((RadioButton)ofGroupBox).Checked = false;
                                    }
                                }
                            }
                            else
                            {
                                control.Text = string.Empty;
                            }
                        }
                        else if (field is GridField)
                        {
                            ((GridField)field).DataSource = null;
                            GetDataGridViewData(field, control);
                            ((GridField)field).DataSource.Clear();
                        }
                    }
                }
            }
        }
        #endregion


        public void DrawRequiredBorder(Control pValue)
        {
            /**/
            // The parents that'll draw the borders for their children
            HashSet<Control> parents = new HashSet<Control>();

            // The controls' types that you want to apply the new border on them
            List<Type> controlsThatHaveBorder = new List<Type> { typeof(TextBox), typeof(ComboBox), typeof(Epi.Windows.Enter.Controls.LegalValuesComboBox) };

            parents.Add(pValue.Parent);

            foreach (var parent in parents)
            {
                parent.Paint += (sender, e) =>
                {
                    if (pValue.Parent != sender || pValue is Epi.Windows.Controls.TransparentLabel) return;
                    // Create the border's bounds
                    var bounds = pValue.Bounds;
                    var activeCountrolBounds = new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 1, bounds.Height + 1);
                    
                    // Draw the border...
                    ((Control)sender).CreateGraphics().DrawRectangle(Pens.IndianRed, activeCountrolBounds);
                };
            }
        }

        public void UnDrawRequiredBorder(Control pValue)
        {
            if (pValue is Epi.Windows.Controls.TransparentLabel)
            {
                return;
            }
            // Create the border's bounds
            var bounds = pValue.Bounds;
            var activeCountrolBounds = new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 1, bounds.Height + 1);

            // Draw the border...
            this.CreateGraphics().DrawRectangle(Pens.White, activeCountrolBounds);

/*
            // The parents that'll draw the borders for their children
            HashSet<Control> parents = new HashSet<Control>();

            // The controls' types that you want to apply the new border on them
            List<Type> controlsThatHaveBorder = new List<Type> { typeof(TextBox), typeof(ComboBox), typeof(Epi.Windows.Enter.Controls.LegalValuesComboBox) };

            parents.Add(pValue.Parent);

            foreach (var parent in parents)
            {
                parent.Paint += (sender, e) =>
                {
                    if (pValue.Parent != sender || pValue is Epi.Windows.Controls.TransparentLabel) return;
                    // Create the border's bounds
                    Rectangle bounds = pValue.Bounds;
                    Rectangle activeCountrolBounds = new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 1, bounds.Height + 1);

                    // Draw the border...
                    ((Control)sender).CreateGraphics().DrawRectangle(Pens.White, activeCountrolBounds);
                };
            }*/
        }


        void DrawBorderRect(object sender, System.Windows.Forms.PaintEventArgs e)
        {

            Panel panel = ((Panel)sender);
            Control.ControlCollection cc = panel.Controls;
            foreach (Control control in cc)
            {
                if (control is Epi.Windows.Controls.TransparentLabel || control is Epi.Windows.Controls.FieldGroupBox)
                {
                    continue;
                }
    
                Field f =  ControlFactory.Instance.GetAssociatedField(control);
                if (f is InputFieldWithSeparatePrompt)
                {
                    InputFieldWithSeparatePrompt field = (InputFieldWithSeparatePrompt)f;

                    Graphics g = e.Graphics;

                    var bounds = control.Bounds;
                    var activeCountrolBounds = new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 1, bounds.Height + 1);
                    // Draw the border...
                    if (field.IsRequired == true)
                    {
                        g.DrawRectangle(Pens.Goldenrod, activeCountrolBounds);
                    }
                    else
                    {
                        g.DrawRectangle(Pens.White, activeCountrolBounds);
                    }
                }
                
            };
        }

        public void DrawBorderRect(Control control)
        {
            if (control is Epi.Windows.Controls.TransparentLabel || control is Epi.Windows.Controls.FieldGroupBox)
            {
                return;
            }

            Field associatedField = ControlFactory.Instance.GetAssociatedField(control);
            
            if (associatedField is InputFieldWithSeparatePrompt)
            {
                InputFieldWithSeparatePrompt field = (InputFieldWithSeparatePrompt)associatedField;
                var bounds = control.Bounds;
                var activeCountrolBounds = new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 1, bounds.Height + 1);

                Graphics graphics = ((Control)Panels[0]).CreateGraphics();

                if (field.IsRequired == true)
                {
                    graphics.DrawRectangle(Pens.Goldenrod, activeCountrolBounds);
                }
                else
                {
                    graphics.DrawRectangle(Pens.White, activeCountrolBounds);
                }
            }
        }
    }
}
