using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;

namespace Epi.ImportExport
{
    /// <summary>
    /// A class used to convert metadata from an external data source (e.g. Excel spreadsheet) into an Epi Info 7 form.
    /// </summary>
    /// <remarks>
    /// This class converts metadata only; it does not handle data importing or data conversions.
    /// </remarks>
    public class TableToFormMetadataConverter
    {
        #region Private Members
        /// <summary>
        /// The data driver for the data to be converted
        /// </summary>
        private IDbDriver sourceDriver;

        /// <summary>
        /// The data driver for the project
        /// </summary>
        private IDbDriver destinationDriver;

        /// <summary>
        /// The project that will contain the converted data
        /// </summary>
        private Project project;

        /// <summary>
        /// The name of the desired form
        /// </summary>
        private string formName;

        /// <summary>
        /// The name of the table in the source data
        /// </summary>
        private string tableName;

        /// <summary>
        /// The column mappings set up in the user interface.
        /// </summary>
        List<ColumnConversionInfo> columnMapping;

        /// <summary>
        /// Used for diagnostic purposes.
        /// </summary>
        private System.Diagnostics.Stopwatch stopwatch;
        #endregion // Private Members

        #region Events
        public event SetMaxProgressBarValueDelegate SetMaxProgressBarValue;
        public event SetProgressBarDelegate SetProgressBar;
        public event UpdateStatusEventHandler SetStatus;
        public event UpdateStatusEventHandler AddStatusMessage;
        public event CheckForCancellationHandler CheckForCancellation;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="pProject">The project that will contain the converted metadata</param>
        /// <param name="pFormName">The name of the form that will be created in the Epi Info 7 project</param>
        /// <param name="pSourceDriver">The data driver for the external data source</param>
        /// <param name="pTableName">The name of the table within the external data source</param>        
        /// <param name="pColumnMapping">The column mappings that determine how the fields in the form will be created</param>
        public TableToFormMetadataConverter(Project pProject, string pFormName, IDbDriver pSourceDriver, string pTableName, List<ColumnConversionInfo> pColumnMapping)
        {
            project = pProject;
            formName = pFormName;
            sourceDriver = pSourceDriver;
            tableName = pTableName;
            columnMapping = pColumnMapping;
            destinationDriver = project.CollectedData.GetDatabase();
        }
        #endregion // Constructors

