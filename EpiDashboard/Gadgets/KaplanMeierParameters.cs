using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class KaplanMeierParameters : FrequencyParametersBase
    {
        public string OutcomeVariableName { get; set; }
        public bool ShowANOVA { get; set; }
        public bool ShowMaxCol { get; set; }
        public bool ShowModeCol { get; set; }
        public bool ShowMeanCol { get; set; }
        public bool ShowObservationsCol { get; set; }
        public bool ShowVarianceCol { get; set; }
        public bool ShowStdDevCol { get; set; }
        public bool ShowMinCol { get; set; }
        public bool Show25PercentCol { get; set; }
        public bool ShowMedianCol { get; set; }
        public bool Show75PercentCol { get; set; }
        public bool ShowTotalCol { get; set; }
        public List<int> columnsToHide;

        public KaplanMeierParameters()
            : base()
        {
            ShowANOVA = true;
            ShowMaxCol = true;
            ShowModeCol = true;
            ShowMeanCol = true;
            ShowObservationsCol = true;
            ShowVarianceCol = true;
            ShowStdDevCol = true;
            ShowMinCol = true;
            Show25PercentCol = true;
            ShowMedianCol = true;
            Show75PercentCol = true;
            ShowTotalCol = true;
            columnsToHide = new List<int>();
        }
    }
}
