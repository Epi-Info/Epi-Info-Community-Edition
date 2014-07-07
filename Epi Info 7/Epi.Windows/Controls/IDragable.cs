namespace Epi.Windows.Controls
{
	/// <summary>
	/// To be implemented by all controls that can be dragged.
	/// </summary>
	public interface IDragable
	{

		#region Properties

		/// <summary>
		/// Gets or sets the horizontal distance of the mouse from the edge of the control
		/// </summary>
		int XOffset
		{
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the vertical distance of the mouse from the edge of the control
		/// </summary>
		int YOffset
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets whether or not the dynamic control has moved
		/// </summary>
		bool HasMoved
		{
			get;
			set;
		}

		#endregion

	}
}
