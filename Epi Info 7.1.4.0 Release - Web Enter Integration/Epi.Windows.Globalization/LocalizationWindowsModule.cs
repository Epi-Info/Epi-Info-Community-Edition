using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using Epi;
using Epi.Windows;
using Epi.Diagnostics;
using Epi.Windows.Globalization.Forms;

namespace Epi.Windows.Globalization
{
    /// <summary>
    /// LocalizationWindowsModule()
    /// </summary>
    public class LocalizationWindowsModule : ModuleBase//, IWindowsModule
    {
        #region Private Attributes
        private LocalizationManager form = null;
        private Container container = null;
        #endregion Pricate Attributes

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public LocalizationWindowsModule()
        {
            //container = new ModuleContainer(this);
        }
        #endregion Constructors

        #region Protected Properties

        /// <summary>
        /// Module's Epi Info module type 
        /// </summary>
        protected override string ModuleName
        {
            get
            {
                return "Localization";
            }
        }

        #endregion Protected Properties

        #region Public Methods

        /// <summary>
        /// Dispose of container and components
        /// </summary>
		public override void Dispose()
        {
            // trash container and any components
            this.container.Dispose();

            form = null;

            // base class will finish the job
            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Attach the current project with module loading
        /// </summary>
        protected override void Load(IModuleManager moduleManager, ICommandLine commandLine)
        {
            base.Load(moduleManager, commandLine);

            try
            {
                if (form == null)
                {
                    form = new LocalizationManager();
                    container.Add(form);
                    form.Closed += new EventHandler(MainForm_Closed);
                    form.Disposed += new EventHandler(MainForm_Disposed);
                    form.Show();
                    form.Activate();

                    // assure handle creation
                    System.IntPtr handle = form.Handle;

                    // read the command line
                    if (commandLine != null)
                    {
                        string titleArgument = commandLine.GetArgument("title");
                        if (titleArgument != null)
                        {
                            form.Text = titleArgument;
                        }
                    }
                    base.OnLoaded();
                }
                else
                {
                    if (!form.IsDisposed)
                    {
                        form.Show();
                        if (form.WindowState == FormWindowState.Minimized)
                        {
                            form.WindowState = FormWindowState.Normal;
                        }
                        form.Activate();
                    }

                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Unload this module
        /// </summary>
		protected override void Unload()
        {
            if (form != null && !form.IsDisposed)
            {
                // form can cancel closing if necessary, module will only be unloaded if
                // the form's Closed event is fired
                form.Close();
            }
            else
            {
                this.Dispose();
            }
        }

        #endregion Protected Methods

        #region Event Handlers

        /// <summary>
        /// Handles the Disposed event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        public void MainForm_Disposed(object sender, EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Handles the Closed event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        public void MainForm_Closed(object sender, EventArgs e)
        {
            this.Dispose();
        }
        #endregion Event Handlers
    } // Class
} // Namespace