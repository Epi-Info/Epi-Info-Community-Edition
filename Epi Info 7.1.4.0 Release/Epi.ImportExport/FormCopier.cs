#region Using
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
#endregion // Using

namespace Epi.ImportExport
{
    /// <summary>
    /// This class is used to copy a form and its associated data table from one project to a new, completely
    /// blank project. Intended to be used as part of the package for transport feature in the Enter module
    /// and called by the Project Packager series of classes. Must be given a source project, a destination
    /// project, and the form that will be copied.
    /// -- E. Knudsen, 2012
    /// </summary>
    /// <remarks>
    /// The form data copier should not only copy the form and its data, but also any related data to include
    /// related forms and grid data.
    /// </remarks>
    public class FormCopier : IDisposable
    {
        #region Private Attributes
        private Project sourceProject;
        private Project destinationProject;
        private View sourceView;
        private View destinationView;
        private Query selectQuery;
        private Dictionary<string, List<string>> columnsToNull;
        private Dictionary<string, List<string>> gridColumnsToNull;
        #endregion // Private Attributes

        #region Events
        public event SetProgressBarDelegate SetProgressBar;
        public event UpdateStatusEventHandler SetStatus;
        public event UpdateStatusEventHandler AddStatusMessage;
        public event SetMaxProgressBarValueDelegate SetMaxProgressBarValue;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// DataCopier Constructor
        /// </summary>
        /// <param name="sourceProject">The source project</param>
        /// <param name="destinationProject">The destination project</param>
        /// <param name="sourceView">The source form to copy</param>
        /// <remarks>
        /// The destination project should always be Access-based, but the code may still work (perhaps with
        /// some minor tweaks) if it's in some other database format.
        /// </remarks>
        public FormCopier(Project sourceProject, Project destinationProject, View sourceView)
        {
            this.SourceProject = sourceProject;
            this.DestinationProject = destinationProject;
            this.SourceView = sourceView;
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the columns that should be nulled.
        /// </summary>
        public Dictionary<string, List<string>> ColumnsToNull
        {
            get
            {
                return this.columnsToNull;
            }
            set
            {
                this.columnsToNull = value;
            }
        }

        /// <summary>
        /// Gets/sets the grid columns that should be nulled.
        /// </summary>
        public Dictionary<string, List<string>> GridColumnsToNull
        {
            get
            {
                return this.gridColumnsToNull;
            }
            set
            {
                this.gridColumnsToNull = value;
            }
        }

        /// <summary>
        /// Gets/sets the select query used to filter records during the copying process.
        /// </summary>
        public Query SelectQuery
        {
            get
            {
                return this.selectQuery;
            }
            set
            {
                if (!value.SqlStatement.ToLower().Trim().StartsWith("select"))
                {
                    throw new ArgumentException(ImportExportSharedStrings.ERROR_INVALID_SELECT_QUERY);
                }
                else
                {
                    this.selectQuery = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the source project
        /// </summary>
        public Project SourceProject
        {
            get
            {
                return this.sourceProject;
            }
            set
            {
                this.sourceProject = value;
            }
        }

        /// <summary>
        /// Gets/sets the source form
        /// </summary>
        public View SourceView
        {
            get
            {
                return this.sourceView;
            }
            set
            {
                this.sourceView = value;
            }
        }

        /// <summary>
        /// Gets/sets the destination project
        /// </summary>
        public Project DestinationProject
        {
            get
            {
                return this.destinationProject;
            }
            set
            {
                this.destinationProject = value;
            }
        }

        /// <summary>
        /// Gets the source project's DB Driver
        /// </summary>
        public IDbDriver SourceDbDriver
        {
            get
            {
                return this.SourceProject.CollectedData.GetDbDriver();
            }
        }

        /// <summary>
        /// Gets the destination project's DB Driver
        /// </summary>
        public IDbDriver DestinationDbDriver
        {
            get
            {
                return this.DestinationProject.CollectedData.GetDbDriver();
            }
        }
        #endregion // Public Properties

        #region Private Methods
        /// <summary>
        /// Adds a status message
        /// </summary>
        /// <param name="message">The message</param>
        private void OnAddStatusMessage(string message)
        {
            if (AddStatusMessage != null)
            {
                AddStatusMessage(message);
            }
        }

        /// <summary>
        /// Sets status message
        /// </summary>
        /// <param name="message">The message</param>
        private void OnSetStatusMessage(string message)
        {
            if (AddStatusMessage != null && SetStatus != null)
            {
                AddStatusMessage(message);
                SetStatus(message);
            }
        }

        /// <summary>
        /// Sets progess bar value
        /// </summary>
        /// <param name="progress">The progress</param>
        private void OnSetProgress(double progress)
        {
            if (SetProgressBar != null)
            {
                SetProgressBar(progress);
            }
        }

        /// <summary>
        /// Sets progess bar max value
        /// </summary>
        /// <param name="maxProgress">The max progress</param>
        private void OnSetMaxProgressBarValue(double maxProgress)
        {
            if (SetMaxProgressBarValue != null)
            {
                SetMaxProgressBarValue(maxProgress);
            }
        }

        /// <summary>
        /// Sets up the pages in the destination project's metadata
        /// </summary>
        /// <param name="typedMetadata">The metadata to use for the destination project</param>
        private void PreprocessPages(Epi.Data.Services.MetadataDbProvider typedMetadata)
        {
            SortedDictionary<int, Page> pageDictionary = new SortedDictionary<int, Page>();
            int highestPageId = 0;
            foreach (View view in sourceProject.Views)
            {
                foreach (Page page in view.Pages)
                {
                    if (page.Id > highestPageId)
                    {
                        highestPageId = page.Id;
                    }
                    pageDictionary.Add(page.Id, page);
                }
            }

            List<int> pagesToRemove = new List<int>();
            for (int i = 1; i <= highestPageId; i++)
            {
                if (!pageDictionary.ContainsKey(i))
                {
                    Page page = new Page(sourceView, i);
                    pageDictionary.Add(i, page);
                    pagesToRemove.Add(i);
                }
            }

            foreach (KeyValuePair<int, Page> kvp in pageDictionary)
            {
                Page page = kvp.Value;
                typedMetadata.InsertPage(page);

                if (pagesToRemove.Contains(kvp.Key))
                {
                    typedMetadata.DeletePage(page);
                }
            }
        }

        /// <summary>
        /// Sets up the forms in the destination project's metadata
        /// </summary>
        /// <param name="typedMetadata">The metadata to use for the destination project</param>
        private void PreprocessForms(Epi.Data.Services.MetadataDbProvider typedMetadata)
        {
            /* 
             * This method may not make a lot of sense at first glance, but the idea is to
             * make sure that the View ID values in the destination database (found in the 
             * metaViews table) match those in the source. Imagine the following scenario:
             * The user creates ten forms, then wants to create a copy of the fifth one.
             * In the source project, the ID of this view is 5. In the destination, if we
             * simply insert the data from that row, the view ID value will become 1 because
             * the ID column in metaViews is an autonumber. This ID column is used by other
             * tables to determine which fields and pages go where. Rather than trying to
             * keep track of all these IDs and re-assign them appropriately in memory, then
             * update them in the database, we're instead going to add filler records into
             * the metaViews and metaPages tables so that the actual rows we want to copy
             * have matching ID values. In the aforementioned scenario, four dummy rows
             * would be added to metaViews to make sure the view we actually want to copy
             * ends up with an ID value of five. Those dummy rows will be removed later
             * during the metadata cleanup phase.
             */

            SortedDictionary<int, View> formDictionary = new SortedDictionary<int, View>();
            int highestFormId = 0;
            foreach (View form in sourceProject.Views)
            {
                if (form.Id > highestFormId)
                {
                    highestFormId = form.Id;
                }
                formDictionary.Add(form.Id, form);                
            }

            List<int> formsToRemove = new List<int>();
            for (int i = 1; i <= highestFormId; i++)
            {
                if (!formDictionary.ContainsKey(i))
                {
                    View form = new View(sourceProject);
                    form.Name = "_____VIEW______" + i.ToString();
                    formDictionary.Add(i, form);
                    formsToRemove.Add(i);
                }
            }

            foreach (KeyValuePair<int, View> kvp in formDictionary)
            {
                View view = kvp.Value;
                typedMetadata.InsertView(view);

                if (formsToRemove.Contains(kvp.Key))
                {
                    typedMetadata.DeleteView(view.Name);
                }
            }
        }

        /// <summary>
        /// Sets up the fields in the destination project's metadata
        /// </summary>
        /// <param name="form">The form whose fields should be processed</param>
        /// <param name="typedMetadata">The metadata to use for the destination project</param>
        private void PreprocessFields(View form, Epi.Data.Services.MetadataDbProvider typedMetadata)
        {
            foreach (Epi.Fields.Field field in form.Fields)
            {
                switch (field.FieldType)
                {
                    case MetaFieldType.Checkbox: // 1
                        typedMetadata.CreateField(field as CheckBoxField);
                        break;
                    case MetaFieldType.YesNo: // 2
                        typedMetadata.CreateField(field as YesNoField);
                        break;
                    case MetaFieldType.Text: // 3
                    case MetaFieldType.TextUppercase:
                        typedMetadata.CreateField(field as SingleLineTextField);
                        break;
                    case MetaFieldType.Number: // 4
                        typedMetadata.CreateField(field as NumberField);
                        break;
                    case MetaFieldType.Date: // 5
                        typedMetadata.CreateField(field as DateField);
                        break;
                    case MetaFieldType.DateTime: // 6
                        typedMetadata.CreateField(field as DateTimeField);
                        break;
                    case MetaFieldType.Time: // 7
                        typedMetadata.CreateField(field as TimeField);
                        break;
                    case MetaFieldType.Codes: // 8
                        typedMetadata.CreateField(field as DDLFieldOfCodes);
                        break;
                    case MetaFieldType.CommandButton: // 9
                        typedMetadata.CreateField(field as CommandButtonField);
                        break;
                    case MetaFieldType.CommentLegal: // 10
                        typedMetadata.CreateField(field as DDLFieldOfCommentLegal);
                        break;
                    case MetaFieldType.LegalValues: // 11
                        typedMetadata.CreateField(field as DDLFieldOfLegalValues);
                        break;
                    case MetaFieldType.LabelTitle: // 12
                        typedMetadata.CreateField(field as LabelField);
                        break;
                    case MetaFieldType.Grid: // 13
                        int id = typedMetadata.CreateField(field as GridField);
                        GridField gridField = field as GridField;
                        int originalId = gridField.Id;
                        
                        foreach (GridColumnBase gc in gridField.Columns)
                        {
                            gridField.Id = id;
                            if (gc is TextColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as TextColumn);
                            }
                            else if (gc is ContiguousColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as ContiguousColumn);
                            }
                            else if (gc is DDLColumnOfCommentLegal)
                            {
                                typedMetadata.CreateGridColumn(gc as DDLColumnOfCommentLegal);
                            }
                            else if (gc is DDLColumnOfLegalValues)
                            {
                                typedMetadata.CreateGridColumn(gc as DDLColumnOfLegalValues);
                            }
                            else if (gc is NumberColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as NumberColumn);
                            }
                            else if (gc is PhoneNumberColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as PhoneNumberColumn);
                            }

                            else if (gc is UniqueKeyColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as UniqueKeyColumn);
                            }
                            else if (gc is UniqueRowIdColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as UniqueRowIdColumn);
                            }
                            else if (gc is GlobalRecordIdColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as GlobalRecordIdColumn);
                            }
                            else if (gc is ForeignKeyColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as ForeignKeyColumn);
                            }
                            else if (gc is RecStatusColumn)
                            {
                                typedMetadata.CreateGridColumn(gc as RecStatusColumn);
                            }
                            gridField.Id = originalId;
                        }
                        gridField.Id = originalId;
                        break;
                    case MetaFieldType.Group: // 14
                        typedMetadata.CreateField(field as GroupField);
                        break;
                    case MetaFieldType.GUID: // 15
                        typedMetadata.CreateField(field as GUIDField);
                        break;
                    case MetaFieldType.Image: // 16
                        typedMetadata.CreateField(field as ImageField);
                        break;
                    //case MetaFieldType.List: // 17
                    //    typedMetadata.CreateField(field as DDListField);
                    //    break;
                    case MetaFieldType.Mirror: // 18
                        typedMetadata.CreateField(field as MirrorField);
                        break;
                    case MetaFieldType.Multiline: // 19
                        typedMetadata.CreateField(field as MultilineTextField);
                        break;
                    case MetaFieldType.Option: // 20
                        typedMetadata.CreateField(field as OptionField);
                        break;
                    case MetaFieldType.PhoneNumber: // 21
                        typedMetadata.CreateField(field as PhoneNumberField);
                        break;
                    case MetaFieldType.Relate: // 22
                        typedMetadata.CreateField(field as RelatedViewField);
                        break;
                    //case MetaFieldType.TextUppercase: // 23
                    //    typedMetadata.CreateField(field as UpperCaseTextField);
                    //    break;
                    case MetaFieldType.UniqueKey: // 24
                        typedMetadata.CreateField(field as UniqueKeyField);
                        break;
                    case MetaFieldType.ForeignKey: // 25
                        typedMetadata.CreateField(field as ForeignKeyField);
                        break;
                    case MetaFieldType.GlobalRecordId: // 26
                        typedMetadata.CreateField(field as GlobalRecordIdField);
                        break;
                    case MetaFieldType.RecStatus: // 27
                        typedMetadata.CreateField(field as RecStatusField);
                        break;
                }
            }
        }

        /// <summary>
        /// Sets up the metadata in the destination project.
        /// </summary>
        /// <param name="formsToProcess">The forms to process for metadata</param>
        private void SetupMetadata(List<View> formsToProcess)
        {
            OnAddStatusMessage(ImportExportSharedStrings.SETTING_UP_METADATA_START);

            // Create an in-memory copy of the source project
            Project sourceProjectCopy = Util.CreateProjectFileFromDatabase(SourceProject.CollectedDataConnectionString, false, sourceProject.Location, sourceProject.Name);
            sourceProjectCopy.Metadata.AttachDbDriver(sourceProjectCopy.CollectedData.GetDbDriver());
            sourceProjectCopy.LoadViews();

            // Remove the views from the source copy that we don't need
            List<View> viewsToKeep = sourceView.GetDescendantViews();
            View[] viewCollection = new View[sourceProjectCopy.Views.Count];

            sourceProjectCopy.Views.CopyTo(viewCollection, 0);

            foreach (View view in viewCollection)
            {
                if (!viewsToKeep.Contains(view))
                {
                    sourceProjectCopy.Views.Remove(view.Name);
                }
            }

            // set up ID dictionaries, as the IDs come from autonumber fields in the tables and may be mis-matching at this point.
            Dictionary<int, int> formIds = new Dictionary<int, int>();
            Dictionary<int, int> pageIds = new Dictionary<int, int>();
            Dictionary<int, int> fieldIds = new Dictionary<int, int>();

            // Set up the metadata
            DestinationProject.MetadataSource = MetadataSource.SameDb;
            Epi.Data.Services.MetadataDbProvider typedMetadata = DestinationProject.Metadata as Epi.Data.Services.MetadataDbProvider;
            typedMetadata.AttachDbDriver(DestinationProject.CollectedData.GetDbDriver());
            typedMetadata.CreateMetadataTables();
            //typedMetadata.InsertView(SourceView);

            PreprocessForms(typedMetadata);
            PreprocessPages(typedMetadata);
            PreprocessFields(SourceView, typedMetadata);

            foreach (View form in formsToProcess)
            {
                PreprocessFields(form, typedMetadata);
            }

            OnAddStatusMessage(ImportExportSharedStrings.SETTING_UP_METADATA_END);
            sourceProjectCopy.Dispose();
        }

        /// <summary>
        /// Cleans up the metadata of any unused forms and pages.
        /// </summary>        
        private void CleanupMetadata()
        {
            Epi.Data.Services.MetadataDbProvider typedMetadata = DestinationProject.Metadata as Epi.Data.Services.MetadataDbProvider;
            List<View> descendantViews = destinationView.GetDescendantViews();
            List<View> viewsToRemove = new List<View>();

            foreach (View view in destinationProject.Views)
            {
                if (!descendantViews.Contains(view) && destinationView.Name != view.Name)
                {
                    viewsToRemove.Add(view);
                }
            }

            foreach (View view in viewsToRemove)
            {
                typedMetadata.DeleteView(view.Name);

                foreach (Page page in view.Pages)
                {
                    typedMetadata.DeletePage(page);
                }
            }
        }

        #endregion // Private Methods

        #region Public Methods
        /// <summary>
        /// Releases all resources used by the form data importer
        /// </summary>
        public void Dispose() // Implements IDisposable.Dispose
        {
            SetProgressBar = null;
            SetStatus = null;
            AddStatusMessage = null;

            if (columnsToNull != null)
            {
                columnsToNull.Clear();
                columnsToNull = null;
            }
            if (gridColumnsToNull != null)
            {
                gridColumnsToNull.Clear();
                gridColumnsToNull = null;
            }
            if (selectQuery != null)
            {
                selectQuery = null;
            }

            destinationProject = null;
            destinationView = null;
        }

        /// <summary>
        /// Starts the form data and metadata copying process.
        /// </summary>
        public void Copy()
        {
            // Figure out which forms should be processed. Remember that this is a form-to-form copier, not a project-to-project
            // copier, so it's vital that only the selected form and its descendants are included in the resulting copy.
            List<View> formsToProcess = new List<View>();

            // Iterate over all the views in the source project...
            foreach (View view in sourceProject.Views)
            {
                if (view.Name == sourceView.Name)
                {
                    continue;
                }

                // If the form is a descendant of the source view...
                if (ImportExportHelper.IsFormDescendant(view, sourceView))
                {
                    // Include this form in the list of forms to process
                    formsToProcess.Add(view);
                }
            }

            // Setup the metadata in the project with the appropriate rows in metaFields, metaPages, and metaViews to support the
            // list of views that should be included in the copy.
            SetupMetadata(formsToProcess);            

            // None of the forms in the destination project have data tables. Without data tables, no data can be copied. Create
            // the tables for the current form and all of its descendants.
            destinationView = destinationProject.Views[SourceView.Name];
            destinationProject.CollectedData.CreateDataTableForView(destinationView, 1);

            foreach(View form in destinationView.GetDescendantViews()) 
            {
                destinationProject.CollectedData.CreateDataTableForView(form, 1);
            }            

            // Create a new instance of the form data importer class. This class is used to carry out data imports of form data into
            // other Epi Info 7 forms.
            FormDataImporter fdi = new FormDataImporter(sourceProject, destinationProject, destinationView, formsToProcess);

            fdi.SetProgressBar += new SetProgressBarDelegate(OnSetProgress);
            fdi.SetStatus += new UpdateStatusEventHandler(OnSetStatusMessage);
            fdi.AddStatusMessage += new UpdateStatusEventHandler(OnAddStatusMessage);
            fdi.SetMaxProgressBarValue += new SetMaxProgressBarValueDelegate(OnSetMaxProgressBarValue);

            if (this.SelectQuery != null)
            {
                fdi.SelectQuery = this.SelectQuery; // Pass the select query that will be used to filter out certain rows.
            }
            fdi.ColumnsToNull = this.ColumnsToNull; // Tell the data importer which columns to skip.
            fdi.GridColumnsToNull = this.GridColumnsToNull; // Tell the data importer which grid columns to skip.
            fdi.Update = false;
            fdi.Append = true;
            fdi.ImportFormData(); // Start the import process.
            fdi.Dispose(); // Dispose the form data importer.

            // Clean up the metadata to remove any unused rows.
            CleanupMetadata();
        }

        #endregion // Public Methods

        #region Junk Pile
        ///// <summary>
        ///// DataCopier Constructor
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="destination"></param>
        //public DataCopier(string source, string destination)
        //{
        //    sourceConnectionString = source;
        //    destinationConnectionString = destination;
        //}
        //#endregion Constructor

        //#region Public Methods

        ///// <summary>
        ///// Method to copy a table from source connection to destination connection
        ///// </summary>
        ///// <param name="table"></param>
        //public void CopyTable(string table)
        //{
        //    using (SqlConnection source = new SqlConnection(sourceConnectionString))
        //    {
        //        string sql = string.Format("select * from [{0}]", table);
        //        SqlCommand command = new SqlCommand(sql, source);
        //        source.Open();
        //        IDataReader dr = command.ExecuteReader();
        //        using (SqlBulkCopy copy = new SqlBulkCopy(destinationConnectionString))
        //        {
        //            copy.DestinationTableName = table;
        //            copy.WriteToServer(dr);
        //        }
        //    }
        //}        
        //#endregion Public Methods
    }

    //Copying table data is as simple as:
    //
    // 
    //
    //DataCopier copier = new DataCopier(".ConnectionString1.", ".ConnectionString2.");
    //copier.CopyTable(".TableName.");
    #endregion // Junk Pile
}
