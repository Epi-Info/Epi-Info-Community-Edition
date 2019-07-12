using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Epi.Data.REDCap.Models;

namespace Epi.Data.REDCap.Wrappers
{
    public class REDCapWrapper
    {
        private CertInfo certInfo;
		private string redcapuri;
		private string redcapapikey;
		private string redcaptablename;
        private bool expired;

        private REDCapWrapper()
        {

        }

        public REDCapWrapper(string config)
        {
            try
            {
                expired = false;
                string certFile = config.Split('@')[0];
                string key = config.Split('@')[1];
				redcaptablename = config.Split('@')[2];
				redcapuri = certFile;
				redcapapikey = key;
			}
			catch (CryptographicException ce)
            {
                Epi.Windows.MsgBox.ShowError("Invalid org key or certificate file");
            }
            catch (JsonSerializationException je)
            {
                Epi.Windows.MsgBox.ShowError("Certificate file is malformed. Please ask your Epi Info administrator for a new certificate file.");
            }
            catch (Exception ex)
            {
                Epi.Windows.MsgBox.ShowError(ex.ToString());
            }
        }


        public bool TestConnection()
        {
            if (expired)
                return false;

            SqlConnection conn = new SqlConnection(certInfo.ConnectionString);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }

            return true;
        }

		public List<string> GetTableNames()
        {
            List<string> names = new List<string>();

            if (expired)
            {
                Epi.Windows.MsgBox.ShowError("Your certificate file has expired. Please ask your Epi Info administrator for a new certificate file.");
                return names;
            }

			if (!String.IsNullOrEmpty(redcaptablename))
			{
				names.Add(redcaptablename);
			}

			if (certInfo != null)
            {
                using (SqlConnection connection = new SqlConnection(certInfo.ConnectionString))
                {
                    connection.Open();
                    string commandString = "select o.Organization, m.SurveyId, m.SurveyName, m.DateCreated from surveymetadata m inner join organization o on m.OrganizationId = o.OrganizationId where o.OrganizationId = '" + certInfo.OrganizationId + "'";
                    using (SqlCommand command = new SqlCommand(commandString, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                string name = reader.GetFieldValue<string>(2) + " (" + reader.GetFieldValue<DateTime>(3) + ")" + " {{" + reader.GetFieldValue<Guid>(1) + "}}";
                                names.Add(name);
                            }
                        }
                    }
                }
            }

