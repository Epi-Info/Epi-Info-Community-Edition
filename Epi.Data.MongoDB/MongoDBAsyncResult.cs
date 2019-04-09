using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Epi.Data.MongoDB
{
    public class MongoDBAsyncResult : System.IAsyncResult
    {
        public bool IsCompleted { get; }
        public WaitHandle AsyncWaitHandle { get; }
        public object AsyncState { get; }
        public bool CompletedSynchronously { get; }
    }
}
