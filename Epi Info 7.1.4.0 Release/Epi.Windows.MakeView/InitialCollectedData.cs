using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Epi.Data;
using Epi.Data.Services;

namespace Epi.Windows.MakeView
{
    public class InitialCollectedData
    {
        DataSet dataSet;
        CollectedDataProvider userData;

        public InitialCollectedData(CollectedDataProvider provider)
        {
            userData = provider;
        }
        
        public void Add(string viewName, string fieldName)
        {
            if (dataSet == null)
            {
                dataSet = new DataSet();
            }

            if (dataSet.Tables.Contains(viewName) == false)
            {
                dataSet.Tables.Add(viewName);
                dataSet.Tables[viewName].Columns.Add("GlobalRecordId", System.Type.GetType("System.String"));
            }

            if (dataSet.Tables[viewName].Columns.Contains(fieldName) == false)
            {
                dataSet.Tables[viewName].Columns.Add(fieldName, System.Type.GetType("System.String"));
                
                int pageIndex = 1;
                string sourcePageTableName = viewName + pageIndex.ToString();

                while (userData.TableExists(sourcePageTableName))
                {
                    if (userData.GetTableData(sourcePageTableName).Columns.Contains(fieldName))
                    {
                        string queryString = string.Format("select {0}, GlobalRecordId from {1}", fieldName, sourcePageTableName);
                        Query query = userData.GetDbDriver().CreateQuery(queryString);
                        DataTable tableOfCutData = userData.Select(query);

                        object dataType = userData.GetTableData(sourcePageTableName).Columns[fieldName].DataType;

                        foreach (DataRow row in tableOfCutData.Rows)
                        {
                            string id = row["GlobalRecordId"].ToString();
                            object data = row[fieldName];
                            DataRow[] rows = dataSet.Tables[viewName].Select(string.Format("[GlobalRecordId] = '{0}'", id));

                            if (rows.Length == 0)
                            {
                                DataRow newRow = dataSet.Tables[viewName].NewRow();
                                newRow["GlobalRecordId"] = row["GlobalRecordId"].ToString();
                                newRow[fieldName] = row[fieldName];
                                dataSet.Tables[viewName].Rows.Add(newRow);
                            }
                            else
                            {
                                rows[0][fieldName] = data;
                            }
                        }
                    }

                    pageIndex++;
                    sourcePageTableName = viewName + pageIndex.ToString();
                }
            }
        }

        public void MergeWithPageTable(string viewName, string fieldName, string pageTableName)
        {
            string queryString;

            bool collectedDataTableExists = userData.TableExists(pageTableName);
            bool clipboardTableExists = dataSet != null && dataSet.Tables.Contains(viewName);

            if (collectedDataTableExists && clipboardTableExists && dataSet.Tables[viewName].Columns.Contains(fieldName))
            {
                DataTable collectedDataTable = userData.GetTableData(pageTableName);

                foreach (DataRow initialRow in dataSet.Tables[viewName].Rows)
                {
                    string initialGlobalRecordId = initialRow["GlobalRecordId"].ToString();

                    Type dataType = collectedDataTable.Columns[fieldName].DataType;
                    
                    queryString = string.Format("GlobalRecordId = '{0}'", initialGlobalRecordId);
                    DataRow[] rows = collectedDataTable.Select(queryString);

                    string fieldData = initialRow[fieldName].ToString();

                    if (dataType.Name == "String" || dataType.Name == "DateTime")
                    {
                        fieldData = string.Format("'{0}'", fieldData);
                    }

                    queryString = string.Format("update [{0}] set [{1}] = {2} where GlobalRecordId = '{3}'",
                        pageTableName,
                        fieldName,
                        fieldData,
                        initialGlobalRecordId);

                    Query updateQuery = userData.CreateQuery(queryString);
                    userData.ExecuteNonQuery(updateQuery);
                }
            }
        }
    }
}
