using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.Collections.ObjectModel;

namespace Epi.Windows.Docking
{
	/// <summary>
	/// This enumator contains a list of available visual styles for the docking framework.
	/// </summary>
	public enum DockVisualStyle
	{
		/// <summary>
		/// A theme like the one in VS2003.
		/// </summary>
		VS2003,

		/// <summary>
		/// A theme like the one in VS2005.
		/// </summary>
		VS2005
	}

	/// <summary>
	/// This class is derived from the <see cref="DockContainer"/> class to extend its features.
	/// Use this class as top-level container in the destination windows of your application.
	/// </summary>
    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
	public partial class DockManager : DockContainer
	{
		#region Construct and dispose
		/// <summary>
		/// Initializes a new instance of the <seealso cref="DockManager"/> class.
		/// </summary>
		/// <param name="container">The host container.</param>
		public DockManager(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();

			Init();
		}

		/// <summary>
		/// Initializes a new instance of the <seealso cref="DockManager"/> class.
		/// </summary>
		public DockManager()
		{
			InitializeComponent();

			Init();
		}

		/// <summary>
		/// Initializes the <seealso cref="DockManager"/> (paint styles and startup containter type).
		/// </summary>
		void Init()
		{
			// Enable double buffering.
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			// Set container type to document and clear removeable flag to prevent manager to destroy itself.
			DockType = DockContainerType.Document;
			removeable = false;

			// Create event handler.
			dragWindowHandler = new DockEventHandler(this.DragWindow);

			DockManager.RegisterManager(this);
		}

		#endregion

		#region Variables
		private DockPanel activeDoc = null;
		private DockContainer autoHideContainer = null;

		private Collection<DockContainer> autoHideL = new Collection<DockContainer>();
		private Collection<DockContainer> autoHideT = new Collection<DockContainer>();
		private Collection<DockContainer> autoHideR = new Collection<DockContainer>();
		private Collection<DockContainer> autoHideB = new Collection<DockContainer>();
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the visual style of the docking framework.
		/// </summary>
        [Category("DockDotNET"), Description("Gets or sets the visual style of the docking framework.")]
		public DockVisualStyle VisualStyle
		{
			get { return DockManager.Style; }
			set { DockManager.Style = value; }
		}

		/// <summary>
		/// Gets or sets the flag that controls fast, but less breathtaking drawing when a moving a window to enhance performance.
		/// </summary>
        [Category("DockDotNET"), Description("Gets or sets the flag that controls fast, but less breathtaking drawing when a moving a window to enhance performance.")]
		public bool FastDrawing
		{
			get { return DockManager.FastMoveDraw; }
			set { DockManager.FastMoveDraw = value; }
		}
		#endregion

		#region Control management
		/// <summary>
		/// Overrides the base class function <see cref="DockContainer.OnControlAdded"/>.
		/// It blocks any attempt of adding a control that is not a <see cref="DockContainer"/>, <see cref="DockPanel"/> or <see cref="FlatButton"/>.
		/// </summary>
		/// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
		protected override void OnControlAdded(ControlEventArgs e)
		{
			if (!(e.Control is DockContainer) && !(e.Control is DockPanel) && !(e.Control is FlatButton))
			{
				if (Parent != null)
					Parent.Controls.Add(e.Control);
				else
					this.Controls.Remove(e.Control);

				Invalidate();
			}
			else
			{
				base.OnControlAdded(e);
			}
		}
		#endregion

		#region Paint
		/// <summary>
		/// Overrides the base class function.
		/// Draws the panel descriptions of AutoHide members.
		/// </summary>
		/// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			StringFormat sf = StringFormat.GenericDefault;
			Graphics g = e.Graphics;
			SizeF size;
			int start = 3;
			int end = 0;
			int yOff = 0;

			sf.Trimming = StringTrimming.EllipsisCharacter;

			//g.FillRectangle(new LinearGradientBrush(this.ClientRectangle, SystemColors.Control, Color.White, LinearGradientMode.Horizontal), this.ClientRectangle);
			
