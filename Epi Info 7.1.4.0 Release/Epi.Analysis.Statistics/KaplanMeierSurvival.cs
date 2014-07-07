using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpiInfo.Plugin;

namespace Epi.Analysis.Statistics
{    
    public class KaplanMeierSurvival: IAnalysisStatistic
    {
        private StatisticsRepository.EIKaplanMeierSurvival KMSurvival = new StatisticsRepository.EIKaplanMeierSurvival();
        
        IAnalysisStatisticContext Context;

        public KaplanMeierSurvival(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            this.Construct(AnalysisStatisticContext);

        }

        public string Name { get { return "Epi.Analysis.Statistics.KaplanMeierSurvival"; } }
        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            this.Context = AnalysisStatisticContext;
            KMSurvival = new StatisticsRepository.EIKaplanMeierSurvival();
            KMSurvival.Construct(this.Context);  
            
        }

        /// <summary>
        /// performs execution of the KMSurvival command
        /// </summary>
        /// <returns>object</returns>
        public void Execute()
        {
            KMSurvival.Execute();
        }

    }
}