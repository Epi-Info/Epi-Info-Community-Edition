using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// Interface for a single row filter condition, part of a larger SQL SELECT query's WHERE clause
    /// </summary>
    public interface IRowFilterCondition
    {
        QueryParameter Parameter { get; set; }
        object Value { get; set; }
        string ColumnName { get; set; }
        string Sql { get; set; }
        string ParameterName { get; set; }
        string Description { get; set; }
        XmlNode Serialize(XmlDocument doc);
        void CreateFromXml(XmlElement element);
    }
}
