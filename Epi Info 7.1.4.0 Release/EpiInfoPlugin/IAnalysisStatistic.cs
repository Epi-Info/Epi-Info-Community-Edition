using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiInfo.Plugin
{
    public interface IAnalysisStatistic
    {
        string Name { get; }
        void Construct(IAnalysisStatisticContext AnalysisStatisticContext);
        void Execute();
    }
}
