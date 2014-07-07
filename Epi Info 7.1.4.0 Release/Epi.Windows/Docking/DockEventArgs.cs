using System;
using System.Drawing;

namespace Epi.Windows.Docking
{
	/// <summary>
	/// DockEventArgs is used with DockEventHandler to report docking requests of a moving DockWindow to DockManagers.
	/// </summary>
	public class DockEventArgs : EventArgs
	{
		#region Variables
		private DockContainer target = null;
		private DockContainerType dockType;
		
		private Point point;
		
		private bool release;
		private bool handled = false;
		private bool showDockGuide = false;
		private bool ignoreHierarchy = false;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the point where to dock the window.
		/// </summary>
		public Point Point
		{
			get { return point; }
			set { point = value; }
		}

		/// <summary>
		/// Gets the needed target container type.
		/// </summary>
		public DockContainerType DockType
		{
			get { return dockType; }
		}

		/// <summary>
		/// Gets the state of the dock process. True, if dock is finally performed.
		/// </summary>
		public bool Release
		{
			get { return release; }
		}

		/// <summary>
		/// Gets or sets the handled state of this event to prevent the framework from docking multiple times.
		/// </summary>
		public bool Handled
		{
			get { return handled; }
			set { handled = value; }
		}

		/// <summary>
		/// Gets or sets the flag that keeps the DockGuide open.
		/// </summary>
		public bool ShowDockGuide
		{
			get { return showDockGuide; }
			set { showDockGuide = value; }
		}

		/// <summary>
		/// Gets or sets the flag that enables the possibility to dock into higher levels of the hierarchy.
		/// </summary>
		public bool IgnoreHierarchy
		{
			get { return ignoreHierarchy; }
			set { ignoreHierarchy = value; }
		}

		/// <summary>
		/// Gets or sets the target retrieved by one of the containers.
		/// </summary>
		public DockContainer Target
		{
			get { return target; }
			set { target = value; }
		}
		#endregion

		#region Construct
		/// <summary>
		/// Creates a new DockEventArgs object.
		/// </summary>
		/// <param name="point">The point where to dock the window.</param>
		/// <param name="dockType">The needed target container type.</param>
		/// <param name="release">The state of the dock process. True, if dock is finally performed.</param>
		public DockEventArgs(Point point, DockContainerType dockType, bool release)
		{
			this.point = point;
			this.dockType = dockType;
			this.release = release;
			this.target = null;
			this.handled = false;
			this.showDockGuide = false;
		}
		#endregion
	}

	/// <summary>
	/// The event handler for dock events.
	/// </summary>
	public delegate bool DockEventHandler(object sender, DockEventArgs e);
}
