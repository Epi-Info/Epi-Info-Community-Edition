using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data;
using System.Threading.Tasks;

namespace Epi.Data.MongoDB.Wrappers
{
    public class MongoDBWrapper
    {
        private string host;
        private int port;
        private string databaseName;
        private string userName;
        private string password;
        private string connectionString;

        private MongoDBWrapper()
        {

        }

        public MongoDBWrapper(string connectionString)
        {
            if (connectionString.Contains("~"))
            {
                this.connectionString = connectionString.Split('~')[0];
                this.databaseName = connectionString.Split('~')[1];
            }
            else
            {
                this.connectionString = connectionString;
            }
        }

        public MongoDBWrapper(string host, string database) : this(host, 27017, database)
        {

        }

        public MongoDBWrapper(string host, int port, string database)
        {
            this.host = host;
            this.databaseName = database;
            this.port = port;

            this.connectionString = "mongodb://" + userName + password + host + ":" + port;
        }

        public MongoDBWrapper(string host, string database, string userName, string password) : this(host, 27017, database, userName, password)
        {

        }

        public MongoDBWrapper(string host, int port, string database, string userName, string password) : this(host, port, database)
        {
            this.userName = userName;
            this.password = ":" + password + "@";

            this.connectionString = "mongodb://" + userName + password + host + ":" + port;
        }

        public bool TestConnection()
        {
            try
            {
                var client = new MongoClient(connectionString);
                var names = client.ListDatabaseNames();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<string> GetTableNames()
        {
            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                return database.ListCollectionNames().ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<string> GetDatabaseNames()
        {
            try
            {
                var client = new MongoClient(connectionString);
                return client.ListDatabaseNames().ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async void CreateCollection(string collectionName)
        {
            try
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);
                await database.CreateCollectionAsync(collectionName);
            }
            catch (Exception ex)
            {

            }
        }

        public async void Insert(BsonDocument document, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertOneAsync(document);
        }

        public async Task<DataTable> GetDataTableAsync(string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            var collection = database.GetCollection<BsonDocument>(collectionName);
            DataTable dt = new DataTable(collectionName);

            await collection.Find(new BsonDocument())
                .ForEachAsync(async (document) =>
                {
                    dt = GetDataTableFromBson(dt, document);
                });

            return dt;
        }

        private DataTable GetDataTableFromBson(DataTable dt, BsonDocument doc)
        {
            AddColumns(dt, doc, "");

            DataRow dr = dt.NewRow();
            SetValue(dr, doc, "");
            dt.Rows.Add(dr);

            return dt;
        }

        private void SetValue(DataRow dr, BsonDocument doc, string prefix)
        {
            foreach (BsonElement elm in doc.Elements)
            {
                if (elm.Value.IsBsonDocument)
                {
                    SetValue(dr, elm.Value.AsBsonDocument, prefix);
                }
                else if (elm.Value.IsBsonArray)
                {
                    BsonArray array = elm.Value.AsBsonArray;

                    for (int x = 0; x < array.Count; x++)
                    {
                        if (array[x].IsBsonDocument)
                        {
                            SetValue(dr, array[x].AsBsonDocument, prefix + elm.Name + "_" + x + "_");
                        }
                        else
                        {
                            if (!array[x].IsBsonNull)
                                dr[prefix + elm.Name + "_" + x] = array[x];
                        }
                    }
                }
                else
                {
                    if (!elm.Value.IsBsonNull)
                        dr[prefix + elm.Name] = elm.Value;
                }
            }
        }

        private void AddColumns(DataTable dt, BsonDocument doc, string prefix)
        {
            foreach (BsonElement elm in doc.Elements)
            {
                if (elm.Value.IsBsonDocument)
                {
                    AddColumns(dt, elm.Value.AsBsonDocument, prefix);
                }
                else if (elm.Value.IsBsonArray)
                {
                    BsonArray array = elm.Value.AsBsonArray;

                    for (int x = 0; x < array.Count; x++)
                    {
                        if (array[x].IsBsonDocument)
                        {
                            AddColumns(dt, array[x].AsBsonDocument, prefix + elm.Name + "_" + x + "_");
                        }
                        else
                        {
                            if (!dt.Columns.Contains(prefix + elm.Name + "_" + x))
                            {
                                dt.Columns.Add(new DataColumn(prefix + elm.Name + "_" + x, GetDataType(array[x])));
                            }
                        }
                    }
                }
                else
                {
                    if (!dt.Columns.Contains(prefix + elm.Name))
                    {
                        dt.Columns.Add(new DataColumn(prefix + elm.Name, GetDataType(elm.Value)));
                    }
                }
            }
        }

        private Type GetDataType(BsonValue val)
        {
            if (val.IsBsonDateTime)
                return typeof(System.DateTime);
            else if (val.IsValidDateTime)
                return typeof(System.DateTime);
            else if (val.IsDouble)
                return typeof(double);
            else if (val.IsInt32)
                return typeof(int);
            else if (val.IsInt64)
                return typeof(long);
            else
                return typeof(string);
        }
    }
}
