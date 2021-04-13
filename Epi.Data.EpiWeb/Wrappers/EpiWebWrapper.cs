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
using EpiDashboard.Dialogs;
using System.Windows.Forms;

namespace Epi.Data.EpiWeb.Wrappers
{
    public class EpiWebWrapper
    {
        private CertInfo certInfo;
        private bool expired;
        public string system;
        public Guid OrgId;
        private EpiWebWrapper()
        {

        }

        public EpiWebWrapper(string config)
        {
            try
            {
                //expired = false;
                //string certFile = config.Substring(9).Split('@')[0];
                //string key = config.Substring(9).Split('@')[1];
                //string contents = Epi.Configuration.DecryptFileToString(certFile, key);
                //certInfo = JsonConvert.DeserializeObject<CertInfo>(contents);
                //if (certInfo.ExpirationDate < DateTime.Now)
                //{
                //    expired = true;
                //}
                ////new code 
                var ConfigInfo = config.Split('@');
                system = ConfigInfo[0];
                OrgId = Guid.Parse(ConfigInfo[1].ToString());

            }
            catch (CryptographicException ce)
            {
                Epi.Windows.MsgBox.ShowError("Invalid org key or certificate file");
            }
            //catch (JsonSerializationException je)
            //{
            //    Epi.Windows.MsgBox.ShowError("Certificate file is malformed. Please ask your Epi Info administrator for a new certificate file.");
            //}
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
            if (this.system == "epiweb://Epi Info Web Survey") {
                Epi.SurveyManagerServiceV4.SurveyInfoResponse Response = new Epi.SurveyManagerServiceV4.SurveyInfoResponse();

                try
                {
                    var Client = Epi.Core.ServiceClient.ServiceClient.GetClientV4();

                    Response = Client.GetAllSurveysByOrgKey(OrgId.ToString());
                }
                catch (Exception ex)
                {

                    WebSurveyOptions dialog1 = new WebSurveyOptions();
                    DialogResult result1 = dialog1.ShowDialog();
                    if (result1 == System.Windows.Forms.DialogResult.OK)
                    {

                        var Client = Epi.Core.ServiceClient.ServiceClient.GetClientV4();

                        Response = Client.GetAllSurveysByOrgKey(OrgId.ToString());
                    }
                }
                foreach (var items in Response.SurveyInfoList)
                {
                    // names.Add(items.SurveyId + "_" + items.SurveyName);
                    names.Add(items.SurveyName + "_" + items.SurveyId);
                }
            }
          

            if (this.system == "epiweb://Epi Info Cloud Data Capture")
            {
                Epi.EWEManagerServiceV2.SurveyInfoResponse EWEResponse = new EWEManagerServiceV2.SurveyInfoResponse();
                
                try
                {
                    var EWEClient = Epi.Core.ServiceClient.EWEServiceClient.Get2Client();
                   
                    EWEResponse = EWEClient.GetAllSurveysByOrgKey(OrgId.ToString());
                }
                catch (Exception ex)
                {

                    WebSurveyOptions dialog1 = new WebSurveyOptions();
                    DialogResult result1 = dialog1.ShowDialog();
                    if (result1 == System.Windows.Forms.DialogResult.OK)
                    {

                        var EWEClient = Epi.Core.ServiceClient.EWEServiceClient.Get2Client();

                        EWEResponse = EWEClient.GetAllSurveysByOrgKey(OrgId.ToString());
                    }
                }
                foreach (var items in EWEResponse.SurveyInfoList)
                {
                    // names.Add(items.SurveyId + "_" + items.SurveyName);
                    names.Add(items.SurveyName + "_" + items.SurveyId);
                }
            }
                // List<SurveyManagerServiceV4.SurveyInfoDTO> DTOList = Response.SurveyInfoList.OrderBy(o => o.SurveyName).ToList();
           
            //if (expired)
            //{
            //    Epi.Windows.MsgBox.ShowError("Your certificate file has expired. Please ask your Epi Info administrator for a new certificate file.");
            //    return names;
            //}

            //if (certInfo != null)
            //{
            //    using (SqlConnection connection = new SqlConnection(certInfo.ConnectionString))
            //    {
            //        connection.Open();
            //        string commandString = "select o.Organization, m.SurveyId, m.SurveyName, m.DateCreated from surveymetadata m inner join organization o on m.OrganizationId = o.OrganizationId where o.OrganizationId = '" + certInfo.OrganizationId + "'";
            //        using (SqlCommand command = new SqlCommand(commandString, connection))
            //        {
            //            using (SqlDataReader reader = command.ExecuteReader())
            //            {

            //                while (reader.Read())
            //                {
            //                    string name = reader.GetFieldValue<string>(2) + " (" + reader.GetFieldValue<DateTime>(3) + ")" + " {{" + reader.GetFieldValue<Guid>(1) + "}}";
            //                    names.Add(name);
            //                }
            //            }
            //        }
            //    }
            //}

            return names;
        }

