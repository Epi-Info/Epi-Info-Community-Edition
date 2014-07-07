#region Namespaces
using System;
using Epi;
#endregion

namespace Epi.Windows.Enter.Controls
{
    /// <summary>
    /// The Yes No combo box
    /// </summary>
	public class YesNoComboBox: Epi.Windows.Controls.LocalizedComboBox
	{
		#region Constructors
		/// <summary>
		/// Constructor for the class
		/// </summary>
		public YesNoComboBox()
		{
			//Add the 'Represenation of Yes', the 'Representation of No', 
			//and the 'Representation of Missing' to the combobox
            Configuration config = Configuration.GetNewInstance();
            this.Items.Add(config.Settings.RepresentationOfYes);
            this.Items.Add(config.Settings.RepresentationOfNo);
            this.Items.Add(config.Settings.RepresentationOfMissing);
		}
		#endregion
	}
}
