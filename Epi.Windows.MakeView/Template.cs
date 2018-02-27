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
using Epi.Windows.Controls;
using Epi.Windows.MakeView.PresentationLogic;

namespace Epi.Windows.MakeView
{
    class Template
    {
        GuiMediator mediator = null;
        ICommandContext context = null;
        IMetadataProvider metadata = null;
        View view = null;
        Page page = null;
        Dictionary<int, string> pageIdViewNamePairs = new Dictionary<int, string>();
        Dictionary<int, string> viewIdViewNamePairs = new Dictionary<int, string>();
        List<string> _sourceTableNames = new List<string>();
        Dictionary<int,string> _gridFieldIds = new Dictionary<int,string>();
        bool _fieldDrop = false;
        Point _fieldDropLocaton = new Point();
        int _dropOnNodePosition = -1;
        Enums.TemplateLevel _templateLevel = Enums.TemplateLevel.Unknown;
        Dictionary<int, int> _templateFieldId_dbFieldId = new Dictionary<int, int>();

        //metaBackground	BackgroundId
        //metaFields	    FieldId
        //metaGridColumns	GridColumnId
        //metaImages	    ImageId
        //metaLayers	    LayerId
        //metaLinks	        LinkId
        //metaMapLayers	    MapLayerId
        //metaMapPoints	    MapPointId
        //metaMaps	        MapId
        //metaPages	        PageId
        //metaPrograms	    ProgramId
        //metaViews	        ViewId

        Dictionary<string, string> _sourceTableRenames = new Dictionary<string, string>();
        Dictionary<string, string> _fieldRenames = new Dictionary<string, string>();
        List<DDLFieldOfCodes> _codeFields = new List<DDLFieldOfCodes>();
        List<MirrorField> _mirrorFields = new List<MirrorField>();

        DataTable _fieldRecord = new DataTable();

        public static System.Globalization.NumberFormatInfo ni = null;

        public Template(GuiMediator mediator)
        {
            this.mediator = mediator;
            this.context = ((Epi.Windows.MakeView.Forms.MakeViewMainForm)mediator.Canvas.MainForm).EpiInterpreter.Context;
            if (mediator.ProjectExplorer.currentPage != null)
            {
                this.view = mediator.ProjectExplorer.currentPage.view;
                this.page = mediator.ProjectExplorer.currentPage;
                this.metadata = view.GetMetadata();
            }

            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;

            ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            _fieldRecord.Columns.Add(FieldRecordColumn.NameTemplate.ToString(), typeof(string));
            _fieldRecord.Columns.Add(FieldRecordColumn.IdTemplate.ToString(), typeof(int));
            _fieldRecord.Columns.Add(FieldRecordColumn.NameCreated.ToString(), typeof(string));
            _fieldRecord.Columns.Add(FieldRecordColumn.IdCreated.ToString(), typeof(int));
        }

        static public string GetTemplatePath(TemplateNode node)
        {
            return GetTemplatePath((string)node.Tag);
        }

        static public string GetTemplatePath(string nodeTag)
        {
            string templatePath = "";

            string programFilesDirectoryName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).ToLowerInvariant();
            string installFolder = AppDomain.CurrentDomain.BaseDirectory.ToLowerInvariant();

            Configuration config = Configuration.GetNewInstance();
            string configPath = config.Directories.Templates;

            if (configPath != string.Empty)
            {
                templatePath = Path.Combine(configPath, nodeTag);
            }
            else if (installFolder.ToLowerInvariant().StartsWith(programFilesDirectoryName))
            {
                templatePath = Path.Combine(installFolder, "Templates\\" + nodeTag);
            }
            else
            {
                string asmPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                templatePath = asmPath + nodeTag;
            }
            return templatePath;
        }

        static public Enums.TemplateLevel GetTemplateLevel(TemplateNode node)
        {
            if (node.Tag is string)
            {
                return GetTemplateLevel(GetTemplatePath(node));
            }
            return Enums.TemplateLevel.Unknown;
        }

