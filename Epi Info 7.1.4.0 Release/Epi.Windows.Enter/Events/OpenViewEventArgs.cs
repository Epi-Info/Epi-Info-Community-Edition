using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Windows.Enter.PresentationLogic
{
    public class OpenViewEventArgs : EventArgs
    {
        Epi.View mView;
        string recordNumber;

        public OpenViewEventArgs(Epi.View pView)
        {
            this.mView = pView;
            this.RecordNumber = string.Empty;
        }

        public OpenViewEventArgs(Epi.View pView, string record)
        {
            this.mView = pView;
            this.RecordNumber = record;
        }

        public Epi.View View
        {
            get { return this.mView; }
        }

        public string RecordNumber
        {
            get
            {
                return this.recordNumber;
            }
            set
            {
                this.recordNumber = value;
            }
        }
    }

}
