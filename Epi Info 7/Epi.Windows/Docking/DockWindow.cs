using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Xml;

namespace Epi.Windows.Docking
{
    /// <summary>
    /// Enumerates the standard types for dockable windows.
    /// </summary>
    public enum DockContainerType
    {
        /// <summary>
        /// No docking features enabled.
        /// This window will behave normally.
        /// </summary>
        None,
        /// <summary>
        /// The window will get the document status.
        /// Document windows can solely be added (fill or split) to other documents.
        /// </summary>
        Document,
        /// <summary>
        /// The window will get the tool window status.
        /// Tool windows may be docked everywhere.
        /// </summary>
        ToolWindow
    }

    /// <summary>
    /// The DockWindow class is derived from the standard framework class System.Windows.Forms.Form.
    /// It prepares the window for docking with the help of an own container of the DockPanel type.
    /// </summary>
    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    public partial class DockWindow : FormBase
    {
        #region Protected Attributes
        /// <summary>
        /// The main form the dock window belongs to
        /// </summary>
        protected MainForm mainForm = null;
        #endregion Protected Attributes

        #region Construction and dispose
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWindow"/> class.
        /// For exclusive use of the designer.
        /// </summary>
        public DockWindow()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockWindow"/> class.
        /// For exclusive use of the designer.
        /// <param name="frm">The Main Form the dock window belongs to</param>
        /// </summary>
        public DockWindow(MainForm frm)
        {
            InitializeComponent();
            this.mainForm = frm;
            Construct();
        }

        private void Construct()
        {
            if (!this.Modal)
            {
                this.Opacity = 0;
                ShowInTaskbar = false;
                controlContainer.Form = this;
                controlContainer.Resize += new EventHandler(controlContainer_Resize);
                controlContainer.Activated += new EventHandler(controlContainer_Activated);
                controlContainer.Deactivate += new EventHandler(controlContainer_Deactivate);
            }
        }
        #endregion


        #region Variables
        private bool isLoaded = false;
        private bool wasDocked = false;
        private bool hideOnClose = false;
        private bool allowDock = true;
        private bool allowSave = true;
        private bool allowUnDock = true;
        private bool allowClose = true;
        private bool allowSplit = true;
        private bool layoutFinished = false;
        private bool showFormAtOnLoad = true;

        private DockContainerType dockType = DockContainerType.None;
        private DockContainer lastHost = null;
        internal DockContainer dragTarget = null;

        private DockPanel controlContainer = new DockPanel();
        #endregion

        #region Properties

        /// <summary>
        /// MainForm that the dock window is attached to ..
        /// </summary>
        public MainForm MainForm
        {
            get
            {
                return this.mainForm;
            }
        }

        /// <summary>
        /// Gets or sets the dock element type.
        /// </summary>
        [Category("DockDotNET"), Description("Gets or sets the dock element type.")]
        public DockContainerType DockType
        {
            get { return dockType; }
            set { dockType = value; }
        }

        /// <summary>
        /// Allow this window to be undocked.
        /// </summary>
        [Category("DockDotNET"), Description("Allow this window to be undocked.")]
        public bool AllowUnDock
        {
            get { return allowUnDock; }
            set { allowUnDock = value; }
        }

        /// <summary>
        /// Allow this window to be closed.
        /// </summary>
        [Category("DockDotNET"), Description("Allow this window to be closed.")]
        public bool AllowClose
        {
            get { return allowClose; }
            set { allowClose = value; }
        }

        /// <summary>
        /// Allow other windows or containers to dock into this one.
        /// </summary>
        [Category("DockDotNET"), Description("Allow other windows or containers to dock into this one.")]
        public bool AllowDock
        {
            get { return allowDock; }
            set { allowDock = value; }
        }

        /// <summary>
        /// Allow other windows to split the control container.
        /// </summary>
        [Category("DockDotNET"), Description("Allow other windows to split the control container.")]
        public bool AllowSplit
        {
            get { return allowSplit; }
            set { allowSplit = value; }
        }

        /// <summary>
        /// Allow the framework to save and restore the window position.
        /// </summary>
        [Category("DockDotNET"), Description("Allow the framework to save and restore the window position.")]
        public bool AllowSave
        {
            get { return allowSave; }
            set { allowSave = value; }
        }

        /// <summary>
        /// Gets or sets a flag that prevents the window from closing.
        /// </summary>
        [Category("DockDotNET"), Description("Gets or sets a flag that prevents the window from closing.")]
        public bool HideOnClose
        {
            get { return hideOnClose; }
            set { hideOnClose = value; }
        }

        /// <summary>
        /// Gets the panel that is connected to this window.
        /// </summary>
        [Browsable(false)]
        public DockPanel ControlContainer
        {
            get { return controlContainer; }
            set { controlContainer = value; }
        }

        /// <summary>
        /// Gets the retrieved target of a drag operation.
        /// </summary>
        [Browsable(false)]
        internal DockContainer DragTarget
        {
            get { return dragTarget; }
        }

        /// <summary>
        /// Gets the container of the associated panel (null if not docked).
        /// </summary>
        [Browsable(false)]
        public DockContainer HostContainer
        {
            get
            {
                if (controlContainer.Parent is DockContainer)
                    return controlContainer.Parent as DockContainer;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the docking state of the window.
        /// </summary>
        [Browsable(false)]
        public bool IsDocked
        {
            get
            {
                Control c = controlContainer.Parent;

                if (c == this)
                    return false;

                if (!(c is DockContainer))
                    return false;

                DockContainer cont = c as DockContainer;

                if (cont.IsRootContainer & (cont.panList.Count == 1) & (cont.conList.Count == 0))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Gets the docking state after last close.
        /// </summary>
        [Browsable(false)]
        public bool WasDocked
        {
            get { return wasDocked; }
        }

        /// <summary>
        /// Gets the loaded flag.
        /// </summary>
        [Browsable(false)]
        public bool IsLoaded
        {
            get { return isLoaded; }
        }

        /// <summary>
        /// Overwrites the Focused property from the base class.
        /// </summary>
        public override bool Focused
        {
            get
            {
                if (!this.IsVisible)
                    return false;

                if (controlContainer.Parent is DockContainer)
                {
                    if ((controlContainer.Parent as DockContainer).ActivePanel == controlContainer)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the real visibility state of the window contents.
        /// Use this property instead of Visible, since it can not be overridden.
        /// </summary>
        [Browsable(false)]
        public bool IsVisible
        {
            get
            {
                if (this.Modal) return this.Visible;

                bool bVisible = false;

                if (controlContainer.TopLevelControl != null)
                    bVisible = controlContainer.TopLevelControl.Visible;

                return this.Visible || IsDocked || bVisible;
            }
            set
            {
                if (this.Modal) this.Visible = value;

                DockContainer host = controlContainer.Parent as DockContainer;

                if (!value && (host != null) && host.AutoHide)
                    host.StopAutoHide();

                if (value)
                {
                    if (lastHost != null)
                    {
                        if (!lastHost.IsDisposed)
                        {
                            lastHost.DockWindow(this, DockStyle.Fill);
                            return;
                        }

                        lastHost = null;
                    }

                    if (!this.DesignMode && layoutFinished)
                    {
                        CreateContainer();
                        LoadDockForm();
                    }
                }
                else if (hideOnClose)
                {
                    wasDocked = IsDocked;

                    if (wasDocked)
                    {
                        lastHost = host;
                        Release();
                    }
                    else
                        lastHost = null;

                    if (controlContainer.TopLevelControl != null)
                        controlContainer.TopLevelControl.Hide();
                }
                else
                {
                    //if (this.IsDocked)
                    //	this.HostContainer.ReleaseWindow(this);
                    if (this.IsDocked)
                        host.ReleaseWindow(this);

                    this.Close();
                    OnClosing(new CancelEventArgs());
                }
            }
        }

        /// <summary>
        /// This property is used by the DockContainer class while docking a form directly to a container,
        /// to prevent the form from being shown for a short time.
        /// </summary>
        [Browsable(false)]
        internal bool ShowFormAtOnLoad
        {
            get { return showFormAtOnLoad; }
            set { showFormAtOnLoad = value; }
        }
        #endregion

        #region Load and container management
        /// <summary>
        /// Calls the <see cref="CreateContainer"/> function, if not in design mode.
        /// Raises the Load event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (this.Modal)
            {
                base.OnLoad(e);
                return;
            }

            if (!this.IsDocked & !this.DesignMode)
            {
                if (dockType != DockContainerType.None)
                {
                    this.Opacity = 0;

                    CreateContainer();
                    LoadDockForm();
                }
                else
                {
                    this.Opacity = 1;
                }
            }

            base.OnLoad(e);
        }

        private void LoadDockForm()
        {
            DockForm form = new DockForm();
            CopyToDockForm(form);
            if (showFormAtOnLoad)
                form.Show();
        }

        internal void CopyToDockForm(DockForm form)
        {
            form.Location = this.Location;
            form.Size = this.Size;
            form.RootContainer.Controls.Add(controlContainer);
            form.RootContainer.DockType = this.DockType;

            CopyPropToDockForm(form);
        }

        internal void CopyPropToDockForm(DockForm form)
        {
            form.AllowDock = this.allowDock;
            form.FormBorderStyle = this.FormBorderStyle;
            form.Icon = this.Icon;
            form.Text = this.Text;
        }

        /// <summary>
        /// Creates the control container and fills it with the controls contained by the window.
        /// The controls will behave like at design time.
        /// </summary>
        public void CreateContainer()
        {
            if (this.DesignMode)
                return;

            DockManager.RegisterWindow(this);

            controlContainer.Dock = DockStyle.Fill;

            int max = this.Controls.Count;
            int off = 0;
            Control c;

            if (!this.Controls.Contains(controlContainer))
            {
                controlContainer.Dock = DockStyle.None;
                this.Controls.Add(controlContainer);
                controlContainer.Location = Point.Empty;
                controlContainer.Size = this.ClientSize;

                Size diffSize = Size.Subtract(this.Size, this.ClientSize);

                if (!this.MinimumSize.IsEmpty)
                    controlContainer.MinFormSize = Size.Subtract(this.MinimumSize, diffSize);

                if (!this.MaximumSize.IsEmpty)
                    controlContainer.MaxFormSize = Size.Subtract(this.MaximumSize, diffSize);
            }

            while (this.Controls.Count > off)
            {
                if (this.Controls[0] != controlContainer)
                {
                    c = this.Controls[off];
                    this.Controls.Remove(c);

                    if (c != null)
                        controlContainer.Controls.Add(c);
                }
                else
                    off = 1;
            }

            controlContainer.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Releases the window from its host container.
        /// </summary>
        internal void Release()
        {
            /*if (this.HostContainer != null)
                HostContainer.ReleaseWindow(this);*/

            DockContainer host = controlContainer.Parent as DockContainer;

            if (host != null)
                host.ReleaseWindow(this);
        }

        /// <summary>
        /// Raises the <see cref="Control.TextChanged"/> event.
        /// Used also to set the text of the host container.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (this.HostContainer != null)
            {
                this.HostContainer.SetWindowText();
            }
        }

        /// <summary>
        /// Overrides the OnLayout event handler.
        /// </summary>
        /// <param name="levent">A LayoutEventArgs object.</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            layoutFinished = true;
            base.OnLayout(levent);
        }

        private void controlContainer_Resize(object sender, EventArgs e)
        {
            this.OnResize(e);
        }

        private void controlContainer_Activated(object sender, EventArgs e)
        {
            this.OnActivated(e);
        }

        private void controlContainer_Deactivate(object sender, EventArgs e)
        {
            this.OnDeactivate(e);
        }
        #endregion

        #region Key mapping
        /// <summary>
        /// Invokes the KeyDown event.
        /// Is used as interface to the key routines of the <see cref="DockManager"/>.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        public void InvokeKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        /// <summary>
        /// Invokes the KeyUp event.
        /// Is used as interface to the key routines of the <see cref="DockManager"/>.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        public void InvokeKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            OnKeyUp(e);
        }
        #endregion

        #region Show and close
        /// <summary>
        /// Implements the VisibleChanged event of the base class.
        /// Releases the window if showed.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (this.Modal)
                return;

            if (this.Visible && !this.DesignMode && (dockType != DockContainerType.None))
                this.Visible = false;
        }

        /// <summary>
        /// Implements the Closing event of the base class.
        /// Adds the container back to the own control list, if docked and sets the <see cref="WasDocked"/> flag.
        /// </summary>
        /// <param name="e">A <see cref="CancelEventArgs"/> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if ((e.Cancel == true) | this.Modal)
                return;

            if (!this.DesignMode & this.IsVisible & (dockType != DockContainerType.None))
            {
                //this.IsVisible = false;   //Causes a StackFlowException as IsVisible wants to close the form.

                if (hideOnClose)
                    e.Cancel = true;
            }
            else
            {
                OnClosed(new EventArgs());
            }
        }

        /// <summary>
        /// Sets the focus to the <see cref="DockForm"/> or selects the <see cref="DockPanel"/> in its <see cref="DockContainer"/>.
        /// </summary>
        /// <returns></returns>
        public new bool Focus()
        {
            if (this.Modal)
                return base.Focus();

            if (this.IsDocked)
                controlContainer.SelectTab();
            else
                controlContainer.TopLevelControl.Focus();

            return this.Focused;
        }
        #endregion

        #region XML r/w
        /// <summary>
        /// Writes user specific data to the window save list.
        /// </summary>
        /// <param name="writer">The <see cref="XmlTextWriter"/> object that writes to the target stream.</param>
        public virtual void WriteXml(XmlTextWriter writer) { }

        /// <summary>
        /// Reads user specific data from the window save list.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> object that reads from the source stream.</param>
        public virtual void ReadXml(XmlReader reader) { }
        #endregion
    }
}
