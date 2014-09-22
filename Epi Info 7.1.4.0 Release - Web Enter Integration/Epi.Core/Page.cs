using System;
using System.Data;
using System.Drawing;
using System.Xml;
using Epi.Collections;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.Data.Services;

namespace Epi
{
    #region Delegate Definition
    /// <summary>
    /// ChildViewRequestedEventArgs class
    /// </summary>
    public class ChildViewRequestedEventArgs : EventArgs
    {
        private View view = null;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view of an Epi.Project.</param>
        public ChildViewRequestedEventArgs(View view)
        {
            this.view = view;
        }

        /// <summary>
        /// The view of an Epi.Project.
        /// </summary>
        public View View
        {
            get
            {
                return this.view;
            }
        }
    }
    /// <summary>
    /// Delegate for handing the selection of a related view
    /// </summary>
    public delegate void ChildViewRequestedEventHandler(object sender, ChildViewRequestedEventArgs e);

    ///// <summary>
    ///// Delegate for handling the request of showing a field definition dialog
    ///// </summary>
    //public delegate void FieldDialogRequestHandler(object sender, FieldEventArgs e);
    /// <summary>
    /// ContextMenuEventArgs class
    /// </summary>
    public class ContextMenuEventArgs : EventArgs
    {
        private Page page = null;
        private int x = 0;
        private int y = 0;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page the context menu belongs to</param>
        /// <param name="x">The X-coordinate of the context menu.</param>
        /// <param name="y">The Y-coordinate of the context menu.</param>
        public ContextMenuEventArgs(Page page, int x, int y)
        {
            this.page = page;
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Gets the page this context menu belongs to
        /// </summary>
        public Page Page
        {
            get
            {
                return this.page;
            }
        }

        /// <summary>
        /// Gets the X-coordinate of the context menu.
        /// </summary>
        public int X
        {
            get
            {
                return this.x;
            }
        }

        /// <summary>
        /// Gets the Y-coordinate of the context menu.
        /// </summary>
        public int Y
        {
            get
            {
                return this.y;
            }
        }
    }

    /// <summary>
    /// Delegate for handling the request of showing the page's context menu
    /// </summary>
    public delegate void ContextMenuRequestHandler(object sender, ContextMenuEventArgs e);

    #endregion

    /// <summary>
    /// A page in a view of a project.
    /// </summary>
    public class Page : INamedObject
    {
        #region Private Class Members
        private int id;
        private NamedObjectCollection<GroupField> groupFields;
        private bool designMode;
        private string name = string.Empty;
        private int position = 0;
        private string checkCodeBefore = string.Empty;
        private string checkCodeAfter = string.Empty;
        private int backgroundId = 0;
        private bool flipLabelColor = false;
        /// <summary>
        /// view
        /// </summary>
        public View view = null;
        private XmlElement viewElement;
        #endregion Private Class Members

        #region Events

        /// <summary>
        /// Occurs when a related view is requested by a relate field
        /// </summary>
        public event ChildViewRequestedEventHandler ChildViewRequested;

        #endregion

        #region Constructors
        
		/// <summary>
		/// Private constructor - not used
		/// </summary>
		public Page()
		{
		}

        /// <summary>
        /// Constructs a page linked to a view
        /// </summary>
        /// <param name="view">A view object</param>
        public Page(View view)
        {
            this.view = view;
        }

        //public Page(View view, int pageId)
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view"></param>
        /// <param name="pageId"></param>
        public Page(View view, int pageId)
        {
            this.view = view;
            this.viewElement = view.ViewElement;
            this.name = GetPageName(view, pageId);
            this.id = pageId;
        }

