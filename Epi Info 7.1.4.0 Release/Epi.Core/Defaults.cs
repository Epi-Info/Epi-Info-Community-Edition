using System.Drawing;
using Epi.Resources;

namespace Epi
{
    /// <summary>
    /// Defaults used in Epi Info
    /// </summary>
	public static class Defaults
	{		

		#region Public Properties

		/// <summary>
		/// Default BackgroundImage file
		/// </summary>
		public static Image BackgroundImageFile
		{
			get
			{
                return ResourceLoader.GetImage("BG.jpg");
			}
		}

		/// <summary>
		/// The default LANGUAGE is English
		/// </summary>
		public const string LANGUAGE = "en-US";
        /// <summary>
        /// Show debug tracing
        /// </summary>
        public const bool SHOW_TRACE = false;
		
		/// <summary>
		///  Returns the default variable scope.
		/// </summary>
		public static VariableType VariableScope
		{
			get
			{
				return VariableType.Standard;
				// return AppData.Instance.GetDefaultVariableScope();
			}
		}

        /// <summary>
        /// returns Font Family Name
        /// </summary>
		public static string FontFamilyName 
		{
			get
			{
				return FontFamily.GenericSansSerif.Name;
			}
		}

        /// <summary>
        /// Returns Font Size
        /// </summary>
		public static float FontSize
		{
			get
			{
				return 8.5f;
			}
		}

        /// <summary>
        /// Returns Font Style
        /// </summary>
		public static FontStyle FontStyle
		{
			get
			{
				return FontStyle.Regular;
			}
		}

        /// <summary>
        /// Returns font
        /// </summary>
		public static Font Font
		{
			get
			{
				return new Font(FontFamilyName, FontSize, FontStyle);
			}
		}

        /// <summary>
        /// If this is set, all new pages will be given the current background image.
        /// Else, no background image will be added to the new page.
        /// </summary>
        public static bool UseBackgroundOnAllPages
        {
            get
            {
                return (bool) false;
            }
        }        
        #endregion Public Properties

	}
}