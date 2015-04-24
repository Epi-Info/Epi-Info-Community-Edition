using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Xml;
using Epi.Data;
using Epi.Fields;
using Epi.ImportExport.Filters;

namespace Epi.ImportExport.ProjectPackagers
{
    /// <summary>
    /// A class used to import data from an Epi Info 7 data package (represented as Xml) into the specified Epi Info 7 form and any of its descendant forms.
    /// </summary>
    /// <remarks>
    /// The ImportInfo object contains information about what was imported and can be accessed after the import 
    /// process has been completed.
    /// </remarks>
    public class XmlDataUnpackager
    {
        #region Events
        public event SetProgressBarDelegate UpdateProgress;
        public event SimpleEventHandler ResetProgress;
        public event UpdateStatusEventHandler StatusChanged;
        public event UpdateStatusEventHandler MessageGenerated;
        public event EventHandler ImportFinished;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="destinationForm">The form that will receive the incoming data</param>
        /// <param name="xmlDataPackage">The data package in Xml format</param>
        public XmlDataUnpackager(View destinationForm, XmlDocument xmlDataPackage)
        {
            #region Input Validation
            if (destinationForm == null) { throw new ArgumentNullException("sourceForm"); }
            if (xmlDataPackage == null) { throw new ArgumentNullException("xmlDataPackage"); }
            #endregion // Input Validation

            DestinationForm = destinationForm;
            DestinationProject = DestinationForm.Project;
            XmlDataPackage = xmlDataPackage;
            KeyFields = new List<Field>();
            Update = true;
            Append = true;
            Delete = true;
            Undelete = true;
            RecordProcessingScope = Epi.RecordProcessingScope.Both;
        }
        #endregion // Constructors

        #region Properties
        /// <summary>
        /// Gets/sets the results of the unpackaging process
        /// </summary>
        public ImportInfo ImportInfo { get; protected set; }
        /// <summary>
        /// Gets/sets the source form within the project that will be used for the packaging routine.
        /// </summary>
        public View DestinationForm { get; protected set; }

        /// <summary>
        /// Gets/sets whether to append unmatched records during the import.
        /// </summary>
        public bool Append { get; set; }

        /// <summary>
        /// Gets/sets whether to update matching records during the import.
        /// </summary>
        public bool Update { get; set; }

        /// <summary>
        /// Gets/sets whether to include records that are marked for deletion
        /// </summary>
        public RecordProcessingScope RecordProcessingScope { get; set; }

        /// <summary>
        /// Gets/sets whether to soft-delete records during the import.
        /// </summary>
        public bool Delete { get; set; }

        /// <summary>
        /// Gets/sets whether to undo soft-deletions of records during the import.
        /// </summary>
        public bool Undelete { get; set; }

        /// <summary>
        /// Gets/sets the source project for the packaging routine
        /// </summary>
        protected Project DestinationProject { get; set; }

        /// <summary>
        /// Gets/sets the XmlDataPackage that will be used to import data
        /// </summary>
        protected XmlDocument XmlDataPackage { get; set; }

        /// <summary>
        /// Gets/sets the name of the current package being imported.
        /// </summary>
        protected string PackageName { get; set; }

        /// <summary>
        /// Gets whether or not the package process is proceeding with custom keys instead of the built-in GlobalRecordId key field.
        /// </summary>
        public bool IsUsingCustomMatchkeys { get { return (this.KeyFields.Count > 0); } }

        /// <summary>
        /// Gets/sets the list of fields to use as match keys. If none are specified, the GlobalRecordId field will be used as the match key.
        /// </summary>
        public List<Field> KeyFields { get; set; }

        /// <summary>
        /// Gets/sets a connection to the database
        /// </summary>
        /// <remarks>
        /// Intended only for use with Ole-based databases for performance reasons; keeping this connection
        /// open and using it for the DB calls is much faster as opposed to relying on the data drivers, which 
        /// open and close the connection each time they are used.
        /// </remarks>
        protected IDbConnection Conn { get; set; }
        #endregion // Properties

