#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

#endregion

namespace Epi.Windows.Controls
{
	/// <summary>
	/// The rectangle that appears around resizable controls.
	/// </summary>
	public class ControlTracker : System.Windows.Forms.UserControl
	{
		#region Private Members
		private System.ComponentModel.Container components = null;
		private Rectangle baseRect;
		private Rectangle ControlRect;
		private Rectangle[] SmallRect = new Rectangle[8];
        private Rectangle FenceRect = new Rectangle();
		private Size Square = new Size(6,6);
		private Graphics g;
		private Control currentControl;
		private Point prevLeftClick;
		private bool isFirst = true;
        private bool isResizeSelect;
		private ResizeBorder CurrBorder;
        private Enums.TrackerStatus trackerStatus;
		#endregion

		#region Private Enums

		/// <summary>
		/// Keep track of which border of the box is to be resized.
		/// </summary>
		private enum ResizeBorder
		{
			None = 0,
			Top = 1,
			Right = 2,
			Bottom = 3,
			Left = 4,
			TopLeft = 5,
			TopRight = 6,
			BottomLeft = 7,
			BottomRight = 8
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the control has been resized.
		/// </summary>
		public event EventHandler Resized;

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}

            this.MouseUp -= new System.Windows.Forms.MouseEventHandler(this.ControlTracker_MouseUp);
            this.Paint -= new System.Windows.Forms.PaintEventHandler(this.ControlTracker_Paint);
            this.MouseMove -= new System.Windows.Forms.MouseEventHandler(this.ControlTracker_MouseMove);
			base.Dispose( disposing );
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// ControlTracker
			// 
            this.Name = "ControlTracker";
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ControlTracker_MouseUp);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ControlTracker_Paint);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ControlTracker_MouseMove);
		}
		#endregion

		#region Constructors

        public ControlTracker(Control theControl, bool isResizeSelect)
        {
            InitializeComponent();
            this.isResizeSelect = isResizeSelect;
            currentControl = theControl;
            WrapControl();
        }

		/// <summary>
		/// Constructor
		/// </summary>
		public ControlTracker()
		{			
			InitializeComponent();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// The Control the tracker is used on 
		/// </summary>
		public Control Control
		{
			get 
			{
				return currentControl;
			}
		}

		/// <summary>
		/// The current position of the rectangle in client coordinates (pixels).
		/// </summary>
		public Rectangle Rect
		{
			get 
			{
				return baseRect;
			}
			set 
			{
				int X = Square.Width;
				int Y = Square.Height;
				int Height = value.Height;
				int Width = value.Width;
				baseRect = new Rectangle(X,Y,Width,Height);
				SetRectangles();
			}
		}

        public Enums.TrackerStatus TrackerStatus
        {
            get { return trackerStatus; }
            set
            {
                trackerStatus = value;

                switch (trackerStatus)
                {
                    case Enums.TrackerStatus.NotSelected:
                        Hide();
                        Refresh();
                        break;
                    case Enums.TrackerStatus.Selected:
                        IsResizeSelect = false;
                        break;
                    case Enums.TrackerStatus.Resize:
                        IsResizeSelect = true;
                        break;
                }
            }
        }

        private bool IsResizeSelect
        {
            get { return isResizeSelect; }
            set 
            {
                if (value != isResizeSelect)
                {
                    isResizeSelect = value;
                    WrapControl();
                }

                Show();
            }
        }


		#endregion

		#region Public Methods

        /// <summary>
        /// Draws Squares
        /// </summary>
        public void Draw()
		{
            Pen pen = new Pen(SystemColors.HotTrack);
            //pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            if (g == null)
            {
                //create graphics
                g = this.CreateGraphics();
                FenceRect.Size = this.Size;
            }

            g.DrawRectangle(pen, FenceRect);

            if (isResizeSelect)
            {
                g.FillRectangles(Brushes.White, SmallRect);
                g.DrawRectangles(Pens.Black, SmallRect);
            }
		}

		/// <summary>
		/// Check point position to see if it's on tracker
		/// </summary>
		/// <param name="point">Current Point Position</param>
		/// <returns></returns>
		public bool IsPointInTracker(Point point)
		{
			//Check if the point is somewhere on the tracker
			if (!ControlRect.Contains(point))
			{
				//should never happen
				Cursor.Current = Cursors.Arrow;
				
				return false;
			}
			else if(SmallRect[0].Contains(point))
			{
				Cursor.Current = Cursors.SizeNWSE;
				CurrBorder = ResizeBorder.TopLeft;
			}
			else if(SmallRect[3].Contains(point))
			{
				Cursor.Current = Cursors.SizeNWSE;
				CurrBorder = ResizeBorder.BottomRight;
			}
			else if(SmallRect[1].Contains(point))
			{
				Cursor.Current = Cursors.SizeNESW;
				CurrBorder = ResizeBorder.TopRight;
			}
			else if(SmallRect[2].Contains(point))
			{
				Cursor.Current = Cursors.SizeNESW;
				CurrBorder = ResizeBorder.BottomLeft;
			}
			else if(SmallRect[4].Contains(point))
			{
				Cursor.Current = Cursors.SizeNS;
				CurrBorder = ResizeBorder.Top;
			}
			else if(SmallRect[5].Contains(point))
			{
				Cursor.Current = Cursors.SizeNS;
				CurrBorder = ResizeBorder.Bottom;
			}
			else if(SmallRect[6].Contains(point))
			{
				Cursor.Current = Cursors.SizeWE;
				CurrBorder = ResizeBorder.Left;
			}
			else if(SmallRect[7].Contains(point))
			{
				Cursor.Current = Cursors.SizeWE;
				CurrBorder = ResizeBorder.Right;
			}
			else if(ControlRect.Contains(point))
			{
				Cursor.Current = Cursors.Default;
				CurrBorder = ResizeBorder.None;
			}
						
			return true;
		}

		/// <summary>
		/// Check point position to see if it's on tracker
		/// </summary>
		/// <param name="x">X position</param>
		/// <param name="y">Y position</param>
		/// <returns>bool</returns>
		public bool IsPointInTracker(int x, int y)
		{
			return IsPointInTracker(new Point(x,y));
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Set the position of the control tracker's "handlebars"
		/// </summary>
		private void SetRectangles()
		{
            if (isResizeSelect)
            {
                // do the cast once up top to avoid doing it (potentially) several times below
                IFieldControl control = ((IFieldControl)currentControl);
                
                int xLeft = baseRect.X - Square.Width;
                int yTop = baseRect.Y - Square.Height;
                int xRight = baseRect.X + baseRect.Width + 1;
                int yBottom = baseRect.Y + baseRect.Height + 1;
                int xMiddle = baseRect.X + (baseRect.Width / 2) - (Square.Width / 2);
                int yMiddle = baseRect.Y + (baseRect.Height / 2) - (Square.Height / 2);

                if (control.Field is Epi.Fields.MultilineTextField
                   || control.Field is Epi.Fields.ImageField
                   || control.Field is Epi.Fields.GridField
                   || control.Field is Epi.Fields.GroupField
                   || control.Field is Epi.Fields.RelatedViewField
                   || control.Field is Epi.Fields.CommandButtonField
                   || control.Field is Epi.Fields.LabelField)
                {
                    //TopLeft
                    SmallRect[0] = new Rectangle(new Point(xLeft, yTop), Square);
                    //TopRight
                    SmallRect[1] = new Rectangle(new Point(xRight, yTop), Square);
                    //BottomLeft
                    SmallRect[2] = new Rectangle(new Point(xLeft, yBottom), Square);
                    //BottomRight
                    SmallRect[3] = new Rectangle(new Point(xRight, yBottom), Square);
                    //TopMiddle
                    SmallRect[4] = new Rectangle(new Point(xMiddle, yTop), Square);
                    //BottomMiddle
                    SmallRect[5] = new Rectangle(new Point(xMiddle, yBottom), Square);
                }

                //LeftMiddle
                SmallRect[6] = new Rectangle(new Point(xLeft, yMiddle), Square);
                //RightMiddle
                SmallRect[7] = new Rectangle(new Point(xRight, yMiddle), Square);
            }
            
            int side = Square.Width;
            int width = currentControl.Width + side + 1;
            int height = currentControl.Height + side + 1;

            FenceRect = new Rectangle(side / 2, side / 2, width, height);
			ControlRect = new Rectangle(new Point(0,0),this.Bounds.Size);
		}

		public void WrapControl()
		{
            if (currentControl != null)
            {
                Square = isResizeSelect ? new Size(6,6):  new Size(0,0);
                
                int X = currentControl.Bounds.X - Square.Width-1;
                int Y = currentControl.Bounds.Y - Square.Height-1;
                int Height = currentControl.Bounds.Height + (Square.Height * 2) + 133;
                int Width = currentControl.Bounds.Width + (Square.Width * 2) + 13;

                this.Bounds = new Rectangle(X, Y, Width, Height);

                this.BringToFront();
                Rect = currentControl.Bounds;
                this.Region = new Region(BuildFrame());
            }

            if (this.Control.IsDisposed == false)
            {
                g = this.CreateGraphics();
            }
        }

		/// <summary>
		/// Transparents the control area in the tracker
		/// </summary>
		/// <returns>Graphics Path made for transparenting</returns>
		private GraphicsPath BuildFrame()
		{
			//make the tracker to "contain" the control like this:
			//
			//+++++++++++++++++++++++
			//+						+
			//+						+
			//+	 	 CONTROL		+
			//+						+
			//+						+
			//+++++++++++++++++++++++
			//

            GraphicsPath path = new GraphicsPath();

            int margin = Square.Width + 1;
            int trackerWidth = currentControl.Size.Width + (margin * 2);
            int trackerHeight = currentControl.Size.Height + (margin * 2);
            Rectangle rec = new Rectangle(0, 0, trackerWidth,trackerHeight);
            path.AddRectangle(rec);

            int controlWidth = currentControl.Size.Width;
            int controlHeight = currentControl.Size.Height;
            rec = new Rectangle(margin, margin, controlWidth, controlHeight);
            path.AddRectangle(rec);

            return path;
		}

		#endregion

		#region Event Handlers

		private void Mouse_Move(object sender,System.Windows.Forms.MouseEventArgs e)
		{
			if (currentControl.Height < 8)
			{
				currentControl.Height = 8;
				return;
			}
			else if (currentControl.Width < 8)
			{
				currentControl.Width = 8;
				return;
			}
					
			switch(this.CurrBorder)
			{
				case ResizeBorder.Top:
					currentControl.Height = currentControl.Height - e.Y + prevLeftClick.Y;
					if (currentControl.Height > 8)
						currentControl.Top = currentControl.Top + e.Y - prevLeftClick.Y;					
					break;
				case ResizeBorder.TopLeft:
					currentControl.Height = currentControl.Height - e.Y + prevLeftClick.Y;
					if (currentControl.Height > 8)
						currentControl.Top =currentControl.Top + e.Y - prevLeftClick.Y;
					currentControl.Width = currentControl.Width - e.X + prevLeftClick.X;
					if (currentControl.Width > 8)
						currentControl.Left =currentControl.Left + e.X - prevLeftClick.X;
					break;
				case ResizeBorder.TopRight:
					currentControl.Height = currentControl.Height - e.Y + prevLeftClick.Y;
					if (currentControl.Height > 8)
						currentControl.Top = currentControl.Top + e.Y - prevLeftClick.Y;
					currentControl.Width = currentControl.Width + e.X - prevLeftClick.X;
					break;
				case ResizeBorder.Right:
					currentControl.Width = currentControl.Width + e.X - prevLeftClick.X;
					break;
				case ResizeBorder.Bottom:
					currentControl.Height = currentControl.Height + e.Y - prevLeftClick.Y;
					break;
				case ResizeBorder.BottomLeft:
					currentControl.Height = currentControl.Height + e.Y - prevLeftClick.Y;
					currentControl.Width = currentControl.Width - e.X + prevLeftClick.X;
					if (currentControl.Width > 8)
						currentControl.Left = currentControl.Left + e.X - prevLeftClick.X;
					break;
				case ResizeBorder.BottomRight:
					currentControl.Height = currentControl.Height + e.Y - prevLeftClick.Y;
					currentControl.Width = currentControl.Width + e.X - prevLeftClick.X;
					break;
				case ResizeBorder.Left:
					currentControl.Width = currentControl.Width - e.X + prevLeftClick.X;
					if (currentControl.Width > 8)
						currentControl.Left = currentControl.Left + e.X - prevLeftClick.X;
					break;				
			}
		}

		private void ControlTracker_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Cursor = Cursors.Arrow;

            if (isResizeSelect)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // If this is the first mouse move event for left click dragging of the form,
                    // store the current point clicked so that we can use it to calculate the form's
                    // new location in subsequent mouse move events due to left click dragging of the form
                    if (isFirst == true)
                    {
                        // Store previous left click position
                        prevLeftClick = new Point(e.X, e.Y);

                        // Subsequent mouse move events will not be treated as first time, until the
                        // left mouse click is released or other mouse click occur
                        isFirst = false;
                    }
                    else
                    {
                        //hide tracker
                        this.Visible = false;
                        //forward mouse position to append changes
                        Mouse_Move(this, e);
                        // Store new previous left click position
                        prevLeftClick = new Point(e.X, e.Y);
                    }
                }
                else
                {
                    // This is a new mouse move event so reset flag
                    isFirst = true;
                    //show the tracker
                    this.Visible = true;

                    //update the mouse pointer to other cursors by checking it's position
                    IsPointInTracker(e.X, e.Y);
                }
            }
		}

		private void ControlTracker_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Draw();
		}

		private void ControlTracker_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			//the mouse is up, recreate the control to append changes
            WrapControl();
			this.Visible = false;
			if (this.Resized != null)
			{
				if (currentControl != null)
				{
					this.Resized(currentControl, new EventArgs());
				}
			}
		}

		#endregion
	}
}
