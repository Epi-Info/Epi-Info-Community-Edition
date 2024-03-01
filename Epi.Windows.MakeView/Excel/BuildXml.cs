using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Epi.Windows.MakeView.Excel
{
    public class BuildXml
    {
        public BuildXml()
        {
        }
        public XDocument BuildNewXml(List<Card> PageList, XDocument NewXmlDoc, string formName)
        {
            foreach (var NewPage in PageList)
            {
                XElement XmlElement = AddPageXml(NewXmlDoc, NewPage);

                if (NewPage.List_Values != null && NewPage.List_Values.Count() > 0 && NewPage.Question_Type == 17)
                {
                    XElement SourceTableElement = AddSourceTableXml(NewPage);
                    XElement TemplaitElement = NewXmlDoc.XPathSelectElement("Template");
                    TemplaitElement.Add(SourceTableElement);
                }

                string checkCode = string.Empty;
                if (!string.IsNullOrEmpty(NewPage.If_Condition) && !string.IsNullOrEmpty(NewPage.Then_Question))
                {
                    checkCode = GetCheckCode(XmlElement.Attribute("CheckCode").Value, NewPage);
                }

                if (string.IsNullOrEmpty(NewPage.GPSCheckCode) == false)
                {
                    checkCode = Environment.NewLine + XmlElement.Attribute("CheckCode").Value + NewPage.GPSCheckCode + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(checkCode) == false)
                {
                    XmlElement.SetAttributeValue("CheckCode", checkCode);
                }

                XmlElement.SetAttributeValue("Name", formName);
            }

            return NewXmlDoc;

        }

        private static XElement AddPageXml(XDocument NewXmlDoc, Card NewPage)
        {
            XmlDocument PageXml = new XmlDocument();
            PageXml.Load("./Excel/PageTemplate.xml");
            XDocument XPage = ToXDocument(PageXml);

            XElement PageElement = XPage.XPathSelectElement("Page");
            // change Default values
            PageElement.SetAttributeValue("PageId", NewPage.PageId);
            PageElement.SetAttributeValue("Name", NewPage.PageName);
            PageElement.SetAttributeValue("Position", NewPage.PageId - 1);
 
            XElement FiledElement = null;

            if (NewPage.Question_Type == 31) // GPS
            {
                XElement[] FieldElements = null;
                FieldElements = Add_GPS_Xml(NewPage);
                PageElement.Add(FieldElements);
            }
            else if (NewPage.Question_Type == 10) // CHECKBOX
            {
                NewPage.Question = NewPage.Question;
                NewPage.Variable_Name = NewPage.Variable_Name + "_" + NewPage.PageName.Replace(" ", "");
                NewPage.Question_Type = 2;
                FiledElement = AddControlXml(NewPage);
                FiledElement.SetAttributeValue("ControlTopPositionPercentage", 0.2);
                PageElement.Add(FiledElement);
                int count = 1;
                var Variable_Name = NewPage.Variable_Name;
                foreach (var checkbox in NewPage.List_Values)
                {
                    NewPage.Question_Type = 10;
                    NewPage.Question = checkbox;
                    NewPage.Variable_Name = Variable_Name + "_" + count;
                    NewPage.Counter = count;
                    FiledElement = AddControlXml(NewPage);
                    PageElement.Add(FiledElement);
                    count++;
                }

                NewPage.Variable_Name = Variable_Name;
            }
            else 
            {
                FiledElement = AddControlXml(NewPage);
                PageElement.Add(FiledElement);
            }

            // GroupBox Title
            if (!string.IsNullOrEmpty(NewPage.Title))
            {
                FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Title']");
                FiledElement.SetAttributeValue("Name", "Grp_" + NewPage.Variable_Name );
                FiledElement.SetAttributeValue("PromptText", NewPage.Title);
                FiledElement.SetAttributeValue("PageId", NewPage.PageId);
                FiledElement.SetAttributeValue("FieldTypeId", 21);
                FiledElement.SetAttributeValue("UniqueId", Guid.NewGuid().ToString());
                FiledElement.SetAttributeValue("PageName", NewPage.PageName);
                FiledElement.SetAttributeValue("Position", NewPage.PageId - 1);
                FiledElement.SetAttributeValue("FieldId", NewPage.PageId + 4);
                if (NewPage.Question_Type == 12 || NewPage.Question_Type == 10)
                {
                    FiledElement.SetAttributeValue("ControlHeightPercentage", GetPositionValue(NewPage.List_Values.Count(), 0.14, 0.05).ToString());
                }
                if (NewPage.Question_Type == 31)
                {
                    FiledElement.SetAttributeValue("ControlHeightPercentage", "0.66120218579235");
                }
            }
            else
            {
                FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Title']");
                FiledElement.Remove();
            }
            
            // Description
            if (!string.IsNullOrEmpty(NewPage.Description))
            {
                FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Description']");
                FiledElement.SetAttributeValue("Name", "Lbl_" + NewPage.Variable_Name );
                FiledElement.SetAttributeValue("PromptText", NewPage.Description);
                FiledElement.SetAttributeValue("PageId", NewPage.PageId);
                FiledElement.SetAttributeValue("FieldTypeId", 2);
                FiledElement.SetAttributeValue("UniqueId", Guid.NewGuid().ToString());
                FiledElement.SetAttributeValue("PageName", NewPage.PageName);
                FiledElement.SetAttributeValue("Position", NewPage.PageId - 1);
                FiledElement.SetAttributeValue("FieldId", NewPage.PageId + 5);

                if (NewPage.Question_Type == 12 || NewPage.Question_Type == 10)
                {
                    FiledElement.SetAttributeValue("ControlTopPositionPercentage", GetPositionValue(NewPage.List_Values.Count(), 0.14, 0.05).ToString());
                }
            }
            else
            {
                FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Description']");
                FiledElement.Remove();
            }

            bool pageAlreadyAdded = false;
            IEnumerable<XElement> PageElements = NewXmlDoc.Descendants("Page");
            foreach (XElement pgel in PageElements)
            {
                string pgid = pgel.Attribute("PageId").Value.ToString();
                if (pgid.Equals("" + NewPage.PageId))
                {
                    pageAlreadyAdded = true;
                    IEnumerable<XElement> pgelDescendants = pgel.Descendants("Field");
                    XElement lastField = pgelDescendants.Last<XElement>();
                    for (int di = pgelDescendants.Count<XElement>() - 1; di >= 0; di--)
                    {
                        lastField = pgelDescendants.ElementAt(di);
                        if (lastField.Attribute("Name").Value.StartsWith("Grp_"))
                            break;
                    }
                    int highestposition = (int)lastField.Attribute("Position");
                    float highesttoppositionpercentage = (float)lastField.Attribute("ControlTopPositionPercentage") + (float)lastField.Attribute("ControlHeightPercentage");
                    int fieldLoops = -1;
                    foreach (XElement fieldElement in PageElement.Descendants("Field"))
                    {
                        float currenttopposition = (float)fieldElement.Attribute("ControlTopPositionPercentage");
                        float currentcontrolheight = (float)fieldElement.Attribute("ControlHeightPercentage");
                        float currentprompttopposition = (float)-1.0;
                        if (!string.IsNullOrEmpty(fieldElement.Attribute("PromptTopPositionPercentage").Value))
                            currentprompttopposition = (float)fieldElement.Attribute("PromptTopPositionPercentage");
                        fieldElement.SetAttributeValue("Position", highestposition + 1);
                        fieldElement.SetAttributeValue("ControlTopPositionPercentage", Math.Min((549.0 / 780.0) * (highesttoppositionpercentage + currenttopposition), 0.999));
                        if (fieldElement.Attribute("Name").Value.StartsWith("Grp_"))
                            fieldElement.SetAttributeValue("ControlHeightPercentage", (549.0 / 780.0) * currentcontrolheight);
                        if (currentprompttopposition >= 0.0)
                            fieldElement.SetAttributeValue("PromptTopPositionPercentage", Math.Min((549.0 / 780.0) * (highesttoppositionpercentage + currentprompttopposition), 0.999));
                        if (fieldLoops > 0)
                            fieldElement.SetAttributeValue("Position", highestposition + 1 + fieldLoops);
                        pgel.Add(fieldElement);
                        fieldLoops++;
                    }
                    PageElement = pgel;
                    break;
                }
            }

            // Add page element to Xml
            XElement XmlElement = NewXmlDoc.XPathSelectElement("Template/Project/View");
            if (!pageAlreadyAdded)
                XmlElement.Add(PageElement);
            else
                XmlElement.SetAttributeValue("Orientation", "Portrait");
            return XmlElement;
        }

        private static string ListToString(List<string> list)
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 1; x < list.Count; x++)
            {
                sb.Append(list[x]);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private static XElement AddSourceTableXml(Card NewPage)
        {
            XmlDocument SourceTableXml = new XmlDocument();
            SourceTableXml.Load("./Excel/SourceTable.xml");
            XDocument XSourceTable = ToXDocument(SourceTableXml);
            XElement SourceTableElement = XSourceTable.XPathSelectElement("SourceTable");

            SourceTableElement.SetAttributeValue("TableName", "code" + NewPage.Variable_Name);
            for (int i = 1; NewPage.List_Values.Count() > i; i++)
            {
                XElement ItemElement = new XElement("Item");
                ItemElement.SetAttributeValue(NewPage.Variable_Name, NewPage.List_Values[i]);
                SourceTableElement.Add(ItemElement);
            }
            return SourceTableElement;
        }
        private string GetCheckCode(string CheckCode, Card NewPage)
        {
            StringBuilder builder = new StringBuilder(CheckCode);

            builder.Append("\rPage [" + NewPage.PageName + "]\r");
            builder.Append("    After\r");
            builder.Append("        IF " + NewPage.Variable_Name + " = \"" + NewPage.If_Condition  + "\"\r");
            builder.Append("        THEN GOTO " + NewPage.Then_Question + "\r");
            builder.Append("        ELSE GOTO " + NewPage.Else_Question + "\r");
            builder.Append("        END-IF\r");
            builder.Append("    End-After\r");
            builder.Append("End-Page\r");

            return builder.ToString();
        }
        public static XDocument ToXDocument(XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
        private static double GetPositionValue(int ListCount, Double StartValue, Double IncrementValue)
        {

            Double ControlPosition = StartValue;
            for (int i = 0; ListCount > i; i++)
            {
                ControlPosition = ControlPosition + IncrementValue;

            }
            return ControlPosition;
        }

        private static XElement[] Add_GPS_Xml(Card NewPage)
        {
            XmlDocument GPSXml = new XmlDocument();
            GPSXml.Load("./Excel/GPS.xml");
            XDocument GPSField = ToXDocument(GPSXml);
            List<XElement> fieldElements = GPSField.XPathSelectElement("GPS").Elements().ToList();

            string fieldName;
            int position = NewPage.PageId - 1;
            int fieldId = NewPage.PageId + 3;
            string buttonName = string.Empty;
            string latName = string.Empty;
            string lonName = string.Empty;

            foreach (XElement field in fieldElements)
            {
                fieldName = field.Attribute("Name").Value;

                field.SetAttributeValue("Name", NewPage.Variable_Name + fieldName);
                field.SetAttributeValue("PageId", NewPage.PageId);
                field.SetAttributeValue("UniqueId", Guid.NewGuid().ToString());
                field.SetAttributeValue("PageName", NewPage.PageName);
                field.SetAttributeValue("Position", position++);
                field.SetAttributeValue("FieldId", fieldId++);

                if(fieldName == "BTN")
                {
                    buttonName = NewPage.Variable_Name + fieldName;
                }
                else if (fieldName == "LAT")
                {
                    field.SetAttributeValue("IsRequired", NewPage.Required.ToString());
                    latName = NewPage.Variable_Name + fieldName;
                }
                else if (fieldName == "LONG")
                {
                    field.SetAttributeValue("IsRequired", NewPage.Required.ToString());
                    lonName = NewPage.Variable_Name + fieldName;
                }
                else
                {
                    field.SetAttributeValue("PromptText", NewPage.Question);
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("\rField " + buttonName + "\r");
            builder.Append("    Click\r");
            builder.Append("        ASSIGN  " + latName + " = SYSLATITUDE\r");
            builder.Append("        ASSIGN  " + lonName + " = SYSLONGITUDE\r");
            builder.Append("    End-Click\r");
            builder.Append("End-Field\r");

            NewPage.GPSCheckCode = builder.ToString();

            return fieldElements.ToArray();
        }
        private static XElement AddControlXml(Card NewPage)
        {
            // Control
            // XElement FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Controls']");

            XmlDocument FieldXml = new XmlDocument();
            FieldXml.Load("./Excel/Control.xml");
            XDocument XField = ToXDocument(FieldXml);
            XElement FiledElement = XField.XPathSelectElement("Field");

            FiledElement.SetAttributeValue("IsRequired", NewPage.Required.ToString());
            FiledElement.SetAttributeValue("Name", NewPage.Variable_Name);
            FiledElement.SetAttributeValue("PromptText", NewPage.Question);
            FiledElement.SetAttributeValue("PageId", NewPage.PageId);
            FiledElement.SetAttributeValue("FieldTypeId", NewPage.Question_Type);
            FiledElement.SetAttributeValue("UniqueId", Guid.NewGuid().ToString());
            FiledElement.SetAttributeValue("PageName", NewPage.PageName);
            FiledElement.SetAttributeValue("Position", NewPage.PageId - 1);
            FiledElement.SetAttributeValue("FieldId", NewPage.PageId + 3);

            if (NewPage.Question_Type == 17) // DROPDOWN
            {
                XmlDocument SourceTableXml = new XmlDocument();
                string SourceTableXmlpath = "./Excel/SourceTable.xml";
                SourceTableXml.Load(SourceTableXmlpath);
                XDocument XSourceTable = ToXDocument(SourceTableXml);
                XElement SourceTableElement = XSourceTable.XPathSelectElement("SourceTable");
                SourceTableElement.SetAttributeValue("TableName", "code" + NewPage.Variable_Name);
                var TableName = SourceTableElement.Attribute("TableName").Value;
                FiledElement.SetAttributeValue("SourceTableName", TableName);
                FiledElement.SetAttributeValue("CodeColumnName", NewPage.Variable_Name);
                FiledElement.SetAttributeValue("TextColumnName", NewPage.Variable_Name);
            }
            else if (NewPage.Question_Type == 12) // OPTIONS
            {
                string Values = GetOptionsValue(NewPage.List_Values);
                FiledElement.SetAttributeValue("List", Values);
                FiledElement.SetAttributeValue("ShowTextOnRight", "True");
                FiledElement.SetAttributeValue("ControlWidthPercentage", "0.80");
                FiledElement.SetAttributeValue("ControlHeightPercentage", GetPositionValue(NewPage.List_Values.Count(), 0.12, 0.03));
                FiledElement.SetAttributeValue("ControlLeftPositionPercentage", "0.07 ");
                FiledElement.SetAttributeValue("ControlTopPositionPercentage", "0.17");
            }
            else if (NewPage.Question_Type == 10) // CHECKBOX
            {
                Double ControlTopPositionPercentage = 0.23 + (0.04 * NewPage.Counter);
                FiledElement.SetAttributeValue("ControlTopPositionPercentage", ControlTopPositionPercentage.ToString());
            }

            return FiledElement;
        }

        private static string GetOptionsValue(List<string> list)
        {
            StringBuilder Value = new StringBuilder();
            Value.Append(string.Join(",", list));
            Value.Append("||");
            var x = ".01231";

            for (int i = 1; i < list.Count() + 1; i++)
            {
                var y = .03000;
                y = y * i;
                Value.Append(y.ToString() + ":" + x);
                if (i < list.Count())
                {
                    Value.Append(",");
                }
            }
            return Value.ToString();
        }
    }
}