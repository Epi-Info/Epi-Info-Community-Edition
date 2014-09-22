using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
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

    public class GadgetAnchorSetEventArgs : EventArgs
    {
        public enum GadgetAnchorType
        {
            Top = 1,
            Left = 2,
            Bottom = 3,
            Right = 4
        }

        private GadgetAnchorType mAnchorType;
        private Guid mGuid;

        public GadgetAnchorSetEventArgs(GadgetAnchorType pAnchorType, Guid pGuid)
        {
            this.mAnchorType = pAnchorType;
            this.mGuid = pGuid;
        }

        public GadgetAnchorType AnchorType
        {
            get { return this.mAnchorType; }
        }

        public Guid Guid
        {
            get { return this.mGuid; }
        }
    }
}
