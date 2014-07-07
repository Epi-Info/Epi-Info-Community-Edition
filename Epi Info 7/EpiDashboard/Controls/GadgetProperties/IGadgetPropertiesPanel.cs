using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EpiDashboard.Controls.GadgetProperties
{
    public interface IGadgetPropertiesPanel
    {
        RowFilterControl RowFilterControl { get; }
        DataFilters DataFilters { get; }
        DashboardHelper DashboardHelper { get; }
        IGadget Gadget { get; }

        event EventHandler Cancelled;
        event EventHandler ChangesAccepted;
    }
}
