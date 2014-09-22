using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;
using Epi.Collections;
using Epi.Data;
using Epi.Fields;
using Epi;
using Epi.Data.Services;

namespace Epi
{

    /// <summary>
    /// A <see cref="Epi.View"/> of a <see cref="Epi.Project"/>. Contains one or more <see cref="Epi.Page"/> objects.
    /// </summary>
    public class View : ITable, INamedObject
    {
        
        #region Fields
        private string name = string.Empty;
        private bool isRelatedView = false;
        private string checkCode = string.Empty;
        private string checkCodeVariableDefinitions = string.Empty;
        private string checkCodeBefore = string.Empty;
        private string checkCodeAfter = string.Empty;
        private string recordCheckCodeBefore = string.Empty;
        private string recordCheckCodeAfter = string.Empty;
        private string pageOrientation = string.Empty;
        private string pageLabelAlign = string.Empty;
        private int pageWidth = 256;
        private int pageHeight = 512;
        private bool mustRefreshFieldCollection = false;
        private int id = 0;
        private bool returnToParent = false;
        private View parentView;
        private FieldCollectionMaster fields = null;
        private Project project;
        private XmlElement viewElement;
        private ArrayList fieldLockToken;
        private bool isDirty = false;

        /// <summary>
        /// Project that this view belongs to
        /// </summary>
        /// <summary>
        /// View's table name
        /// </summary>
        protected string dataTableName = string.Empty;
        /// <summary>
        /// Collection of all pages of the view
        /// </summary>
        protected List<Page> pages = null;
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Private constructor - not used
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        protected View()
        {
        }

        /// <summary>
        /// Creates a View object based on the Project XML file's view element
        /// </summary>
        /// <param name="viewElement"></param>
        /// <param name="proj"></param>
        public View(Project proj, XmlElement viewElement)
        {
            this.project = proj;
            this.viewElement = viewElement;
            this.Id = int.Parse(this.viewElement.Attributes["ViewId"].Value);
            this.Name = (this.viewElement.Attributes["Name"].Value);
            this.fieldLockToken = new ArrayList();
        }

        /// <summary>
        /// Constructs a new view linked to a project
        /// </summary>
        /// <param name="proj">A project object</param>
        public View(Project proj)
        {
            this.project = proj;
            Configuration configuration = Configuration.GetNewInstance();
            PageWidth = configuration.Settings.DefaultPageWidth;
            PageHeight = configuration.Settings.DefaultPageHeight;
            PageOrientation = configuration.Settings.DefaultPageOrientation;
            PageLabelAlign = configuration.Settings.DefaultLabelAlign;
            this.fieldLockToken = new ArrayList();
        }

        /// <summary>
        /// Constructs a new view from a data row
        /// </summary>
        /// <param name="row">Data row containing view information</param>
        /// <param name="proj">The view this project belongs to</param>
        public View(DataRow row, Project proj)
            : this(proj)
        {
            Name = row[ColumnNames.NAME].ToString();
            Id = (int)row[ColumnNames.VIEW_ID];
            CheckCodeVariableDefinitions = row[ColumnNames.CHECK_CODE_VARIABLE_DEFINITIONS].ToString();
            CheckCode = row[ColumnNames.CHECK_CODE].ToString();
            CheckCodeBefore = row[ColumnNames.CHECK_CODE_BEFORE].ToString();
            CheckCodeBefore = row[ColumnNames.CHECK_CODE_BEFORE].ToString();
            WebSurveyId = row[ColumnNames.CHECK_CODE_AFTER].ToString();
            RecordCheckCodeBefore = row[ColumnNames.RECORD_CHECK_CODE_BEFORE].ToString();
            RecordCheckCodeAfter = row[ColumnNames.RECORD_CHECK_CODE_AFTER].ToString();
            IsRelatedView = (bool)row[ColumnNames.IS_RELATED_VIEW];

            if( row.Table.Columns.Contains(ColumnNames.PAGE_WIDTH))
            {
                PageWidth = (int)row[ColumnNames.PAGE_WIDTH];
            }
            if (row.Table.Columns.Contains(ColumnNames.PAGE_HEIGHT))
            {
                PageHeight = (int)row[ColumnNames.PAGE_HEIGHT];
            }
            if (row.Table.Columns.Contains(ColumnNames.PAGE_ORIENTATION))
            {            
                PageOrientation = row[ColumnNames.PAGE_ORIENTATION].ToString();
            }
            if (row.Table.Columns.Contains(ColumnNames.PAGE_LABEL_ALIGN))
            {
                PageLabelAlign = row[ColumnNames.PAGE_LABEL_ALIGN].ToString();
            }

            if (IsRelatedView)
            {
                this.GetParent(Id);
            }
            this.fieldLockToken = new ArrayList();
        }

