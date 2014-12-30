using System;
using System.Collections;
using System.Collections.Generic;
using Epi.Collections;
using Epi.Fields;

namespace Epi.Collections
{
    /// <summary>
    /// Named Object Field Collection Master class
    /// </summary>
    public class FieldCollectionMaster : NamedObjectCollection<Field>
    {
        #region Private Class Members
        private NamedObjectCollection<TextField> textFields = null;
        private NamedObjectCollection<MirrorField> mirrorFields = null;
        private NamedObjectCollection<IInputField> inputFields = null;
        private NamedObjectCollection<IDataField> dataFields = null;
        private NamedObjectCollection<IDataField> tableColumnFields = null;
        private NamedObjectCollection<RelatedViewField> relatedFields = null;
        private NamedObjectCollection<GridField> gridFields = null;
        #endregion Private Class Members

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldCollectionMaster()
        {
            textFields = new NamedObjectCollection<TextField>();
            mirrorFields = new NamedObjectCollection<MirrorField>();
            inputFields = new NamedObjectCollection<IInputField>();
            dataFields = new NamedObjectCollection<IDataField>();
            tableColumnFields = new NamedObjectCollection<IDataField>();
            relatedFields = new NamedObjectCollection<RelatedViewField>();
            gridFields = new NamedObjectCollection<GridField>();
        }

        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Returns the Unique Key Field.
        /// </summary>
        public UniqueKeyField UniqueKeyField
        {
            get
            {
                return this[ColumnNames.UNIQUE_KEY] as UniqueKeyField;
            }
        }


        /// <summary>
        /// Returns the Unique Identifier Field.
        /// </summary>
        public UniqueIdentifierField UniqueIdentifierField
        {
            get
            {
                return this[ColumnNames.UNIQUE_IDENTIFIER] as UniqueIdentifierField;
            }
        }
        /// <summary>
        /// Returns the Record Status Field.
        /// </summary>
        public RecStatusField RecStatusField
        {
            get
            {
                return this[ColumnNames.REC_STATUS] as RecStatusField;
            }
        }
        //-----123
        /// <summary>
        /// Returns the FirstSaveTime Field.
        /// </summary>
        public FirstSaveTimeField FirstSaveTimeField
        {
            get
            {
                return this[ColumnNames.RECORD_FIRST_SAVE_TIME] as FirstSaveTimeField;
            }
        }
        
        /// <summary>
        /// Returns the LastSaveTime Field.
        /// </summary>
        public LastSaveTimeField LastSaveTimeField
        {
            get
            {
                return this[ColumnNames.RECORD_LAST_SAVE_TIME] as LastSaveTimeField;
            }
        }
         //-------
        /// <summary>
        /// Returns the Foreign Key Field.
        /// </summary>
        public ForeignKeyField ForeignKeyField
        {
            get
            {
                return this[ColumnNames.FOREIGN_KEY] as ForeignKeyField;
            }
        }

        public bool ForeignKeyFieldExists
        {
            get
            {
                return this.Contains(ColumnNames.FOREIGN_KEY);
            }
        }

        /// <summary>
        /// Returns the Foreign Key Field.
        /// </summary>
        public GlobalRecordIdField GlobalRecordIdField
        {
            get
            {
                return this[ColumnNames.GLOBAL_RECORD_ID] as GlobalRecordIdField;
            }
        }
        /// <summary>
        /// Returns the named collection of Text Fields.
        /// </summary>
        public NamedObjectCollection<TextField> TextFields
        {
            get
            {
                return this.textFields;
            }
        }

        /// <summary>
        /// Returns the named collection of Mirror Fields.
        /// </summary>
        public NamedObjectCollection<MirrorField> MirrorFields
        {
            get
            {
                return this.mirrorFields;
            }
        }

        /// <summary>
        /// Returns the named collection of IInput Fields.
        /// </summary>
        public NamedObjectCollection<IInputField> InputFields
        {
            get
            {
                return this.inputFields;
            }
        }

        /// <summary>
        /// Returns the named collection of IDataFields.
        /// </summary>
        public NamedObjectCollection<IDataField> DataFields
        {
            get
            {
                return dataFields;
            }
        }

        /// <summary>
        /// Returns the named collection of Table column data fields.
        /// </summary>
        public NamedObjectCollection<IDataField> TableColumnFields
        {
            get
            {
                return this.tableColumnFields;
            }
        }

        /// <summary>
        /// Returns the named collection of Related View Fields
        /// </summary>
        public NamedObjectCollection<RelatedViewField> RelatedFields
        {
            get
            {
                return this.relatedFields;
            }
        }

        /// <summary>
        /// Returns the named collection of Grid Fields
        /// </summary>
        public NamedObjectCollection<GridField> GridFields
        {
            get
            {
                return this.gridFields;
            }
        }
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Add a new field collection master.
        /// </summary>
        /// <param name="field">Field object to add.</param>
        public override void Add(Field field)
        {
            base.Add(field);
            if (field is TextField)
            {
                textFields.Add(field as TextField);
            }
            if (field is MirrorField)
            {
                mirrorFields.Add(field as MirrorField);
            }
            if (field is RelatedViewField)
            {
                relatedFields.Add(field as RelatedViewField);
            }
            if (field is IDataField)
            {
                dataFields.Add(field as IDataField);
                // Only IDataFields make it into table columns with the exception of MirrorFields
                if (!(field is MirrorField))
                {
                    tableColumnFields.Add(field as IDataField);
                }
            }
            if (field is IInputField)
            {
                inputFields.Add(field as IInputField);
            }
            if (field is GridField)
            {
                gridFields.Add(field as GridField);
            }
        }

        /// <summary>
        /// Deletes a field from the master field collection.
        /// </summary>
        /// <param name="field">Field object to delete.</param>
        /// <returns></returns>
        public override bool Remove(Field field)
        {
            bool result = base.Remove(field);
            if (field is TextField)
            {
                textFields.Remove(field.Name);
            }
            if (field is MirrorField)
            {
                mirrorFields.Remove(field.Name);
            } 
            if (field is RelatedViewField)
            {
                relatedFields.Add(field as RelatedViewField);
            }
            if (field is IDataField)
            {
                dataFields.Remove(field.Name);
                // Only IDataFields make it into table columns with the exception of MirrorFields
                if (!(field is MirrorField))
                {
                    tableColumnFields.Remove(field.Name);
                }
            }
            if (field is IInputField)
            {
                inputFields.Remove(field.Name);
            }
            if (field is GridField)
            {
                gridFields.Remove(field as GridField);
            }
            return result;
        }

        /// <summary>
        /// Dispose of the field collections.
        /// </summary>
        public override void Dispose()
        {
            if (this.dataFields != null)
            {
                dataFields.Dispose();
                dataFields = null;
            }
            if (this.inputFields != null)
            {
                inputFields.Dispose();
                inputFields = null;
            }
            if (this.mirrorFields != null)
            {
                mirrorFields.Dispose();
                mirrorFields = null;
            }
            if (this.tableColumnFields != null)
            {
                tableColumnFields.Dispose();
                tableColumnFields = null;
            }
            if (this.textFields != null)
            {
                textFields.Dispose();
                textFields = null;
            }
            if (this.relatedFields != null)
            {
                relatedFields.Dispose();
                relatedFields = null;
            }
            if (this.gridFields != null)
            {
                gridFields.Dispose();
                gridFields = null;
            }
            base.Dispose();
        }
        #endregion Public Methods
    }
}