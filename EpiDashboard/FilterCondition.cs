using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using Epi;
using Epi.Core;

namespace EpiDashboard
{
    /// <summary>
    /// A class for representing a single data filter condition
    /// </summary>
    public class FilterCondition
    {
        #region Private Members
        private string condition;
        private string friendlyCondition;
        private string columnName;
        private string rawColumnName;
        private string columnType;
        private string operand;
        private string friendlyOperand;
        private string value;
        private string friendlyValue;
        private string highValue;
        private string friendlyHighValue;
        private string lowValue;
        private string friendlyLowValue;
        private bool isBetween;
        private bool isEnabled;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public FilterCondition()
        {
        }

        /// <summary>
        /// Constructor for a standard data filter
        /// </summary>
        /// <param name="friendlyCondition">The human-readable version of the condition</param>
        /// <param name="columnName">The name of the column that the condition is based upon</param>
        /// <param name="columnType">The type of the column</param>
        /// <param name="operand">The usable operand</param>
        /// <param name="friendlyOperand">The human-readable version of the operand</param>
        /// <param name="value">The usable value to filter on</param>
        /// <param name="friendlyValue">The human-readable value to filter on</param>
        public FilterCondition(string friendlyCondition, string columnName, string rawColumnName, string columnType, string operand, string friendlyOperand, string value, string friendlyValue)
        {
            this.isBetween = false;
            this.isEnabled = true;
            this.columnName = columnName;
            this.columnType = columnType;
            this.operand = operand;
            this.friendlyOperand = friendlyOperand;
            this.value = value;
            this.friendlyValue = friendlyValue;
            this.highValue = string.Empty;
            this.lowValue = string.Empty;
            this.friendlyCondition = friendlyCondition;
            this.rawColumnName = rawColumnName;

            SetupFilterCondition();
        }

        /// <summary>
        /// Constructor for a data filter using the between operator
        /// </summary>
        /// <param name="friendlyCondition">The human-readable version of the condition</param>
        /// <param name="columnName">The name of the column that the condition is based upon</param>
        /// <param name="columnType">The type of the column</param>
        /// <param name="operand">The usable operand</param>
        /// <param name="friendlyOperand">The human-readable version of the operand</param>
        /// <param name="lowValue">The usable lower value to filter on</param>
        /// <param name="highValue">The usable upper value to filter on</param>
        /// <param name="friendlyLowValue">The friendly lower value to filter on</param>
        /// <param name="friendlyHighValue">The friendly upper value to filter on</param>
        public FilterCondition(string friendlyCondition, string columnName, string rawColumnName, string columnType, string operand, string friendlyOperand, string lowValue, string highValue, string friendlyLowValue, string friendlyHighValue)
        {
            this.isBetween = true;
            this.isEnabled = true;
            this.columnName = columnName;
            this.columnType = columnType;
            this.operand = operand;
            this.friendlyOperand = friendlyOperand;
            this.value = string.Empty;
            this.friendlyValue = string.Empty;
            this.highValue = highValue;
            this.lowValue = lowValue;
            this.friendlyHighValue = friendlyHighValue;
            this.friendlyLowValue = friendlyLowValue;
            this.friendlyCondition = friendlyCondition;
            this.rawColumnName = rawColumnName;

            SetupFilterCondition();
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the full human-readable filter condition
        /// </summary>
        public string FriendlyCondition
        {
            get
            {
                return this.friendlyCondition;
            }
        }

        /// <summary>
        /// Gets the filter condition
        /// </summary>
        public string Condition
        {
            get
            {
                return this.condition;
            }
        }

        /// <summary>
        /// Gets the column name on which the filter is based
        /// </summary>
        public string ColumnName
        {
            get
            {
                return this.columnName;
            }
        }

        /// <summary>
        /// Gets the column type on which the filter is based
        /// </summary>
        public string ColumnType
        {
            get
            {
                return this.columnType;
            }
        }

        /// <summary>
        /// Gets the column name on which the filter is based, without brackets
        /// </summary>
        public string RawColumnName
        {
            get
            {
                return this.rawColumnName;
            }
        }

        /// <summary>
        /// Gets the operand for the filter condition
        /// </summary>
        public string Operand
        {
            get
            {
                return this.operand;
            }
        }

        /// <summary>
        /// Gets the display operand for the filter condition
        /// </summary>
        public string FriendlyOperand
        {
            get
            {
                return this.friendlyOperand;
            }
        }

        /// <summary>
        /// Gets the value for the filter condition
        /// </summary>
        public string Value
        {
            get
            {                
                return this.value;
            }
        }

        /// <summary>
        /// Gets the display value for the filter condition
        /// </summary>
        public string FriendlyValue
        {
            get
            {
                return this.friendlyValue;
            }
        }

        /// <summary>
        /// Gets the high value for the filter condition if the condition uses the 'is between' operator
        /// </summary>
        public string HighValue
        {
            get
            {
                return this.highValue;
            }
        }

        /// <summary>
        /// Gets the display high value for the filter condition if the condition uses the 'is between' operator
        /// </summary>
        public string FriendlyHighValue
        {
            get
            {
                return this.friendlyHighValue;
            }
        }

        /// <summary>
        /// Gets the display low value for the filter condition if the condition uses the 'is between' operator
        /// </summary>
        public string FriendlyLowValue
        {
            get
            {
                return this.friendlyLowValue;
            }
        }

        /// <summary>
        /// Gets the low value for the filter condition if the condition uses the 'is between' operator
        /// </summary>
        public string LowValue
        {
            get
            {
                return this.lowValue;
            }
        }

        /// <summary>
        /// Gets/sets whether or not the filter condition uses the 'is between' operator
        /// </summary>
        public bool IsBetween
        {
            get
            {
                return this.isBetween;
            }
            set
            {
                this.isBetween = value;
            }
        }

        /// <summary>
        /// Gets/sets whether or not the filter condition is active
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return this.isEnabled;
            }
            set
            {
                this.isEnabled = value;
            }
        }        
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// The string representation of this filter condition
        /// </summary>
        /// <returns>A string representing this filter condition</returns>
        public override string ToString()
        {
            return this.FriendlyCondition;
        }

