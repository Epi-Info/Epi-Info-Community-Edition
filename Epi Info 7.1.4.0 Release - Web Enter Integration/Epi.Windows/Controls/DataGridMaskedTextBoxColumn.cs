using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using Epi.Core;
using Epi.Fields;

namespace Epi.Windows.Controls
{
    /// <summary>
    /// MaskedTextBoxCell
    /// </summary>
    public class MaskedTextBoxCell : DataGridViewTextBoxCell
    {
        private string mask;
        private char promptChar;
        private DataGridViewTriState includePrompt;
        private DataGridViewTriState includeLiterals;
        private DataGridViewTriState hidePromptOnLeave;
        private Type validatingType;
        private GridColumnBase gridColumn;

        /// <summary>
        /// MaskInputRejectedEventHandler
        /// </summary>
        public static MaskInputRejectedEventHandler control_MaskInputRejected;

        //=------------------------------------------------------------------=
        // MaskedTextBoxCell
        //=------------------------------------------------------------------=
        /// <summary>
        ///   Initializes a new instance of this class.  Fortunately, there's
        ///   not much to do here except make sure that our base class is 
        ///   also initialized properly.
        /// </summary>
        public MaskedTextBoxCell()
            : base()
        {
            this.mask = "";
            this.promptChar = '_';
            this.includePrompt = DataGridViewTriState.NotSet;
            this.includeLiterals = DataGridViewTriState.NotSet;
            this.hidePromptOnLeave = DataGridViewTriState.NotSet;
            this.validatingType = typeof(string);
        }

        /// <summary>
        ///   Whenever the user is to begin editing a cell of this type, the editing
        ///   control must be created, which in this column type's
        ///   case is a subclass of the MaskedTextBox control.
        /// 
        ///   This routine sets up all the properties and values
        ///   on this control before the editing begins.
        /// </summary>
        /// <param name="rowIndex">Integer Row Index</param>
        /// <param name="initialFormattedValue">Initial Formatted value</param>
        /// <param name="dataGridViewCellStyle">Data Grid View Cell Style</param>
        public override void InitializeEditingControl(int rowIndex,
                                                      object initialFormattedValue,
                                                      DataGridViewCellStyle dataGridViewCellStyle)
        {
            MaskedTextBoxEditingControl mtbec;
            MaskedTextBoxColumn mtbcol;
            DataGridViewColumn dgvc;

            base.InitializeEditingControl(rowIndex, initialFormattedValue,
                                          dataGridViewCellStyle);

            mtbec = DataGridView.EditingControl as MaskedTextBoxEditingControl;

            // set up properties that are specific to the MaskedTextBox

            dgvc = this.OwningColumn;
            if (dgvc is MaskedTextBoxColumn)
            {
                mtbcol = dgvc as MaskedTextBoxColumn;

                // get the mask from this instance or the parent column.
                if (string.IsNullOrEmpty(this.mask))
                {
                    mtbec.Mask = mtbcol.Mask;
                }
                else
                {
                    mtbec.Mask = this.mask;
                }

                // prompt char.
                mtbec.PromptChar = this.PromptChar;

                // IncludePrompt
                if (this.includePrompt == DataGridViewTriState.NotSet)
                {
                    //mtbec.IncludePrompt = mtbcol.IncludePrompt;
                }
                else
                {
                    //mtbec.IncludePrompt = BoolFromTri(this.includePrompt);
                }

                // IncludeLiterals
                if (this.includeLiterals == DataGridViewTriState.NotSet)
                {
                    //mtbec.IncludeLiterals = mtbcol.IncludeLiterals;
                }
                else
                {
                    //mtbec.IncludeLiterals = BoolFromTri(this.includeLiterals);
                }

                if (this.hidePromptOnLeave == DataGridViewTriState.NotSet)
                {
                    mtbec.HidePromptOnLeave = false;
                }
                else
                {
                    mtbec.HidePromptOnLeave = BoolFromTri(this.hidePromptOnLeave);
                }

                // validating type 
                if (this.ValidatingType == null)
                {
                    mtbec.ValidatingType = mtbcol.ValidatingType;
                }
                else
                {
                    mtbec.ValidatingType = this.ValidatingType;
                }

                mtbec.GridColumn = mtbcol.GridColumn;

                string fmt;
                string textValue;

                if (this.RowIndex == -1)
                {
                    mtbec.Text = initialFormattedValue.ToString();
                }
                else
                {
                    try
                    {
                        if (this.Value == null || this.Value == DBNull.Value)
                        {
                            mtbec.Text = string.Empty;
                        }
                        else if (this.Value is DateTime)
                        {
                            fmt = "{0:" + dataGridViewCellStyle.Format + "}";
                            textValue = string.Format(fmt, this.Value);
                            mtbec.Text = textValue;
                        }
                        else if (this.Value is Double && !string.IsNullOrEmpty(dataGridViewCellStyle.Format))
                        {
                            string format = dataGridViewCellStyle.Format;
                            fmt = format.Substring(0, format.IndexOf('.'));
                            fmt = fmt.Replace("#", "0");
                            fmt += format.Substring(format.IndexOf('.'));
                            fmt = "{0:" + fmt + "}";
                            textValue = string.Format(fmt, this.Value);
                            mtbec.Text = textValue;
                        }
                        else
                        {
                            mtbec.Text = this.Value.ToString();
                        }
                    }
                    catch { }
                }

                mtbec.MaskInputRejected -= control_MaskInputRejected;
                mtbec.MaskInputRejected += control_MaskInputRejected;
            }
        }

