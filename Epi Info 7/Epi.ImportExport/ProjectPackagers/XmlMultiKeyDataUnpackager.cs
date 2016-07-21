using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.ImportExport.ProjectPackagers;

namespace Epi.ImportExport.ProjectPackagers
{
    public class XmlMultiKeyDataUnpackager : XmlDataUnpackager
    {
        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="destinationForm">The form that will receive the incoming data</param>
        /// <param name="xmlDataPackage">The data package in Xml format</param>
        public XmlMultiKeyDataUnpackager(View destinationForm, XmlDocument xmlDataPackage) : base (destinationForm, xmlDataPackage)
        {
            #region Input Validation
            if (destinationForm == null) { throw new ArgumentNullException("sourceForm"); }
            if (xmlDataPackage == null) { throw new ArgumentNullException("xmlDataPackage"); }
            #endregion // Input Validation
        }
        #endregion // Constructors

        #region Private Methods
        /// <summary>
        /// Checks to see if two match key dictionaries are equal
        /// </summary>
        /// <param name="dictionary1">The first dictionary to check</param>
        /// <param name="dictionary2">The second dictionary to check</param>
        /// <returns>bool; represents whether the dictionaries are the same</returns>
        private bool AreKeyFieldDictionariesEqual(Dictionary<Field, object> dictionary1, Dictionary<Field, object> dictionary2)
        {
            if (dictionary1.Count != dictionary2.Count)
            {
                return false;
            }

            foreach (KeyValuePair<Field, object> kvp in dictionary1)
            {
                if (!dictionary2.ContainsKey(kvp.Key))
                {
                    return false;
                }
                else if (!dictionary2[kvp.Key].ToString().Equals(kvp.Value.ToString()))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion // Private Methods

        /// <summary>
        /// Creates a new blank row for a given form's base table and all of its page tables.
        /// </summary>
        /// <param name="form">The form where the row should be added.</param>
        /// <param name="guid">The Guid value to use for the row.</param>
        /// <param name="keyValues">The key values to use for custom matching</param>
        /// <param name="fkey">The foreign key for the row.</param>
        /// <param name="firstSaveId">The user ID of the first person that saved this record.</param>
        /// <param name="firstSaveTime">The time when the record was first saved.</param>
        /// <param name="lastSaveId">The user ID of the last person that saved this record.</param>
        /// <param name="lastSaveTime">The time when the record was last saved.</param>
        protected virtual void CreateNewBlankRow(View form, string guid, Dictionary<Field, object> keyValues = null, string fkey = "", string firstSaveId = "", string lastSaveId = "", DateTime? firstSaveTime = null, DateTime? lastSaveTime = null)
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

            if (!string.IsNullOrEmpty(fkey)) { fields.Append("[FKEY]"); }
            if (!string.IsNullOrEmpty(firstSaveId)) { fields.Append("[FirstSaveLogonName]"); }
            if (!string.IsNullOrEmpty(lastSaveId)) { fields.Append("[LastSaveLogonName]"); }
            if (firstSaveTime.HasValue) { fields.Append("[FirstSaveTime]"); }
            if (lastSaveTime.HasValue) { fields.Append("[LastSaveTime]"); }

            sb.Append("(" + fields.ToString() + ")");
            sb.Append(" values (");

            List<QueryParameter> parameters = new List<QueryParameter>();
            WordBuilder values = new WordBuilder(",");
            values.Append("'" + guid + "'");

            if (!string.IsNullOrEmpty(fkey))
            {
                values.Append("@FKEY");
                parameters.Add(new QueryParameter("@FKEY", DbType.String, fkey));
            }
            if (!string.IsNullOrEmpty(firstSaveId))
            {
                values.Append("@FirstSaveLogonName");
                parameters.Add(new QueryParameter("@FirstSaveLogonName", DbType.String, firstSaveId));
            }
            if (!string.IsNullOrEmpty(lastSaveId))
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

            if (DestinationProject.CollectedDataDriver.ToLowerInvariant().Contains("epi.data.office"))
            {
                IDbCommand command = GetCommand(insertQuery.SqlStatement, Conn, insertQuery.Parameters);
                object obj = command.ExecuteNonQuery();
            }
            else
            {
                db.ExecuteNonQuery(insertQuery);
            }

            //Parallel.ForEach(form.Pages, page =>
            foreach(Page page in form.Pages)
                {
                    WordBuilder wbFields = new WordBuilder(",");
                    WordBuilder wbParams = new WordBuilder(",");
                    List<QueryParameter> queryParams = new List<QueryParameter>();

                    wbFields.Add("[GlobalRecordId]");
                    wbParams.Add("@GlobalRecordId");
                    queryParams.Add(new QueryParameter("@GlobalRecordId", DbType.String, guid));

                    foreach (KeyValuePair<Field, object> kvp in keyValues)
                    {
                        RenderableField field = kvp.Key as RenderableField;

                        PackageFieldData fieldData = new PackageFieldData();
                        fieldData.FieldValue = kvp.Value;
                        fieldData.FieldName = field.Name;
                        fieldData.Page = field.Page;

                        if (field.Page.TableName.Equals(page.TableName))
                        {

                            wbFields.Add(db.InsertInEscape(fieldData.FieldName));
                            wbParams.Add("@" + fieldData.FieldName);

                            QueryParameter parameter = GetQueryParameterForField(fieldData, form, field.Page);
                            queryParams.Add(parameter);
                        }
                    }
                    sb = new StringBuilder();
                    sb.Append(" insert into ");
                    sb.Append(db.InsertInEscape(page.TableName));
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append("(");
                    sb.Append(wbFields.ToString());
                    sb.Append(")");
                    sb.Append(" values (");
                    sb.Append(wbParams.ToString());
                    sb.Append(") ");
                    insertQuery = db.CreateQuery(sb.ToString());

                    foreach (QueryParameter queryParam in queryParams)
                    {
                        insertQuery.Parameters.Add(queryParam);
                    }

                    if (DestinationProject.CollectedDataDriver.ToLowerInvariant().Contains("epi.data.office"))
                    {
                        IDbCommand command = GetCommand(insertQuery.SqlStatement, Conn, insertQuery.Parameters);
                        object obj = command.ExecuteNonQuery();
                    }
                    else
                    {
                        db.ExecuteNonQuery(insertQuery);
                    }
                }
            //);
        }

        /// <summary>
        /// Begins the process of importing records from the data package into the destination form
        /// </summary>
        /// <param name="form">The form that will receive the data</param>
        /// <param name="formNode">The XmlNode representing the form</param>
        /// <param name="records">The data to be imported</param>
        protected override void ImportRecords(View form, XmlNode formNode, List<PackageFieldData> records)
        {
            if (!IsUsingCustomMatchkeys) // Calling class should instantiate normal data packager if custom keys aren't used
            {
                throw new ApplicationException("This class should not be used without custom match keys.");
            }

            ImportInfo.RecordsAppended.Add(form, 0);
            ImportInfo.RecordsUpdated.Add(form, 0);

            IDbDriver destinationDb = DestinationProject.CollectedData.GetDatabase();
            
            DataTable destinationKeyTable = new DataTable();
            destinationKeyTable.Columns.Add(new DataColumn("GlobalRecordId", typeof(string)));
            destinationKeyTable.Columns.Add(new DataColumn("Update", typeof(bool)));
            destinationKeyTable.Columns.Add(new DataColumn("KeyDictionary", typeof(Dictionary<Field, object>)));
            
            DataColumn [] primaryKey = new DataColumn[1];
            primaryKey[0] = destinationKeyTable.Columns["GlobalRecordId"];
            destinationKeyTable.PrimaryKey = primaryKey;

            WordBuilder wb = new WordBuilder(",");
            wb.Add("t.GlobalRecordId");
            foreach (Field field in KeyFields)
            {
                wb.Add(field.Name);
            }

            Query selectQuery = destinationDb.CreateQuery("SELECT " + wb.ToString() + " " + form.FromViewSQL);
            using (IDataReader keyTableReader = destinationDb.ExecuteReader(selectQuery))
            {
                while (keyTableReader.Read())
                {
                    string guid = keyTableReader["GlobalRecordId"].ToString();
                    Dictionary<Field, object> keys = new Dictionary<Field, object>();

                    foreach (Field field in KeyFields)
                    {
                        keys.Add(field, keyTableReader[field.Name]);
                    }

                    destinationKeyTable.Rows.Add(guid, true, keys);
                }
            }

            var query = from record in records
                        group record by record.RecordGUID;

            IEnumerable<IEnumerable<PackageFieldData>> fieldDataLists = query as IEnumerable<IEnumerable<PackageFieldData>>;

            foreach (IEnumerable<PackageFieldData> fieldDataList in fieldDataLists)
            {
                PackageFieldData fieldData = fieldDataList.First();
                bool found = false;

                foreach (DataRow row in destinationKeyTable.Rows)
                {
                    Dictionary<Field, object> keyDictionary = row["KeyDictionary"] as Dictionary<Field, object>;

                    if (AreKeyFieldDictionariesEqual(keyDictionary, fieldData.KeyValues))
                    {
                        found = true;
                        if (!Update)
                        {
                            row["Update"] = false;
                        }
                        else
                        {
                            ImportInfo.TotalRecordsUpdated++;
                            ImportInfo.RecordsUpdated[form]++;
                        }
                    }
                }

                if (!found && Append) // no match, this is a new record that must be inserted
                {
                    CreateNewBlankRow(form, fieldData.RecordGUID, fieldData.KeyValues);
                    ImportInfo.TotalRecordsAppended++;
                    ImportInfo.RecordsAppended[form]++;
                }
            }

            System.Threading.Thread.Sleep(2000); // give time for DB to update

            destinationKeyTable.Clear();
            selectQuery = destinationDb.CreateQuery("SELECT " + wb.ToString() + " " + form.FromViewSQL);
            using (IDataReader keyTableReader = destinationDb.ExecuteReader(selectQuery))
            {
                while (keyTableReader.Read())
                {
                    string guid = keyTableReader["GlobalRecordId"].ToString();
                    Dictionary<Field, object> keys = new Dictionary<Field, object>();

                    foreach (Field field in KeyFields)
                    {
                        keys.Add(field, keyTableReader[field.Name]);
                    }

                    destinationKeyTable.Rows.Add(guid, true, keys);
                }
            }

            //Parallel.ForEach(records, rec =>

            // TODO: Make this faster (note that Parallel foreach seems to make it worse)
            foreach(PackageFieldData rec in records)
                {
                    bool found = false;
                    string targetGuid = String.Empty;
                    bool shouldUpdate = true;

                    foreach (DataRow row in destinationKeyTable.Rows)
                    {
                        Dictionary<Field, object> keyDictionary = row["KeyDictionary"] as Dictionary<Field, object>;

                        if (AreKeyFieldDictionariesEqual(keyDictionary, rec.KeyValues))
                        {
                            found = true;
                            targetGuid = row["GlobalRecordId"].ToString();
                            shouldUpdate = (bool)row["Update"];
                            break;
                        }
                    }

                    if (shouldUpdate && found && !String.IsNullOrEmpty(targetGuid) && rec.FieldValue != null && !String.IsNullOrEmpty(rec.FieldValue.ToString()))
                    {
                        Query updateQuery = destinationDb.CreateQuery("UPDATE " + rec.Page.TableName + " SET " +
                        "[" + rec.FieldName + "] = @" + rec.FieldName + " WHERE [GlobalRecordId] = @GlobalRecordId");

                        QueryParameter fieldParam = GetQueryParameterForField(rec, form, rec.Page);

                        if (fieldParam != null)
                        {
                            updateQuery.Parameters.Add(fieldParam);
                            updateQuery.Parameters.Add(new QueryParameter("@GlobalRecordId", DbType.String, targetGuid));
                            int rowsAffected = destinationDb.ExecuteNonQuery(updateQuery);

                            if (rowsAffected == 0)
                            {
                                throw new ApplicationException("No records affected.");
                            }
                            else if (rowsAffected > 1)
                            {
                                throw new ApplicationException("Too many records affected.");
                            }
                        }
                    }
                }
            //);
        }
    }
}
