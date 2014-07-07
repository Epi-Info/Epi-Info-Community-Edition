using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace EpiInfo.Plugin
{
    public interface IDataSource
    {
        System.Data.IDataReader GetDataTableReader(string pSQL);
        object GetScalar(string pSQL);
        bool ExecuteSQL(string pSQL);
    }
}
