using System;
using Epi;
using Epi.Fields;

using Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs;

namespace Epi.Windows.MakeView.PresentationLogic
{
    /// <summary>
    /// The Field Definition Dialog factory
    /// </summary>
	public class FieldDefinitionDialogFactory
    {
        #region Static Attributes
//        private static FieldDefinitionDialogFactory factory;
		private static Object classLock = typeof(FieldDefinitionDialogFactory);
        #endregion Static Attributes

        #region Instance Attributes
        private IServiceProvider serviceProvider;
        private Epi.Windows.MakeView.Forms.MakeViewMainForm mainForm = null;
        #endregion Instance Attributes

        private FieldDefinitionDialogFactory(IServiceProvider serviceProvider)
		{
            this.serviceProvider = serviceProvider;
            //this.mainForm = serviceProvider.GetService(typeof(MainForm)) as MainForm;
            //zack 8/10/2008 
            this.mainForm = serviceProvider.GetService(typeof(Epi.Windows.MakeView.Forms.MakeViewMainForm)) as Epi.Windows.MakeView.Forms.MakeViewMainForm;
        }

        /// <summary>
        /// Gets an instance of the Field Definition Dialog factory
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>An instance of Field Definition Dialog factory</returns>
        public static FieldDefinitionDialogFactory GetInstance(IServiceProvider serviceProvider)
        {
            return new FieldDefinitionDialogFactory(serviceProvider);
        }

        ///// <summary>
        ///// Gets an instance of the dialog factory
        ///// </summary>
        //public static FieldDefinitionDialogFactory Instance
        //{
        //    get
        //    {
        //        lock (classLock)
        //        {
        //            if (factory == null)
        //            {
        //                factory = new FieldDefinitionDialogFactory();
        //            }
        //            return factory;
        //        }
        //    }
        //}

		/// <summary>
		/// Gets a field definition dialog
		/// </summary>
		/// <param name="field">The field for which to get the dialog</param>
		/// <returns>A field definition</returns>
		public FieldDefinition GetFieldDefinitionDialog(Field field)
		{
			if (field is CheckBoxField)
			{
				return new CheckBoxFieldDefinition(mainForm, field as CheckBoxField);
			}
			else if (field is CommandButtonField)
			{
				return new CommandButtonFieldDefinition(mainForm, field as CommandButtonField);
			}
            else if (field is TimeField)
            {
                return new TimeFieldDefinition(mainForm, field as TimeField);
            }
			else if (field is DateField)
			{
				return new DateFieldDefinition(mainForm, field as DateField);
			}
			else if (field is DateTimeField)
			{
				return new DateTimeFieldDefinition(mainForm, field as DateTimeField);
			}
            else if (field is DDLFieldOfCodes)
            {
                return new CodesFieldDefinition(mainForm, field as DDLFieldOfCodes);
            }
            else if (field is DDListField)
            {
                return new ListFieldDefinition(mainForm, field as DDListField);
            }
            else if (field is DDLFieldOfCommentLegal)
            {
                return new CommentLegalFieldDefinition(mainForm, field as DDLFieldOfCommentLegal);
            }
            else if (field is GridField)
            {
                return new GridFieldDefinition(mainForm, field as GridField);
            }
            else if (field is GroupField)
            {
                return new GroupFieldDefinition(mainForm, field as GroupField);
            }
            else if (field is GUIDField)
            {
                return new GUIDFieldDefinition(mainForm, field as GUIDField);
            }  
            else if (field is DDLFieldOfLegalValues)
            {
                return new LegalValuesFieldDefinition(mainForm, field as DDLFieldOfLegalValues);
            }
            else if (field is LabelField)
            {
                return new LabelFieldDefinition(mainForm, field as LabelField);
            }
            else if (field is MirrorField)
            {
                return new MirrorFieldDefinition(mainForm, field as MirrorField);
            }
            else if (field is MultilineTextField)
            {
                return new MultilineTextFieldDefinition(mainForm, field as MultilineTextField);
            }
            else if (field is NumberField)
            {
                return new NumberFieldDefinition(mainForm, field as NumberField);
            }
            else if (field is OptionField)
            {
                return new OptionFieldDefinition(mainForm, field as OptionField);
            }
            else if (field is PhoneNumberField)
            {
                return new PhoneNumberFieldDefinition(mainForm, field as PhoneNumberField);
            }
            else if (field is RelatedViewField)
            {
                return new RelateFieldDefinition(mainForm, field as RelatedViewField);
            }
            else if (field is SingleLineTextField && !(field is UpperCaseTextField))
            {
                return new SingleLineTextFieldDefinition(mainForm, field as SingleLineTextField);
            }         
            else if (field is UpperCaseTextField)
            {
                return new UpperCaseTextFieldDefinition(mainForm, field as UpperCaseTextField);
            }
            else if (field is YesNoField)
            {
                return new YesNoFieldDefinition(mainForm, field as YesNoField);
            }
            else if (field is ImageField)
            {
                return new ImageFieldDefinition(mainForm, field as ImageField);
            }
            else
            {
                throw new ArgumentException("No dialog found for field.", field.ToString());
            }
        }

       

    }
}
