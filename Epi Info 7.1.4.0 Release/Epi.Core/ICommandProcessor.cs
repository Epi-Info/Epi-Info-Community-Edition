using System;
using System.Collections;
using System.Collections.Generic;
using Epi.Collections;

namespace Epi
{
    /// <summary>
    /// CommandProcessor Interface
    /// </summary>
    public interface ICommandProcessor : IServiceProvider
    {

        /// <summary>
        /// Boolean to indicate that Execution is to be skipped
        /// </summary>
        bool SkipExecution { get; set;}

        /// <summary>
        /// Return a reference to CommandProcessorResults
        /// </summary>
        /// <remarks>
        /// Side effect - If there is no instance of CommandProcessorResults, it will be created
        /// </remarks>
        CommandProcessorResults Results { get; set;}

        /// <summary>
        /// returns a reference 
        /// </summary>
        /// <returns></returns>
        Stack Stack { get;}

        /// <summary>
        /// 
        /// </summary>
        NamedObjectCollection<SubRoutine> SubRoutines { get;}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void RunCommandBlock(object sender, EventArgs e);
    }
}