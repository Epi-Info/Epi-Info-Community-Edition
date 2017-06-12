using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class RatesParameters : GadgetParametersBase
    {
        public bool SortColumnsByTabOrder { get; set; }
        public bool UsePromptsForColumnNames { get; set; }
        public bool ShowColumnHeadings { get; set; }
        public bool ShowLineColumn { get; set; }
        public bool ShowNullLabels { get; set; }
        public bool NumerDistinct { get; set; }
        public bool DenomDistinct { get; set; }
        public bool ShowAsPercent { get; set; }
        public string NumeratorField { get; set; }
        public string DenominatorField { get; set; }
        public string NumeratorAggregator { get; set; }
        public string DenominatorAggregator { get; set; }
        public string PrimaryGroupField { get; set; }
        public string SecondaryGroupField { get; set; }
        public int MaxColumnLength { get; set; }
        public string RateMultiplierString { get; set; }
        public int MaxRows { get; set; }
        public DataFilters NumerFilter { get; set; }
        public DataFilters DenomFilter { get; set; }

        public string DefaultColor { get; set; }
        public bool UseDefaultColor { get; set; }
        public string Color_L1 { get; set; }
        public double HighValue_L1 { get; set; }
        public string Color_L2 { get; set; }
        public double HighValue_L2 { get; set; }
        public double LowValue_L2 { get; set; }
        public string Color_L3 { get; set; }
        public double HighValue_L3 { get; set; }
        public double LowValue_L3 { get; set; }
        public string Color_L4 { get; set; }
        public double LowValue_L4 { get; set; }

        public double RateMultiplier
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RateMultiplierString))
                {
                    return 100;
                }
                else
                { 
                    string selectedText = RateMultiplierString;

                    int multiplierInt = 100;
                    double multiplierDouble = 100;

                    if (int.TryParse(selectedText, out multiplierInt))
                    {
                        return multiplierInt;
                    }
                    else if (selectedText.Contains("^"))
                    {
                        string ortex = selectedText.Replace("10^", "1E");

                        if (double.TryParse(ortex, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out multiplierDouble))
                        {
                            return (double)multiplierDouble;
                        }
                    }

                    return 100;
                }
            }
        }

        public RatesParameters()
            : base()
        {
            SortColumnsByTabOrder = false;
            UsePromptsForColumnNames = false;
            ShowColumnHeadings = true;
            ShowLineColumn = true;
            NumerDistinct = false;
            DenomDistinct = false;
            ShowNullLabels = true;
            ShowAsPercent = true;
            NumeratorField = String.Empty;
            DenominatorField = String.Empty;
            PrimaryGroupField = String.Empty;
            SecondaryGroupField = String.Empty;
            RateMultiplierString = "100";
            GadgetTitle = DashboardSharedStrings.GADGET_CONFIG_TITLE_RATES;  //"Rates";
            UseDefaultColor = true;

            Color_L1 = "#FF1A237E"; // GOOGLE MATERIAL DESIGN INDEGO 900
            Color_L2 = "#FFF57F17"; // GOOGLE MATERIAL DESIGN YELLOW 900
            Color_L3 = "#FFFF6F00"; // GOOGLE MATERIAL DESIGN ORANGE 900
            Color_L4 = "#FFE65100"; // GOOGLE MATERIAL DESIGN RED    900

            Color_L1 = "#FF3F51B5"; // GOOGLE MATERIAL DESIGN INDEGO 500
            Color_L2 = "#FFFFEB3B"; // GOOGLE MATERIAL DESIGN YELLOW 500
            Color_L3 = "#FFFF9800"; // GOOGLE MATERIAL DESIGN ORANGE 500
            Color_L4 = "#FFF44336"; // GOOGLE MATERIAL DESIGN RED    500

            DefaultColor = Color_L1;

            HighValue_L1 = double.NaN;
            LowValue_L2 = double.NaN;
            HighValue_L2 = double.NaN;
            LowValue_L3 = double.NaN;
            HighValue_L3 = double.NaN;
            LowValue_L4 = double.NaN;
        }
    }
}
