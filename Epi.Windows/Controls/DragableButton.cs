#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

#endregion

namespace Epi.Windows.Controls
{
	/// <summary>
	/// A dragable and Dragable button to be used in MakeView's questionnaire designer
	/// </summary>
	public class DragableButton : System.Windows.Forms.Button, IDragable, IFieldControl 
	{

		#region Private Members

		private bool isMouseDown = false;
		private int x;
		private int y;
		private int fieldId;
		private bool hasMoved = false;
		private Epi.Fields.Field field;
        private ControlTracker controlTracker;
        private Enums.TrackerStatus trackerStatus;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for the class
		/// </summary>
		public DragableButton()
		{
			InitializeComponent();
		}

		#endregion

		#region Override Methods

		/// <summary>
		/// Overrides the button's OnPaint event
		/// </summary>
		/// <param name="e">Parameters for the paint event</param>
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes the Dragable button
		/// </summary>
		private void InitializeComponent()
		{
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragableButton_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DragableButton_MouseMove);
			this.MouseLeave += new System.EventHandler(this.DragableButton_MouseLeave);
			base.DragOver += new DragEventHandler(DragableButton_DragOver);
		}


		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the mouse-move event over the button
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableButton_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
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
		/// Handles the mouse-down event of the button
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableButton_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			isMouseDown = true;
			x = e.X;
			y = e.Y;
		}

		/// <summary>
		/// Handles the mouse-leave event of the button
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableButton_MouseLeave(object sender, System.EventArgs e)
		{
			isMouseDown = false;
		}

		/// <summary>
		/// Handles the drag-over event of the button
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void DragableButton_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the horizontal distance of the mouse from the edge of the button
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
		/// Gets or sets the vertical distance of the mouse from the edge of the button
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
