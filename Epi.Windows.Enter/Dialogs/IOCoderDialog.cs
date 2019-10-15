using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NIOSH.IOCode;
using System.Runtime.InteropServices;

namespace Epi.Enter.Dialogs
{
    public partial class IOCoderDialog : Form
    {
        private NIOSH.IOCode.IOCoder ioCoder;
        private float selectionThreshold;
        private float minProbability;
        private int maxItems;

        public string Industry = string.Empty;
        public string Occupation = string.Empty;
        public string IndustryTitle = string.Empty;
        public string IndustryCode = string.Empty;
        public string OccupationTitle = string.Empty;
        public string OccupationCode = string.Empty;
        public string CodingScheme = string.Empty;
        private bool textUpdatedByUser;


        public IOCoderDialog(
            NIOSH.IOCode.IOCoder ioCoder, 
            string industry, 
            string occupation, 
            float selectionThreshold = 0.0F, 
            float minProbability = 0.0F, 
            int maxItems = 0)
        {
            InitializeComponent();

            this.ioCoder = ioCoder;
            this.selectionThreshold = selectionThreshold;
            this.minProbability = minProbability;
            this.maxItems = maxItems;

            this.populate(industry, occupation);
        }

        private void txt_TextChanged(object sender, EventArgs e)
        {
            if (this.textUpdatedByUser)
                this.updateResults();
        }

        private void lstIndustry_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lblIndCode.Text = this.lstIndustry.SelectedItem.ToString().Split(Convert.ToChar("\t"))[0].Trim();
            this.lblIndTitle.Text = this.lstIndustry.SelectedItem.ToString().Split(Convert.ToChar("\t"))[1].Trim();
        }

        private void lstOccupation_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lblOccCode.Text = this.lstOccupation.SelectedItem.ToString().Split(Convert.ToChar("\t"))[0].Trim();
            this.lblOccTitle.Text = this.lstOccupation.SelectedItem.ToString().Split(Convert.ToChar("\t"))[1].Trim();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.select();
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.clear();
            this.Hide();
        }

        private void populate(string industry, string occupation)
        {
            this.clear();

            if (ioCoder != null)
            {
                this.lblScheme.Text = this.ioCoder.CodingScheme;
                this.lstIndustry.Enabled = true;
                this.lstOccupation.Enabled = true;
                this.txtIndustry.Enabled = true;
                this.txtOccupation.Enabled = true;
                this.btnOK.Enabled = true;
                this.btnCancel.Enabled = true;

                this.textUpdatedByUser = false;
                this.txtIndustry.Text = industry;
                this.txtOccupation.Text = occupation;
                this.textUpdatedByUser = true;

                this.updateResults();
            }
            else
            {
                this.lstIndustry.Enabled = false;
                this.lstOccupation.Enabled = false;
                this.txtIndustry.Enabled = false;
                this.txtOccupation.Enabled = false;
                this.btnOK.Enabled = false;
                this.btnCancel.Enabled = false;
            }
        }

        private void updateResults()
        {
            bool inputEntered = (this.txtIndustry.Text.Trim() != "") || (this.txtOccupation.Text.Trim() != "");

            if (inputEntered)
            {
                bool partialMatchInd = (this.txtIndustry.ContainsFocus && this.txtIndustry.SelectionStart == this.txtIndustry.Text.Length);
                bool partialMatchOcc = (this.txtOccupation.ContainsFocus && this.txtOccupation.SelectionStart == this.txtOccupation.Text.Length);

                List<CodeResult> iResults = ioCoder.CodeIndustry(
                    this.txtIndustry.Text + ((partialMatchInd) ? "*" : ""),
                    this.txtOccupation.Text + ((partialMatchOcc) ? "*" : ""),
                    maxReturned: this.maxItems,
                    minProbability: this.minProbability);

                List<CodeResult> oResults = ioCoder.CodeOccupation(
                    this.txtIndustry.Text + ((partialMatchInd) ? "*" : ""),
                    this.txtOccupation.Text + ((partialMatchOcc) ? "*" : ""),
                    maxReturned: this.maxItems,
                    minProbability: this.minProbability);

                this.lblIndCode.Text = "";
                this.lblIndTitle.Text = "";
                this.lstIndustry.Items.Clear();
                bool firstItem = true;
                foreach (CodeResult cr in iResults)
                {
                    this.lstIndustry.Items.Add(cr.Code + "\t" + cr.Title);
                    if (firstItem && cr.Probability >= this.selectionThreshold)
                    {
                        this.lblIndCode.Text = cr.Code;
                        this.lblIndTitle.Text = cr.Title;
                        this.lstIndustry.SelectedIndex = 0;
                    }
                    firstItem = false;
                }


                this.lblOccCode.Text = "";
                this.lblOccTitle.Text = "";
                this.lstOccupation.Items.Clear();
                firstItem = true;
                foreach (CodeResult cr in oResults)
                {
                    this.lstOccupation.Items.Add(cr.Code + "\t" + cr.Title);
                    if (firstItem && cr.Probability >= this.selectionThreshold)
                    {
                        this.lblOccCode.Text = cr.Code;
                        this.lblOccTitle.Text = cr.Title;
                        this.lstOccupation.SelectedIndex = 0;
                    }
                    firstItem = false;
                }

                this.lblScheme.Text = ioCoder.CodingScheme;
            }
            else
            {
                this.clear();
            }


        }

        private string formatProbability(float probability)
        {
            if (probability < 0.001)
                return "< 0.1%";
            else
                return string.Format("{0:#0.0}%", 100 * probability);
        }

        private void select()
        {
            this.Industry = this.txtIndustry.Text;
            this.Occupation = this.txtOccupation.Text;
            this.IndustryCode = this.lblIndCode.Text;
            this.IndustryTitle = this.lblIndTitle.Text;
            this.OccupationCode = this.lblOccCode.Text;
            this.OccupationTitle = this.lblOccTitle.Text;
            this.CodingScheme = this.lblScheme.Text;
        }

        private void clear()
        {
            this.textUpdatedByUser = false;
            this.txtIndustry.Text = "";
            this.txtOccupation.Text = "";
            this.textUpdatedByUser = true;

            this.lblIndCode.Text = "";
            this.lblIndTitle.Text = "";
            this.lblOccCode.Text = "";
            this.lblOccTitle.Text = "";

            this.lstIndustry.Items.Clear();
            this.lstOccupation.Items.Clear();

            this.Industry = "";
            this.Occupation = "";
            this.IndustryCode = "";
            this.IndustryTitle = "";
            this.OccupationCode = "";
            this.OccupationTitle = "";
            this.CodingScheme = "";
        }

    }
}
