using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace StatCalc
{
    public partial class StatCalc : Form
    {
        private System.Windows.UIElement control;
        private ElementHost host;

        public enum Calculators
        {
            TwoByTwo,
            PopulationSurvey,
            UnmatchedCaseControl,
            Cohort,
            ChiSquareForTrend,
            Poisson,
            Binomial,
            MatchedPairCaseControl
        }

        public StatCalc()
        {
            InitializeComponent();
        }

        public StatCalc(Calculators calculator)
        {
            InitializeComponent();

            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            switch (calculator)
            {
                case Calculators.TwoByTwo:
                    control = new EpiDashboard.StatCalc.TwoByTwo();
                    break;
                case Calculators.PopulationSurvey:
                    control = new EpiDashboard.StatCalc.PopulationSurvey();
                    break;
                case Calculators.Poisson:
                    control = new EpiDashboard.StatCalc.Poisson();
                    break;
                case Calculators.Binomial:
                    control = new EpiDashboard.StatCalc.Binomial();
                    break;
                case Calculators.UnmatchedCaseControl:
                    control = new EpiDashboard.StatCalc.UnmatchedCaseControl();
                    break;
                case Calculators.Cohort:
                    control = new EpiDashboard.StatCalc.Cohort();
                    break;
                case Calculators.ChiSquareForTrend:
                    control = new EpiDashboard.StatCalc.ChiSquareControl();
                    //((EpiDashboard.StatCalc.ChiSquareControl)control).SizeChanged += new System.Windows.SizeChangedEventHandler(StatCalc_SizeChanged);
                    break;
                case Calculators.MatchedPairCaseControl:
                    control = new EpiDashboard.Gadgets.StatCalc.MatchedPairCaseControlGadget();
                    (control as EpiDashboard.Gadgets.StatCalc.MatchedPairCaseControlGadget).IsHostedInOwnWindow = true;
                    break;
                default:
                    break;
            }
            if (control != null)
            {
                if (control is EpiDashboard.StatCalc.IStatCalcControl)
                {
                    host.Child = control;
                    this.Controls.Add(host);
                    this.Width = ((EpiDashboard.StatCalc.IStatCalcControl)control).PreferredUIWidth;
                    this.Height = ((EpiDashboard.StatCalc.IStatCalcControl)control).PreferredUIHeight + 10;
                    ((EpiDashboard.StatCalc.IStatCalcControl)control).HideCloseIcon();
                }
                else if (control is EpiDashboard.Gadgets.IStatCalcControl)
                {
                    host.Child = control;
                    this.Controls.Add(host);
                    this.Width = ((EpiDashboard.Gadgets.IStatCalcControl)control).PreferredUIWidth;
                    this.Height = ((EpiDashboard.Gadgets.IStatCalcControl)control).PreferredUIHeight + 10;
                }
            }           
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatCalc));
            this.SuspendLayout();
            // 
            // StatCalc
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true; 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;                              
            this.ClientSize = new System.Drawing.Size(682, 555);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StatCalc";           
            this.ResumeLayout(false);

        }
    }
}
