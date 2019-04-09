using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace Epi.Data.MongoDB
{
    public class MongoDBTransaction : DbTransaction
    {
        public MongoDBTransaction(MongoDBConnection connection) { }
        public MongoDBTransaction(MongoDBConnection connection, IsolationLevel isolationLevel) { }

        public override IsolationLevel IsolationLevel { get; }
        public MongoDBConnection Connection { get; }
        protected override DbConnection DbConnection { get; }

        public override void Commit() { }
        public override void Rollback() { }
        protected override void Dispose(bool disposing) { }
    }
}