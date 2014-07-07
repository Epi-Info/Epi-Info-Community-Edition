using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Epi.WPF.Dashboard
{
    /// <summary>
    /// A data structure used for managing the configuration of text blocks within the output grid
    /// </summary>
    public struct TextBlockConfig
    {
        public string Text;
        public Thickness Margin;
        public VerticalAlignment VerticalAlignment;
        public HorizontalAlignment HorizontalAlignment;
        public TextAlignment TextAlignment;
        public int ColumnNumber;
        public int RowNumber;
        public Visibility ControlVisibility;

        public TextBlockConfig(string text, Thickness margin, VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment, TextAlignment textAlignment, int rowNumber, int columnNumber, Visibility controlVisibility)
        {
            this.Text = text;
            this.Margin = margin;
            this.VerticalAlignment = verticalAlignment;
            this.HorizontalAlignment = horizontalAlignment;
            this.TextAlignment = textAlignment;
            this.RowNumber = rowNumber;
            this.ColumnNumber = columnNumber;
            this.ControlVisibility = controlVisibility;
        }
    }
}
