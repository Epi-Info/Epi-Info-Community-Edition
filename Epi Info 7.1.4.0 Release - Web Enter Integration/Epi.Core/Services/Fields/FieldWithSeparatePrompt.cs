using System;
using System.Data;
using System.Drawing;
using Epi;

namespace Epi.Fields
{

    /// <summary>
    /// Base class for all fields (in a view) that have a prompt separate from the control
    /// </summary>
    public abstract class FieldWithSeparatePrompt : RenderableField
    {
        #region Private Class Members
        private double promptLeftPositionPercentage;
        private double promptTopPositionPercentage;
        private double promptHeightPositionPercentage;
        private double promptWidthPositionPercentage;
        #endregion Private Class Members

        #region Protected Class Members
        #endregion Protected Class Members

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="page"><see cref="Epi.Page"/></param>
        public FieldWithSeparatePrompt(Page page)
            : base(page)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="view"><see cref="Epi.View"/></param>
        public FieldWithSeparatePrompt(View view)
            : base(view)
        {
        }

        /// <summary>
        /// Load FieldWithSeparatePrompt from a <see cref="System.Data.DataRow"/>
        /// </summary>
        /// <param name="row">A row of data in a <see cref="System.Data.DataTable"/>.</param>
        public override void LoadFromRow(DataRow row)
        {
            base.LoadFromRow(row);
            if (row["PromptTopPositionPercentage"] is DBNull)
            {
                promptTopPositionPercentage = (double)(row["ControlTopPositionPercentage"]);
            }
            else
            {
                promptTopPositionPercentage = (double)(row["PromptTopPositionPercentage"]);
            }

            if (row["PromptLeftPositionPercentage"] is DBNull)
            {
                promptLeftPositionPercentage = (double)(row["ControlLeftPositionPercentage"]);
            }
            else
            {
                promptLeftPositionPercentage = (double)(row["PromptLeftPositionPercentage"]);
            }
        }

        public override void AssignMembers(Object field)
        {
            (field as FieldWithSeparatePrompt).promptTopPositionPercentage = this.promptTopPositionPercentage;
            (field as FieldWithSeparatePrompt).promptLeftPositionPercentage = this.promptLeftPositionPercentage;
            base.AssignMembers(field);
        }

        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets the left position percentage of a prompt.
        /// </summary>
        public double PromptLeftPositionPercentage
        {
            get
            {
                return (promptLeftPositionPercentage - (int)promptLeftPositionPercentage);
            }
            set
            {
                promptLeftPositionPercentage = value - (int)value;
            }
        }

        /// <summary>
        /// Gets/sets the top position percentage of a prompt.
        /// </summary>
        public double PromptTopPositionPercentage
        {
            get
            {
                return (promptTopPositionPercentage - (int)promptTopPositionPercentage);
            }
            set
            {
                promptTopPositionPercentage = value - (int)value;
            }
        }

        /// <summary>
        /// Gets/sets the top position percentage of a prompt.
        /// </summary>
        public double PromptWidthPositionPercentage
        {
            get
            {
                return (promptWidthPositionPercentage);
            }
            set
            {
                promptWidthPositionPercentage = value;
            }
        }
        
        /// <summary>
        /// Gets/sets the top position percentage of a prompt.
        /// </summary>
        public double PromptHeightPositionPercentage
        {
            get
            {
                return (promptHeightPositionPercentage);
            }
            set
            {
                promptHeightPositionPercentage = value;
            }
        }
        #endregion Public Properties

        #region Protected Properties


        #endregion Protected Properties

        #region Public Methods
        /// <summary>
        /// Updates the prompt position in a field's metadata.
        /// </summary>
        public void UpdatePromptPosition()
        {
            GetMetadata().UpdatePromptPosition(this);
        }

        #endregion Public Methods

        #region Protected Methods


        #endregion Protected Methods

        #region Private Methods


        #endregion Provate Methods

        #region Event Handlers

        #endregion Event Handlers
    }
}
