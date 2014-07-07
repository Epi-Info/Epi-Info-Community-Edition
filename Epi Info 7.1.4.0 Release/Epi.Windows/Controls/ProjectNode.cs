#region Namespaces

using System;
using Epi.Windows.Controls;
using Epi;

#endregion  //Namespaces

namespace Epi.Windows.Controls
{
	/// <summary>
	/// Tree node for Project.
	/// </summary>
	public class ProjectNode : TreeValueNode
    {
        #region Constructors

        /// <summary>
		/// Default constructor
		/// </summary>
		public ProjectNode() : base()
		{
			Initialize();
		}

        /// <summary>
        /// Constructor for Project Node
        /// </summary>
        /// <param name="project">The current project</param>
		public ProjectNode(Project project)
		{
			Initialize();
			this.Project = project;
			this.Text = project.Name;
			foreach (View view in project.Views)
			{
				Nodes.Add(new ViewNode(view));
			}
        }

        #endregion  //Constructors

        #region Public Methods

        /// <summary>
        /// Selects a view node
        /// </summary>
        /// <param name="index">Index of the view node</param>
        public void SelectViewNode(int index)
        {
            this.TreeView.SelectedNode = this.Nodes[index];
        }

        /// <summary>
        /// Inserts a view node
        /// </summary>
        /// <param name="view">The view in which node is to be added</param>
        /// <returns>The view node</returns>
        public ViewNode InsertViewNode(View view)
        {
            ViewNode node = new ViewNode(view);
            Nodes.Add(node);
            return node;
        }

        /// <summary>
		/// Disposes object
		/// </summary>
		public override void Dispose()
		{
			if (Project != null)
			{
				Project.Dispose();
				Project = null;
			}
			base.Dispose();
        }

        #endregion  //Public Methods

        #region Private Methods        
        
        /// <summary>
        /// Initialize the project node
        /// </summary>
        private void Initialize()
		{
			this.ImageIndex = 0;
			this.SelectedImageIndex = 0;
        }

        #endregion  //Private Methods

        #region Public Properties        
        
        /// <summary>
        /// Gets or set the project
        /// </summary>
		public Project Project
		{
			get
			{
				return (Project)this.Value;
			}
			set
			{
				this.Value = value;
			}
        }

        #endregion  //Public Properties
	}
}
