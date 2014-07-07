using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.WPF.Dashboard
{
    public struct StrataGridListRow
    {
        public string StrataLabel;
        public decimal? OutcomeRateExposure;
        public decimal? OutcomeRateNoExposure;
        public decimal? RiskRatio;
        public decimal? OddsRatio;

        public decimal? OddsLower;
        public decimal? OddsUpper;
    }
}
