using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;


namespace EpiDashboard
{
    public class WordCloudParameters : GadgetParametersBase
    {
        public string CommonWords { get; set; }

        public WordCloudParameters()
            : base()
        {
            GadgetTitle = DashboardSharedStrings.GADGET_CONFIG_TITLE_WORDCLOUD;
            CommonWords = ", . a am an and are as at but by for her his i if in is it me my not of on or our pm that the their this to us was we were will with would you your";
        }

        public List<string> ColorList { get; set; }
    }
}
