#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using Epi.Fields;
using Epi.Windows;

#endregion

namespace Epi.Windows.Controls
{
    /// <summary>
    /// A dragable group box to be used in MakeView's questionnaire designer
    /// </summary>
    public class DragableGroupBox : FieldGroupBox, IDragable, IFieldControl
    {
        #region Private Members
        private bool isMouseDown = false;
        private int x;
        private int y;
        private bool hasMoved = false;
        private Epi.Fields.Field field;
        private ControlTracker controlTracker;
        private Enums.TrackerStatus trackerStatus;
        private List<Control> childControls = new List<Control>();
        private Dictionary<Control, int> verticalDistances;
        private Dictionary<Control, int> horizontalDistances;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for the class
        /// </summary>
        public DragableGroupBox()
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
            base.OnPaint(e);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the Dragable button
        /// </summary>
        private void InitializeComponent()
        {
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragableGroupBox_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DragableGroupBox_MouseMove);
            this.MouseLeave += new System.EventHandler(this.DragableGroupBox_MouseLeave);
            base.DragOver   += new DragEventHandler(DragableGroupBox_DragOver);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the mouse-move event over the group box
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void DragableGroupBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < Font.Height) // This was split to avoid expensive MeasureText calls.
            {
                Size size = TextRenderer.MeasureText(Text, this.Font);

                if (e.X < size.Width)
                {
                    base.Cursor = Cursors.SizeAll;

                    if (isMouseDown)
                    {
                        DataObject data = new DataObject("DragControl", this);
                        this.DoDragDrop(data, DragDropEffects.Move);
                        isMouseDown = false;
                        this.hasMoved = true;
                    }
                }
                else
                {
					base.Cursor = Cursors.Arrow;
				}
			}
            else
            {
				base.Cursor = Cursors.Arrow;
			}
		}
      

        /// <summary>
        /// Handles the mouse-down event of the group box
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void DragableGroupBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (this.ChildControls != null))
            {
                isMouseDown = true;
                x = e.X;
                y = e.Y;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentPanel"></param>
        public void CaptureDistances(Panel currentPanel)
        {
            verticalDistances = new Dictionary<Control, int>();
            horizontalDistances = new Dictionary<Control, int>();
            foreach (Control control in GetChildControls(currentPanel))
            {
                if (!verticalDistances.ContainsKey(control))
                {
                    verticalDistances.Add(control, control.Top - this.Top);
                }
                if (!horizontalDistances.ContainsKey(control))
                {
                    horizontalDistances.Add(control, control.Left - this.Left);
                }
            }
        }

        /// <summary>
        /// Gets the vertical distance to a hosted control
        /// </summary>
        /// <param name="control">The hosted control</param>
        /// <returns>Distance</returns>
        public int GetVerticalDistanceTo(Control control)
        {
            return verticalDistances[control];
        }

        /// <summary>
        /// Gets the horizontal distance to a hosted control
        /// </summary>
        /// <param name="control">The hosted control</param>
        /// <returns>Distance</returns>
        public int GetHorizontalDistanceTo(Control control)
        {
            return horizontalDistances[control];
        }

        /// <summary>
        /// Handles the mouse-leave event of the group box
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void DragableGroupBox_MouseLeave(object sender, System.EventArgs e)
        {
            isMouseDown = false;
        }

        /// <summary>
        /// Handles the drag-over event of the group box
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void DragableGroupBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets all the controls being hosted by the group box
        /// </summary>
        public List<Control> ChildControls
        {
            get { return childControls; }
        }

        /// <summary>
        /// Gets all the controls being hosted by the group box
        /// </summary>
        private List<Control> GetChildControls(Panel currentPanel)
        {
            List<Control> controls = new List<Control>();
            
            if (this.field != null && this.field is OptionField)
            {
                return controls;
            }
            else if (this.field != null)
            {
                String[] names = ((GroupField)this.Field).ChildFieldNames.Split(new char[] { ',' });

                foreach (Control pageControl in currentPanel.Controls)
                {
                    IFieldControl fieldControl = pageControl as IFieldControl;

                    if (fieldControl != null )
                    {
                        foreach (String name in names)
                        {
                            if (fieldControl.Field.Name == name)
                            {
                                controls.Add(pageControl);
                            }
                        }
                    }
                }
            }
            childControls = controls;
            return controls;
        }

        /// <summary>
        /// Gets or sets the horizontal distance of the mouse from the edge of the group box
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
        /// Gets or sets the vertical distance of the mouse from the edge of the group box
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

        #endregion

        #region Public Methods

        private void CheckLabelPosition(List<Control> controls, Control control, int bottom, int right)
        {
            //if label is in the group box, then add it
            DragableLabel label = control as DragableLabel;
            if (label.Top >= this.Top && (label.Top + label.Height) <= bottom
                && label.Left >= this.Left && (label.Left + label.Width) <= right)
            {
                label.BringToFront();

                if (label.LabelFor != null)
                {
                    label.LabelFor.BringToFront();
                }
            }
            //if the label is not in the group box, then remove it's linked control
            else
            {
                control.SendToBack();
                controls.Remove(control);

                if (label.LabelFor != null && controls.Contains(label.LabelFor))
                {
                    label.LabelFor.SendToBack();
                    controls.Remove(label.LabelFor);
                    controls.Remove(label);
                }
            }
        }
        #endregion

        #region IFieldControl Members

        /// <summary>
        /// IFieldControl implementation
        /// </summary>
        public int FieldId
        {
            get
            {
                return 0;
            }
            set
            {
                // do nothing
            }
        }

        /// <summary>
        /// IFieldControl implementation
        /// </summary>
        public Epi.Fields.Field Field
        {
            get
            {
                return this.field;
            }
            set
            {
                this.field = value;
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
        /// IFieldControl implementation
        /// </summary>
        public bool IsFieldGroup
        {
            get
            {
                return (this.Field == null);
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