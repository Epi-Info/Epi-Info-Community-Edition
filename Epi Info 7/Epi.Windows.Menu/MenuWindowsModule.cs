using System;
using System.ComponentModel;
using System.Windows.Forms;
using Epi.Windows;

namespace Epi.Windows.Menu
{
	/// <summary>
	/// Windows module for Menu
	/// </summary>
    public class MenuWindowsModule : ModuleBase, IWindowsModule
	{
        #region Private Attributes
       // private MenuMainForm form = null;
        private WindowMain form = null;
        private Container container = null;
        //private MenuCommandProcessor processor = null;
        private string mnuFile = String.Empty;
        #endregion Private Attributes

        #region Constructors
        /// <summary>
		/// Default constructor
		/// </summary>
        public MenuWindowsModule()
		{
            container = new ModuleContainer(this);
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
				return "Menu";
			}
        }
        #endregion Protected Properties

        #region Public Methods        
        #endregion Public Methods

        #region Public Properties
        #endregion Public Properties

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
                    // Create main form
                   // form = new MenuMainForm(this);
                    form = new WindowMain();
                    container.Add(form);
                    form.Closed += new EventHandler(MainForm_Closed);
                    form.Disposed += new EventHandler(MainForm_Disposed);

                    // Fill the contents of the form
                    //processor = new MenuCommandProcessor(this, form, Files.MnuFilePath);
                    try
                    {
                        // processor.FillForm();
                    }
                    catch (Exception ex)
                    {
                        // For now, show the exception and move on.
                        MsgBox.ShowException(ex);
                    }

                    // Display the form
                    form.Show();
                    form.Activate();

                    // assure handle creation
                    System.IntPtr handle = form.Handle;

                    //// read the command line
                    //if (commandLine != null)
                    //{
                    //    string titleArgument = commandLine.GetArgument("title");
                    //    if (titleArgument != null)
                    //    {
                    //        form.Text = titleArgument;
                    //    }
                    //}
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
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
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
        /// Handlers DISPOSE event of mainform.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		public void MainForm_Disposed(object sender, EventArgs e)
        {
            this.Dispose();
        }
        /// <summary>
        /// Handlers CLOSE event of main form
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		public void MainForm_Closed(object sender, EventArgs e)
        {
            this.Dispose();
        }
        #endregion Event Handlers

        #region Private Methods
        
        #endregion Private Methods


    }
}
