using System;
using System.Data;

namespace Epi.Fields
{
    /// <summary>
    /// Field without Separate Prompt Field
    /// </summary>
	public abstract class FieldWithoutSeparatePrompt : RenderableField
	{
		#region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="page"><see cref="Epi.Page"/></param>
		public FieldWithoutSeparatePrompt(Page page) : base(page)
		{
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="view"><see cref="Epi.View"/></param>
		public FieldWithoutSeparatePrompt(View view) : base(view)
		{
		}

        public void AssignMembers(Field field)
        {
            base.AssignMembers(field);
        }

		#endregion Constructors

		#region Protected Methods


		#endregion Protected Methods

		#region Event Handlers

		#endregion Event Handlers

	}
}
