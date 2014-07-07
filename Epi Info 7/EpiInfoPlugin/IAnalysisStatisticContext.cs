using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IAnalysisStatisticContext
    {
        Dictionary<string, string> SetProperties { get; }
        Dictionary<string, string> InputVariableList { get; }
        System.Data.DataColumnCollection Columns { get; }
        List<System.Data.DataRow> GetDataRows(List<string> pVariableList);
        void Display(Dictionary<string, string> pDisplayArgs);
        bool OutTable(System.Data.DataTable pDataTable);
        Dictionary<string,IVariable> EpiViewVariableList { get; }
    }
}
