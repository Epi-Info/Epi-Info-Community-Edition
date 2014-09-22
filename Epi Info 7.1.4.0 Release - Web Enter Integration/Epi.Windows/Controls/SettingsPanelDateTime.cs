#region Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Text;
using Epi.Data.Services;
using Epi.Windows;

#endregion

namespace Epi.Windows.Controls
{
    public partial class SettingsPanelDateTime : System.Windows.Forms.UserControl
	{
		#region Public Event Handlers

        public event System.EventHandler DateFormatChanged;
        public event System.EventHandler TimeFormatChanged;
        public event System.EventHandler DateTimeFormatChanged;

        public event System.EventHandler DateFormatTextChanged;
        public event System.EventHandler TimeFormatTextChanged;
        public event System.EventHandler DateTimeFormatTextChanged;

		#endregion

        DataView _dateTimeFormats;
        DataTable _dataTypes;

		#region Constructor
	
		public SettingsPanelDateTime()
		{
            InitializeComponent();

            _dateTimeFormats = AppData.Instance.DataPatternsDataTable.DefaultView;
            _dataTypes = AppData.Instance.DataTypesDataTable;
            LoadLists();
		}

		#endregion

		#region Public Methods

		public void LoadLists()
		{
            LoadDateFormat();
            LoadTimeFormat();
            LoadDateTimeFormat();
		}

		public void ShowSettings()
		{
            Configuration config = Configuration.GetNewInstance();
            Epi.DataSets.Config.SettingsRow settings = config.Settings;

            cmbDateFormat.SelectedItem = settings.DateFormat;
            cmbTimeFormat.SelectedItem = settings.TimeFormat;
            cmbDateTimeFormat.SelectedItem = settings.DateTimeFormat;
		}

        public void GetSettings(Configuration newConfig)
		{
            Epi.DataSets.Config.SettingsRow settings = newConfig.Settings;

            settings.DateFormat = cmbDateFormat.Text;
            settings.TimeFormat = cmbTimeFormat.Text;
            settings.DateTimeFormat = cmbDateTimeFormat.Text;
		}

		#endregion Public Methods

		#region Public Properties

        public string DateFormat { get { return cmbDateFormat.Text; } }
        public string TimeFormat { get { return cmbTimeFormat.Text; } }
        public string DateTimeFormat { get { return cmbDateTimeFormat.Text; } }

		#endregion

		#region Private Methods		

		private void LoadDateFormat()
		{
            bool isNull = true;// Configuration.GetNewInstance().Settings.IsDateFormatNull();
            string currentDateFormat = "MM-DD-YYYY";

            if(isNull == false)
            {
                currentDateFormat = Configuration.GetNewInstance().Settings.DateFormat;
            }
            
            cmbDateFormat.Items.Clear();
            if (!string.IsNullOrEmpty(currentDateFormat))
            {
                cmbDateFormat.Items.Add(currentDateFormat);
            }
            
            int dataTypeId = (int)_dataTypes.Select("Name = 'Date'")[0]["DataTypeId"];
            string selectExpression = string.Format("DataTypeId = '{0}' ", dataTypeId);
            DataRow[] rows = _dateTimeFormats.Table.Select(selectExpression); 

            foreach (DataRow row in rows)
            {
                string expression = row["Expression"].ToString();
                if (expression != currentDateFormat)
                {
                    cmbDateFormat.Items.Add(expression);
                }
            }

            cmbDateFormat.SelectedIndex = 0;
		}

		private void LoadTimeFormat()
		{
            bool isNull = true;// Configuration.GetNewInstance().Settings.IsDateFormatNull();
            string currenTimeFormat = "HH:MM:SS AMPM";

            if (isNull == false)
            {
                currenTimeFormat = Configuration.GetNewInstance().Settings.TimeFormat;
            }

            cmbTimeFormat.Items.Clear();
            if (!string.IsNullOrEmpty(currenTimeFormat))
            {
                cmbTimeFormat.Items.Add(currenTimeFormat);
            }

            int dataTypeId = (int)_dataTypes.Select("Name = 'Time'")[0]["DataTypeId"];
            string selectExpression = string.Format("DataTypeId = '{0}' ", dataTypeId);
            DataRow[] rows = _dateTimeFormats.Table.Select(selectExpression);

            foreach (DataRow row in rows)
            {
                string expression = row["Expression"].ToString();
                if (expression != currenTimeFormat)
                {
                    cmbTimeFormat.Items.Add(expression);
                }
            }

            cmbTimeFormat.SelectedIndex = 0;
        }

		private void LoadDateTimeFormat()
		{
            bool isNull = true;// Configuration.GetNewInstance().Settings.IsDateFormatNull();
            string currenDateTimeFormat = "MM-DD-YYYY HH:MM:SS AMPM";

            if (isNull == false)
            {
                currenDateTimeFormat = Configuration.GetNewInstance().Settings.DateTimeFormat;
            }

            cmbDateTimeFormat.Items.Clear();
            if (!string.IsNullOrEmpty(currenDateTimeFormat))
            {
                cmbDateTimeFormat.Items.Add(currenDateTimeFormat);
            }

            int dataTypeId = (int)_dataTypes.Select("Name = 'DateTime'")[0]["DataTypeId"];
            string selectExpression = string.Format("DataTypeId = '{0}' ", dataTypeId);
            DataRow[] rows = _dateTimeFormats.Table.Select(selectExpression);

            foreach (DataRow row in rows)
            {
                string expression = row["Expression"].ToString();
                if (expression != currenDateTimeFormat)
                {
                    cmbDateTimeFormat.Items.Add(expression);
                }
            }

            cmbDateTimeFormat.SelectedIndex = 0;
        }
		
		#endregion Private Methods

		#region Event Handlers

		private void SettingsPanelDateTime_Load(object sender, System.EventArgs e)
		{
			//rdbNormal.Tag = ((short)RecordProcessingScope.Undeleted).ToString();  whut that heck?
			//LoadMe();
		}

		private void cmbDateFormat_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.DateFormatChanged != null) 
			{
                this.DateFormatChanged(sender, e); 
			} 
		}

        private void cmbDateFormat_TextChanged(object sender, System.EventArgs e)
		{
            if (this.DateFormatTextChanged != null) 
			{
                this.DateFormatTextChanged(sender, e); 
			} 
		}

        private void cmbTimeFormat_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.TimeFormatChanged != null) 
			{
                this.TimeFormatChanged(sender, e); 
			} 
		}

        private void cmbTimeFormat_TextChanged(object sender, EventArgs e)
		{
            if (this.TimeFormatTextChanged != null)
			{
                this.TimeFormatTextChanged(sender, e);
			}
		}

		private void cmbDateTimeFormat_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            if (this.DateTimeFormatChanged != null) 
			{
                this.DateTimeFormatChanged(sender, e); 
			} 
		}

		private void cmbDateTimeFormat_TextChanged(object sender, EventArgs e)
		{
			if (this.DateTimeFormatTextChanged != null)
			{
				this.DateTimeFormatTextChanged(sender,e);
			}
		}
		
		#endregion Event Handlers
	}
}
