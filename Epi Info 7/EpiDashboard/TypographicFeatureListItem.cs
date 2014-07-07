﻿using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;

namespace EpiDashboard
{
    internal class TypographicFeatureListItem : TextBlock, IComparable
    {
        private readonly string _displayName;
        private readonly DependencyProperty _chooserProperty;

        public TypographicFeatureListItem(string displayName, DependencyProperty chooserProperty)
        {
            _displayName = displayName;
            _chooserProperty = chooserProperty;
            this.Text = displayName;
        }

        public DependencyProperty ChooserProperty
        {
            get { return _chooserProperty; }
        }

        public override string ToString()
        {
            return _displayName;
        }

        int IComparable.CompareTo(object obj)
        {
            return string.Compare(_displayName, obj.ToString(), true, CultureInfo.CurrentCulture);
        }
    }
}
