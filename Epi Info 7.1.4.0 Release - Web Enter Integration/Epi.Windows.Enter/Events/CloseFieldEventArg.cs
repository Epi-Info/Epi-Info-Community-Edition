using System;
using System.Collections.Generic;
using System.Text;
using Epi.Fields;

namespace Epi.Windows.Enter.PresentationLogic
{
    public class CloseFieldEventArg : EventArgs
    {
        private Field field;
        private bool tab;
        private string reasonForClosing;

        public CloseFieldEventArg(Field field, bool tab, string reasonForClosing)
        {
            this.field = field;
            this.tab = tab;
            this.reasonForClosing = reasonForClosing;
        }

        public Field Field
        {
            get { return field; }
        }

        /// <summary>
        /// Gets whether or not the close field event should force a tab on the last field on a page
        /// </summary>
        public bool Tab
        {
            get { return tab; }
        }

        /// <summary>
        /// Gets a reason for closing so the event handler can decide whether to execute certain actions
        /// </summary>
        public string ReasonForClosing
        {
            get { return reasonForClosing; }
        }
    }
}
