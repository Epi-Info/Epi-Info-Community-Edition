using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace Epi.Windows.Docking
{
	/// <summary>
	/// The <see cref="DockContainer"/> class is derived from the standard framework class <see cref="System.Windows.Forms.Panel"/>.
	/// It provides the basic functions to dock windows including the whole panel management.
	/// </summary>
	public partial class DockContainer : System.Windows.Forms.Panel
	{
		#region Construct and dispose
		/// <summary>
		/// Initializes a new instance of the <see cref="DockContainer"/> class.
		/// </summary>
		/// <param name="container">The host container.</param>
		public DockContainer(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();

			Init();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DockContainer"/> class.
		/// </summary>
		public DockContainer()
		{
			InitializeComponent();

			Init();
		}

		/// <summary>
		/// The central initialization method for all constructors.
		/// </summary>
		private void Init()
		{
			dragWindowHandler = new DockEventHandler(this.DragWindow);

			// Enable double buffering.
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			if (DockManager.Style == DockVisualStyle.VS2003)
			{
				topDock = 22;

				// Create tab left button.
				btnTabL.Hide();
				btnTabL.ShadowColor = Color.Black;
				btnTabL.Anchor = AnchorStyles.Top | AnchorStyles.Right;
				btnTabL.Enabled = false;
				btnTabL.PostPaint += new PaintEventHandler(btnTabL_PostPaint);
				btnTabL.Click += new EventHandler(btnTabL_Click);
				this.Controls.Add(btnTabL);

				// Create tab right button.
				btnTabR.Hide();
				btnTabR.ShadowColor = Color.Black;
				btnTabR.Anchor = AnchorStyles.Top | AnchorStyles.Right;
				btnTabR.Enabled = false;
				btnTabR.PostPaint += new PaintEventHandler(btnTabR_PostPaint);
				btnTabR.Click += new EventHandler(btnTabR_Click);
				this.Controls.Add(btnTabR);
			}
			else if (DockManager.Style == DockVisualStyle.VS2005)
			{
				topDock = 16;

				// Create autohide button.
				btnAutoHide.Hide();
				btnAutoHide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
				btnAutoHide.PostPaint += new PaintEventHandler(btnAutoHide_PostPaint);
				btnAutoHide.Click += new EventHandler(btnAutoHide_Click);
				this.Controls.Add(btnAutoHide);

				// Create menu button.
                // EK 9/14/11 - Served no purpose, removed
                //btnMenu.Hide();
                //btnMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                //btnMenu.PostPaint += new PaintEventHandler(btnMenu_PostPaint);
                //btnMenu.Click += new EventHandler(btnMenu_Click);
				//this.Controls.Add(btnMenu);
			}

			// Create close button.
			btnClose.Hide();
			btnClose.ShadowColor = Color.Black;
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnClose.PostPaint += new PaintEventHandler(btnClose_PostPaint);
			btnClose.Click += new EventHandler(btnClose_Click);
			this.Controls.Add(btnClose);

			// Drag Panel.
			dragPanel.Hide();
			dragPanel.MouseDown += new MouseEventHandler(dragPanel_MouseDown);
			dragPanel.MouseMove += new MouseEventHandler(dragPanel_MouseMove);
			dragPanel.MouseUp += new MouseEventHandler(dragPanel_MouseUp);
			dragPanel.Paint += new PaintEventHandler(dragPanel_Paint);
			this.Controls.Add(dragPanel);

			// Context menu.
			miUndock = contextMenuStrip.Items.Add("Undock", null, new EventHandler(UndockClick));
			miSep = contextMenuStrip.Items.Add("-");
			miClose = contextMenuStrip.Items.Add("Close", null, new EventHandler(CloseClick));
		}
		#endregion

		#region Variables
		private Point ptStart = Point.Empty;
		private Size oldSize = Size.Empty;

		private DockContainerType dockType = DockContainerType.None;
		private DockPanel activePanel = null;
		private DockPanel dragPanel = new DockPanel();
		private DockContainer dragDummy = null;
		private Panel dragObject = null;

		internal DockEventHandler dragWindowHandler;
		internal bool isActive = false;
		internal bool isDragContainer = false;
		internal bool disableOnControlAdded = false;
		internal bool disableOnControlRemove = false;
		internal bool removeable = true;

		#region AutoHide
		private struct AutoHideStorage
		{
			public DockStyle parentDock;
			public DockStyle toplevelDock;
			public DockContainer parent;
			public DockManager manager;

			public AutoHideStorage(DockManager manager, DockContainer parent, DockStyle parentDock, DockStyle toplevelDock)
			{
				this.manager = manager;
				this.parent = parent;
				this.parentDock = parentDock;
				this.toplevelDock = toplevelDock;
			}
		}

		private bool autoHide = false;
		private AutoHideStorage hideStorage;
		private bool fadeIn = false;
		internal bool fading = false;
		internal Bitmap fadeImage = null;
		internal Bitmap fadeBkImage = null;
		private Size fadeSize;
		private Point fadeLocation;

		private const int fadeSpeed = 50;
		#endregion

		private bool panelOverflow = false;
		private bool showIcons = true;
		private bool resizing = false;
		private bool blockFocusEvents = false;

		private int panelOffset = 0;
		private int splitterWidth = 4;
		private int dockBorder = 20;

		internal ArrayList conList = new ArrayList();
		internal ArrayList panList = new ArrayList();

		private FlatButton btnClose = new FlatButton();
		private FlatButton btnTabL = new FlatButton();
		private FlatButton btnTabR = new FlatButton();
		private FlatButton btnAutoHide = new FlatButton();
		private FlatButton btnMenu = new FlatButton();

		#region Dock offsets and constants
		/// <summary>
		/// Needed for the DockManager to implement his autohide borders.
		/// </summary>
		protected int dockOffsetL = 0;
		/// <summary>
		/// Needed for the DockManager to implement his autohide borders.
		/// </summary>
		protected int dockOffsetT = 0;
		/// <summary>
		/// Needed for the DockManager to implement his autohide borders.
		/// </summary>
		protected int dockOffsetR = 0;
		/// <summary>
		/// Needed for the DockManager to implement his autohide borders.
		/// </summary>
		protected int dockOffsetB = 0;

		private const int bottomDock = 26;
		private int topDock = 16;
		#endregion

		private ToolStripItem miUndock;
		private ToolStripItem miSep;
		private ToolStripItem miClose;
		#endregion

		#region Properties
		internal bool AllowSplit
		{
			get
			{
				foreach (DockPanel p in panList)
					if (!p.Form.AllowSplit)
						return false;

				return true;
			}
		}

		private bool HideContainer
		{
			get { return ((this.Parent is DockForm) & (panList.Count < 2)); }
		}

		internal bool IsRootContainer
		{
			get { return (this.Parent is DockForm); }
		}

		internal bool IsEmpty
		{
			get { return (panList.Count == 0) & (conList.Count == 0); }
		}

		internal bool AutoHide
		{
			get { return autoHide; }
			set
			{
				try
				{
					if (!(this.TopLevelContainer is DockManager))
						return;

					autoHide = value;

					// Get top level Container(!).
					DockContainer topLevel = this;

					while (topLevel.Parent != null)
					{
						if ((topLevel.Parent is DockManager) |
							!(topLevel.Parent is DockContainer))
							break;

						if ((topLevel.Parent as DockContainer).DockType == DockContainerType.Document)
							break;

						topLevel = topLevel.Parent as DockContainer;
					}

					if (autoHide)
					{
						// Fill structure with data.
						DockContainer conP = this.Parent as DockContainer;
						hideStorage = new AutoHideStorage(this.TopLevelContainer as DockManager, conP, this.Dock, topLevel.Dock);

						// Correct the DockStyle.Fill variant.
						if (hideStorage.parentDock == DockStyle.Fill)
						{
							foreach (DockContainer c in hideStorage.parent.conList)
								if (c != this)
								{
									if (c.Dock == DockStyle.Left)
										hideStorage.parentDock = DockStyle.Right;
									else if (c.Dock == DockStyle.Right)
										hideStorage.parentDock = DockStyle.Left;
									else if (c.Dock == DockStyle.Top)
										hideStorage.parentDock = DockStyle.Bottom;
									else if (c.Dock == DockStyle.Bottom)
										hideStorage.parentDock = DockStyle.Top;
									break;
								}
						}

						// Remove from parent container.
						hideStorage.parent.Controls.Remove(this);

						this.Hide();

						// Dock to the manager.
						hideStorage.manager.AutoHideContainer(this, hideStorage.toplevelDock, true);
					}
					else if (hideStorage.manager == null)
					{
						// Some strange error happened. Quit quickly. Nobody has seen it... ;-)
						autoHide = false;
						Console.WriteLine("DockContainer.AutoHide: the DockManager was removed or not set properly. Exiting AutoHide silently.");
						return;
					}
					else
						StopAutoHide();

					Invalidate();
				}
				catch (Exception ex)
				{
					Console.WriteLine("DockContainer.AutoHide: " + ex.Message);
					autoHide = false;
				}
			}
		}

		/// <summary>
		/// Gets or sets the width of the splitter.
		/// </summary>
		public int SplitterWidth
		{
			get { return splitterWidth; }
			set { splitterWidth = value; }
		}

		/// <summary>
		/// Determines, if icons are shown in the panel tabs.
		/// </summary>
		public bool ShowIcons
		{
			get { return showIcons; }
			set { showIcons = value; }
		}

		/// <summary>
		/// Gets or sets the type of the container.
		/// </summary>
		[Browsable(false)]
		virtual public DockContainerType DockType
		{
			get { return dockType; }
			set { dockType = value; }
		}

		/// <summary>
		/// Gets or sets the border of the active docking region.
		/// </summary>
		public int DockBorder
		{
			get { return dockBorder; }
			set { dockBorder = value; }
		}

		/// <summary>
		/// Gets or sets the selected panel of the container.
		/// </summary>
		[Browsable(false)]
		internal DockPanel ActivePanel
		{
			get { return activePanel; }
			set
			{
				if (activePanel == value)
					return;

				for (int i = 0; i < panList.Count; i++)
				{
					DockPanel panel = panList[i] as DockPanel;

					if ((activePanel != null) & (panel != value))
					{
						activePanel.Hide();
						activePanel.SetFocus(false);
					}
				}

				activePanel = value;

				if (activePanel != null)
				{
					activePanel.Show();
					activePanel.BringToFront();
					activePanel.SetFocus(true);

					ShowButtons();
					SetWindowText();
				}

				Invalidate();
			}
		}

		/// <summary>
		/// Gets the top container of the current hierarchy.
		/// </summary>
		[Browsable(false)]
		public DockContainer TopLevelContainer
		{
			get
			{
				DockContainer c = this;

				while ((c.Parent is DockContainer) | (c.Parent is DockManager))
				{
					c = c.Parent as DockContainer;
				}

				return c;
			}
		}

        /// <summary>
        /// Gets the number of panels in the container.
        /// </summary>
        [Browsable(false)]
        public int PanelCount
        {
            get { return panList.Count; }
        }
		#endregion

		#region Paint
		/// <summary>
		/// Overrides the base class function.
		/// Draws borders and tabs.
		/// </summary>
		/// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Graphics g = e.Graphics;

			HatchBrush dimBrush = new HatchBrush(HatchStyle.Percent50, Color.Black, Color.Transparent);
			Pen penBlack = new Pen(Color.Black);
			Pen penWhite = new Pen(Color.White);
			float n = 0;
			int y = 0;
			int h = 16;
			int y0 = 0;

			StringFormat sf = StringFormat.GenericDefault;
			sf.Trimming = StringTrimming.EllipsisCharacter;

			if (fading)
			{
				if (fadeBkImage != null)
					g.DrawImage(fadeBkImage, Point.Empty);

				g.DrawImage(fadeImage, fadeLocation.X, fadeLocation.Y);
			}
			else if (isDragContainer)
			{
				#region Drag container
				g.FillRectangle(dimBrush, this.ClientRectangle);
				#endregion
			}
			else if (HideContainer)
			{
				#region Do nothing at all.
				return;
				#endregion
			}
			else if (panList.Count == 0)
			{
				Rectangle rc = new Rectangle(dockOffsetL, dockOffsetT, this.Width - dockOffsetL - dockOffsetR, this.Height - dockOffsetT - dockOffsetB);
				g.FillRectangle(SystemBrushes.ControlDark, rc);
			}
			else if (dockType == DockContainerType.ToolWindow)
			{
				if (DockManager.Style == DockVisualStyle.VS2003)
				{
					DrawToolWndVS2003(g, penBlack, penWhite, ref n, ref y, ref h, ref y0, sf);
				}
				else if (DockManager.Style == DockVisualStyle.VS2005)
				{
					DrawToolWndVS2005(g, penBlack, penWhite, ref n, ref y, ref h, ref y0, sf);
				}
			}
			else if (dockType == DockContainerType.Document)
			{
				if (DockManager.Style == DockVisualStyle.VS2003)
				{
					DrawDocWndVS2003(g, penBlack, penWhite, ref n, ref y, ref h, ref y0, sf);
				}
				else if (DockManager.Style == DockVisualStyle.VS2005)
				{
					DrawDocWndVS2005(g, penBlack, penWhite, ref n, ref y, ref h, ref y0, sf);
				}
			}
		}

		private void DrawDocWndVS2003(Graphics g, Pen penBlack, Pen penWhite, ref float n, ref int y, ref int h, ref int y0, StringFormat sf)
		{
			#region Document drawing - VS2003
			Brush textBrush;
			Font textFont;
			y0 = DockPadding.Top - bottomDock;
			this.BackColor = SystemColors.Control;

			// Draw bounding rectangle.
			g.DrawRectangle(SystemPens.ControlDark, dockOffsetL, y0 + dockOffsetT, this.Width - 1 - dockOffsetL - dockOffsetR, this.Height - 1 - y0 - dockOffsetT - dockOffsetB);

			// Draw background rectangle.
			g.FillRectangle(new SolidBrush(Color.FromArgb(247, 243, 233)), 1, y0 + 1, this.Width - 2 - dockOffsetL - dockOffsetR, bottomDock - 3);
			g.DrawLine(penWhite, 1 + dockOffsetL, y0 + bottomDock - 3, this.Width - 2 - dockOffsetR, y0 + bottomDock - 3);

			// Draw each header tab.
			n = DockPadding.Left - 1 + 4;
			y = 3;
			h = -bottomDock + 6;
			int xMax = Width - 18 - 2 * 14;
			panelOverflow = false;

			foreach (DockPanel panel in panList)
			{
				if (panList.IndexOf(panel) < panelOffset)
				{
					panel.TabRect = RectangleF.Empty;
					continue;
				}

				if ((panel == activePanel) && (this.ContainsFocus))
					textFont = new Font(Font, FontStyle.Bold);
				else
					textFont = Font;

				SizeF panelSize = MeasurePanel(panel, g, false, textFont);
				panel.TabRect = new RectangleF(n, y0 + 3, panelSize.Width, (float)(bottomDock - 4));

				if (n + (int)panelSize.Width >= xMax - 2)
				{
					panelOverflow = true;
					panelSize.Width = xMax - n - 2;
				}

				if (panel == activePanel)
				{
					// Active panel.
					g.FillRectangle(SystemBrushes.Control, panel.TabRect);
					// ...left line
					g.DrawLine(penBlack, n + panelSize.Width, y0 + y, n + panelSize.Width, y0 + y - h);
					// ...long line
					g.DrawLine(penWhite, n + 1, y0 + y, n + panelSize.Width, y0 + y);
					// ...right line
					g.DrawLine(penWhite, n, y0 + y, n, y0 + y - h);
					// Set text brush and font.
					textBrush = new SolidBrush(Color.Black);

				}
				else
				{
					// Inactive panel.
					int i = panList.IndexOf(panel) + 1;
					if (i != panList.IndexOf(activePanel))
						g.DrawLine(new Pen(Color.FromArgb(128, 128, 128)), n + panelSize.Width - 1, y0 + y + 2, n + panelSize.Width - 1, y0 + y - h - 2);
					// Set text brush and font.
					textBrush = new SolidBrush(Color.FromArgb(85, 85, 85));
				}

				// Panel icon.
				RectangleF rc = new RectangleF(n + 3, y0 + y - 1 + (bottomDock - 2 - panelSize.Height) / 2, panelSize.Width - 6, panelSize.Height);

				if (showIcons && (panel.Form.Icon != null))
				{
					g.DrawIcon(panel.Form.Icon, new Rectangle((int)rc.X, y0 - 1 + (int)(y + (bottomDock - 2 - 16) / 2), 16, 16));
					rc.Offset(panelSize.Height + 3, 0);
					rc.Width -= panelSize.Height + 3;
				}

				// Panel text.
				g.DrawString(panel.Form.Text, textFont, textBrush, rc, sf);

				n += (int)panelSize.Width;

				if (panelOverflow)
					break;
			}

			if (panelOverflow)
			{
				// Redraw background.
				g.FillRectangle(new SolidBrush(Color.FromArgb(247, 243, 233)), xMax - 2, y0 + 1, Width - 2 - xMax, bottomDock - 3);

				// Activate buttons.
				if (panelOffset < panList.Count - 1)
					btnTabR.Enabled = true;
				else
					btnTabR.Enabled = false;
			}
			else
			{
				btnTabR.Enabled = false;
			}
			#endregion
		}

		private void DrawDocWndVS2005(Graphics g, Pen penBlack, Pen penWhite, ref float n, ref int y, ref int h, ref int y0, StringFormat sf)
		{
			#region Document drawing - VS2005
			Brush textBrush;
			Font textFont;
			y0 = DockPadding.Top - bottomDock;
			this.BackColor = SystemColors.Control;

			g.Clear(SystemColors.Control);

			sf.Alignment = StringAlignment.Center;
			showIcons = false;

			// Draw bounding rectangle.
			g.DrawRectangle(SystemPens.ControlDark, dockOffsetL, dockOffsetT, this.Width - 1 - dockOffsetL - dockOffsetR, this.Height - 1 - dockOffsetT - dockOffsetB);

			// Draw background rectangle.
			g.DrawLine(SystemPens.ControlDark, 1 + dockOffsetL, y0 + bottomDock - 4, Width - 2 - dockOffsetR, y0 + bottomDock - 4);

			// Draw each header tab.
			n = DockPadding.Left - 1 + 3;
			y = 6;
			h = -bottomDock + 9;
			int xMax = this.Width - 8 - 3 * 14;
			panelOverflow = false;

			for (int i = 0; i < panList.Count; i++)
			{
				DockPanel panel = panList[i] as DockPanel;

				if (i < panelOffset)
				{
					panel.TabRect = RectangleF.Empty;
					continue;
				}

				textFont = new Font(this.Font, FontStyle.Bold);

				SizeF panelSize = MeasurePanel(panel, g, false, textFont);

				if (n + (int)panelSize.Width >= xMax - 2)
				{
					panelOverflow = true;
					panelSize.Width = xMax - n - 2;
				}

				// Create graphic paths
				GraphicsPath path = new GraphicsPath();
				// ...right border
				path.AddLine(n + panelSize.Width + 6, y0 + y + 2, n + panelSize.Width + 6, y0 + y - h - 1);
				// ...top border
				path.AddLine(n + panelSize.Width + 6, y0 + y + 2, n + panelSize.Width + 4, y0 + y);
				path.AddLine(n + panelSize.Width + 4, y0 + y, n + 18, y0 + y);
				path.AddLine(n + 18, y0 + y, n + 13, y0 + y + 3);
				// ...right line
				path.AddLine(n, y0 + bottomDock - 4, n + 13, y0 + y + 3);

				GraphicsPath closedPath = path.Clone() as GraphicsPath;
				// ...bottom line
				closedPath.AddLine(n, y0 + bottomDock - 4, n + panelSize.Width + 6, y0 + bottomDock - 4);

				panel.TabRect = closedPath.GetBounds();

				if (panel == activePanel)
					g.FillPath(new SolidBrush(Color.White), closedPath);
				else
					g.FillPath(SystemBrushes.Control, closedPath);

				g.DrawPath(SystemPens.ControlDark, path);

				// Setup text.
				if ((panel == activePanel) && (this.ContainsFocus))
					textFont = new Font(this.Font, FontStyle.Bold);
				else
					textFont = new Font(this.Font, FontStyle.Regular);

				textBrush = new SolidBrush(Color.Black);

				// Panel text.
				RectangleF rc = new RectangleF(n + 12, y0 + y - 3 + (bottomDock - 2 - panelSize.Height) / 2, panelSize.Width - 10, panelSize.Height);
				g.DrawString(panel.Form.Text, textFont, textBrush, rc, sf);

				n += (int)panelSize.Width;

				if (panelOverflow)
					break;
			}

			if (panelOverflow)
			{
				// Redraw background.
				g.FillRectangle(SystemBrushes.Control, xMax, y0 + 4, Width - 2 - xMax, bottomDock - 8);

				// Activate buttons.
				if (panelOffset < panList.Count - 1)
					btnTabR.Enabled = true;
				else
					btnTabR.Enabled = false;
			}
			else
			{
				btnTabR.Enabled = false;
			}

			sf.Alignment = StringAlignment.Near;
			#endregion
		}

		private void DrawToolWndVS2003(Graphics g, Pen penBlack, Pen penWhite, ref float n, ref int y, ref int h, ref int y0, StringFormat sf)
		{
			#region Toolbox drawing - VS2003
			Brush textBrush;
			int w = Width - DockPadding.Left - DockPadding.Right + 2;
			float hTitle = 0;
			y0 = DockPadding.Top - topDock;

			this.BackColor = SystemColors.Control;

			// Draw top bar.
			if (!IsRootContainer)
			{
				if (this.ContainsFocus)
				{
					// active
					g.FillRectangle(SystemBrushes.ActiveCaption, DockPadding.Left - 1, y0 + 3, DockPadding.Left - 1 + w, y0 + h);
					textBrush = SystemBrushes.ActiveCaptionText;

					btnClose.BackColor = SystemColors.ActiveCaption;
					btnClose.ForeColor = SystemColors.ActiveCaptionText;
					btnClose.Invalidate();
				}
				else
				{
					// inactive
					g.FillRectangle(SystemBrushes.Control, DockPadding.Left - 1, y0 + 3, DockPadding.Left - 1 + w, y0 + h);
					g.DrawLine(SystemPens.ControlDark, DockPadding.Left, y0 + 3, DockPadding.Left - 1 + w - 2, y0 + 3);
					g.DrawLine(SystemPens.ControlDark, DockPadding.Left - 1, y0 + 4, DockPadding.Left - 1, y0 + h + 1);
					g.DrawLine(SystemPens.ControlDark, DockPadding.Left, y0 + h + 2, DockPadding.Left - 1 + w - 2, y0 + h + 2);
					g.DrawLine(SystemPens.ControlDark, DockPadding.Left - 1 + w - 1, y0 + 4, DockPadding.Left - 1 + w - 1, y0 + h + 1);
					textBrush = SystemBrushes.ControlText;

					btnClose.BackColor = SystemColors.Control;
					btnClose.ForeColor = SystemColors.ControlText;
					btnClose.Invalidate();
				}

				// Draw top bar string.
				if (activePanel != null)
				{
					hTitle = g.MeasureString(activePanel.Form.Text, Font).Height;
					g.DrawString(activePanel.Form.Text, Font, textBrush, DockPadding.Left - 1 + 2, y0 + (topDock - hTitle) / 2 + 1, sf);
				}
			}

			// Draw panel border.
			g.DrawRectangle(SystemPens.ControlDark, DockPadding.Left - 1, y0 + 21, w - 1, Height - DockPadding.Top - DockPadding.Bottom + 1);

			// Draw bottom bar, if needed.
			if (panList.Count > 1)
			{
				// Background rectangle.
				g.FillRectangle(new SolidBrush(Color.FromArgb(247, 243, 233)), DockPadding.Left - 1 + 1, Height - DockPadding.Bottom + 3, w - 2, bottomDock - 3);
				g.DrawLine(penBlack, DockPadding.Left - 1 + 1, Height - DockPadding.Bottom + 3, w - 2, Height - DockPadding.Bottom + 3);

				n = DockPadding.Left - 1 + 4;
				y = Height - DockPadding.Bottom - 2 + bottomDock;
				h = bottomDock - 5;

				foreach (DockPanel panel in panList)
				{
					SizeF panelSize = MeasurePanel(panel, g, true, Font);
					panel.TabRect = new RectangleF(n, (float)(Height - DockPadding.Bottom + 2), panelSize.Width, (float)(bottomDock - 4));

					if (panel == activePanel)
					{
						// Active panel.
						g.FillRectangle(SystemBrushes.Control, panel.TabRect);
						// ...left line
						g.DrawLine(penBlack, n + panelSize.Width, y, n + panelSize.Width, y - h);
						// ...long line
						g.DrawLine(penBlack, n + 1, y, n + panelSize.Width, y);
						// ...right line
						g.DrawLine(penWhite, n, y, n, y - h);
						// Set text brush.
						textBrush = new SolidBrush(Color.Black);
					}
					else
					{
						// Inactive panel.
						int i = panList.IndexOf(panel) + 1;
						if (i != panList.IndexOf(activePanel))
							g.DrawLine(new Pen(Color.FromArgb(128, 128, 128)), n + panelSize.Width - 1, y - 2, n + panelSize.Width - 1, y - h + 3);
						textBrush = new SolidBrush(Color.FromArgb(85, 85, 85));
					}

					// Panel icon.
					RectangleF rc = new RectangleF(n + 3, y + 3 + (-bottomDock + 2 - panelSize.Height) / 2, panelSize.Width - 6, panelSize.Height);

					if (showIcons && (panel.Form.Icon != null))
					{
						g.DrawIcon(panel.Form.Icon, new Rectangle((int)rc.X + 1, (int)(y + 2 + (-bottomDock + 2 - 16) / 2), 16, 16));
						rc.Offset(panelSize.Height + 3, 0);
						rc.Width -= panelSize.Height + 3;
					}

					// Panel text.
					g.DrawString(panel.Form.Text, Font, textBrush, rc, sf);

					n += (int)panelSize.Width;
				}
			}
			#endregion
		}

		private void DrawToolWndVS2005(Graphics g, Pen penBlack, Pen penWhite, ref float n, ref int y, ref int h, ref int y0, StringFormat sf)
		{
			#region Toolbox drawing - VS2005
			Brush textBrush;
			int w = Width - DockPadding.Left - DockPadding.Right + 2;
			float hTitle = 0;
			y0 = DockPadding.Top - topDock;

			this.BackColor = SystemColors.Control;
			g.Clear(Color.White);

			// Draw top bar.
			if (!IsRootContainer)
			{
				if (this.ContainsFocus)
				{
					// active
					g.FillRectangle(SystemBrushes.ActiveCaption, DockPadding.Left - 1, y0, DockPadding.Left - 1 + w, y0 + h);
					textBrush = SystemBrushes.ActiveCaptionText;

					btnAutoHide.BackColor = btnMenu.BackColor = btnClose.BackColor = SystemColors.ActiveCaption;
					btnAutoHide.ForeColor = btnMenu.ForeColor = btnClose.ForeColor = SystemColors.ActiveCaptionText;
				}
				else
				{
					// inactive
					g.FillRectangle(SystemBrushes.ControlDark, DockPadding.Left - 1, y0, DockPadding.Left - 1 + w, y0 + h);
					textBrush = SystemBrushes.Control;

					btnAutoHide.BackColor = btnMenu.BackColor = btnClose.BackColor = SystemColors.ControlDark;
					btnAutoHide.ForeColor = btnMenu.ForeColor = btnClose.ForeColor = SystemColors.Control;
				}

				btnClose.Invalidate();
				btnMenu.Invalidate();
				btnAutoHide.Invalidate();

				// Draw top bar string.
				if (activePanel != null)
				{
					hTitle = g.MeasureString(activePanel.Form.Text, Font).Height;
					g.DrawString(activePanel.Form.Text, Font, textBrush, DockPadding.Left - 1 + 2, y0 + (topDock - hTitle) / 2, sf);
				}
			}

			// Draw panel border.
			g.DrawRectangle(SystemPens.ControlDark, DockPadding.Left - 1, y0 + topDock - 1, w - 1, Height - DockPadding.Top - DockPadding.Bottom + 1);

			// Draw bottom bar, if needed.
			if ((panList.Count > 1) & !autoHide)
			{
				// Background rectangle.
				DockContainer host = this.TopLevelContainer;
				Rectangle rcHost = host.RectangleToScreen(host.ClientRectangle);
				Rectangle rcThis = this.RectangleToScreen(this.ClientRectangle);
				rcHost.X -= rcThis.X;
				rcHost.Y -= rcThis.Y;
				Rectangle rcBg = new Rectangle(DockPadding.Left - 1, Height - DockPadding.Bottom + 3, w + 1, bottomDock - 3);
				LinearGradientBrush gradBrush = new LinearGradientBrush(rcHost, SystemColors.Control, Color.White, LinearGradientMode.Horizontal);
				g.FillRectangle(gradBrush, rcBg);
				g.DrawLine(SystemPens.ControlDark, DockPadding.Left - 1, Height - DockPadding.Bottom + 3, w - 1, Height - DockPadding.Bottom + 3);

				n = DockPadding.Left - 1 + 4;
				y = this.Height - this.DockPadding.Bottom - 2 + bottomDock;
				h = bottomDock - 5;

				foreach (DockPanel panel in panList)
				{
					SizeF panelSize = MeasurePanel(panel, g, true, Font);
					panel.TabRect = new Rectangle((int)n + 1, Height - DockPadding.Bottom + 2, (int)panelSize.Width - 2, bottomDock - 4);

					int x2 = (int)(n + panelSize.Width) - 1;

					if (panel == activePanel)
					{
						// Active panel.
						g.FillRectangle(new SolidBrush(Color.White), panel.TabRect);
						// ...left line
						g.DrawLine(SystemPens.ControlDark, x2, y - 2, x2, y - h);
						// ...long line
						g.DrawLine(SystemPens.ControlDark, n + 2, y, x2 - 2, y);
						// ...right line
						g.DrawLine(SystemPens.ControlDark, n, y - 2, n, y - h);
						// Punkte
						g.DrawLine(SystemPens.ControlDark, n, y - 2, n + 2, y);
						g.DrawLine(SystemPens.ControlDark, x2, y - 2, x2 - 2, y);
						// Set text brush.
						textBrush = new SolidBrush(Color.Black);
					}
					else
					{
						// Inactive panel.
						int i = panList.IndexOf(panel) + 1;
						if ((i != panList.IndexOf(activePanel)) & (i != panList.Count))
							g.DrawLine(SystemPens.ControlDark, x2 - 2, y - 4, x2 - 2, y - h + 3);
						textBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
					}

					// Panel icon.
					RectangleF rc = new RectangleF(n + 3, y + 2 + (-bottomDock + 2 - panelSize.Height) / 2, panelSize.Width - 6, panelSize.Height);

					if (showIcons && (panel.Form.Icon != null))
					{
						g.DrawIcon(panel.Form.Icon, new Rectangle((int)rc.X + 1, (int)(y + 2 + (-bottomDock + 2 - 16) / 2), 16, 16));
						rc.Offset(panelSize.Height + 3, 0);
						rc.Width -= panelSize.Height + 3;
					}

					// Panel text.
					g.DrawString(panel.Form.Text, Font, textBrush, rc, sf);

					n += (int)panelSize.Width;
				}
			}
			#endregion
		}

		/// <summary>
		/// Measures the size of a <see cref="DockPanel"/> tab depending on a given font.
		/// </summary>
		/// <param name="panel">The <see cref="DockPanel"/> object.</param>
		/// <param name="graphics">The <see cref="Graphics"/> interface.</param>
		/// <param name="cut">Enables the reduction of the panel size according to the available space.</param>
		/// <param name="font">The target font.</param>
		/// <returns>The size of the panel tab.</returns>
		private SizeF MeasurePanel(DockPanel panel, Graphics graphics, bool cut, Font font)
		{
			SizeF ret = graphics.MeasureString(panel.Form.Text, font);

			if (font.Bold)
				ret.Width += 12;
			else
				ret.Width += 6;

			if (showIcons && (panel.Form.Icon != null))
				ret.Width += ret.Height + 3;

			if ((ret.Width > (this.Width - DockPadding.Left - DockPadding.Right + 2 - 8) / panList.Count) && cut)
				ret.Width = (this.Width - DockPadding.Left - DockPadding.Right + 2 - 8) / panList.Count;

			return ret;
		}

		/// <summary>
		/// Overrides the base class function.
		/// Invokes an Invalidate after calling the base class function.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			if ((activePanel != null) && !blockFocusEvents)
				activePanel.SetFocus(true);
			Invalidate();
		}

		/// <summary>
		/// Overrides the base class function.
		/// Invokes an Invalidate after calling the base class function.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			if ((activePanel != null) && !blockFocusEvents)
				activePanel.SetFocus(false);
			Invalidate();
		}

		/// <summary>
		/// Overrides the base class function.
		/// Invokes an Invalidate after calling the base class function.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			Invalidate();
		}

		private void dragPanel_Paint(object sender, PaintEventArgs e)
		{
			if (DockManager.Style == DockVisualStyle.VS2005)
			{
				Graphics g = e.Graphics;

				DockContainer host = this.TopLevelContainer;
				Rectangle rcHost = host.RectangleToScreen(host.ClientRectangle);
				Rectangle rcThis = dragPanel.RectangleToScreen(dragPanel.ClientRectangle);
				rcHost.X -= rcThis.X;
				rcHost.Y -= rcThis.Y;
				LinearGradientBrush gradBrush = new LinearGradientBrush(rcHost, SystemColors.Control, Color.White, LinearGradientMode.Horizontal);
				g.FillRectangle(gradBrush, dragPanel.ClientRectangle);
			}
		}
		#endregion

		#region Panel management
		/// <summary>
		/// Determines if a point is contained in the client region.
		/// </summary>
		/// <param name="pt">The point to test.</param>
		/// <returns>True, if the point is in the client region.</returns>
		protected bool HitTest(Point pt)
		{
			try
			{
				Rectangle rc = RectangleToScreen(this.ClientRectangle);

				if (this.IsRootContainer)
					rc = this.Parent.Bounds;

				return rc.Contains(pt);
			}
			catch (Exception e)
			{
				Console.WriteLine("DockContainer.HitTest: " + e.Message);
				return false;
			}
		}

		/// <summary>
		/// Retrieves a container that matches a specific mouse location, when a window is dragged.
		/// </summary>
		/// <param name="srcSize">The size of the source object.</param>
		/// <param name="type">The target container type.</param>
		/// <param name="pt">The target position.</param>
		/// <returns>A valid container or null, if no suitable container was found.</returns>*/
		internal DockContainer GetTarget(Size srcSize, DockContainerType type, Point pt)
		{
			// Prepare rectangles.
			Rectangle rcClient = RectangleToScreen(this.ClientRectangle);
			Rectangle rcDock = new Rectangle(rcClient.Left + this.DockPadding.Left,
				rcClient.Top + this.DockPadding.Top,
				rcClient.Width - this.DockPadding.Left - this.DockPadding.Right,
				rcClient.Height - this.DockPadding.Top - this.DockPadding.Bottom);

			// Test on split.
			if (rcDock.Contains(pt))
			{
				DockContainer cont = null;

				if (pt.X - rcDock.Left <= dockBorder)
				{
					// Split left.
					cont = new DockContainer();
					cont.DockType = type;
					cont.Dock = DockStyle.Left;
					if (srcSize.Width > this.Width / 2)
						cont.Width = this.Width / 2;
					else
						cont.Width = srcSize.Width;
					cont.Height = this.Height;
					cont.Location = new Point(rcClient.X, rcClient.Y);
				}
				else if (rcDock.Right - pt.X <= dockBorder)
				{
					// Split right.
					cont = new DockContainer();
					cont.DockType = type;
					cont.Dock = DockStyle.Right;
					if (srcSize.Width > this.Width / 2)
						cont.Width = this.Width / 2;
					else
						cont.Width = srcSize.Width;
					cont.Height = this.Height;
					cont.Location = new Point(rcClient.X + this.Width - cont.Width, rcClient.Y);
				}
				else if (pt.Y - rcDock.Top <= dockBorder)
				{
					// Split top.
					cont = new DockContainer();
					cont.DockType = type;
					cont.Dock = DockStyle.Top;
					if (srcSize.Height > this.Height / 2)
						cont.Height = this.Height / 2;
					else
						cont.Height = srcSize.Height;
					cont.Width = this.Width;
					cont.Location = new Point(rcClient.X, rcClient.Y);
				}
				else if (rcDock.Bottom - pt.Y <= dockBorder)
				{
					// Split bottom.
					cont = new DockContainer();
					cont.DockType = type;
					cont.Dock = DockStyle.Bottom;
					if (srcSize.Height > this.Height / 2)
						cont.Height = this.Height / 2;
					else
						cont.Height = srcSize.Height;
					cont.Width = this.Width;
					cont.Location = new Point(rcClient.X, rcClient.Y + this.Height - cont.Height);
				}

				return cont;
			}

			// For direct children of DockWindows only:
			if (this.IsRootContainer)
			{
				DockForm wnd = this.Parent as DockForm;
				if (wnd.Bounds.Contains(pt) & (dockType == type))
					return this;
			}

			// Test on add to own panel list.
			if (rcClient.Contains(pt))
			{
				if ((pt.Y <= rcClient.Top + this.DockPadding.Top) && (dockType == type))
					return this;
				else if ((dockType == type) && (dockType == DockContainerType.ToolWindow) && (panList.Count > 1) && (pt.Y > rcClient.Top + this.Height - this.DockPadding.Bottom))
					return this;
			}

			return null;
		}

		/// <summary>
		/// Selects a specific tab beginning with 0 from left to the right.
		/// </summary>
		/// <param name="i">The index of the tab.</param>
		public void SelectTab(int i)
		{
			try
			{
				if (panList[i] != null)
					ActivePanel = panList[i] as DockPanel;
			}
			catch (Exception e)
			{
				Console.WriteLine("DockContainer.SelectTab: " + e.Message);
			}
		}

		/// <summary>
		/// Selects a specific tab based on its reference pointer.
		/// </summary>
		/// <param name="p">The tab reference.</param>
		public void SelectTab(DockPanel p)
		{
			try
			{
				if ((panList != null) && (p != null))
				{
					if (!panList.Contains(p))
						return;

					ActivePanel = p;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("DockContainer.SelectTab: " + e.Message);
			}
		}

		internal void SetWindowText()
		{
			if ((this.TopLevelControl == null) || (activePanel == null))
				return;

			DockWindow wnd = activePanel.Form;

			if (this.TopLevelControl is DockForm)
			{
				DockForm form = this.TopLevelControl as DockForm;
				wnd.CopyPropToDockForm(form);
				SetFormSizeBounds(form);
			}
			else
				Invalidate();

			if (autoHide)
				hideStorage.manager.Invalidate();
		}

		internal void SetFormSizeBounds(DockForm form)
		{
			Size minSize = Size.Empty;
			Size maxSize = Size.Empty;

			foreach (DockPanel p in panList)
			{
				if (!p.MinFormSize.IsEmpty)
				{
					if (p.MinFormSize.Width > minSize.Width)
						minSize.Width = p.MinFormSize.Width;
					if (p.MinFormSize.Height > minSize.Height)
						minSize.Height = p.MinFormSize.Height;
				}

				if (!p.MaxFormSize.IsEmpty)
				{
					if (p.MaxFormSize.Width < maxSize.Width)
						maxSize.Width = p.MaxFormSize.Width;
					if (p.MaxFormSize.Height < maxSize.Height)
						maxSize.Height = p.MaxFormSize.Height;
				}
			}

			try
			{
				Size paddingSize = new Size(this.Padding.Left + this.Padding.Right, this.Padding.Top + this.Padding.Bottom);

				if (minSize.IsEmpty)
					form.MinimumSize = Size.Empty;
				else
					form.MinimumSize = Size.Add(minSize, paddingSize);

				if (maxSize.IsEmpty)
					form.MaximumSize = Size.Empty;
				else
					form.MaximumSize = Size.Add(maxSize, paddingSize);
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockContainer.SetFormSizeBounds : " + ex.Message);
			}
		}
		#endregion

		#region Control management
		/// <summary>
		/// Overrides the base class function.
		/// Before adding the control, the internal lists are updated depending on the type.
		/// </summary>
		/// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
		protected override void OnControlAdded(ControlEventArgs e)
		{
			if (disableOnControlAdded)
			{
				if (e.Control is DockContainer)
					conList.Add(e.Control);
				else if (e.Control is DockPanel)
					panList.Add(e.Control);

				return;
			}

			if ((e.Control is DockPanel) && (e.Control != dragPanel))
			{
				// Add the panel to the list object and show buttons.
				panList.Add(e.Control);
				ShowButtons();

				//activePanel = null;
				ActivePanel = e.Control as DockPanel;
			}
			else if (e.Control is FlatButton)
			{
				// Show or hide buttons.
				if (panList.Count > 0)
					ShowButtons();
				else
					HideButtons();
			}
			else if (e.Control is DockContainer)
			{
				// Check, if the container is a drag dummy and add the container to the internal list.
				if (!(e.Control as DockContainer).isDragContainer)
					conList.Add(e.Control);
			}

			// Adjust the borders of the container.
			AdjustBorders();

			// Raise event and redraw client area.
			base.OnControlAdded(e);
			Invalidate();
		}

		/// <summary>
		/// Overrides the base class function.
		/// Before removing the control, the internal lists are updated depending on the type.
		/// </summary>
		/// <param name="e">A <see cref="ControlEventArgs"/> that contains the event data.</param>
		protected override void OnControlRemoved(ControlEventArgs e)
		{
			if (disableOnControlRemove)
			{
				if (e.Control is DockContainer)
					conList.Remove(e.Control);
				else if (e.Control is DockPanel)
					panList.Remove(e.Control);

				return;
			}

			if ((e.Control is DockPanel) && (e.Control != dragPanel))
			{
				// Remove panel from list object.
				panList.Remove(e.Control);

				// Show or hide buttons.
				if (panList.Count == 0)
					HideButtons();
				else
					ShowButtons();
			}
			else if (e.Control is DockContainer)
			{
				// Check, if the container is a drag dummy.
				if ((e.Control as DockContainer).isDragContainer)
					return;

				// Remove container from list object.
				conList.Remove(e.Control);

				if (conList.Count == 1)
				{
					// Remove the last container and copy all panels.
					// -> Revert the encapsulation into the second container.
					DockContainer cont = conList[0] as DockContainer;
					cont.disableOnControlRemove = true;

					this.Controls.Remove(cont);
					this.DockType = cont.DockType;
					removeable = cont.removeable;

					this.Controls.AddRange((DockContainer[])cont.conList.ToArray(typeof(DockContainer)));
					this.Controls.AddRange((DockPanel[])cont.panList.ToArray(typeof(DockPanel)));

					cont.Dispose();
					cont = null;
				}
			}

			// Adjust the borders of the container.
			AdjustBorders();

			// Retrieve the active panel.
			//activePanel = null;
			if (panList.Count > 0)
				ActivePanel = (DockPanel)panList[panList.Count - 1];

			// Raise event and redraw client area.
			base.OnControlRemoved(e);
			Invalidate();
		}

		/// <summary>
		/// Adjusts the <see cref="ScrollableControl.DockPadding"/> property and drag panel according to the state of the container.
		/// </summary>
		internal void AdjustBorders()
		{
			int topOff = 0;
			int bottomOff = 0;
			int sideOff = 0;

			if (HideContainer)
			{
				this.DockPadding.All = 0;

				dragPanel.Hide();

				Invalidate();
				return;
			}

			if ((panList.Count != 0) | !removeable)
			{
				if (dockType == DockContainerType.ToolWindow)
				{
					if (IsRootContainer)
						topOff = 1;
					else
						topOff = topDock;
					if ((panList.Count > 1) & !autoHide)
						bottomOff = bottomDock;
					else
						bottomOff = 1;
					sideOff = 1;
				}
				else if (dockType == DockContainerType.Document)
				{
					topOff = bottomDock - 3;
					bottomOff = 2;
					sideOff = 2;
				}
			}

			this.DockPadding.Top = topOff + dockOffsetT;
			this.DockPadding.Bottom = bottomOff + dockOffsetB;
			this.DockPadding.Right = sideOff + dockOffsetR;
			this.DockPadding.Left = sideOff + dockOffsetL;

			switch (this.Dock)
			{
				case DockStyle.Left:
					this.DockPadding.Right += splitterWidth;
					dragPanel.Height = this.Height;
					dragPanel.Width = splitterWidth;
					dragPanel.Location = new Point(this.Width - splitterWidth, 0);
					dragPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
					dragPanel.Cursor = Cursors.VSplit;
					dragPanel.Show();
					break;
				case DockStyle.Right:
					this.DockPadding.Left += splitterWidth;
					dragPanel.Height = this.Height;
					dragPanel.Width = splitterWidth;
					dragPanel.Location = new Point(0, 0);
					dragPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
					dragPanel.Cursor = Cursors.VSplit;
					dragPanel.Show();
					break;
				case DockStyle.Top:
					this.DockPadding.Bottom += splitterWidth;
					dragPanel.Height = splitterWidth;
					dragPanel.Width = this.Width;
					dragPanel.Location = new Point(0, this.Height - splitterWidth);
					dragPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
					dragPanel.Cursor = Cursors.HSplit;
					dragPanel.Show();
					break;
				case DockStyle.Bottom:
					this.DockPadding.Top += splitterWidth;
					dragPanel.Height = splitterWidth;
					dragPanel.Width = this.Width;
					dragPanel.Location = new Point(0, 0);
					dragPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
					dragPanel.Cursor = Cursors.HSplit;
					dragPanel.Show();
					break;
				case DockStyle.Fill:
					dragPanel.Hide();
					break;
				default:
					dragPanel.Hide();
					break;
			}

			if (dockType == DockContainerType.Document)
			{
				if (DockManager.Style == DockVisualStyle.VS2003)
				{
					btnTabR.Size = btnTabL.Size = btnClose.Size = new Size(14, 15);
					btnTabR.ForeColor = btnTabL.ForeColor = btnClose.ForeColor = Color.FromArgb(85, 85, 85);
					btnTabR.BackColor = btnTabL.BackColor = btnClose.BackColor = Color.FromArgb(247, 243, 233);
					btnTabR.LightColor = btnTabL.LightColor = btnClose.LightColor = SystemColors.Control;

					btnClose.Location = new Point(this.Width - 18 - dockOffsetR, this.DockPadding.Top - bottomDock + 5);
					btnTabR.Location = new Point(this.Width - 18 - 14 - dockOffsetR, this.DockPadding.Top - bottomDock + 5);
					btnTabL.Location = new Point(this.Width - 18 - 2 * 14 - dockOffsetR, this.DockPadding.Top - bottomDock + 5);
					btnMenu.Hide();
				}
				else if (DockManager.Style == DockVisualStyle.VS2005)
				{
					btnMenu.Size = btnClose.Size = new Size(14, 13);
					btnMenu.ForeColor = btnClose.ForeColor = SystemColors.ControlText;
					btnMenu.BackColor = btnClose.BackColor = SystemColors.Control;
					btnMenu.LightColor = btnClose.LightColor = Color.FromArgb(10, 36, 106);
					btnMenu.ShadowColor = btnClose.ShadowColor = Color.FromArgb(10, 36, 106);
					btnMenu.SelectColor = btnClose.SelectColor = Color.FromArgb(50, 10, 36, 106);

					btnClose.Location = new Point(this.Width - 18 - dockOffsetR, this.DockPadding.Top - bottomDock + 7);
					btnTabR.Hide();
					btnTabL.Hide();
					btnMenu.Location = new Point(this.Width - 18 - 14 - dockOffsetR, this.DockPadding.Top - bottomDock + 7);
				}

				btnAutoHide.Hide();
			}
			else if (dockType == DockContainerType.ToolWindow)
			{
				btnAutoHide.Size = btnMenu.Size = new Size(14, 13);
				btnAutoHide.ForeColor = btnMenu.ForeColor = btnClose.ForeColor = SystemColors.ControlText;
				btnAutoHide.BackColor = btnMenu.BackColor = btnClose.BackColor = SystemColors.Control;
				btnAutoHide.LightColor = btnMenu.LightColor = btnClose.LightColor = Color.White;
				btnAutoHide.ShadowColor = btnMenu.ShadowColor = btnClose.ShadowColor = Color.White;

				if (DockManager.Style == DockVisualStyle.VS2003)
				{
					btnMenu.Hide();
					btnAutoHide.Hide();

					btnClose.Size = new Size(16, 13);
					btnClose.Location = new Point(this.Width - this.DockPadding.Right - 18, this.DockPadding.Top - topDock + 5);
				}
				else if (DockManager.Style == DockVisualStyle.VS2005)
				{
					btnMenu.Location = new Point(this.Width - this.DockPadding.Right - 15 - 15 - 15, this.DockPadding.Top - topDock + 2);
					btnAutoHide.Location = new Point(this.Width - this.DockPadding.Right - 15 - 15, this.DockPadding.Top - topDock + 2);

					btnClose.Size = new Size(14, 13);
					btnClose.Location = new Point(this.Width - this.DockPadding.Right - 15, this.DockPadding.Top - topDock + 2);

					//if (this.TopLevelControl is DockForm)
					//	btnAutoHide.Hide();
				}
			}

			Invalidate();

			foreach (DockContainer c in conList)
				c.AdjustBorders();
		}

		/// <summary>
		/// Removes a child container from the internal list and disposes it.
		/// The other container will be transferred back to the host container at the call of the <see cref="OnControlRemoved"/> method.
		/// </summary>
		/// <param name="cont">The <see cref="DockContainer"/> that is to be removed.</param>
		protected void RemoveContainer(DockContainer cont)
		{
			if (this.Controls.Contains(cont))
			{
				this.Controls.Remove(cont);
				cont.Dispose();
				cont = null;
			}
		}

		/// <summary>
		/// Recursively resolves the <see cref="DockContainer"/> hierarchy.
		/// </summary>
		/// <param name="type">The <see cref="DockContainerType"/> of the child that is to be retrieved.</param>
		/// <param name="last">The last hit on a suitable <see cref="DockContainer"/>.</param>
		/// <returns>The child <see cref="DockContainer"/> object.</returns>
		public DockContainer GetNextChild(DockContainerType type, DockContainer last)
		{
			DockContainer ret = null;

			foreach (DockContainer cont in conList)
			{
				ret = cont.GetNextChild(type, last);
				if (ret != null)
					return ret;
			}

			if ((conList.Count == 0) && (dockType == type) && (this != last))
				ret = this;

			return ret;
		}

		/// <summary>
		/// Recursively collects all panels in the hierarchy and adds them to the list.
		/// </summary>
		/// <param name="list">The list for all panels.</param>
		internal void GetPanels(ArrayList list)
		{
			if (panList.Count > 0)
			{
				foreach (DockPanel p in panList)
					list.Add(p);
			}
			else
			{
				foreach (DockContainer c in conList)
					c.GetPanels(list);
			}
		}
		#endregion

		#region Buttons
		/// <summary>
		/// Shows all buttons depending on the type of the container.
		/// </summary>
		private void ShowButtons()
		{
			if ((activePanel != null) && activePanel.Form.AllowClose)
				btnClose.Show();
			else
				btnClose.Hide();

			if (DockManager.Style == DockVisualStyle.VS2005)
			{
				btnMenu.Show();

				if (dockType == DockContainerType.ToolWindow)
					btnAutoHide.Show();
				else
					btnAutoHide.Hide();
			}

			if ((dockType == DockContainerType.Document) & (DockManager.Style == DockVisualStyle.VS2003))
			{
				btnTabL.Show();
				btnTabR.Show();
			}
		}

		/// <summary>
		/// Hides all buttons.
		/// </summary>
		private void HideButtons()
		{
			btnClose.Hide();
			btnAutoHide.Hide();
			btnMenu.Hide();

			btnTabL.Hide();
			btnTabR.Hide();
		}

		/// <summary>
		/// The Click event handler for the close button.
		/// Closes the active panel and destoys the container, if needed.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		private void btnClose_Click(object sender, EventArgs e)
		{
			DockPanel panel = null;

			if (conList.Count > 0)
			{
				disableOnControlRemove = true;

				while (conList.Count > 0)
				{
					DockContainer c = conList[0] as DockContainer;
					c.disableOnControlRemove = true;
					c.activePanel = null;
					c.btnClose_Click(sender, e);

					if (conList.Contains(c))
						conList.Remove(c);
				}

				disableOnControlRemove = false;
			}
			else if ((panList.Count > 1) & (activePanel != null))
			{
				if (!activePanel.Form.AllowClose)
					return;

				panel = activePanel;
                panel.Form.Close();
				ReleaseWindow(panel.Form);
			}
			else
			{
                ArrayList panels = new ArrayList();
                panels.AddRange(panList);

				foreach (DockPanel p in panels)
				{
                    p.Form.Close();

                    if (panel != null)
                    {
                        ReleaseWindow(panel.Form);
                    }
				}
			}

			if ((panList.Count == 0) & (conList.Count == 0) & (removeable))
			{
				if (autoHide)
					hideStorage.manager.AutoHideContainer(this, DockStyle.Fill, false);
				else if (this.Parent is DockContainer)
					(this.Parent as DockContainer).RemoveContainer(this);
			}
		}

		/// <summary>
		/// The Click event handler for the tab left button.
		/// Switches to the next tab to the left.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		private void btnTabL_Click(object sender, EventArgs e)
		{
			panelOffset--;

			if (panelOffset == 0)
				btnTabL.Enabled = false;

			Invalidate();
		}

		/// <summary>
		/// The Click event handler for the tab right button.
		/// Switches to the next tab to the right.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		private void btnTabR_Click(object sender, EventArgs e)
		{
			panelOffset++;

			if (panelOffset > 0)
				btnTabL.Enabled = true;

			if (panelOffset == panList.Count - 1)
				btnTabR.Enabled = false;

			Invalidate();
		}

		/// <summary>
		/// The Menu event handler for the menu button.
		/// Calls the context menu.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		private void btnMenu_Click(object sender, EventArgs e)
		{
			dragObject = this;
			contextMenuStrip.Show(MousePosition);
		}

		/// <summary>
		/// The AutoHide event handler for the menu button.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		private void btnAutoHide_Click(object sender, EventArgs e)
		{
			AutoHide = !autoHide;
		}

		/// <summary>
		/// The <see cref="FlatButton.PostPaint"/> event handler of the close button.
		/// Used for custom drawing.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="PaintEventArgs"/> that contains the drawing data.</param>
		private void btnClose_PostPaint(object sender, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;

			Pen pen = new Pen(btnClose.ForeColor);
			int x0 = 0;
			int y0 = 0;

			if (btnClose.Pressed)
			{
				x0 = 1;
				y0 = 1;
			}

			if (DockManager.Style == DockVisualStyle.VS2003)
			{
				if (dockType == DockContainerType.Document)
				{
					graphics.DrawLine(pen, x0 + 3, y0 + 3, x0 + 9, y0 + 9);
					graphics.DrawLine(pen, x0 + 4, y0 + 3, x0 + 10, y0 + 9);
					graphics.DrawLine(pen, x0 + 3, y0 + 9, x0 + 9, y0 + 3);
					graphics.DrawLine(pen, x0 + 4, y0 + 9, x0 + 10, y0 + 3);
				}
				else
				{
					graphics.DrawLine(pen, x0 + 6, y0 + 3, x0 + 12, y0 + 9);
					graphics.DrawLine(pen, x0 + 5, y0 + 3, x0 + 11, y0 + 9);
					graphics.DrawLine(pen, x0 + 12, y0 + 3, x0 + 6, y0 + 9);
					graphics.DrawLine(pen, x0 + 11, y0 + 3, x0 + 5, y0 + 9);
				}
			}
			else
			{
				x0 = btnMenu.Width >> 1;
				y0 = btnMenu.Height >> 1;

				graphics.DrawLine(pen, x0 - 3, y0 + 3, x0 + 3, y0 - 3);
				graphics.DrawLine(pen, x0 - 4, y0 + 3, x0 + 2, y0 - 3);
				graphics.DrawLine(pen, x0 - 3, y0 - 3, x0 + 3, y0 + 3);
				graphics.DrawLine(pen, x0 - 4, y0 - 3, x0 + 2, y0 + 3);
			}
		}

		/// <summary>
		/// The PostPaint event handler of the tab left button.
		/// Used for custom drawing.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="PaintEventArgs"/> that contains the drawing data.</param>
		private void btnTabL_PostPaint(object sender, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;

			Pen pen = new Pen(btnTabL.ForeColor);
			int x0 = 0;
			int y0 = 0;

			if (btnTabL.Pressed)
			{
				x0 = 1;
				y0 = 1;
			}

			GraphicsPath path = new GraphicsPath();
			path.AddLine(x0 + 4, y0 + 6, x0 + 8, y0 + 2);
			path.AddLine(x0 + 8, y0 + 2, x0 + 8, y0 + 10);
			path.AddLine(x0 + 8, y0 + 10, x0 + 4, y0 + 6);

			graphics.DrawPath(pen, path);

			if (btnTabL.Enabled)
				graphics.FillPath(new SolidBrush(pen.Color), path);
		}

		/// <summary>
		/// The <see cref="FlatButton.PostPaint"/> event handler of the tab right button.
		/// Used for custom drawing.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="PaintEventArgs"/> that contains the drawing data.</param>
		private void btnTabR_PostPaint(object sender, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;

			Pen pen = new Pen(btnTabR.ForeColor);
			int x0 = 0;
			int y0 = 0;

			if (btnTabR.Pressed)
			{
				x0 = 1;
				y0 = 1;
			}

			GraphicsPath path = new GraphicsPath();
			path.AddLine(x0 + 4, y0 + 2, x0 + 4, y0 + 10);
			path.AddLine(x0 + 4, y0 + 10, x0 + 8, y0 + 6);
			path.AddLine(x0 + 8, y0 + 6, x0 + 4, y0 + 2);

			graphics.DrawPath(pen, path);

			if (btnTabR.Enabled)
				graphics.FillPath(new SolidBrush(pen.Color), path);
		}

		/// <summary>
		/// The <see cref="FlatButton.PostPaint"/> event handler of the menu button.
		/// Used for custom drawing.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="PaintEventArgs"/> that contains the drawing data.</param>
		private void btnMenu_PostPaint(object sender, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;

			Pen pen = new Pen(btnMenu.ForeColor);
			int x0 = btnMenu.Width >> 1;
			int y0 = btnMenu.Height >> 1;

			GraphicsPath path = new GraphicsPath();
			path.AddLine(x0 - 3, y0 + 1, x0 + 2, y0 + 1);
			path.AddLine(x0 + 2, y0 + 1, x0 - 0.5F, y0 + 3);
			path.AddLine(x0 - 0.5F, y0 + 3, x0 - 3, y0 + 1);

			graphics.DrawPath(pen, path);

			if (btnMenu.Enabled)
				graphics.FillPath(new SolidBrush(pen.Color), path);
		}

		/// <summary>
		/// The <see cref="FlatButton.PostPaint"/> event handler of the menu button.
		/// Used for custom drawing.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="PaintEventArgs"/> that contains the drawing data.</param>
		private void btnAutoHide_PostPaint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			Pen pen = new Pen(btnMenu.ForeColor);
			int x0 = (btnMenu.Width >> 1) - 1;
			int y0 = btnMenu.Height >> 1;

			g.TranslateTransform(x0, y0);

			if (autoHide)
				g.RotateTransform(90);

			g.DrawLine(pen, -3, 1, 3, 1);
			g.DrawLine(pen, -2, -4, 2, -4);
			g.DrawLine(pen, 0, 2, 0, 4);
			g.DrawLine(pen, -2, -3, -2, 0);
			g.DrawLine(pen, 1, -3, 1, 0);
			g.DrawLine(pen, 2, -3, 2, 0);
		}
		#endregion

		#region Drag event and docking
		/// <summary>
		/// The <see cref="DragWindow"/> event handler for dockable windows.
		/// Used to enable a <see cref="DockWindow"/> to send its position changes to this container.
		/// Calls recursively all <see cref="DragWindow"/> event handlers of the child containers.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An <see cref="DockEventArgs"/> that contains the docking data.</param>
		/// <returns>The success of the operation.</returns>
		public bool DragWindow(object sender, DockEventArgs e)
		{
			if (e.Handled)
				return true;

			try
			{
				// First, check if the given point is in the current tree and lots of more cool features.
				if ((e.DockType == DockContainerType.None) |
					((e.DockType == DockContainerType.Document) & (dockType == DockContainerType.ToolWindow)) |
					((conList.Count > 0) & !e.IgnoreHierarchy) |
					(sender == this.TopLevelControl) |
					(autoHide == true) |
					((!this.TopLevelControl.Visible) & (sender is DockForm)))
				{
					//Console.WriteLine(e.invokeCounter + "\t" + HitTest(e.Point).ToString() + "\t" + dockType.ToString() + "\t" + e.DockType.ToString() + "\t" + conList.Count + "\t" + (sender == this.TopLevelControl));
					return false;
				}

				// Last important question: am I'm moving myself or do I like getting more containers?
				if (this.TopLevelControl is DockForm)
					if ((this.TopLevelControl as DockForm).Moving | !(this.TopLevelControl as DockForm).AllowDock | !(this.TopLevelControl as DockForm).Visible)
						return false;

				// Hit test.
				if (!HitTest(e.Point))
					return false;

				DockForm f = DockManager.GetFormAtPoint(e.Point, 1);
				if ((f != this.TopLevelControl) & (f != null))
					return false;

				// Update DockGuide.
				DockManager.UpdateDockGuide(this, e);

				if (e.Handled)
					return false;

				// Get the source object size.
				Size size = Size.Empty;

				if (sender is DockForm)
					size = (sender as DockForm).Size;
				else if (sender is DockContainer)
					size = (sender as DockContainer).Size;
				else if (sender is DockWindow)
					size = (sender as DockWindow).Size;

				// Then check for valid destinations.
				e.Target = GetTarget(size, e.DockType, e.Point);

				if (e.Target == null)
					return false;

				// The event has finished.
				e.Handled = true;

				// Take the results and add the whole stuff to the own collection.
				if (e.Release)
				{
					// Transfer all own panels to a higher hierarchy level.
					if (!conList.Contains(e.Target) & (e.Target != this))
					{
						// Create new container and fill it with own panels.
						DockContainer cont = new DockContainer();
						cont.removeable = removeable;
						cont.DockBorder = dockBorder;

						removeable = true;

						DockPanel temp = ActivePanel;
						cont.Controls.AddRange((DockPanel[])panList.ToArray(typeof(DockPanel)));
						cont.ActivePanel = temp;
						cont.DockType = dockType;

						if (conList.Count > 1)
						{
							disableOnControlRemove = true;
							cont.Controls.AddRange((DockContainer[])conList.ToArray(typeof(DockContainer)));
							disableOnControlRemove = false;
						}

						this.Controls.Add(cont);
						cont.Dock = DockStyle.Fill;
					}

					// Add the container to the list object.
					if (sender is DockWindow)
					{
						AddPanel((sender as DockWindow).ControlContainer, e.Target);
					}
					else
					{
						DockContainer src = null;

						if (sender is DockForm)
							src = (sender as DockForm).RootContainer;
						else if (sender is DockContainer)
							src = (sender as DockContainer);

						AddWindowContent(src, e.Target);
					}

					// Set focus.
					this.TopLevelControl.BringToFront();
					this.TopLevelControl.Invalidate(true);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockContainer.DragWindow: " + ex.Message);
			}

			return true;
		}

		/// <summary>
		/// Adds a <see cref="DockPanel"/> to the container.
		/// </summary>
		/// <param name="src">The source panel.></param>
		/// <param name="dst">The destination container.</param>
		internal void AddPanel(DockPanel src, DockContainer dst)
		{
			if ((src == null) | (dst == null))
				return;

			try
			{
				blockFocusEvents = true;

				if (dst == this)
				{
					this.Controls.Add(src);
				}
				else
				{
					dst.Controls.Add(src);
					this.Controls.Add(dst);
				}

				blockFocusEvents = false;
			}
			catch (Exception e)
			{
				Console.WriteLine("DockContainer.AddPanel: " + e.Message);
			}
		}

		/// <summary>
		/// Adds the content of a <see cref="DockContainer"/> to the container.
		/// </summary>
		/// <param name="src">The source container.></param>
		/// <param name="dst">The destination container.</param>
		internal void AddWindowContent(DockContainer src, DockContainer dst)
		{
			if ((src == null) | (dst == null))
				return;

			try
			{
				blockFocusEvents = true;

				if (dst == this)
				{
					// Transfer flat hierarchy (all panels).
					ArrayList list = new ArrayList();
					src.GetPanels(list);

					this.Controls.AddRange((DockPanel[])list.ToArray(typeof(DockPanel)));

					AdjustBorders();
				}
				else
				{
					// Transfer complete hierarchy.
					src.Location = dst.Location;
					src.Size = dst.Size;
					src.Dock = dst.Dock;

					this.Controls.Add(src);
				}

				blockFocusEvents = false;
			}
			catch (Exception e)
			{
				Console.WriteLine("DockContainer.AddWindowContent: " + e.Message);
			}
		}

		/// <summary>
		/// The direct version of the <see cref="DragWindow"/> event handler to dock a window into the container.
		/// Can be used for every container.
		/// </summary>
		/// <param name="wnd">The <see cref="DockForm"/> that is to be docked.</param>
		/// <param name="style">The preferred dock style. Use <see cref="DockStyle.Fill"/> to dock directly into the container.</param>
		/// <returns>The success of the operation.</returns>
		public bool DockWindow(DockWindow wnd, DockStyle style)
		{
			try
			{
				Point pt = Point.Empty;

				// Check hierarchy.
				if (conList.Count > 0)
					return (conList[0] as DockContainer).DockWindow(wnd, style);

				// Check Visibility.
				if (!wnd.IsVisible)
				{
					wnd.ShowFormAtOnLoad = false;
					wnd.Show();
					wnd.ShowFormAtOnLoad = true;
				}

				// Release panel from any DockContainer.
				if (wnd.HostContainer != null)
					wnd.Release();

				// Prepare target point.
				if (style == DockStyle.None)
					return false;

				pt = GetVirtualDragDest(style);

				DockManager.NoGuidePlease = true;
				DragWindow(wnd, new DockEventArgs(pt, wnd.DockType, true));
				DockManager.NoGuidePlease = false;

				return wnd.IsDocked;
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockContainer.DockWindow: " + ex.Message);
			}

			return false;
		}

		internal Point GetVirtualDragDest(DockStyle style)
		{
			Point pt = Point.Empty;
			Rectangle rc = RectangleToScreen(this.ClientRectangle);

			switch (style)
			{
				case DockStyle.Left:
					pt = new Point(rc.Left + this.DockPadding.Left + dockBorder / 2, rc.Top + rc.Height / 2);
					break;
				case DockStyle.Right:
					pt = new Point(rc.Left + rc.Width - this.DockPadding.Right - dockBorder / 2, rc.Top + rc.Height / 2);
					break;
				case DockStyle.Top:
					pt = new Point(rc.Left + rc.Width / 2, rc.Top + this.DockPadding.Top + dockBorder / 2);
					break;
				case DockStyle.Bottom:
					pt = new Point(rc.Left + rc.Width / 2, rc.Top + rc.Height - this.DockPadding.Bottom - dockBorder / 2);
					break;
				case DockStyle.Fill:
					if (this.IsRootContainer)
						rc = this.Parent.Bounds;
					pt = new Point(rc.Left + rc.Width / 2, rc.Top + this.DockPadding.Top / 2);
					break;
			}

			return pt;
		}

		/// <summary>
		/// Releases a <see cref="DockWindow"/> from the container.
		/// The container may destroy itself at the end of this procedure if it is empty and removeable.
		/// </summary>
		/// <param name="form">The <see cref="DockWindow"/> that is to be released.</param>
		/// <returns>The success of the operation.</returns>
		internal bool ReleaseWindow(DockWindow form)
		{
			bool ret = false;

			if (this.Controls.Contains(form.ControlContainer))
			{
				this.Controls.Remove(form.ControlContainer);
				ret = true;
			}

			if ((panList.Count == 0) & (conList.Count == 0) & removeable)
			{
				if (this.Parent is DockContainer)
					(this.Parent as DockContainer).RemoveContainer(this);
				else if (this.Parent is DockForm)
					(this.Parent as DockForm).Close();
			}

			return ret;
		}
		#endregion

		#region Misc overrides
		/// <summary>
		/// Extends the key processing features of the base class (Panel).
		/// This method is called recursively beginning from the top-level container.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A <see cref="KeyEventArgs"/> that contains the key data.</param>
		public void InvokeKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			try
			{
				foreach (DockContainer c in conList)
				{
					if (c.ContainsFocus)
					{
						c.InvokeKeyDown(sender, e);
						return;
					}
				}

				if (activePanel != null)
					activePanel.Form.InvokeKeyDown(e);
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockContainer.InvokeKeyDown: " + ex.Message);
			}
		}

		/// <summary>
		/// Extends the key processing features of the base class (<see cref="Panel"/>).
		/// This method is called recursively beginning from the top-level container.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A <see cref="KeyEventArgs"/> that contains the key data.</param>
		public void InvokeKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			try
			{
				foreach (DockContainer c in conList)
				{
					if (c.ContainsFocus)
					{
						c.InvokeKeyUp(sender, e);
						break;
					}
				}

				if (activePanel != null)
					activePanel.Form.InvokeKeyUp(e);
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockContainer.InvokeKeyUp: " + ex.Message);
			}
		}

		/// <summary>
		/// Overrides the base class function to pass "true" at any time.
		/// </summary>
		/// <param name="keyData">The key data.</param>
		/// <returns>True at any time.</returns>
		protected override bool IsInputKey(Keys keyData)
		{
			return true;
		}

		/// <summary>
		/// Overrides the base class function to adjust the sizes of the child containers.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnResize(EventArgs e)
		{
			foreach (DockContainer cont in conList)
			{
				if (cont.Dock == DockStyle.Fill)
					continue;

				if ((this.Dock == DockStyle.Left) || (this.Dock == DockStyle.Right))
					cont.Height += (this.Height - oldSize.Height) / 2;
				else if ((this.Dock == DockStyle.Top) || (this.Dock == DockStyle.Bottom))
					cont.Width += (this.Width - oldSize.Width) / 2;
			}

			oldSize = this.Size;

			base.OnResize(e);
		}

		/// <summary>
		/// Overrides the base class function to adjust the borders.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnParentChanged(EventArgs e)
		{
			AdjustBorders();

			// Register container in the static manager field.
			if (this.Parent != null)
				DockManager.RegisterContainer(this);
			else
				DockManager.UnRegisterContainer(this);

			base.OnParentChanged(e);
		}
		#endregion

		#region AutoHide timer
		internal void FadeOut()
		{
			// Save temporary image.
			fadeImage = new Bitmap(this.Width, this.Height);
			DrawToBitmap(fadeImage, this.ClientRectangle);

			fading = true;
			fadeIn = false;
			fadeSize = this.Size;
			fadeLocation = Point.Empty;

			foreach (Control c in this.Controls)
				c.Hide();

			this.Show();
			BringToFront();

			Invalidate();

			autoHideTimer.Start();
		}

		internal void FadeIn()
		{
			// Save temporary image.
			fadeImage = new Bitmap(this.Width, this.Height);
			DrawToBitmap(fadeImage, this.ClientRectangle);

			// Save background image.
			Bitmap tempImage = new Bitmap(hideStorage.manager.Width, hideStorage.manager.Height);
			fadeBkImage = new Bitmap(this.Width, this.Height);
			Rectangle rc = hideStorage.manager.ClientRectangle;
			hideStorage.manager.DrawToBitmap(tempImage, rc);

			Graphics g = Graphics.FromImage(fadeBkImage);
			g.DrawImage(tempImage, 0, 0, new Rectangle(this.Left, this.Top, this.Width, this.Height), GraphicsUnit.Pixel);
			g.Dispose();

			fading = true;
			fadeIn = true;
			fadeSize = this.Size;
			fadeLocation = Point.Empty;

			foreach (Control c in this.Controls)
				c.Hide();

			this.Show();
			BringToFront();

			switch (hideStorage.toplevelDock)
			{
				case DockStyle.Left:
					fadeLocation.X = -this.Width;
					break;
				case DockStyle.Right:
					fadeLocation.X = this.Width;
					break;
				case DockStyle.Top:
					fadeLocation.Y = -this.Height;
					break;
				case DockStyle.Bottom:
					fadeLocation.Y = this.Height;
					break;
			}

			Invalidate();

			autoHideTimer.Start();
		}

		private void autoHideTimer_Tick(object sender, EventArgs e)
		{
			if (fadeIn)
			{
				switch (hideStorage.toplevelDock)
				{
					case DockStyle.Left:
						if (fadeLocation.X + fadeSpeed >= 0)
						{
							StopFading(false);
							return;
						}
						else
							fadeLocation.X += fadeSpeed;
						break;
					case DockStyle.Right:
						if (fadeLocation.X - fadeSpeed <= 0)
						{
							StopFading(false);
							return;
						}
						else
							fadeLocation.X -= fadeSpeed;
						break;
					case DockStyle.Top:
						if (fadeLocation.Y + fadeSpeed >= 0)
						{
							StopFading(false);
							return;
						}
						else
							fadeLocation.Y += fadeSpeed;
						break;
					case DockStyle.Bottom:
						if (fadeLocation.Y - fadeSpeed <= 0)
						{
							StopFading(false);
							return;
						}
						else
							fadeLocation.Y -= fadeSpeed;
						break;
				}
			}
			else
			{
				switch (hideStorage.toplevelDock)
				{
					case DockStyle.Left:
						if (fadeLocation.X - fadeSpeed <= -this.Width)
						{
							StopFading(true);
							return;
						}
						else
							fadeLocation.X -= fadeSpeed;
						break;
					case DockStyle.Right:
						if (fadeLocation.X + fadeSpeed >= this.Width)
						{
							StopFading(true);
							return;
						}
						else
							fadeLocation.X += fadeSpeed;
						break;
					case DockStyle.Top:
						if (fadeLocation.Y - fadeSpeed <= -this.Height)
						{
							StopFading(true);
							return;
						}
						else
							fadeLocation.Y -= fadeSpeed;
						break;
					case DockStyle.Bottom:
						if (fadeLocation.Y + fadeSpeed >= this.Height)
						{
							StopFading(true);
							return;
						}
						else
							fadeLocation.Y += fadeSpeed;
						break;
				}
			}

			Invalidate();
		}

		internal void StopFading(bool release)
		{
			fading = false;

			if (release)
				Hide();

			foreach (Control c in this.Controls)
			{
				if (!(c is DockPanel) | (c == activePanel))
					c.Show();
			}

			autoHideTimer.Stop();

			if (release)
			{
				hideStorage.manager.ReleaseAutoHideContainer();

				if (fadeImage != null)
				{
					fadeImage.Dispose();
					fadeImage = null;
				}
				if (fadeBkImage != null)
				{
					fadeBkImage.Dispose();
					fadeBkImage = null;
				}
			}
			else
			{
				this.BringToFront();
				this.Focus();
				this.ShowButtons();
				Invalidate();
			}
		}

		internal void StopAutoHide()
		{
			// Undock.
			hideStorage.manager.AutoHideContainer(this, hideStorage.toplevelDock, false);

			// Is the old parent container still properly docked?
			//  - If the TopLevelContainer property returns the same object, the container is on its own.
			//  - The DockStyle property has to be set.
			//  - The container must not contain more containers or the whole thing will end up in a mess.
			if ((hideStorage.parent.TopLevelContainer != hideStorage.parent) &
				(hideStorage.parent.Dock != DockStyle.None))
			{
				// The container is still present in its old shape.
				Point pt = hideStorage.parent.GetVirtualDragDest(hideStorage.parentDock);
				DockEventArgs e = new DockEventArgs(pt, this.DockType, true);
				e.IgnoreHierarchy = true;

				DockManager.NoGuidePlease = true;
				hideStorage.parent.DragWindow(this, e);
				DockManager.NoGuidePlease = false;
			}
			else
			{
				// The container has moved or changed its content in some way.
				Point pt = hideStorage.manager.GetVirtualDragDest(hideStorage.toplevelDock);
				DockEventArgs e = new DockEventArgs(pt, this.DockType, true);
				e.IgnoreHierarchy = true;

				DockManager.NoGuidePlease = true;
				hideStorage.manager.DragWindow(this, e);
				DockManager.NoGuidePlease = false;
			}
		}
		#endregion

		#region Mouse
		#region Mouse down
		/// <summary>
		/// Overrides the base class function to handle drag of panels and resizing.
		/// </summary>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the mouse data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			// Test header.
			Focus();

			ArrayList list = new ArrayList();
			GetPanels(list);

			if (!IsRootContainer & (list.Count > 0))
				dragObject = this;
			else
				dragObject = null;

			ptStart = new Point(e.X, e.Y);

			// Test footer.
			int count = panList.Count;

			for (int i = 0; i < count; i++)
			{
				DockPanel panel = panList[i] as DockPanel;

				if (panel.TabRect.Contains(e.X, e.Y))
				{
					ActivePanel = panel;

					if (panel.Form.AllowUnDock)
						dragObject = panel;
					else
						dragObject = null;

					if ((e.Button == MouseButtons.Middle) & (this.DockType == DockContainerType.Document))
						CloseClick(this, null);

					return;
				}
			}

			base.OnMouseDown(e);
		}
		#endregion

		#region Mouse move
		/// <summary>
		/// Overrides the base class function to handle drag of panels and resizing.
		/// </summary>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the mouse data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (autoHide)
				return;

			bool inPanelBar = ((panList.Count > 1) & (e.Y > this.Height - this.DockPadding.Bottom) & (this.DockType != DockContainerType.Document)) | ((e.Y < bottomDock) & (this.DockType == DockContainerType.Document));

			if (inPanelBar && (this.ClientRectangle.Contains(e.X, e.Y)))
			{
				// Display tool-tip.
				UpdateToolTip(new Point(e.X, e.Y));
			}
			else
			{
				if ((this is DockManager) | ((panList.Count == 0) & (conList.Count == 0)))
					return;

				// Check panel movement.
				if ((e.Button == MouseButtons.Left) & (dragObject != null) & !ptStart.Equals(new Point(e.X, e.Y)))
				{
					if (autoHide)
						hideStorage.manager.AutoHideContainer(this, DockStyle.Fill, false);
                    if (this.activePanel.Form.AllowUnDock)
					FloatWindow();
				}
			}

			base.OnMouseMove(e);
		}

		private void FloatWindow()
		{
			Rectangle rc = RectangleToScreen(this.ClientRectangle);
			DockForm form = new DockForm(dragObject);
			form.Show();
			form.Location = new Point(MousePosition.X, MousePosition.Y);

			if (!DockManager.FastMoveDraw)
				form.Opacity = 1;

			form.StartMoving(new Point(MousePosition.X + 10, MousePosition.Y + 10));

			dragObject = null;

			if ((panList.Count == 0) & (dockType == DockContainerType.Document) & (removeable) & (this.Parent is DockContainer))
				(this.Parent as DockContainer).Controls.Remove(this);
		}
		#endregion

		#region Mouse leave
		/// <summary>
		/// Overrides the base class function to disable the context menu.
		/// </summary>
		/// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnMouseLeave(EventArgs e)
		{
			this.ContextMenuStrip = null;
			base.OnMouseLeave(e);

			if (autoHide)
				hideStorage.manager.OnMouseLeave(e);
		}
		#endregion

		#region Drag panel
		/// <summary>
		/// The MouseDown event handler of the drag panel.
		/// Used to handle resizing of the container.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the mouse data.</param>
		private void dragPanel_MouseDown(object sender, MouseEventArgs e)
		{
			// Copy last point.
			ptStart = new Point(e.X + dragPanel.Left, e.Y + dragPanel.Top);

			// Create dummy container.
			dragDummy = new DockContainer();
			dragDummy.isDragContainer = true;

			// Set size and location.
			if (this.Dock == DockStyle.Left)
			{
				dragDummy.Location = new Point(this.Location.X + ptStart.X - 2 + this.TopLevelContainer.dockOffsetL, this.Location.Y);
				dragDummy.Size = new Size(4, this.Height);
			}
			else if (this.Dock == DockStyle.Right)
			{
				dragDummy.Location = new Point(this.Location.X + ptStart.X - 2 - this.TopLevelContainer.dockOffsetR, this.Location.Y);
				dragDummy.Size = new Size(4, this.Height);
			}
			else if (this.Dock == DockStyle.Bottom)
			{
				dragDummy.Location = new Point(this.Location.X, this.Location.Y + ptStart.Y - 2 - this.TopLevelContainer.dockOffsetB);
				dragDummy.Size = new Size(this.Width, 4);
			}
			else
			{
				dragDummy.Location = new Point(this.Location.X, this.Location.Y + ptStart.Y - 2 + this.TopLevelContainer.dockOffsetT);
				dragDummy.Size = new Size(this.Width, 4);
			}

			// Add container to parent and resizing flag.
			Parent.Controls.Add(dragDummy);
			dragDummy.BringToFront();
			resizing = true;
		}

		/// <summary>
		/// The MouseMove event handler of the drag panel.
		/// Used to handle resizing of the container.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the mouse data.</param>
		private void dragPanel_MouseMove(object sender, MouseEventArgs e)
		{
			// If resizing, drag dummy to new position.
			if (resizing)
			{
				ptStart = new Point(e.X + dragPanel.Left, e.Y + dragPanel.Top);

				DockContainer c = this.Parent as DockContainer;

				int w = c.Width;
				int h = c.Height;

				if (this.Dock == DockStyle.Left)
				{
					if (ptStart.X < dockBorder * 3)
						ptStart.X = dockBorder * 3;
					else if (ptStart.X > w - dockBorder * 3)
						ptStart.X = w - dockBorder * 3;

					dragDummy.Location = new Point(ptStart.X - 2 + TopLevelContainer.dockOffsetL, 0);
				}
				else if (this.Dock == DockStyle.Right)
				{
					if (ptStart.X > this.Width - dockBorder * 3)
						ptStart.X = this.Width - dockBorder * 3;
					else if (ptStart.X < this.Width - w + dockBorder * 3)
						ptStart.X = this.Width - w + dockBorder * 3;

					dragDummy.Location = new Point(w - this.Width + ptStart.X - 2 - TopLevelContainer.dockOffsetR, 0);
				}
				else if (this.Dock == DockStyle.Top)
				{
					if (ptStart.Y < dockBorder * 3)
						ptStart.Y = dockBorder * 3;
					else if (ptStart.Y > h - dockBorder * 3)
						ptStart.Y = h - dockBorder * 3;

					dragDummy.Location = new Point(0, ptStart.Y - 2 + TopLevelContainer.dockOffsetT);
				}
				else if (this.Dock == DockStyle.Bottom)
				{
					if (ptStart.Y > this.Height - dockBorder * 3)
						ptStart.Y = this.Height - dockBorder * 3;
					else if (ptStart.Y < this.Height - h + dockBorder * 3)
						ptStart.Y = this.Height - h + dockBorder * 3;

					dragDummy.Location = new Point(0, h - this.Height + ptStart.Y - 2 - TopLevelContainer.dockOffsetB);
				}

				dragDummy.BringToFront();
				Invalidate();
			}
		}

		/// <summary>
		/// The MouseUp event handler of the drag panel.
		/// Used to handle resizing of the container.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A <see cref="MouseEventArgs"/> that contains the mouse data.</param>
		private void dragPanel_MouseUp(object sender, MouseEventArgs e)
		{
			// If resizing, release dummy and set new size.
			if (resizing)
			{
				resizing = false;

				// Release dummy.
				Parent.Controls.Remove(dragDummy);
				Parent.Invalidate();
				dragDummy.Dispose();
				dragDummy = null;

				// Set new size according to the actual state.
				ptStart = new Point(e.X + dragPanel.Left, e.Y + dragPanel.Top);

				if (this.Dock == DockStyle.Left)
				{
					if (ptStart.X < dockBorder * 3)
						ptStart.X = dockBorder * 3;
					else if (ptStart.X > this.Parent.Width - dockBorder * 3)
						ptStart.X = this.Parent.Width - dockBorder * 3;
				}
				else if (this.Dock == DockStyle.Right)
				{
					if (ptStart.X > this.Width - dockBorder * 3)
						ptStart.X = this.Width - dockBorder * 3;
					else if (ptStart.X < this.Width - this.Parent.Width + dockBorder * 3)
						ptStart.X = this.Width - this.Parent.Width + dockBorder * 3;
				}
				else if (this.Dock == DockStyle.Top)
				{
					if (ptStart.Y < dockBorder * 3)
						ptStart.Y = dockBorder * 3;
					else if (ptStart.Y > this.Parent.Height - dockBorder * 3)
						ptStart.Y = this.Parent.Height - dockBorder * 3;
				}
				else if (this.Dock == DockStyle.Bottom)
				{
					if (ptStart.Y > this.Height - dockBorder * 3)
						ptStart.Y = this.Height - dockBorder * 3;
					else if (ptStart.Y < this.Height - this.Parent.Height + dockBorder * 3)
						ptStart.Y = this.Height - this.Parent.Height + dockBorder * 3;
				}

				switch (this.Dock)
				{
					case DockStyle.Left:
						if (ptStart.X < 40)
							this.Width = 40;
						else
							this.Width = ptStart.X;
						break;
					case DockStyle.Right:
						int x = 0;

						if (this.Width - ptStart.X < 40)
							x = this.Width - 40;
						else
							x = ptStart.X;

						this.Location = new Point(this.Location.X + x, this.Location.Y);
						this.Width -= x;
						break;
					case DockStyle.Top:
						if (ptStart.Y < 60)
							this.Height = 60;
						else
							this.Height = ptStart.Y;
						break;
					case DockStyle.Bottom:
						int y = 0;

						if (this.Height - ptStart.Y < 60)
							y = this.Height - 60;
						else
							y = ptStart.Y;

						this.Location = new Point(this.Location.X, this.Location.Y + y);
						this.Height -= y;
						break;
				}
			}
		}
		#endregion

		#region Tool tip
		/// <summary>
		/// Updates the <see cref="ToolTip"/> and reorders the panels, if needed (mouse movement + LMB).
		/// </summary>
		/// <param name="pt">The pointer position.</param>
		private void UpdateToolTip(Point pt)
		{
			RectangleF rc;

			if (activePanel == null)
				return;

			for (int i = 0; i < panList.Count; i++)
			{
				DockPanel panel = panList[i] as DockPanel;

				rc = panel.TabRect;
				if (!rc.Contains(pt))
					continue;

				if ((MouseButtons == MouseButtons.Left) && (panel != activePanel))
				{
					if (i > panList.IndexOf(activePanel))
					{
						if (pt.X - activePanel.TabRect.Right < rc.Width - activePanel.TabRect.Width)
							return;
						panList.Remove(activePanel);
						panList.Insert(Math.Min(i + 1, panList.Count), activePanel);
					}
					else
					{
						if (activePanel.TabRect.Left - pt.X < rc.Width - activePanel.TabRect.Width)
							return;
						panList.Remove(activePanel);
						panList.Insert(i, activePanel);
					}
					Invalidate();
					return;
				}
				else
				{
					toolTip.SetToolTip(this, panel.Form.Text);
					toolTip.Active = true;
					this.ContextMenuStrip = contextMenuStrip;
					return;
				}
			}

			this.ContextMenuStrip = null;
			toolTip.Active = false;
		}
		#endregion
		#endregion

		#region Context menu
		/// <summary>
		/// The message handler for the undock event of the context menu.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
		public void UndockClick(object sender, EventArgs e)
		{
			if (autoHide)
				hideStorage.manager.AutoHideContainer(this, DockStyle.Fill, false);

			FloatWindow();
		}

		/// <summary>
		/// The message handler for the close event of the context menu.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
		public void CloseClick(object sender, EventArgs e)
		{
			btnClose_Click(sender, e);
		}

		private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			try
			{
				if (activePanel == null)
				{
					e.Cancel = true;
					return;
				}

				bool allowClose = activePanel.Form.AllowClose;
				bool allowUnDock = activePanel.Form.AllowUnDock;

				if (!allowClose & !allowUnDock)
					e.Cancel = true;

				miUndock.Visible = allowUnDock;
				miClose.Visible = allowClose;
				miSep.Visible = allowClose & allowUnDock;
			}
			catch (Exception ex)
			{
				Console.WriteLine("DockContainer.contextMenuStrip_Opening : " + ex.Message);
				e.Cancel = true;
			}
		}
		#endregion

		#region XML r/w
		/// <summary>
		/// Writes the container data to the window save list.
		/// </summary>
		/// <param name="writer">The <see cref="XmlTextWriter"/> object that writes to the target stream.</param>
		virtual internal void WriteXml(XmlTextWriter writer)
		{
			writer.WriteStartElement("container");
			writer.WriteAttributeString("dock", this.Dock.ToString());
			writer.WriteAttributeString("width", this.Width.ToString());
			writer.WriteAttributeString("height", this.Height.ToString());
			writer.WriteAttributeString("type", dockType.ToString());

			foreach (DockContainer c in conList)
				c.WriteXml(writer);

			foreach (DockPanel p in panList)
				if (p.Form.AllowSave)
					p.WriteXml(writer);

			writer.WriteEndElement();
		}

		/// <summary>
		/// Reads the container data from the window save list.
		/// </summary>
		/// <param name="reader">The <see cref="XmlReader"/> object that reads from the source stream.</param>
		virtual internal void ReadXml(XmlReader reader)
		{
			reader.Read();
			//disableOnControlAdded = true;

			if (!(this is DockManager))
			{
				string s;

				switch (reader.GetAttribute("dock"))
				{
					case "Fill":
						this.Dock = DockStyle.Fill;
						break;
					case "Top":
						this.Dock = DockStyle.Top;
						break;
					case "Bottom":
						this.Dock = DockStyle.Bottom;
						break;
					case "Left":
						this.Dock = DockStyle.Left;
						break;
					case "Right":
						this.Dock = DockStyle.Right;
						break;
					default:
						disableOnControlAdded = false;
						return;
				}

				s = reader.GetAttribute("width");
				if (s != null)
					this.Width = int.Parse(s);

				s = reader.GetAttribute("height");
				if (s != null)
					this.Height = int.Parse(s);

				switch (reader.GetAttribute("type"))
				{
					case "Document":
						this.DockType = DockContainerType.Document;
						break;
					case "ToolWindow":
						this.DockType = DockContainerType.ToolWindow;
						break;
					default:
						disableOnControlAdded = false;
						return;
				}
			}

			while (reader.Read())
			{
				if (!reader.IsStartElement())
					continue;

				if (reader.Name == "container")
				{
					Console.WriteLine("container");

					DockContainer c = new DockContainer();
					c.ReadXml(reader.ReadSubtree());

					if (!c.IsEmpty)
					{
						this.Controls.Add(c);
						this.DockPadding.All = 0;
					}
				}
				else if (reader.Name == "panel")
				{
					Console.WriteLine("panel");

					DockPanel p = new DockPanel();
					p.ReadXml(reader);

					this.Controls.Add(p);
				}
			}

			disableOnControlAdded = false;
		}
		#endregion
	}
}