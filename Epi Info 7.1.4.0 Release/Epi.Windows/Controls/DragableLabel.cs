#region Namespaces

using System;
using System.Windows.Forms;

#endregion

namespace Epi.Windows.Controls
{
	/// <summary>
	/// A dragable label to be used in MakeView's questionnaire designer
	/// </summary>
	public class DragableLabel : PairedLabel, IDragable, IFieldControl
	{
		#region Private Members

		private int x;
		private int y;
        private bool hasMoved = false;

        private int fieldId;
		private int horizontalDistanceToControl;
		private int verticalDistanceToControl;
		private bool isMouseDown = false;
		private Epi.Fields.Field field;
        private ControlTracker controlTracker;
        private Enums.TrackerStatus trackerStatus;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public DragableLabel()
		{
			InitializeComponent();
		}

		#endregion

		#region Override Methods

		/// <summary>
		/// Overrides the label's OnPaint event
		/// </summary>
		/// <param name="e">Parameters for the paint event</param>
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
            if (Text == string.Empty)
            {
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, System.Drawing.Color.LightGray, ButtonBorderStyle.Solid);
            }
            else
            { 
                base.OnPaint (e);
            }
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes the dragable label
		/// </summary>
		private void InitializeComponent()
		{
			base.DragOver += new System.Windows.Forms.DragEventHandler(this.DragableLabel_DragOver);
			base.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DragableLabel_MouseMove);
			base.MouseLeave += new System.EventHandler(this.DragableLabel_MouseLeave);
			base.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragableLabel_MouseDown);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the horizontal distance of the mouse from the edge of the label
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
		/// Gets or sets the vertical distance of the mouse from the edge of the control
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


        /// <summary>
        /// Gets and sets the ControlTracker this control is associated with.
        /// </summary>
        public ControlTracker ControlTracker
        {
            get
            {
                return controlTracker;
            }
            set
            {
                controlTracker = value;
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
		/// Gets the horizontal distance from the label to the control
		/// </summary>
		public int HorizontalDistanceToControl
		{
			get
			{
				return this.horizontalDistanceToControl;
			}
		}

		/// <summary>
		/// Gets the vertical distance from the label to the control
		/// </summary>
		public int VerticalDistanceToControl
		{
			get
			{
				return this.verticalDistanceToControl;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the mouse-down event of the label
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableLabel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			isMouseDown = true;
            x = e.X;
            y = e.Y;

            if (this.LabelFor != null)
			{
				horizontalDistanceToControl = this.LabelFor.Left - this.Left;
				verticalDistanceToControl = this.LabelFor.Top - this.Top;
			}            
		}

		/// <summary>
		/// Handles the mouse-move event over the label
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (isMouseDown)
			{
				DataObject data = new DataObject("DragControl",this);
				this.DoDragDrop(data, DragDropEffects.Move);
				isMouseDown = false;
				this.hasMoved = true;
			}
		}

		/// <summary>
		/// Handles the mouse-leave event of the label
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableLabel_MouseLeave(object sender, System.EventArgs e)
		{
			isMouseDown = false;
		}

		/// <summary>
		/// Handles the drag-over event of the label
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableLabel_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
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