        static public Enums.TemplateLevel GetTemplateLevel(String templatePath)
        {
            if (ValidateTemplate(templatePath))
            {
                using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(templatePath))
                {
                    while (reader.ReadToFollowing("Template"))
                    {
                        if (reader.MoveToAttribute("Level"))
                        {
                            switch (reader.Value)
                            {
                                case "Project":
                                    return Enums.TemplateLevel.Project;
                                case "Form":
                                case "View":
                                    return Enums.TemplateLevel.Form;
                                case "Page":
                                    return Enums.TemplateLevel.Page;
                                case "Field":
                                    return Enums.TemplateLevel.Field;
                                default:
                                    return Enums.TemplateLevel.Unknown;
                            }
                        }
                    }
                }
            }
            return Enums.TemplateLevel.Unknown;
        }

        public static bool ValidateTemplate(string templatePath)
        {
            if (File.Exists(templatePath) == false) return false;

            if (new FileInfo(templatePath).Length == 0) return false;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(ValidationCallBack);
            XmlReader reader = XmlReader.Create(templatePath, settings);

            try
            {
                while (reader.Read()) ;
                return true;
            }
            catch
            {
                ValidationCallBack(null, null);
            }

            return false;
        }

        private static void ValidationCallBack(object sender, System.Xml.Schema.ValidationEventArgs args)
        {
            if (sender == null && args == null)
            {
            }
        }

        public Size GetFieldFootprint(TemplateNode node)
        {
            Size size = new Size();
            string templatePath = GetTemplatePath(node);

            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(templatePath))
            {
                while (reader.ReadToFollowing("FieldFootprint"))
                {
                    if (reader.MoveToFirstAttribute())
                    {
                        do
                        {
                            if (reader.Name == "Width") size.Width = int.Parse(reader.Value);
                            if (reader.Name == "Height") size.Height = int.Parse(reader.Value);
                        }
                        while (reader.MoveToNextAttribute());
                    }
                    break;
                }
            }

            return size;
        }

        #region Public Template Creation Methods

        private void AddThenCloseWriter(System.Xml.XmlWriter writer)
        {
            AddCodeTableTemplates(writer);
            AddGridColumnTemplate(writer);
            AddBackgroundTableTemplate(writer);
            writer.WriteEndDocument();
            writer.Close();
        }
        
        public void CreateProjectTemplate(string nameWithSubfolders, string templateDescription)
        {
            _templateLevel = Enums.TemplateLevel.Project;
            System.Xml.XmlWriter writer = CreateWriter(nameWithSubfolders, "Projects");
            CreateProjectTemplate(nameWithSubfolders, templateDescription, writer);
            AddThenCloseWriter(writer);
        }

        public void CreateProjectTemplate(string projectName, string templateDescription, string fileNameWithSubfolders)
        {
            _templateLevel = Enums.TemplateLevel.Project;
            System.Xml.XmlWriter writer = CreateWriter(fileNameWithSubfolders, "Projects");
            CreateProjectTemplate(projectName, templateDescription, writer);
            AddThenCloseWriter(writer);
        }

        public void SaveProjectTemplateAs(string projectName, string templateDescription, string folderName)
        {
            System.Xml.XmlWriter writer = CreateWriter(System.IO.Path.Combine(folderName, projectName) + ".xml");
            CreateProjectTemplate(projectName, templateDescription, writer);
            AddThenCloseWriter(writer);
        }

        private XmlWriter CreateWriter(string fileNameWithPath)
        {
            System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();
            IMetadataProvider metadata = view.GetMetadata();

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileNameWithPath)) == false)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileNameWithPath));
            }

            XmlWriter writer = XmlWriter.Create(fileNameWithPath, null);
            return writer;
        }

        public void CreateProjectTemplate(string nameWithSubfolders, string templateDescription, XmlWriter writer)
        {
            writer.WriteStartElement("Template");
            writer.WriteAttributeString("Name", nameWithSubfolders);
            writer.WriteAttributeString("Description", templateDescription);
            writer.WriteAttributeString("CreateDate", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            writer.WriteAttributeString("Level", "Project");

            writer.WriteStartElement("Project");
            writer.WriteAttributeString("Id", string.Empty);
            writer.WriteAttributeString("Name", mediator.Project.Name);
            writer.WriteAttributeString("Location", string.Empty);
            writer.WriteAttributeString("Description", mediator.Project.Description);
            writer.WriteAttributeString("EpiVersion", mediator.Project.EpiVersion);
            writer.WriteAttributeString("CreateDate", string.Empty);

            Configuration configuration = Configuration.GetNewInstance();

            writer.WriteAttributeString("ControlFontBold", configuration.Settings.ControlFontBold.ToString());
            writer.WriteAttributeString("ControlFontItalics", configuration.Settings.ControlFontItalics.ToString());
            writer.WriteAttributeString("ControlFontName", configuration.Settings.ControlFontName);
            writer.WriteAttributeString("ControlFontSize", configuration.Settings.ControlFontSize.ToString());

            writer.WriteAttributeString("DefaultLabelAlign", configuration.Settings.DefaultLabelAlign);
            writer.WriteAttributeString("DefaultPageHeight", configuration.Settings.DefaultPageHeight.ToString());
            writer.WriteAttributeString("DefaultPageOrientation", configuration.Settings.DefaultPageOrientation);
            writer.WriteAttributeString("DefaultPageWidth", configuration.Settings.DefaultPageWidth.ToString());

            writer.WriteAttributeString("EditorFontBold", configuration.Settings.EditorFontBold.ToString());
            writer.WriteAttributeString("EditorFontItalics", configuration.Settings.EditorFontItalics.ToString());
            writer.WriteAttributeString("EditorFontName", configuration.Settings.EditorFontName);
            writer.WriteAttributeString("EditorFontSize", configuration.Settings.EditorFontSize.ToString());

            writer.WriteStartElement("CollectedData");
            writer.WriteStartElement("Database");
            writer.WriteAttributeString("Source", string.Empty);
            writer.WriteAttributeString("DataDriver", string.Empty);
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("Metadata");
            writer.WriteAttributeString("Source", string.Empty);
            writer.WriteEndElement();

            writer.WriteStartElement("EnterMakeviewInterpreter");
            writer.WriteAttributeString("Source", mediator.Project.EnterMakeviewIntepreter);
            writer.WriteEndElement();

            foreach (View view in mediator.Project.Views)
            {
                this.view = view;
                CreateViewTemplate(nameWithSubfolders, writer);
            }
            writer.WriteEndElement();
        }
        public void CreateEWEProjectTemplate(string nameWithSubfolders, string templateDescription, XmlWriter writer)
            {
            writer.WriteStartElement("Template");
            writer.WriteAttributeString("Name", nameWithSubfolders);
            writer.WriteAttributeString("Description", templateDescription);
            writer.WriteAttributeString("CreateDate", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            writer.WriteAttributeString("Level", "Project");

            writer.WriteStartElement("Project");
            writer.WriteAttributeString("Id", string.Empty);
            writer.WriteAttributeString("Name", mediator.Project.Name);
            writer.WriteAttributeString("Location", string.Empty);
            writer.WriteAttributeString("Description", mediator.Project.Description);
            writer.WriteAttributeString("EpiVersion", mediator.Project.EpiVersion);
            writer.WriteAttributeString("CreateDate", string.Empty);

            Configuration configuration = Configuration.GetNewInstance();

            writer.WriteAttributeString("ControlFontBold", configuration.Settings.ControlFontBold.ToString());
            writer.WriteAttributeString("ControlFontItalics", configuration.Settings.ControlFontItalics.ToString());
            writer.WriteAttributeString("ControlFontName", configuration.Settings.ControlFontName);
            writer.WriteAttributeString("ControlFontSize", configuration.Settings.ControlFontSize.ToString());

            writer.WriteAttributeString("DefaultLabelAlign", configuration.Settings.DefaultLabelAlign);
            writer.WriteAttributeString("DefaultPageHeight", configuration.Settings.DefaultPageHeight.ToString());
            writer.WriteAttributeString("DefaultPageOrientation", configuration.Settings.DefaultPageOrientation);
            writer.WriteAttributeString("DefaultPageWidth", configuration.Settings.DefaultPageWidth.ToString());

            writer.WriteAttributeString("EditorFontBold", configuration.Settings.EditorFontBold.ToString());
            writer.WriteAttributeString("EditorFontItalics", configuration.Settings.EditorFontItalics.ToString());
            writer.WriteAttributeString("EditorFontName", configuration.Settings.EditorFontName);
            writer.WriteAttributeString("EditorFontSize", configuration.Settings.EditorFontSize.ToString());

            writer.WriteStartElement("CollectedData");
            writer.WriteStartElement("Database");
            writer.WriteAttributeString("Source", string.Empty);
            writer.WriteAttributeString("DataDriver", string.Empty);
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("Metadata");
            writer.WriteAttributeString("Source", string.Empty);
            writer.WriteEndElement();

            writer.WriteStartElement("EnterMakeviewInterpreter");
            writer.WriteAttributeString("Source", mediator.Project.EnterMakeviewIntepreter);
            writer.WriteEndElement();

            foreach (View view in mediator.Project.Views)
                {
                this.view = view;
                CreateViewTemplate(nameWithSubfolders, writer);
                }
            writer.WriteEndElement();
            }
        public string CreateWebSurveyTemplate()
        {
            StringWriter sw = new StringWriter();

            XmlWriter writer = XmlWriter.Create(sw); //CreateWriter(nameWithSubfolders, "Forms");
            writer.WriteStartDocument();
            writer.WriteStartElement("Template");
            writer.WriteAttributeString("Level", "View");
            writer.WriteStartElement("Project");
            CreateViewTemplate("web", writer);
            writer.WriteEndElement();
            AddCodeTableTemplates(writer);
            writer.WriteEndElement();
            writer.Close();
            return sw.ToString();
        }
        public string CreateWebEnterTemplate()
            {

            StringWriter sw = new StringWriter();

            XmlWriter writer = XmlWriter.Create(sw);  
            writer.WriteStartDocument();
            //writer.WriteStartElement("Template");
            //writer.WriteAttributeString("Level", "View");
            //writer.WriteStartElement("Project");
            CreateProjectTemplate("web", "dis", writer);
           // writer.WriteEndElement();
            AddCodeTableTemplates(writer);
           // writer.WriteEndElement();
            writer.Close();
            return sw.ToString();
            }
        public void CreatePhoneTemplate(string path)
        {
            StringWriter sw = new StringWriter();

            XmlWriter writer = XmlWriter.Create(path, null);
            writer.WriteStartDocument();
            writer.WriteStartElement("Template");
            writer.WriteAttributeString("Level", "View");
            writer.WriteStartElement("Project");
            CreateViewTemplate("web", writer);
            writer.WriteEndElement();
            AddCodeTableTemplates(writer);
            writer.WriteEndElement();
            writer.Close();
        }

        public void CreateViewTemplate(string nameWithSubfolders, View rightclickview)
        {
            if (rightclickview != null)
                view = rightclickview;
            System.Xml.XmlWriter writer = CreateWriter(nameWithSubfolders, "Forms");
            writer.WriteStartDocument();
            writer.WriteStartElement("Template");
            writer.WriteAttributeString("Name", nameWithSubfolders);
            writer.WriteAttributeString("Description", "");
            writer.WriteAttributeString("CreateDate", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            writer.WriteAttributeString("Level", "View");
            writer.WriteStartElement("Project");
            CreateViewTemplate(nameWithSubfolders, writer);
            writer.WriteEndElement();
            AddCodeTableTemplates(writer);
            AddGridColumnTemplate(writer);
            writer.WriteEndElement(); 
            writer.Close();
        }

        public void CreateViewTemplate(string nameWithSubfolders, XmlWriter writer)
        {
            writer.WriteStartElement("View");

            writer.WriteAttributeString("ViewId", view.Id.ToString());
            writer.WriteAttributeString("Name", view.Name);
            writer.WriteAttributeString("IsRelatedView", view.IsRelatedView.ToString());
            writer.WriteAttributeString("CheckCode", view.CheckCode);
            writer.WriteAttributeString("Width", view.PageWidth.ToString());
            writer.WriteAttributeString("Height", view.PageHeight.ToString());
            writer.WriteAttributeString("Orientation", view.PageOrientation.ToString());
            writer.WriteAttributeString("LabelAlign", view.PageLabelAlign.ToString());

            List<Page> pages = view.GetMetadata().GetViewPages(view);

            foreach (Page page in pages)
            {
                this.page = page;
                CreatePageTemplate(nameWithSubfolders, writer);
            }

            writer.WriteEndElement();
        }

        public void CreatePageTemplate(string nameWithSubfolders)
        {
            System.Xml.XmlWriter writer = CreateWriter(nameWithSubfolders, "Pages");
            writer.WriteStartDocument();
            writer.WriteStartElement("Template");
            writer.WriteAttributeString("Level", "Page");
            writer.WriteStartElement("Project");
            writer.WriteStartElement("View");

            string pageCode = GetCheckCodeSubset(page.Name, view.CheckCode);
            writer.WriteAttributeString("CheckCode", pageCode);

            CreatePageTemplate(nameWithSubfolders, writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
            AddCodeTableTemplates(writer);
            AddGridColumnTemplate(writer);
            writer.WriteEndElement();
            writer.Close();
        }

        public void CreatePageTemplate(string nameWithSubfolders, XmlWriter writer)
        {
            writer.WriteStartElement("Page");
            writer.WriteAttributeString("PageId", page.Id.ToString());
            writer.WriteAttributeString("Name", page.Name);
            writer.WriteAttributeString("Position", page.Position.ToString());
            writer.WriteAttributeString("BackgroundId", page.BackgroundId.ToString());
            writer.WriteAttributeString("ViewId", view.Id.ToString());

            DataTable table = page.GetMetadata().GetFieldsOnPageAsDataTable(page.Id);
            DataRow[] rows = EnhancedSort(table);

            int currentFieldTypeId;

            foreach (DataRow row in rows)
            {
                currentFieldTypeId = (int)row["FieldTypeId"];

                if (currentFieldTypeId == (int)MetaFieldType.Relate) 
                {
                    if (_templateLevel == Enums.TemplateLevel.Field || _templateLevel == Enums.TemplateLevel.Page || _templateLevel == Enums.TemplateLevel.Form)
                    {
                        continue;
                    }
                }
                
                writer.WriteStartElement("Field");

                if (currentFieldTypeId == (int)MetaFieldType.Grid)
                {
                    _gridFieldIds.Add((int)row["FieldId"], (string)row["Name"]);
                }

                foreach (DataColumn column in table.Columns)
                {
                    bool skipAddAttribute = column.ColumnName == "BackgroundColor" && row[column.ColumnName].ToString() == "";

                    if (skipAddAttribute == false)
                    {
                        writer.WriteAttributeString(column.ColumnName, row[column.ColumnName].ToString());
                    }

                    if (column.ColumnName == ColumnNames.FIELD_TYPE_ID)
                    {
                        currentFieldTypeId = (int)row[column.ColumnName];

                        if (currentFieldTypeId == (int)MetaFieldType.Relate)
                        {
                            int relatedViewId = -1;

                            if(int.TryParse(row["RelatedViewId"].ToString(), out relatedViewId))
                            {
                                View relateView = metadata.GetViewById(relatedViewId);

                                if (relateView != null)
                                {
                                    writer.WriteAttributeString("RelatedViewName", relateView.Name);
                                }
                            }
                        }

                        if (currentFieldTypeId == (int)MetaFieldType.LegalValues || currentFieldTypeId == (int)MetaFieldType.Codes || currentFieldTypeId == (int)MetaFieldType.CommentLegal)
                        {
                            string sourceTableName = row["SourceTableName"].ToString();
                            if (sourceTableName.Length > 0 && _sourceTableNames.Contains(sourceTableName) == false && sourceTableName.StartsWith("code"))
                            {
                                _sourceTableNames.Add(sourceTableName);
                            }
                        }

                        if (currentFieldTypeId == (int)MetaFieldType.Grid)
                        {
                            int fieldId = (int)row["FieldId"];
                            DataTable gridColumnTable = page.GetMetadata().GetGridColumns(fieldId);

                            foreach (DataRow dataRow in gridColumnTable.Rows)
                            {
                                string sourceTableName = dataRow["SourceTableName"].ToString();
                                if (sourceTableName.Length > 0 && _sourceTableNames.Contains(sourceTableName) == false && sourceTableName.StartsWith("code"))
                                {
                                    _sourceTableNames.Add(dataRow["SourceTableName"].ToString());
                                }
                            }
                        }
                    }
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        string GetCheckCodeSubset(string pageName, string checkCode)
        {
            string checkCodeSuperset = checkCode;
            StringBuilder subset = new StringBuilder();

            string candidate = string.Empty;
            int index = 0;
            bool ContainsField = checkCode.Contains("End-Page");

            while (checkCode.Length > 0 && ContainsField)
            {
                if (checkCode.Contains("End-Page"))
                {
                    checkCode = checkCode.Substring(0, checkCode.LastIndexOf("End-Page"));
                }
                else
                {
                    break;
                }

                index = checkCode.LastIndexOf("\nPage ");
                index = index == -1 ? checkCode.LastIndexOf("\n\tPage ") : index;
                index = index == -1 ? checkCode.LastIndexOf(" Page ") : index;
                index = index == -1 ? checkCode.LastIndexOf("Page ") : index;

                candidate = checkCode.Substring(index, checkCode.Length - index);
                candidate = candidate.TrimStart(new char[] { '\r', '\n', '\t' });
                candidate = candidate.Substring(4, candidate.Length - 4);
                candidate = candidate.TrimStart(' ');

                string pageBlockPrefix = "Page ";
                if (candidate.StartsWith("["))
                {
                    candidate = candidate.TrimStart('[');
                    pageBlockPrefix = pageBlockPrefix + "[";
                }

                if (candidate.StartsWith(pageName))
                {
                    subset.Append(pageBlockPrefix);
                    subset.Append(candidate);
                    subset.Append("End-Page\r\n\r\n");
                }
            }

            string fieldCode = GetCheckCodeSubset(page.Fields.Names, checkCodeSuperset);

            subset.Append(fieldCode);
            string pageCheckCode = subset.ToString();

            return pageCheckCode; 
        }

        string GetCheckCodeSubset(List<string> fieldNames, string checkCode)
        {
            StringBuilder subset = new StringBuilder();
            List<string> endFieldFragments = new List<string>();
            string candidate = string.Empty;
            int index = 0;

            string searchCheckCode = checkCode.ToUpperInvariant();

            bool ContainsField = searchCheckCode.Contains("END-FIELD");

            while (searchCheckCode.Length > 0 && ContainsField)
            {
                if (searchCheckCode.Contains("END-FIELD"))
                {
                    searchCheckCode = searchCheckCode.Substring(0, searchCheckCode.LastIndexOf("END-FIELD"));
                }
                else
                {
                    break;
                }

                index = searchCheckCode.LastIndexOf("\nFIELD ");
                index = index == -1 ? searchCheckCode.LastIndexOf(" FIELD ") : index;
                index = index == -1 ? searchCheckCode.LastIndexOf("FIELD ") : index;

                candidate = checkCode.Substring(index, searchCheckCode.Length - index);
                candidate = candidate.TrimStart(new char[] { '\r', '\n' });
                candidate = candidate.Substring(5, candidate.Length - 5);
                candidate = candidate.TrimStart(' ');

                string fieldBlockPrefix = "Field ";
                if (candidate.StartsWith("["))
                {
                    candidate = candidate.TrimStart('[');
                    fieldBlockPrefix = fieldBlockPrefix + "[";
                }

                foreach(string fieldName in fieldNames)
                {
                    if (candidate.StartsWith(fieldName))
                    {
                        candidate = string.Format("{0}{1}End-Field\r\n\r\n", fieldBlockPrefix, candidate);
                        endFieldFragments.Add(candidate);
                    }
                }
            }

            string[] fragments = new string[endFieldFragments.Count]; 
            endFieldFragments.CopyTo(fragments);

            for (int i = fragments.Length - 1; i >= 0; i--)
            {
                subset.Append(fragments[i]);
            }

            string fieldBlocks = subset.ToString(); 
            return fieldBlocks;
        }

        string GetCheckCodeSubset(SortedDictionary<IFieldControl, Point> selectedFieldControls, string checkCode)
        {
            string subset = string.Empty;
            List<string> fieldNames = new List<string>();

            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (fieldNames.Contains(kvp.Key.Field.Name) == false)
                { 
                    fieldNames.Add(kvp.Key.Field.Name);
                }
            }

            subset = GetCheckCodeSubset(fieldNames, checkCode);
            return subset;
        }

        public void CreateSelectedFieldsTemplate(string nameWithSubfolders, SortedDictionary<IFieldControl, Point> selectedFieldControls)
        {
            int minOver = int.MaxValue, minDown = int.MaxValue, maxOver = int.MinValue, maxDown = int.MinValue;
            bool messageBoxHasBeenDisplayed = false;

            System.Xml.XmlWriter writer = CreateWriter(nameWithSubfolders, "Fields");
            writer.WriteStartDocument();
            writer.WriteStartElement("Template");
            writer.WriteAttributeString("Level", "Field");
            writer.WriteStartElement("Project");
            writer.WriteStartElement("View");

            string fieldCode = GetCheckCodeSubset(selectedFieldControls, view.CheckCode);

            writer.WriteAttributeString("CheckCode", fieldCode);
            writer.WriteStartElement("Page");

            DataTable fieldsOnPageTable = page.GetMetadata().GetFieldsOnPageAsDataTable(page.Id);
            DataTable selectedFieldsTable = fieldsOnPageTable.Clone();
            
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                int dim = ((Control)kvp.Key).Location.X;
                if (minOver >= dim) minOver = dim;

                dim = ((Control)kvp.Key).Location.Y;
                if (minDown >= dim) minDown = dim;

                dim = ((Control)kvp.Key).Location.X + ((Control)kvp.Key).Width;
                if (maxOver <= dim) maxOver = dim;

                dim = ((Control)kvp.Key).Location.Y + ((Control)kvp.Key).Height;
                if (maxDown <= dim) maxDown = dim;
                
                if ((kvp.Key.Field is FieldWithSeparatePrompt && kvp.Key is DragableLabel) || kvp.Key.Field is FieldWithoutSeparatePrompt)
                {
                    if ((kvp.Key.Field is ImageField) && messageBoxHasBeenDisplayed == false)
                    {
                        const string message = "Note: Relate buttons and images fields are currently not supported via templates.";
                        const string caption = "Note: Relate and Image Field Template Support";
                        MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        messageBoxHasBeenDisplayed = true;
                    }
                    else
                    {
                        DataRow[] fieldCandidate = fieldsOnPageTable.Select("UniqueId = '" + kvp.Key.Field.UniqueId.ToString() + "'");
                        
                        if (fieldCandidate.Length > 0)
                        {
                            double percentDifferenceTop = kvp.Value.Y / (double)mediator.Canvas.PagePanel.Height;
                            double percentDifferenceLeft = kvp.Value.X / (double)mediator.Canvas.PagePanel.Width;

                            if (fieldCandidate[0]["PromptTopPositionPercentage"] != DBNull.Value)
                            {
                                fieldCandidate[0]["ControlTopPositionPercentage"] = percentDifferenceTop 
                                    + (double)fieldCandidate[0]["ControlTopPositionPercentage"] 
                                    - (double)fieldCandidate[0]["PromptTopPositionPercentage"];
                                
                                fieldCandidate[0]["ControlLeftPositionPercentage"] = percentDifferenceLeft 
                                    + (double)fieldCandidate[0]["ControlLeftPositionPercentage"] 
                                    - (double)fieldCandidate[0]["PromptLeftPositionPercentage"];

                                fieldCandidate[0]["PromptTopPositionPercentage"] = percentDifferenceTop;
                                
                                fieldCandidate[0]["PromptLeftPositionPercentage"] = percentDifferenceLeft;
                            }
                            else
                            {
                                fieldCandidate[0]["ControlTopPositionPercentage"] = percentDifferenceTop;
                                fieldCandidate[0]["ControlLeftPositionPercentage"] = percentDifferenceLeft;
                            }
                            
                            selectedFieldsTable.ImportRow(fieldCandidate[0]);
                        }
                    }
                }
            }

            DataRow[] rows = EnhancedSort(selectedFieldsTable);

            foreach (DataRow row in rows)
            {
                writer.WriteStartElement("Field");

                foreach (DataColumn column in selectedFieldsTable.Columns)
                {
                    writer.WriteAttributeString(column.ColumnName, row[column.ColumnName].ToString());
                }
                writer.WriteEndElement();
            }
            
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            AddCodeTableTemplates(selectedFieldControls, writer);
            AddGridColumnTemplate(selectedFieldControls, writer);

            if (minOver < int.MaxValue && minDown < int.MaxValue && maxOver > int.MinValue && maxDown > int.MinValue)
            {
                writer.WriteStartElement("FieldFootprint");
                writer.WriteAttributeString("Width", (maxOver - minOver).ToString());
                writer.WriteAttributeString("Height", (maxDown - minDown).ToString());
                writer.WriteEndElement();
            }
               
            writer.WriteEndElement();
            writer.Close();
        }

        public void CreateTemplate(string nameWithSubfolders, SortedDictionary<IFieldControl, Point> selectedFieldControls, Page page)
        {
            CreateSelectedFieldsTemplate(nameWithSubfolders, selectedFieldControls);
        }

        #endregion
        
        #region Private Template Creation Methods

        private void AddGridColumnTemplate(SortedDictionary<IFieldControl, Point> selectedFieldControls, System.Xml.XmlWriter writer)
        {
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (false == kvp.Key is DragableLabel)
                {
                    if (kvp.Key.Field is GridField)
                    {
                        GridField gridField = (GridField)kvp.Key.Field;
                        DataTable gridTable = metadata.GetGridColumns(gridField.Id);

                        if (gridTable != null)
                        {
                            gridTable.TableName = gridField.Name;
                            InsertGridTable(gridTable, writer);

                            foreach (GridColumnBase column in gridField.Columns)
                            {
                                if (column is TableBasedDropDownColumn)
                                {
                                    if (column is DDLColumnOfCommentLegal || column is DDLColumnOfLegalValues)
                                    InsertSourceTable(((TableBasedDropDownColumn)column).GetSourceData(), writer);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InsertGridTable(DataTable table, System.Xml.XmlWriter writer)
        {
            if (table != null)
            {
                writer.WriteStartElement("GridTable");
                writer.WriteAttributeString("TableName", table.TableName);

                foreach (DataRow row in table.Rows)
                {
                    writer.WriteStartElement("Item");

                    foreach (DataColumn column in table.Columns)
                    {
                        writer.WriteAttributeString(column.ColumnName, row[column.ColumnName].ToString());
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        private void AddGridColumnTemplate(System.Xml.XmlWriter writer)
        {
            foreach (KeyValuePair<int,string> kvp in _gridFieldIds)
            {
                DataTable gridTable = metadata.GetGridColumns(kvp.Key);
                
                if (gridTable != null)
                {
                    gridTable.TableName = kvp.Value;
                    InsertGridTable(gridTable, writer);
                }
            }
        }

        private void AddCodeTableTemplates(System.Xml.XmlWriter writer)
        {
            foreach (string sourceTableName in _sourceTableNames)
            {
                string pattern = " - ";

                if (sourceTableName.Contains(pattern) == false)
                {
                    DataTable sourceTable = metadata.GetCodeTableData(sourceTableName);

                    if (sourceTable != null)
                    {
                        sourceTable.TableName = sourceTableName;
                        InsertSourceTable(sourceTable, writer);
                    }

                }
            }
        }

        private void AddBackgroundTableTemplate(System.Xml.XmlWriter writer)
        {
            DataTable table = metadata.GetBackgroundData();

            if (table != null)
            {
                InsertTable(table, "Backgrounds", writer);
            }
        }


        private void InsertTable(DataTable table, String tableName, System.Xml.XmlWriter writer)
        {
            if (table != null)
            {
                writer.WriteStartElement(tableName);
                writer.WriteAttributeString("TableName", tableName);

                foreach (DataRow row in table.Rows)
                {
                    writer.WriteStartElement("Item");

                    foreach (DataColumn column in table.Columns)
                    {
                        writer.WriteAttributeString(column.ColumnName, row[column.ColumnName].ToString());
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        private void AddCodeTableTemplates(SortedDictionary<IFieldControl, Point> selectedFieldControls, System.Xml.XmlWriter writer)
        {
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if ((kvp.Key.Field is FieldWithSeparatePrompt && kvp.Key is DragableLabel) || kvp.Key.Field is FieldWithoutSeparatePrompt)
                {
                    if (kvp.Key.Field is TableBasedDropDownField)
                    {
                        TableBasedDropDownField field = ((TableBasedDropDownField)kvp.Key.Field);

                        try
                        {
                            DataTable table = metadata.GetCodeTableData(field.SourceTableName);

                            if (table != null)
                            {
                                table.TableName = field.SourceTableName;
                                InsertSourceTable(table, writer);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private void InsertSourceTable(DataTable table, System.Xml.XmlWriter writer)
        {
            if (table != null)
            {
                writer.WriteStartElement("SourceTable");
                writer.WriteAttributeString("TableName", table.TableName);

                foreach (DataRow row in table.Rows)
                {
                    writer.WriteStartElement("Item");

                    foreach (DataColumn column in table.Columns)
                    {
                        string columnName = column.ColumnName;
                        columnName = columnName.Replace(" ", "__space__");
                        writer.WriteAttributeString(columnName, row[column.ColumnName].ToString());
                    }
                    writer.WriteEndElement();
                }
                
                writer.WriteEndElement();
            }
        }

        private XmlWriter CreateWriter(string nameWithSubfolders, string nodePath)
        {
            System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();
            IMetadataProvider metadata = view.GetMetadata();

            string templatesFolderPath = GetTemplatePath(nodePath);

            string xmlFullPath = System.IO.Path.Combine(templatesFolderPath, nameWithSubfolders) + ".xml";

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(xmlFullPath)) == false)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(xmlFullPath));
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = false;
            XmlWriter writer = XmlWriter.Create(xmlFullPath, settings);

            return writer;
        }

        #endregion

        #region Public Field Creation Methods

        public void CreateFromTemplate(TemplateNode node, Point location)
        {
            string templatePath = GetTemplatePath(node);
            _fieldDrop = true;

            _fieldDropLocaton = location;
            
            _templateLevel = GetTemplateLevel(node);
            CreateFromTemplate(templatePath);
        }

        public void CreateFromTemplate(TemplateNode node, int dropOnNodePosition)
        {
            string templatePath = GetTemplatePath(node);
            _fieldDrop = true;

            _dropOnNodePosition = dropOnNodePosition;
            
            _templateLevel = GetTemplateLevel(node);
            CreateFromTemplate(templatePath);
        }

        public void CreateFromTemplate(string templatePath)
        {
            ArrayList codeTableAssociations = new ArrayList();
            Page firstPageCreated = null;
            if (templatePath == null) return;

            _templateLevel = GetTemplateLevel(templatePath);
            mediator.Canvas.HideUpdateStart("Loading fields from template...");
            bool isnewview = false;
            System.Text.StringBuilder output = new System.Text.StringBuilder();
            DataTable metadataSchema;
            _sourceTableRenames.Clear();
            pageIdViewNamePairs.Clear();
            viewIdViewNamePairs.Clear();            
            
            using (XmlReader reader = XmlReader.Create(templatePath))
            {
                while (reader.ReadToFollowing("View"))
                {
                    do
                    {
                        int viewId;

                        if (reader["ViewId"] != null && int.TryParse(reader["ViewId"].ToString(), out viewId))
                        {
                            viewIdViewNamePairs.Add(int.Parse(reader["ViewId"]), reader["Name"]);
                        }

                    } while (reader.ReadToNextSibling("View"));
                    break;
                }
                
                while (reader.ReadToFollowing("SourceTable"))
                {
                    do
                    {
                        CreateSourceTable(reader);
                    
                    } while (reader.ReadToNextSibling("SourceTable"));
                    break;
                }

                CreateBackgroundsTable(reader);
            }

            if (_templateLevel == Enums.TemplateLevel.Project || _templateLevel == Enums.TemplateLevel.Form)
            {
                bool cancel; 
                CreateViews(templatePath, out cancel, out isnewview);               
                    isnewview = true;
                if (cancel == true)
                    return;
            }
            else
            {
                ConcatCheckCode(templatePath);
            }
            
            using (XmlReader reader = XmlReader.Create(templatePath))
            {
                while (reader.ReadToFollowing("Page"))
                {
                    Page page = null;
                    XmlReader fieldSubtree = reader.ReadSubtree();

                    if (reader.HasAttributes)
                    {
                        mediator.Canvas.UpdateHidePanel("Creating Page...");
                        View newView;
                        page = CreatePage(reader, isnewview, out newView);
                        if (newView == null)
                        {
                            mediator.Canvas.HideUpdateEnd();
                            return;
                        }
                        int pagePosition = _dropOnNodePosition == -1 ? newView.Pages.Count : _dropOnNodePosition;
                        page = newView.CreatePage(page.Name, pagePosition);

                        if (_templateLevel == Enums.TemplateLevel.Form)
                        {
                            mediator.ProjectExplorer.AddPageNode(page, newView, true);
                        }
                        else
                        {
                            mediator.ProjectExplorer.AddPageNode(page, newView);
                        }

                    }
                    else
                    {
                        page = mediator.ProjectExplorer.currentPage;
                    }
                    
                    if (firstPageCreated == null)
                    {
                        firstPageCreated = page;
                    }      
                    
                    metadataSchema = metadata.GetMetaFieldsSchema(mediator.ProjectExplorer.currentPage.GetView().Id);

                    int fieldIdFromTemplate = int.MinValue;

                    while (fieldSubtree.ReadToFollowing("Field"))
                    {
                        do
                        {
                            

                            Field field = CreateFields(fieldSubtree, metadataSchema, page, out fieldIdFromTemplate);
                            
                            if (field == null) continue;

                           if (field.Id != fieldIdFromTemplate && !_templateFieldId_dbFieldId.ContainsKey(fieldIdFromTemplate))
                                
                            {
                                _templateFieldId_dbFieldId.Add(fieldIdFromTemplate, field.Id);
                            }

                            if (field.FieldType == MetaFieldType.Codes)
                            {
                                _codeFields.Add(field as DDLFieldOfCodes);
                                KeyValuePair kvp = new KeyValuePair(field.Name, ((Epi.Fields.DDLFieldOfCodes)(field)).AssociatedFieldInformation);
                                codeTableAssociations.Add(kvp);
                            }

                            if (field.FieldType == MetaFieldType.Mirror)
                            {
                                _mirrorFields.Add(field as MirrorField);
                            }

                        }
                        while (fieldSubtree.ReadToNextSibling("Field"));
                        break;
                    }
                }               
            }

            CreateGridTables(templatePath);

            UpdateCodeTableAssociations(templatePath, codeTableAssociations);
            UpdateMirrorFieldAssociations(templatePath);

            mediator.SetZeeOrderOfGroups();
            ArrayList controlArrayList = new ArrayList(mediator.SelectedFieldControls.Keys);
            mediator.SelectedFieldControls.Clear(); 
            mediator.Canvas.HideUpdateEnd();

            if (firstPageCreated != null)
            {
                mediator.ProjectExplorer.currentPage = firstPageCreated;
                mediator.ProjectExplorer.SelectPage( mediator.ProjectExplorer.currentPage);
                mediator.LoadPage(mediator.ProjectExplorer.currentPage);
            }
            mediator.OnViewFieldTabsChanged();
        }
        
        #endregion

        #region Private Field Creation Methods

        private void UpdateMirrorFieldAssociations(string templatePath)
        {
            foreach (MirrorField mirrorField in _mirrorFields)
            {
                int originalSourceFieldId = mirrorField.SourceFieldId;

                if (_templateFieldId_dbFieldId.ContainsKey(originalSourceFieldId))
                {
                    int newSourceFieldId = _templateFieldId_dbFieldId[originalSourceFieldId];

                    if (newSourceFieldId != null)
                    {
                        mirrorField.SourceFieldId = _templateFieldId_dbFieldId[originalSourceFieldId];
                        metadata.UpdateField(mirrorField);
                    }
                }
            }
        }

        private void UpdateCodeTableAssociations(string templatePath, ArrayList codeTableAssociations)
        {
            foreach (DDLFieldOfCodes codesField in _codeFields)
            {
                string originalRelateCondition = codesField.AssociatedFieldInformation;
                string[] sourceTableColumnName_targetFieldId = originalRelateCondition.Split(new char[] { ',' });

                codesField.AssociatedFieldInformation = string.Empty;

                foreach (string pair in sourceTableColumnName_targetFieldId)
                {
                    if (String.IsNullOrEmpty(pair) == false)
                    { 
                        string sourceTableColumnName = pair.Substring(0, pair.IndexOf(':'));
                        int idTemplate = int.Parse(pair.Substring(pair.IndexOf(':') + 1));

                        string select = string.Format("{0} = {1}", FieldRecordColumn.IdTemplate.ToString(), idTemplate);
                        DataRow[] rows = _fieldRecord.Select(select);

                        if (rows.Length > 0)
                        {
                            int idCreated = (int)rows[0][FieldRecordColumn.IdCreated.ToString()];

                            if (codesField.AssociatedFieldInformation.Length > 0)
                            {
                                codesField.AssociatedFieldInformation += ",";
                            }

                            codesField.AssociatedFieldInformation += string.Format("{0}:{1}", sourceTableColumnName, idCreated);
                        }
                    }
                }

                metadata.UpdateField(codesField);
            }
        }

        private void ConcatCheckCode(string templatePath)
        {
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(templatePath))
            {
                while (reader.ReadToFollowing("View"))
                {
                    if (reader.GetAttribute("CheckCode") != null)
                    {
                        mediator.Canvas.UpdateHidePanel("Adding CheckCode...");
                        this.view.CheckCode += "\n\r" + reader.GetAttribute("CheckCode");
                        this.view.SaveToDb();
                    }
                }
            }
        }
        
        private void AddViewAttributeToView(string name, string value, View newView)
        {
            switch (name)
            {
                case ColumnNames.CHECK_CODE:
                    newView.CheckCode += "\n\r" + (string)value;
                    break;
                case ColumnNames.PAGE_HEIGHT:
                    newView.PageHeight = int.Parse(value);
                    break;
                case ColumnNames.PAGE_WIDTH:
                    newView.PageWidth = int.Parse(value);
                    break;
                case ColumnNames.NAME:
                    newView.Name = (string)value;
                    break;
                case ColumnNames.VIEW_ID:
                    newView.Id = int.Parse(value);
                    break;
                case ColumnNames.IS_RELATED_VIEW:
                    newView.IsRelatedView = bool.Parse(value);
                    break;
                case ColumnNames.PAGE_ORIENTATION:
                    newView.PageOrientation = (string)value;
                    break;
                case ColumnNames.PAGE_LABEL_ALIGN:
                    newView.PageLabelAlign = (string)value;
                    break;
            }
        }

        private void AddPageAttributeToPage(string name, string value, Page newPage)
        {
            switch (name)
            {
                case ColumnNames.PAGE_ID:
                    newPage.Id = int.Parse(value);
                    break;
                case ColumnNames.NAME:
                    newPage.Name = (string)value;
                    break;
                case ColumnNames.POSITION:
                    newPage.Position = int.Parse(value);
                    break;
                case ColumnNames.BACKGROUND_ID:
                    newPage.BackgroundId = int.Parse(value);
                    break;
            }
        }

        void CreateGridTables(string templatePath)
        {
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(templatePath))
            {
                while (reader.ReadToFollowing("GridTable"))
                {
                    do
                    {
                        ArrayList names = new ArrayList();
                        reader.MoveToAttribute("TableName");
                        string codeTableName = reader.Value.ToString();

                        DataTable fromXml = new DataTable(codeTableName);
                        DataRow rowFromXml;

                        reader.ReadToFollowing("Item");

                        do
                        {
                            if (reader.MoveToFirstAttribute())
                            {
                                if (fromXml.Columns.Contains(reader.Name) == false)
                                {
                                    fromXml.Columns.Add(reader.Name, System.Type.GetType("System.String"));
                                }

                                rowFromXml = fromXml.NewRow();
                                rowFromXml[reader.Name] = reader.Value;

                                while (reader.MoveToNextAttribute())
                                {
                                    if (fromXml.Columns.Contains(reader.Name) == false)
                                    {
                                        fromXml.Columns.Add(reader.Name, System.Type.GetType("System.String"));
                                        DataRow tempRow = fromXml.NewRow();
                                        tempRow.ItemArray = rowFromXml.ItemArray;
                                    }

                                    rowFromXml[reader.Name] = reader.Value;
                                }

                                foreach (object obj in rowFromXml.ItemArray)
                                {
                                }

                                fromXml.Rows.Add(rowFromXml);

                                int fieldType = int.Parse((string)rowFromXml["FieldTypeId"]);
                                int fieldId = int.Parse((string)rowFromXml["FieldId"]);
                                
                                if(_templateFieldId_dbFieldId.ContainsKey(fieldId))
                                {
                                    rowFromXml["FieldId"] = _templateFieldId_dbFieldId[fieldId];
                                }

                                string columnName = rowFromXml["Name"].ToString().ToLowerInvariant();

                                if ((   columnName.Equals("uniquerowid") ||
                                        columnName.Equals("recstatus") ||
                                        columnName.Equals("fkey") ||
                                        columnName.Equals("globalrecordid")) == false)
                                { 
                                    mediator.Project.Metadata.AddGridColumn(rowFromXml);
                                }
                            }

                        } while (reader.ReadToNextSibling("Item"));


                    } while (reader.ReadToNextSibling("GridTable"));
                    break;
                }
            }
        }

        private void CreateSourceTable(System.Xml.XmlReader reader)
        {
            string[] columnNames;
            ArrayList names = new ArrayList();
            reader.MoveToAttribute("TableName");
            string codeTableName = reader.Value.ToString();

            DataTable fromXml = new DataTable(codeTableName);
            DataRow rowFromXml;

            reader.ReadToFollowing("Item");

            do
            {
                if (reader.MoveToFirstAttribute())
                {
                    string columnName = reader.Name.Replace("__space__", " ");

                    if (fromXml.Columns.Contains(columnName) == false)
                    {
                        fromXml.Columns.Add(columnName, System.Type.GetType("System.String"));
                    }

                    rowFromXml = fromXml.NewRow();
                    rowFromXml[columnName] = reader.Value;

                    while (reader.MoveToNextAttribute())
                    {
                        columnName = reader.Name.Replace("__space__", " ");
                        if (fromXml.Columns.Contains(columnName) == false)
                        {
                            
                            fromXml.Columns.Add(columnName, System.Type.GetType("System.String"));
                            DataRow tempRow = fromXml.NewRow();
                            tempRow.ItemArray = rowFromXml.ItemArray;
                        }

                        rowFromXml[columnName] = reader.Value;
                    }

                    fromXml.Rows.Add(rowFromXml);
                }
            } 
            while (reader.ReadToNextSibling("Item"));

            if (mediator.Project.Metadata.TableExists(codeTableName))
            {
                DataTable existing = mediator.Project.Metadata.GetCodeTableData(codeTableName);

                if (TablesAreDifferent(existing, fromXml))
                {
                    foreach (DataColumn column in fromXml.Columns)
                    {
                        names.Add(column.ColumnName);
                    }
                    columnNames = (string[])names.ToArray(typeof(string));

                    DialogResult replaceWithTemplate = MsgBox.ShowQuestion("A code table with the following name already exists in the database: " + existing + ".  Replace code table with the code table defined in the template?" );
                    
                    if (replaceWithTemplate == DialogResult.Yes)
                    {
                        mediator.Project.Metadata.DeleteCodeTable(codeTableName);
                        mediator.Project.Metadata.CreateCodeTable(codeTableName, columnNames);
                        mediator.Project.Metadata.SaveCodeTableData(fromXml, codeTableName, columnNames);
                    }
                    else
                    {
                        string newCodeTableName = codeTableName;

                        DataSets.TableSchema.TablesDataTable tables = mediator.Project.Metadata.GetCodeTableList();

                        int arbitraryMax = 2048;

                        for (int nameExtension = 1; nameExtension < arbitraryMax; nameExtension++)
                        {
                            foreach (DataRow row in tables)
                            {
                                if (newCodeTableName.ToLowerInvariant() == ((string)row[ColumnNames.TABLE_NAME]).ToLowerInvariant())
                                {
                                    newCodeTableName = codeTableName + nameExtension.ToString();
                                    break;
                                }
                            }

                            if (newCodeTableName != codeTableName + nameExtension.ToString())
                            {
                                break;
                            }
                        }

                        _sourceTableRenames.Add(codeTableName, newCodeTableName);
                        mediator.Project.Metadata.CreateCodeTable(newCodeTableName, columnNames);
                        mediator.Project.Metadata.SaveCodeTableData(fromXml, newCodeTableName, columnNames);
                    }
                }
            }
            else
            {
                foreach (DataColumn column in fromXml.Columns)
                {
                    string columnName = column.ColumnName.Replace("__space__", " ");
                    names.Add(columnName);
                }
                columnNames = (string[])names.ToArray(typeof(string));

                mediator.Project.Metadata.CreateCodeTable(codeTableName, columnNames);
                mediator.Project.Metadata.SaveCodeTableData(fromXml, codeTableName, columnNames);
            }
        }

        private void CreateBackgroundsTable(System.Xml.XmlReader reader)
        {
            string[] columnNames;
            ArrayList names = new ArrayList();
            reader.MoveToAttribute("TableName");
            string tableName = reader.Value.ToString();

            DataTable fromXml = new DataTable(tableName);
            DataRow rowFromXml;

            reader.ReadToFollowing("Item");

            do
            {
                if (reader.MoveToFirstAttribute())
                {
                    string columnName = reader.Name;

                    if (fromXml.Columns.Contains(columnName) == false)
                    {
                        fromXml.Columns.Add(columnName, System.Type.GetType("System.String"));
                    }

                    rowFromXml = fromXml.NewRow();
                    rowFromXml[columnName] = reader.Value;

                    while (reader.MoveToNextAttribute())
                    {
                        columnName = reader.Name;
                        if (fromXml.Columns.Contains(columnName) == false)
                        {
                            fromXml.Columns.Add(columnName, System.Type.GetType("System.String"));
                            DataRow tempRow = fromXml.NewRow();
                            tempRow.ItemArray = rowFromXml.ItemArray;
                        }

                        rowFromXml[columnName] = reader.Value;
                    }

                    fromXml.Rows.Add(rowFromXml);
                }
            }
            while (reader.ReadToNextSibling("Item"));

            //if (mediator.Project.Metadata.TableExists(tableName))
            //{
            //    DataTable existing = mediator.Project.Metadata.GetCodeTableData(tableName);

            //    if (TablesAreDifferent(existing, fromXml))
            //    {
            //        foreach (DataColumn column in fromXml.Columns)
            //        {
            //            names.Add(column.ColumnName);
            //        }
            //        columnNames = (string[])names.ToArray(typeof(string));

            //        DialogResult replaceWithTemplate = MsgBox.ShowQuestion("A code table with that name already exists in the database. Replace code table with the code table defined in the template?");
            //        if (replaceWithTemplate == DialogResult.Yes)
            //        {
            //            mediator.Project.Metadata.DeleteCodeTable(tableName);
            //            mediator.Project.Metadata.CreateCodeTable(tableName, columnNames);
            //            mediator.Project.Metadata.SaveCodeTableData(fromXml, tableName, columnNames);
            //        }
            //        else
            //        {
            //            string newCodeTableName = tableName;

            //            DataSets.TableSchema.TablesDataTable tables = mediator.Project.Metadata.GetCodeTableList();

            //            int arbitraryMax = 2048;

            //            for (int nameExtension = 1; nameExtension < arbitraryMax; nameExtension++)
            //            {
            //                foreach (DataRow row in tables)
            //                {
            //                    if (newCodeTableName.ToLowerInvariant() == ((string)row[ColumnNames.TABLE_NAME]).ToLowerInvariant())
            //                    {
            //                        newCodeTableName = tableName + nameExtension.ToString();
            //                        break;
            //                    }
            //                }

            //                if (newCodeTableName != tableName + nameExtension.ToString())
            //                {
            //                    break;
            //                }
            //            }

            //            _sourceTableRenames.Add(tableName, newCodeTableName);
            //            mediator.Project.Metadata.CreateCodeTable(newCodeTableName, columnNames);
            //            mediator.Project.Metadata.SaveCodeTableData(fromXml, newCodeTableName, columnNames);
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (DataColumn column in fromXml.Columns)
            //    {
            //        string columnName = column.ColumnName.Replace("__space__", " ");
            //        names.Add(columnName);
            //    }
            //    columnNames = (string[])names.ToArray(typeof(string));

            //    mediator.Project.Metadata.CreateCodeTable(tableName, columnNames);
            //    mediator.Project.Metadata.SaveCodeTableData(fromXml, tableName, columnNames);
            //}
        }

        private void CreateViews(string templatePath, out bool cancel, out bool isnewview)
        {
            View newView = this.view; isnewview = false;
            mediator.Canvas.UpdateHidePanel("Creating View...");
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(templatePath))
            {
                while (reader.ReadToFollowing("View"))
                {
                    do
                    {
                        XmlReader viewSubTree = reader.ReadSubtree();
                        
                        if (reader.HasAttributes)
                        {
                            if (reader.GetAttribute("ViewId") != null)
                            {
                                newView = CreateView(reader);

                                mediator.Canvas.UpdateHidePanel(string.Format("Creating View - {0}", newView.Name));
                                string viewNamePlaceHolder = newView.Name;
                                int viewNameIncrement = 1;
                                string viewNameCandidate = viewNamePlaceHolder;

                                while (mediator.Project.Views.Contains(newView))
                                {
                                    viewNameIncrement++;
                                    viewNameCandidate = string.Format("{0}_{1}", viewNamePlaceHolder, viewNameIncrement);
                                    newView.Name = viewNameCandidate;
                                }

                                newView.Name = viewNamePlaceHolder;
                                if (mediator.Project.Views.Contains(newView))
                                {
                                    ShowTemplateFormDialog(viewNameCandidate, ref newView);
                                    isnewview = true;
                                }
                            }
                        }

                        if (newView.Name == string.Empty)
                        {
                            cancel = true; isnewview = false;
                            mediator.Canvas.HideUpdateEnd();
                            return;
                        }

                        metadata = newView.GetMetadata();

                        if (mediator.Project.Views.Contains(newView) == false)
                        {
                            metadata.InsertView(newView);
                            mediator.Project.Views.Add(newView);
                            mediator.ProjectExplorer.AddView(newView);
                            mediator.Project.LoadViews();
                            mediator.ProjectExplorer.SelectView(newView.Name);
                        }

                        while (viewSubTree.ReadToFollowing("Page"))
                        {
                            int pageId = -1;

                            do
                            {
                                if (viewSubTree.MoveToFirstAttribute())
                                {
                                    if (viewSubTree.Name == ColumnNames.PAGE_ID)
                                    {
                                        int.TryParse(reader.Value, out pageId);
                                        if (pageIdViewNamePairs.ContainsKey(pageId) == false)
                                        {
                                            pageIdViewNamePairs.Add(pageId, newView.Name);
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                            while (viewSubTree.ReadToNextSibling("Page"));
                        }

                    } while (reader.ReadToNextSibling("View"));
                }
            }
            cancel = false;
        }

        private void ShowTemplateFormDialog(string viewNameCandidate, ref View newView)
        {
            Dialogs.RenameFormFromTemplatePageDialog dialog = new Dialogs.RenameFormFromTemplatePageDialog();
            dialog.FormName = viewNameCandidate;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                newView.Name = dialog.FormName;
            }
            else
            {
                newView.Name = "";
                return;
            }
        }

        private View CreateView(System.Xml.XmlReader reader)
        {
            View newView = new View(mediator.Project);

            if (reader.MoveToFirstAttribute())
            {
                AddViewAttributeToView(reader.Name, reader.Value, newView);

                while (reader.MoveToNextAttribute())
                {
                    AddViewAttributeToView(reader.Name, reader.Value, newView);
                }
            }
            return newView;
        }

        private Page CreatePage(System.Xml.XmlReader reader, bool isnewview, out View view)
        {
            Page newPage = new Page();

            if (reader.MoveToFirstAttribute())
            {
                AddPageAttributeToPage(reader.Name, reader.Value, newPage);

                while (reader.MoveToNextAttribute())
                {
                    if (reader.Name == "Name")
                    {
                        if (!isnewview)
                        {
                            string _name;
                            ShowRenamePageDialog(reader.Value, out _name);
                            if (_name != string.Empty)
                                newPage.Name = _name;
                            else
                            {
                                view = null;
                                return newPage;
                            }
                        }
                        else
                            AddPageAttributeToPage(reader.Name, reader.Value, newPage);
                    }
                    else
                        AddPageAttributeToPage(reader.Name, reader.Value, newPage);
                }
            }

            if (pageIdViewNamePairs.Count == 0)
            {
                view = this.mediator.ProjectExplorer.currentPage.view;
            }
            else
            {
                view = mediator.Project.Views[pageIdViewNamePairs[newPage.Id]];
            }

            newPage.Id = 0;
            newPage.view = view;
            return newPage;
        }

        private void ShowRenamePageDialog(string readervalue, out string newpagename)
        {
            readervalue = "Page " + (view.Pages.Count + 1).ToString();
            Dialogs.RenamePageDialog dialog = new Dialogs.RenamePageDialog(mediator.Canvas.MainForm, view);
            dialog.PageName = readervalue;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                newpagename = dialog.PageName;
            }
            else
            {
                newpagename = "";
            }
        }

        private void CopyReaderValueToRow(System.Xml.XmlReader reader, DataTable metadataSchema, DataRow row)
        {
            if (metadataSchema.Columns.Contains(reader.Name) && reader.Value != string.Empty)
            {
                switch (metadataSchema.Columns[reader.Name].DataType.Name.ToLowerInvariant())
                {
                    case "boolean":
                        row[reader.Name] = bool.Parse(reader.Value);
                        break;
                    case "string":
                    case "guid":
                        row[reader.Name] = reader.Value;
                        break;
                    case "decimal":
                    case "double":
                        double result = 0.0;
                        ni.NumberDecimalSeparator = ".";
                        if (double.TryParse(reader.Value, System.Globalization.NumberStyles.Float, ni, out result))
                        {
                            row[reader.Name] = result;
                            break;
                        }
                        else
                        {
                            ni.NumberDecimalSeparator = ",";
                            if (double.TryParse(reader.Value, System.Globalization.NumberStyles.Float, ni, out result))
                            {
                                row[reader.Name] = result;
                                break;
                            }
                        }
                        break;
                    default:
                        row[reader.Name] = int.Parse(reader.Value);
                        break;
                }
            }
        }

        private Field CreateFields(System.Xml.XmlReader reader, DataTable metadataSchema, Page page, out int fieldIdFromTemplate)
        {
            DataRow row;
            Field field = null;
            MetaFieldType returnMetaFieldType = MetaFieldType.Text;
            fieldIdFromTemplate = int.MinValue;
            String relatedViewName = string.Empty;

            if (reader.MoveToFirstAttribute())
            {
                row = metadataSchema.NewRow();
                relatedViewName = string.Empty;
                CopyReaderValueToRow(reader, metadataSchema, row);
                
                while (reader.MoveToNextAttribute())
                {
                    CopyReaderValueToRow(reader, metadataSchema, row);

                    if (reader.Name.ToLowerInvariant() == "fieldtypeid")
                    {
                        field = page.CreateField(((MetaFieldType)Int32.Parse(reader.Value)));
                    }

                    if (reader.Name.ToLowerInvariant() == "relatedviewname")
                    {
                        relatedViewName = reader.Value.ToString();
                    }
                }

                fieldIdFromTemplate = (int)row["FieldId"];
                field.Name = (string)row["Name"];
                mediator.Canvas.UpdateHidePanel(string.Format("Creating {0} field.", field.Name));

                double controlLeftPercentage = (double)row["ControlLeftPositionPercentage"];
                double controlTopPercentage = (double)row["ControlTopPositionPercentage"];
                double promptLeftPercentage = row["PromptLeftPositionPercentage"] != DBNull.Value ? (double)row["PromptLeftPositionPercentage"] : double.NaN;
                double promptTopPercentage = row["PromptTopPositionPercentage"] != DBNull.Value ? (double)row["PromptTopPositionPercentage"] : double.NaN;

                if(_fieldDrop)
                {
                    double mouseOffsetOver = _fieldDropLocaton.X / (double)mediator.Canvas.PagePanel.Width;
                    double mouseOffsetDown = _fieldDropLocaton.Y / (double)mediator.Canvas.PagePanel.Height;

                    controlLeftPercentage = controlLeftPercentage + mouseOffsetOver;
                    controlTopPercentage = controlTopPercentage + mouseOffsetDown;
                    promptLeftPercentage = promptLeftPercentage + mouseOffsetOver;
                    promptTopPercentage = promptTopPercentage + mouseOffsetDown;
                }

                ((RenderableField)field).PromptText = row["PromptText"] != DBNull.Value ? (string)row["PromptText"] : string.Empty; 
                ((RenderableField)field).ControlFont = new System.Drawing.Font(row["ControlFontFamily"].ToString(), float.Parse(row["ControlFontSize"].ToString()), (FontStyle)System.Enum.Parse(typeof(FontStyle), row["ControlFontStyle"].ToString(), true));
                ((RenderableField)field).HasTabStop = (bool)row["HasTabStop"];
                ((RenderableField)field).TabIndex = double.Parse(row["TabIndex"].ToString());

                ((RenderableField)field).ControlHeightPercentage = (double)row["ControlHeightPercentage"];
                ((RenderableField)field).ControlWidthPercentage = (double)row["ControlWidthPercentage"];

                ((RenderableField)field).ControlLeftPositionPercentage = controlLeftPercentage;
                ((RenderableField)field).ControlTopPositionPercentage = controlTopPercentage;

                ((RenderableField)field).Page = page;

                try
                {
                    string fontFamily = row["PromptFontFamily"].ToString();
                    float fontSize;

                    if(float.TryParse(row["PromptFontSize"].ToString(), out fontSize))
                    {
                        ((RenderableField)field).PromptFont = new System.Drawing.Font(fontFamily, fontSize, (FontStyle)System.Enum.Parse(typeof(FontStyle), row["PromptFontStyle"].ToString(), true));
                    }
                }
                catch { }

                if (field is InputFieldWithoutSeparatePrompt)
                {
                    ((InputFieldWithoutSeparatePrompt)field).ShouldRepeatLast = (bool)row["ShouldRepeatLast"];
                    ((InputFieldWithoutSeparatePrompt)field).IsRequired = (bool)row["IsRequired"];
                    ((InputFieldWithoutSeparatePrompt)field).IsReadOnly = (bool)row["IsReadOnly"];
                }

                if (field is InputFieldWithSeparatePrompt)
                {
                    ((InputFieldWithSeparatePrompt)field).PromptLeftPositionPercentage = promptLeftPercentage;
                    ((InputFieldWithSeparatePrompt)field).PromptTopPositionPercentage = promptTopPercentage;
                    
                    ((InputFieldWithSeparatePrompt)field).ShouldRepeatLast = (bool)row["ShouldRepeatLast"];
                    ((InputFieldWithSeparatePrompt)field).IsRequired = (bool)row["IsRequired"];
                    ((InputFieldWithSeparatePrompt)field).IsReadOnly = (bool)row["IsReadOnly"];

                    if (field is TextField && row.Table.Columns.Contains("IsEncrypted") && row["IsEncrypted"] != DBNull.Value )
                    {
                        ((TextField)field).IsEncrypted = (bool)row["IsEncrypted"];
                    }
                }

                if (field is GridField)
                {
                    ((GridField)field).PromptLeftPositionPercentage = promptLeftPercentage;
                    ((GridField)field).PromptTopPositionPercentage = promptTopPercentage;
                }
  
  
                if (field is ImageField)
                {
                    ((ImageField)field).ShouldRetainImageSize = (bool)row["ShouldRetainImageSize"];
                }

                if (field is TextField)
                {
                    if (row["SourceFieldId"] != System.DBNull.Value)
                    {
                        ((TextField)field).SourceFieldId = (int)row["SourceFieldId"];
                    }

                    if (row["MaxLength"] != System.DBNull.Value)
                    {
                        ((TextField)field).MaxLength = (int)(short)row["MaxLength"];
                    }
                }

                if (field is NumberField)
                {
                    if (row["Lower"] != System.DBNull.Value)
                    {
                        ((NumberField)field).Lower = (string)row["Lower"];
                    }
                    if (row["Upper"] != System.DBNull.Value)
                    {
                        ((NumberField)field).Upper = (string)row["Upper"];
                    }
                    if (row["Pattern"] != System.DBNull.Value)
                    {
                        ((NumberField)field).Pattern = (string)row["Pattern"];
                    }
                }

                if (field is DateField)
                {
                    if (row["Lower"] != System.DBNull.Value)
                    {
                        ((DateField)field).Lower = (string)row["Lower"];
                    }
                    if (row["Upper"] != System.DBNull.Value)
                    {
                        ((DateField)field).Upper = (string)row["Upper"];
                    }
                }

                if (field is PhoneNumberField)
                {
                    if (row["Pattern"] != System.DBNull.Value)
                    {
                        ((PhoneNumberField)field).Pattern = (string)row["Pattern"];
                    }
                }

                if (field is OptionField)
                {
                    string list = ((string)row["List"]);
                    
                    if (list.Contains("||"))
                    {
                        list = list.Substring(0, list.IndexOf("||"));
                    }

                    ((OptionField)field).Options = new List<string>(list.Split(Constants.LIST_SEPARATOR));
                    ((OptionField)field).ShowTextOnRight = (bool)row["ShowTextOnRight"];
                    if (row["Pattern"] is String)
                    {
                        ((OptionField)field).Pattern = (string)row["Pattern"];
                    }
                    else
                    {
                        ((OptionField)field).Pattern = string.Empty;
                    }
                }

                if (field is RelatedViewField)
                {
                    View relatedView = null;

                    if (relatedViewName == string.Empty && row.Table.Columns.Contains("RelatedViewId"))
                    {
                        if (row["RelatedViewId"] is int)
                        {
                            int relatedViewId = (int)row["RelatedViewId"];

                            if (viewIdViewNamePairs.ContainsKey(relatedViewId))
                            {
                                relatedViewName = viewIdViewNamePairs[relatedViewId];
                                relatedView = metadata.GetViewByFullName(relatedViewName);
                            }
                        }
                    }
                    else
                    {
                        if (viewIdViewNamePairs.ContainsValue(relatedViewName))
                        {
                            relatedView = metadata.GetViewByFullName(relatedViewName);
                        }
                    }

                    if (relatedView != null)
                    {
                        ((RelatedViewField)field).RelatedViewID = relatedView.Id;

                        if (row["RelateCondition"] is String)
                        {
                            ((RelatedViewField)field).Condition = (string)row["RelateCondition"];
                        }
                        else
                        {
                            ((RelatedViewField)field).Condition = string.Empty;
                        }

                        ((RelatedViewField)field).ShouldReturnToParent = (bool)row["ShouldReturnToParent"];
                    }
                }

                if (field is TableBasedDropDownField)
                {
                    string codeTableName = string.Empty;

                    if(row["SourceTableName"] != System.DBNull.Value)
                    {
                        codeTableName = (string)row["SourceTableName"];
                    }

                    if (_sourceTableRenames.ContainsKey(codeTableName))
                    {
                        codeTableName = _sourceTableRenames[codeTableName];
                    }

                    ((TableBasedDropDownField)field).SourceTableName = codeTableName;
                        
                    if (row["CodeColumnName"] != System.DBNull.Value)
                    {
                        ((TableBasedDropDownField)field).CodeColumnName = (string)row["CodeColumnName"];
                    }

                    if (row["TextColumnName"] != System.DBNull.Value)
                    {
                        ((TableBasedDropDownField)field).TextColumnName = (string)row["TextColumnName"];
                    }

                    ((TableBasedDropDownField)field).ShouldSort = (bool)row["Sort"];
                    ((TableBasedDropDownField)field).IsExclusiveTable = (bool)row["IsExclusiveTable"];

                    if (row["RelateCondition"] != System.DBNull.Value && field is DDLFieldOfCodes)
                    {
                        ((Epi.Fields.DDLFieldOfCodes)field).AssociatedFieldInformation = row["RelateCondition"].ToString();
                    }
                }

                if (field is MirrorField)
                {
                    ((FieldWithSeparatePrompt)field).PromptLeftPositionPercentage = promptLeftPercentage;
                    ((FieldWithSeparatePrompt)field).PromptTopPositionPercentage = promptTopPercentage;
                    
                    ((MirrorField)field).SourceFieldId = (int)row["SourceFieldId"];
                }

                if (field is GroupField)
                {
                    if (row["List"] != System.DBNull.Value && row["List"] is string)
                    {
                        ((GroupField)field).ChildFieldNames = (string)row["List"];
                    }
                    if (row["BackgroundColor"] != System.DBNull.Value)
                    {
                        ((GroupField)field).BackgroundColor = Color.FromArgb((int)row["BackgroundColor"]);
                    }
                }

                mediator.Canvas.MainForm.EndBusy();

                if (((RenderableField)field).Page == null)
                {
                }

                string nameTemplate = field.Name;
                
                    mediator.PasteFromTemplate((RenderableField)field);
                 
                if (field.Name != (string)row["Name"] && !_fieldRenames.ContainsKey((string)row["Name"]))
                {
                    _fieldRenames.Add((string)row["Name"], field.Name);
                }

                DataRow fieldRecordDataRow = _fieldRecord.NewRow();
                fieldRecordDataRow[FieldRecordColumn.NameTemplate.ToString()] = nameTemplate;
                fieldRecordDataRow[FieldRecordColumn.IdTemplate.ToString()] = fieldIdFromTemplate;
                fieldRecordDataRow[FieldRecordColumn.NameCreated.ToString()] = field.Name;
                fieldRecordDataRow[FieldRecordColumn.IdCreated.ToString()] = field.Id;

                _fieldRecord.Rows.Add(fieldRecordDataRow);
            }

            return field;
        }

        private bool TablesAreDifferent(DataTable firstTable, DataTable secondTable)
        {
            if (firstTable.Columns.Count != secondTable.Columns.Count)
            {
                return true;
            }
            if (firstTable.Rows.Count != secondTable.Rows.Count)
            {
                return true;
            }

            for (int i = 0; i < firstTable.Columns.Count; i++)
            {
                if (firstTable.Columns[i].ColumnName != secondTable.Columns[i].ColumnName)
                {
                    return true;
                }
            }
            return false; // dpb todo given time
        }

        /// <summary>
        /// Tweaks the TabIndex of some fields (Groups) then sorts by TabIndex.
        /// </summary>
        /// <param name="metaFieldsTable"></param>
        /// <returns></returns>
        private DataRow[] EnhancedSort(DataTable metaFieldsTable)
        {
            DataRow[] groups = metaFieldsTable.Select("FieldTypeId = 21");

            String[] memberFields;

            foreach (DataRow group in groups)
            {
                memberFields = group[ColumnNames.LIST].ToString().Split((Constants.LIST_SEPARATOR));
                StringBuilder expression = new StringBuilder();

                foreach (string memberFieldName in memberFields)
                {
                    if (expression.Length > 0) expression.Append(" OR ");
                    expression.Append(String.Format("{0} = '{1}'", ColumnNames.NAME, memberFieldName));
                }

                DataRow[] min = metaFieldsTable.Select(expression.ToString(), "TabIndex ASC");

                if (min.Length > 0)
                {
                    if (min[0][ColumnNames.TAB_INDEX] is int)
                    {
                        group[ColumnNames.TAB_INDEX] = (int)min[0][ColumnNames.TAB_INDEX] - 0.2;
                    }
                    else if (min[0][ColumnNames.TAB_INDEX] is double)
                    {
                        group[ColumnNames.TAB_INDEX] = (double)min[0][ColumnNames.TAB_INDEX] - 0.2;
                    }
                }
            }

            return metaFieldsTable.Select("", "TabIndex ASC");
        }

        #endregion
    }
    
    enum FieldRecordColumn
    {
        NameTemplate,
        IdTemplate,
        NameCreated,
        IdCreated
    }
}
