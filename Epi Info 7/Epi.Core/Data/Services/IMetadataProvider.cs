using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Xml;

namespace Epi.Data.Services
{
    /// <summary>
    /// Metadata Provider interface class
    /// </summary>
    public interface IMetadataProvider
    {
        #region Events
        /// <summary>
        /// Progress Report Begin Event Handler
        /// </summary>
        event ProgressReportBeginEventHandler ProgressReportBeginEvent;
        /// <summary>
        /// Progress Report Update Event Handler
        /// </summary>
        event ProgressReportUpdateEventHandler ProgressReportUpdateEvent;
        /// <summary>
        /// Simple Event Handler
        /// </summary>
        event SimpleEventHandler ProgressReportEndEvent;
        #endregion Events

        /// <summary>
        /// Add Layer To Map
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="layerId"></param>
        void AddLayerToMap(int mapId, int layerId);
        

        /// <summary>
        /// Add Grid Column
        /// </summary>
        /// <param name="gridColumnRow"></param>
        void AddGridColumn(DataRow gridColumnRow);

        /// <summary>
        /// UpdateFonts
        /// </summary>
        /// <param name="controlFont"></param>
        /// <param name="promptFont"></param>
        /// <param name="viewId"></param>
        /// <param name="pageId"></param>
        void UpdateFonts(Font controlFont, Font promptFont, float viewId = -1, float pageId = -1);

        /// <summary>
        /// Attaches a db driver object to this provider.
        /// </summary>
        /// <param name="dbDriver"></param>
        void AttachDbDriver(IDbDriver dbDriver);

        /// <summary>
        /// Change the field type by only changing the FieldTypeId in metaFields
        /// </summary>
        /// <param name="field">Epi.Fields.Field to be changed</param>
        /// <param name="fieldType">the fields new MetaFieldType</param>
        void UpdateFieldType(Epi.Fields.Field field, Epi.MetaFieldType fieldType);

        /// <summary>
        /// Create Check Code Variable Definition
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <param name="checkCode">Code for field check.</param>
        void CreateCheckCodeVariableDefinition(int viewId, string checkCode);
        /// <summary>
        /// Create Code Table
        /// </summary>
        /// <param name="tableName">Name of table in database.</param>
        /// <param name="columnName">Name of column in table.</param>
        void CreateCodeTable(string tableName, string columnName);
        /// <summary>
        /// Create Code Table
        /// </summary>
        /// <param name="tableName">Name of table in database.</param>
        /// <param name="columnNames">List of column names in table.</param>
        /// <returns>True/False result of code table creation.</returns>
        bool CreateCodeTable(string tableName, string[] columnNames);
        /// <summary>
        /// Create Code Table Record
        /// </summary>
        /// <param name="tableName">Name of table in database.</param>
        /// <param name="columnNames">List of column names in table.</param>
        /// <param name="columnData"></param>
        void CreateCodeTableRecord(string tableName, string[] columnNames, string[] columnData);
        /// <summary>
        /// Create Control for a Field.
        /// </summary>
        /// <param name="fieldId">Id of field</param>
        /// <param name="topPosition">X-coordinate of control position.</param>
        /// <param name="leftPosition">Y-coordinate of control position.</param>
        /// <param name="height">Vertical length of control size.</param>
        /// <param name="width">Horizontal length of control size.</param>
        /// <param name="isRepeatLast">Repeat Last flag.</param>
        /// <param name="isRequired">Required field flag.</param>
        /// <param name="isReadOnly">Read only field flag.</param>
        /// <param name="isRetainImgSize">Retain image size flag.</param>
        /// <param name="tabOrder">Tab index order</param>
        /// <returns>New Control</returns>
        int CreateControl(int fieldId, double topPosition, double leftPosition, double height, double width, bool isRepeatLast, bool isRequired, bool isReadOnly, bool isRetainImgSize, int tabOrder);
        /// <summary>
        /// Create Control After Check Code
        /// </summary>
        /// <param name="fieldId">Id of field</param>
        /// <param name="checkCode">Code for field check.</param>
        /// <param name="view">Current view in Epi.Project.</param>
        void CreateControlAfterCheckCode(int fieldId, string checkCode, View view);
        /// <summary>
        /// Create Control Before Check Code
        /// </summary>
        /// <param name="fieldId">Id of field</param>
        /// <param name="checkCode">Code for field check.</param>
        /// <param name="view">Current view in Epi.Project.</param>
        void CreateControlBeforeCheckCode(int fieldId, string checkCode, View view);
        /// <summary>
        /// Create a new MultilineTextField field.
        /// </summary>
        /// <param name="field">MultilineTextField field object reference.</param>
        /// <returns>Returns a new MultilineTextField field.</returns>
        int CreateField(Epi.Fields.MultilineTextField field);
        /// <summary>
        /// Create a new NumberField field.
        /// </summary>
        /// <param name="field">NumberField field object reference.</param>
        /// <returns>Returns a new NumberField field.</returns>
        int CreateField(Epi.Fields.NumberField field);
        /// <summary>
        /// Create a new TimeField field.
        /// </summary>
        /// <param name="field">TimeField field object reference.</param>
        /// <returns>Returns a new TimeField field.</returns>
        int CreateField(Epi.Fields.TimeField field);
        /// <summary>
        /// Create a new UpperCaseTextField field.
        /// </summary>
        /// <param name="field">UpperCaseTextField field object reference.</param>
        /// <returns>Returns a new UpperCaseTextField field.</returns>
        int CreateField(Epi.Fields.UpperCaseTextField field);
        /// <summary>
        /// Create a new YesNoField field.
        /// </summary>
        /// <param name="field">YesNoField field object reference.</param>
        /// <returns>Returns a new YesNoField field.</returns>
        int CreateField(Epi.Fields.YesNoField field);
        /// <summary>
        /// Create a new SingleLineTextField field.
        /// </summary>
        /// <param name="field">SingleLineTextField field object reference.</param>
        /// <returns>Returns a new SingleLineTextField field.</returns>
        int CreateField(Epi.Fields.SingleLineTextField field);
        /// <summary>
        /// Create a new OptionField field.
        /// </summary>
        /// <param name="field">OptionField field object reference.</param>
        /// <returns>Returns a new OptionField field.</returns>
        int CreateField(Epi.Fields.OptionField field);
        /// <summary>
        /// Create a new PhoneNumberField field.
        /// </summary>
        /// <param name="field">PhoneNumberField field object reference.</param>
        /// <returns>Returns a new PhoneNumberField field.</returns>
        int CreateField(Epi.Fields.PhoneNumberField field);
        /// <summary>
        /// Create a new RelatedViewField field.
        /// </summary>
        /// <param name="field">RelatedViewField field object reference.</param>
        /// <returns>Returns a new RelatedViewField field.</returns>
        int CreateField(Epi.Fields.RelatedViewField field);
        /// <summary>
        /// Create a new MirrorField field.
        /// </summary>
        /// <param name="field">MirrorField field object reference.</param>
        /// <returns>Returns a new MirrorField field.</returns>
        int CreateField(Epi.Fields.MirrorField field);
        /// <summary>
        /// Create a new CommandButtonField field.
        /// </summary>
        /// <param name="field">CommandButtonField field object reference.</param>
        /// <returns>Returns a new CommandButtonField field.</returns>
        int CreateField(Epi.Fields.CommandButtonField field);
        /// <summary>
        /// Create a new DateField field.
        /// </summary>
        /// <param name="field">DateField field object reference.</param>
        /// <returns>Returns a new DateField field.</returns>
        int CreateField(Epi.Fields.DateField field);
        /// <summary>
        /// Create a new DateTimeField field.
        /// </summary>
        /// <param name="field">DateTimeField field object reference.</param>
        /// <returns>Returns a new DateTimeField field.</returns>
        int CreateField(Epi.Fields.DateTimeField field);
        /// <summary>
        /// Create a new UniqueKeyField field.
        /// </summary>
        /// <param name="field">UniqueKeyField field object reference.</param>
        /// <returns>Returns a new UniqueKeyField field.</returns>
        int CreateField(Epi.Fields.UniqueKeyField field);

