using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EpiDashboard.Controls.GadgetProperties
{
    public class GadgetPropertiesPanelBase : UserControl, IGadgetPropertiesPanel
    {
        public RowFilterControl RowFilterControl { get; protected set; }
        public DataFilters DataFilters { get; protected set; }
        public DashboardHelper DashboardHelper { get; protected set; }
        public IGadget Gadget { get; protected set; }

        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        public GadgetPropertiesPanelBase()
            : base()
        {
        }

        protected void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (ChangesAccepted != null)
            {
                if (ValidateInput())
                {
                    CreateInputVariableList();
                    ChangesAccepted(this, new EventArgs());
                }
            }
        }

        protected virtual bool ValidateInput()
        {
            return true;
        }

        protected void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        protected void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        protected virtual void CreateInputVariableList() { }

        protected virtual void txtInput_PositiveIntegerOnly_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isNumPadNumeric = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
            bool isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9);

            if ((isNumeric || isNumPadNumeric) && Keyboard.Modifiers != ModifierKeys.None)
            {
                e.Handled = true;
                return;
            }
            bool isControl = ((Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift) || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Insert || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Tab || e.Key == Key.PageDown || e.Key == Key.PageUp || e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape || e.Key == Key.Home || e.Key == Key.End);
            e.Handled = !isControl && !isNumeric && !isNumPadNumeric;
        }

        protected virtual void txtInput_IntegerOnly_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isNumPadNumeric = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.OemMinus;
            bool isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9);

            if ((isNumeric || isNumPadNumeric) && Keyboard.Modifiers != ModifierKeys.None)
            {
                e.Handled = true;
                return;
            }
            bool isControl = ((Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift) || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Insert || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Tab || e.Key == Key.PageDown || e.Key == Key.PageUp || e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape || e.Key == Key.Home || e.Key == Key.End);
            e.Handled = !isControl && !isNumeric && !isNumPadNumeric;
        }

        protected virtual void CheckButtonStates(SettingsToggleButton sender)
        {
            StackPanel panelSidebar = this.FindName("panelSidebar") as StackPanel;
            if (panelSidebar != null)
            {
                foreach (UIElement element in panelSidebar.Children)
                {
                    if (element is SettingsToggleButton)
                    {
                        SettingsToggleButton tbtn = element as SettingsToggleButton;
                        if (tbtn != sender)
                        {
                            tbtn.IsChecked = false;
                        }
                    }
                }
            }
        }
    }
}
