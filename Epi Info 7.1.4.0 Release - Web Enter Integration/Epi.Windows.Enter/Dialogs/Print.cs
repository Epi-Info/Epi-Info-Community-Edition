using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi.Windows.Enter.PresentationLogic;
using Epi.EnterCheckCodeEngine;
using Epi.Resources;
using Epi.Tools;

namespace Epi.Windows.Enter.Dialogs
{
	public partial class Print: Form
	{
        int fromRecord;
        int toRecord;
        int _recordStart, _recordEnd, _pageNumberStart, _pageNumberEnd;        

		public Print()
		{
            InitializeComponent();
            HandleOsIssues();
            this.mediator = GuiMediator.Instance;
            selectRecords_None.Checked = true;
            selectPages_All.Checked = true;

            selectRecords_Current.Enabled = true;
            selectRecords_Range.Enabled = true;
            selectRecords_All.Enabled = true;

            //*****
            //Added by PSimon
            //check if records exist...if none then disable printing options for records
            int recordCount = this.mediator.View.GetRecordCount();
            if (recordCount == 0)
            {
                //disable controls               
                this.selectRecords_Current.Enabled = false;
                this.selectRecords_All.Enabled = false;
                this.selectRecords_Range.Enabled = false;
            }
            //End of modified code
            //*****
            SelectTab_Order.Visible = false;
            int _recordStart = -1;
            int _recordEnd = -1;
            int _pageStart = 1;
            int _pageEnd = mediator.View.Pages.Count();
  	    }
      
        private void HandleOsIssues()
        {
            Epi.Tools.DetermineOS Os = new Epi.Tools.DetermineOS();            
            string runningOS;

            runningOS = Os.getOSInfo();

            if (runningOS.Contains("7"))
            {
                OnlyPrintBlankForm(true);
                EnableRecordRange(true);
                EnablePageRange(true);
            }
            else
            {
                OnlyPrintBlankForm(false);
            }
        }

        private void OnlyPrintBlankForm(bool enabled)
        {
            selectRecords_Range.Enabled = enabled;
            selectRecords_Current.Enabled = enabled;
        }

        private void buttonPrint_Click(object sender, EventArgs e)
        {
            mediator.Print(_recordStart, _recordEnd, _pageNumberStart, _pageNumberEnd);
        }

        private void buttonPrintPreview_Click(object sender, EventArgs e)
        {
            mediator.PrintPreview(_recordStart, _recordEnd, _pageNumberStart, _pageNumberEnd);
        }

        private void EnableRecordRange(bool enabled)
        {
            recordStart.Enabled = enabled;
            recordEnd.Enabled = enabled;
        }

        private void EnablePageRange(bool enabled)
        {
            pageStart.Enabled = enabled;
            pageEnd.Enabled = enabled;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void recordStart_TextChanged(object sender, System.EventArgs e)
        {
            if (int.TryParse(recordStart.Text, out _recordStart) == false)
            {
                _recordStart = mediator.View.GetRecordCount();
            }
        }

        void recordEnd_TextChanged(object sender, System.EventArgs e)
        {
            if (int.TryParse(recordEnd.Text, out _recordEnd) == false)
            {
                _recordEnd = mediator.View.GetRecordCount();
            }
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
                _pageNumberEnd = mediator.View.Pages.Count();
            }
        }

        private void recordStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void recordEnd_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void pageStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void pageEnd_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void selectRecords_None_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                EnableRecordRange(false);
                _recordStart = -1;
                _recordEnd = -1;             
            }
        }

        private void selectRecords_Current_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                EnableRecordRange(false);
                _recordStart = mediator.View.CurrentRecordId;
                _recordEnd = mediator.View.CurrentRecordId;               
            }
        }

        private void selectRecords_Range_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                EnableRecordRange(true);
                recordStart.Focus();

                if (string.IsNullOrEmpty(recordStart.Text))
                {
                    recordStart.Text = "1";
                }
                if (string.IsNullOrEmpty(recordEnd.Text))
                {
                    recordEnd.Text = mediator.View.GetRecordCount().ToString();
                }

                _recordStart = Convert.ToInt32(recordStart.Text);
                _recordEnd = Convert.ToInt32(recordEnd.Text);                
            }
        }

        private void selectRecords_All_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                EnableRecordRange(false);
                _recordStart = 1;
                _recordEnd = mediator.View.GetRecordCount();               
            }
        }

        private void selectPages_All_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                EnablePageRange(false);
                _pageNumberStart = 1;
                _pageNumberEnd = mediator.View.Pages.Count();
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
                    pageEnd.Text = mediator.View.Pages.Count().ToString();
                }

                _pageNumberStart = Convert.ToInt32(pageStart.Text);
                _pageNumberEnd = Convert.ToInt32(pageEnd.Text);
            }
        }

        private void SelectTab_Order_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                EnableRecordRange(false);               
            }

        }
	}
}