        #endregion Constructors

        #region Public Properties

        public object FieldLockToken
        {
            get
            {
                return fieldLockToken;
            }
        }

        public string FromViewSQL
        {
            get
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append(SqlKeyWords.FROM);
                sb.Append(StringLiterals.SPACE);
                sb.Append("");
                

                System.Text.StringBuilder sb2 = new System.Text.StringBuilder();
                sb2.Append(Util.InsertInSquareBrackets(this.TableName));
                sb2.Append(" t inner join ");

                foreach (Page page in this.Pages)
                {
                    // prepend open paren
                    sb.Append("(");

                    // notice sb2 is used here
                    sb2.Append(Util.InsertInSquareBrackets(page.TableName));
                    sb2.Append(" on t.GlobalRecordId = ");
                    sb2.Append(Util.InsertInSquareBrackets(page.TableName));
                    sb2.Append(".GlobalRecordId) inner join ");
                }
                sb2.Length = sb2.Length - 12;

                sb.Append(sb2);

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the view's <see cref="Epi.Project"/>.
        /// </summary>
        public Project Project
        {
            get
            {
                return this.project;
            }
        }

        //public string Name
        /// <summary>
        /// Returns the name of the <see cref="Epi.View"/>.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return (name);
            }
            set
            {
                name = value;
            }
        }

