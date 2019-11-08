using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Epi.Data.RimportSPSS
{
    public sealed class RimportSPSSParameter : DbParameter, ICloneable, IDbDataParameter, IDataParameter
    {
        public RimportSPSSParameter() { }
        public RimportSPSSParameter(string name, object value) { }
        public RimportSPSSParameter(string name, System.Data.DbType dataType) { }
        public RimportSPSSParameter(string name, System.Data.DbType dataType, int size) { }
        public RimportSPSSParameter(string name, System.Data.DbType dataType, int size, string srcColumn) { }
        public RimportSPSSParameter(string parameterName, System.Data.DbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value) { }
        public RimportSPSSParameter(string parameterName, System.Data.DbType dbType, int size, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value) { }
        public override string SourceColumn { get; set; }
        public override int Size { get; set; }
        public override bool IsNullable { get; set; }
        public override ParameterDirection Direction { get; set; }
        public override object Value { get; set; }
        public byte Scale { get; set; }
        public override DbType DbType { get; set; }
        public override string ParameterName { get; set; }
        public RimportSPSSType OleDbType { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public byte Precision { get; set; }
        public override DataRowVersion SourceVersion { get; set; }

        public override void ResetDbType()
        {
            throw new NotImplementedException();
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }
}