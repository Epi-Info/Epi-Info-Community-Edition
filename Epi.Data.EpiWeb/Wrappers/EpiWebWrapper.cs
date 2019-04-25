using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Epi.Data.EpiWeb.Wrappers
{
    public class EpiWebWrapper
    {
        private CertInfo certInfo;

        private EpiWebWrapper()
        {

        }

        public EpiWebWrapper(string config)
        {
            try
            {
                string certFile = config.Substring(9);
                string contents = System.IO.File.ReadAllText(certFile);
                certInfo = JsonConvert.DeserializeObject<CertInfo>(contents);
            }
            catch (Exception ex)
            {

            }
        }


        public bool TestConnection()
        {
            SqlConnection conn = new SqlConnection(certInfo.ConnectionString);
            try
            {
                conn.Open();
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

            return names;
        }

        public async Task<DataTable> GetDataTableAsync(string collectionName)
        {
            DataTable dataTable = new DataTable("Table1");
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
            string surveyId = collectionName.Substring(collectionName.IndexOf("{{") + 2, 36);
            using (SqlConnection connection = new SqlConnection(certInfo.ConnectionString))
            {
                connection.Open();
                string commandString = "select top ResponseJson from SurveyResponse where ResponseJson is not null and surveyid = '" + surveyId + "'";
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
            public int DataSourceType;
            public string ConnectionString;
        }

        private DataTable GetDataTableFromJson(DataTable dt, string json)
        {
            try
            {
                Message msg = JsonConvert.DeserializeObject<Message>(json);
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
            return certInfo.OrganizationName;
        }
    }
}
