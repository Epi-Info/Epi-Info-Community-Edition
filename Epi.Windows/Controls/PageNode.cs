#region //Namespaces

using System;
using Epi.Windows.Controls;
using Epi;

#endregion  //Namespaces

namespace Epi.Windows.Controls
{
	/// <summary>
	/// Tree node for page.
	/// </summary>
	public class PageNode : TreeValueNode
    {
        #region Constructors       

        /// <summary>
		/// Default constructor
		/// </summary>
		public PageNode() : base()
		{
			Initialize();
		}

        /// <summary>
        /// Constructor for Page Node
        /// </summary>
        /// <param name="page">The current page</param>
		public PageNode(Page page)
		{
			Initialize();
			this.Page = page;
			this.Text = page.Name;
        }

        #endregion  //Constructors

        #region Private Methods
        
        /// <summary>
        /// Initializes the Page Node
        /// </summary>
		private void Initialize()
		{
			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
            
        }

        #endregion  //Private Methods

        #region Public Properties
        
        /// <summary>
		/// Gets or sets the Page object associated with this node
		/// </summary>
		public Page Page
		{
			get
			{
				return (Page)this.Value;
			}
			set
			{
				this.Value = value;
			}
        }

        #endregion  //Public Properties
    }
}
