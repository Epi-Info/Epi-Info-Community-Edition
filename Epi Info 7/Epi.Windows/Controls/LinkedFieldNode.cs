#region Namespaces
using System;
#endregion  //Namespaces

namespace Epi.Windows.Controls
{
    /// <summary>
    /// The field node of linked fields
    /// </summary>
	public class LinkedFieldNode : FieldNode
    {
        #region Constructors

        /// <summary>
        /// The default constructor
        /// </summary>
        public LinkedFieldNode() : base()
		{
        }

        #endregion  //Constructors
    }

    /// <summary>
    /// The tree value node of linked fields
    /// </summary>
    public class LinkedRootNode : TreeValueNode
    {
        #region Constructors

        /// <summary>
        /// The Default constructor
        /// </summary>
        public LinkedRootNode()
            : base()
        {
        }
        #endregion //Constructors
    }
}
