using System;
using Epi.Windows.Controls;
using Epi;

namespace Epi.Windows.Controls
{
	/// <summary>
	/// Tree node for view.
	/// </summary>
	public class ViewNode : TreeValueNode
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ViewNode() : base()
		{
			Initialize();
		}

        /// <summary>
        /// Constructor for ViewNode
        /// </summary>
        /// <param name="view">The current view</param>
		public ViewNode(View view)
		{
			Initialize();
			this.View = view;            
			this.Text = view.Name;
			foreach (Page page in view.Pages)
			{
				this.Nodes.Add(new PageNode(page));
			}
		}

		private void Initialize()
		{
			this.ImageIndex = 1;
			this.SelectedImageIndex = 1;
		}

		/// <summary>
		/// Selects a page node
		/// </summary>
		/// <param name="index">Index of the page node</param>
		public void SelectPageNode(int index)
		{
			this.TreeView.SelectedNode = this.Nodes[index];
		}

		/// <summary>
		/// Gets or sets the View object associated with this node
		/// </summary>
		public View View
		{
			get
			{
				return (View)this.Value;
			}
			set
			{
				this.Value = value;
			}
		}

        /// <summary>
        /// Inserts a page node for a given page
        /// </summary>
        /// <param name="page">Page in which node is to be inserted</param>
        /// <param name="index">Index of insertion</param>
        /// <returns></returns>
		public PageNode InsertPageNode(Page page, int index)
		{
            PageNode node = new PageNode(page);
            if (View.Pages.Count == 0)
            {
                Nodes.Add(node);
            }
            else
            {
                if (index == View.Pages.Count)
                {
                    Nodes.Add(node);
                }
                else
                {
                    Nodes.Insert(index, node);
                }
            }
            return node;
		}

	}
}
