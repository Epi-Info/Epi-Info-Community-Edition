using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpiInfo.Plugin;


namespace Epi.Analysis.Statistics
{
    public class LinearRegression : IAnalysisStatistic
    {

        private StatisticsRepository.LinearRegression LinearRegress = new StatisticsRepository.LinearRegression();

        public string Name { get { return "Epi.Analysis.Statistics.LinearRegression"; } }

        public LinearRegression(IAnalysisStatisticContext AnalysisStatisticContext) 
        {
            LinearRegress = new StatisticsRepository.LinearRegression();
            this.Construct(AnalysisStatisticContext);
            
        }

        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            LinearRegress.Construct(AnalysisStatisticContext);
        }

        /// <summary>
        /// performs execution of the REGRESS command
        /// </summary>
        /// <returns>object</returns>
        public void Execute()
        {
            LinearRegress.Execute(); 
        }
    }
}

