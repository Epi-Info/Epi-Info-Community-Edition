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


            XElement FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Controls']");
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
                SourceTableXml.Load("./Excel/SourceTable.xml");
                XDocument XSourceTable = ToXDocument(SourceTableXml);
                XElement SourceTableElement = XSourceTable.XPathSelectElement("SourceTable");
                //var TableName = SourceTableElement.Attribute("TableName").Value;
                FiledElement.SetAttributeValue("SourceTableName", "code" + NewPage.List_Values[0]);
                FiledElement.SetAttributeValue("CodeColumnName", NewPage.List_Values[0]);
                FiledElement.SetAttributeValue("TextColumnName", NewPage.List_Values[0]);

            }
            // GroupBox Title
            FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Title']");
            FiledElement.SetAttributeValue("Name", NewPage.Variable_Name + "_Title");
            FiledElement.SetAttributeValue("PromptText", NewPage.Question);
            FiledElement.SetAttributeValue("PageId", NewPage.PageId);
            FiledElement.SetAttributeValue("FieldTypeId", 21);
            FiledElement.SetAttributeValue("UniqueId", Guid.NewGuid().ToString());
            FiledElement.SetAttributeValue("PageName", NewPage.PageName);
            FiledElement.SetAttributeValue("Position", NewPage.PageId - 1);
            FiledElement.SetAttributeValue("FieldId", NewPage.PageId + 4);

            // Description
            FiledElement = XPage.XPathSelectElement("Page/Field[@Name='Description']");
            FiledElement.SetAttributeValue("Name", NewPage.Variable_Name + "_Description");
            FiledElement.SetAttributeValue("PromptText", NewPage.Question);
            FiledElement.SetAttributeValue("PageId", NewPage.PageId);
            FiledElement.SetAttributeValue("FieldTypeId", 2);
            FiledElement.SetAttributeValue("UniqueId", Guid.NewGuid().ToString());
            FiledElement.SetAttributeValue("PageName", NewPage.PageName);
            FiledElement.SetAttributeValue("Position", NewPage.PageId - 1);
            FiledElement.SetAttributeValue("FieldId", NewPage.PageId + 5);
            if (NewPage.Question_Type == 12)
            {
                FiledElement.SetAttributeValue("List", ListToString(NewPage.List_Values));
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
                ItemElement.SetAttributeValue(NewPage.List_Values[0], NewPage.List_Values[i]);
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

    }
}