        /// <summary>
        /// Grid Column
        /// </summary>
        public virtual GridColumnBase GridColumn
        {
            get
            {
                return this.gridColumn;
            }
            set
            {
                this.gridColumn = value;
            }
        }


        /// <summary>
        /// Returns the type of the control that will be used for editing
        ///  cells of this type.  This control must be a valid Windows Forms
        ///  control and must implement IDataGridViewEditingControl.
        /// </summary>
        public override Type EditType
        {
            get
            {
                return typeof(MaskedTextBoxEditingControl);
            }
        }


        /// <summary>
        /// A string value containing the Mask against input for cells of
        ///   this type will be verified.
        /// </summary>
        public virtual string Mask
        {
            get
            {
                return this.mask;
            }
            set
            {
                this.mask = value;
            }
        }


        /// <summary>
        /// The character to use for prompting for new input.
        /// </summary>
        public virtual char PromptChar
        {
            get
            {
                return this.promptChar;
            }
            set
            {
                this.promptChar = value;
            }
        }

        /// <summary>
        /// DGV tristate (boolean and NotSet value) whether to hide prompt values on leave.
        /// </summary>
        public virtual DataGridViewTriState HidePromptOnLeave
        {
            get
            {
                return this.hidePromptOnLeave;
            }
            set
            {
                this.hidePromptOnLeave = value;
            }
        }

  
        /// <summary>
        /// A boolean indicating whether to include prompt characters in
        ///  the Text property's value.
        /// </summary>
        public virtual DataGridViewTriState IncludePrompt
        {
            get
            {
                return this.includePrompt;
            }
            set
            {
                this.includePrompt = value;
            }
        }

        /// <summary>
        /// A boolean value indicating whether to include literal characters
        ///  in the Text property's output value.
        /// </summary>
        public virtual DataGridViewTriState IncludeLiterals
        {
            get
            {
                return this.includeLiterals;
            }
            set
            {
                this.includeLiterals = value;
            }
        }

        /// <summary>
        /// A Type object for the validating type.
        /// </summary>
        public virtual Type ValidatingType
        {
            get
            {
                return this.validatingType;
            }
            set
            {
                this.validatingType = value;
            }
        }
 
        /// <summary>
        /// Quick routine to convert from DataGridViewTriState to boolean.
        ///   True goes to true while False and NotSet go to false.
        /// </summary>
        /// <param name="tri">DataGridViewTriState</param>
        /// <returns>boolean</returns>
        protected static bool BoolFromTri(DataGridViewTriState tri)
        {
            return (tri == DataGridViewTriState.True) ? true : false;
        }
    }

    /// <summary>
    /// The base object for the custom column type.  Programmers manipulate
    ///  the column types most often when working with the DataGridView, and
    ///  this one sets the basics and Cell Template values controlling the
    ///  default behaviour for cells of this column type.
    /// </summary>
    public class MaskedTextBoxColumn : DataGridViewColumn
    {
        private string mask;
        private char promptChar;
        private bool includePrompt;
        private bool includeLiterals;
        private Type validatingType;
        private GridColumnBase gridColumn;
        private DataGridViewTriState hidePromptOnLeave;
        private MaskInputRejectedEventHandler control_MaskInputRejected;