			if (panList.Count == 0)
			{
				g.Clear(SystemColors.Control);

				Rectangle rc = new Rectangle(dockOffsetL, dockOffsetT, this.Width - dockOffsetL - dockOffsetR, this.Height - dockOffsetT - dockOffsetB);
				g.FillRectangle(SystemBrushes.ControlDark, rc);

				//g.FillRectangle(new LinearGradientBrush(this.ClientRectangle, SystemColors.Control, Color.White, LinearGradientMode.Horizontal), this.ClientRectangle);
			}
			else
			{
				base.OnPaint(e);
			}

			#region AutoHide Top
			start = 3 + dockOffsetL;

			foreach (DockContainer c in autoHideT)
			{
				foreach (DockPanel p in c.panList)
				{
					size = g.MeasureString(p.Form.Text, this.Font);

					end = start + (int)size.Width + 18;

					g.DrawString(p.Form.Text, this.Font, SystemBrushes.ControlDarkDark, start + 18, 3);
					g.DrawIcon(p.Form.Icon, new Rectangle(start + 3, 2, 16, 16));
					g.DrawLine(SystemPens.ControlDark, start, 0, start, 17);
					g.DrawLine(SystemPens.ControlDark, start, 17, start + 2, 19);
					g.DrawLine(SystemPens.ControlDark, start + 2, 19, end - 2, 19);
					g.DrawLine(SystemPens.ControlDark, end - 2, 19, end, 17);
					g.DrawLine(SystemPens.ControlDark, end, 0, end, 17);

					size.Height = 20;
					size.Width += 18;

					p.TabRect = new RectangleF(start, 0, size.Width, dockOffsetT);

					start += (int)size.Width;
				}

				start += 16;
			}
			#endregion

			#region AutoHide Bottom
			start = 3 + dockOffsetL;
			yOff = this.Height;

			foreach (DockContainer c in autoHideB)
			{
				foreach (DockPanel p in c.panList)
				{
					size = g.MeasureString(p.Form.Text, this.Font);

					end = start + (int)size.Width + 18;

					g.DrawString(p.Form.Text, this.Font, SystemBrushes.ControlDarkDark, start + 18, yOff - 3 - size.Height);
					g.DrawIcon(p.Form.Icon, new Rectangle(start + 3, this.Height - dockOffsetB + 5, 16, 16));
					g.DrawLine(SystemPens.ControlDark, start, yOff, start, yOff - 17);
					g.DrawLine(SystemPens.ControlDark, start, yOff - 17, start + 2, yOff - 19);
					g.DrawLine(SystemPens.ControlDark, start + 2, yOff - 19, end - 2, yOff - 19);
					g.DrawLine(SystemPens.ControlDark, end - 2, yOff - 19, end, yOff - 17);
					g.DrawLine(SystemPens.ControlDark, end, yOff, end, yOff - 17);

					size.Height = 20;
					size.Width += 18;

					p.TabRect = new RectangleF(start, this.Height - size.Height - 2, size.Width, dockOffsetB + 2);

					start += (int)size.Width;
				}

				start += 16;
			}
			#endregion

			g.RotateTransform(90);
			
			#region AutoHide Left
			start = 3 + dockOffsetT;
			
			foreach (DockContainer c in autoHideL)
			{
				foreach (DockPanel p in c.panList)
				{
					size = g.MeasureString(p.Form.Text, this.Font);

					end = start + (int)size.Width + 18;

					g.DrawString(p.Form.Text, this.Font, SystemBrushes.ControlDarkDark, start + 18, -size.Height);
					g.DrawIcon(p.Form.Icon, new Rectangle(2, start+3, 16, 16));
					g.DrawLine(SystemPens.ControlDark, start, 0, start, -17);
					g.DrawLine(SystemPens.ControlDark, start, -17, start + 2, -19);
					g.DrawLine(SystemPens.ControlDark, start + 2, -19, end - 2, -19);
					g.DrawLine(SystemPens.ControlDark, end - 2, -19, end, -17);
					g.DrawLine(SystemPens.ControlDark, end, 0, end, -17);

					size.Height = 20;
					size.Width += 18;

					p.TabRect = new RectangleF(0, start, dockOffsetL, size.Width);

					start += (int)size.Width;
				}

				start += 16;
			}
			#endregion