        /// <summary>
        /// Generate an XML node based on the filter properties
        /// </summary>
        /// <param name="doc">The Xml document</param>
        /// <param name="joinType">The type of join</param>
        /// <returns>XmlNode</returns>
        public XmlNode Serialize(XmlDocument doc, string joinType)
        {
            string xmlFriendlyOperand = operand;
            string xmlFriendlyCondition = condition;

            switch (operand)
            {
                case ">":
                    xmlFriendlyOperand = "&gt;";
                    break;
                case ">=":
                    xmlFriendlyOperand = "&gt;=";
                    break;
                case "<":
                    xmlFriendlyOperand = "&lt;";
                    break;
                case "<=":
                    xmlFriendlyOperand = "&lt;=";
                    break;
                case "<>":
                    xmlFriendlyOperand = "&lt;&gt;";
                    break;
            }

            xmlFriendlyCondition = xmlFriendlyCondition.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");

            if (ColumnType.Equals("System.DateTime"))
            {
                if(IsBetween) 
                {
                    highValue = "#" + DateTime.Parse(highValue.Trim('#'), System.Globalization.CultureInfo.InvariantCulture).ToString("s") + "#";
                    lowValue = "#" + DateTime.Parse(lowValue.Trim('#'), System.Globalization.CultureInfo.InvariantCulture).ToString("s") + "#";

                    friendlyHighValue = DateTime.Parse(friendlyHighValue, System.Globalization.CultureInfo.CurrentCulture).ToString("d", System.Globalization.CultureInfo.InvariantCulture);
                    friendlyLowValue = DateTime.Parse(friendlyLowValue, System.Globalization.CultureInfo.CurrentCulture).ToString("d", System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (!String.IsNullOrEmpty(value.Trim()))
                {
                    value = "#" + DateTime.Parse(value.Trim('#')).ToString("s") + "#";

                    friendlyValue = DateTime.Parse(friendlyValue, System.Globalization.CultureInfo.CurrentCulture).ToString("d", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            
            string xmlString =
            "<condition>" + xmlFriendlyCondition + "</condition>" +
            "<friendlyCondition>" + friendlyCondition.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</friendlyCondition>" +
            "<columnName>" + columnName + "</columnName>" +
            "<columnType>" + columnType + "</columnType>" +
            "<rawColumnName>" + rawColumnName + "</rawColumnName>" +
            "<operand>" + xmlFriendlyOperand + "</operand>" +
            "<friendlyOperand>" + friendlyOperand.Replace("<", "&lt;").Replace(">", "&gt;") + "</friendlyOperand>" +
            "<value>" + value.Replace("&", "&amp;").Replace("<", "&lt;") + "</value>" +
            "<friendlyValue>" + friendlyValue.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;") + "</friendlyValue>" +
            "<highValue>" + highValue + "</highValue>" +
            "<friendlyHighValue>" + friendlyHighValue + "</friendlyHighValue>" +
            "<lowValue>" + lowValue + "</lowValue>" +
            "<friendlyLowValue>" + friendlyLowValue + "</friendlyLowValue>" +
            "<isBetween>" + isBetween.ToString().ToLower() + "</isBetween>" +
            "<isEnabled>" + isEnabled.ToString().ToLower() + "</isEnabled>" +
            "<joinType>" + joinType.ToString() + "</joinType>";

            System.Xml.XmlElement element = doc.CreateElement("filterCondition");
            element.InnerXml = xmlString;

            return element;
        }

        /// <summary>
        /// Create a filter condition object using XML data
        /// </summary>
        public void CreateFromXml(XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("condition"))
                {
                    this.condition = child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
                }
                else if (child.Name.Equals("friendlyCondition"))
                {
                    this.friendlyCondition = child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
                }
                else if (child.Name.Equals("columnName"))
                {
                    this.columnName = child.InnerText;
                }
                else if (child.Name.Equals("columnType"))
                {
                    this.columnType = child.InnerText;
                }
                else if (child.Name.Equals("rawColumnName"))
                {
                    this.rawColumnName = child.InnerText;
                }
                else if (child.Name.Equals("operand"))
                {
                    this.operand = child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<");
                }
                else if (child.Name.Equals("friendlyOperand"))
                {
                    this.friendlyOperand = child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<");
                }
                else if (child.Name.Equals("value"))
                {
                    this.value = child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
                }
                else if (child.Name.Equals("friendlyValue"))
                {
                    this.friendlyValue = child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
                }
                else if (child.Name.Equals("highValue"))
                {
                    this.highValue = child.InnerText;
                }
                else if (child.Name.Equals("friendlyHighValue"))
                {
                    this.friendlyHighValue = child.InnerText;
                }
                else if (child.Name.Equals("lowValue"))
                {
                    this.lowValue = child.InnerText;
                }
                else if (child.Name.Equals("friendlyLowValue"))
                {
                    this.friendlyLowValue = child.InnerText;
                }
                else if (child.Name.Equals("isBetween"))
                {
                    this.isBetween = bool.Parse(child.InnerText);
                }
                else if (child.Name.Equals("isEnabled"))
                {
                    this.isEnabled = bool.Parse(child.InnerText);
                }
            }

            if (this.ColumnType.Equals("System.DateTime"))
            {
                if (IsBetween)
                {
                    this.highValue = "#" + DateTime.Parse(this.highValue.Trim('#')).ToString(System.Globalization.CultureInfo.InvariantCulture) + "#";
                    this.lowValue = "#" + DateTime.Parse(this.lowValue.Trim('#')).ToString(System.Globalization.CultureInfo.InvariantCulture) + "#";

                    this.friendlyLowValue = DateTime.Parse(this.friendlyLowValue, System.Globalization.CultureInfo.InvariantCulture).ToString("d", System.Globalization.CultureInfo.CurrentCulture);
                    this.friendlyHighValue = DateTime.Parse(this.friendlyHighValue, System.Globalization.CultureInfo.InvariantCulture).ToString("d", System.Globalization.CultureInfo.CurrentCulture);

                    this.friendlyCondition = string.Format(SharedStrings.FRIENDLY_CONDITION_DATA_FILTER_BETWEEN, columnName, friendlyLowValue, friendlyHighValue);
                }
                else if (!String.IsNullOrEmpty(this.value.Trim()))
                {
                    this.value = "#" + DateTime.Parse(this.value.Trim('#')).ToString(System.Globalization.CultureInfo.InvariantCulture) + "#";
                    this.friendlyValue = DateTime.Parse(this.friendlyValue, System.Globalization.CultureInfo.InvariantCulture).ToString("d", System.Globalization.CultureInfo.CurrentCulture);
                    this.friendlyCondition = string.Format(SharedStrings.FRIENDLY_CONDITION_DATA_FILTER, columnName, friendlyOperand, friendlyValue);                    
                }
            }

            SetupFilterCondition();
        }
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Generates the necessary friendly values and other internal logic needed to create the filter
        /// </summary>        
        private void SetupFilterCondition() 
        {            
            Configuration config = Configuration.GetNewInstance();
            StringBuilder conditionBuilder = new StringBuilder();

            conditionBuilder.Append(StringLiterals.PARANTHESES_OPEN);

            if (IsBetween)
            {
                conditionBuilder.Append(this.ColumnName);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(StringLiterals.GREATER_THAN_OR_EQUAL);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(this.LowValue);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append("and");
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(this.ColumnName);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(StringLiterals.LESS_THAN_OR_EQUAL);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(this.HighValue);
            }
            else if (!IsBetween && columnType.Equals("System.String") && operand.Equals("=") && value.Equals("''"))
            {
                conditionBuilder.Append(StringLiterals.PARANTHESES_OPEN);

                conditionBuilder.Append(this.ColumnName);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(this.Operand);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(this.Value);

                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append("or");
                conditionBuilder.Append(StringLiterals.SPACE);

                conditionBuilder.Append(this.ColumnName);
                conditionBuilder.Append("is null");

                conditionBuilder.Append(StringLiterals.PARANTHESES_CLOSE);
            }
            else
            {
                conditionBuilder.Append(this.ColumnName);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(this.Operand);
                conditionBuilder.Append(StringLiterals.SPACE);
                conditionBuilder.Append(this.Value);
            }

            conditionBuilder.Append(StringLiterals.PARANTHESES_CLOSE);

            this.condition = conditionBuilder.ToString();
        }
        #endregion // Private Methods
    }
}
