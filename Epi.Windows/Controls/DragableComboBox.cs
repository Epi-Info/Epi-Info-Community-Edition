#region Namespaces

using System;
using System.Windows.Forms;

#endregion

namespace Epi.Windows.Controls
{

	/// <summary>
	/// A resizable combo box to be used in MakeView's questionnaire designer
	/// </summary>
	public class DragableComboBox : System.Windows.Forms.ComboBox, IDragable, IFieldControl 
	{

		#region Private Members

		private bool isMouseDown = false;
		private int x;
		private int y;
		private bool hasMoved = false;
		private int fieldId;
		private Epi.Fields.Field field;
        private ControlTracker controlTracker;
        private Enums.TrackerStatus trackerStatus;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public DragableComboBox()
		{
			InitializeComponent();
		}

		#endregion

		#region Override Methods

		/// <summary>
		/// Overrides the combo box's OnPaint event
		/// </summary>
		/// <param name="e">Parameters for the paint event</param>
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes the textbox
		/// </summary>
		private void InitializeComponent()
		{
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragableComboBox_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DragableComboBox_MouseMove);
			this.MouseLeave += new System.EventHandler(this.DragableComboBox_MouseLeave);
			this.DragOver += new DragEventHandler(DragableComboBox_DragOver);
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the mouse-move event over the combo box
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableComboBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (isMouseDown)
			{
				DataObject data = new DataObject("DragControl",this);
				this.DoDragDrop(data,DragDropEffects.Move);
				isMouseDown = false;
				this.hasMoved = true;
			}
		}

		/// <summary>
		/// Handles the mouse-down event of the combo box
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableComboBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.X < this.Width - 20)
			{
				isMouseDown = true;
				x = e.X;
				y = e.Y;
			}
		}

		/// <summary>
		/// Handles the mouse-leave event of the combo box
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableComboBox_MouseLeave(object sender, System.EventArgs e)
		{
			isMouseDown = false;
		}

		/// <summary>
		/// Handles the drag-over event of the combo box
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableComboBox_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the horizontal distance of the mouse from the edge of the combo box
		/// </summary>
		public int XOffset
		{
			get
			{
				return this.x;
			}
			set
			{
				this.x = value;
			}
		}

		/// <summary>
		/// Gets or sets the vertical distance of the mouse from the edge of the combo box
		/// </summary>
		public int YOffset
		{
			get
			{
				return this.y;
			}
			set
			{
				this.y = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not the dynamic control has moved
		/// </summary>
		public bool HasMoved
		{
			get
			{
				return hasMoved;
			}
			set
			{
				hasMoved = value;
			}
		}

		/// <summary>
		/// Gets or sets the ID of the MakeView field referenced by the control
		/// </summary>
		public int FieldId
		{
			get
			{
				return fieldId;
			}
			set
			{
				fieldId = value;
			}
		}

		/// <summary>
		/// Gets and sets the field this control is associated with.
		/// </summary>
		public Epi.Fields.Field Field
		{
			get
			{
				return field;
			}
			set
			{
				field = value;
			}
		}

		#endregion

        #region IFieldControl Members

        /// <summary>
        /// IFieldControl implementation
        /// </summary>
        public bool IsFieldGroup
        {
            get
            {
                return false;
            }
            set
            {
                // do nothing
            }
        }

        /// <summary>
        /// IFieldControl implementation
        /// </summary>
        public Epi.Fields.GroupField GroupField
        {
            get
            {
                return null;
            }
            set
            {
                // do nothing
            }
        }
        
        public ControlTracker Tracker
        {
            get { return controlTracker; }
            set { controlTracker = value; }
        }

        public Enums.TrackerStatus TrackerStatus
        {
            get { return trackerStatus; }
            set
            {
                controlTracker.TrackerStatus = value;
                this.trackerStatus = value;
            }
        }

        #endregion

	}
}