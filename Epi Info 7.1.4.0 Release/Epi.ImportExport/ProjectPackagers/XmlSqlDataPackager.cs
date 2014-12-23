using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.ImportExport.Filters;

namespace Epi.ImportExport.ProjectPackagers
{
    /// <summary>
    /// A class used to turn the data for an Epi Info 7 SQL-basedform (and the data for any forms related to it) into Xml format. 
    /// </summary>
    public class XmlSqlDataPackager : XmlDataPackager
    {
        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceForm">The form within the project whose data will be packaged.</param>
        /// <param name="packageName">The name of the data package.</param>
        public XmlSqlDataPackager(View sourceForm, string packageName)
            : base(sourceForm, packageName)
        {
            #region Input Validation
            if (sourceForm == null) { throw new ArgumentNullException("sourceForm"); }
            if (String.IsNullOrEmpty(packageName)) { throw new ArgumentNullException("packageName"); }
            #endregion // Input Validation
        }
        #endregion // Constructors

        #region Private Methods

        /// <summary>
        /// Checks for problems in the source project
        /// </summary>
        protected override void CheckForProblems()
        {
            IDbDriver driver = SourceProject.CollectedData.GetDatabase();

            if (driver == null)
            {
                ExportInfo.Succeeded = false;
                ExportInfo.AddError("Data driver is null.", "999999");
                throw new InvalidOperationException("Data driver cannot be null");
            }

            int rows = 0;
            foreach (View form in SourceForm.Project.Views)
            {
                Query selectDistinctQuery = driver.CreateQuery("SELECT GlobalRecordId, COUNT(GlobalRecordId) AS n FROM " + form.TableName + " GROUP BY GlobalRecordId HAVING COUNT(GlobalRecordId) > 1");
                rows = driver.Select(selectDistinctQuery).Rows.Count;

                if (rows > 0)
                {
                    ExportInfo.Succeeded = false;
                    ExportInfo.AddError(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_GUID_NOT_UNIQUE, "101002");
                    throw new InvalidOperationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_GUID_NOT_UNIQUE);
                }

                foreach (Page page in form.Pages)
                {
                    Query deleteQuery = driver.CreateQuery("DELETE FROM " + form.Name + " WHERE GlobalRecordId NOT IN (SELECT GlobalRecordId from " + page.TableName + ")");
                    rows = driver.ExecuteNonQuery(deleteQuery);
                    if (rows > 0)
                    {
                        // report ??
                    }

                    Query pageDeleteQuery = driver.CreateQuery("DELETE FROM " + page.TableName + " WHERE GlobalRecordId NOT IN (SELECT GlobalRecordId from " + form.Name + ")");
                    rows = driver.ExecuteNonQuery(deleteQuery);
                    if (rows > 0)
                    {
                        // report ??
                    }

                    Query selectDistinctPageQuery = driver.CreateQuery("SELECT GlobalRecordId, COUNT(GlobalRecordId) AS n FROM " + page.TableName + " GROUP BY GlobalRecordId HAVING COUNT(GlobalRecordId) > 1");
                    rows = driver.Select(selectDistinctPageQuery).Rows.Count;

                    if (rows > 0)
                    {
                        ExportInfo.Succeeded = false;
                        ExportInfo.AddError(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_GUID_NOT_UNIQUE, "101002");
                        throw new InvalidOperationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_GUID_NOT_UNIQUE);
                    }
                }
            }

            // Check #7 - Should never get here because the UI should prevent it, but do a check just in case
            if (SourceForm.IsRelatedView == true)
            {
                ExportInfo.Succeeded = false;
                ExportInfo.AddError(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_RELATED_FORM, "101005");
                throw new InvalidOperationException(ImportExportSharedStrings.ERROR_PACKAGER_CHECK_RELATED_FORM);
            }

