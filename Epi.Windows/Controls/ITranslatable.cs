namespace Epi.Windows.Controls
{
	/// <summary>
	/// Defines required members for all custom translatable controls
	/// </summary>
	public interface ITranslatable
	{

		/// <summary>
		/// Gets or sets whether or not to skip translation for the control
		/// </summary>
		bool SkipTranslation
		{
			get;
			set;
		}
	}
}