        /// <summary>
        /// Constructs a page from database table row
        /// </summary>
        /// <param name="row"></param>
        /// <param name="view"></param>
        public Page(DataRow row, View view)
            : this(view)
        {
            if (row[ColumnNames.NAME] != DBNull.Value)
                this.Name = row[ColumnNames.NAME].ToString();
            this.Id = (int)row[ColumnNames.PAGE_ID];
            this.Position = (short)row[ColumnNames.POSITION];
            this.CheckCodeBefore = row[ColumnNames.CHECK_CODE_BEFORE].ToString();
            this.CheckCodeAfter = row[ColumnNames.CHECK_CODE_AFTER].ToString();
            this.BackgroundId = (int)row[ColumnNames.BACKGROUND_ID];
        }
        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Name of the page
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = "New Page";
                }
                return (name);
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Database Table Name of the page
        /// </summary>
        public string TableName
        {
            get
            {
                if (this.view == null)
                {
                    return null;
                }
                else
                {
                    return this.view.TableName + this.id.ToString();
                }
            }
        }

        /// <summary>
        /// Display name of the page
        /// </summary>
        public string DisplayName
        {
            get
            {
                return (view.DisplayName + "::" + this.Name);
            }
        }

        /// <summary>
        /// Position of the page in it's views
        /// </summary>
        public int Position
        {
            get
            {
                return (position);
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// The value of the primary key, BackgroundId, in metaBackgrounds.
        /// </summary>
        public int BackgroundId
        {
            get
            {
                return (backgroundId);
            }
            set
            {
                backgroundId = value;

                if (view != null)
                {
                    DataTable table = this.GetMetadata().GetBackgroundData();
                    DataRow[] rows = table.Select(string.Format("{0} = {1}", ColumnNames.BACKGROUND_ID, value));

                    if (rows.Length > 0)
                    {
                        int color = (int)rows[0]["Color"];

                        byte a = (byte)(color >> 24);
                        byte r = (byte)(color >> 16);
                        byte g = (byte)(color >> 8);
                        byte b = (byte)(color >> 0);

                        if (r < 64 && g < 64 && b < 64 && a == 255)
                        {
                            flipLabelColor = true;
                        }
                    }
                }
            }
        }

        public bool FlipLabelColor
        {
            get { return flipLabelColor; }
        }

        /// <summary>
        /// Check code that executes after all the data is entered on the page
        /// </summary>
        public string CheckCodeAfter
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

        /// <summary>
        /// Check code that executes before the page is loaded for data entry
        /// </summary>
        public string CheckCodeBefore
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

        /// <summary>
        /// Gets/sets whether this page is in design mode or data entry mode.
        /// </summary>
        public bool DesignMode
        {
            get
            {
                return designMode;
            }
            set
            {
                designMode = value;
            }
        }

        /// <summary>
        /// Gets/sets the Id of the page
        /// </summary>
        public virtual int Id
        {
            get
            {
                return (id);
            }
            set
            {
                id = value;
            }
        }

        /// <summary>
        /// Returns a collection of all page's fields
        /// </summary>
        public NamedObjectCollection<RenderableField> Fields
        {
            get
            {
                NamedObjectCollection<RenderableField> pageFields = new NamedObjectCollection<RenderableField>();
                FieldCollectionMaster fields = this.GetView().Fields;

                foreach (Field field in fields)
                {
                    if (field is RenderableField)
                    {
                        RenderableField renderableField = (RenderableField)field;

                        if (renderableField.Page != null)
                        {
                            if (renderableField.Page.Id == this.Id)
                            {
                                pageFields.Add(renderableField);
                            }
                        }
                    }
                }
                return pageFields;
            }
        }

        /// <summary>
        /// Gets the Field Groups in a Named Object Collection
        /// </summary>
        public NamedObjectCollection<GroupField> GroupFields
        {
            get
            {
                if (groupFields == null)
                {
                    if (!this.GetView().Project.MetadataSource.Equals(MetadataSource.Xml))
                    {
                        groupFields = GetMetadata().GetGroupFields(this);
                    }
                    else
                    {
                        groupFields = new NamedObjectCollection<GroupField>();
                    }
                }
                return (groupFields);
            }
        }

        /// <summary>
        /// Gets field tab order information for the page
        /// </summary>
        public DataSets.TabOrders.TabOrderDataTable TabOrderForFields
        {
            get
            {
                return (GetMetadata().GetTabOrderForFields(this.Id));
            }
        }

        #endregion Public Properties

        #region Private Properties
        #endregion Private Properties

        #region Static Methods

        /// <summary>
        /// Checks the name of a page to make sure the syntax is valid.
        /// </summary>
        /// <param name="projectName">The name of the page to validate</param>
        /// <param name="validationStatus">The message that is passed back to the calling method regarding the status of the validation attempt</param>
        /// <returns>Whether or not the name passed validation; true for a valid name, false for an invalid name</returns>
        public static bool IsValidPageName(string pageName, ref string validationStatus)
        {
            // assume valid by default
            bool valid = true;

            if (string.IsNullOrEmpty(pageName.Trim()))
            {
                validationStatus = SharedStrings.INVALID_PAGE_NAME_BLANK;
                valid = false;                
            }
            else
            {
                for (int i = 0; i < pageName.Length; i++)
                {
                    string viewChar = pageName.Substring(i, 1);
                    System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(viewChar, "[A-Za-z0-9 .]");
                    if (!m.Success)
                    {
                        validationStatus = SharedStrings.INVALID_PAGE_NAME;
                        valid = false;
                    }
                }


                if (pageName.IndexOf(". ") > -1)
                {
                    validationStatus = SharedStrings.INVALID_PAGE_NAME;
                    valid = false;
                }

            }

            return valid;
        }

        #endregion // Static Methods

        #region Public Methods

        /// <summary>
        /// Returns the parent view
        /// </summary>
        /// <returns>The parent view of an Epi.Project.</returns>
        public View GetView()
        {
            return view;
        }

        /// <summary>
        /// Creates field based on the meta field type.
        /// </summary>
        /// <param name="fieldType">Enumeration of Field Types.</param>
        /// <returns>New Field</returns>
        public Field CreateField(MetaFieldType fieldType)
        {
            switch (fieldType)
            {
                case MetaFieldType.Checkbox:
                    return new CheckBoxField(this, viewElement);
                case MetaFieldType.CommandButton:
                    return new CommandButtonField(this, viewElement);
                case MetaFieldType.Date:
                    return new DateField(this, viewElement);
                case MetaFieldType.DateTime:
                    return new DateTimeField(this, viewElement);
                case MetaFieldType.LegalValues:
                    return new DDLFieldOfLegalValues(this, viewElement);
                case MetaFieldType.Codes:
                    return new DDLFieldOfCodes(this, viewElement);
                case MetaFieldType.List:
                    return new DDListField(this, viewElement);
                case MetaFieldType.CommentLegal:
                    return new DDLFieldOfCommentLegal(this, viewElement);
                case MetaFieldType.Grid:
                    return new GridField(this, viewElement);
                case MetaFieldType.Group:
                    return new GroupField(this, viewElement);
                case MetaFieldType.GUID:
                    return new GUIDField(this, viewElement);
                case MetaFieldType.Image:
                    return new ImageField(this, viewElement);
                case MetaFieldType.LabelTitle:
                    return new LabelField(this, viewElement);
                case MetaFieldType.Mirror:
                    return new MirrorField(this, viewElement);
                case MetaFieldType.Multiline:
                    return new MultilineTextField(this, viewElement);
                case MetaFieldType.Number:
                    return new NumberField(this, viewElement);
                case MetaFieldType.Option:
                    return new OptionField(this, viewElement);
                case MetaFieldType.PhoneNumber:
                    return new PhoneNumberField(this, viewElement);
                case MetaFieldType.Relate:
                    return new RelatedViewField(this, viewElement);
                case MetaFieldType.Text:
                    return new SingleLineTextField(this, viewElement);
                case MetaFieldType.TextUppercase:
                    return new UpperCaseTextField(this, viewElement);
                case MetaFieldType.Time:
                    return new TimeField(this, viewElement);
                case MetaFieldType.YesNo:
                    return new YesNoField(this, viewElement);
                default:
                    return new SingleLineTextField(this, viewElement);
            }
        }

        public double MaxTabIndex
        {
            get
            {
                double maxTabIndex = 0;
                foreach (RenderableField field in this.Fields)
                {
                    if (field.TabIndex > maxTabIndex)
                    {
                        maxTabIndex = field.TabIndex;
                    }
                }
                return (maxTabIndex);
            }
        }

        /// <summary>
        /// Copy Page to another page.
        /// </summary>
        /// <param name="other">Destination page.</param>
        public void CopyTo(Page other)
        {
            other.Name = this.Name;
            other.Position = this.Position;
            other.CheckCodeBefore = this.CheckCodeBefore;
            other.CheckCodeAfter = this.CheckCodeAfter;
        }

        /// <summary>
        /// Implements IDisposable.Dispose() method
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Returns the Metadata via a provider
        /// </summary>
        /// <returns>Metadata</returns>
        public IMetadataProvider GetMetadata()
        {
            return view.GetMetadata();
        }

        /// <summary>
        /// Returns the project object
        /// </summary>
        /// <returns>Epi.Project</returns>
        public Project GetProject()
        {
            return view.GetProject();
        }

        /// <summary>
        /// Save page to database.
        /// </summary>
        public void SaveToDb()
        {
            //if this is the first page, insert it			
            if (this.Id == 0)
            {
                GetMetadata().InsertPage(this);

            }
            else
            {
                GetMetadata().UpdatePage(this);
            }
        }

        /// <summary>
        /// Adds a new field to the page
        /// </summary>
        /// <param name="field">Field to add</param>
        public void AddNewField(RenderableField field)
        {
            field.Page = this;

            if (!((field is MirrorField) || (field is LabelField)))
            {
                field.HasTabStop = true;
                field.TabIndex = MaxTabIndex + 1;
            }
            field.SaveToDb();


            // Invalidate the current in-memory field collections and force the app to retreive
            // a fresh collection from the database whenever Page.Fields or Page.RenderableFields
            // is invoked.
            view.MustRefreshFieldCollection = true;
        }

        ///// <summary>
        ///// Adds a new group field to the page
        ///// </summary>
        ///// <param name="field">Field to add</param>
        //public void AddNewGroupField(FieldGroup field)
        //{
        //    field.Page = this;
        //    view.MustRefreshFieldCollection = true;
        //}

        /// <summary>
        /// Updates Renderable Field.
        /// </summary>
        /// <param name="field">The field that is updated</param>
        public void UpdateField(RenderableField field)
        {
            if (!((field is MirrorField) || (field is LabelField)))
            {
                field.HasTabStop = true;
                field.TabIndex = MaxTabIndex + 1;
            }
            field.SaveToDb();
        }

        /// <summary>
        /// Ensures that the page is currently in design mode. Throws an exception otherwise.
        /// </summary>
        public void AssertDesignMode()
        {
            if (!DesignMode)
            {
                throw new System.ApplicationException(SharedStrings.NOT_VALID_IN_FORM_DESIGN_MODE);
            }
        }

        /// <summary>
        /// Ensures that the page is currently in data entry mode. Throws an exception otherwise.
        /// </summary>
        public void AssertDataEntryMode()
        {
            if (DesignMode)
            {
                throw new System.ApplicationException(SharedStrings.NOT_VALID_IN_DATA_ENTRY_MODE);
            }
        }

        /// <summary>
        /// Deletes fields from a page
        /// </summary>
        public void DeleteFields()
        {
            this.GetMetadata().DeleteFields(this);
        }

        #endregion Public Methods

        #region Event Handlers

        private void Field_RelatedViewRequested(object sender, ChildViewRequestedEventArgs e)
        {
            if (ChildViewRequested != null)
            {
                ChildViewRequested(sender, e);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the page name for XML metadata view pages
        /// </summary>
        /// <param name="view">The view the page belongs to</param>
        /// <param name="pageId">The page id</param>
        /// <returns>Page name.</returns>
        private string GetPageName(View view, int pageId)
        {
            if (this.view.GetProject().MetadataSource.Equals(MetadataSource.Xml))
            {
                XmlNode pagesNode = view.ViewElement.SelectSingleNode("Pages");
                XmlNodeList pageNodeList = pagesNode.SelectNodes("//Page[@PageId= '" + pageId + "']");
                if (pageNodeList != null)
                {
                    foreach (XmlNode pageNode in pageNodeList)
                    {
                        this.name = pageNode.Attributes["Name"].Value;
                    }
                }
            }
            return this.name;
        }
        #endregion //Private Methods
    }
}