            driver.Dispose();
            driver = null;
        }

        private bool HasColumn(IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Creates an XmlElement representing an Epi Info 7 view's data.
        /// </summary>
        /// <param name="xmlDataPackage">The data package xml document that the XmlElement should be added to</param>
        /// <param name="form">The form whose data will be serialized</param>
        /// <returns>XmlElement; represents the form's data in Xml format, suitable for use in data packaging</returns>
        protected override XmlElement CreateXmlFormDataElement(XmlDocument xmlDataPackage, View form)
        {
            #region Input Validation
            if (xmlDataPackage == null) { throw new ArgumentNullException("xmlDataPackage"); }
            if (form == null) { throw new ArgumentNullException("form"); }
            #endregion // Input Validation

            XmlElement data = xmlDataPackage.CreateElement("Data");

            OnStatusChanged(String.Format("Packaging data for form {0}...", form.Name));
            OnResetProgress();

            /* This seems like an usual set of steps to just iterate over the data. The problem is that we can't "just
             * iterate over the data" - the data is split up page tables, with one table representing one page on the
             * form. While a JOIN might be able to bring everything together into one table, it might not - for example,
             * if there are >255 fields after the JOIN, an OleDb exception will be thrown.
             * 
             * To get around this issue: The code first iterates over the rows in the BASE TABLE, obtaining the GUID 
             * values for each. The GUIDs and their corresponding XmlElement go into a dictionary.
             * 
             * Later, each row in each page is iterated over; as the GUIDs for each page table are accessed, the corresponding 
             * XmlElement is pulled from the dictionary. Field data is added to it for each field that has data. In this
             * manner, it doesn't matter that each row is technically accessed out-of-order because they'll still show up
             * in-order in the resulting Xml.
             * 
             * Filtering adds another layer of complexity. To filter, a JOIN operation is needed so that the filters can
             * be applied across all those tables, since the fields in the filter may be across different tables. The
             * RowFilter class provides a way to handle this; we simply get the query from that object and apply it to the
             * reader. Only GUIDs that match the filter are added to the dictionary of guids.
             */

            // We need to exclude records from child forms that may now be orphaned as a result of a filter applied to the parent            
            if (form.IsRelatedView && PreviousDistanceFromRoot < CurrentDistanceFromRoot)
            {
                ParentIdList.Clear();
                foreach (KeyValuePair<string, XmlElement> kvp in IdList) { ParentIdList.Add(kvp.Key); }
            }

            IdList.Clear(); // Very important, this needs to be re-set in case we've already processed a form (this is a class level variable)

            if (!ExportInfo.RecordsPackaged.ContainsKey(form))
            {
                ExportInfo.RecordsPackaged.Add(form, 0);
            }

            //bool filterThisForm = false;
            RowFilters filters = null;
            Query selectQuery = null;

            IDbDriver db = SourceProject.CollectedData.GetDatabase();

            if (Filters != null && Filters.ContainsKey(form.Name) && Filters[form.Name].Count() > 0)
            {
                //filterThisForm = true;
                filters = Filters[form.Name];
                filters.RecordProcessingScope = RecordProcessingScope;
                selectQuery = filters.GetGuidSelectQuery(form);
            }

            double totalRecords = Convert.ToDouble(db.ExecuteScalar(db.CreateQuery("SELECT COUNT(*) FROM " + form.TableName)));

            string selectQueryText = "SELECT * " + form.FromViewSQL;

            if (selectQuery != null)
            {
                int whereClauseIndex = selectQuery.SqlStatement.LastIndexOf(" WHERE ");

                if (whereClauseIndex >= 0)
                {
                    selectQueryText = "SELECT * " + form.FromViewSQL + " " + selectQuery.SqlStatement.Substring(whereClauseIndex);
                }
            }

            selectQuery = db.CreateQuery(selectQueryText);

            int processedRecords = 0;

            using (IDataReader guidReader = db.ExecuteReader(selectQuery))
            //using (IDataReader guidReader = filterThisForm ? db.ExecuteReader(selectQuery) : db.GetTableDataReader(form.TableName))
            {
                while (guidReader.Read())
                {
                    string guid = guidReader["GlobalRecordId"].ToString();
                    string fkey = guidReader["FKEY"].ToString();
                    string recstatus = guidReader["RECSTATUS"].ToString();
                    string firstSaveUserId = string.Empty;
                    DateTime? firstSaveTime = null;
                    string lastSaveUserId = string.Empty;
                    DateTime? lastSaveTime = null;

                    firstSaveUserId = guidReader["FirstSaveLogonName"].ToString();
                    if (guidReader["FirstSaveTime"] != DBNull.Value)
                    {
                        firstSaveTime = (DateTime)guidReader["FirstSaveTime"];
                    }
                    lastSaveUserId = guidReader["LastSaveLogonName"].ToString();
                    if (guidReader["LastSaveTime"] != DBNull.Value)
                    {
                        lastSaveTime = (DateTime)guidReader["LastSaveTime"];
                    }

                    if (
                        (recstatus.Equals("1", StringComparison.OrdinalIgnoreCase) && RecordProcessingScope == Epi.RecordProcessingScope.Undeleted) ||
                        (recstatus.Equals("0", StringComparison.OrdinalIgnoreCase) && RecordProcessingScope == Epi.RecordProcessingScope.Deleted) ||
                        (RecordProcessingScope == Epi.RecordProcessingScope.Both))
                    {
                        if (!form.IsRelatedView || ParentIdList.Contains(fkey))
                        {
                            XmlElement record = xmlDataPackage.CreateElement("Record");
                            XmlAttribute id = xmlDataPackage.CreateAttribute("Id");
                            id.Value = guid;
                            record.Attributes.Append(id);

                            if (!string.IsNullOrEmpty(fkey))
                            {
                                XmlAttribute foreignKey = xmlDataPackage.CreateAttribute("Fkey");
                                foreignKey.Value = fkey;
                                record.Attributes.Append(foreignKey);
                            }
                            if (!string.IsNullOrEmpty(firstSaveUserId))
                            {
                                XmlAttribute firstSaveId = xmlDataPackage.CreateAttribute("FirstSaveUserId");
                                firstSaveId.Value = firstSaveUserId;
                                record.Attributes.Append(firstSaveId);
                            }
                            if (!string.IsNullOrEmpty(lastSaveUserId))
                            {
                                XmlAttribute lastSaveId = xmlDataPackage.CreateAttribute("LastSaveUserId");
                                lastSaveId.Value = lastSaveUserId;
                                record.Attributes.Append(lastSaveId);
                            }
                            if (firstSaveTime.HasValue)
                            {
                                XmlAttribute firstSaveDateTime = xmlDataPackage.CreateAttribute("FirstSaveTime");
                                firstSaveDateTime.Value = firstSaveTime.Value.Ticks.ToString();
                                record.Attributes.Append(firstSaveDateTime);
                            }
                            if (lastSaveTime.HasValue)
                            {
                                XmlAttribute lastSaveDateTime = xmlDataPackage.CreateAttribute("LastSaveTime");
                                lastSaveDateTime.Value = lastSaveTime.Value.Ticks.ToString();
                                record.Attributes.Append(lastSaveDateTime);
                            }
                            if (!String.IsNullOrEmpty(recstatus))
                            {
                                XmlAttribute recStatusAttribute = xmlDataPackage.CreateAttribute("RecStatus");
                                recStatusAttribute.Value = recstatus;
                                record.Attributes.Append(recStatusAttribute);
                            }
                            IdList.Add(guid, record);
                            //totalRecords++;

                            ExportInfo.TotalRecordsPackaged++;
                            ExportInfo.RecordsPackaged[form]++;

                            foreach (Field field in form.Fields)
                            {
                                if (field is IDataField && field is RenderableField && !(field is GridField) && !(FieldsToNull.ContainsKey(form.Name) && FieldsToNull[form.Name].Contains(field.Name)))
                                {
                                    RenderableField renderableField = field as RenderableField;
                                    if (renderableField != null)
                                    {
                                        XmlElement fieldData = xmlDataPackage.CreateElement("Field");

                                        XmlAttribute name = xmlDataPackage.CreateAttribute("Name");
                                        name.Value = renderableField.Name;
                                        fieldData.Attributes.Append(name);

                                        string value = guidReader[field.Name].ToString();

                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            if (field is DateTimeField)
                                            {
                                                DateTime dt = Convert.ToDateTime(value);
                                                fieldData.InnerText = dt.Ticks.ToString();
                                            }
                                            else if (field is ImageField)
                                            {
                                                value = Convert.ToBase64String((Byte[])guidReader[field.Name]);
                                                fieldData.InnerText = value;
                                            }
                                            else if (field is NumberField)
                                            {
                                                value = Convert.ToDouble(value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                fieldData.InnerText = value;
                                            }
                                            else
                                            {
                                                fieldData.InnerText = value;
                                            }
                                        }

                                        if (String.IsNullOrEmpty(fieldData.InnerText) && IncludeNullFieldData == false)
                                        {
                                            // do nothing, for now...
                                        }
                                        else
                                        {
                                            record.AppendChild(fieldData);
                                        }
                                        data.AppendChild(record);
                                    }
                                }
                            }
                        }
                    }

                    processedRecords++;
                    double progress = (((double)processedRecords) / ((double)totalRecords)) * 100;
                    OnProgressChanged(progress);
                }
            }

            foreach (GridField gridField in form.Fields.GridFields)
            {
                data.AppendChild(CreateXmlGridElement(xmlDataPackage, form, gridField));
                ExportInfo.GridsProcessed++;
            }

            return data;
        }
        #endregion // Private/Protected Methods
    }
}