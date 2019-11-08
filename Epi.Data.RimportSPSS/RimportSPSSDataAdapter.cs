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

namespace Epi.Data.RimportSPSS
{
    public class RimportSPSSDataAdapter : IDbDataAdapter, IDataAdapter
    {
        public RimportSPSSDataAdapter() { }
        public RimportSPSSDataAdapter(RimportSPSSCommand command) { }
        public RimportSPSSDataAdapter(string commandText, RimportSPSSConnection connection) { }
        public RimportSPSSDataAdapter(string commandText, string connectionString) { }
        public RimportSPSSCommand DeleteCommand { get; set; }
        public RimportSPSSCommand UpdateCommand { get; set; }
        public RimportSPSSCommand InsertCommand { get; set; }
        public RimportSPSSCommand SelectCommand { get; set; }
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