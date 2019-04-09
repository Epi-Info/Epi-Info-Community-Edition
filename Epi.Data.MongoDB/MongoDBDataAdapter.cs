using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing.Design;
using System.Data;
using Epi.DataSets;

namespace Epi.Data.MongoDB
{
    public class MongoDBDataAdapter : IDbDataAdapter, IDataAdapter
    {
        public MongoDBDataAdapter() { }
        public MongoDBDataAdapter(MongoDBCommand command) { }
        public MongoDBDataAdapter(string commandText, MongoDBConnection connection) { }
        public MongoDBDataAdapter(string commandText, string connectionString) { }
        public MongoDBCommand DeleteCommand { get; set; }
        public MongoDBCommand UpdateCommand { get; set; }
        public MongoDBCommand InsertCommand { get; set; }
        public MongoDBCommand SelectCommand { get; set; }
        // public override int UpdateBatchSize { get; set; }
        public string About { get; set; }
        public MissingMappingAction MissingMappingAction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public MissingSchemaAction MissingSchemaAction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ITableMappingCollection TableMappings => throw new NotImplementedException();

        IDbCommand IDbDataAdapter.SelectCommand { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IDbCommand IDbDataAdapter.InsertCommand { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IDbCommand IDbDataAdapter.UpdateCommand { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IDbCommand IDbDataAdapter.DeleteCommand { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Fill(DataSet dataSet)
        {
            throw new NotImplementedException();
        }

        public DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
        {
            throw new NotImplementedException();
        }

        public IDataParameter[] GetFillParameters()
        {
            throw new NotImplementedException();
        }

        public int Update(DataSet dataSet)
        {
            throw new NotImplementedException();
        }

        internal void Fill(TableKeysSchema.Primary_KeysDataTable primary_Keys)
        {
            throw new NotImplementedException();
        }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}