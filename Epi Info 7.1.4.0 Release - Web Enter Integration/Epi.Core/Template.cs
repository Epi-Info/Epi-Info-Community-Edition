using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Epi.Data.Services;
using Epi.Fields;
using EpiInfo.Plugin;

namespace Epi
{
    public class Template
    {
        ICommandContext context = null;
        IMetadataProvider metadata = null;
        View view = null;
        Page page = null;
        String templatesPath = string.Empty;

        public Template()
        {
            templatesPath = Configuration.GetNewInstance().Directories.Templates;
        }

        #region Template Utility Methods
        public static string[] GetFiles(string path, string searchPattern) 
        { 
            string[] m_arExt = searchPattern.Split(';'); 
            List<string> strFiles = new List<string>();
            List<string> strValidFiles = new List<string>(); 
            
            foreach (string filter in m_arExt) 
            { 
                strFiles.AddRange(System.IO.Directory.GetFiles(path, filter)); 
            }

            foreach (string fileName in strFiles)
            {
                using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(fileName))
                {
                    while (reader.ReadToFollowing("Template"))
                    {
                        string name = reader.GetAttribute("Name");

                        if (string.IsNullOrEmpty(name) == false)
                        {
                            strValidFiles.Add(fileName);
                        }
                    }
                }
            }

            return strValidFiles.ToArray(); 
        }

        public DataTable GetProjectTable(string subFolder)
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("TemplateName", System.Type.GetType("System.String")));
            table.Columns.Add(new DataColumn("TemplateDescription", System.Type.GetType("System.String")));
            table.Columns.Add(new DataColumn("TemplatePath", System.Type.GetType("System.String")));
            table.Columns.Add(new DataColumn("TemplateCreateDate", System.Type.GetType("System.String")));
 
            table.Columns.Add(new DataColumn("Name", System.Type.GetType("System.String")));
            table.Columns.Add(new DataColumn("Location", System.Type.GetType("System.String")));
            table.Columns.Add(new DataColumn("Description", System.Type.GetType("System.String")));
            table.Columns.Add(new DataColumn("EpiVersion", System.Type.GetType("System.String")));
            table.Columns.Add(new DataColumn("CreateDate", System.Type.GetType("System.String")));
            DataRow row;

            string projectFolderPath = Path.Combine(templatesPath, subFolder);
            
            if (Directory.Exists(projectFolderPath) != true)
            {
                return table;
            }

            String[] projectTemplates = GetFiles(projectFolderPath, "*.xml;*.eit");

            foreach(string path in projectTemplates)
            {
                row = table.NewRow();

                try
                {
                    using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(path))
                    {
                        while (reader.ReadToFollowing("Template"))
                        {
                            if (reader.MoveToFirstAttribute())
                            {
                                AddAttributeToProjectTableRow(row, reader);

                                while (reader.MoveToNextAttribute())
                                {
                                    AddAttributeToProjectTableRow(row, reader);
                                }

                                while (reader.ReadToFollowing("Project"))
                                {
                                    if (reader.MoveToFirstAttribute())
                                    {

                                        if (table.Columns.Contains(reader.Name))
                                        {
                                            row[reader.Name] = reader.Value;
                                        }

                                        while (reader.MoveToNextAttribute())
                                        {
                                            if (table.Columns.Contains(reader.Name))
                                            {
                                                row[reader.Name] = reader.Value;
                                            }
                                        }
                                    }
                                }

                                row["TemplatePath"] = path;
                                table.Rows.Add(row);
                            }
                        }
                    }
                }
                catch{}
            }

            return table;
        }

        private static void AddAttributeToProjectTableRow(DataRow row, System.Xml.XmlReader reader)
        {
            switch (reader.Name)
            {
                case "Name":
                    row["TemplateName"] = reader.Value;
                    break;
                case "Description":
                    row["TemplateDescription"] = reader.Value;
                    break;
                case "CreateDate":
                    row["TemplateCreateDate"] = reader.Value;
                    break;
            }
        }

        #endregion

    }
}
