using Epi.Fields;

namespace Epi.Windows.Controls
{
	/// <summary>
	/// To be implemented by all controls that are rendered in MakeView and Enter
	/// </summary>
	public interface IFieldControl
	{
		#region Properties

		/// <summary>
		/// Gets and sets the ID of the MakeView field referenced by the control
		/// </summary>
		int FieldId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets the field this control is associated with.
		/// </summary>
		Field Field
		{
			get;
			set;
		}
        
        /// <summary>
        /// Gets and sets the ControlTracker this control is associated with.
        /// </summary>
        ControlTracker Tracker
        {
            get;
            set;
        }

        Enums.TrackerStatus TrackerStatus
        {
            get;
            set;
        }

		#endregion Properties
	}
}