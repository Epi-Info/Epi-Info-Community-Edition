using System;
using System.Collections.Generic;
using System.IO;
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
using Epi.Windows;
using Epi.Windows.Dialogs;
using System.Threading;

namespace StatCalc
{
    /// <summary>
    /// Interaction logic for StatCalcMenu.xaml
    /// </summary>
    public partial class StatCalcMenu : UserControl
    {
        MainForm mainform = null;
        public StatCalcMenu()
        {
           InitializeComponent();
           Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
           this.tsslLocale.Text = Thread.CurrentThread.CurrentUICulture.Name;
           this.tsslVersion.Text = appId.Version;

            #region Translation

            txtPopulationSurvey.Text = StatCalcSharedStrings.STATCALC_POPULATION_SURVEY;
            txtCohortorCross.Text = StatCalcSharedStrings.STATCALC_COHORT_CROSS;
            unmatchedCaseControl.Text = StatCalcSharedStrings.STATCALC_UNMATCHED_CASE;
            txtChiSquare.Text = StatCalcSharedStrings.STATCALC_CHI_SQUARE;
            txtTables22N.Text = StatCalcSharedStrings.STATCALC_TABLES_22N;
            txtPoisson.Text = StatCalcSharedStrings.STATCALC_POISSON_RARE;
            txtPopulationBinomial.Text = StatCalcSharedStrings.STATCALC_POPULATION_BINOMIAL;
            txtMatchedPairCase.Text = StatCalcSharedStrings.STATCALC_MATCHED_PAIR_CASE;
            txtLanguage.Text = StatCalcSharedStrings.STATCALC_LANGUAGE;
            txtVersion.Text = StatCalcSharedStrings.STATCALC_VERSION;
            #endregion Translation

        }

        private void populationSurveyControl_Click(object sender, RoutedEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.PopulationSurvey).Show();
        }

        private void cohortCrossControl_Click(object sender, RoutedEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Cohort).Show();
        }

        private void unmatchedControl_Click(object sender, RoutedEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.UnmatchedCaseControl).Show();
        }

        private void tables2x2Control_Click(object sender, RoutedEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.TwoByTwo).Show();
        }

        private void poissonControl_Click(object sender, RoutedEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Poisson).Show();
        }

        private void populationBinomial_Click(object sender, RoutedEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.Binomial).Show();
        }

        private void matchedPairControl_Click(object sender, RoutedEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.MatchedPairCaseControl).Show();
        }

        private void chiSquareTrend_Click(object sender, RoutedEventArgs e)
        {
            new StatCalc(StatCalc.Calculators.ChiSquareForTrend).Show();
        }

        private void Hyperlink_epiInfoWebsite(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        protected void OnAboutClicked()
        {
            if (mainform == null)
                mainform = new MainForm();
            AboutEpiInfoDialog dlg = new AboutEpiInfoDialog(mainform);
            dlg.ShowDialog();
        }
        private void aboutEpiInfo_click(object sender, RoutedEventArgs e)
        {
            OnAboutClicked();
        }

        protected virtual void OnApplyChanges(object sender, EventArgs args)
        {
            OnOptionsChanged();
        }
        protected virtual void OnOptionsChanged()
        {
        }
        protected void OnOptionsClicked(OptionsDialog.OptionsTabIndex tabIndex)
        {
            if (mainform == null)
                mainform = new MainForm();
            OptionsDialog dlg = new OptionsDialog(mainform, tabIndex);
            try
            {
                dlg.ApplyChanges += new OptionsDialog.ApplyChangesHandler(OnApplyChanges);
                System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    OnOptionsChanged();
                }
            }
            finally
            {
                dlg.Close();
                dlg.Dispose();
            }
        }
        private void btnLocaleClick(object sender, RoutedEventArgs e)
        {
            OnOptionsClicked(OptionsDialog.OptionsTabIndex.Language);
        }

        private void populationSurveyControl_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                new StatCalc(StatCalc.Calculators.PopulationSurvey).Show();
            }
        }

        private void cohortCrossControl_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                new StatCalc(StatCalc.Calculators.Cohort).Show();
            }
        }

        private void unmatchedControl_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                new StatCalc(StatCalc.Calculators.UnmatchedCaseControl).Show();
            }
        }

        private void chiSquareTrend_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                new StatCalc(StatCalc.Calculators.ChiSquareForTrend).Show();
            }
        }

        private void tables2x2Control_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                new StatCalc(StatCalc.Calculators.TwoByTwo).Show();
            }
        }

        private void poissonControl_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                new StatCalc(StatCalc.Calculators.Poisson).Show();
            }
        }

        private void populationBinomial_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                new StatCalc(StatCalc.Calculators.Binomial).Show();
            }
        }

        private void matchedPairControl_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                new StatCalc(StatCalc.Calculators.MatchedPairCaseControl).Show();
            }
        }

        private void epiInfoWebsite_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
               
                System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/");
            }
        }

        private void aboutEpiInfo_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
               
                OnAboutClicked();
            }
        }

        private void language_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                
                OnOptionsClicked(OptionsDialog.OptionsTabIndex.Language);
            }
        }
        private void epiInfoWebsite_Click(object sender, EventArgs e)
        {
            
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/");
        }
    }
}