        /// <summary>
        /// Initializes a new instance of this class, making sure to pass
        ///  to its base constructor an instance of a MaskedTextBoxCell 
        ///  class to use as the basic template.
        /// </summary>
        public MaskedTextBoxColumn()
            : base(new MaskedTextBoxCell())
        {
        }

        //  Routine to convert from boolean to DataGridViewTriState.
        private static DataGridViewTriState TriBool(bool value)
        {
            return value ? DataGridViewTriState.True
                         : DataGridViewTriState.False;
        }

        /// <summary>
        /// Grid Column
        /// </summary>
        public GridColumnBase GridColumn
        {
            get
            {
                return this.gridColumn;
            }
            set
            {
                MaskedTextBoxCell mtbc;
                DataGridViewCell dgvc;
                int rowCount;

                if (this.gridColumn != value)
                {
                    this.gridColumn = value;

                    // first, update the value on the template cell.
                    mtbc = (MaskedTextBoxCell)this.CellTemplate;
                    mtbc.GridColumn = value;

                    // now set it on all cells in other rows as well.
                    if (this.DataGridView != null && this.DataGridView.Rows != null)
                    {
                        rowCount = this.DataGridView.Rows.Count;
                        for (int x = 0; x < rowCount; x++)
                        {
                            dgvc = this.DataGridView.Rows.SharedRow(x).Cells[x];
                            if (dgvc is MaskedTextBoxCell)
                            {
                                mtbc = (MaskedTextBoxCell)dgvc;
                                mtbc.GridColumn = value;
                            }
                        }
                    }
                }
            }
        }
  
        /// <summary>
        /// The template cell that will be used for this column by default,
        ///  unless a specific cell is set for a particular row.
        ///
        ///  A MaskedTextBoxCell cell which will serve as the template cell
        ///  for this column.
        /// </summary>
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }

