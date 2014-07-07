using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using Epi.Collections;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

namespace Epi
{
    /// <summary>
    /// Memory Region interface class
    /// </summary>
    public interface IMemoryRegion
    {
        /// <summary>
        /// Create variable and place in memory region.
        /// </summary>
        /// <param name="variable">Variable to define.</param>
        void DefineVariable(IVariable variable);
        /// <summary>
        /// Remove variable from memory region.
        /// </summary>
        /// <param name="varName">Name of variable to remove.</param>
        void UndefineVariable(string varName);
        /// <summary>
        /// Get variable in memory region.
        /// </summary>
        /// <param name="varName">Name of variable to retrieve.</param>
        /// <returns>Variable in memory region.</returns>
        IVariable GetVariable(string varName);
        /// <summary>
        /// Is Variable In Scope flag.
        /// </summary>
        /// <param name="varName">Name of variable to test.</param>
        /// <returns>True/False on results of in scope test.</returns>
        bool IsVariableInScope(string varName);
        /// <summary>
        /// Get all variables in scope.
        /// </summary>
        /// <returns>All variables in scope.</returns>
        VariableCollection GetVariablesInScope();
        /// <summary>
        /// Get all variables of a certain type in scope
        /// </summary>
        /// <param name="scopeCombination">Types of variables by enumeration combination to retrieve.</param>
        /// <returns>All variables of a certain types in scope.</returns>
        VariableCollection GetVariablesInScope(VariableType scopeCombination);
        /// <summary>
        /// Remove variables of a certain type in scope.
        /// </summary>
        /// <param name="varTypes">Type of variable by enumeration combination to remove.</param>
        void RemoveVariablesInScope(VariableType varTypes);
        /// <summary>
        /// Push local memory region off stack.
        /// </summary>
        void PushLocalRegion();
        /// <summary>
        /// Push Local block memory region off stack.
        /// </summary>
        void PushLocalBlockRegion();
        /// <summary>
        /// Pop region on stack.
        /// </summary>
        void PopRegion();
        /// <summary>
        /// Get variables as a DataTable.
        /// </summary>
        /// <param name="varTypes">Type of variable by enumeration combination to retrieve.</param>
        /// <returns>All variables of a certain types in scope.</returns>
        DataTable GetVariablesAsDataTable(VariableType varTypes);

        /// <summary>
        /// Try to get variable
        /// </summary>
        /// <param name="p">string</param>
        /// <param name="var">variable</param>
        /// <returns>bool</returns>
        bool TryGetVariable(string p, out IVariable var);
    }
}
