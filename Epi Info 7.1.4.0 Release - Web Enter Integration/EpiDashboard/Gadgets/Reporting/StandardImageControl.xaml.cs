using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using EpiDashboard;

namespace EpiDashboard.Gadgets.Reporting
{
    /// <summary>
    /// Interaction logic for StandardImageControl.xaml
    /// </summary>
    public partial class StandardImageControl : UserControl, IGadget, IReportingGadget
    {
        private bool isProcessing = false;
        private bool isSelected = true;
        private bool drawBorders = true;
        private byte[] imageBytes;
        private DashboardHelper dashboardHelper;
        private DashboardControl dashboardControl;

        public UserControl AnchorLeft { get; set; }
        public UserControl AnchorTop { get; set; }
        public UserControl AnchorBottom { get; set; }
        public UserControl AnchorRight { get; set; }
        public Guid UniqueIdentifier { get; private set; }

        public event GadgetRefreshedHandler GadgetRefreshed;
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event GadgetRepositionEventHandler GadgetReposition;
        public event GadgetEventHandler GadgetDragStart;
        public event GadgetEventHandler GadgetDragStop;
        public event GadgetEventHandler GadgetDrag;
        public event GadgetAnchorSetEventHandler GadgetAnchorSetFromXml;

        public StandardImageControl()
        {
            InitializeComponent();
            Construct();
        }

        public StandardImageControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            Construct();
            this.dashboardHelper = dashboardHelper;            
        }

        public StandardImageControl(DashboardHelper dashboardHelper, DashboardControl dashboardControl)
        {
            InitializeComponent();
            Construct();
            this.dashboardHelper = dashboardHelper;
            this.dashboardControl = dashboardControl;
        }

        private void Construct()
        {
            this.MinWidth = 10;
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;
                if (IsSelected)
                {
                    Select();
                }
                else
                {
                    Deselect();
                }
            }
        }

        public bool DrawBorders
        {
            get
            {
                return this.drawBorders;
            }
            set
            {
                this.drawBorders = value;
                if (DrawBorders)
                {
                    TurnOnBorders();
                }
                else
                {
                    TurnOffBorders();
                }
            }
        }

        protected void TurnOnBorders()
        {
            Select();
        }

        protected void TurnOffBorders()
        {
            Deselect();
        }

        #region IGadget Members
        public void RefreshResults()
        {
        }

        public XmlNode Serialize(XmlDocument doc)
        {
            string data = string.Empty;

            if (standardImage.Source is BitmapImage && imageBytes != null)
            {
                data = Convert.ToBase64String(imageBytes);
            }

            //string xmlString =
            //"<filePath>" + this.standardImage.Source.ToString() + "</filePath>";

            string xmlString =
            "<imageData>" + data + "</imageData>";

            System.Xml.XmlElement element = doc.CreateElement("standardImageReportGadget");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");

            System.Xml.XmlAttribute width = doc.CreateAttribute("width");
            System.Xml.XmlAttribute height = doc.CreateAttribute("height");

            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");

            width.Value = this.ActualWidth.ToString("F0");
            height.Value = this.ActualHeight.ToString("F0");

            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.Gadgets.Reporting.StandardImageControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);

            element.Attributes.Append(width);
            element.Attributes.Append(height);

            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "filepath":
                        try
                        {
                            SetImage(child.InnerText);
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            // do nothing for now, later on add some sort of notice
                        }
                        break;
                    case "imagedata":
                        try
                        {
                            if (!string.IsNullOrEmpty(child.InnerText.Trim()))
                            {
                                imageBytes = Convert.FromBase64String(child.InnerText);

                                Image image = new Image();
                                BitmapImage bitmapimage = new BitmapImage();

                                bitmapimage.BeginInit();

                                MemoryStream imageStream = new MemoryStream(imageBytes);
                                bitmapimage.StreamSource = imageStream;

                                bitmapimage.EndInit();

                                standardImage.Source = bitmapimage;
                            }
                            //SetImage(child.InnerText);
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            // do nothing for now, later on add some sort of notice
                        }
                        break;
                }
            }

            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLower())
                {
                    case "top":
                        string top = attribute.Value.Replace(',', '.');
                        int topP = top.IndexOf('.');
                        if (topP >= 0)
                        {
                            top = top.Substring(0, topP);
                        }
                        Canvas.SetTop(this, double.Parse(top, culture));
                        break;
                    case "left":
                        string left = attribute.Value.Replace(',', '.');
                        int leftP = left.IndexOf('.');
                        if (leftP >= 0)
                        {
                            left = left.Substring(0, leftP);
                        }
                        Canvas.SetLeft(this, double.Parse(left, culture));
                        break;
                    case "width":
                        this.Width = double.Parse(attribute.Value);
                        break;
                    case "height":
                        this.Height = double.Parse(attribute.Value);
                        break;
                }
            }
        }

        public string ToHTML(string htmlFileName = "", int count = 0)
        {
            return string.Empty;
        }

        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                this.isProcessing = value;
            }
        }
        public void SetGadgetToProcessingState() { }
        public void SetGadgetToFinishedState() { }
        public void UpdateVariableNames() { }

        public string CustomOutputHeading { get; set; }
        public string CustomOutputDescription { get; set; }
        public string CustomOutputCaption { get; set; }
        #endregion // IGadget Members        

        //private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    IsSelected = !IsSelected;

        //    if (IsSelected)
        //    {
        //        Deselect();
        //    }
        //    else
        //    {
        //        Select();
        //    }
        //}

        public void Select()
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                adornerLayer.Visibility = System.Windows.Visibility.Visible;
            }
            this.borderMain.Style = this.Resources["mainReportGadgetBorderOn"] as Style;            
        }

        public void Deselect()
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                adornerLayer.Visibility = System.Windows.Visibility.Hidden;
            }
            this.borderMain.Style = this.Resources["mainReportGadgetBorderOff"] as Style; 
        }

        private void standardImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (standardImage.Source == null)
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.DefaultExt = ".png";
                dlg.Filter = "PNG Image (.png)|*.png";
                System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    SetImage(dlg.FileName);
                }
            }
        }

        private void SetImage(string imagePath)
        {
            BitmapImage userImage = new BitmapImage();
            userImage.BeginInit();            

            // attempt to load image from the current canvas's folder if the image is not found
            if (dashboardHelper != null && dashboardHelper.CurrentCanvas != null && !string.IsNullOrEmpty(dashboardHelper.CurrentCanvas) && !System.IO.File.Exists(imagePath))
            {
                imagePath = imagePath.Replace("file:///", "");
                imageBytes = System.IO.File.ReadAllBytes(imagePath);
                System.IO.FileInfo fiCanvas = new System.IO.FileInfo(dashboardHelper.CurrentCanvas);
                System.IO.FileInfo fiImage = new System.IO.FileInfo(imagePath);                
                System.IO.DirectoryInfo diCanvas = fiCanvas.Directory;
                string path = diCanvas.FullName + "\\" + fiImage.Name;
                userImage.UriSource = new Uri(path);
            }
            else
            {
                userImage.UriSource = new Uri("file:///" + imagePath);
                imageBytes = System.IO.File.ReadAllBytes(imagePath);
            }
            userImage.EndInit();
            standardImage.Source = userImage;
        }

        private void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            if (GadgetClosing != null)
                GadgetClosing(this);
        }
    }
}
