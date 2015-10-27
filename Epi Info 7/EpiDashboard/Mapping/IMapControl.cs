using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard.Mapping
{
    public interface IMapControl
    {
        event TimeVariableSetHandler TimeVariableSet;
        event EventHandler MapDataChanged;
        void OnRecordSelected(int id);
        void OnDateRangeDefined(DateTime start, DateTime end, List<KeyValuePair<DateTime, int>> intervalCounts);
    }
    

}