            set
            {
                //  Only cell types that derive from MaskedTextBoxCell are supported as the cell template.
                if (value != null && !value.GetType().IsAssignableFrom(typeof(MaskedTextBoxCell)))
                {
                    string s = "Cell type is not based upon the MaskedTextBoxCell.";//CustomColumnMain.GetResourceManager().GetString("excNotMaskedTextBox");
                    throw new InvalidCastException(s);
                }

                base.CellTemplate = value;
            }
        }

        /// <summary>
        /// HidePromptOnLeave
        /// </summary>
        public virtual DataGridViewTriState HidePromptOnLeave
        {
            get
            {
                return this.hidePromptOnLeave;
            }
            set
            {
                MaskedTextBoxCell mtbc;
                DataGridViewCell dgvc;
                int rowCount;

                if (this.hidePromptOnLeave != value)
                {
                    this.hidePromptOnLeave = value;

                    // first, update the value on the template cell.
                    mtbc = (MaskedTextBoxCell)this.CellTemplate;
                    mtbc.HidePromptOnLeave = value;

                    // now set it on all cells in other rows as well.
                    if (this.DataGridView != null && this.DataGridView.Rows != null)
                    {
                        rowCount = this.DataGridView.Rows.Count;
                        for (int x = 0; x < rowCount; x++)
                        {
                            dgvc = this.DataGridView.Rows.SharedRow(x).Cells[x];
                            if (dgvc is MaskedTextBoxCell)
                            {
                                mtbc = (MaskedTextBoxCell)dgvc;
                                mtbc.HidePromptOnLeave = value;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// MaskInputRejected
        /// </summary>
        public virtual MaskInputRejectedEventHandler MaskInputRejected
        {
            set
            {
                if (this.control_MaskInputRejected != value)
                {
                    this.control_MaskInputRejected = value;
                    MaskedTextBoxCell.control_MaskInputRejected = value;
                }
            }
        }

        /// <summary>
        /// Indicates the Mask property that is used on the MaskedTextBox
        ///  for entering new data into cells of this type.
        /// 
        ///  See the MaskedTextBox control documentation for more details.
        /// </summary>
        public virtual string Mask
        {
            get
            {
                return this.mask;
            }
            set
            {
                MaskedTextBoxCell mtbc;
                DataGridViewCell dgvc;
                int rowCount;

                if (this.mask != value)
                {
                    this.mask = value;

                    // first, update the value on the template cell.
                    mtbc = (MaskedTextBoxCell)this.CellTemplate;
                    mtbc.Mask = value;

                    // now set it on all cells in other rows as well.
                    if (this.DataGridView != null && this.DataGridView.Rows != null)
                    {
                        rowCount = this.DataGridView.Rows.Count;
                        for (int x = 0; x < rowCount; x++)
                        {
                            dgvc = this.DataGridView.Rows.SharedRow(x).Cells[x];
                            if (dgvc is MaskedTextBoxCell)
                            {
                                mtbc = (MaskedTextBoxCell)dgvc;
                                mtbc.Mask = value;

                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// By default, the MaskedTextBox uses the underscore (_) character
        ///  to prompt for required characters.  This propertly lets you 
        ///  choose a different one.
        /// 
        ///  See the MaskedTextBox control documentation for more details.
        /// </summary>
        public virtual char PromptChar
        {
            get
            {
                return this.promptChar;
            }
            set
            {
                MaskedTextBoxCell mtbc;
                DataGridViewCell dgvc;
                int rowCount;

                if (this.promptChar != value)
                {
                    this.promptChar = value;

                    //
                    // first, update the value on the template cell.
                    //
                    mtbc = (MaskedTextBoxCell)this.CellTemplate;
                    mtbc.PromptChar = value;

                    //
                    // now set it on all cells in other rows as well.
                    //
                    if (this.DataGridView != null && this.DataGridView.Rows != null)
                    {
                        rowCount = this.DataGridView.Rows.Count;
                        for (int x = 0; x < rowCount; x++)
                        {
                            dgvc = this.DataGridView.Rows.SharedRow(x).Cells[x];
                            if (dgvc is MaskedTextBoxCell)
                            {
                                mtbc = (MaskedTextBoxCell)dgvc;
                                mtbc.PromptChar = value;
                            }
                        }
                    }
                }
            }
        }

        
        /// <summary>
        /// Indicates whether any unfilled characters in the mask should be
        ///   be included as prompt characters when somebody asks for the text
        ///   of the MaskedTextBox for a particular cell programmatically.
        /// 
        ///   See the MaskedTextBox control documentation for more details.
        /// </summary>
        public virtual bool IncludePrompt
        {
            get
            {
                return this.includePrompt;
            }
            set
            {
                MaskedTextBoxCell mtbc;
                DataGridViewCell dgvc;
                int rowCount;

                if (this.includePrompt != value)
                {
                    this.includePrompt = value;

                    // first, update the value on the template cell.
                    mtbc = (MaskedTextBoxCell)this.CellTemplate;
                    mtbc.IncludePrompt = TriBool(value);

                    // now set it on all cells in other rows as well.
                    if (this.DataGridView != null && this.DataGridView.Rows != null)
                    {
                        rowCount = this.DataGridView.Rows.Count;
                        for (int x = 0; x < rowCount; x++)
                        {
                            dgvc = this.DataGridView.Rows.SharedRow(x).Cells[x];
                            if (dgvc is MaskedTextBoxCell)
                            {
                                mtbc = (MaskedTextBoxCell)dgvc;
                                mtbc.IncludePrompt = TriBool(value);
                            }
                        }
                    }
                }
            }
        }
 
        /// <summary>
        /// Controls whether or not literal (non-prompt) characters should
        ///  be included in the output of the Text property for newly entered
        ///  data in a cell of this type.
        /// 
        ///  See the MaskedTextBox control documentation for more details.
        /// </summary>
        public virtual bool IncludeLiterals
        {
            get
            {
                return this.includeLiterals;
            }
            set
            {
                MaskedTextBoxCell mtbc;
                DataGridViewCell dgvc;
                int rowCount;

                if (this.includeLiterals != value)
                {
                    this.includeLiterals = value;

                    // first, update the value on the template cell.
                    mtbc = (MaskedTextBoxCell)this.CellTemplate;
                    mtbc.IncludeLiterals = TriBool(value);

                    // now set it on all cells in other rows as well.
                    if (this.DataGridView != null && this.DataGridView.Rows != null)
                    {

                        rowCount = this.DataGridView.Rows.Count;
                        for (int x = 0; x < rowCount; x++)
                        {
                            dgvc = this.DataGridView.Rows.SharedRow(x).Cells[x];
                            if (dgvc is MaskedTextBoxCell)
                            {
                                mtbc = (MaskedTextBoxCell)dgvc;
                                mtbc.IncludeLiterals = TriBool(value);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Indicates the type against any data entered in the MaskedTextBox
        ///  should be validated.  The MaskedTextBox control will attempt to
        ///  instantiate this type and assign the value from the contents of
        ///  the text box.  An error will occur if it fails to assign to this
        ///  type.
        ///
        ///  See the MaskedTextBox control documentation for more details.
        /// </summary>
        public virtual Type ValidatingType
        {
            get
            {
                return this.validatingType;
            }
            set
            {
                MaskedTextBoxCell mtbc;
                DataGridViewCell dgvc;
                int rowCount;

                if (this.validatingType != value)
                {
                    this.validatingType = value;

                    // first, update the value on the template cell.
                    mtbc = (MaskedTextBoxCell)this.CellTemplate;
                    mtbc.ValidatingType = value;

                    // now set it on all cells in other rows as well.
                    if (this.DataGridView != null && this.DataGridView.Rows != null)
                    {
                        rowCount = this.DataGridView.Rows.Count;
                        for (int x = 0; x < rowCount; x++)
                        {
                            dgvc = this.DataGridView.Rows.SharedRow(x).Cells[x];
                            if (dgvc is MaskedTextBoxCell)
                            {
                                mtbc = (MaskedTextBoxCell)dgvc;
                                mtbc.ValidatingType = value;
                            }
                        }
                    }
                }
            }
        }

    }

    /// <summary>
    /// class DataGridMaskedTextBoxColumn
    /// </summary>
    public class MaskedTextBoxEditingControl : MaskedTextBox, IDataGridViewEditingControl
    {
        /// <summary>
        /// rowIndex
        /// </summary>
        protected int rowIndex;

        /// <summary>
        /// gridColumn
        /// </summary>
        protected GridColumnBase gridColumn;

        /// <summary>
        /// dataGridView
        /// </summary>
        protected DataGridView dataGridView;

        /// <summary>
        /// valueChanged
        /// </summary>
        protected bool valueChanged = false;

        /// <summary>
        /// Masked Text Box Editing Control
        /// </summary>
        public MaskedTextBoxEditingControl()
        {

        }
        /// <summary>
        /// Grid Column
        /// </summary>
        public GridColumnBase GridColumn
        {
            get
            {
                return gridColumn;
            }
            set
            {
                this.gridColumn = value;
            }
        }

        /// <summary>
        /// OnTextChanged
        /// </summary>
        /// <param name="e">Event</param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            // Let the DataGridView know about the value change
            NotifyDataGridViewOfValueChange();
        }

        /// <summary>
        /// Notify DataGridView that the value has changed.
        /// </summary>
        protected virtual void NotifyDataGridViewOfValueChange()
        {
            this.valueChanged = true;
            if (this.dataGridView != null)
            {
                this.dataGridView.NotifyCurrentCellDirty(true);
            }
        }



        #region IDataGridViewEditingControl Members

      
        /// <summary>
        /// Indicates the cursor that should be shown when the user hovers their
        ///  mouse over this cell when the editing control is shown.
        /// </summary>
        public Cursor EditingPanelCursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }

 
        /// <summary>
        /// Returns or sets the parent DataGridView.
        /// </summary>
        public DataGridView EditingControlDataGridView
        {
            get
            {
                return this.dataGridView;
            }

            set
            {
                this.dataGridView = value;
            }
        }


        /// <summary>
        /// Sets/Gets the formatted value contents of this cell.
        /// </summary>
        public object EditingControlFormattedValue
        {
            set
            {
                this.Text = value.ToString();
                NotifyDataGridViewOfValueChange();
            }
            get
            {
                return this.Text;
            }

        }
  
        /// <summary>
        /// Get the value of the editing control for formatting.
        /// </summary>
        /// <param name="context">DataGridViewDataErrorContexts</param>
        /// <returns>object</returns>
        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return this.Text;
        }

        /// <summary>
        /// Process input key and determine if the key should be used for the editing control
        ///  or allowed to be processed by the grid. Handle cursor movement keys for the MaskedTextBox
        ///  control; otherwise if the DataGridView doesn't want the input key then let the editing control handle it.
        /// </summary>
        /// <param name="keyData">Keys keyData</param>
        /// <param name="dataGridViewWantsInputKey">bool dataGridViewWantsInputKey</param>
        /// <returns></returns>
        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Right:
                    // If the end of the selection is at the end of the string
                    // let the DataGridView treat the key message
                    if (!(this.SelectionLength == 0
                          && this.SelectionStart == this.ToString().Length))
                    {
                        return true;
                    }
                    break;

                case Keys.Left:
                    // If the end of the selection is at the begining of the
                    // string or if the entire text is selected send this character 
                    // to the dataGridView; else process the key event.
                    if (!(this.SelectionLength == 0
                          && this.SelectionStart == 0))
                    {
                        return true;
                    }
                    break;

                case Keys.Home:
                case Keys.End:
                    if (this.SelectionLength != this.ToString().Length)
                    {
                        return true;
                    }
                    break;

                case Keys.Prior:
                case Keys.Next:
                    if (this.valueChanged)
                    {
                        return true;
                    }
                    break;

                case Keys.Delete:
                    if (this.SelectionLength > 0 || this.SelectionStart < this.ToString().Length)
                    {
                        return true;
                    }
                    break;
            }

            // defer to the DataGridView and see if it wants it.
            return !dataGridViewWantsInputKey;
        }


        /// <summary>
        /// Prepare the editing control for edit.
        /// </summary>
        /// <param name="selectAll">bool selectAll</param>
        public void PrepareEditingControlForEdit(bool selectAll)
        {
            if (selectAll)
            {
                SelectAll();
            }
            else
            {
                // Do not select all the text, but position the caret at the 
                // end of the text.
                this.SelectionStart = this.ToString().Length;
            }
        }
 
        /// <summary>
        /// Indicates whether or not the parent DataGridView control should
        ///  reposition the editing control every time value change is indicated.
        ///  There is no need to do this for the MaskedTextBox.
        /// </summary>
        public bool RepositionEditingControlOnValueChange
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Indicates the row index of this cell.  This is often -1 for the
        ///  template cell, but for other cells, might actually have a value
        ///  greater than or equal to zero.
        /// </summary>
        public int EditingControlRowIndex
        {
            get
            {
                return this.rowIndex;
            }

            set
            {
                this.rowIndex = value;
            }
        }



        /// <summary>
        /// Make the MaskedTextBox control match the style and colors of
        ///  the host DataGridView control and other editing controls 
        ///  before showing the editing control.
        /// </summary>
        /// <param name="dataGridViewCellStyle">DataGridViewCellStyle dataGridViewCellStyle</param>
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.Font = dataGridViewCellStyle.Font;
            this.ForeColor = dataGridViewCellStyle.ForeColor;
            this.BackColor = dataGridViewCellStyle.BackColor;
            this.TextAlign = translateAlignment(dataGridViewCellStyle.Alignment);
        }

 
        /// <summary>
        /// Gets or sets our flag indicating whether the value has changed.
        /// </summary>
        public bool EditingControlValueChanged
        {
            get
            {
                return valueChanged;
            }

            set
            {
                this.valueChanged = value;
            }
        }

        #endregion // IDataGridViewEditingControl.

        ///   Routine to translate between DataGridView
        ///   content alignments and text box horizontal alignments.
        private static HorizontalAlignment translateAlignment(DataGridViewContentAlignment align)
        {
            switch (align)
            {
                case DataGridViewContentAlignment.TopLeft:
                case DataGridViewContentAlignment.MiddleLeft:
                case DataGridViewContentAlignment.BottomLeft:
                    return HorizontalAlignment.Left;

                case DataGridViewContentAlignment.TopCenter:
                case DataGridViewContentAlignment.MiddleCenter:
                case DataGridViewContentAlignment.BottomCenter:
                    return HorizontalAlignment.Center;

                case DataGridViewContentAlignment.TopRight:
                case DataGridViewContentAlignment.MiddleRight:
                case DataGridViewContentAlignment.BottomRight:
                    return HorizontalAlignment.Right;
            }

            throw new ArgumentException("Error: Invalid Content Alignmentfs");
        }


    }
}
