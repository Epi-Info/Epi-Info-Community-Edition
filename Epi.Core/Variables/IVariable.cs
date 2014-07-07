using System;
using System.Collections.Generic;
using System.Text;


namespace Epi
{
    /// <summary>
    /// Variable interface class.
    /// </summary>
    public interface IVariable : INamedObject
    {
        /// <summary>
        /// Variable data type enumeration.
        /// </summary>
        DataType DataType { get; set; }
        /// <summary>
        /// Variable type enumeration.
        /// </summary>
        VariableType VarType { get; }
        /// <summary>
        /// Variable expression.
        /// </summary>
        string Expression { get; set; }
        /// <summary>
        /// Variable prompt text.
        /// </summary>
        string PromptText { get; set;}
        /// <summary>
        /// Is variable <paramref name="typeCombination"/> type?
        /// </summary>
        /// <param name="typeCombination">Type combination variable type enumeration.</param>
        /// <returns>True/False result of variable type test.</returns>
        bool IsVarType(VariableType typeCombination);
    }
}
