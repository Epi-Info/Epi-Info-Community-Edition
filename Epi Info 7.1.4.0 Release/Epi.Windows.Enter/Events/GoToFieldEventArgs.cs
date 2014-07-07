using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Windows.Enter.PresentationLogic
{
    public class GoToFieldEventArgs : EventArgs
    {
        private Epi.Fields.Field mField;

        public GoToFieldEventArgs(Epi.Fields.Field pField)
        {
            this.mField = pField;
        }

        public Epi.Fields.Field Field
        {
            get { return this.mField; }
        }
    }
}