			#region AutoHide Right
			start = 3 + dockOffsetT;
			yOff = -this.Width;

			foreach (DockContainer c in autoHideR)
			{
				foreach (DockPanel p in c.panList)
				{
					size = g.MeasureString(p.Form.Text, this.Font);

					end = start + (int)size.Width + 18;

					g.DrawString(p.Form.Text, this.Font, SystemBrushes.ControlDarkDark, start + 18, yOff + 3);
					g.DrawIcon(p.Form.Icon, new Rectangle(this.Width - dockOffsetR + 5, start + 3, 16, 16));
					g.DrawLine(SystemPens.ControlDark, start, yOff, start, yOff + 17);
					g.DrawLine(SystemPens.ControlDark, start, yOff + 17, start + 2, yOff + 19);
					g.DrawLine(SystemPens.ControlDark, start + 2, yOff + 19, end - 2, yOff + 19);
					g.DrawLine(SystemPens.ControlDark, end - 2, yOff + 19, end, yOff + 17);
					g.DrawLine(SystemPens.ControlDark, end, yOff, end, yOff + 17);

					size.Height = 20;
					size.Width += 18;

					p.TabRect = new RectangleF(this.Width - size.Height - 2, start, dockOffsetR, size.Width + 2);

					start += (int)size.Width;
				}

				start += 16;
			}
			#endregion
		}
		#endregion

		#region AutoHide
		internal void AutoHideContainer(DockContainer c, DockStyle dst, bool hide)
		{
			if (c == null)
				return;

			switch (dst)
			{
				case DockStyle.Left:
					dockOffsetL = UpdateAutoHideList(c, hide, autoHideL);
					break;
				case DockStyle.Top:
					dockOffsetT = UpdateAutoHideList(c, hide, autoHideT);
					break;
				case DockStyle.Right:
					dockOffsetR = UpdateAutoHideList(c, hide, autoHideR);
					break;
				case DockStyle.Bottom:
					dockOffsetB = UpdateAutoHideList(c, hide, autoHideB);
					break;
				default:
					dockOffsetL = UpdateAutoHideList(c, false, autoHideL);
					dockOffsetT = UpdateAutoHideList(c, false, autoHideT);
					dockOffsetR = UpdateAutoHideList(c, false, autoHideR);
					dockOffsetB = UpdateAutoHideList(c, false, autoHideB);
					break;
			}

			AdjustBorders();
		}

		private int UpdateAutoHideList(DockContainer c, bool hide, Collection<DockContainer> list)
		{
			if (hide)
			{
				list.Add(c);
			}
			else if (list.Contains(c))
			{
				autoHideContainer = null;
				list.Remove(c);
				this.Controls.Remove(c);
			}

			if (list.Count > 0)
				return 22;
			else
				return 0;
		}

		/// <summary>
		/// Expands the base class function with auto-hide checks.
		/// </summary>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the mouse data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			Rectangle rc;
			Size size;

			if (autoHideContainer != null)
				if (autoHideContainer.fading)
					return;

			#region AutoHide Left
			foreach (DockContainer c in autoHideL)
			{
				foreach (DockPanel p in c.panList)
				{
					rc = new Rectangle((int)p.TabRect.Left, (int)p.TabRect.Top, (int)p.TabRect.Width, (int)p.TabRect.Height);
					
					if (RectangleToScreen(rc).Contains(MousePosition.X, MousePosition.Y))
					{
						c.ActivePanel = p;
						
						if (c != autoHideContainer)
						{
							disableOnControlAdded = true;
							disableOnControlRemove = true;

							this.Controls.Remove(autoHideContainer);
							autoHideContainer = c;
							size = c.Size;
							c.Dock = DockStyle.None;
							c.Location = new Point(dockOffsetL, dockOffsetT);
							c.Size = new Size(size.Width, this.Height - dockOffsetT - dockOffsetB);
							c.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
							this.Controls.Add(c);
							c.FadeIn();

							disableOnControlAdded = false;
							disableOnControlRemove = false;
						}

						return;
					}
				}
			}
			#endregion