            return names;
        }

        public async Task<DataTable> GetDataTableAsync(string collectionName)
        {
            DataTable dataTable = new DataTable("Table1");
			var red_cap_api = new RedcapApi(redcapuri);
			var ExportRecordsAsync = red_cap_api.ExportRecordsAsync(redcapapikey, Content.Record).Result;
			var ExportRecordsAsyncData = JsonConvert.DeserializeObject(ExportRecordsAsync);
			dataTable = JsonConvert.DeserializeObject<System.Data.DataTable>(ExportRecordsAsyncData.ToString());
			var ExportMetaDataAsync = red_cap_api.ExportMetaDataAsync(redcapapikey, Content.MetaData, ReturnFormat.json, null, null, OnErrorFormat.json).Result;
			var ExportMetaDataAsyncData = JsonConvert.DeserializeObject(ExportMetaDataAsync);
			DataTable metaDataTable =
				JsonConvert.DeserializeObject<System.Data.DataTable>(ExportMetaDataAsyncData.ToString());
			int typeInt = metaDataTable.Columns.IndexOf("field_type");
			int validationInt = metaDataTable.Columns.IndexOf("text_validation_type_or_show_slider_number");
			DataTable dataTableCloned = dataTable.Clone();
			bool cloneAgain = false;
			List<string> booleanDataColumns = new List<string>();
			foreach (DataColumn dc in dataTableCloned.Columns)
			{
				string columnName = dc.ColumnName;
				string select = "field_name = '" + dc.ColumnName + "'";
				DataRow[] selected = metaDataTable.Select(select);
				if (selected.Length > 0)
				{
					DataRow selected0 = selected[0];
					string type = (string)selected0[typeInt];
					string validation = (string)selected0[validationInt];
					if (type.Equals("calc") || validation.Equals("number"))
					{
						dc.DataType = typeof(double);
					}
					else if (validation.Equals("integer"))
					{
						dc.DataType = typeof(Int32);
					}
					else if (validation.Contains("date"))
					{
						dc.DataType = typeof(System.DateTime);
					}
				}
				else if (dc.ColumnName.IndexOf("__") >= 0)
				{
					select = "field_name = '" + dc.ColumnName.Substring(0, dc.ColumnName.IndexOf("__")) + "'";
					selected = metaDataTable.Select(select);
					if (selected.Length > 0)
					{
						dc.DataType = typeof(System.Byte);
						cloneAgain = true;
						booleanDataColumns.Add(dc.ColumnName);
					}
				}
			}
			foreach (DataRow dr in dataTable.Rows)
				dataTableCloned.ImportRow(dr);
			if (cloneAgain)
			{
				DataTable dataTableClonedCloned = dataTableCloned.Clone();
				foreach (DataColumn dc in dataTableClonedCloned.Columns)
				{
					if (booleanDataColumns.Contains(dc.ColumnName))
					{
						dc.DataType = typeof(System.Boolean);
					}
				}
				foreach (DataRow dr in dataTableCloned.Rows)
					dataTableClonedCloned.ImportRow(dr);
				return dataTableClonedCloned;
			}
			return dataTableCloned;

			if (expired)
                return dataTable;

            string surveyId = collectionName.Substring(collectionName.IndexOf("{{") + 2, 36);
            using (SqlConnection connection = new SqlConnection(certInfo.ConnectionString))
            {
                await connection.OpenAsync();
                string commandString = "select ResponseJson from SurveyResponse where ResponseJson is not null and surveyid = '" + surveyId + "'";
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {

                        while (await reader.ReadAsync())
                        {
                            string json = reader.GetFieldValue<string>(0);
                            dataTable = GetDataTableFromJson(dataTable, json);
                        }
                    }
                }
            }
            return dataTable;
        }

        public DataTable GetFirstDataRow(string collectionName)
        {
            DataTable dataTable = new DataTable("Table1");

            if (expired)
                return dataTable;

            string surveyId = collectionName.Substring(collectionName.IndexOf("{{") + 2, 36);
            using (SqlConnection connection = new SqlConnection(certInfo.ConnectionString))
            {
                connection.Open();
                string commandString = "select top 1 ResponseJson from SurveyResponse where ResponseJson is not null and surveyid = '" + surveyId + "'";
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            string json = reader.GetFieldValue<string>(0);
                            dataTable = GetDataTableFromJson(dataTable, json);
                        }
                    }
                }
            }
            return dataTable;
        }

        public async Task<long> GetCollectionSize(string collectionName)
        {

            if (expired)
                return 0;

            string surveyId = collectionName.Substring(collectionName.IndexOf("{{") + 2, 36);

            using (SqlConnection connection = new SqlConnection(certInfo.ConnectionString))
            {
                await connection.OpenAsync();
                string commandString = "select count(*) from SurveyResponse where ResponseJson is not null and surveyid = '" + surveyId + "'";
                using (SqlCommand command = new SqlCommand(commandString, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {

                        if (await reader.ReadAsync())
                        {
                            return reader.GetFieldValue<int>(0);
                        }
                    }
                }
            }
            return 0;
        }

        private class Message
        {
            public string ResponseId;
            public string FormId;
            public string FormName;
            public string ParentResponseId;
            public string ParentFormId;
            public string OKey;
            public object ResponseQA;
        }

        private class CertInfo
        {
            public string OrganizationId;
            public string OrganizationName;
            public DateTime ExpirationDate;
            public int DataSourceType;
            public string ConnectionString;
        }

        private DataTable GetDataTableFromJson(DataTable dt, string json)
        {
            try
            {
                Message msg = JsonConvert.DeserializeObject<Message>(json);

                if (msg.ResponseQA.ToString().Length < 5)
                    return dt;

                DataSet dataSet = JsonConvert.DeserializeObject<DataSet>("{'Table1': [" + msg.ResponseQA.ToString() + "]}");
                DataTable dataTable = dataSet.Tables["Table1"];
                dt.Merge(dataTable);

                return dt;
            }
            catch (Exception ex)
            {
                return dt;
            }
        }

        internal string GetOrgName()
        {
            if (!String.IsNullOrEmpty(redcapuri) && !String.IsNullOrEmpty(redcapapikey))
            {
                return redcapuri + "@" + redcapapikey;
            }
            else
            {
                return "";
            }
        }
    }
}
