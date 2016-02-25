using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Resources;

namespace StatCalc
{
    /// <summary>
    /// Interaction logic for StatCalcMenu.xaml
    /// </summary>
    public partial class StatCalcMenu : UserControl
    {
        public StatCalcMenu()
        {
           InitializeComponent();
        }

        private void populationSurveyControl_Click(object sender, MouseButtonEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.PopulationSurvey).Show();
        }

        private void cohortCrossControl_Click(object sender, MouseButtonEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Cohort).Show();
        }

        private void unmatchedControl_Click(object sender, MouseButtonEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.UnmatchedCaseControl).Show();
        }

        private void tables2x2Control_Click(object sender, MouseButtonEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.TwoByTwo).Show();
        }

        private void poissonControl_Click(object sender, MouseButtonEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Poisson).Show();
        }

        private void populationBinomial_Click(object sender, MouseButtonEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Binomial).Show();
        }

        private void matchedPairControl_Click(object sender, MouseButtonEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.MatchedPairCaseControl).Show();
        }

        private void chiSquareTrend_Click(object sender, MouseButtonEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.ChiSquareForTrend).Show();
        }
    }
}
