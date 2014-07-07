using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard.StatCalc
{
    public interface IStatCalcControl
    {
        int PreferredUIHeight { get; }
        int PreferredUIWidth { get; }
        void HideCloseIcon();
    }
}
