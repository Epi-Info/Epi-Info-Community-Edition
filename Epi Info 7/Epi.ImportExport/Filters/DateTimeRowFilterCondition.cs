using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Epi.Data;

namespace Epi.ImportExport
{
    /// <summary>
    /// Row filter condition for date conditions, e.g. DOB = 1/1/1990 23:00:00
    /// </summary>
    public class DateTimeRowFilterCondition : RowFilterConditionBase
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public DateTimeRowFilterCondition(string sql, string columnName, string paramName, object value) :
            base(sql, columnName, paramName, value)
        {
        }
        #endregion // Constructors

        #region Public Methods
        /// <summary>
        /// Creates the row filter condition from an XML element
        /// </summary>
        /// <param name="element">The element from which to create the condition</param>
        public override void CreateFromXml(XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "description":
                        this.Description = child.InnerText;
                        break;
                    case "sql":
                        this.Sql = child.InnerText;
                        break;
                    case "parametername":
                        this.ParameterName = child.InnerText;
                        break;
                    case "value":
                        this.Value = child.InnerText;
                        break;
                }
            }

            // try to parse the value into date time; it will always be string otherwise
            this.Value = DateTime.Parse(this.Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);

            ValidateCondition();
            Construct();
        }

        /// <summary>
        /// Generates Xml representation of this filter condition
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            string xmlString =
            "<description>" + this.Description + "</description>" +
            "<sql>" + this.Sql + "</sql>" +
            "<value>" + DateTime.Parse(this.Value.ToString(), System.Globalization.CultureInfo.InvariantCulture).ToString("s") + "</value>" +
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
        #endregion // Public Methods

        #region Protected Methods
        /// <summary>
        /// Validates the condition
        /// </summary>
        protected override void ValidateCondition()
        {
            if (
                (Value == null) ||                
                !(Value is DateTime) ||
                (string.IsNullOrEmpty(ParameterName.Trim())) ||
                !(ParameterName.Contains("@"))
                )
            {
                throw new InvalidInputException();
            }
        }

        /// <summary>
        /// Constructs the object
        /// </summary>
        protected override void Construct()
        {
            Parameter = new QueryParameter(ParameterName, System.Data.DbType.DateTime2, Convert.ToDateTime(Value));
        }
        #endregion // Protected Methods
    }
}
