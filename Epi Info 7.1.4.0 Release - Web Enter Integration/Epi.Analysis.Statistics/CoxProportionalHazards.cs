using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EpiInfo.Plugin;

namespace Epi.Analysis.Statistics
{
    public class CoxProportionalHazards : IAnalysisStatistic
    {

        private StatisticsRepository.EICoxProportionalHazards mCoxPH = new StatisticsRepository.EICoxProportionalHazards();


        IAnalysisStatisticContext Context;

        public CoxProportionalHazards(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            
            this.Construct(AnalysisStatisticContext);

        }

        public string Name { get { return "Epi.Analysis.Statistics.CoxProportionalHazards"; } }
        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            this.Context = AnalysisStatisticContext;
            mCoxPH = new StatisticsRepository.EICoxProportionalHazards();
            mCoxPH.Construct(this.Context);  
            
        }
        
        /// <summary>
        /// performs execution of the CoxPH command
        /// </summary>
        /// <returns>object</returns>
        public void Execute()
        {
            mCoxPH.Execute();
        }

    }
}
