using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi.Windows.MakeView.PresentationLogic;

namespace Epi.Windows.MakeView.Dialogs
{
    public partial class Print : Form
    {
        int _pageNumberStart, _pageNumberEnd;
        public Print( GuiMediator mediator)
        {
            InitializeComponent();
            this.mediator = mediator;
            HandleOsIssues();         
            selectPages_All.Checked = true;          
            int _pageStart = 1;
            int _pageEnd = mediator.ProjectExplorer.currentPage.view.Pages.Count();
        }           

        private void HandleOsIssues()
        {
            Epi.Tools.DetermineOS Os = new Epi.Tools.DetermineOS();
            string runningOS;

            runningOS = Os.getOSInfo();

            if (runningOS.Contains("7"))
            {                            
                EnablePageRange(true);
            }
            else
            {
                EnablePageRange(false);
            }
        }

        private void EnablePageRange(bool enabled)
        {
            pageStart.Enabled = enabled;
            pageEnd.Enabled = enabled;
        }

        void pageStart_TextChanged(object sender, System.EventArgs e)
        {
            if (int.TryParse(pageStart.Text, out _pageNumberStart) == false)
            {
                _pageNumberStart = 1;
            }
        }

        void pageEnd_TextChanged(object sender, System.EventArgs e)
        {
            if (int.TryParse(pageEnd.Text, out _pageNumberEnd) == false)
            {
                _pageNumberEnd = mediator.ProjectExplorer.currentPage.view.Pages.Count();
            }
        }

        private void selectPages_All_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                EnablePageRange(false);
                _pageNumberStart = 1;
                _pageNumberEnd = mediator.ProjectExplorer.currentPage.view.Pages.Count();
            }
        }

        private void selectPages_Range_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                EnablePageRange(true);
                if (string.IsNullOrEmpty(pageStart.Text))
                {
                    pageStart.Text = "1";
                }
                if (string.IsNullOrEmpty(pageEnd.Text))
                {
                    pageEnd.Text = mediator.ProjectExplorer.CurrentView.Pages.Count().ToString();
                }

                _pageNumberStart = Convert.ToInt32(pageStart.Text);
                _pageNumberEnd = Convert.ToInt32(pageEnd.Text);
            }
        }

        private void buttonPrint_Click_1(object sender, EventArgs e)
        {
            if (this.selectPages_All.Checked)
            {
                _pageNumberStart = 1;
                _pageNumberEnd = mediator.ProjectExplorer.currentPage.view.Pages.Count();

            }
            else
            {
                _pageNumberStart = Convert.ToInt32(pageStart.Text);
                _pageNumberEnd = Convert.ToInt32(pageEnd.Text);
            }
            mediator.Print(_pageNumberStart, _pageNumberEnd);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
