using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpiInfo.Plugin;


namespace Epi.Analysis.Statistics
{
    public class LogisticRegression : IAnalysisStatistic
    {
        private StatisticsRepository.LogisticRegression LogisticRegress = new StatisticsRepository.LogisticRegression();

        public string Name { get { return "Epi.Analysis.Statistics.LogisticRegression"; } }

        public LogisticRegression(IAnalysisStatisticContext AnalysisStatisticContext) 
        {
            LogisticRegress = new StatisticsRepository.LogisticRegression();
            this.Construct(AnalysisStatisticContext);
            
        }

        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            LogisticRegress.Construct(AnalysisStatisticContext);
        }

        /// <summary>
        /// performs execution of the LOGISTIC command
        /// </summary>
        /// <returns>object</returns>
        public void Execute()
        {
            LogisticRegress.Execute(); 
        }
    }
}

