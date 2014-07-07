#region Namespaces
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Epi;
using Epi.Fields;

#endregion

namespace Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs
{
    /// <summary>
    /// The Label Field Definition dialog
    /// </summary>
    public partial class LabelFieldDefinition : GenericFieldDefinition
	{
		#region	Private Controls
		private LabelField field;
		#endregion	//Private Controls

		#region	Constructors
        /// <summary>
        /// Default Constsructor, for exclusive use by the designer.
        /// </summary>
        public LabelFieldDefinition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for Label Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="page">Page to render label field</param>
		public LabelFieldDefinition(MainForm frm, Page page) : base(frm)
		{
			InitializeComponent();
			this.mode = FormMode.Create;
			this.page = page;
		}

        /// <summary>
        /// Constructor for Label Field Definition
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="field">The label field</param>
		public LabelFieldDefinition(MainForm frm, LabelField field) : base(frm)
		{
            InitializeComponent();
            this.txtPrompt.AcceptsReturn = true;
            this.txtPrompt.AcceptsTab = true;
			this.mode = FormMode.Edit;
			this.field = field;
			this.page = field.Page;
			LoadFormData();
		}
		#endregion	Constructors

		#region	Private Methods

		private void LoadFormData()
		{
            Configuration config = Configuration.GetNewInstance();
            FontStyle style = FontStyle.Regular;
            if (config.Settings.EditorFontBold)
            {
                style |= FontStyle.Bold;
            }
            if (config.Settings.EditorFontItalics)
            {
                style |= FontStyle.Italic;
            }
            if ((field.ControlFont == null) || ((field.ControlFont.Name == "Microsoft Sans Serif") && (field.ControlFont.Size == 8.5)))
            {
                field.ControlFont = new Font(config.Settings.EditorFontName, (float)config.Settings.EditorFontSize, style);
            }
			txtPrompt.Text = field.PromptText;
			txtFieldName.Text = field.Name;
            promptFont = field.ControlFont;
            controlFont = field.ControlFont;
		}
		#endregion	//Private Methods

		#region Public Methods

        /// <summary>
        /// Sets the field's properties based on GUI values
        /// </summary>
        protected override void SetFieldProperties()
        {
            field.PromptText = txtPrompt.Text;
            field.Name = txtFieldName.Text;
            
            if (promptFont != null)
            {
                field.PromptFont = promptFont;
            }
            if (controlFont != null)
            {
                field.ControlFont = controlFont;
            }
        }

		/// <summary>
		/// Gets the field defined by this field definition dialog
		/// </summary>
		public override RenderableField Field
		{
			get
			{
				return field;
			}
		}
		#endregion	//Public Methods

		#region Event Handlers
		private void btnLabelFont_Click(object sender, System.EventArgs e)
		{
			FontDialog dialog = new FontDialog();
			if (controlFont != null)
			{
				dialog.Font = controlFont;
			}
			DialogResult result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				controlFont = dialog.Font;
                promptFont = dialog.Font;
                ((GenericFieldDefinition)this).Controls["txtPrompt"].Focus();  
			}
		}
        
        private void btnOk_Click(object sender, EventArgs e)
        {

        }
		#endregion	//Event Handlers		

        private void txtPrompt_TextChanged(object sender, EventArgs e)
        {
            this.charCountLabel.Text = string.Format("{0} of 2047", txtPrompt.Text.Length);

            if (txtPrompt.Text.Length > 2047)
            {
                this.charCountLabel.ForeColor = Color.Firebrick;
            }
            else
            {
                this.charCountLabel.ForeColor = Color.Black;
            }
        }

        protected override void txtPrompt_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.G:
                        if (txtPrompt.Focused == true)
                        {
                            txtPrompt.Text += AddGreek();
                        }
                        else if (txtFieldName.Focused == true)
                        {
                            txtFieldName.Text += AddGreek();
                        }
                        break;
                }
            }
        }

        String AddGreek()
        {
            String latinSentence = string.Empty;
            
            string words = "omnis qui bill transisse camera repraesentantum et senatus et antequam fiat praeceptum deferat praeses civitatum foederatarum si ipse probat signant sin ille reddit cum obiectiones penates in quibus nata erit qui intrábunt in latitudine obiecta eorum journal et transiens retracto eam si post talem recogitatio duas partes domus illius conveniat bill transeat erit misit cum primum ad aliam domum qua similiter in relata si probari quod duas partes domus erit Law sed in omnibus casibus et sententiis houses dirimatur et nays yeas et nomina personarum et rogandis bill in ingressus fuerit acta singulis domibus respective si non praeses reversus intra decem dies dominica exceptis postquam fuerit oblata ille sit lex non secus acsi signavi nisi per dilationem congress ne reditum in hoc casu non lege protinus convenerunt ipsi ob primam electionem erunt aeque divisi ut in tria genera sedibus senatorum primi generis investigationes erunt in elapso anno secundo classis ad elapso anno quarto et tertium genus in elapso anno ut sit tertia omnis electus anno secundo et si contingat vacancies renuntiatione vel aliter per sinum cuiuslibet legifero publicae earum executive appointments temporariam sequenti sessionem legifero qui talia sic imple Ti quisque domo iudex comitia redit neu propriis alumnis ac pars maior constituunt quisque facere quorum business sed ut paucioribus dimitti diem et simus auctores cogere ministrorum absentis membra ita et sub talibus vt efficiantur singulis domibus";
            string[] wordArray = words.Split(' ');
            Random random = new Random(Convert.ToInt32(DateTime.Now.Millisecond));
            int sentenceLength = 4 + random.Next() % 6;
            string sentence = string.Empty;
            for (int i = 0; i < sentenceLength; i++)
            {
                sentence = sentence + wordArray[random.Next(wordArray.Length)] + " ";
            }
            sentence = sentence.TrimEnd(' ');
            sentence = sentence + ". ";
            latinSentence = sentence[0].ToString().ToUpper() + sentence.Substring(1);

            return latinSentence;
        }
	}
}
