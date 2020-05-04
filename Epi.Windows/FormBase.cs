using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Epi.Windows
{
	/// <summary>
	/// Base class for all forms in Epi Info
	/// </summary>
    public partial class FormBase : Form
	{

		#region Constructors

		/// <summary>
		/// Default constructor for the form.
		/// </summary>
		public FormBase()
		{
			InitializeComponent();
		}

		#endregion

		#region Public Methods
        
        /// <summary>
        /// Returns a boolean value indicating if the key state is toggled on or off
        /// </summary>
        /// <param name="keyValue">The integer value of the key in question</param>
        /// <returns>Returns true if the key is toggled and false if the key is not toggled</returns>
        public bool IsKeyToggled(int keyValue)
        {
            bool RetVal = false;
            int state = GetLowOrder(GetKeyState(keyValue));
            if (state == 1)
            {
                RetVal = true;
            }

            return RetVal;
        }

        private int GetLowOrder(int param)
        {
            return (param & 0x01);
        }

        #endregion Public Methods

		#region Event Handlers

		/// <summary>
		/// Handles the load event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void Form_Load(object sender, System.EventArgs e)
		{
		}

        /// <summary>
        /// Handles the Click event of the Help button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		protected virtual void btnHelp_Click(object sender, System.EventArgs e)
		{
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/support/userguide.html");
        }

        /// <summary>
        /// Handles the HelpRequest event of the Form eg. F1 key
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void FormBase_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            btnHelp_Click(sender, hlpevent);
        }

        /// <summary>
        /// Handles the Click event of features not yet implemented
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		protected virtual void OnFeatureNotImplemented(object sender, System.EventArgs e)
		{
			DisplayFeatureNotImplementedMessage();
		}
		#endregion

		#region Protected Methods
	
        /// <summary>
        /// Displays the "Feature not implemented" message
        /// </summary>
		protected void DisplayFeatureNotImplementedMessage()
		{
			MsgBox.ShowInformation(SharedStrings.FEATURE_NOT_YET_IMPLEMENTED);
		}

        /// <summary>
        /// Shortcut for Translation routine
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [Obsolete("Translation has been eliminated.",true)]
        protected string T(string str)
        {
            throw new NotSupportedException();
            //return Localization.T(str);
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("AssertDesignMode method is deprecated", false)]
        protected void AssertDesignMode()
        {
        }

		#endregion Private Methods		

        #region Private Methods
        
        /// <summary>
        /// Determines the state of a keyboard key
        /// </summary>
        /// <param name="VirtKey">The integer value of the key in question</param>
        /// <returns>Returns 1 if the key state is on and 0 if the key state is off</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetKeyState(int VirtKey);

        #endregion
    }
}