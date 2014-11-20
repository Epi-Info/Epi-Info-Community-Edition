using System;
using System.Xml;
using Epi.Data;
using Epi.ImportExport.Filters;

namespace Epi.ImportExport
{
    /// <summary>
    /// Interface for a single row filter condition, part of a larger SQL SELECT query's WHERE clause
    /// </summary>
    public interface IRowFilterCondition
    {
        QueryParameter Parameter { get; }
        object Value { get; }
        string ColumnName { get; }
        string Sql { get; set; }
        string ParameterName { get; set; }
        string Description { get; set; }
        ConditionOperators ConditionOperator { get; }
        XmlNode Serialize(XmlDocument doc);
        void CreateFromXml(XmlElement element);
        void BuildSql();
    }
}
