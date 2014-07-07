using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    public class FieldInfo
    {
        public string RelationName;
        public string FieldName;	//source table field name
        public string FieldAlias;	//destination table field name
        public string Aggregate;
    }
}