        /// <summary>
        /// Create a new UniqueIdentifierField field.
        /// </summary>
        /// <param name="field">UniqueIdentifierField field object reference.</param>
        /// <returns>Returns a new UniqueIdentifierField field.</returns>
        int CreateField(Epi.Fields.UniqueIdentifierField field);

        /// <summary>
        /// Create a new GlobalRecordIdField field.
        /// </summary>
        /// <param name="field">GlobalRecordIdField field object reference.</param>
        /// <returns>Returns a new GlobalRecordIdField field.</returns>
        int CreateField(Epi.Fields.GlobalRecordIdField field);

        /// <summary>
        /// Create a new RecStatusField field.
        /// </summary>
        /// <param name="field">RecStatusField field object reference.</param>
        /// <returns>Returns a new RecStatusField field.</returns>
        int CreateField(Epi.Fields.RecStatusField field);
        //----------Id -123
        /// <summary>
        /// Create a new FirstsaveTime field.
        /// </summary>
        /// <param name="field">FirstSaveTime field object reference.</param>
        /// <returns>Returns a new FirstSaveTimeField field.</returns>
        int CreateField(Epi.Fields.FirstSaveTimeField field);
        /// <summary>
        /// Create a new LastsaveTime field.
        /// </summary>
        /// <param name="field">LastSaveTime field object reference.</param>
        /// <returns>Returns a new FirstSaveTimeField field.</returns>
        int CreateField(Epi.Fields.LastSaveTimeField field);
        //--------
        /// <summary>
        /// Create a new ForeignKeyField field.
        /// </summary>
        /// <param name="field">ForeignKeyField field object reference.</param>
        /// <returns>Returns a new ForeignKeyField field.</returns>
        int CreateField(Epi.Fields.ForeignKeyField field);
        /// <summary>
        /// Create a new CheckBoxField field.
        /// </summary>
        /// <param name="field">CheckBoxField field object reference.</param>
        /// <returns>Returns a new CheckBoxField field.</returns>
        int CreateField(Epi.Fields.CheckBoxField field);
        /// <summary>
        /// Create a new GridField field.
        /// </summary>
        /// <param name="field">GridField field object reference.</param>
        /// <returns>Returns a new GridField field.</returns>
        int CreateField(Epi.Fields.GridField field);
        /// <summary>
        /// Create a new GUIDField field.
        /// </summary>
        /// <param name="field">GUIDField field object reference.</param>
        /// <returns>Returns a new GUIDField field.</returns>
        int CreateField(Epi.Fields.GUIDField field);
        /// <summary>
        /// Create a new ImageField field.
        /// </summary>
        /// <param name="field">ImageField field object reference.</param>
        /// <returns>Returns a new ImageField field.</returns>
        int CreateField(Epi.Fields.ImageField field);        
        /// <summary>
        /// Create a new GroupFiled field.
        /// </summary>
        /// <param name="field">GroupFiled field object reference.</param>
        /// <returns>Returns a new GroupFiled field.</returns>
        int CreateField(Epi.Fields.GroupField field);
        /// <summary>
        /// Create a new LabelField field.
        /// </summary>
        /// <param name="field">LabelField field object reference.</param>
        /// <returns>Returns a new LabelField field.</returns>
        int CreateField(Epi.Fields.LabelField field);
        /// <summary>
        /// Create a new DDLFieldOfCodes field.
        /// </summary>
        /// <param name="field">DDLFieldOfCodes field object reference.</param>
        /// <returns>Returns a new DDLFieldOfCodes field.</returns>
        int CreateField(Epi.Fields.DDLFieldOfCodes field);
        /// <summary>
        /// Create a new DDLFieldOfCodes field.
        /// </summary>
        /// <param name="field">DDLFieldOfCodes field object reference.</param>
        /// <returns>Returns a new DDLFieldOfCodes field.</returns>
        int CreateField(Epi.Fields.DDListField field);
        /// <summary>
        /// Create a new DDLFieldOfCommentLegal field.
        /// </summary>
        /// <param name="field">DDLFieldOfCommentLegal field object reference.</param>
        /// <returns>Returns a new DDLFieldOfCommentLegal field.</returns>
        int CreateField(Epi.Fields.DDLFieldOfCommentLegal field);
        /// <summary>
        /// Create a new DDLFieldOfLegalValues field.
        /// </summary>
        /// <param name="field">DDLFieldOfLegalValues field object reference.</param>
        /// <returns>Returns a new DDLFieldOfLegalValues field.</returns>
        int CreateField(Epi.Fields.DDLFieldOfLegalValues field);
        /// <summary>
        /// Create DDLColumnOfCommentLegal Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">DDLColumnOfCommentLegal column object reference</param>
        /// <returns>New DDLColumnOfCommentLegal Grid Column</returns>
        int CreateGridColumn(Epi.Fields.DDLColumnOfCommentLegal column);
        /// <summary>
        /// Create DDLColumnOfLegalValues Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">DDLColumnOfLegalValues column object reference</param>
        /// <returns>New DDLColumnOfLegalValues Grid Column</returns>
        int CreateGridColumn(Epi.Fields.DDLColumnOfLegalValues column);
        /// <summary>
        /// Create GlobalRecordIdColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">GlobalRecordIdColumn column object reference</param>
        /// <returns>New GlobalRecordIdColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.GlobalRecordIdColumn column);
        /// <summary>
        /// Create UniqueRowIdColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">UniqueRowIdColumn column object reference</param>
        /// <returns>New UniqueRowIdColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.UniqueRowIdColumn column);
        /// <summary>
        /// Create UniqueKeyColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">UniqueKeyColumn column object reference</param>
        /// <returns>New UniqueKeyColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.UniqueKeyColumn column);
        /// <summary>
        /// Create RecStatusColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">RecStatusColumn column object reference</param>
        /// <returns>New RecStatusColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.RecStatusColumn column);
        /// <summary>
        /// Create ForeignKeyColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">ForeignKeyColumn column object reference</param>
        /// <returns>New ForeignKeyColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.ForeignKeyColumn column);
        /// <summary>
        /// Create NumberColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">NumberColumn column object reference</param>
        /// <returns>New NumberColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.NumberColumn column);
        /// <summary>
        /// Create TextColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">TextColumn column object reference</param>
        /// <returns>New TextColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.TextColumn column);
        /// <summary>
        /// Create CheckboxColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">CheckboxColumn column object reference</param>
        /// <returns>New CheckboxColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.CheckboxColumn column);
        /// <summary>
        /// Create YesNoColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">YesNoColumn column object reference</param>
        /// <returns>New YesNoColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.YesNoColumn column);
        /// <summary>
        /// Create PhoneNumberColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">PhoneNumberColumn column object reference</param>
        /// <returns>New PhoneNumberColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.PhoneNumberColumn column);
        /// <summary>
        /// Create DateColumn, TimeColumn, or DateTimeColumn Grid Column on a Grid Field.
        /// </summary>
        /// <param name="column">DateColumn column object reference</param>
        /// <returns>New DateColumn Grid Column</returns>
        int CreateGridColumn(Epi.Fields.ContiguousColumn column);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldId">Id of field</param>
        /// <param name="labelText"></param>
        /// <param name="topPosition">X-coordinate of control position.</param>
        /// <param name="leftPosition">Y-coordinate of control position.</param>
        /// <param name="font"></param>
        /// <param name="fontSize"></param>
        void CreateLabel(int fieldId, string labelText, double topPosition, double leftPosition, string font, decimal fontSize);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gml"></param>
        /// <param name="gmlSchema"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        int CreateLayer(string gml, string gmlSchema, string layerName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        int CreateLayer(string fileName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        int CreateMap(string name, string description);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view">Current view in Epi.Project.</param>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        Page CreatePage(View view, string name, int position);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <param name="checkCode">Code for field check.</param>
        /// <param name="view">Current view in Epi.Project.</param>
        void CreatePageAfterCheckCode(int pageId, string checkCode, View view);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <param name="checkCode">Code for field check.</param>
        /// <param name="view">Current view in Epi.Project.</param>
        void CreatePageBeforeCheckCode(int pageId, string checkCode, View view);
        /// <summary>
        /// Create Record After Check Code
        /// </summary>
        /// <param name="viewId">Id of current View.</param>
        /// <param name="checkCode">Field check code.</param>
        void CreateRecordAfterCheckCode(int viewId, string checkCode);
        /// <summary>
        /// Create Record Before Check Code
        /// </summary>
        /// <param name="viewId">Id of current View.</param>
        /// <param name="checkCode">Field check code.</param>
        void CreateRecordBeforeCheckCode(int viewId, string checkCode);
        /// <summary>
        /// Create View After Check Code
        /// </summary>
        /// <param name="viewId">Id of current View.</param>
        /// <param name="checkCode">Field check code.</param>
        void CreateViewAfterCheckCode(int viewId, string checkCode);
        /// <summary>
        /// Create View Before Check Code
        /// </summary>
        /// <param name="viewId">Id of current View.</param>
        /// <param name="checkCode">Field check code.</param>
        void CreateViewBeforeCheckCode(int viewId, string checkCode);
        /// <summary>
        /// DataBase Factory interface class reference
        /// </summary>
        IDbDriverFactory DBFactory { get; }
        /// <summary>
        /// DeleteCodeTable
        /// </summary>
        /// <param name="tableName"></param>
        void DeleteCodeTable(string tableName);
        /// <summary>
        /// Delete Field
        /// </summary>
        /// <param name="field"></param>
        void DeleteField(Epi.Fields.Field field);
        /// <summary>
        /// Delete Field
        /// </summary>
        /// <param name="field"></param>
        void DeleteField(string fieldName, int viewId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="column">Grid column of Grid field.</param>
        void DeleteGridColumn(Epi.Fields.GridColumnBase column);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        void DeleteFields(Page page);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        void DeletePage(Page page);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pgmId"></param>
        void DeletePgm(int pgmId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pgmName"></param>
        void DeletePgm(string pgmName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName">Name of current view in Epi.Project</param>
        void DeleteView(string viewName);

        /// <summary>
        /// Get the next available data table name. The table does not have to
        /// exist with this method is called. If the maximum number of columns
        /// for a table been hit, it will go to the next table name following 
        /// this pattern:
        /// exampleTableName, exampleTableNam2, exampleTableNam3, ...
        /// </summary>
        /// <param name="viewName">Name of the current Id of view in Epi.Project.</param>
        /// <returns></returns>
        string GetAvailDataTableName(string viewName);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewID">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetCheckCodeVariableDefinition(int viewID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName">Name of current view in Epi.Project</param>
        /// <returns></returns>
        System.Data.DataTable GetCheckCodeVariableDefinitions(string viewName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        View GetChildView(Epi.Fields.RelatedViewField field);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName">Name of table in database.</param>
        /// <returns></returns>
        System.Data.DataTable GetCodeTable(string tableName);
        //System.Data.DataTable GetCodeTableColumnSchema(string tableName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName">Name of table in database.</param>
        /// <returns></returns>
        System.Data.DataTable GetCodeTableData(string tableName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName">Name of table in database.</param>
        /// <param name="columnNames">List of column names in table.</param>
        /// <param name="sortCriteria"></param>
        /// <returns></returns>
        System.Data.DataTable GetCodeTableData(string tableName, string columnNames, string sortCriteria);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName">Name of table in database.</param>
        /// <param name="columnNames">List of column names in table.</param>
        /// <returns></returns>
        System.Data.DataTable GetCodeTableData(string tableName, string columnNames);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        DataSets.TableSchema.TablesDataTable GetCodeTableList();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        DataSets.TableSchema.TablesDataTable GetCodeTableList(IDbDriver db);
        //DataTable GetCodeTableList();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewTableName"></param>
        /// <param name="tableVariableName"></param>
        /// <returns></returns>
        System.Data.DataTable GetCodeTableName(string viewTableName, string tableVariableName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        DataTable GetCodeTableNamesForProject(Project project);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetControlsForPage(int pageId);

        System.Data.DataTable GetDataDictionary(View view);

        /// <summary>
        /// Get Data Table List 
        /// </summary>
        /// <returns></returns>
        List<string> GetDataTableList();
        
        /// <summary>
        /// Get Data Table Name
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        string GetDataTableName(int viewId);

        /// <summary>
        /// Returns the names of all data tables used by metadata for a given view.
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns>a DataTable containing all the data table names.</returns>
        DataTable GetDataTableNames(int viewId);
        
        /// <summary>
        /// Get Field Id By Name As Data Row
        /// </summary>
        /// <param name="viewName">Name of current view in Epi.Project</param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        System.Data.DataRow GetFieldIdByNameAsDataRow(string viewName, string fieldName);

        System.Data.DataRow GetFieldGUIDByNameAsDataRow(string viewName, string fieldName);

        /// <summary>
        /// Gets field metadata needed for synchronizing the view's data tables
        /// [ColumnNames.NAME],[ColumnNames.FIELD_TYPE_ID],[ColumnNames.DATA_TABLE_NAME]
        /// </summary>
        /// <param name="view">View</param>
        /// <returns>A DataTable containing [ColumnNames.NAME],[ColumnNames.FIELD_TYPE_ID],[ColumnNames.DATA_TABLE_NAME]</returns>
        DataTable GetFieldMetadataSync(View view);


        /// <summary>
        /// Gets field metadata needed for synchronizing the view's data tables
        /// [ColumnNames.NAME],[ColumnNames.FIELD_TYPE_ID],[ColumnNames.DATA_TABLE_NAME]
        /// </summary>
        /// <param name="view">View</param>
        /// <returns>A DataTable containing [ColumnNames.NAME],[ColumnNames.FIELD_TYPE_ID],[ColumnNames.DATA_TABLE_NAME]</returns>
        DataTable GetFieldMetadataSync(int pageId);

        /// <summary>
        /// Get Field As Data Row
        /// </summary>
        /// <param name="viewId"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        System.Data.DataRow GetFieldAsDataRow(int viewId, string fieldName);
        /// <summary>
        /// Get Field Check Code After
        /// </summary>
        /// <param name="fieldID">Id of field</param>
        /// <returns></returns>
        System.Data.DataTable GetFieldCheckCode_After(int fieldID);
        /// <summary>
        /// Get Field Check Code Before
        /// </summary>
        /// <param name="fieldID">Id of field</param>
        /// <returns></returns>
        System.Data.DataTable GetFieldCheckCode_Before(int fieldID);


        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.GUIDField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for MultilineTextField field.
        /// </summary>
        /// <param name="field">MultilineTextField field with data to get.</param>
        /// <param name="fieldNode">Xml representation of MultilineTextField field.</param>
        void GetFieldData(Epi.Fields.MultilineTextField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for NumberField field.
        /// </summary>
        /// <param name="field">NumberField field with data to get.</param>
        /// <param name="fieldNode">Xml representation of NumberField field.</param>
        void GetFieldData(Epi.Fields.NumberField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for TimeField field.
        /// </summary>
        /// <param name="field">TimeField field with data to get.</param>
        /// <param name="fieldNode">Xml representation of TimeField field.</param>
        void GetFieldData(Epi.Fields.TimeField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for UpperCaseTextField field.
        /// </summary>
        /// <param name="field">UpperCaseTextField field with data to get.</param>
        /// <param name="fieldNode">Xml representation of UpperCaseTextField field.</param>
        void GetFieldData(Epi.Fields.UpperCaseTextField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.YesNoField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.SingleLineTextField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.OptionField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.PhoneNumberField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.RelatedViewField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.MirrorField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.CommandButtonField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.DateField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.DateTimeField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.UniqueKeyField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for UniqueIdentifier Field.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="fieldNode"></param>
        void GetFieldData(Epi.Fields.UniqueIdentifierField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.RecStatusField field, XmlNode fieldNode);

        //--123
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.FirstSaveTimeField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.LastSaveTimeField field, XmlNode fieldNode);
        //----
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.ForeignKeyField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.CheckBoxField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.GridField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.ImageField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.LabelField field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.DDLFieldOfCodes field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.DDListField field, XmlNode fieldNode);

        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.DDLFieldOfCommentLegal field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Data for field.
        /// </summary>
        /// <param name="field">field with data to get.</param>
        /// <param name="fieldNode">Xml representation of field.</param>
        void GetFieldData(Epi.Fields.DDLFieldOfLegalValues field, XmlNode fieldNode);
        /// <summary>
        /// Get Field Tab Index
        /// </summary>
        /// <param name="fieldId">Id of field</param>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <returns></returns>
        int GetFieldTabIndex(int fieldId, int viewId, int pageId);
        /// <summary>
        /// Get all of the GroupFields on a given page as a NamedObjectCollection
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        /// <returns></returns>
        Epi.Collections.NamedObjectCollection<Epi.Fields.GroupField> GetGroupFields(Page page);

        /// <summary>
        /// Gets the number of collected fields currently defined in metaFields for a given view.
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns></returns>
        int GetCollectedFieldCount(int viewId);

        /// <summary>
        /// Get Fields from Field Collection Master
        /// </summary>
        /// <param name="view">Current view in Epi.Project.</param>
        /// <returns></returns>
        Epi.Collections.FieldCollectionMaster GetFields(View view);
        /// <summary>
        /// Get Fields As Data Table
        /// </summary>
        /// <param name="view">Current view in Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetFieldsAsDataTable(View view);
        /// <summary>
        /// Get Fields On Page As Data Table
        /// </summary>
        /// <param name="viewName">Name of current view in Epi.Project</param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        System.Data.DataTable GetFieldsOnPageAsDataTable(string viewName, int pageNumber);
        /// <summary>
        /// Get Fields On Page As Data Table
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetFieldsOnPageAsDataTable(int pageId);
        /// <summary>
        /// Get Field Types
        /// </summary>
        /// <returns></returns>
        System.Data.DataTable GetFieldTypes();
        /// <summary>
        /// Returns a field as a data row
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        DataRow GetFieldAsDataRow(Epi.Fields.Field field);
        /// <summary>
        /// Get Grid Column Collection
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        System.Collections.Generic.List<Epi.Fields.GridColumnBase> GetGridColumnCollection(Epi.Fields.GridField field);
        /// <summary>
        /// Get Grid Columns by field Id.
        /// </summary>
        /// <param name="fieldId">Id of field</param>
        /// <returns>Grid Columns</returns>
        System.Data.DataTable GetGridColumns(int fieldId);
        /// <summary>
        /// Get Grid Columns by table name.
        /// </summary>
        /// <param name="gridTableName">Name of grid table.</param>
        /// <returns>Grid Columns</returns>
        System.Data.DataTable GetGridColumns(string gridTableName);
        /// <summary>
        /// Get Grid Field Types
        /// </summary>
        /// <returns>Grid Field Types</returns>
        System.Data.DataTable GetGridFieldTypes();
        /// <summary>
        /// Get Groups For Page
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <returns>Groups For Page</returns>
        System.Data.DataTable GetGroupsForPage(int pageId);
        /// <summary>
        /// Get Latest View Inserted
        /// </summary>
        /// <returns></returns>
        View GetLatestViewInserted();
        /// <summary>
        /// Get Layer
        /// </summary>
        /// <param name="layerId"></param>
        /// <returns></returns>
        Epi.DataSets.MapMetadata.metaLayersRow GetLayer(int layerId);
        /// <summary>
        /// Get Layers
        /// </summary>
        /// <returns></returns>
        Epi.DataSets.MapMetadata.metaLayersDataTable GetLayers();
        /// <summary>
        /// GetMap
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        Epi.DataSets.MapMetadata.metaMapsDataTable GetMap(int mapId);
        /// <summary>
        /// GetMapLayer
        /// </summary>
        /// <param name="mapLayerId"></param>
        /// <returns></returns>
        Epi.DataSets.MapMetadata.metaMapLayersRow GetMapLayer(int mapLayerId);
        /// <summary>
        /// GetMapLayers
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        Epi.DataSets.MapMetadata.metaMapLayersDataTable GetMapLayers(int mapId);
        /// <summary>
        /// GetMapPoints
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        Epi.DataSets.MapMetadata.metaMapPointsDataTable GetMapPoints(int mapId);
        /// <summary>
        /// GetMaps
        /// </summary>
        /// <returns></returns>
        Epi.DataSets.MapMetadata.metaMapsDataTable GetMaps();
        /// <summary>
        /// GetMaxBackgroundId
        /// </summary>
        /// <returns></returns>
        int GetMaxBackgroundId();
        /// <summary>
        /// GetMaxFieldId
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        int GetMaxFieldId(int viewId);
        /// <summary>
        /// GetMaxGridColumnId
        /// </summary>
        /// <param name="gridFieldId"></param>
        /// <returns></returns>
        int GetMaxGridColumnId(int gridFieldId);
        /// <summary>
        /// GetMaxImageId
        /// </summary>
        /// <returns></returns>
        int GetMaxImageId();
        /// <summary>
        /// GetMaxPageId
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        int GetMaxPageId(int viewId);
        /// <summary>
        /// GetMaxTabIndex
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        double GetMaxTabIndex(int pageId, int viewId, bool? includeReadOnly = null);
        /// <summary>
        /// GetMaxViewId
        /// </summary>
        /// <returns></returns>
        int GetMaxViewId();
        /// <summary>
        /// GetMaxViewPagesPosition
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        int GetMaxViewPagesPosition(int viewId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewId"></param>
        /// <returns></returns>
        DataTable GetMetaFieldsSchema(int viewId);
        /// <summary>
        /// GetMinTabIndex
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        double GetMinTabIndex(int pageId, int viewId);
        /// <summary>
        /// GetNextTabIndex
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        /// <param name="view">Current view in Epi.Project.</param>
        /// <param name="currentTabIndex"></param>
        /// <returns></returns>
        double GetNextTabIndex(Page page, View view, double currentTabIndex);
        /// <summary>
        /// Get NonView Tables As Data Table
        /// </summary>
        /// <returns></returns>
        System.Data.DataTable GetNonViewTablesAsDataTable();
        /// <summary>
        /// Get Page Check Code After
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetPageCheckCode_After(Page page);
        /// <summary>
        /// Get Page Check Code Before
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetPageCheckCode_Before(Page page);
        /// <summary>
        /// Get Pages For View
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetPagesForView(int viewId);        
        /// <summary>
        /// Returns the parent view of a related view
        /// </summary>
        /// <param name="id">The ID of the related view</param>
        /// <returns>The View object of the parent view</returns>
        View GetParentView(int id);
        /// <summary>
        /// Get Patterns
        /// </summary>
        /// <returns></returns>
        System.Data.DataTable GetPatterns();
        /// <summary>
        /// Get Programs
        /// </summary>
        /// <returns></returns>
        System.Data.DataTable GetPgms();
        /// <summary>
        /// Get Record Check Code After
        /// </summary>
        /// <param name="viewID">Current Id of view in Epi.Project.</param>
        /// <returns>CheckCode_After</returns>
        System.Data.DataTable GetRecordCheckCode_After(int viewID);
        /// <summary>
        /// Get Record Check Code Before
        /// </summary>
        /// <param name="viewID">Current Id of view in Epi.Project.</param>
        /// <returns>CheckCode_Before</returns>
        System.Data.DataTable GetRecordCheckCode_Before(int viewID);
        /// <summary>
        /// Get Source Field Name
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="viewName">Name of current view in Epi.Project</param>
        /// <returns>SourceFieldName</returns>
        string GetSourceFieldName(string fieldName, string viewName);
        /// <summary>
        /// GetSystemFields
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetSystemFields(int viewId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns></returns>
        string GetTableColumnNames(int viewId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <returns></returns>
        DataSets.TabOrders.TabOrderDataTable GetTabOrderForFields(int pageId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <returns></returns>
        System.Data.DataTable GetTextFieldsForPage(int viewId, int pageId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId">pageId</param>
        /// <returns></returns>
        System.Data.DataTable GetCodeTargetCandidates(int pageId);
        /// <summary>
        /// Get View By Full Name
        /// </summary>
        /// <param name="viewFullName">Fully qualified name of current view in Epi.Project.</param>
        /// <returns>View By Full Name</returns>
        View GetViewByFullName(string viewFullName);
        /// <summary>
        /// Get View By Id
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns>View</returns>
        View GetViewById(int viewId);

        /// <summary>
        /// Get View By Id
        /// </summary>
        /// <param name="viewId">Current Id of view in Epi.Project.</param>
        /// <returns>View</returns>
        DataTable GetPublishedViewKeys(int viewId);
        /// <summary>
        /// Get View Check Code
        /// </summary>
        /// <param name="viewName">Name of current view in Epi.Project</param>
        /// <returns>View Check Code</returns>
        System.Data.DataTable GetViewCheckCode(string viewName);
        /// <summary>
        /// Get View Check Code After
        /// </summary>
        /// <param name="viewID">Current Id of view in Epi.Project.</param>
        /// <returns>View Check Code After</returns>
        System.Data.DataTable GetViewCheckCode_After(int viewID);
        /// <summary>
        /// Get View Check Code Before
        /// </summary>
        /// <param name="viewID">Current Id of view in Epi.Project.</param>
        /// <returns>View Check Code Before</returns>
        System.Data.DataTable GetViewCheckCode_Before(int viewID);
        /// <summary>
        /// Get View Pages
        /// </summary>
        /// <param name="view">Current view in Epi.Project.</param>
        /// <returns>View Pages</returns>
        System.Collections.Generic.List<Page> GetViewPages(View view);
        /// <summary>
        /// Get View Record Var Check Codes
        /// </summary>
        /// <param name="viewID">Current Id of view in Epi.Project.</param>
        /// <returns>View Record Var Check Codes</returns>
        System.Data.DataTable GetViewRecordVarCheckCodes(int viewID);
        /// <summary>
        /// Get View Relations
        /// </summary>
        /// <param name="viewTableName"></param>
        /// <returns>View Relations</returns>
        System.Data.DataTable GetViewRelations(string viewTableName);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Epi.Collections.ViewCollection GetViews();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentViewElement"></param>
        /// <param name="viewsNode"></param>
        /// <returns></returns>
        Epi.Collections.ViewCollection GetViews(XmlElement currentViewElement, XmlNode viewsNode);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        System.Data.DataTable GetViewsAsDataTable();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTableName"></param>
        /// <returns></returns>
        System.Collections.Generic.List<string> GetViewsOfDataTable(string dataTableName);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<string, int> DuplicateFieldNames();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string IdentifyDatabase();

        /// <summary>
        /// Constructor initialization.
        /// </summary>
        /// <param name="MetaDbInfo">Database driver information.</param>
        /// <param name="driver">Database driver name.</param>
        /// <param name="createDatabase">Create database flag.</param>
        void Initialize(DbDriverInfo MetaDbInfo, string driver, bool createDatabase);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName">Name of table in database.</param>
        /// <param name="columnNames">List of column names in table.</param>
        void InsertCodeTableData(System.Data.DataTable dataTable, string tableName, string[] columnNames);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        void InsertPage(Page page);

        void InsertFields(DataTable field);

        Int32 InsertMetaImage(string imagePath);

        Int32 InsertMetaImage(byte[] imageAsBytes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="comment"></param>
        /// <param name="author"></param>
        void InsertPgm(string name, string content, string comment, string author);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view">Current view in Epi.Project.</param>
        void InsertView(View view);
        /// <summary>
        /// 
        /// </summary>
        Project Project { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldId">Id of field</param>
        /// <param name="relatedViewId"></param>
        /// <param name="relateCondition"></param>
        /// <param name="shouldReturnToParent"></param>
        void RelateFieldToView(Guid uniqueId, int relatedViewId, string relateCondition, bool shouldReturnToParent);

        /// <summary>
        /// Removes all the fields in metaFields that are positioned off of the page.
        /// </summary>
        void RemovePageOutlierFields();
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId">Id of current page in current Epi.View of Epi.Project.</param>
        /// <param name="newPageName"></param>
        void RenamePage(int pageId, string newPageName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        void ResetTabIndexes(Page page);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName">Name of table in database.</param>
        /// <param name="columnName">Name of column in table.</param>
        void SaveCodeTableData(System.Data.DataTable dataTable, string tableName, string columnName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName">Name of table in database.</param>
        /// <param name="columnNames">List of column names in table.</param>
        void SaveCodeTableData(System.Data.DataTable dataTable, string tableName, string[] columnNames);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view">Current view in Epi.Project.</param>
        /// <param name="position"></param>
        void SynchronizePageNumbersOnDelete(View view, int position);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view">Current view in Epi.Project.</param>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        void SynchronizePageNumbersOnInsert(View view, Page page);


        /// <summary>
        /// Check if table exits.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>Returns true if the table exists.</returns>
        bool TableExists(string tableName);

        /// <summary>
        /// Update Codes Field Sources
        /// </summary>
        /// <param name="fieldId">Id of field</param>
        /// <param name="sourceFieldId"></param>
        /// <param name="codeColumnName"></param>
        /// <param name="sourceTableName"></param>
        void UpdateCodesFieldSources(int fieldId, int sourceFieldId, string codeColumnName, string sourceTableName);
        /// <summary>
        /// Update Control Position
        /// </summary>
        /// <param name="field"></param>
        void UpdateControlPosition(Epi.Fields.RenderableField field);
        /// <summary>
        /// Update Control Size
        /// </summary>
        /// <param name="field"></param>
        void UpdateControlSize(Epi.Fields.RenderableField field);
        /// <summary>
        /// Update Data Table Name
        /// </summary>
        /// <param name="viewId"></param>
        /// <param name="tableName"></param>
        void UpdateDataTableName(int viewId, string tableName);
        /// <summary>
        /// Update GridField field.
        /// </summary>
        /// <param name="field">GridField field to update.</param>
        void UpdateField(Epi.Fields.GridField field);
        /// <summary>
        /// Update GUIDField field.
        /// </summary>
        /// <param name="field">GUIDField field to update.</param>
        void UpdateField(Epi.Fields.GUIDField field);
        /// <summary>
        /// Update DDLFieldOfLegalValues field.
        /// </summary>
        /// <param name="field">DDLFieldOfLegalValues field to update.</param>
        void UpdateField(Epi.Fields.DDLFieldOfLegalValues field);
        /// <summary>
        /// Update ImageField field.
        /// </summary>
        /// <param name="field">ImageField field to update.</param>
        void UpdateField(Epi.Fields.ImageField field);
        /// <summary>
        /// Update DateField field.
        /// </summary>
        /// <param name="field">DateField field to update.</param>
        void UpdateField(Epi.Fields.DateField field);
        /// <summary>
        /// Update group field.
        /// </summary>
        /// <param name="field">GroupField field to update.</param>
        void UpdateField(Epi.Fields.GroupField field);
        /// <summary>
        /// Update LabelField field.
        /// </summary>
        /// <param name="field">LabelField field to update.</param>
        void UpdateField(Epi.Fields.LabelField field);
        /// <summary>
        /// Update DDLFieldOfCodes field.
        /// </summary>
        /// <param name="field">DDLFieldOfCodes field to update.</param>
        void UpdateField(Epi.Fields.DDLFieldOfCodes field);
        /// <summary>
        /// Update DDLFieldOfCodes field.
        /// </summary>
        /// <param name="field">DDLFieldOfCodes field to update.</param>
        void UpdateField(Epi.Fields.DDListField field);
        /// <summary>
        /// Update DateTimeField field.
        /// </summary>
        /// <param name="field">DateTimeField field to update.</param>
        void UpdateField(Epi.Fields.DateTimeField field);
        /// <summary>
        /// Update DDLFieldOfCommentLegal field.
        /// </summary>
        /// <param name="field">DDLFieldOfCommentLegal field to update.</param>
        void UpdateField(Epi.Fields.DDLFieldOfCommentLegal field);
        /// <summary>
        /// Update CommandButtonField field.
        /// </summary>
        /// <param name="field">CommandButtonField field to update.</param>
        void UpdateField(Epi.Fields.CommandButtonField field);
        /// <summary>
        /// Update CheckBoxField field.
        /// </summary>
        /// <param name="field">CheckBoxField field to update.</param>
        void UpdateField(Epi.Fields.CheckBoxField field);
        /// <summary>
        /// Update RelatedViewField field.
        /// </summary>
        /// <param name="field">RelatedViewField field to update.</param>
        void UpdateField(Epi.Fields.RelatedViewField field);
        /// <summary>
        /// Update PhoneNumberField field.
        /// </summary>
        /// <param name="field">PhoneNumberField field to update.</param>
        void UpdateField(Epi.Fields.PhoneNumberField field);
        /// <summary>
        /// Update TimeField field.
        /// </summary>
        /// <param name="field">TimeField field to update.</param>
        void UpdateField(Epi.Fields.TimeField field);
        /// <summary>
        /// Update SingleLineTextField field.
        /// </summary>
        /// <param name="field">SingleLineTextField field to update.</param>
        void UpdateField(Epi.Fields.SingleLineTextField field);
        /// <summary>
        /// Update MultilineTextField field.
        /// </summary>
        /// <param name="field">MultilineTextField field to update.</param>
        void UpdateField(Epi.Fields.MultilineTextField field);
        /// <summary>
        /// Update MirrorField field.
        /// </summary>
        /// <param name="field">MirrorField field to update.</param>
        void UpdateField(Epi.Fields.MirrorField field);
        /// <summary>
        /// Update OptionField field.
        /// </summary>
        /// <param name="field">OptionField field to update.</param>
        void UpdateField(Epi.Fields.OptionField field);
        /// <summary>
        /// Update NumberField field.
        /// </summary>
        /// <param name="field">NumberField field to update.</param>
        void UpdateField(Epi.Fields.NumberField field);
        /// <summary>
        /// Update YesNoField field.
        /// </summary>
        /// <param name="field">YesNoField field to update.</param>
        void UpdateField(Epi.Fields.YesNoField field);
        /// <summary>
        /// Update UpperCaseTextField field.
        /// </summary>
        /// <param name="field">UpperCaseTextField field to update.</param>
        void UpdateField(Epi.Fields.UpperCaseTextField field);
        /// <summary>
        /// Update DDLColumnOfCommentLegal Grid Column
        /// </summary>
        /// <param name="column">DDLColumnOfCommentLegal Grid column of Grid field.</param>
        void UpdateGridColumn(Epi.Fields.DDLColumnOfCommentLegal column);
        /// <summary>
        /// Update DDLColumnOfLegalValues Grid Column
        /// </summary>
        /// <param name="column">DDLColumnOfLegalValues Grid column of Grid field.</param>
        void UpdateGridColumn(Epi.Fields.DDLColumnOfLegalValues column);
        /// <summary>
        /// Update DateColumn Grid Column
        /// </summary>
        /// <param name="column">DateColumn Grid column of Grid field.</param>
        void UpdateGridColumn(Epi.Fields.ContiguousColumn column);
        /// <summary>
        /// Update TextColumn Grid Column
        /// </summary>
        /// <param name="column">TextColumn Grid column of Grid field.</param>
        void UpdateGridColumn(Epi.Fields.TextColumn column);
        /// <summary>
        /// Update CheckboxColumn Grid Column
        /// </summary>
        /// <param name="column">CheckboxColumn Grid column of Grid field.</param>
        void UpdateGridColumn(Epi.Fields.CheckboxColumn column);
        /// <summary>
        /// Update YesNoColumn Grid Column
        /// </summary>
        /// <param name="column">YesNoColumn Grid column of Grid field.</param>
        void UpdateGridColumn(Epi.Fields.YesNoColumn column);
        /// <summary>
        /// Update NumberColumn Grid Column
        /// </summary>
        /// <param name="column">NumberColumn Grid column of Grid field.</param>
        void UpdateGridColumn(Epi.Fields.NumberColumn column);
        /// <summary>
        /// Update PhoneNumberColumn Grid Column
        /// </summary>
        /// <param name="column">PhoneNumberColumn Grid column of Grid field.</param>
        void UpdateGridColumn(Epi.Fields.PhoneNumberColumn column);
        /// <summary>
        /// Update Page
        /// </summary>
        /// <param name="page">Current page in current Epi.View of Epi.Project.</param>
        void UpdatePage(Page page);
        /// <summary>
        /// Update Program by name.
        /// </summary>
        /// <param name="name">Pgm name.</param>
        /// <param name="content">Pgm code and logic.</param>
        /// <param name="comment">Notes describing the Pgm.</param>
        /// <param name="author">User that created the Pgm.</param>
        void UpdatePgm(string name, string content, string comment, string author);
        /// <summary>
        /// Update Program by Id.
        /// </summary>
        /// <param name="programId">Id of Pgm</param>
        /// <param name="name">Pgm name.</param>
        /// <param name="content">Pgm code and logic.</param>
        /// <param name="comment">Notes describing the Pgm.</param>
        /// <param name="author">User that created the Pgm.</param>
        void UpdatePgm(int programId, string name, string content, string comment, string author);
        /// <summary>
        /// Update Prompt Position
        /// </summary>
        /// <param name="field">Field that has a prompt separate from a control.</param>
        void UpdatePromptPosition(Epi.Fields.FieldWithSeparatePrompt field);
        /// <summary>
        /// Update Tab Order Index
        /// </summary>
        /// <param name="tabOrder">Tab index order.</param>
        /// <param name="view">Current view in Epi.Project.</param>
        /// <param name="currentPage">Current page in current Epi.View of Epi.Project.</param>
        void UpdateTabOrder(System.Data.DataTable tabOrder, View view, Page currentPage);
        /// <summary>
        /// Update View
        /// </summary>
        /// <param name="view">Current view in Epi.Project.</param>
        void UpdateView(View view);
        /// <summary>
        /// Extract Prompt Font
        /// </summary>
        /// <param name="fieldRow">Data Row of field with prompt font information.</param>
        /// <returns>Format for prompt text including font face, size, and style attributes.</returns>
        Font ExtractPromptFont(DataRow fieldRow);
        /// <summary>
        /// Get Background Data
        /// </summary>
        /// <returns>DataTable</returns>
        DataTable GetBackgroundData();
        /// <summary>
        /// Get Page Background Data
        /// </summary>
        /// <param name="page">Page</param>
        /// <returns>DataTable</returns>
        DataTable GetPageBackgroundData(Page page);
        /// <summary>
        /// Create Backgrounds Table
        /// </summary>
        void CreateBackgroundsTable();
        /// <summary>
        /// Create Image Table
        /// </summary>
        void CreateImagesTable();
        /// <summary>
        /// Insert Page Background Data
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="imagePath">Image File Path</param>
        /// <param name="imageLayout">Layout</param>
        /// <param name="color">Color</param>
        void InsertPageBackgroundData(Page page, int imageId, string imageLayout, int color);
        /// <summary>
        /// Update Page Background Data
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="imagePath">Image File Path</param>
        /// <param name="imageLayout">Layout</param>
        /// <param name="color">Color</param>
        void UpdatePageBackgroundData(Page page, string imagePath, string imageLayout, int color);
        /// <summary>
        /// Update Page Setup Data
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="width">Width of the page in pixels.</param>
        /// <param name="height">Height of the page in pixels.</param>
        /// <param name="orientation">Orientation: Protrait/Landscape</param>
        /// <param name="labelAlign">The label-field alignment.</param>
        /// <param name="targetMedium">The target viewing medium of the questionare: Paper/Monitor</param>
        void UpdatePageSetupData(View view, int width, int height, string orientation, string labelAlign, string targetMedium);
        /// <summary>
        /// Get Page Setup Data
        /// </summary>
        /// <returns>DataTable</returns>
        DataRow GetPageSetupData(View view);
    }
}
