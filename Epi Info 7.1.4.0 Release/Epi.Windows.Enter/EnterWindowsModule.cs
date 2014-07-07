using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;
using Epi;
using Epi.Enter;
using Epi.Windows;
using Epi.Diagnostics;

namespace Epi.Windows.Enter
{
    /// <summary>
    /// The Windows Module for Enter
    /// </summary>
    public class EnterWindowsModule : IWindowsModule, IProjectHost
    {
        #region Private Attributes
        private EnterMainForm form = null;
        private Container container = null;
        private IModuleManager moduleManager;
        private string ProjectFilePath = null;
        private string SelectedView = null;
        #endregion Private Attributes

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public EnterWindowsModule()
        {
            container = new ModuleContainer(this);
        }
        #endregion Constructors    
      
        #region Protected Properties

        /// <summary>
        /// Module's Epi Info module type 
        /// </summary>
        public string ModuleName
        {
            get
            {
                return "Enter";
            }
        }

        #endregion Protected Properties             

        #region Public Methods

        /// <summary>
        /// Gets the service for a specified type                
        /// </summary>
        /// <returns>An object that represents a service for the specified type </returns>
        [System.Diagnostics.DebuggerStepThrough()]       
        public object GetService(Type serviceType)
        {
            if (serviceType == this.GetType())
            {
                return this;
            }
            return null;
                /*
            else if (serviceType == typeof(EnterCommandProcessor)  || serviceType == typeof(ICommandProcessor))
            {
                return this.Processor;
            }
            else
            {
                return base.GetService(serviceType);
            }*/
        }

        /// <summary>
        /// Disposes of the module's container and components
        /// </summary>
		public void Dispose()
        {
            // trash container and any components
            if (this.container != null)
            {
                this.container.Dispose();
            }

            form = null;

            // base class will finish the job
            //base.Dispose();
        }

        #endregion Public Methods

        #region Private Methods
        /*
        private bool MsgResponse(EpiMessages msg, MessageType msgtype)
        {
            MessageBoxButtons buttons;
            switch (msgtype)
            {
                case MessageType.OkOnly:
                    buttons = MessageBoxButtons.OK;
                    MessageBox.Show(msg.Message, msg.Caption, buttons);
                    return true;
                case MessageType.YesNo:
                    buttons = MessageBoxButtons.YesNo;
                    DialogResult result;
                    result = MessageBox.Show(msg.Message, msg.Caption, buttons);
                    if (result == DialogResult.Yes)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }              
                default:
                    return false;
            }
        }*/

        #endregion  //Private Methods

        #region Protected Methods

        /// <summary>
        /// Attach the current project with module loading
        /// </summary>
        public void Load(IModuleManager moduleManager, ICommandLine commandLine)
        {
            //base.Load(moduleManager, commandLine);
            this.moduleManager = moduleManager;
            try
            {
                if (form == null)
                {
                    form = new EnterMainForm(this);
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

                        if (commandLine.GetArgument("project") != null)
                        {
                            Project p = new Project(commandLine.GetArgument("project"));
                            form.FireOpenViewEvent(p.Views[commandLine.GetArgument("view")]);
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
                    //base.OnLoaded();
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

            //Processor.CommunicateUI += new CommunicateUIEventHandler(MsgResponse);
        }

        /// <summary>
        /// Unloads the form
        /// </summary>
		public void Unload()
        {

            
            
            if (form != null && !form.IsDisposed)
            {
                // form can cancel closing if necessary, module will only be unloaded if
                // the form's Closed event is fired
                form.Close();//
            }
            else
            {
                this.Dispose();
            }/**/
        }

        #endregion Protected Methods

        #region Event Handlers

        /// <summary>
        /// Disposes the module once the main form is disposed of
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        public void MainForm_Disposed(object sender, EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes the module once the main form is closed
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        public void MainForm_Closed(object sender, EventArgs e)
        {
            this.Dispose();
        }
        #endregion Event Handlers

        public Project CurrentProject { get; set; }
        public IModuleManager ModuleManager
        {
            get { return this.moduleManager; }
        }

        public void SetAllReferencesToNull()
        {
            this.form = null;
            this.container = null;
            this.moduleManager = null;
            this.ProjectFilePath = null;
            this.SelectedView = null;
        }

    } 
} 