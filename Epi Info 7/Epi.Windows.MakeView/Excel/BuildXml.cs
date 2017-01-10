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
            //  "C:/WorkSpace/Challenge/PageTemplate.xml"
            foreach (var NewPage in PageList)
            {

                XElement XmlElement = AddPageXml(NewXmlDoc, NewPage);


                if (NewPage.List_Values != null && NewPage.List_Values.Count() > 0 && NewPage.Question_Type == 17)
                {
                    XElement SourceTableElement = AddSourceTableXml(NewPage);
                    XElement TemplaitElement = NewXmlDoc.XPathSelectElement("Template");
                    TemplaitElement.Add(SourceTableElement);
                }
                // Update Check code
                if (!string.IsNullOrEmpty(NewPage.If_Condition) && !string.IsNullOrEmpty(NewPage.Then_Question))
                {
                    XmlElement.SetAttributeValue("CheckCode", GetCheckCode(XmlElement.Attribute("CheckCode").Value, NewPage));
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
            if (NewPage.Question_Type != 10)
            {
                FiledElement = AddControlXml(NewPage);
                PageElement.Add(FiledElement);
            }
            else
            {
                int count = 1;
                foreach (var checkbox in NewPage.List_Values)
                {

                    NewPage.Question = checkbox;
                    NewPage.Variable_Name = "checkbox_" + checkbox;
                    NewPage.Counter = count;
                    FiledElement = AddControlXml(NewPage);
                    PageElement.Add(FiledElement);
                    count++;
                }


            }
            // GroupBox Title
            if (!string.IsNullOrEmpty(NewPage.Title))
            {
                FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Title']");
                FiledElement.SetAttributeValue("Name", NewPage.Variable_Name + "_Title");
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
              
            }
            else {
                FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Title']");
                FiledElement.Remove();
            }
            // Description
            if (!string.IsNullOrEmpty(NewPage.Description))
            {
                FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Description']");
                FiledElement.SetAttributeValue("Name", NewPage.Variable_Name + "_Description");
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
            else {
                FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Description']");
                FiledElement.Remove();
            }
            // Add page element to Xml
            XElement XmlElement = NewXmlDoc.XPathSelectElement("Template/Project/View");
            XmlElement.Add(PageElement);
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

            SourceTableElement.SetAttributeValue("TableName", "code" + NewPage.List_Values[0]);
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
            StringBuilder _CheckCode = new StringBuilder();
            _CheckCode.Append(CheckCode);
            string text;
            using (var streamReader = new StreamReader(@"./Excel/IFElse.txt", Encoding.UTF8))
            {
                text = streamReader.ReadToEnd();
            }
            text = string.Format(text, NewPage.PageName, NewPage.Variable_Name, NewPage.If_Condition, NewPage.Then_Question, NewPage.Else_Question);
            _CheckCode.Append("\n" + text);
            return _CheckCode.ToString();
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
            if (NewPage.Question_Type == 17)
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
            if (NewPage.Question_Type == 12)
            {
                string Values = GetOptionsValue(NewPage.List_Values);
                FiledElement.SetAttributeValue("List", Values);
                FiledElement.SetAttributeValue("ShowTextOnRight", "True");
                FiledElement.SetAttributeValue("ControlWidthPercentage", "0.80");
                FiledElement.SetAttributeValue("ControlHeightPercentage", GetPositionValue(NewPage.List_Values.Count(), 0.12, 0.03));
                FiledElement.SetAttributeValue("ControlLeftPositionPercentage", "0.07 ");
                FiledElement.SetAttributeValue("ControlTopPositionPercentage", "0.17");
            }
            if (NewPage.Question_Type == 10)
            {
                Double ControlTopPositionPercentage = 0.12 + (0.04 * NewPage.Counter);

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