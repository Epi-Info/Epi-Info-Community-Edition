using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing.Design;
using System.Data;

namespace Epi.Data.MongoDB
{
    public class MongoDBCommand : IDbCommand, IDisposable, ICloneable
    {
        public MongoDBCommand() { }
        public MongoDBCommand(string commandText) { }
        public MongoDBCommand(MongoDBCommand cmd) { }
        public MongoDBCommand(string commandText, MongoDBConnection connection) { }
        public MongoDBCommand(string commandText, MongoDBConnection connection, MongoDBTransaction transaction) { }

        public MongoDBParameter Parameters { get; }

        public MongoDBConnection Connection { get; set; }

        public string About { get; set; }
        public MongoDBTransaction Transaction { get; set; }
        public string CommandText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public CommandType CommandType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IDbConnection IDbCommand.Connection { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IDbTransaction IDbCommand.Transaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IDataParameterCollection IDbCommand.Parameters => throw new NotImplementedException();

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public void Prepare()
        {
            throw new NotImplementedException();
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        IDbDataParameter IDbCommand.CreateParameter()
        {
            throw new NotImplementedException();
        }

        IDataReader IDbCommand.ExecuteReader()
        {
            throw new NotImplementedException();
        }

        IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }
    }
}
