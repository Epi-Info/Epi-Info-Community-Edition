using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard.Gadgets.Reporting
{
    public interface IReportingGadget
    {
        void Select();
        void Deselect();
    }
}
