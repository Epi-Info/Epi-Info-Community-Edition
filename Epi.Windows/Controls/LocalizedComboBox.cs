namespace Epi.Windows.Controls
{
	/// <summary>
	/// A custom combobox that can be localized
	/// </summary>
	public class LocalizedComboBox : System.Windows.Forms.ComboBox, ITranslatable
	{

		#region Private Members

		private bool skipTranslation = false;

		#endregion

		#region ITranslatable Members

		/// <summary>
		/// Gets or sets whether or not to skip translation for the control
		/// </summary>
		public bool SkipTranslation
		{
			get
			{
				return skipTranslation;
			}
			set
			{
				skipTranslation = value;
			}
		}

		#endregion

	}
}
