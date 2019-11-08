using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace Epi.Data.RimportSPSS
{
    public class RimportSPSSTransaction : DbTransaction
    {
        public RimportSPSSTransaction(RimportSPSSConnection connection) { }
        public RimportSPSSTransaction(RimportSPSSConnection connection, IsolationLevel isolationLevel) { }

        public override IsolationLevel IsolationLevel { get; }
        public RimportSPSSConnection Connection { get; }
        protected override DbConnection DbConnection { get; }

        public override void Commit() { }
        public override void Rollback() { }
        protected override void Dispose(bool disposing) { }
    }
}