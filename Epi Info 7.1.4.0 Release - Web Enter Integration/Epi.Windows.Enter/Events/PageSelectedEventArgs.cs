using System;
using System.Collections.Generic;
using System.Text;

namespace Epi.Windows.Enter.PresentationLogic
{
    public class PageSelectedEventArgs : EventArgs
    {
        private Epi.Page page;

        public PageSelectedEventArgs(Epi.Page pPage)
        {
            this.page = pPage;
        }

        public Epi.Page Page
        {
            get { return this.page; }
        }
    }
}
