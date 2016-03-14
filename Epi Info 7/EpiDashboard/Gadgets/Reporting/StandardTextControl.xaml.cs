using System;
using System.Collections.Generic;
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
using EpiDashboard.Gadgets;

namespace EpiDashboard.Gadgets.Reporting
{
    /// <summary>
    /// Interaction logic for StandardTextControl.xaml
    /// </summary>
    public partial class StandardTextControl : GadgetBase, IReportingGadget
    {
        private bool isSelected = true;

        private Brush textBoxBorderBrush;

     
        public StandardTextControl()
        {
            InitializeComponent();
            Construct();
        }

        public StandardTextControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            Construct();
        }

        public StandardTextControl(DashboardHelper dashboardHelper, DashboardControl dashboardControl)
        {
            InitializeComponent();
            Construct();
        }

        protected override void Construct()
        {
            this.MinWidth = 10;
            if (this.Parent is DragCanvas)
            {
                this.standardTextBox.MaxWidth = (this.Parent as DragCanvas).ActualWidth - 10;
            }
            textBoxBorderBrush = this.standardTextBox.BorderBrush;
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

        #region IGadget Members
        public override void RefreshResults()
        {
        }

        public override XmlNode Serialize(XmlDocument doc)
        {
            string fontWeight = "regular";

            if (standardTextBox.FontWeight == FontWeights.Bold)
            {
                fontWeight = "bold";
            }
            else if (standardTextBox.FontWeight == FontWeights.SemiBold)
            {
                fontWeight = "semibold";
            }
            else if (standardTextBox.FontWeight == FontWeights.DemiBold)
            {
                fontWeight = "demibold";
            }
            else if (standardTextBox.FontWeight == FontWeights.Thin)
            {
                fontWeight = "thin";
            }
            else if (standardTextBox.FontWeight == FontWeights.Light)
            {
                fontWeight = "light";
            }

            string xmlString =
            "<text>" + this.standardTextBox.Text.Replace("<", "&lt;").Replace("&", "&amp;") + "</text>" +
            "<fontWeight>" + fontWeight + "</fontWeight>" +
            "<fontSize>" + this.standardTextBox.FontSize.ToString("F0") + "</fontSize>" +
            "<fontStretch>" + this.standardTextBox.FontStretch + "</fontStretch>" +
            "<fontStyle>" + this.standardTextBox.FontStyle.ToString() + "</fontStyle>" +
            "<fontFamily>" + this.standardTextBox.FontFamily + "</fontFamily>";

            xmlString += "<textDecorations>";

            foreach (TextDecoration td in standardTextBox.TextDecorations)
            {
                xmlString += "<textDecoration>";
                xmlString += td.Location.ToString();
                xmlString += "</textDecoration>";
            }

            xmlString += "</textDecorations>";

            xmlString += "<foregroundColor>";
            xmlString += "<red>";
            xmlString += ((SolidColorBrush)standardTextBox.Foreground).Color.R;
            xmlString += "</red>";

            xmlString += "<green>";
            xmlString += ((SolidColorBrush)standardTextBox.Foreground).Color.G;
            xmlString += "</green>";

            xmlString += "<blue>";
            xmlString += ((SolidColorBrush)standardTextBox.Foreground).Color.B;
            xmlString += "</blue>";
            xmlString += "</foregroundColor>";
            
            //+this.standardTextBox.TextDecorations.ToString() + "</textDecorations>";

            System.Xml.XmlElement element = doc.CreateElement("standardTextReportGadget");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            id.Value = this.UniqueIdentifier.ToString();

            System.Xml.XmlAttribute width = doc.CreateAttribute("width");
            System.Xml.XmlAttribute height = doc.CreateAttribute("height");

            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");

            width.Value = this.ActualWidth.ToString("F0");
            height.Value = this.ActualHeight.ToString("F0");

            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.Gadgets.Reporting.StandardTextControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(id);

            element.Attributes.Append(width);
            element.Attributes.Append(height);

            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            SerializeAnchors(element);
            return element;
        }

        public override void CreateFromXml(XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "text":
                        standardTextBox.Text = child.InnerText.Replace("&lt;", "<").Replace("&amp;", "&");
                        break;
                    case "fontsize":
                        standardTextBox.FontSize = int.Parse(child.InnerText);
                        break;
                    case "fontweight":
                        switch (child.InnerText.ToLower())
                        {
                            case "semibold":
                                standardTextBox.FontWeight = FontWeights.SemiBold;
                                break;
                            case "thin":
                                standardTextBox.FontWeight = FontWeights.Thin;
                                break;
                            case "demibold":
                                standardTextBox.FontWeight = FontWeights.DemiBold;
                                break;
                            case "light":
                                standardTextBox.FontWeight = FontWeights.Light;
                                break;
                            case "bold":
                                standardTextBox.FontWeight = FontWeights.Bold;
                                break;
                            default:
                                standardTextBox.FontWeight = FontWeights.Regular;
                                break;
                        }
                        //standardTextBox.FontWeight
                        break;
                    case "fontstretch":
                        switch (child.InnerText)
                        {
                            case "Condensed":
                                standardTextBox.FontStretch = FontStretches.Condensed;
                                break;
                            case "Expanded":
                                standardTextBox.FontStretch = FontStretches.Expanded;
                                break;
                            case "ExtraCondensed":
                                standardTextBox.FontStretch = FontStretches.ExtraCondensed;
                                break;
                            case "ExtraExpanded":
                                standardTextBox.FontStretch = FontStretches.ExtraExpanded;
                                break;
                            case "Medium":
                                standardTextBox.FontStretch = FontStretches.Medium;
                                break;                            
                            case "SemiCondensed":
                                standardTextBox.FontStretch = FontStretches.SemiCondensed;
                                break;
                            case "SemiExpanded":
                                standardTextBox.FontStretch = FontStretches.SemiExpanded;
                                break;
                            case "UltraCondensed":
                                standardTextBox.FontStretch = FontStretches.UltraCondensed;
                                break;
                            case "UltraExpanded":
                                standardTextBox.FontStretch = FontStretches.UltraExpanded;
                                break;                            
                            case "Normal":
                            default:
                                standardTextBox.FontStretch = FontStretches.Normal;
                                break;
                        }
                        break;
                    case "fontstyle":
                        switch (child.InnerText)
                        {
                            case "Italic":
                                standardTextBox.FontStyle = FontStyles.Italic;
                                break;                            
                            case "Oblique":
                                standardTextBox.FontStyle = FontStyles.Oblique;
                                break;
                            case "Normal":
                            default:
                                standardTextBox.FontStyle = FontStyles.Normal;
                                break;
                        }
                        break;
                    case "fontfamily":
                        standardTextBox.FontFamily = new FontFamily(child.InnerText);
                        break;
                    case "textdecorations":
                        standardTextBox.TextDecorations.Clear();
                        foreach (XmlNode tdNode in child.ChildNodes)
                        {
                            switch (tdNode.InnerText)
                            {
                                case "Underline":
                                    standardTextBox.TextDecorations.Add(TextDecorations.Underline);
                                    break;
                                case "Overline":
                                    standardTextBox.TextDecorations.Add(TextDecorations.OverLine);
                                    break;
                                case "Baseline":
                                    standardTextBox.TextDecorations.Add(TextDecorations.Baseline);
                                    break;
                                case "Strikethrough":
                                    standardTextBox.TextDecorations.Add(TextDecorations.Strikethrough);
                                    break;
                            }
                            break;
                        }
                        break;
                    case "foregroundcolor":
                        byte red = 0;
                        byte green = 0;
                        byte blue = 0;
                        foreach (XmlNode tdNode in child.ChildNodes)
                        {
                            switch (tdNode.Name)
                            {
                                case "red":
                                    red = byte.Parse(tdNode.InnerText);
                                    break;
                                case "green":
                                    green = byte.Parse(tdNode.InnerText);
                                    break;
                                case "blue":
                                    blue = byte.Parse(tdNode.InnerText);
                                    break;
                            }
                        }
                        
                        SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
                        standardTextBox.Foreground = brush;
                        
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
            base.CreateFromXml(element);
        }

        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<p>");
            htmlBuilder.AppendLine("<span style=\"");

            int red = ((SolidColorBrush)standardTextBox.Foreground).Color.R;
            int green = ((SolidColorBrush)standardTextBox.Foreground).Color.G;
            int blue = ((SolidColorBrush)standardTextBox.Foreground).Color.B;

            htmlBuilder.AppendLine("font-family: '" + standardTextBox.FontFamily.ToString() + "', 'Arial', sans-serif;");
            htmlBuilder.AppendLine("font-size: " + standardTextBox.FontSize.ToString() + ";");
            htmlBuilder.AppendLine("font-weight: " + standardTextBox.FontWeight.ToString() + ";");
            htmlBuilder.AppendLine("color: rgb(" + red + ", " + green + ", " + blue + ");");
                
            htmlBuilder.AppendLine("\">");
            htmlBuilder.AppendLine(standardTextBox.Text);
            htmlBuilder.AppendLine("</p>");
            return htmlBuilder.ToString();
        }
        #endregion // IGadget Members

