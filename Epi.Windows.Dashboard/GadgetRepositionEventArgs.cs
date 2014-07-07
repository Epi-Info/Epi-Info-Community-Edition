using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.WPF.Dashboard
{
    public class GadgetRepositionEventArgs : EventArgs
    {
        public enum GadgetRepositionType
        {
            Top = 1,
            Left = 2,
            Bottom = 3,
            Right = 4,
            Center = 5
        }

        private GadgetRepositionType mRepositionType;

        public GadgetRepositionEventArgs(GadgetRepositionType pRepositionType)
        {
            this.mRepositionType = pRepositionType;
        }

        public GadgetRepositionType RepositionType
        {
            get { return this.mRepositionType; }
        }
    }
}