        #region Private Methods
        /// <summary>
        /// Sets the appropriate properties for a given field, based on the specified column conversion information
        /// </summary>
        /// <param name="field">The Epi Info 7 field</param>
        /// <param name="cci">The column conversion information</param>
        private void SetFieldProperties(Field field, ColumnConversionInfo cci) 
        {
            if (cci.Prompt == null)
            {
                cci.Prompt = cci.DestinationColumnName;
            }
            switch (field.FieldType)
            {
                case MetaFieldType.Checkbox:
                    CheckBoxField checkboxField = (CheckBoxField)field;
                    checkboxField.TabIndex = cci.TabIndex;
                    checkboxField.IsReadOnly = cci.IsReadOnly;
                    checkboxField.IsRequired = cci.IsRequired;
                    checkboxField.ShouldRepeatLast = cci.IsRepeatLast;
                    break;
                case MetaFieldType.YesNo:
                    YesNoField yesNoField = (YesNoField)field;
                    yesNoField.TabIndex = cci.TabIndex;
                    yesNoField.IsReadOnly = cci.IsReadOnly;
                    yesNoField.IsRequired = cci.IsRequired;
                    yesNoField.ShouldRepeatLast = cci.IsRepeatLast;
                    break;
                case MetaFieldType.Text:
                    SingleLineTextField textField = (SingleLineTextField)field;
                    textField.TabIndex = cci.TabIndex;
                    textField.IsReadOnly = cci.IsReadOnly;
                    textField.IsRequired = cci.IsRequired;
                    textField.ShouldRepeatLast = cci.IsRepeatLast;
                    if (cci.UpperBound is int)
                    {
                        textField.MaxLength = (int)cci.UpperBound;
                    }
                    break;
                case MetaFieldType.Multiline:
                    MultilineTextField multilineTextField = (MultilineTextField)field;
                    multilineTextField.TabIndex = cci.TabIndex;
                    multilineTextField.IsReadOnly = cci.IsReadOnly;
                    multilineTextField.IsRequired = cci.IsRequired;
                    multilineTextField.ShouldRepeatLast = cci.IsRepeatLast;
                    break;
                case MetaFieldType.Date:
                    DateField dateField = (DateField)field;
                    dateField.TabIndex = cci.TabIndex;
                    dateField.IsReadOnly = cci.IsReadOnly;
                    dateField.IsRequired = cci.IsRequired;
                    dateField.ShouldRepeatLast = cci.IsRepeatLast;
                    break;
                case MetaFieldType.DateTime:
                    DateTimeField dateTimeField = (DateTimeField)field;
                    dateTimeField.TabIndex = cci.TabIndex;
                    dateTimeField.IsReadOnly = cci.IsReadOnly;
                    dateTimeField.IsRequired = cci.IsRequired;
                    dateTimeField.ShouldRepeatLast = cci.IsRepeatLast;
                    break;
                case MetaFieldType.Time:
                    TimeField timeField = (TimeField)field;
                    timeField.TabIndex = cci.TabIndex;
                    timeField.IsReadOnly = cci.IsReadOnly;
                    timeField.IsRequired = cci.IsRequired;
                    timeField.ShouldRepeatLast = cci.IsRepeatLast;
                    break;
                case MetaFieldType.Number:
                    NumberField numberField = (NumberField)field;
                    numberField.TabIndex = cci.TabIndex;
                    numberField.IsReadOnly = cci.IsReadOnly;
                    numberField.IsRequired = cci.IsRequired;
                    numberField.ShouldRepeatLast = cci.IsRepeatLast;
                    break;
                case MetaFieldType.LegalValues:
                    DDLFieldOfLegalValues legalValuesField = (DDLFieldOfLegalValues)field;
                    legalValuesField.TabIndex = cci.TabIndex;
                    legalValuesField.IsReadOnly = cci.IsReadOnly;
                    legalValuesField.IsRequired = cci.IsRequired;
                    legalValuesField.ShouldRepeatLast = cci.IsRepeatLast;

                    if (string.IsNullOrEmpty(cci.ListSourceTableName))
                    {
                        DataTable dt = new DataTable(cci.SourceColumnName);
                        dt.Columns.Add(new DataColumn(cci.SourceColumnName, typeof(string)));
                        // table is blank, so assume user wants to use a SELECT DISTINCT as the value source
                        Query selectDistinctQuery = sourceDriver.CreateQuery("SELECT DISTINCT [" + cci.SourceColumnName + "] FROM [" + tableName + "]");
                        IDataReader distinctReader = sourceDriver.ExecuteReader(selectDistinctQuery);
                        while (distinctReader.Read())
                        {
                            dt.Rows.Add(distinctReader[0].ToString());
                        }

                        cci.ListSourceTable = dt;
                        cci.ListSourceTableName = cci.SourceColumnName;
                        cci.ListSourceTextColumnName = cci.SourceColumnName;

                        IDbDriver db = project.CollectedData.GetDatabase();
                        if (!db.TableExists(cci.ListSourceTableName))
                        {
                            project.CreateCodeTable(cci.ListSourceTableName, cci.ListSourceTextColumnName);
                            project.SaveCodeTableData(cci.ListSourceTable, cci.ListSourceTableName, cci.ListSourceTextColumnName);
                        }

                        legalValuesField.SourceTableName = cci.ListSourceTableName;
                        legalValuesField.TextColumnName = cci.ListSourceTextColumnName;
                        legalValuesField.CodeColumnName = cci.ListSourceTextColumnName;
                    }
                    else
                    {
                        IDbDriver db = project.CollectedData.GetDatabase();
                        if (!db.TableExists(cci.ListSourceTableName))
                        {
                            project.CreateCodeTable(cci.ListSourceTableName, cci.ListSourceTextColumnName);
                            string[] columns = new string[1];
                            columns[0] = cci.ListSourceTextColumnName;
                            project.InsertCodeTableData(cci.ListSourceTable, cci.ListSourceTableName, columns);
                        }
                        legalValuesField.SourceTableName = cci.ListSourceTableName;
                        legalValuesField.TextColumnName = cci.ListSourceTextColumnName;
                        legalValuesField.CodeColumnName = cci.ListSourceTextColumnName;
                    }
                    break;
                default:
                    throw new ApplicationException("Invalid field type");
                    //break;
            }

            double ControlHeightPercentage = 0.0;
            double ControlWidthPercentage = 0.0;

            if (field is FieldWithSeparatePrompt)
            {
                FieldWithSeparatePrompt fieldWithPrompt;
                fieldWithPrompt = (FieldWithSeparatePrompt)field;
                fieldWithPrompt.PromptText = cci.Prompt;
                fieldWithPrompt.PromptFont = cci.PromptFont;
                fieldWithPrompt.ControlFont = cci.ControlFont;
                fieldWithPrompt.PromptLeftPositionPercentage = cci.ControlLeftPosition / 100;
                fieldWithPrompt.PromptTopPositionPercentage = cci.ControlTopPosition / 100;
                fieldWithPrompt.Name = cci.DestinationColumnName;
                fieldWithPrompt.ControlHeightPercentage = ControlHeightPercentage / 100;
                fieldWithPrompt.ControlWidthPercentage = ControlWidthPercentage / 100;
                fieldWithPrompt.ControlTopPositionPercentage = cci.ControlTopPosition / 100;
                fieldWithPrompt.ControlLeftPositionPercentage = (cci.ControlLeftPosition / 100) + 0.090702947845805;

                fieldWithPrompt.UpdatePromptPosition();
                fieldWithPrompt.UpdateControlPosition();
            }
            else
            {
                FieldWithoutSeparatePrompt fieldWithoutPrompt;
                fieldWithoutPrompt = (FieldWithoutSeparatePrompt)field;
                fieldWithoutPrompt.PromptText = cci.Prompt;
                fieldWithoutPrompt.PromptFont = cci.PromptFont;
                fieldWithoutPrompt.Name = cci.DestinationColumnName;

                fieldWithoutPrompt.ControlHeightPercentage = ControlHeightPercentage / 100;
                fieldWithoutPrompt.ControlWidthPercentage = ControlWidthPercentage / 100;
                fieldWithoutPrompt.ControlTopPositionPercentage = cci.ControlTopPosition / 100;
                fieldWithoutPrompt.ControlLeftPositionPercentage = (cci.ControlLeftPosition / 100) + 0.090702947845805;

                fieldWithoutPrompt.UpdateControlPosition();
            }
        }
        #endregion // Private Methods

