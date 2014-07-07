using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Windows.Enter.PresentationLogic
{
    public class GoToRecordEventArgs : EventArgs
    {
        private string mRecordString;

        public GoToRecordEventArgs(string pRecordString)
        {
            this.mRecordString = pRecordString;
        }

        public string RecordString
        {
            get { return this.mRecordString; }
        }
    }
}