        #region Public Methods
        /// <summary>
        /// Unpackages the specified XmlDocument and imports the data into the specified Epi Info 7 form (and any descendant forms).
        /// </summary>
        public virtual void Unpackage()
        {
            ImportInfo = new ImportInfo();
            ImportInfo.UserID = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
            ImportInfo.ImportInitiated = DateTime.Now;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            using (Conn = DestinationProject.CollectedData.GetDatabase().GetConnection())
            {
                Conn.Open();
                CheckForProblems();

                foreach (XmlNode node in XmlDataPackage.ChildNodes)
                {
                    if (node.Name.Equals("datapackage", StringComparison.OrdinalIgnoreCase))
                    {
                        PackageName = node.Attributes["Name"].Value;
                        if (StatusChanged != null) { StatusChanged(string.Format(UnpackagerStrings.IMPORT_INITIATED, PackageName, ImportInfo.UserID)); }
                        if (MessageGenerated != null) { MessageGenerated(string.Format(UnpackagerStrings.IMPORT_INITIATED, PackageName, ImportInfo.UserID)); }

                        foreach (XmlNode dpNode in node.ChildNodes)
                        {
                            if (dpNode.Name.Equals("form", StringComparison.OrdinalIgnoreCase))
                            {
                                List<PackageFieldData> records = new List<PackageFieldData>();

                                XmlNode formNode = dpNode;
                                View form = DestinationProject.Views[formNode.Attributes["Name"].Value.ToString()]; //DestinationForm; 

                                if (formNode.ChildNodes.Count >= 2) // if less than 2 then the package is incomplete and can't be imported; throw exception if so?
                                {
                                    XmlNode metaDataNode = formNode.SelectSingleNode("FieldMetadata");
                                    XmlNode dataNode = formNode.SelectSingleNode("Data");
                                    XmlNode keyNode = formNode.SelectSingleNode("KeyFields");

                                    if (keyNode != null)
                                    {
                                        foreach (XmlNode key in keyNode.ChildNodes)
                                        {
                                            string fieldName = key.Attributes["Name"].Value;
                                            Field keyField = form.Fields[fieldName];
                                            this.KeyFields.Add(keyField);
                                        }
                                    }

                                    Dictionary<string, Page> pageDictionary = new Dictionary<string, Page>();

                                    foreach (XmlElement fieldMetadataElement in metaDataNode.ChildNodes)
                                    {
                                        string fieldName = fieldMetadataElement.Attributes["Name"].InnerText;
                                        foreach (Page page in form.Pages)
                                        {
                                            if (page.Fields.Contains(fieldName))
                                            {
                                                pageDictionary.Add(fieldName, page);
                                                break;
                                            }
                                        }
                                    }

                                    foreach (XmlElement recordElement in dataNode.ChildNodes)
                                    {
                                        if (recordElement.Name.Equals("Record"))
                                        {
                                            string guid = recordElement.Attributes[0].Value.ToString();
                                            string recordStatus = String.Empty;

                                            if (recordElement.HasAttribute("RecStatus"))
                                            {
                                                recordStatus = recordElement.GetAttribute("RecStatus");
                                            }

                                            // if we're processing only deleted records and the record status is undeleted, OR if we're processing only
                                            // undeleted (that is, active) records and the record status is deleted, then skip adding this to the list 
                                            // of records to process
                                            if ((RecordProcessingScope == Epi.RecordProcessingScope.Deleted && recordStatus.Equals("1")) || 
                                                (RecordProcessingScope == Epi.RecordProcessingScope.Undeleted && recordStatus.Equals("0")))
                                            {
                                                continue;
                                            }

                                            Dictionary<Field, object> customKey = new Dictionary<Field, object>();

                                            #region Custom Match Keys

                                            int keysFound = 0;

                                            if (IsUsingCustomMatchkeys == true)
                                            {
                                                foreach (XmlNode fieldNode in recordElement.ChildNodes)
                                                {
                                                    string fieldName = string.Empty;
                                                    if (fieldNode.Name.Equals("Field"))
                                                    {
                                                        fieldName = fieldNode.Attributes["Name"].Value;

                                                        if (form.Fields.Contains(fieldName))
                                                        {
                                                            Field field = form.Fields[fieldName];
                                                            if (KeyFields.Contains(field))
                                                            {
                                                                keysFound++;
                                                                customKey.Add(field, FormatFieldData(fieldName, fieldNode.InnerText));
                                                            }
                                                        }
                                                    }

                                                    if (keysFound == KeyFields.Count) break; // stop looping if we have all the keys
                                                }
                                            }

                                            #endregion // Custom Match Keys

                                            foreach (XmlNode fieldNode in recordElement.ChildNodes)
                                            {
                                                string fieldName = string.Empty;
                                                if (fieldNode.Name.Equals("Field"))
                                                {
                                                    fieldName = fieldNode.Attributes[0].Value;

                                                    if (pageDictionary.ContainsKey(fieldName)) // needed in case a field exists in the package but not on the form
                                                    {
                                                        Page destinationPage = pageDictionary[fieldName];

                                                        object fieldValue = FormatFieldData(fieldName, fieldNode.InnerText);
                                                        PackageFieldData fieldData = new PackageFieldData();
                                                        fieldData.FieldName = fieldName;
                                                        fieldData.FieldValue = fieldValue;
                                                        fieldData.RecordGUID = guid;
                                                        fieldData.Page = destinationPage;
                                                        fieldData.KeyValues = customKey;
                                                        records.Add(fieldData);
                                                    }
                                                }
                                            }
                                        }
                                        else if (recordElement.Name.Equals("Grid"))
                                        {
                                            #region Grid Records
                                            List<PackageFieldData> gridRecords = new List<PackageFieldData>();
                                            XmlNode gridNode = recordElement;
                                            if (form.Fields.Contains(gridNode.Attributes["Name"].Value))
                                            {
                                                Field field = form.Fields.GridFields[gridNode.Attributes["Name"].Value];
                                                GridField gridField = field as GridField;
                                                if (gridField != null)
                                                {
                                                    if (gridNode.ChildNodes.Count == 2)
                                                    {
                                                        XmlNode gridMetaDataNode = gridNode.ChildNodes[0];
                                                        XmlNode gridDataNode = gridNode.ChildNodes[1];

                                                        foreach (XmlElement gridRecordElement in gridDataNode.ChildNodes)
                                                        {
                                                            string gridGuid = gridRecordElement.Attributes["UniqueRowId"].Value.ToString();
                                                            foreach (XmlNode gridColumnNode in gridRecordElement.ChildNodes)
                                                            {
                                                                string columnName = string.Empty;
                                                                if (gridColumnNode.Name.Equals("GridColumn"))
                                                                {
                                                                    columnName = gridColumnNode.Attributes[0].Value;

                                                                    object gridColumnValue = gridColumnNode.InnerText;

                                                                    PackageFieldData fieldData = new PackageFieldData();
                                                                    fieldData.FieldName = columnName;
                                                                    fieldData.FieldValue = gridColumnValue;
                                                                    fieldData.RecordGUID = gridGuid;

                                                                    gridRecords.Add(fieldData);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    if (gridRecords.Count > 0)
                                                    {
                                                        ImportGridRecords(gridField, gridNode, gridRecords);
                                                        ImportInfo.GridsProcessed++;
                                                    }
                                                }
                                            }
                                            #endregion // Grid Records
                                        }
                                    }
                                }

                                ImportInfo.FormsProcessed++;
                                if (StatusChanged != null) { StatusChanged(string.Format(UnpackagerStrings.IMPORT_START_FORM, PackageName, form.Name)); }
                                if (MessageGenerated != null) { MessageGenerated(string.Format(UnpackagerStrings.IMPORT_START_FORM, PackageName, form.Name)); }
                                ImportRecords(form, formNode, records);
                                if (StatusChanged != null) { StatusChanged(string.Format(UnpackagerStrings.IMPORT_END_FORM, PackageName, form.Name, ImportInfo.RecordsUpdated[form].ToString(), ImportInfo.RecordsAppended[form].ToString())); }
                                if (MessageGenerated != null) { MessageGenerated(string.Format(UnpackagerStrings.IMPORT_END_FORM, PackageName, form.Name, ImportInfo.RecordsUpdated[form].ToString(), ImportInfo.RecordsAppended[form].ToString())); }
                            }
                        }
                    }
                }
            }

            if (StatusChanged != null) { StatusChanged(string.Format(UnpackagerStrings.IMPORT_END, PackageName)); }
            if (MessageGenerated != null) { MessageGenerated(string.Format(UnpackagerStrings.IMPORT_END, PackageName)); }

            sw.Stop();
            ImportInfo.TimeElapsed = sw.Elapsed;
            ImportInfo.ImportCompleted = DateTime.Now;
            ImportInfo.Succeeded = true;

            if (ImportFinished != null) { ImportFinished(this, new EventArgs()); }
        }
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Checks for problems in the destination form and the packaged data, and for any problematic
        /// iscrepancies in the metadata.
        /// </summary>
        protected virtual void CheckForProblems()
        {
            XmlNodeList xnList = XmlDataPackage.SelectNodes("/DataPackage/Form");
            foreach (XmlNode xn in xnList)
            {
                View form = DestinationProject.Views[xn.Attributes["Name"].Value.ToString()];

                XmlNode fieldMetaDataNode = xn.FirstChild;

                foreach (XmlNode fieldInfoNode in fieldMetaDataNode.ChildNodes)
                {
                    string fieldName = fieldInfoNode.Attributes["Name"].Value.ToString();
                    string fieldType = fieldInfoNode.Attributes["FieldType"].Value.ToString();
                    string fieldPage = fieldInfoNode.Attributes["Page"].Value.ToString();

                    if (form.Fields.Contains(fieldName))
                    {
                        Field field = form.Fields[fieldName];
                        string t = field.FieldType.ToString();

                        if (!fieldType.Equals(t, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!(fieldType.Equals("Text") &&
                                (t.Equals("CommentLegal") ||
                                t.Equals("LegalValues") ||
                                t.Equals("Text") ||
                                t.Equals("Codes")))
                                &&
                                !(fieldType.Equals("LegalValues") &&
                                (t.Equals("CommentLegal") ||
                                t.Equals("LegalValues") ||
                                t.Equals("Text") ||
                                t.Equals("Codes")))
                                &&
                                !(fieldType.Equals("Codes") &&
                                (t.Equals("CommentLegal") ||
                                t.Equals("LegalValues") ||
                                t.Equals("Text") ||
                                t.Equals("Codes")))
                                &&
                                !(fieldType.Equals("CommentLegal") &&
                                (t.Equals("CommentLegal") ||
                                t.Equals("LegalValues") ||
                                t.Equals("Text") ||
                                t.Equals("Codes")))
                                )
                            {
                                ImportInfo.Succeeded = false;
                                string message = string.Format(ImportExportSharedStrings.UNPACKAGE_PROBLEM_CHECK_ERROR_E3013, fieldName, form.Name, fieldType, t);
                                ImportInfo.AddError(message, "3013");
                                throw new ApplicationException(message);
                            }
                        }
                    }
                    else
                    {
                        string message = string.Format(ImportExportSharedStrings.UNPACKAGE_PROBLEM_CHECK_ERROR_E3014, fieldName, form.Name);
                        ImportInfo.AddError(message, "3014");
                        if (MessageGenerated != null) { MessageGenerated(message); }
                    }
                }
            }
        }

        /// <summary>
        /// Begins the process of importing data into each of the page tables on the form
        /// </summary>
        /// <param name="form">The form that will receive the data</param>
        /// <param name="records">The data to be imported</param>
        /// <param name="destinationGuids">A dictionary of GUIDs in the destination project; the key represents the GUID itself and the value (either true or false) represents whether or not to process that record</param>
        protected virtual void ImportRecordsToPageTables(View form, List<PackageFieldData> records, Dictionary<string, bool> destinationGuids)
        {
            if (records.Count == 0) { return; }

            if (Conn.State != ConnectionState.Open) { Conn.Open(); }

            IDbDriver db = DestinationProject.CollectedData.GetDatabase();

            if (ResetProgress != null) { ResetProgress(); }
            double total = records.Count;

            Page previousPage = null;
            string lastGuid = string.Empty;
            List<string> fieldsInQuery = new List<string>();

            WordBuilder setFieldText = new WordBuilder(StringLiterals.COMMA);
            List<QueryParameter> fieldValueParams = new List<QueryParameter>();

            PackageFieldData lastRecord = new PackageFieldData();
            lastRecord.FieldName = "__--LastRecord--__";
            lastRecord.RecordGUID = String.Empty;
            records.Add(lastRecord);

            for (int i = 0; i < records.Count; i++)
            {
                PackageFieldData fieldData = records[i];

                if (i % 200 == 0)
                {
                    if (StatusChanged != null) { StatusChanged(string.Format(UnpackagerStrings.IMPORT_FIELD_PROGRESS, (i / total).ToString("P0"))); }
                    if (UpdateProgress != null) { UpdateProgress((i / total) * 100); }
                }

                string guid = fieldData.RecordGUID;
                bool isLast = fieldData.Equals(lastRecord);
                Page currentPage = fieldData.Page;

                if ((previousPage != currentPage && previousPage != null) || isLast || fieldsInQuery.Contains(fieldData.FieldName) || (!String.IsNullOrEmpty(lastGuid) && !guid.Equals(lastGuid, StringComparison.OrdinalIgnoreCase)))
                {
                    // run the update with the fields we currently have...

                    string updateHeader = String.Empty;
                    string whereClause = String.Empty;
                    StringBuilder sb = new StringBuilder();

                    // Build the Update statement which will be reused
                    sb.Append(SqlKeyWords.UPDATE);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(db.InsertInEscape(previousPage.TableName));
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(SqlKeyWords.SET);
                    sb.Append(StringLiterals.SPACE);

                    updateHeader = sb.ToString();

                    sb.Remove(0, sb.ToString().Length);

                    // Build the WHERE caluse which will be reused
                    sb.Append(SqlKeyWords.WHERE);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(db.InsertInEscape(ColumnNames.GLOBAL_RECORD_ID));
                    sb.Append(StringLiterals.EQUAL);
                    sb.Append("'");
                    sb.Append(lastGuid);
                    sb.Append("'");
                    whereClause = sb.ToString();

                    sb.Remove(0, sb.ToString().Length);

                    sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                    sb.Append(fieldData.FieldName);
                    sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                    sb.Append(StringLiterals.EQUAL);

                    sb.Append(StringLiterals.COMMERCIAL_AT);
                    sb.Append(fieldData.FieldName);

                    if (fieldsInQuery.Count > 0 && fieldValueParams.Count > 0)
                    {
                        Query updateQuery = db.CreateQuery(updateHeader + StringLiterals.SPACE + setFieldText.ToString() + StringLiterals.SPACE + whereClause);
                        updateQuery.Parameters = fieldValueParams;

                        if (DestinationProject.CollectedDataDriver.ToLower().Contains("epi.data.office"))
                        {
                            IDbCommand command = GetCommand(updateQuery.SqlStatement, Conn, updateQuery.Parameters);
                            object obj = command.ExecuteNonQuery();
                        }
                        else
                        {
                            db.ExecuteNonQuery(updateQuery);
                        }
                    }

                    setFieldText = new WordBuilder(StringLiterals.COMMA);
                    fieldValueParams = new List<QueryParameter>();
                    fieldsInQuery = new List<string>();

                    if (isLast) { break; }
                }

                if (destinationGuids.ContainsKey(guid) && destinationGuids[guid] == true)
                {
                    QueryParameter parameter = GetQueryParameterForField(fieldData, form, fieldData.Page);
                    fieldsInQuery.Add(fieldData.FieldName);
                    if (parameter != null)
                    {
                        setFieldText.Append(db.InsertInEscape(fieldData.FieldName) + " = " + "@" + fieldData.FieldName);
                        fieldValueParams.Add(parameter);
                    }
                    lastGuid = guid;
                    previousPage = currentPage;
                }
            }
        }

        /// <summary>
        /// Begins the process of importing records from the data package into the destination form
        /// </summary>
        /// <param name="form">The form that will receive the data</param>
        /// <param name="formNode">The XmlNode representing the form</param>
        /// <param name="records">The data to be imported</param>
        protected virtual void ImportRecords(View form, XmlNode formNode, List<PackageFieldData> records)
        {
            ImportInfo.RecordsAppended.Add(form, 0);
            ImportInfo.RecordsUpdated.Add(form, 0);

            IDbDriver destinationDb = DestinationProject.CollectedData.GetDatabase();
            Dictionary<string, bool> destinationGuids = new Dictionary<string, bool>();
            Dictionary<string, int> destinationGuidsAndRecStatus = new Dictionary<string, int>();
            Dictionary<Dictionary<Field, object>, bool> destinationKeyValues = new Dictionary<Dictionary<Field, object>, bool>();

            using (IDataReader baseTableReader = destinationDb.GetTableDataReader(form.TableName))
            {
                while (baseTableReader.Read())
                {
                    string readerGuid = baseTableReader["GlobalRecordId"].ToString();
                    int readerRecStatus = 0;

                    bool success = Int32.TryParse(baseTableReader["RecStatus"].ToString(), out readerRecStatus);

                    destinationGuids.Add(readerGuid, true);
                    destinationGuidsAndRecStatus.Add(readerGuid, readerRecStatus);
                }
            }

            #region Custom Keys
            if (this.IsUsingCustomMatchkeys)
            {
                WordBuilder wb = new WordBuilder(",");
                foreach (Field field in KeyFields)
                {
                    wb.Add(field.Name);
                }

                Query selectQuery = destinationDb.CreateQuery("SELECT " + wb.ToString() + " " + form.FromViewSQL);
                using (IDataReader keyTableReader = destinationDb.ExecuteReader(selectQuery))
                {
                    while (keyTableReader.Read())
                    {
                        Dictionary<Field, object> keys = new Dictionary<Field, object>();

                        foreach (Field field in KeyFields)
                        {
                            keys.Add(field, keyTableReader[field.Name]);
                            //destinationKeyValues.Add(keyTableReader["GlobalRecordId"].ToString(), true);
                        }

                        destinationKeyValues.Add(keys, true);
                    }
                }
            }
            #endregion // Custom Keys

            foreach (XmlNode recordNode in formNode.SelectSingleNode("Data").ChildNodes)
            {
                if (recordNode.Name.Equals("record", StringComparison.OrdinalIgnoreCase))
                {
                    string guid = String.Empty;
                    string recStatus = String.Empty;
                    string fkey = String.Empty;
                    string firstSaveId = String.Empty;
                    string lastSaveId = String.Empty;
                    DateTime? firstSaveTime = null;
                    DateTime? lastSaveTime = null;

                    foreach (XmlAttribute attrib in recordNode.Attributes)
                    {
                        if (attrib.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) guid = attrib.Value;
                        if (attrib.Name.Equals("recstatus", StringComparison.OrdinalIgnoreCase)) recStatus = attrib.Value;
                        if (attrib.Name.Equals("fkey", StringComparison.OrdinalIgnoreCase)) fkey = attrib.Value;
                        if (attrib.Name.Equals("firstsaveuserid", StringComparison.OrdinalIgnoreCase)) firstSaveId = attrib.Value;
                        if (attrib.Name.Equals("lastsaveuserid", StringComparison.OrdinalIgnoreCase)) lastSaveId = attrib.Value;
                        if (attrib.Name.Equals("firstsavetime", StringComparison.OrdinalIgnoreCase)) firstSaveTime = new DateTime(Convert.ToInt64(attrib.Value));
                        if (attrib.Name.Equals("lastsavetime", StringComparison.OrdinalIgnoreCase)) lastSaveTime = new DateTime(Convert.ToInt64(attrib.Value));
                    }

                    if (!destinationGuids.ContainsKey(guid))
                    {
                        if (Append)
                        {
                            destinationGuids.Add(guid, true);
                            CreateNewBlankRow(form, guid, fkey, recStatus, firstSaveId, lastSaveId, firstSaveTime, lastSaveTime);
                            ImportInfo.TotalRecordsAppended++;
                            ImportInfo.RecordsAppended[form]++;
                            ImportInfo.AddRecordIdAsAppended(form, guid);
                        }
                    }
                    else
                    {
                        if (!Update)
                        {
                            destinationGuids[guid] = false;
                        }
                        else
                        {
                            #region Record status update for deletions
                            // the destination recStatus doesn't match the one in the sync file. Now we need to do an update...
                            if (!String.IsNullOrEmpty(recStatus) && destinationGuidsAndRecStatus[guid].ToString() != recStatus)
                            {
                                // if the desired rec status is 0 (deleted) then try to change the target recstatus to this value
                                if (recStatus == "0")
                                {
                                    // but we only update rec status if the caller allowed it via the Delete property
                                    if (Delete)
                                    {
                                        // update RecStatus with DB query
                                        IDbDriver db = DestinationProject.CollectedData.GetDatabase();
                                        Query updateQuery = db.CreateQuery("UPDATE [" + form.TableName + "] SET [RecStatus] = @RecStatus WHERE [GlobalRecordId] = @GlobalRecordId");
                                        updateQuery.Parameters.Add(new QueryParameter("@RecStatus", DbType.Int32, Double.Parse(recStatus)));
                                        updateQuery.Parameters.Add(new QueryParameter("@GlobalRecordId", DbType.String, guid));
                                        db.ExecuteNonQuery(updateQuery);
                                    }

                                    // and regardless of the delete property's setting, we still add the record ID to the list of deleted records
                                    ImportInfo.AddRecordIdAsDeleted(form, guid);
                                }
                                else if (recStatus == "1")
                                {
                                    // now, the destination has a value of 0 and the sender has a value of 1, meaning the sender
                                    // is trying to UNDELETE the record for the receiver.

                                    // but only undelete if the caller said so via API
                                    if (Undelete)
                                    {
                                        IDbDriver db = DestinationProject.CollectedData.GetDatabase();
                                        Query updateQuery = db.CreateQuery("UPDATE [" + form.TableName + "] SET [RecStatus] = @RecStatus WHERE [GlobalRecordId] = @GlobalRecordId");
                                        updateQuery.Parameters.Add(new QueryParameter("@RecStatus", DbType.Int32, Double.Parse(recStatus)));
                                        updateQuery.Parameters.Add(new QueryParameter("@GlobalRecordId", DbType.String, guid));
                                        db.ExecuteNonQuery(updateQuery);
                                    }

                                    // regardless of whether we hit the DB, add the ID to the undelete list
                                    ImportInfo.AddRecordIdAsUndeleted(form, guid);
                                }
                            }
                            #endregion

                            ImportInfo.TotalRecordsUpdated++;
                            ImportInfo.RecordsUpdated[form]++;
                            ImportInfo.AddRecordIdAsUpdated(form, guid);
                        }
                    }
                }
            }

            ImportRecordsToPageTables(form, records, destinationGuids);
        }

        /// <summary>
        /// Begins the process of importing grid data
        /// </summary>
        /// <param name="gridField">The grid that will receive the data</param>
        /// <param name="gridRecords">The data to be imported</param>
        /// <param name="destinationGuids">A dictionary of GUIDs in the destination project; the key represents the GUID itself and the value (either true or false) represents whether or not to process that record</param>
        protected virtual void ImportGridRecordsToGridTable(GridField gridField, List<PackageFieldData> gridRecords, Dictionary<string, bool> destinationGuids)
        {
            if (Conn.State != ConnectionState.Open) { Conn.Open(); }

            IDbDriver db = DestinationProject.CollectedData.GetDatabase();

            if (ResetProgress != null) { ResetProgress(); }
            double total = gridRecords.Count;

            string lastGuid = string.Empty;
            List<string> fieldsInQuery = new List<string>();

            WordBuilder setFieldText = new WordBuilder(StringLiterals.COMMA);
            List<QueryParameter> fieldValueParams = new List<QueryParameter>();

            PackageFieldData lastRecord = new PackageFieldData();
            lastRecord.FieldName = "__--LastRecord--__";
            lastRecord.RecordGUID = string.Empty;
            gridRecords.Add(lastRecord);

            for (int i = 0; i < gridRecords.Count; i++)
            {
                PackageFieldData fieldData = gridRecords[i];
                if (i % 200 == 0)
                {
                    if (StatusChanged != null) { StatusChanged(string.Format(UnpackagerStrings.IMPORT_GRID_PROGRESS, (i / total).ToString("P0"))); }
                    if (UpdateProgress != null) { UpdateProgress((i / total) * 100); }
                }

                string guid = fieldData.RecordGUID;
                bool isLast = fieldData.Equals(lastRecord);

                if (isLast || fieldsInQuery.Contains(fieldData.FieldName) || (!String.IsNullOrEmpty(lastGuid) && !guid.Equals(lastGuid, StringComparison.OrdinalIgnoreCase)))
                {
                    // run the update with the fields we currently have...

                    string updateHeader = string.Empty;
                    string whereClause = string.Empty;
                    StringBuilder sb = new StringBuilder();

                    // Build the Update statement which will be reused
                    sb.Append(SqlKeyWords.UPDATE);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(db.InsertInEscape(gridField.TableName));
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(SqlKeyWords.SET);
                    sb.Append(StringLiterals.SPACE);

                    updateHeader = sb.ToString();

                    sb.Remove(0, sb.ToString().Length);

                    // Build the WHERE caluse which will be reused
                    sb.Append(SqlKeyWords.WHERE);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(db.InsertInEscape(ColumnNames.UNIQUE_ROW_ID));
                    sb.Append(StringLiterals.EQUAL);
                    sb.Append("'");
                    sb.Append(lastGuid);
                    sb.Append("'");
                    whereClause = sb.ToString();

                    sb.Remove(0, sb.ToString().Length);

                    sb.Append(StringLiterals.LEFT_SQUARE_BRACKET);
                    sb.Append(fieldData.FieldName);
                    sb.Append(StringLiterals.RIGHT_SQUARE_BRACKET);
                    sb.Append(StringLiterals.EQUAL);

                    sb.Append(StringLiterals.COMMERCIAL_AT);
                    sb.Append(fieldData.FieldName);

                    if (fieldsInQuery.Count > 0 && fieldValueParams.Count > 0)
                    {
                        Query updateQuery = db.CreateQuery(updateHeader
                            + StringLiterals.SPACE
                            + StringLiterals.LEFT_SQUARE_BRACKET
                            + setFieldText.ToString()
                            + StringLiterals.RIGHT_SQUARE_BRACKET
                            + StringLiterals.SPACE + whereClause);
                        
                        updateQuery.Parameters = fieldValueParams;

                        if (DestinationProject.CollectedDataDriver.ToLower().Contains("epi.data.office"))
                        {
                            IDbCommand command = GetCommand(updateQuery.SqlStatement, Conn, updateQuery.Parameters);
                            object obj = command.ExecuteNonQuery();
                        }
                        else
                        {
                            db.ExecuteNonQuery(updateQuery);
                        }
                    }

                    setFieldText = new WordBuilder(StringLiterals.COMMA);
                    fieldValueParams = new List<QueryParameter>();
                    fieldsInQuery = new List<string>();

                    if (isLast) { break; }
                }

                if (destinationGuids.ContainsKey(guid) && destinationGuids[guid] == true)
                {
                    fieldsInQuery.Add(fieldData.FieldName);
                    //setFieldText.Append(fieldData.FieldName + " = " + "@" + fieldData.FieldName);

                    QueryParameter parameter = GetQueryParameterForField(fieldData, gridField);
                    if (parameter != null)
                    {
                        setFieldText.Append(fieldData.FieldName + " = " + "@" + fieldData.FieldName);
                        fieldValueParams.Add(parameter);
                    }
                    lastGuid = guid;
                }
            }
        }

        /// <summary>
        /// Begins the process of importing grid records from the data package into the specified grid field
        /// </summary>
        /// <param name="gridField">The grid that will receive the data</param>
        /// <param name="gridNode">The XmlNode representing the grid</param>        
        /// <param name="gridRecords">The data to be imported</param>
        protected virtual void ImportGridRecords(GridField gridField, XmlNode gridNode, List<PackageFieldData> gridRecords)
        {
            IDbDriver destinationDb = DestinationProject.CollectedData.GetDatabase();
            Dictionary<string, bool> destinationGuids = new Dictionary<string, bool>();
            using (IDataReader baseTableReader = destinationDb.GetTableDataReader(gridField.TableName))
            {
                while (baseTableReader.Read())
                {
                    //destinationGuids.Add(baseTableReader["GlobalRecordId"].ToString(), true);
                    destinationGuids.Add(baseTableReader["UniqueRowId"].ToString(), true);
                }
            }

            foreach (XmlNode recordNode in gridNode.ChildNodes[1].ChildNodes)
            {
                if (recordNode.Name.Equals("record", StringComparison.OrdinalIgnoreCase))
                {
                    string guid = string.Empty;
                    string urid = string.Empty;
                    string fkey = string.Empty;

                    foreach (XmlAttribute attrib in recordNode.Attributes)
                    {
                        if (attrib.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) guid = attrib.Value;
                        else if (attrib.Name.Equals("uniquerowid", StringComparison.OrdinalIgnoreCase)) urid = attrib.Value;
                        else if (attrib.Name.Equals("fkey", StringComparison.OrdinalIgnoreCase)) fkey = attrib.Value;
                    }

                    //if (!destinationGuids.ContainsKey(guid))
                    if (!destinationGuids.ContainsKey(urid))
                    {
                        if (Append)
                        {
                            //destinationGuids.Add(guid, true);                                                      
                            destinationGuids.Add(urid, true);
                            CreateNewBlankGridRow(gridField, guid, urid, fkey);
                        }
                    }
                    else
                    {
                        if (!Update)
                        {
                            //destinationGuids[guid] = false;
                            destinationGuids[urid] = false;
                        }
                    }
                }
            }

            ImportGridRecordsToGridTable(gridField, gridRecords, destinationGuids);
        }

        /// <summary>
        /// Returns a native equivalent of a DbParameter
        /// </summary>
        /// <returns>Native equivalent of a DbParameter</returns>
        protected OleDbParameter ConvertToNativeParameter(QueryParameter parameter)
        {
            if (parameter.DbType.Equals(DbType.Guid))
            {
                parameter.Value = new Guid(parameter.Value.ToString());
            }

            OleDbParameter param = new OleDbParameter(parameter.ParameterName, CovertToNativeDbType(parameter.DbType), parameter.Size, parameter.Direction, parameter.IsNullable, parameter.Precision, parameter.Scale, parameter.SourceColumn, parameter.SourceVersion, parameter.Value);
            return param;
        }

        /// <summary>
        /// Gets the Access version of a generic DbType
        /// </summary>
        /// <returns>Access version of the generic DbType</returns>
        protected OleDbType CovertToNativeDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    return OleDbType.VarChar;
                case DbType.AnsiStringFixedLength:
                    return OleDbType.Char;
                case DbType.Binary:
                    return OleDbType.Binary;
                case DbType.Boolean:
                    return OleDbType.Boolean;
                case DbType.Byte:
                    return OleDbType.UnsignedTinyInt;
                case DbType.Currency:
                    return OleDbType.Currency;
                case DbType.Date:
                    return OleDbType.DBDate;
                case DbType.DateTime:
                case DbType.DateTime2:
                    return OleDbType.DBTimeStamp;
                case DbType.Decimal:
                    return OleDbType.Decimal;
                case DbType.Double:
                    return OleDbType.Double;
                case DbType.Guid:
                    return OleDbType.Guid;
                case DbType.Int16:
                    return OleDbType.SmallInt;
                case DbType.Int32:
                    return OleDbType.Integer;
                case DbType.Int64:
                    return OleDbType.BigInt;
                case DbType.Object:
                    //  return OleDbType.VarChar;
                    return OleDbType.Binary;
                case DbType.SByte:
                    return OleDbType.TinyInt;
                case DbType.Single:
                    return OleDbType.Single;
                case DbType.String:
                    return OleDbType.VarWChar;
                case DbType.StringFixedLength:
                    return OleDbType.WChar;
                case DbType.Time:
                    return OleDbType.DBTimeStamp;
                case DbType.UInt16:
                    return OleDbType.UnsignedSmallInt;
                case DbType.UInt32:
                    return OleDbType.UnsignedInt;
                case DbType.UInt64:
                    return OleDbType.UnsignedBigInt;
                case DbType.VarNumeric:
                    return OleDbType.VarNumeric;
                default:
                    return OleDbType.VarChar;
            }
        }

        /// <summary>
        /// Gets a new command using an existing connection
        /// </summary>
        /// <param name="sqlStatement">The query to be executed against the database</param>
        /// <param name="connection">Parameters for the query to be executed</param>
        /// <param name="parameters">An OleDb command object</param>
        /// <returns></returns>
        protected IDbCommand GetCommand(string sqlStatement, IDbConnection connection, List<QueryParameter> parameters)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(sqlStatement))
            {
                throw new ArgumentNullException("sqlStatement");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            #endregion

            IDbCommand command = connection.CreateCommand();
            command.CommandText = sqlStatement;

            foreach (QueryParameter parameter in parameters)
            {
                command.Parameters.Add(this.ConvertToNativeParameter(parameter));
            }

            return command;
        }

        /// <summary>
        /// Creates a new blank row for a given form's base table and all of its page tables.
        /// </summary>
        /// <param name="gridField">The grid field where the row should be added.</param>
        /// <param name="guid">The Guid value to use for the row.</param>
        /// <param name="urid">The unique row id.</param>
        /// <param name="fkey">The foreign key for the row.</param>
        protected virtual void CreateNewBlankGridRow(GridField gridField, string guid, string urid, string fkey)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(guid)) { throw new ArgumentNullException("guid"); }
            if (string.IsNullOrEmpty(urid)) { throw new ArgumentNullException("urid"); }
            if (string.IsNullOrEmpty(fkey)) { throw new ArgumentNullException("fkey"); }
            if (gridField == null) { throw new ArgumentNullException("gridField"); }
            #endregion // Input Validation

            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            IDbDriver db = DestinationProject.CollectedData.GetDatabase();
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into " + db.InsertInEscape(gridField.TableName) + " ");

