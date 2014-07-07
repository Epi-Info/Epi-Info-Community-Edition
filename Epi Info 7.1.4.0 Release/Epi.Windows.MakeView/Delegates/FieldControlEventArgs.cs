using System;
using Epi.Windows.Controls;

namespace Epi.Windows.MakeView
{
    /// <summary>
    /// The Event Argument Handler for field controls
    /// </summary>
	public class FieldControlEventArgs : EventArgs
    {
        private IFieldControl control;
        /// <summary>
        /// The event handler
        /// </summary>
        /// <param name="control">A field control</param>
        public FieldControlEventArgs(IFieldControl control)
        {
            this.control = control;
        }

        /// <summary>
        /// A field control
        /// </summary>
		public IFieldControl Control
        {
            get
            {
                return this.control;
            }
        }
    }
}
