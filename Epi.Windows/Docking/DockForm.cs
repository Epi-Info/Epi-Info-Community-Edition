using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Xml;

namespace Epi.Windows.Docking
{
	internal partial class DockForm : Form
	{
		#region Constructor
		public DockForm()
		{
			InitializeComponent();

			// Create and initialize the root container.
			rootContainer = new DockContainer();
			rootContainer.Dock = DockStyle.Fill;
			this.Controls.Add(rootContainer);

			RegisterToMdiContainer();
		}

		public DockForm(Panel dragObject)
		{
			InitializeComponent();

			this.Opacity = 0;

			if (dragObject is DockContainer)
			{
				DockContainer c = dragObject as DockContainer;

				if (c.panList.Count == 1)
					this.ClientSize = (c.panList[0] as DockPanel).Form.ClientSize;
				else
					this.ClientSize = dragObject.Size;

				if (c.removeable)
					rootContainer = c;
				else
				{
					rootContainer = new DockContainer();
					rootContainer.Controls.AddRange((DockPanel[])c.panList.ToArray(typeof(DockPanel)));
					rootContainer.Controls.AddRange((DockContainer[])c.conList.ToArray(typeof(DockContainer)));

					ArrayList list = new ArrayList();
					rootContainer.GetPanels(list);
					if (list.Count > 0)
						rootContainer.DockType = (list[0] as DockPanel).Form.DockType;
				}
			}
			else if (dragObject is DockPanel)
			{
				DockPanel p = dragObject as DockPanel;

				this.ClientSize = p.Form.ClientSize;
				
				rootContainer = new DockContainer();
				p.Form.CopyToDockForm(this);
			}

			if (rootContainer.panList.Count > 0)
			{
				(rootContainer.panList[0] as DockPanel).Form.CopyPropToDockForm(this);
				rootContainer.SetFormSizeBounds(this);
				rootContainer.SelectTab(0);
			}

			rootContainer.Dock = DockStyle.Fill;
			this.Controls.Add(rootContainer);

			RegisterToMdiContainer();
		}

		private void RegisterToMdiContainer()
		{
			if (Application.OpenForms.Count > 0)
				Application.OpenForms[0].AddOwnedForm(this);

			DockManager.RegisterForm(this);
		}
		#endregion

		#region Variables
		private OverlayForm dragWindow = null;
		private DockContainer dragTarget = null;
		private DockContainer rootContainer = null;

		private bool moving = false;
		private bool allowDock = true;

		private Point ptStart;
		private Point ptRef;
		#endregion

		#region Properties
		public DockContainer DragTarget
		{
			get { return dragTarget; }
		}

		public bool Moving
		{
			get { return moving; }
		}

		public bool AllowDock
		{
			get { return allowDock; }
			set { allowDock = value; }
		}

		public DockContainer RootContainer
		{
			get { return rootContainer; }
			set
			{
				if (rootContainer != null)
				{
					this.Controls.Remove(rootContainer);
					rootContainer.Dispose();
				}

				rootContainer = value;
				this.Controls.Add(rootContainer);
			}
		}
		#endregion

		#region Moving and Docking
		#region WndProc for mouse events
		/// <summary>
		/// Since there are no non-client area notifications in .NET, this message loop hook is needed.
		/// Captures a window move event and starts own movement procedure with an attached drag window.
		/// </summary>
		/// <param name="m">The Windows Message to process.</param>
		protected override void WndProc(ref Message m)
		{
			if ((m.Msg == (int)Win32.Msgs.WM_NCLBUTTONDOWN) & (m.WParam == (IntPtr)Win32.HitTest.HTCAPTION))
			{
				StartMoving(new Point(MousePosition.X, MousePosition.Y));
				return;
			}
			else if (moving)
			{
				if (m.Msg == (int)Win32.Msgs.WM_MOUSEMOVE)
				{
					Application.DoEvents();

					if (this.IsDisposed)
						return;
					
					if (MouseButtons == MouseButtons.None)
					{
						EndMoving();
						Show();
					}
					else
					{
						if (DockManager.FastMoveDraw & this.Visible)
							Hide();

						this.Location = new Point(ptRef.X + MousePosition.X - ptStart.X, ptRef.Y + MousePosition.Y - ptStart.Y);
						MoveWindow();
						this.Capture = true;
					}					
					return;
				}
				else if ((m.Msg == (int)Win32.Msgs.WM_LBUTTONUP) | ((m.Msg == (int)Win32.Msgs.WM_NCMOUSEMOVE) & (MouseButtons == MouseButtons.None)))
				{
					EndMoving();

					if (SendDockEvent(true) != null)
					{
						rootContainer = null;
						this.Close();
						return;
					}

					Show();
				}
				else if (m.Msg == (int)Win32.Msgs.WM_MOUSELEAVE)
				{
					EndMoving();
					Show();
				}
			}

			base.WndProc(ref m);
		}

