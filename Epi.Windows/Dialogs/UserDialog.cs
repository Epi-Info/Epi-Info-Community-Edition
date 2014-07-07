using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Xml;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Epi.Windows.Dialogs
{
    /// <summary>
    /// Class UserDialog
    /// </summary>
    /// <remarks>The dialog that is created when the pgm invokes the DIALOG command</remarks>
    public partial class UserDialog : DialogBase
    {
        #region Private Attributes
        private String _value = string.Empty;
        private XmlDocument _doc = null;
        private String title = String.Empty;
        #endregion Private Attributes

        #region Constructors
        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public UserDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// UserDialog Constructor
        /// </summary>
        /// <param name="frm">Analysis module parent form</param>
        /// <param name="doc">UserDialog.xml template</param>
        public UserDialog(MainForm frm, XmlDocument doc)
            : base(frm)
        {
            _doc = doc;
            InitializeComponent();
            title = frm.Text;
            Construct(doc);
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Public property value
        /// </summary>
        /// <remarks>The value (if any) that the user entered/selected</remarks>
        public string Value
        {
            get
            {
                return _value;
            }
        }
        #endregion Public Properties

        #region Private Methods

        /// <summary>
        /// UI Setup Code for UserDialog Constructor
        /// </summary>
        /// <param name="controls">UserDialog.xml UserDialog/Controls node</param>
        private void SetControls(XmlNode controls)
        {
            foreach (XmlElement control in controls)
            {
                string controlName = control.Name;
                bool isVisible = (control.Attributes["Visible"].Value.ToString() != "0");
                // DEFECT #238
                if (controlName.StartsWith("btn"))
                {
                    panel2.Controls[controlName].Visible = isVisible;
                }
                else
                {
                    panel1.Controls[controlName].Visible = isVisible;
                    if (controlName == "txt1")
                    {
                        int maxLen;
                        if (int.TryParse(StripQuotes(control.Attributes["MaxLength"].Value), out maxLen))
                        {
                            ((TextBox)panel1.Controls[controlName]).MaxLength = maxLen;
                        }
                    }
                }
            }
        }

        private void FillList(ComboBox cmb, XmlNode node)
        {
            cmb.Items.Clear();
            foreach (XmlElement element in node.ChildNodes)
            {
                //                cmb.Items.Add(element.Name);
                cmb.Items.Add(element.Attributes["Value"].Value.ToString());
            }
        }

        /// <summary>
        /// StripQuotes
        /// </summary>
        /// <remarks>Just to make it more understandable</remarks>
        /// <param name="quotedString"></param>
        /// <returns>Literal minus Epi.StringLiterals.DOUBLEQUOTES</returns>
        private string StripQuotes(string quotedString)
        {
            return quotedString.Replace(StringLiterals.DOUBLEQUOTES, string.Empty);
        }

        /// <summary>
        /// UI Setup Code for UserDialog Constructor
        /// </summary>
        /// <param name="doc">UserDialog.xml template</param>
        private void Construct(XmlDocument doc)
        {
            //DEFECT #238
            XmlElement xmlDocElement = doc.DocumentElement;
            if (xmlDocElement != null)
            {
                this.Text = StripQuotes(xmlDocElement.GetAttribute("Title"));
                if (String.IsNullOrEmpty(Text))
                //dcs0 9/25 use a default title.
                {
                    Text = title;
                }
                Prompt.Text = StripQuotes(xmlDocElement.GetAttribute("Prompt"));
                string mask = StripQuotes(xmlDocElement.GetAttribute("Mask"));
                dtp1.Format = (string.IsNullOrEmpty(mask)) ? DateTimePickerFormat.Short : DateTimePickerFormat.Custom;
                dtp1.CustomFormat = mask;
                mtb1.Mask = mask;
                XmlNode controls = doc.SelectSingleNode("/UserDialog/Controls");
                SetControls(controls);
                if (xmlDocElement.GetAttribute("DialogType") == "Simple")
                {
                    Prompt.Dock = DockStyle.Fill;
                }
                else
                {
                    // dcs0 don't check the visible attribute (it's not valid at this time), just fill 'er up
                    FillList(cmb1, doc.SelectSingleNode("/UserDialog/ListItems"));
                }
            }
        }

        #endregion Private Methods

        #region Event handlers
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (this.mtb1.Visible)
            {
                _value = mtb1.Text;
            }
            else if (this.cmb1.Visible)
            {
                _value = cmb1.Text;
            }
            else if (this.dtp1.Visible)
            {

                _value = dtp1.Text;
                // Parser will not handle the AM/PM indicator, so convert it to 24 hr time
                DateTime aDate = DateTime.Now;
                if (DateTime.TryParse(_value, out aDate))
                {
                    string fmt = dtp1.CustomFormat.Replace(" tt", "").Replace("h", "H");
                    _value = aDate.ToString(fmt);
                }
            }
            else if (this.txt1.Visible)
            {
                _value = txt1.Text;
            }
            _doc.DocumentElement.SetAttribute("VarValue", _value);
            Close();
        }
        private void btnYes_Click(object sender, EventArgs e)
        {
            _value = "(+)";
            _doc.DocumentElement.SetAttribute("VarValue", _value);
            Close();
        }
        private void btnNo_Click(object sender, EventArgs e)
        {
            _value = "(-)";
            _doc.DocumentElement.SetAttribute("VarValue", _value);
            Close();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion Event handlers

    }
}