using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using Epi;
using Epi.Analysis;
using Epi.Windows;
using Epi.Diagnostics;

namespace Epi.Windows.Analysis
{

    /// <summary>
    /// The Windows module for Analysis
    /// </summary>
    public class AnalysisWindowsModule : AnalysisEngine, IWindowsModule, IProjectHost
    {
        #region Private Attributes
        private Forms.AnalysisMainForm form = null;
        private Container container = null;
        #endregion Pricate Attributes

        #region Private Methods

        private string DialogResponse(CommandProcessorResults results)
        {
            return string.Empty;
        }
        #endregion

        #region Constructors

        /// <summary>
		/// Default constructor
		/// </summary>
		public AnalysisWindowsModule()
		{
            container = new ModuleContainer(this);

            
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public AnalysisCommandProcessor CommandProcessor
        //{
        //    get
        //    {
        //        return this.engine.Processor;
        //    }
        //}
        #endregion Constructors

        #region Public Properties
        ///// <summary>
        ///// Current project per requirements for analysis
        ///// </summary>
        //public Project CurrentProject
        //{
        //    get
        //    {
        //        return engine.CurrentProject;
        //    }
        //    set
        //    {
        //        engine.CurrentProject = value;
        //        if (value != null)
        //        {
        //            Configuration config = Configuration.GetNewInstance();
        //            config.CurrentProjectFilePath = value.FilePath;
        //            Configuration.Save(config);
        //        }
        //    }
        //}
        #endregion Public Properties

        #region Protected Properties

		/// <summary>
		/// Module's Epi Info module type 
		/// </summary>
        protected override string ModuleName
		{
			get
			{
				return "Analysis";
			}
        }
     

        #endregion Protected Properties
        
        #region Public Methods
        /// <summary>
        /// <para> Returns an object that represents a service provided by the
        /// <see cref="T:System.ComponentModel.Component" /> or by its <see cref="T:System.ComponentModel.Container" />.
        /// </para>
        /// </summary>
        /// <param name="serviceType">A service provided by the <see cref="T:System.ComponentModel.Component" />. </param>
        /// <returns>
        /// <para> An <see cref="T:System.Object" /> that represents a service provided by the
        /// <see cref="T:System.ComponentModel.Component" />.
        /// </para>
        /// <para> This value is <see langword="null" /> if the <see cref="T:System.ComponentModel.Component" /> does not provide the
        /// specified service.</para>
        /// </returns>
        [System.Diagnostics.DebuggerStepThrough()]
        public override object GetService(Type serviceType)
        {
            if (serviceType == this.GetType())
            {
                return this;
            }
            else if (serviceType == typeof(IProjectHost))
            {
                return this;
            }
                /*
            else if (serviceType == typeof(Session))
            {
                return this.Processor.Session;
            }*/

            /*else if (serviceType == typeof(AnalysisCommandProcessor) || serviceType == typeof(ICommandProcessor))
            {
                return this.Processor;
            }*/
            else
            {
                return base.GetService(serviceType);
            }
        }

        /// <summary>
        /// Dispose of the container
        /// </summary>
		public override void Dispose()
        {
            // trash container and any components
            this.container.Dispose();
            //if (this.engine != null)
            //{
            //    this.engine.Dispose();
            //}

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

            // create a new instance of the analysis engine module. Module will be isolated from Windows
            // ApplicationManager GetService chain.
            // engine = new AnalysisEngine(moduleManager, null);
            
            base.Load(moduleManager, commandLine);

            try
            {
                 //Attach CurrentProject
                Configuration config = Configuration.GetNewInstance();
                string filePath = config.CurrentProjectFilePath;
                if (!string.IsNullOrEmpty(filePath))
                {

                    // Try to open the current project in the configuration file.
                    // If the project can't be opened, recover from the error and update the configuration file.
                    try
                    {
                        CurrentProject = new Project(filePath);
                    }
                    catch (Exception ex)
                    {
                        MsgBox.ShowException(ex);
                        config.CurrentProjectFilePath = string.Empty;
                        Configuration.Save(config);
                    }
                }

                if (form == null)
                {
                    base.OnLoaded();
                    form = new Forms.AnalysisMainForm(this);
                    container.Add(form);
                    form.Closed += new EventHandler(MainForm_Closed);
                    form.Disposed += new EventHandler(MainForm_Disposed);
                    form.Show();
                    if (!Util.IsEmpty(form))
                    {
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
                    }
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
                //zack 1/2/08
                //Processor.CommunicateUI += new CommunicateUIEventHandler(MsgResponse);

                // KKM4 Commented this out since HandleDialogEvent is never raised.
                // Processor.HandleDialogEvent += new DialogBoxUIEventHandler(DialogResponse); //dcs0 4/23/08
               
            }
            catch (Exception ex)
            {
                throw new GeneralException(SharedStrings.MODULE_NOT_LOADED + ": \n" + ex.Message, ex);
            }
            finally
            {
                if (Util.IsEmpty(form))
                {
                    this.Dispose();
                    Application.ExitThread();  
                }
            }
        }

        /// <summary>
        /// Unload - close the form
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
        /// Handles the Closed event for the MainForm
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