		public void StartMoving(Point start)
		{
			moving = true;
			ptStart = start;
			ptRef = new Point(this.Location.X, this.Location.Y);

			BringToFront();
			this.Capture = true;

			this.Opacity = 1;
		}

		private void EndMoving()
		{
			CloseDragWindow();
			BringToFront();
			this.Capture = false;
			moving = false;

			DockManager.HideDockGuide();
		}
		#endregion

		#region DragWindow
		/// <summary>
		/// Closes the attached drag window.
		/// </summary>
		private void CloseDragWindow()
		{
			if (dragWindow != null)
			{
				dragWindow.Close();
				dragWindow.Dispose();
				dragWindow = null;
			}
		}
		#endregion

		#region MoveWindow function
		/// <summary>
		/// Invokes the drag event and adjusts the size and location if a valid docking position was received.
		/// This method is only used by explicit drag windows (see flag).
		/// </summary>
		internal void MoveWindow()
		{
			dragTarget = SendDockEvent(false);

			if (dragTarget != null)
			{
				if (dragWindow == null)
				{
					dragWindow = new OverlayForm();
					dragWindow.Size = dragTarget.Size;
					dragWindow.Show();
					this.BringToFront();
				}
				else
					dragWindow.Size = dragTarget.Size;

				if (dragTarget.Parent != null)
					dragWindow.Location = dragTarget.RectangleToScreen(dragTarget.ClientRectangle).Location;
				else
					dragWindow.Location = dragTarget.Location;
			}
			else if (DockManager.FastMoveDraw)
			{
				if (dragWindow == null)
				{
					dragWindow = new OverlayForm();
					dragWindow.Size = this.Size;
					dragWindow.Show();
				}
				else
					dragWindow.Size = this.Size;

				dragWindow.Location = this.Location;
			}
			else
				CloseDragWindow();
		}
		#endregion

		#region Dock Event
		/// <summary>
		/// Invokes a drag event that accepts a valid position for docking.
		/// </summary>
		/// <param name="confirm">Set this flag to confirm the docking position, if available.</param>
		/// <returns>Returns the target <see cref="DockContainer"/>.</returns>
		internal DockContainer SendDockEvent(bool confirm)
		{
			DockEventArgs e = new DockEventArgs(new Point(MousePosition.X, MousePosition.Y), rootContainer.DockType, confirm);
			DockManager.InvokeDragEvent(this, e);

			if (e.Release)
				DockManager.HideDockGuide();

			return e.Target;
		}
		#endregion
		#endregion

		#region Overrides
		protected override void OnKeyDown(KeyEventArgs e)
		{
			this.rootContainer.InvokeKeyDown(this, e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			this.rootContainer.InvokeKeyUp(this, e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (rootContainer != null)
			{
				rootContainer.ActivePanel = null;
				rootContainer.CloseClick(this, new EventArgs());
			}

			base.OnClosing(e);
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			DockManager.FormActivated(this);
		} 
		#endregion

		#region XML r/w
		/// <summary>
		/// Writes the form data to the window save list.
		/// </summary>
		/// <param name="writer">The <see cref="XmlTextWriter"/> object that writes to the target stream.</param>
		internal void WriteXml(XmlTextWriter writer)
		{
			writer.WriteStartElement("form");
			writer.WriteAttributeString("width", this.Width.ToString());
			writer.WriteAttributeString("height", this.Height.ToString());
			writer.WriteAttributeString("x", this.Location.X.ToString());
			writer.WriteAttributeString("y", this.Location.Y.ToString());
			rootContainer.WriteXml(writer);
			writer.WriteEndElement();
		}
		
		/// <summary>
		/// Reads the form data from the window save list.
		/// </summary>
		/// <param name="reader">The <see cref="XmlReader"/> object that reads from the source stream.</param>
		internal void ReadXml(XmlReader reader)
		{
			try
			{
				string s;
				int x = 0, y = 0;

				s = reader.GetAttribute("width");
				if (s != null)
					this.Width = int.Parse(s);

				s = reader.GetAttribute("height");
				if (s != null)
					this.Height = int.Parse(s);

				s = reader.GetAttribute("x");
				if (s != null)
					x = int.Parse(s);

				s = reader.GetAttribute("y");
				if (s != null)
					y = int.Parse(s);

				this.Location = new Point(x, y);

				reader.Read();
				while (!reader.EOF)
				{
					if (reader.IsStartElement() & (reader.Name == "container"))
					{
						Console.WriteLine("container");
						rootContainer.ReadXml(reader.ReadSubtree());
						break;
					}
					else
						reader.Read();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockContainer.ReadXml: " + ex.Message);
			}
		}
		#endregion
	}
}