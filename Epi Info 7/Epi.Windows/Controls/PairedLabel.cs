#region Namespaces

using System;
using System.Windows.Forms;

#endregion

namespace Epi.Windows.Controls
{
	/// <summary>
	/// A label that can be paired with another control
	/// </summary>
	public class PairedLabel : TransparentLabel 
	{
	
		#region Private Members

		private Control pairedControl;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public PairedLabel()
		{
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets and sets the control that is paired with the label
		/// </summary>
		public Control LabelFor
		{
			get
			{
				return pairedControl;
			}
			set
			{
				pairedControl = value;
			}
		}

		#endregion

		#region Override Methods

		/// <summary>
		/// Overrides the label's OnPaint event
		/// </summary>
		/// <param name="e">Parameters for the paint event</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		#endregion

	}
}