            WordBuilder fields = new WordBuilder(",");
            fields.Append("[GlobalRecordId], [UniqueRowId], [FKEY]");

            sb.Append("(" + fields.ToString() + ") values (");

            WordBuilder values = new WordBuilder(",");
            values.Append("'" + guid + "', '" + urid + "', '" + fkey + "'");

            sb.Append(values.ToString());
            sb.Append(") ");
            Epi.Data.Query insertQuery = db.CreateQuery(sb.ToString());

            if (DestinationProject.CollectedDataDriver.ToLower().Contains("epi.data.office"))
            {
                IDbCommand command = GetCommand(insertQuery.SqlStatement, Conn, insertQuery.Parameters);
                object obj = command.ExecuteNonQuery();
            }
            else
            {
                db.ExecuteNonQuery(insertQuery);
            }
        }

        /// <summary>
        /// Creates a new blank row for a given form's base table and all of its page tables.
        /// </summary>
        /// <param name="form">The form where the row should be added.</param>
        /// <param name="guid">The Guid value to use for the row.</param>
        /// <param name="fkey">The foreign key for the row.</param>
        /// <param name="recStatus">The record status for this record (0 = deleted, 1 = active)</param>
        /// <param name="firstSaveId">The user ID of the first person that saved this record.</param>
        /// <param name="firstSaveTime">The time when the record was first saved.</param>
        /// <param name="lastSaveId">The user ID of the last person that saved this record.</param>
        /// <param name="lastSaveTime">The time when the record was last saved.</param>
        protected virtual void CreateNewBlankRow(View form, string guid, string fkey = "", string recStatus = "1", string firstSaveId = "", string lastSaveId = "", DateTime? firstSaveTime = null, DateTime? lastSaveTime = null)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(guid)) { throw new ArgumentNullException("guid"); }
            if (form == null) { throw new ArgumentNullException("form"); }
            #endregion // Input Validation

            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            IDbDriver db = DestinationProject.CollectedData.GetDatabase();
            StringBuilder sb = new StringBuilder();
            sb.Append(" insert into ");
            sb.Append(db.InsertInEscape(form.TableName));
            sb.Append(StringLiterals.SPACE);
            sb.Append(StringLiterals.SPACE);

            WordBuilder fields = new WordBuilder(",");
            fields.Append("[GlobalRecordId]");

            if (!String.IsNullOrEmpty(fkey)) { fields.Append("[FKEY]"); }
            if (!String.IsNullOrEmpty(recStatus)) { fields.Append("[RecStatus]"); }
            if (!String.IsNullOrEmpty(firstSaveId)) { fields.Append("[FirstSaveLogonName]"); }
            if (!String.IsNullOrEmpty(lastSaveId)) { fields.Append("[LastSaveLogonName]"); }
            if (firstSaveTime.HasValue)
            {
                firstSaveTime = new DateTime(firstSaveTime.Value.Year,
                firstSaveTime.Value.Month,
                firstSaveTime.Value.Day,
                firstSaveTime.Value.Hour,
                firstSaveTime.Value.Minute,
                firstSaveTime.Value.Second);
                fields.Append("[FirstSaveTime]");
            }
            if (lastSaveTime.HasValue)
            {
                lastSaveTime = new DateTime(lastSaveTime.Value.Year,
                lastSaveTime.Value.Month,
                lastSaveTime.Value.Day,
                lastSaveTime.Value.Hour,
                lastSaveTime.Value.Minute,
                lastSaveTime.Value.Second);
                fields.Append("[LastSaveTime]");
            }

            sb.Append("(" + fields.ToString() + ")");
            sb.Append(" values (");

            List<QueryParameter> parameters = new List<QueryParameter>();
            WordBuilder values = new WordBuilder(",");
            values.Append("'" + guid + "'");

            if (!String.IsNullOrEmpty(fkey))
            {
                values.Append("@FKEY");
                parameters.Add(new QueryParameter("@FKEY", DbType.String, fkey));
            }
            if (!String.IsNullOrEmpty(recStatus))
            {
                values.Append("@RecStatus");
                parameters.Add(new QueryParameter("@RecStatus", DbType.Int32, Convert.ToInt32(recStatus)));
            }
            if (!String.IsNullOrEmpty(firstSaveId))
            {
                values.Append("@FirstSaveLogonName");
                parameters.Add(new QueryParameter("@FirstSaveLogonName", DbType.String, firstSaveId));
            }
            if (!String.IsNullOrEmpty(lastSaveId))
            {
                values.Append("@LastSaveLogonName");
                parameters.Add(new QueryParameter("@LastSaveLogonName", DbType.String, lastSaveId));
            }
            if (firstSaveTime.HasValue)
            {
                values.Append("@FirstSaveTime");
                parameters.Add(new QueryParameter("@FirstSaveTime", DbType.DateTime, firstSaveTime));
            }
            if (lastSaveTime.HasValue)
            {
                values.Append("@LastSaveTime");
                parameters.Add(new QueryParameter("@LastSaveTime", DbType.DateTime, lastSaveTime));
            }

            sb.Append(values.ToString());
            sb.Append(") ");
            Epi.Data.Query insertQuery = db.CreateQuery(sb.ToString());
            insertQuery.Parameters = parameters;

            if (DestinationProject.CollectedDataDriver.ToLower().Contains("epi.data.office"))
            {
                IDbCommand command = GetCommand(insertQuery.SqlStatement, Conn, insertQuery.Parameters);
                object obj = command.ExecuteNonQuery();
            }
            else
            {
                db.ExecuteNonQuery(insertQuery);
            }

            foreach (Page page in form.Pages)
            {
                sb = new StringBuilder();
                sb.Append(" insert into ");
                sb.Append(db.InsertInEscape(page.TableName));
                sb.Append(StringLiterals.SPACE);
                sb.Append(StringLiterals.SPACE);
                sb.Append("([GlobalRecordId])");
                sb.Append(" values (");
                sb.Append("'" + guid + "'");
                sb.Append(") ");
                insertQuery = db.CreateQuery(sb.ToString());
                if (DestinationProject.CollectedDataDriver.ToLower().Contains("epi.data.office"))
                {
                    IDbCommand command = GetCommand(insertQuery.SqlStatement, Conn, insertQuery.Parameters);
                    object obj = command.ExecuteNonQuery();
                }
                else
                {
                    db.ExecuteNonQuery(insertQuery);
                }
            }
        }

        /// <summary>
        /// Gets the appropriate query parameter for a given field.
        /// </summary>
        /// <param name="fieldData">The field data to use to generate the parameter.</param>
        /// <param name="destinationForm">The form on which the field resides.</param>
        /// <param name="sourcePage">The page on the form in which the field resides.</param>
        /// <returns>QueryParameter</returns>
        protected QueryParameter GetQueryParameterForField(PackageFieldData fieldData, View destinationForm, Page sourcePage)
        {
            Field dataField = destinationForm.Fields[fieldData.FieldName];
            if (!(
                dataField is GroupField ||
                dataField is RelatedViewField ||
                dataField is UniqueKeyField ||
                dataField is RecStatusField ||
                dataField is GlobalRecordIdField ||
                fieldData.FieldValue == null ||
                string.IsNullOrEmpty(fieldData.FieldValue.ToString()
                )))
            {
                String fieldName = ((Epi.INamedObject)dataField).Name;
                switch (dataField.FieldType)
                {
                    case MetaFieldType.Date:
                    case MetaFieldType.DateTime:
                    case MetaFieldType.Time:
                        DateTime dt = new DateTime(Convert.ToInt64(fieldData.FieldValue));
                        return new QueryParameter("@" + fieldName, DbType.DateTime, dt);
                    case MetaFieldType.Checkbox:
                        return new QueryParameter("@" + fieldName, DbType.Boolean, Convert.ToBoolean(fieldData.FieldValue));
                    case MetaFieldType.CommentLegal:
                    case MetaFieldType.LegalValues:
                    case MetaFieldType.Codes:
                    case MetaFieldType.Text:
                    case MetaFieldType.TextUppercase:
                    case MetaFieldType.PhoneNumber:
                    case MetaFieldType.UniqueRowId:
                    case MetaFieldType.ForeignKey:
                    case MetaFieldType.GlobalRecordId:
                    case MetaFieldType.Multiline:
                    case MetaFieldType.GUID:
                        return new QueryParameter("@" + fieldName, DbType.String, fieldData.FieldValue);
                    case MetaFieldType.Number:
                    case MetaFieldType.YesNo:
                    case MetaFieldType.RecStatus:
                        return new QueryParameter("@" + fieldName, DbType.Single, Convert.ToDouble(fieldData.FieldValue, System.Globalization.CultureInfo.InvariantCulture));
                    case MetaFieldType.Image:
                        //throw new ApplicationException("Not a supported field type");
                        return new QueryParameter("@" + fieldName, DbType.Binary, Convert.FromBase64String(fieldData.FieldValue.ToString()));
                    case MetaFieldType.Option:
                        return new QueryParameter("@" + fieldName, DbType.Single, fieldData.FieldValue);
                    //this.BeginInvoke(new SetStatusDelegate(AddWarningMessage), "The data for " + fieldName + " was not imported. This field type is not supported.");
                    default:
                        throw new ApplicationException("Not a supported field type");
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the appropriate query parameter for a given grid column.
        /// </summary>
        /// <param name="fieldData">The field data to use to generate the parameter.</param>
        /// <param name="gridField">The grid field that contains this column</param>        
        /// <returns>QueryParameter</returns>
        protected QueryParameter GetQueryParameterForField(PackageFieldData fieldData, GridField gridField)
        {
            if (!string.IsNullOrEmpty(fieldData.FieldValue.ToString()))
            {
                GridColumnBase gridColumnBase = null;
                string fieldName = fieldData.FieldName;
                foreach (GridColumnBase gc in gridField.Columns)
                {
                    if (gc.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        gridColumnBase = gc;
                    }
                }

                if (gridColumnBase == null) { return null; } // the column exists in the source but not the destination, so skip it

                switch (gridColumnBase.GridColumnType)
                {
                    case MetaFieldType.Date:
                    case MetaFieldType.DateTime:
                    case MetaFieldType.Time:
                        DateTime dt = new DateTime(Convert.ToInt64(fieldData.FieldValue));
                        return new QueryParameter("@" + fieldName, DbType.DateTime, dt);
                    case MetaFieldType.Checkbox:
                        return new QueryParameter("@" + fieldName, DbType.Boolean, Convert.ToBoolean(fieldData.FieldValue));
                    case MetaFieldType.CommentLegal:
                    case MetaFieldType.LegalValues:
                    case MetaFieldType.Codes:
                    case MetaFieldType.Text:
                    case MetaFieldType.TextUppercase:
                    case MetaFieldType.PhoneNumber:
                    case MetaFieldType.UniqueRowId:
                    case MetaFieldType.ForeignKey:
                    case MetaFieldType.GlobalRecordId:
                    case MetaFieldType.Multiline:
                        return new QueryParameter("@" + fieldName, DbType.String, fieldData.FieldValue);
                    case MetaFieldType.Number:
                    case MetaFieldType.YesNo:
                        return new QueryParameter("@" + fieldName, DbType.Single, fieldData.FieldValue);
                    default:
                        throw new ApplicationException("Not a supported field type");
                }
            }
            return null;
        }

        /// <summary>
        /// Formats field data
        /// </summary>
        /// <param name="fieldName">The name of the field whose data should be formatted</param>
        /// <param name="value">The value that needs to be formatted</param>
        /// <returns>The formatted value</returns>
        protected object FormatFieldData(string fieldName, object value)
        {
            if (DestinationForm.Fields.Contains(fieldName))
            {
                Field field = DestinationForm.Fields[fieldName];

                if (field is CheckBoxField)
                {
                    if (value.ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        value = true;
                    }
                    else if (value.ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        value = false;
                    }
                }

                if (field is YesNoField)
                {
                    if (value.ToString().Equals("1", StringComparison.OrdinalIgnoreCase))
                    {
                        value = 1;
                    }
                    else if (value.ToString().Equals("0", StringComparison.OrdinalIgnoreCase))
                    {
                        value = 0;
                    }
                }

                if (field is NumberField && !string.IsNullOrEmpty(value.ToString()))
                {
                    double result = -1;
                    if (double.TryParse(value.ToString(), out result))
                    {
                        value = result;
                    }
                }
            }

            return value;
        }
        #endregion // Private Methods
    }
}
