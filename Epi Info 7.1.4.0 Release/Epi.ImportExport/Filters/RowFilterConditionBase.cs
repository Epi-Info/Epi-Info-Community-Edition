using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// Base class for row filter conditions
    /// </summary>
    public abstract class RowFilterConditionBase : IRowFilterCondition
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public RowFilterConditionBase()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RowFilterConditionBase(string sql, string columnName, string paramName, object value)
        {
            this.Sql = sql;
            this.ColumnName = columnName;
            this.ParameterName = paramName;
            this.Value = value;
            ValidateCondition();
            Construct();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the description for this filter condition.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets/sets the column name for which this condition applies
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets/sets the sql
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Gets/sets the parameter name
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets/sets the value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets/sets the query parameter
        /// </summary>
        public QueryParameter Parameter { get; set; }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Generates Xml representation of this filter condition
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public virtual XmlNode Serialize(XmlDocument doc)
        {
            string xmlString =
            "<description>" + this.Description + "</description>" +
            "<sql>" + this.Sql.ToString().Replace("<", "&lt;").Replace(">", "&gt;") + "</sql>" +
            "<value>" + this.Value + "</value>" +
            "<columnName>" + this.ColumnName.Replace("<", "&lt;").Replace(">", "&gt;") + "</columnName>" +
            "<parameterName>" + this.ParameterName + "</parameterName>";

            xmlString += "<queryParameter>";

            xmlString += "<dbType>" + ((int)this.Parameter.DbType).ToString() + "</dbType>";
            xmlString += "<name>" + this.Parameter.ParameterName + "</name>";
            xmlString += "<value>" + this.Parameter.Value + "</value>";

            xmlString += "</queryParameter>";

            System.Xml.XmlElement element = doc.CreateElement("rowFilterCondition");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute type = doc.CreateAttribute("filterType");
            type.Value = this.GetType().ToString(); //"Epi.ImportExport.RowFilterConditionBase";
            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the row filter condition from an XML element
        /// </summary>
        /// <param name="element">The element from which to create the condition</param>
        public virtual void CreateFromXml(XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "description":
                        this.Description = child.InnerText;
                        break;
                    case "columnname":
                        this.ColumnName = child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<");
                        break;
                    case "sql":
                        this.Sql = child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<");
                        break;
                    case "parametername":
                        this.ParameterName = child.InnerText;
                        break;
                    case "value":
                        this.Value = child.InnerText;
                        break;
                }
            }

            ValidateCondition();
            Construct();
        }
        #endregion // Public Methods

        #region Protected Methods

        /// <summary>
        /// Constructs the object
        /// </summary>
        protected abstract void Construct();

        /// <summary>
        /// Validates the inputs
        /// </summary>
        protected abstract void ValidateCondition();

        #endregion // Protected Methods
    }
}
