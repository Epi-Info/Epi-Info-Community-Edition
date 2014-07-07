#region Namespaces

using System;
using System.Windows.Forms;

#endregion

namespace Epi.Windows.Controls
{

	#region Class Definition

	/// <summary>
	/// An extended TreeNode that can contain a hidden value.
	/// </summary>
	public class TreeValueNode : System.Windows.Forms.TreeNode, IDisposable 
	{

		#region Private Members
		private object nodeValue;
		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public TreeValueNode()
		{
		}

        /// <summary>
        /// Disposes all components
        /// </summary>
		public virtual void Dispose()
		{
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// The hidden value of the tree node
		/// </summary>
		public object Value
		{
			get
			{
				return nodeValue;
			}
			set
			{
				nodeValue = value;
			}
		}

		#endregion
	}

	#endregion

	#region Enum Definition

	/// <summary>
	/// Specifies the type of MakeView object a tree node represents
	/// </summary>
	public enum TreeNodeType
	{
		/// <summary>
		/// The node represents a project
		/// </summary>
		Project,
		/// <summary>
		/// The node represents a view
		/// </summary>
		View,
		/// <summary>
		/// The node represents a page
		/// </summary>
		Page,
		/// <summary>
		/// The node represents a field in Linked Mode
		/// </summary>
		LinkedModeField,
		/// <summary>
		/// The node represents a generic Epi Info field
		/// </summary>
		EpiInfoField

	}

	#endregion

}
