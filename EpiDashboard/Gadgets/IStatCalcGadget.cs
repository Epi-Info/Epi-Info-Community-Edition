using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard.Gadgets
{
    public interface IStatCalcControl
    {
        int PreferredUIHeight { get; }
        int PreferredUIWidth { get; }

        bool IsHostedInOwnWindow { get; set; }
    }
}
