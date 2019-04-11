using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing.Design;
using System.Data;

namespace Epi.Data.MongoDB
{
    public class MongoDBConnection : IDbConnection, IDisposable, ICloneable
    {
        private string connectionString;
        public MongoDBConnection() { }
        public MongoDBConnection(MongoDBConnectionStringBuilder settings) { }
        public MongoDBConnection(string connectionString) { this.connectionString = connectionString; }
        public MongoDBConnection(MongoDBConnectionStringBuilder settings, string rtk) { }
        public MongoDBConnection(string connectionString, string rtk) { }

        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ConnectionTimeout => throw new NotImplementedException();

        public string Database => throw new NotImplementedException();

        public ConnectionState State => throw new NotImplementedException();

        public string DataSource
        {
            get
            {
                if (connectionString.Contains("~"))
                    return connectionString.Split('~')[1];
                else
                    return "";
            }
            internal set
            {
            }
        }

        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public IDbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        internal DataTable GetSchema(string v)
        {
            throw new NotImplementedException();
        }

        internal DataTable GetSchema(string collectionName, string[] v)
        {
            throw new NotImplementedException();
        }
    }
}