			#region AutoHide Right
			foreach (DockContainer c in autoHideR)
			{
				foreach (DockPanel p in c.panList)
				{
					rc = new Rectangle((int)p.TabRect.Left, (int)p.TabRect.Top, (int)p.TabRect.Width, (int)p.TabRect.Height);

					if (RectangleToScreen(rc).Contains(MousePosition.X, MousePosition.Y))
					{
						c.ActivePanel = p;
						
						if (c != autoHideContainer)
						{
							this.Controls.Remove(autoHideContainer);
							autoHideContainer = c;
							size = c.Size;
							c.Dock = DockStyle.None;
							c.Location = new Point(this.Width - size.Width - dockOffsetR, dockOffsetT);
							c.Size = new Size(size.Width, this.Height - dockOffsetT - dockOffsetB);
							c.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
							this.Controls.Add(c);
							c.FadeIn();
						}

						return;
					}
				}
			}
			#endregion

			#region AutoHide Top
			foreach (DockContainer c in autoHideT)
			{
				foreach (DockPanel p in c.panList)
				{
					rc = new Rectangle((int)p.TabRect.Left, (int)p.TabRect.Top, (int)p.TabRect.Width, (int)p.TabRect.Height);

					if (RectangleToScreen(rc).Contains(MousePosition.X, MousePosition.Y))
					{
						c.ActivePanel = p;
						
						if (c != autoHideContainer)
						{
							this.Controls.Remove(autoHideContainer);
							autoHideContainer = c;
							size = c.Size;
							c.Dock = DockStyle.None;
							c.Location = new Point(dockOffsetL, dockOffsetT);
							c.Size = new Size(this.Width - dockOffsetL - dockOffsetR, size.Height);
							c.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left;
							this.Controls.Add(c);
							c.FadeIn();
						}

						return;
					}
				}
			}
			#endregion

			#region AutoHide Bottom
			foreach (DockContainer c in autoHideB)
			{
				foreach (DockPanel p in c.panList)
				{
					rc = new Rectangle((int)p.TabRect.Left, (int)p.TabRect.Top, (int)p.TabRect.Width, (int)p.TabRect.Height);

					if (RectangleToScreen(rc).Contains(MousePosition.X, MousePosition.Y))
					{
						c.ActivePanel = p;
						
						if (c != autoHideContainer)
						{
							this.Controls.Remove(autoHideContainer);
							autoHideContainer = c;
							size = c.Size;
							c.Dock = DockStyle.None;
							c.Location = new Point(dockOffsetL, this.Height - size.Height - dockOffsetB);
							c.Size = new Size(this.Width - dockOffsetL - dockOffsetR, size.Height);
							c.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
							this.Controls.Add(c);
							c.FadeIn();
						}

						return;
					}
				}
			}
			#endregion