        #region Public Methods
        /// <summary>
        /// Start the conversion process
        /// </summary>
        public void Convert()
        {
            if (SetStatus != null)
            {
                SetStatus("Creating form...");
            }

            project.CreateView(formName, false);
            View destinationView = project.Views[formName];

            List<int> pages = new List<int>();

            foreach(ColumnConversionInfo cci in columnMapping) 
            {
                if(!pages.Contains(cci.PageNumber)) 
                {
                    pages.Add(cci.PageNumber);
                }
            }

            pages.Sort();

            if (SetStatus != null)
            {
                SetStatus("Creating pages...");
            }

            for(int i = 0; i < pages.Count; i++) 
            {
                Page page = new Page(destinationView);
                page.Name = "Page " + (i + 1).ToString();
                page.Position = i;
                destinationView.Pages.Add(page);
                destinationView.SaveToDb();
                page.SaveToDb();
            }

            if (SetStatus != null)
            {
                SetStatus("Creating fields...");
            }

            foreach(ColumnConversionInfo cci in columnMapping) 
            {
                if (SetStatus != null)
                {
                    SetStatus("Creating field " + cci.DestinationColumnName + "...");
                }

                Page page = destinationView.GetPageByPosition(cci.PageNumber - 1);
                Field field = null;
                switch(cci.FieldType) 
                {
                    case MetaFieldType.Checkbox:
                    case MetaFieldType.YesNo:
                    case MetaFieldType.Text:
                    case MetaFieldType.Multiline:
                    case MetaFieldType.Date:
                    case MetaFieldType.DateTime:
                    case MetaFieldType.Time:
                    case MetaFieldType.LegalValues:
                    case MetaFieldType.Number:
                        field = page.CreateField(cci.FieldType);                        
                        break;
                    default:
                        throw new ApplicationException("Invalid field type");
                        //break;
                }

                SetFieldProperties(field, cci);

                page.AddNewField((RenderableField)field);                
            }
            
            destinationView.SaveToDb();
        }
        #endregion // Public Methods
    }
}
