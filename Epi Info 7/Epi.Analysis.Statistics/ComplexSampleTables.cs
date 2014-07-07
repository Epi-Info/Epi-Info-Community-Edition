using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpiInfo.Plugin;


namespace Epi.Analysis.Statistics
{
    public class ComplexSampleTables : IAnalysisStatistic
    {
        private StatisticsRepository.ComplexSampleTables CSTables = new StatisticsRepository.ComplexSampleTables();

        public string Name { get { return "Epi.Analysis.Statistics.ComplexSampleTables"; } }

        public ComplexSampleTables(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            CSTables = new StatisticsRepository.ComplexSampleTables();
            this.Construct(AnalysisStatisticContext);

        }

        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            CSTables.Construct(AnalysisStatisticContext);
        }

        /// <summary>
        /// performs execution of the Complex Sample MEANS command
        /// </summary>
        /// <returns>object</returns>
        public void Execute()
        {
            CSTables.Execute();
        }
    }
}