			if (autoHideContainer != null)
			{
				if (!autoHideContainer.RectangleToScreen(autoHideContainer.ClientRectangle).Contains(MousePosition.X, MousePosition.Y))
					autoHideContainer.FadeOut();
			}
		}

		/// <summary>
		/// Expands the base class function with auto-hide checks.
		/// </summary>
		/// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (autoHideContainer != null)
			{
				if (autoHideContainer.fading)
					return;

				if (!autoHideContainer.RectangleToScreen(autoHideContainer.ClientRectangle).Contains(MousePosition.X, MousePosition.Y))
					autoHideContainer.FadeOut();
			}
		}

		internal void ReleaseAutoHideContainer()
		{
			this.Controls.Remove(autoHideContainer);
			autoHideContainer = null;
		}
		#endregion

		#region Parent form
		/// <summary>
		/// Overrides the function <see cref="DockContainer.OnParentChanged"/>.
		/// Tries to convert the parent into a form and then signs into some functions of the parent window.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnParentChanged(EventArgs e)
		{
			if (Parent is Form)
			{
				Form frm = Parent as Form;
				frm.KeyDown += new KeyEventHandler(InvokeKeyDown);
				frm.KeyUp += new KeyEventHandler(InvokeKeyUp);
				frm.Deactivate += new EventHandler(DeactivateParent);
				frm.Activated += new EventHandler(ActivateParent);
			}

			base.OnParentChanged(e);
		}

		/// <summary>
		/// A message handler for the Deactivate event of the parent window.
		/// Needed to refresh the child controls.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		private void DeactivateParent(object sender, EventArgs e)
		{
			try
			{
				foreach (DockPanel p in DockManager.ListDocument)
				{
					DockContainer c = p.Form.HostContainer;
					if (c == null)
						continue;

					if ((c.ActivePanel == p) && (c.ContainsFocus))
					{
						activeDoc = p;
						p.SetFocus(false);
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockManager.DeactivateParent: "+ex.Message);
			}
			finally
			{
				Invalidate(true);
			}
		}
		
		/// <summary>
		/// A message handler for the Activate event of the parent window.
		/// Needed to refresh the child controls.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		private void ActivateParent(object sender, EventArgs e)
		{
			try
			{
				if (activeDoc != null)
					activeDoc.SetFocus(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockManager.ActivateParent: "+ex.Message);
			}
			finally
			{
				Invalidate(true);
			}
		}
		#endregion

		#region XML r/w
		/// <summary>
		/// Writes the manager data to the window save list.
		/// </summary>
		/// <param name="writer">The <see cref="XmlTextWriter"/> object that writes to the target stream.</param>
		override internal void WriteXml(XmlTextWriter writer)
		{
			writer.WriteStartElement("manager");
			writer.WriteAttributeString("parent", Parent.GetType().FullName);

			foreach (DockContainer c in conList)
				c.WriteXml(writer);

			foreach (DockPanel p in panList)
				p.WriteXml(writer);

			writer.WriteEndElement();
		}

		/// <summary>
		/// Reads the manager data from the window save list.
		/// </summary>
		/// <param name="reader">The <see cref="XmlReader"/> object that reads from the source stream.</param>
		override internal void ReadXml(XmlReader reader)
		{
			base.ReadXml(reader);
		}
		#endregion

		#region Static variables and functions
		#region Variables
		private static Collection<DockPanel> listPanel = new Collection<DockPanel>();
		private static Collection<DockPanel> listDocument = new Collection<DockPanel>();
		private static Collection<DockPanel> listTool = new Collection<DockPanel>();

		private static ArrayList contList = new ArrayList();
		private static ArrayList formList = new ArrayList();
		private static ArrayList managerList = new ArrayList();
		private static DockEventHandler dragEvent;
		private static DockVisualStyle style = DockVisualStyle.VS2005;
		private static bool fastMoveDraw = true;
		private static OverlayForm dockGuide = null;
		private static bool noGuidePlease = false;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the static <see cref="DockPanel"/> collection.
		/// </summary>
		public static Collection<DockPanel> ListPanel
		{
			get { return listPanel; }
			set { listPanel = value; }
		}

		/// <summary>
		/// Gets or sets the static <see cref="DockPanel"/> (document type) collection.
		/// </summary>
		public static Collection<DockPanel> ListDocument
		{
			get { return listDocument; }
			set { listDocument = value; }
		}

		/// <summary>
		/// Gets or sets the static <see cref="DockPanel"/> (tool type) collection.
		/// </summary>
		public static Collection<DockPanel> ListTool
		{
			get { return listTool; }
			set { listTool = value; }
		}

		/// <summary>
		/// Gets or sets the visual style of the docking framework.
		/// </summary>
		public static DockVisualStyle Style
		{
			get { return style; }
			set { style = value; }
		}

		/// <summary>
		/// Gets or sets the flag that controls fast, but less breathtaking drawing when a moving a window to enhance performance.
		/// </summary>
		public static bool FastMoveDraw
		{
			get { return fastMoveDraw; }
			set { fastMoveDraw = value; }
		}

		internal static OverlayForm DockGuide
		{
			get { return dockGuide; }
			set { dockGuide = value; }
		}

		internal static bool NoGuidePlease
		{
			get { return noGuidePlease; }
			set { noGuidePlease = value; }
		}
		#endregion

		#region Register
		#region Container
		internal static void RegisterContainer(DockContainer cont)
		{
			if (contList.Contains(cont))
				return;

			if (cont == null)
				throw new ArgumentNullException("The container must not be null.");

			cont.Disposed += new EventHandler(ObjectDisposed);
			contList.Add(cont);

			dragEvent += cont.dragWindowHandler;
		}

		internal static void UnRegisterContainer(DockContainer cont)
		{
			if (!contList.Contains(cont))
				return;

			dragEvent -= cont.dragWindowHandler;
			contList.Remove(cont);
		}
		#endregion

		#region Form
		internal static void RegisterForm(DockForm form)
		{
			if (formList.Contains(form))
				return;

			if (form == null)
				throw new ArgumentNullException("The form must not be null.");

			form.Disposed += new EventHandler(ObjectDisposed);
			formList.Add(form);
		}

		internal static void FormActivated(DockForm form)
		{
			// Update Z-Order.
			if (!formList.Contains(form))
				return;

			formList.Remove(form);
			formList.Insert(0, form);
		}

		internal static int GetZIndex(DockForm form)
		{
			return formList.IndexOf(form);
		}

		internal static DockForm GetFormAtPoint(Point pt, int startIndex)
		{
			for (int i = startIndex; i < formList.Count; i++)
			{
				DockForm f = formList[i] as DockForm;

				if (f.Bounds.Contains(pt) & f.Visible)
					return f;
			}

			return null;
		}

		/// <summary>
		/// A small helper function - writes the currently opened <see cref="DockForm"/> objects.
		/// </summary>
		public static void DebugFormList()
		{
			foreach (DockForm f in formList)
			{
				Console.WriteLine(f.Text);
			}
		}

		internal static void UnRegisterForm(DockForm form)
		{
			if (!formList.Contains(form))
				return;

			formList.Remove(form);
		}
		#endregion

		#region Manager
		internal static void RegisterManager(DockManager manager)
		{
			if (managerList.Contains(manager))
				return;

			if (manager == null)
				throw new ArgumentNullException("The manager must not be null.");

			manager.Disposed += new EventHandler(ObjectDisposed);
			managerList.Add(manager);
		}

		internal static void UnRegisterManager(DockManager manager)
		{
			if (!managerList.Contains(manager))
				return;

			managerList.Remove(manager);
		}
		#endregion

		#region Window
		internal static void RegisterWindow(DockWindow wnd)
		{
			if (wnd == null)
				throw new ArgumentNullException("The window must not be null.");
			
			if (listPanel.Contains(wnd.ControlContainer))
				return;

			wnd.Disposed += new EventHandler(ObjectDisposed);

			listPanel.Add(wnd.ControlContainer);
			if (wnd.DockType == DockContainerType.Document)
				listDocument.Add(wnd.ControlContainer);
			else if (wnd.DockType == DockContainerType.ToolWindow)
				listTool.Add(wnd.ControlContainer);
		}

		internal static void UnRegisterWindow(DockWindow wnd)
		{
			if (wnd == null)
				throw new ArgumentNullException("The window must not be null.");

			if (!listPanel.Contains(wnd.ControlContainer))
				return;

			contList.Remove(wnd.ControlContainer);
			if (wnd.DockType == DockContainerType.Document)
				listDocument.Remove(wnd.ControlContainer);
			else if (wnd.DockType == DockContainerType.ToolWindow)
				listTool.Remove(wnd.ControlContainer);
		}
		#endregion

		static void ObjectDisposed(object sender, EventArgs e)
		{
			if (sender is DockContainer)
				UnRegisterContainer(sender as DockContainer);
			else if (sender is DockForm)
				UnRegisterForm(sender as DockForm);
			else if (sender is DockManager)
				UnRegisterManager(sender as DockManager);
			else if (sender is DockWindow)
				UnRegisterWindow(sender as DockWindow);
			else
				throw new ArgumentException("Only DockForm, DockContainer, DockManager and DockWindow objects may be handled by the DockServer.");
		}
		#endregion

		#region Drag event
		internal static void InvokeDragEvent(object sender, DockEventArgs e)
		{
			if (dragEvent != null)
				dragEvent(sender, e);

			if (!e.ShowDockGuide)
				HideDockGuide();
		}
		#endregion

		#region DockGuide
		internal static void UpdateDockGuide(DockContainer target, DockEventArgs e)
		{
			if ((target == null) | noGuidePlease | (style != DockVisualStyle.VS2005) | fastMoveDraw)
			{
				HideDockGuide();
				return;
			}

			if (dockGuide == null)
				dockGuide = new OverlayForm();

			dockGuide.TargetHost = target;
			dockGuide.Size = target.Size;

			if (!dockGuide.Visible)
				dockGuide.Show();

			if (target.Parent != null)
				dockGuide.Location = target.RectangleToScreen(target.ClientRectangle).Location;
			else
				dockGuide.Location = target.Location;

			dockGuide.BringToFront();

			// Make tests.
			DockStyle dstStyle = dockGuide.HitTest(e.Point);

			if (dstStyle != DockStyle.None)
				e.Point = target.GetVirtualDragDest(dstStyle);
			else
				e.Handled = true;

			e.ShowDockGuide = true;
		}

		internal static void HideDockGuide()
		{
			if (dockGuide != null)
				dockGuide.Hide();
		}
		#endregion

		#region XML r/w
		/// <summary>
		/// Writes the complete hierarchy to a file.
		/// </summary>
		/// <param name="file">The target XML file.</param>
		public static void WriteXml(string file)
		{
			XmlTextWriter writer = new XmlTextWriter(file, System.Text.Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument(true);

			writer.WriteStartElement("docktree");

			foreach (DockManager m in managerList)
				m.WriteXml(writer);

			foreach (DockForm f in formList)
				f.WriteXml(writer);

			writer.WriteEndElement();

			writer.WriteEndDocument();
			writer.Close();
		}

		/// <summary>
		/// Reads the complete hierarchy from a file.
		/// </summary>
		/// <param name="file">The source XML file.</param>
		public static void ReadXml(string file)
		{
			XmlTextReader reader = null;

			try
			{
				// Load the reader with the data file and ignore all white space nodes.         
				reader = new XmlTextReader(file);
				reader.WhitespaceHandling = WhitespaceHandling.None;

				// Parse the file and display each of the nodes.
				while (reader.Read())
				{
					if (!reader.IsStartElement())
						continue;
					
					switch (reader.Name)
					{
						case "form":
							Console.WriteLine("form");

							DockForm form = new DockForm();
							form.Opacity = 0;
							form.Show();
							form.ReadXml(reader.ReadSubtree());
						
							//if (!form.RootContainer.IsEmpty)
								form.Opacity = 1;
							//else
							//	form.Close();
							break;

						case "manager":
							Console.WriteLine("manager");

							string hostType = reader.GetAttribute("parent");
							if (hostType != null)
							{
								foreach (DockManager m in managerList)
									if (m.Parent.GetType().FullName == hostType)
										m.ReadXml(reader.ReadSubtree());
							}
							break;
					}
				}
			}
			finally
			{
				if (reader != null)
					reader.Close();
			}
		}
		#endregion
		#endregion
	}
}
