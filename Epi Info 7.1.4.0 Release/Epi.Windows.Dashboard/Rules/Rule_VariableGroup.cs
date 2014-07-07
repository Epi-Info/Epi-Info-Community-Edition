using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace Epi.WPF.Dashboard.Rules
{
    /// <summary>
    /// A class for assigning data to a column in the database
    /// </summary>
    public class Rule_VariableGroup : IDashboardRule
    {
        #region Protected Members
        protected string friendlyRule;
        protected string groupName;
        protected List<string> variables;
        protected DashboardHelper dashboardHelper;        
        #endregion // Protected Members

        #region Constructors

        public Rule_VariableGroup(DashboardHelper dashboardHelper)
        {
            this.dashboardHelper = dashboardHelper;
        }

        public Rule_VariableGroup(DashboardHelper dashboardHelper, string name, List<string> groupVariables)
        {
            this.dashboardHelper = dashboardHelper;
            this.groupName = name;
            this.variables = new List<string>();

            foreach (string s in groupVariables)
            {
                variables.Add(s);
            }

            this.friendlyRule = "Create a variable group called " + this.groupName + " and include: ";
            foreach (string var in variables)
            {
                this.friendlyRule = this.friendlyRule + var + ", ";
            }
            this.friendlyRule = this.friendlyRule.TrimEnd(' ').TrimEnd(',');
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the name of the column where the assigned values will reside
        /// </summary>
        public string GroupName
        {
            get { return this.groupName; }
        }

        /// <summary>
        /// Gets whether or not the variable is dependent on another
        /// </summary>
        public bool HasDependencies
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the list of variables in the group
        /// </summary>
        public List<string> Variables
        {
            get
            {
                return this.variables;
            }
        }

        /// <summary>
        /// Gets the variables in the group as an array of strings
        /// </summary>
        public string[] VariableArray
        {
            get
            {
                string[] variableArray = new string[this.Variables.Count];
                for (int i = 0; i < this.Variables.Count; i++)
                {
                    variableArray[i] = Variables[i];
                }
                return variableArray;
            }
        }
        #endregion // Public Properties

        #region Protected Properties
        /// <summary>
        /// Gets the dashboard helper attached to this rule
        /// </summary>
        protected DashboardHelper DashboardHelper
        {
            get
            {
                return this.dashboardHelper;
            }
            set
            {
                this.dashboardHelper = value;
            }
        }
        #endregion // Protected Properties

        #region IDashboardRule Members
        /// <summary>
        /// Gets the human-readable (display) version of the rule
        /// </summary>
        public string FriendlyRule
        {
            get { return this.friendlyRule; }
        }

        /// <summary>
        /// Converts the value of the current EpiDashboard.Rule_Format object to its equivalent string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.FriendlyRule;
        }

        public void ApplyRule(DataRow row) { }

        public void SetupRule(DataTable table) { }

        public XmlNode Serialize(XmlDocument doc) 
        {
            string xmlString =
            "<friendlyRule>" + friendlyRule + "</friendlyRule>" +
            "<groupName>" + GroupName + "</groupName>";

            xmlString = xmlString + "<variables>";

            foreach (string s in this.Variables)
            {
                xmlString = xmlString + "<variable>";
                xmlString = xmlString + s.Replace("<", "&lt;").Replace(">", "&gt;");
                xmlString = xmlString + "</variable>";
            }

            xmlString = xmlString + "</variables>";

            System.Xml.XmlElement element = doc.CreateElement("rule");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute type = doc.CreateAttribute("ruleType");
            type.Value = "Epi.WPF.Dashboard.Rules.Rule_VariableGroup";

            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(XmlElement element)
        {
            this.variables = new List<string>();            

            foreach (XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("friendlyRule"))
                {
                    this.friendlyRule = child.InnerText;
                }
                else if (child.Name.Equals("groupName"))
                {
                    this.groupName = child.InnerText;
                }
                else if (child.Name.Equals("variables"))
                {
                    foreach (XmlElement recodeRow in child.ChildNodes)
                    {
                        if (recodeRow.Name.ToLower().Equals("variable"))
                        {
                            variables.Add(recodeRow.InnerText.Replace("&lt;", "<").Replace("&gt;", ">"));
                        }
                    }
                }
            }
        }
        #endregion // IDashboardRule Members
    }
}