        public void Select()
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                adornerLayer.Visibility = System.Windows.Visibility.Visible;
            }
            this.borderMain.Style = this.Resources["mainReportGadgetBorderOn"] as Style;
            //this.standardTextBox.BorderThickness = new Thickness(1);
            this.standardTextBox.BorderBrush = textBoxBorderBrush;
        }

        public void Deselect()
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                adornerLayer.Visibility = System.Windows.Visibility.Hidden;
            }
            this.borderMain.Style = this.Resources["mainReportGadgetBorderOff"] as Style;
            //this.standardTextBox.BorderThickness = new Thickness(0);
            this.standardTextBox.BorderBrush = Brushes.Transparent;
        }

        private void mnuFont_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.FontChooser fontChooser = new Dialogs.FontChooser();

            fontChooser.SelectedFontFamily = standardTextBox.FontFamily;
            fontChooser.SelectedFontSize = standardTextBox.FontSize;
            fontChooser.SelectedFontStretch = standardTextBox.FontStretch;
            fontChooser.SelectedFontStyle = standardTextBox.FontStyle;
            fontChooser.SelectedFontWeight = standardTextBox.FontWeight;
            fontChooser.SelectedTextDecorations = standardTextBox.TextDecorations;

            bool? result = fontChooser.ShowDialog();
            if (result == true)
            {
                this.standardTextBox.FontFamily = fontChooser.SelectedFontFamily;
                this.standardTextBox.FontSize = fontChooser.SelectedFontSize;
                this.standardTextBox.FontStretch = fontChooser.SelectedFontStretch;
                this.standardTextBox.FontStyle = fontChooser.SelectedFontStyle;
                this.standardTextBox.FontWeight = fontChooser.SelectedFontWeight;
                this.standardTextBox.TextDecorations = fontChooser.SelectedTextDecorations;                
            }
        }

        private void mnuColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            colorDialog.AnyColor = true;
            colorDialog.AllowFullOpen = true;
            
            System.Drawing.Color color = System.Drawing.Color.FromArgb(((SolidColorBrush)standardTextBox.Foreground).Color.R, ((SolidColorBrush)standardTextBox.Foreground).Color.G, ((SolidColorBrush)standardTextBox.Foreground).Color.B);

            colorDialog.Color = color;

            System.Windows.Forms.DialogResult result = colorDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                standardTextBox.Foreground = brush;
            }
        }
    }
}
