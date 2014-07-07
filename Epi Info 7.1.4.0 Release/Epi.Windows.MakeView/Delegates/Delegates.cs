using System;
using System.Windows.Forms;
using System.Drawing;
using Epi.Windows.Controls;
using Epi.Windows.MakeView.Forms;

namespace Epi.Windows.MakeView
{
    /// <summary>
    /// </summary>
    public delegate void ShowTabOrderEventHandler();
    /// <summary>
    /// </summary>
    public delegate void StartNewTabOrderEventHandler();
    /// <summary>
    /// </summary>
    public delegate void ContinueTabOrderEventHandler();
    /// <summary>
    /// Delegate for control movement
    /// </summary>
    /// <param name="sender">Object that fired the event</param>
    /// <param name="e">Field control event parameters</param> 
    public delegate void ControlMovementEventHandler(object sender, FieldControlEventArgs e);
    /// <summary>
    /// Delegate for field creation requests
    /// </summary>
    /// <param name="sender">The source object</param>
    /// <param name="fieldType">The field type</param>
    /// <param name="panel">The panel that got the request</param>
    /// <param name="location">The location of the request</param>
    public delegate void FieldCreationRequestEventHandler(object sender, MetaFieldType fieldType, Panel panel, Point location);
    /// <summary>
    /// Delegate for group creation requests
    /// </summary>
    /// <param name="panel">The panel that got the request</param>
    /// <param name="outline">The outline of the request</param>
    public delegate void GroupCreationRequestEventHandler(Panel panel, Rectangle outline);
    /// <summary>
    /// Delegate for tree node drag drops
    /// </summary>
    /// <param name="node">The node that was dragged and dropped</param>
    /// <param name="panel">The panel that the node was dropped on</param>
    /// <param name="location">The location of the drop</param>
    public delegate void TreeNodeDragDropEventHandler(TreeValueNode node, Panel panel, Point location);
    /// <summary>
    /// Delegate for page check code requests
    /// </summary>
    /// <param name="panel">The panel that requested check code</param>
    public delegate void PageCheckCodeRequestEventHandler(Panel panel);
    /// <summary>
    /// Delegate for Selected Controls Changes
    /// </summary>
    /// <param name="panel">The panel that got the request</param>
    /// <param name="selectedPanelArea">The selectedPanelArea of the request</param>
    public delegate void SelectedControlsChangedEventHandler(Rectangle selectedPanelArea);
    /// <summary>
    /// Delegate for the Cut menu item is selected
    /// </summary>
    public delegate void CutFieldControlEventHandler();
    /// <summary>
    /// Delegate for the Delete menu item is selected
    /// </summary>
    public delegate void DeleteFieldControlEventHandler();
    /// <summary>
    /// Delegate for the StackAlign menu item is selected
    /// </summary>
    public delegate void StackAlignEventHandler();
    /// <summary>
    /// Delegate for the TableAlign menu item is selected
    /// </summary>
    public delegate void TableAlignEventHandler(int numColumns);
    /// <summary>
    /// Delegate for the AlignAsRow menu item is selected
    /// </summary>
    public delegate void AlignAsRowEventHandler();
    /// <summary>
    /// Delegate for the MakeSame menu item is selected
    /// </summary>
    public delegate void MakeSameEventHandler(Enums.MakeSame makeSame);
    /// <summary>
    /// Delegate for the Copy menu item is selected
    /// </summary>
    public delegate void CopyFieldControlEventHandler();
    /// <summary>
    /// Delegate for the Paste menu item is selected
    /// </summary>
    /// <param name="point">mouse position</param>
    public delegate void PasteFieldControlEventHandler(System.Drawing.Point point);
    /// <summary>
    /// Delegate for the TableAlign menu item is selected
    /// </summary>
    public delegate void CreateTemplateEventHandler(string templateName);

    public delegate void ApplyDefaultFontsEventHandler();


}