        //public string DisplayName
        /// <summary>
        /// Returns the display name of the <see cref="Epi.View"/>.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                return project.DisplayName + "\\" + Name;
            }
        }

        //public bool IsRelatedView
        /// <summary>
        /// Gets/sets the "Is Related View" flag.
        /// </summary>
        public virtual bool IsRelatedView
        {
            get
            {
                return (isRelatedView);
            }
            set
            {
                isRelatedView = value;
            }
        }

        /// <summary>
        /// Gets/sets the "Should return to parent after one record" flag (specifies 1-to-1 relationship
        /// if true, specifies 1-to-many if false; only applies to related views).
        /// </summary>
        public virtual bool ReturnToParent
        {
            get
            {
                return (returnToParent);
            }
            set
            {
                returnToParent = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the view is 'dirty' and needs to be saved.
        /// </summary>
        public virtual bool IsDirty
        {
            get
            {
                return (isDirty);
            }
            set
            {
                isDirty = value;
            }
        }

        //public string CheckCodeVariableDefinitions
        /// <summary>
        /// Gets/sets the "Check Code Variable Definitions" flag.
        /// </summary>
        public virtual string CheckCodeVariableDefinitions
        {
            get
            {
                return (checkCodeVariableDefinitions);
            }
            set
            {
                checkCodeVariableDefinitions = value;
            }
        }

        //public string CheckCode
        /// <summary>
        /// </summary>
        public virtual string CheckCode
        {
            get
            {
                return this.checkCode;
            }
            set
            {
                this.checkCode = value;
            }
        }

        //public string CheckCodeAfter
        /// <summary>
        /// Gets/sets the "Check Code After" flag.
        /// </summary>
        public virtual string WebSurveyId
        {
            get
            {
                return (checkCodeAfter);
            }
            set
            {
                checkCodeAfter = value;
            }
        }

        //public string CheckCodeBefore
        /// <summary>
        /// Gets/sets the "Check Code Before" flag.
        /// </summary>
        public virtual string CheckCodeBefore
        {
            get
            {
                return (checkCodeBefore);
            }
            set
            {
                checkCodeBefore = value;
            }
        }

        //public string RecordCheckCodeAfter
        /// <summary>
        /// Gets/sets the "Record Check Code After" flag.
        /// </summary>
        public virtual string RecordCheckCodeAfter
        {
            get
            {
                return (recordCheckCodeAfter);
            }
            set
            {
                recordCheckCodeAfter = value;
            }
        }

        //public string RecordCheckCodeBefore
        /// <summary>
        /// Gets/sets the "Record Check Code Before" flag.
        /// </summary>
        public virtual string RecordCheckCodeBefore
        {
            get
            {
                return (recordCheckCodeBefore);
            }
            set
            {
                recordCheckCodeBefore = value;
            }
        }

        public string PageOrientation
        {
            get
            {
                return pageOrientation;
            }
            set
            {
                pageOrientation = value;
            }
        }

        public string PageLabelAlign
        {
            get
            {
                return pageLabelAlign;
            }
            set
            {
                pageLabelAlign = value;
            }
        }

        public int PageWidth
        {
            get
            {
                return pageWidth;
            }
            set
            {
                pageWidth = value;
            }
        }

        public int PageHeight
        {
            get
            {
                return pageHeight;
            }
            set
            {
                pageHeight = value;
            }
        }

        /// <summary>
        /// Gets/sets the "Must Refresh Field Collection" flag.
        /// </summary>
        public bool MustRefreshFieldCollection
        {
            get
            {
                return (mustRefreshFieldCollection);
            }
            set
            {
                mustRefreshFieldCollection = value;
            }
        }

        /// <summary>
        /// Gets the fully-qualified project:view name.
        /// </summary>
        public string FullName
        {
            get
            {
                return (project.FullName + StringLiterals.COLON + this.Name);
            }
        }

        /// <summary>
        /// TODO: need to implement this method
        /// </summary>
        public virtual List<string> TableColumnNames // Implements ITable.TableColumnNames
        {
            get
            {
                return Fields.TableColumnFields.Names;
            }
        }

        /// <summary>
        /// Master collection of all fields in the view.
        /// </summary>
        public FieldCollectionMaster Fields
        {
            get
            {
                if ((fields == null) || MustRefreshFieldCollection)
                {
                    if (this.Project.MetadataSource.Equals(MetadataSource.Xml) == false)
                    {
                        fields = GetMetadata().GetFields(this);
                    }
                    else
                    {
                        //fields = null;
                        //fields = new FieldCollectionMaster();
                        //XmlNode fieldsNode = viewElement.SelectSingleNode("Fields");
                        //foreach (XmlNode fieldNode in fieldsNode.ChildNodes)
                        //{
                        //    MetaFieldType fieldTypeId = (MetaFieldType)Enum.Parse(typeof(MetaFieldType), fieldNode.Attributes["FieldTypeId"].Value.ToString());

                        //    Field field = null;

                        //    switch (fieldTypeId)
                        //    {
                        //        case MetaFieldType.Text:
                        //            field = new SingleLineTextField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.LabelTitle:
                        //            field = new LabelField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.TextUppercase:
                        //            field = new UpperCaseTextField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Multiline:
                        //            field = new MultilineTextField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Number:
                        //            field = new NumberField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.PhoneNumber:
                        //            field = new PhoneNumberField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Date:
                        //            field = new DateField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Time:
                        //            field = new TimeField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.DateTime:
                        //            field = new DateTimeField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Checkbox:
                        //            field = new CheckBoxField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.YesNo:
                        //            field = new YesNoField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Option:
                        //            field = new OptionField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.CommandButton:
                        //            field = new CommandButtonField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Image:
                        //            field = new ImageField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Mirror:
                        //            field = new MirrorField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Grid:
                        //            field = new GridField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.LegalValues:
                        //            field = new DDLFieldOfLegalValues(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Codes:
                        //            field = new DDLFieldOfCodes(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.List:
                        //            field = new DDListField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.CommentLegal:
                        //            field = new DDLFieldOfCommentLegal(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.Relate:
                        //            field = new RelatedViewField(this, fieldNode);
                        //            break;
                        //        case MetaFieldType.RecStatus:
                        //            field = new RecStatusField(this);
                        //            break;
                        //        case MetaFieldType.UniqueKey:
                        //            field = new UniqueKeyField(this);
                        //            break;
                        //        case MetaFieldType.ForeignKey:
                        //            field = new ForeignKeyField(this);
                        //            break;
                        //        case MetaFieldType.GlobalRecordId:
                        //            field = new GlobalRecordIdField(this);
                        //            break;
                        //        default:
                        //            throw new GeneralException("Invalid Field Type");
                        //    }
                        //    fields.Add(field);
                        //}
                    }
                    MustRefreshFieldCollection = false;
                }
                return fields;
            }
        }

        /// <summary>
        /// Gets/sets the <see cref="Epi.View"/> Id.
        /// </summary>
        public virtual int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>
        /// The name of the view's collected data table
        /// </summary>
        public string TableName // Implements ITable.TableName
        {
            get
            {
                if (string.IsNullOrEmpty(dataTableName))
                {
                    if (!this.Project.MetadataSource.Equals(MetadataSource.Xml))
                    {
                        dataTableName = GetMetadata().GetDataTableName(Id);
                    }
                    else
                    {
                        dataTableName = viewElement.Attributes["Name"].Value;
                    }

                    if (string.IsNullOrEmpty(dataTableName))
                    {
                        return Name;
                    }
                    else
                    {
                        return dataTableName;
                    }
                }
                else
                {
                    return dataTableName;
                }
            }
            set
            {
                dataTableName = value;

                if (!this.Project.MetadataSource.Equals(MetadataSource.Xml))
                {
                    GetMetadata().UpdateDataTableName(this.Id, dataTableName);
                }
                else
                {
                }
            }
        }

        /// <summary>
        /// Returns a collection of all pages of the <see cref="Epi.View"/>.
        /// </summary>
        public virtual List<Page> Pages
        {
            get
            {
                if (pages == null)
                {
                    pages = GetMetadata().GetViewPages(this);                                                          
                }
                return (pages);
            }
        }

        /// <summary>
        /// Returns the unique key field.
        /// </summary>
        public UniqueKeyField UniqueKeyField
        {
            get
            {
                return Fields.UniqueKeyField;
            }
        }

        /// <summary>
        /// Returns the Unique Identifier Field.
        /// </summary>
        public UniqueIdentifierField UniqueIdentifierField
        {
            get
            {
                return Fields.UniqueIdentifierField;
            }
        }
        /// <summary>
        /// Returns the record status field.
        /// </summary>
        public RecStatusField RecStatusField
        {
            get
            {
                return Fields.RecStatusField;
            }
        }

        /// <summary>
        /// Returns the foreign key field of the unique key.
        /// </summary>
        public ForeignKeyField ForeignKeyField
        {
            get
            {
                return Fields.ForeignKeyField;
            }
        }

        public bool ForeignKeyFieldExists
        {
            get
            {
                return Fields.ForeignKeyFieldExists;
            }
        }

        /// <summary>
        /// Returns the unique key field.
        /// </summary>
        public GlobalRecordIdField GlobalRecordIdField
        {
            get
            {
                return Fields.GlobalRecordIdField;
            }
        }

        /// <summary>
        /// View that this view is related to if IsRelatedView = true.
        /// </summary>
        public View ParentView
        {
            get { return parentView; }
            set
            {
                parentView = value;
                if (parentView != null)
                {
                    ForeignKeyField.CurrentRecordValueString = value.CurrentGlobalRecordId;
                }
            }
        }
	
        /// <summary>
        /// The view element of the view 
        /// </summary>
        public XmlElement ViewElement
        {
            get
            {
                return viewElement;
            }
            set
            {
                viewElement = value;
            }
        }

        /// <summary>
        /// Returns the Current record Id
        /// </summary>
        public int CurrentRecordId
        {
            get
            {
                return this.UniqueKeyField.CurrentRecordValue;
            }
        }
        /// <summary>
        /// Returns the curent record status
        /// </summary>
        public int CurrentRecordStatus
        {
            get
            {
                return this.RecStatusField.CurrentRecordValue;
            }
            set
            {
                this.RecStatusField.CurrentRecordValue = value;
            }
        }

        /// <summary>
        /// Returns the curent record status
        /// </summary>
        public string CurrentGlobalRecordId
        {
            get
            {
                return this.GlobalRecordIdField.CurrentRecordValueString;
            }
            set
            {
                this.GlobalRecordIdField.CurrentRecordValueObject = value;
            }
        }

        #endregion Public Properties

        #region Private Properties
        IDbDriver ITable.Database
        {
            get
            {
                return this.project.CollectedData.GetDatabase();
            }
        }
        #endregion Private Properties

        #region Static Methods

        /// <summary>
        /// Checks the name of a view to make sure the syntax is valid.
        /// </summary>
        /// <param name="viewName">The name of the view to validate</param>
        /// <param name="validationStatus">The message that is passed back to the calling method regarding the status of the validation attempt</param>
        /// <returns>Whether or not the name passed validation; true for a valid view name, false for an invalid view name</returns>
        public static bool IsValidViewName(string viewName, ref string validationStatus)
        {
            // assume valid by default
            bool valid = true;
            
            if (string.IsNullOrEmpty(viewName.Trim()))
            {
                // if the view name is empty, or just a series of spaces, invalidate it
                validationStatus = SharedStrings.MISSING_VIEW_NAME;
                valid = false;
            }            
            else if (AppData.Instance.IsReservedWord(viewName))
            {
                // if the view name is a reserved word, invalidate it
                validationStatus = SharedStrings.INVALID_VIEW_NAME_RESERVED_WORD;
                valid = false;
            }
            else if (viewName.Length > 64)
            {
                validationStatus = SharedStrings.INVALID_VIEW_NAME_TOO_LONG;
                valid = false;
            }
            else
            {
                // if the view name is not empty or in the list of reserved words...
                Match numMatch = Regex.Match(viewName.Substring(0, 1), "[0-9]");

                if (numMatch.Success)
                {
                    // if the view name has numbers for the first character, invalidate it
                    validationStatus = SharedStrings.VIEW_NAME_BEGIN_NUMERIC;
                    valid = false;
                }
                // if the view name doesn't have a number as the first character...
                else
                {
                    // iterate over all of the characters in the view name
                    for (int i = 0; i < viewName.Length; i++)
                    {
                        string viewChar = viewName.Substring(i, 1);
                        Match m = Regex.Match(viewChar, "[A-Za-z0-9]");
                        // if the view name does not consist of only letters and numbers...
                        if (!m.Success)
                        {
                            // we found an invalid character; invalidate the view name
                            validationStatus = SharedStrings.INVALID_VIEW_NAME;
                            valid = false;
                            break; // stop the for loop here, no point in continuing
                        }
                    }
                }
            }

            return valid;
        }

        #endregion // Static Methods

        #region Public Methods

        /// <summary>
        /// Overrides TableName property value
        /// </summary>
        /// <param name="tableName"></param>
        public void SetTableName(string tableName)
        {
            this.TableName = tableName;

        }

        /// <summary>
        /// Gets a reference to the Epi7 project object.
        /// </summary>
        /// <returns></returns>
        public virtual Project GetProject()
        {
            return project;
        }

        /// <summary>
        /// Gets a list of all descendant views.
        /// </summary>
        /// <returns>List of Views</returns>
        public virtual List<View> GetDescendantViews()
        {
            List<View> relatedViews = new List<View>();

            if (this.Project.Views.Count == 1)
            {
                return relatedViews;
            }

            foreach (View view in this.Project.Views)
            {
                if (view != this && view.Name != this.Name)
                {
                    if (Util.IsFormDescendant(view, this))
                    {
                        relatedViews.Add(view);
                    }
                }
            }

            return relatedViews;
        }

        

        /// <summary>
        /// Copies a view object into this
        /// </summary>
        public void CopyFrom(View other)
        {
            this.Name = other.Name;
            this.CheckCodeVariableDefinitions = other.CheckCodeVariableDefinitions;
            this.CheckCode = other.CheckCode;
            this.CheckCodeBefore = other.CheckCodeBefore;
            this.checkCodeAfter = other.WebSurveyId;
            this.RecordCheckCodeBefore = other.RecordCheckCodeBefore;
            this.RecordCheckCodeAfter = other.RecordCheckCodeAfter;
            this.IsRelatedView = other.IsRelatedView;
        }

        /// <summary>
        /// Implements IDisposable.Dispose() method
        /// </summary>
        public virtual void Dispose()
        {
            if (pages != null)
            {
                Pages.Clear();
                // Pages.Dispose();
            }
        }

        /// <summary>
        /// Shortcut for getting Metadata
        /// </summary>
        /// <returns>Metadata Provider.</returns>
        public IMetadataProvider GetMetadata()
        {
            return project.Metadata;
        }

        /// <summary>
        /// Save to database
        /// </summary>
        public virtual void SaveToDb()
        {
            if (Id == 0)
            {
                GetMetadata().InsertView(this);
            }
            else
            {
                GetMetadata().UpdateView(this);
            }
        }

        /// <summary>
        /// Runs the specified check code
        /// </summary>
        /// <param name="module">current module</param>
        /// <param name="checkCode">Check code block to run</param>
        public void RunCheckCode(IModule module, string checkCode)
        {
            //CommandProcessorResults results = module.Processor.RunCommands(checkCode);
            //if (results != null)
            //{
            //    foreach (Action actionId in results.Actions)
            //    {
            //        switch (actionId)
            //        {
            //            case Action.Assign:
            //                break;
            //            case Action.AutoSearch:
            //                break;
            //            case Action.Clear:
            //                break;
            //            case Action.Define:
            //                break;
            //            case Action.Dialog:
            //                break;
            //            case Action.Execute:
            //                break;
            //            case Action.GoTo:
            //                break;
            //            case Action.Hide:
            //                break;
            //            case Action.Unhide:
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Creates a new page and adds it to the page collection
        /// </summary>
        /// <param name="name">Name of the page</param>
        /// <param name="position">Position of the page</param>
        /// <returns>The new page object</returns>
        public Page CreatePage(string name, int position)
        {
            #region Input Validation
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (position < 0)
            {
                throw new ArgumentOutOfRangeException("Position");
            }
            #endregion Input Validation

            Page page = null;
            this.pages = null;
            this.project.Save();
            
            page = new Page(this, 0);
            page.Name = name;
            page.Position = position;
            page.Id = 0;
            page.SaveToDb();

            return page;
        }

        /// <summary>
        /// Delete a page from a view
        /// </summary>
        public void DeletePage(Page page)
        {
            page.DeleteFields();
            GetMetadata().DeletePage(page);
            //GetMetadata().SynchronizePageNumbersOnDelete(this, page.Position + 1);		//incremented position by 1 since the index is 0
            GetMetadata().SynchronizePageNumbersOnDelete(this, page.Position);
            page = null;	//explicit dispose	
            pages = null;
            fields = GetMetadata().GetFields(this);
        }

        /// <summary>
        /// Delete a view's data table(s)
        /// </summary>
        public virtual void DeleteDataTables()
        {
            CollectedDataProvider collectedDataPro = this.Project.CollectedData;
            //remove all tables for GridFields
            foreach (Field field in this.Fields)
            {
                if (field is GridField)
                {
                    GridField gridField = field as GridField;
                    collectedDataPro.DeleteDataTableForGrid(this, gridField);
                }
            }

            // remove main data table
            if (collectedDataPro.TableExists(this.TableName))
            {
                collectedDataPro.DeleteTable(this.TableName);
            }

            // remove page data tables
            foreach (Page page in this.Pages)
            {
                if (collectedDataPro.TableExists(page.TableName))
                {
                    collectedDataPro.DeleteTable(page.TableName);
                }
            }
        }

        /// <summary>
        /// Composes a field name from a prompt name.
        /// </summary>
        /// <param name="promptText">String entered as prompt</param>
        /// <returns>Field name</returns>
        public string ComposeFieldNameFromPromptText(string promptText)
        {
            #region Input Validation
            if (promptText == null)
            {
                throw new ArgumentNullException("Prompt Text");
            }
            #endregion Input Validation
            
            promptText = Util.Squeeze(promptText);

            if (promptText == "")
            {
                promptText = "field";        
            }

            //Remove any special characters in the prompt name
            string fieldName = Util.RemoveNonAlphaNumericCharacters(promptText);

            //If first character is not a letter then prefix field name with "N"
            if (!string.IsNullOrEmpty(fieldName))
            {
                if (!Util.IsFirstCharacterALetter(fieldName))
                {
                    fieldName = "N" + fieldName;
                }
            }

            // Check if this field already exists. If so, increment a suffix counter.
            string newFieldName = fieldName;
            int count = 0;
            while (Fields.Exists(newFieldName))
            {
                count++;
                newFieldName = fieldName + count;
            }

            return newFieldName;
        }

        /// <summary>
        /// Gets the data for the current record in the current view 
        /// </summary>
        /// <param name="recordID">Record Id</param>
        public virtual void LoadRecord(int recordID)
        {
            #region Input Validation
            if (recordID < 1)
            {
                throw new ArgumentOutOfRangeException("Record ID");
            }
            #endregion //Input Validation

            if (!string.IsNullOrEmpty(this.RecordCheckCodeBefore))
            {
                //				RunCheckCode(this.RecordCheckCodeBefore);
            }
            project.CollectedData.LoadRecordIntoView(this, recordID);
        }


        /// <summary>
        /// Loads the first record into view
        /// </summary>
        public virtual void LoadFirstRecord()
        {
            int recId = GetFirstRecordId();
            if (recId > 0)
            {
                LoadRecord(recId);
            }
        }

        /// <summary>
        /// Loads the last record into this <see cref="Epi.View"/>.
        /// </summary>
        public virtual void LoadLastRecord()
        {
            int recId = GetLastRecordId();
            if (recId > 0)
            {
                LoadRecord(recId);
            }
        }

        /// <summary>
        /// Loads the prior to the current record into this <see cref="Epi.View"/> by Id.
        /// </summary>
        /// <param name="currentRecordId">Current record Id.</param>
        public virtual void LoadPreviousRecord(int currentRecordId)
        {
            #region Input Validation
            if (currentRecordId < 0)
            {
                throw new ArgumentOutOfRangeException("currentRecordId");
            }
            #endregion Input Validation

            int recId = 1;
            // handle special case scenario where user is on a new record and navigates backwards
            if (currentRecordId == 0)
            {
                recId = GetLastRecordId();
            }
            else
            {
                recId = GetPreviousRecordId(currentRecordId);
            }

            if (recId > 0)
            {
                LoadRecord(recId);
            }
        }

        /// <summary>
        /// Loads the next to the current record into this <see cref="Epi.View"/> by Id.
        /// </summary>
        /// <param name="currentRecordId">Current record Id.</param>
        public virtual void LoadNextRecord(int currentRecordId)
        {
            #region Input Validation
            if (currentRecordId < 1)
            {
                throw new ArgumentOutOfRangeException("currentRecordId");
            }
            #endregion Input Validation

            int recId = GetNextRecordId(currentRecordId);
            if (recId > 0)
            {
                LoadRecord(recId);
            }
        }

        /// <summary>
        /// Gets the record Id for the first record
        /// </summary>
        /// <returns>Id of the first record</returns>
        public virtual int GetFirstRecordId()
        {
            return project.CollectedData.GetFirstRecordId(this);
        }

        /// <summary>
        /// Gets the record Id for the last record
        /// </summary>
        /// <returns>Id of the last record</returns>
        public virtual int GetLastRecordId()
        {
            return (project.CollectedData.GetLastRecordId(this));
        }

        /// <summary>
        /// Returns the previous record Id
        /// </summary>
        /// <param name="currentRecordId">Current record Id.</param>
        /// <returns>Previous record Id.</returns>
        public virtual int GetPreviousRecordId(int currentRecordId)
        {
            return project.CollectedData.GetPreviousRecordId(this, currentRecordId);
        }

        /// <summary>
        /// Returns the next record Id
        /// </summary>
        /// <param name="currentRecordId">Current record Id.</param>
        /// <returns>Next record Id.</returns>
        public virtual int GetNextRecordId(int currentRecordId)
        {
            return project.CollectedData.GetNextRecordId(this, currentRecordId);
        }

        /// <summary>
        /// Gets the record count for the current view
        /// </summary>
        /// <returns>Record count</returns>
        public virtual int GetRecordCount()
        {
            return (project.CollectedData.GetRecordCount(this));
        }

        /// <summary>
        /// Saves the current record
        /// </summary>		
        /// <param name="recordId">The unique record</param>
        /// <returns>the record Id</returns>
        public virtual int SaveRecord(int recordId)
        {
            #region Input Validation
            if (recordId < 1)
            {
                throw new ArgumentOutOfRangeException("Record ID");
            }
            #endregion Input Validation

            return (project.CollectedData.SaveRecord(this, recordId));
        }

        /// <summary>
        /// Inserts the current record
        /// </summary>
        /// <returns>Record Id</returns>
        public virtual int SaveRecord()
        {
            return (project.CollectedData.SaveRecord(this));
        }

        public bool IsEmptyNewRecord()
        {
            bool fieldsAreEmpty = IsViewRecordEmpty();
            bool newRecord = CurrentRecordId == 0;

            if (fieldsAreEmpty && newRecord)
            {
                return true;
            }

            return false;
        }

        public virtual bool IsParent()
        {
            foreach (Field field in Fields)
            {
                if (field is RelatedViewField)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if all input fields are empty
        /// </summary>
        /// <returns>true if all input fields are empty; otherwise false</returns>
        public bool IsViewRecordEmpty()
        {
            foreach (IDataField field in Fields.DataFields)
            {
                if (field is IInputField)
                {
                    if (field is CheckBoxField)
                    {
                        if (field.CurrentRecordValueObject != null && field.CurrentRecordValueObject != DBNull.Value)
                        {
                            if ((bool)field.CurrentRecordValueObject)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (!Util.IsEmpty(field.CurrentRecordValueObject))
                        {
                            switch (field.FieldType)
                            {
                                case MetaFieldType.Text:
                                    foreach (Epi.Fields.TextField F in this.fields.TextFields)
                                    {
                                        if (F.Name == ((Epi.INamedObject)field).Name)
                                        {
                                            if (F.IsReadOnly)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    break;
                                case MetaFieldType.ForeignKey:
                                case MetaFieldType.Mirror:
                                case MetaFieldType.GUID:
                                    break;

                                default:
                                    return (false);
                            }
                        }
                        else continue;
                    }
                }
            }

            foreach (Field field in Fields)
            {
                if (field is GridField)
                {
                    if (!Util.IsEmpty(((GridField)field).DataSource))
                    {
                        if (((GridField)field).DataSource.Rows.Count > 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return (true);
        }


        /// <summary>
        /// Fetches a field from it's collection by it's Id.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public Field GetFieldById(int fieldId)
        {
            foreach (Field field in Fields)
            {
                if (field.Id == fieldId)
                {
                    return field;
                }
            }            
            return null;
        }


        /// <summary>
        /// Gets a collection of renderable <see cref="Epi.Fields"/> on an <see cref="Epi.Page"/>
        /// </summary>
        /// <param name="page"><see cref="Epi.Page"/></param>
        /// <returns>A named object collection of renderable <see cref="Epi.Fields"/>.</returns>
        public NamedObjectCollection<Field> GetFieldsOnPage(Page page)
        {
            NamedObjectCollection<Field> pageFields = new NamedObjectCollection<Field>();
            foreach (Field field in page.Fields)
            {
                if (field is RenderableField)
                {
                    RenderableField renderableField = field as RenderableField;
                    if (renderableField.Page.Id == page.Id)
                    {
                        pageFields.Add(renderableField);
                    }
                }
            }
            return pageFields;
        }

        /// <summary>
        /// Returns an <see cref="Epi.Page"/> object by its id.
        /// </summary>
        /// <param name="pageId">The Id of the page.</param>
        /// <returns><see cref="Epi.Page"/></returns>
        public Page GetPageById(int pageId)
        {
            foreach (Page page in this.Pages)
            {
                if (page.Id == pageId)
                {
                    return page;
                }
            }
            throw new System.ApplicationException(SharedStrings.ERROR_PAGE_NOT_FOUND);
        }


        /// <summary>
        /// Returns an <see cref="Epi.Page"/> object by its position.
        /// </summary>
        /// <param name="position">The index of the page</param>
        /// <returns><see cref="Epi.Page"/></returns>
        public Page GetPageByPosition(int position)
        {
            foreach (Page page in this.Pages)
            {
                if (page.Position == position)
                {
                    return page;
                }
            }
            throw new System.ApplicationException(SharedStrings.ERROR_PAGE_NOT_FOUND);
        }


        /// <summary>
        /// Returns  a table of mirrorable fields
        /// </summary>
        [Obsolete("Use of DataTable in this context is no different than the use of a multidimensional System.Object array (not recommended).", false)]
        public DataTable GetMirrorableFieldsAsDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add(ColumnNames.NAME);
            table.Columns.Add(ColumnNames.ID);
            DataRow dataRow;

            foreach (Field field in Fields)
            {
                if (field is IMirrorable)
                {
                    dataRow = table.NewRow();
                    dataRow[Epi.ColumnNames.NAME] = field.Name;
                    dataRow[Epi.ColumnNames.ID] = (field).Id;
                    table.Rows.Add(dataRow);
                }
            }
            return table;
        }

        /// <summary>
        /// Retrieves the page Id
        /// </summary>
        /// <param name="pagesNode">The XML pages node</param>
        /// <returns>The page id</returns>
        private int GetPageId(XmlNode pagesNode)
        {
            int pageId;
            int maxId = 0;
            foreach (XmlNode pageNode in pagesNode.ChildNodes)
            {
                if (!string.IsNullOrEmpty(pageNode.Attributes["PageId"].Value.ToString()))
                {
                    pageId = int.Parse(pageNode.Attributes["PageId"].Value.ToString());
                    maxId = Math.Max(pageId, 0);
                }
            }
            return maxId += 1;
        }

        /// <summary>
        /// Returns the next available field Id on a <see cref="Epi.View"/>.
        /// </summary>
        /// <param name="viewElement">An <see cref="Epi.View"/> element.</param>
        /// <returns></returns>
        public int GetFieldId(XmlElement viewElement)
        {
            int maxFieldId = this.Project.Metadata.GetMaxFieldId(this.Id);
            return maxFieldId += 1;
        }

        public void SaveToCollection(Field field)
        {
            if (field.Id == 0)
            {
                fields.Add(field);
            }
            else
            {
                if(fields.Contains(field))
                {
                    fields.Remove(field);
                }
                fields.Add(field);
            }
        }

        public void RemoveFromCollection(Field field)
        {
            fields.Remove(field);
        }

        #endregion Public Methods

        #region Private Methods
        
        private void GetParent(int relatedViewId)
        {
            parentView = this.GetMetadata().GetParentView(relatedViewId);
        }

        #endregion

        public const string InitialCheckCode = 
"/*\n"
+ "	1. Choose a block from the upper right list to create a block for the action to occur.\n"
+ "	2. Select a command to add to the block from the lower right list.\n"
+ "	All Check Code commands must be within a block.\n*/";
        
    }
}
