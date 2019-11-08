using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Epi.Data.RimportSPSS
{
    public class RimportSPSSAsyncResult : System.IAsyncResult
    {
        public bool IsCompleted { get; }
        public WaitHandle AsyncWaitHandle { get; }
        public object AsyncState { get; }
        public bool CompletedSynchronously { get; }
    }
}
