using System;
using System.Collections.Generic;
using System.Text;
using Epi.Fields;

namespace Epi.Windows.Enter.PresentationLogic
{
    public class ClickFieldEventArg : EventArgs
    {
        #region Private Members
        private Field field;
        #endregion // Private Members

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pField">Field</param>
        public ClickFieldEventArg(Field pField)
        {
            this.field = pField;
        }

        /// <summary>
        /// Gets/sets the Field property
        /// </summary>
        public Field Field
        {
            get 
            { 
                return this.field; 
            }
        }
    }
}
