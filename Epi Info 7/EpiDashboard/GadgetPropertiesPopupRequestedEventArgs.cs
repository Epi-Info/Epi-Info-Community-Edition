using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    public class GadgetPropertiesPopupRequestedEventArgs : EventArgs
    {
        public GadgetPropertiesPopupRequestedEventArgs(GadgetParameters parameters)
        {
            this.Parameters = parameters;
        }

        public GadgetParameters Parameters { get; private set; }
    }
}