        public async Task<DataTable> GetDataTableAsync(string collectionName)
        {
            DataTable dataTable = new DataTable("Table1");

            if (expired)
                return dataTable;
            var SurveyId = collectionName.Split('_').Last();
            string json = "";
            if (this.system == "epiweb://Epi Info Web Survey")
            {
                try
                {
                    var Client = Epi.Core.ServiceClient.ServiceClient.GetClientV4();

                    json = Client.GetJsonResponseAll(SurveyId, "", "");
                }
                catch (Exception ex)
                {

                    WebSurveyOptions dialog1 = new WebSurveyOptions();
                    DialogResult result1 = dialog1.ShowDialog();
                    if (result1 == System.Windows.Forms.DialogResult.OK)
                    {
                        //OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, SharedStrings.WEBFORM_ORG_KEY_SUCCESSFUL, SharedStrings.WEBFORM_ORG_KEY_REPUBLISH);
                        //DialogResult result2 = OrgKeyDialog.ShowDialog();
                        //if (result2 == System.Windows.Forms.DialogResult.OK)
                        //{
                        //    this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                        //    if (!string.IsNullOrWhiteSpace(OrganizationKey))
                        //    {
                        //        SetSurveyInfo();
                        //        QuickSurveyInfoUpdate();
                        //    }
                        //}
                    }
                }
                var msg = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);

                foreach (var item in msg)
                {
                    dataTable = GetDataTableFromJson(dataTable, JsonConvert.SerializeObject(item)); ;

                }

            }
            if (this.system == "epiweb://Epi Info Cloud Data Capture")
            {
                try
                {
                    var EWEClient = Epi.Core.ServiceClient.EWEServiceClient.Get2Client();

                    json = EWEClient.GetJsonResponseAll(SurveyId, "", "");
                }
                catch (Exception ex)
                {

                    WebSurveyOptions dialog1 = new WebSurveyOptions();
                    DialogResult result1 = dialog1.ShowDialog();
                    if (result1 == System.Windows.Forms.DialogResult.OK)
                    {
                         
                    }
                }
                var msg = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);

                foreach (var item in msg)
                {
                    dataTable = GetDataTableFromJson(dataTable, JsonConvert.SerializeObject(item)); ;

                }

            }
            //string surveyId = collectionName.Substring(collectionName.IndexOf("{{") + 2, 36);
            //using (SqlConnection connection = new SqlConnection(certInfo.ConnectionString))
            //{
            //    await connection.OpenAsync();
            //    string commandString = "select ResponseJson from SurveyResponse where ResponseJson is not null and surveyid = '" + surveyId + "'";
            //    using (SqlCommand command = new SqlCommand(commandString, connection))
            //    {
            //        using (SqlDataReader reader = await command.ExecuteReaderAsync())
            //        {

            //            while (await reader.ReadAsync())
            //            {
            //                string json = reader.GetFieldValue<string>(0);
            //                dataTable = GetDataTableFromJson(dataTable, json);
            //            }
            //        }
            //    }
            //}
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
                //Message msg = JsonConvert.DeserializeObject<Message>(json);

                //if (msg.ResponseQA.ToString().Length < 5)
                //    return dt;

                //DataSet dataSet = JsonConvert.DeserializeObject<DataSet>("{'Table1': [" + msg.ResponseQA.ToString() + "]}");
                DataSet dataSet = JsonConvert.DeserializeObject<DataSet>("{'Table1': [" + json + "]}");
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
            if (certInfo != null)
            {
                return certInfo.OrganizationName;
            }
            else
            {
                return "";
            }
        }
    }
}
