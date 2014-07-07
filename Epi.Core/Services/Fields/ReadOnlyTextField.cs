using System;

namespace Epi.Fields
{
	/// <summary>
	/// Read Only Text Field.
	/// </summary>
	public abstract class ReadOnlyTextField : FieldWithSeparatePrompt
	{
		#region Private Members		
		// protected DragableTextBox textbox;
		#endregion Private Members

		#region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="page"><see cref="Epi.Page"/></param>
		public ReadOnlyTextField(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fieldRow">Read only text field's <see cref="System.Data.DataRow"/></param>
        /// <param name="view"><see cref="Epi.View"/></param>
		public ReadOnlyTextField(System.Data.DataRow fieldRow, View view) : base(view)
		{
		}

		#endregion Constructors

		#region Protected Properties

		#endregion Protected Properties

		#region Public Methods

		/// <summary>
		/// Deletes the field
		/// </summary>
		public override void Delete()
		{
            this.GetMetadata().DeleteField(this);
            view.MustRefreshFieldCollection = true;
		}
		#endregion

		#region Private Methods

		#endregion Protected Methods

	}
}
