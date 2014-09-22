using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using Epi;
using Epi.Windows.MakeView.Forms;
using Epi.Windows;
using Epi.Diagnostics;

namespace Epi.Windows.MakeView
{
    /// <summary>
    /// Windows specific MakeView module.
    /// </summary>
    public class MakeViewWindowsModule : ModuleBase, IWindowsModule
    {
        #region Private Attributes
        private MakeViewMainForm form = null;
        private Container container = null;

        #endregion Private Attributes

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public MakeViewWindowsModule()
        {
            container = new ModuleContainer(this);
            //this.commandProcessor = new MakeViewCommandProcessor(this);
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
                return "MakeView";
            }
        }

        #endregion Protected Properties

        #region Public Methods

        /// <summary>
        /// Dispose of any components
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
                    form = new MakeViewMainForm(this);
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
        /// Unloads the module
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
        /// The Dispose event of the main form
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		public void MainForm_Disposed(object sender, EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// The Close event of the main form
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		public void MainForm_Closed(object sender, EventArgs e)
        {
            this.Dispose();
        }
        #endregion Event Handlers

        #region Public Properties


        #endregion  //Public Properties

    } // Class
} // Namespace
