using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class CrosstabParameters : FrequencyParametersBase
    {
        public bool TreatOutcomeAsContinuous { get; set; }
        public bool StrataSummaryOnly { get; set; }
        public bool SmartTable { get; set; }
        public bool ShowPercents { get; set; }
        public List<string> YesValues { get; set; }
        public List<string> NoValues { get; set; }
        public bool HorizontalDisplayMode { get; set; }
        public string MaxColumnNameLength { get; set; }
        public bool ConditionalShading { get; set; }
        public SolidColorBrush LoColorFill { get; set; }
        public SolidColorBrush HiColorFill { get; set; }
        public int? BreakType { get; set; }
        public string Break1 { get; set; }
        public string Break2 { get; set; }
        public string Break3 { get; set; }
        public string Break4 { get; set; }
        public string Break5 { get; set; }

        public bool DisplayChiSq { get; set; }
        

        //public FrequencyParametersBase fpbParameters { get; set; }
        //public GadgetParametersBase gpbParameters { get; set; }

        public CrosstabParameters()
            : base()
        {
            GadgetTitle = "Crosstabulation (MxN, 2x2)";
            TreatOutcomeAsContinuous = false;
            StrataSummaryOnly = false;
            SmartTable = true;
            ShowPercents = true;
            YesValues = new List<string>();
            NoValues = new List<string>();
            HorizontalDisplayMode = true;
            MaxColumnNameLength = "24";
            ConditionalShading = false;
            LoColorFill = new SolidColorBrush(Colors.White);
            HiColorFill = new SolidColorBrush(Colors.Tomato);
            BreakType = 0;
            Break1 = "0";
            Break2 = "20";
            Break3 = "40";
            Break4 = "60";
            Break5 = "80";
            DisplayChiSq = true; // EI-146
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public CrosstabParameters(CrosstabParameters parameters)
            : base(parameters)
        {
            TreatOutcomeAsContinuous = parameters.TreatOutcomeAsContinuous;
            StrataSummaryOnly = parameters.StrataSummaryOnly;
            SmartTable = parameters.SmartTable;
            ShowPercents = parameters.ShowPercents;
            YesValues = new List<string>();
            foreach (string YVal in parameters.YesValues)
            {
                YesValues.Add(YVal);
            }
            NoValues = new List<string>();
            foreach (string NVal in parameters.NoValues)
            {
                NoValues.Add(NVal);
            }
            HorizontalDisplayMode = parameters.HorizontalDisplayMode;
            MaxColumnNameLength = parameters.MaxColumnNameLength;
            ConditionalShading = parameters.ConditionalShading;
            LoColorFill = parameters.LoColorFill;
            HiColorFill = parameters.HiColorFill;
            BreakType = parameters.BreakType;
            Break1 = parameters.Break1;
            Break2 = parameters.Break2;
            Break3 = parameters.Break3;
            Break4 = parameters.Break4;
            Break5 = parameters.Break5;
            DisplayChiSq = parameters.DisplayChiSq; //EI-146
        }
    }
}
