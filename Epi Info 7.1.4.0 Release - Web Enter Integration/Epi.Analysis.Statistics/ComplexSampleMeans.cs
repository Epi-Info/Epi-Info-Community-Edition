using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpiInfo.Plugin;


namespace Epi.Analysis.Statistics
{
    public class ComplexSampleMeans : IAnalysisStatistic
    {
        private StatisticsRepository.ComplexSampleMeans CSMeans = new StatisticsRepository.ComplexSampleMeans();

        public string Name { get { return "Epi.Analysis.Statistics.ComplexSampleMeans"; } }

        public ComplexSampleMeans(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            CSMeans = new StatisticsRepository.ComplexSampleMeans();
            this.Construct(AnalysisStatisticContext);

        }

        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            CSMeans.Construct(AnalysisStatisticContext);
        }

        /// <summary>
        /// performs execution of the Complex Sample MEANS command
        /// </summary>
        /// <returns>object</returns>
        public void Execute()
        {
            CSMeans.Execute();
        }
    }
}

