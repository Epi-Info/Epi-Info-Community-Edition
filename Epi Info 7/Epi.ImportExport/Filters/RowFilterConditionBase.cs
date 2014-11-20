using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        #region Members
        private string _parameterName = String.Empty;
        #endregion // Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("Passing in SQL directly should no longer be done. Use other constructor that builds the SQL from the inputs.", false)]
        public RowFilterConditionBase(string sql, string columnName, string paramName, object value)
        {
            this.Sql = sql;
            this.ColumnName = columnName;
            this.ParameterName = paramName;
            this.Value = value;
            ValidateCondition();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnName">The name of the column that this filter applies to; appears on the left side of the condition</param>
        /// <param name="conditionOperator">The operator that will be used to separate the column name from the value (e.g. "equals to")</param>
        /// <param name="paramName">The name of the query parameter</param>
        /// <param name="value">The raw value that will appear on the right side of the condition</param>
        public RowFilterConditionBase(ImportExport.Filters.ConditionOperators conditionOperator, string columnName, string paramName, object value)
        {
            Contract.Requires(!String.IsNullOrEmpty(columnName));
            Contract.Requires(!String.IsNullOrEmpty(paramName));
            Contract.Requires(paramName.StartsWith("@"));
            Contract.Requires(value != null);

            this.ColumnName = columnName;
            this.ParameterName = paramName;
            this.Value = value;
            this.ConditionOperator = conditionOperator;
            ValidateCondition();
            Construct();
            BuildSql();
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
        public string ColumnName { get; protected set; }

        /// <summary>
        /// Gets/sets the sql
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// The operator to use between the column name and the parameter
        /// </summary>
        public ImportExport.Filters.ConditionOperators ConditionOperator { get; protected set; }

        /// <summary>
        /// Gets/sets the parameter name
        /// </summary>
        public string ParameterName
        {
            get
            {
                return this._parameterName;
            }
            set
            {
                Contract.Requires(value.StartsWith("@"));

                this._parameterName = value;

                BuildSql(); // rebuild since param name is now different
            }
        }

        /// <summary>
        /// Gets/sets the value
        /// </summary>
        public object Value { get; protected set; }

        /// <summary>
        /// Gets/sets the query parameter
        /// </summary>
        public QueryParameter Parameter { get; protected set; }
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

        public virtual void BuildSql()
        {
            Parameter = new QueryParameter(ParameterName, System.Data.DbType.String, Value);
            Sql = "[" + ColumnName + "] " + GetOperatorString(ConditionOperator) + " " + ParameterName;
        }

        protected string GetOperatorString(Epi.ImportExport.Filters.ConditionOperators conditionOperator)
        {
            // post
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

            string op = String.Empty;

            switch (conditionOperator)
            {
                case Filters.ConditionOperators.EqualTo:
                    return " = ";
                case Filters.ConditionOperators.GreaterThan:
                    return " > ";
                case Filters.ConditionOperators.GreaterThanOrEqualTo:
                    return " >= ";
                case Filters.ConditionOperators.LessThan:
                    return " < ";
                case Filters.ConditionOperators.LessThenOrEqualTo:
                    return " <= ";
                case Filters.ConditionOperators.NotEqualTo:
                    return " <> ";
                default:
                    throw new InvalidOperationException("Invalid operator.");
            }
        }

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
