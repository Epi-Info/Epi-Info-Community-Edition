using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Data;


using Epi.Fields;

namespace Epi.Windows.Enter.PresentationLogic
{
    public partial class GuiMediator
    {
        /// <summary>
        /// Handles the Close Forms Requested event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void CloseFormEventHandler(object sender, EventArgs e)
        {
            this.EnterCheckCodeEngine.Close();
            this.viewExplorer.Close();
            this.canvas.Close();
            this.UnSubscribe();
        }
        
        ///<summary>
        /// Displays the format for masked input text boxes
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        private void mediator_DisplayFormat(object sender, EventArgs e)
        {
            mainForm.UpdateStatus((string)sender, false);
            
            /// call to this overload caused error after click {[n'] of n} then multiline textbox click
            /// mainForm.UpdateStatus((string)sender, false, false); 
            /// 
        }
    }
}
