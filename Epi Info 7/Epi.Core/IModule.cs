using System;
using System.ComponentModel;
using System.Data;
using Epi.Collections;

namespace Epi
{
	/// <summary>
	/// Public interface that is used to represent Epi Info application modules
	/// </summary>
	public interface IModule : IServiceProvider, IDisposable
    {
        #region Properties

        /// <summary>
        /// Epi Info module type 
        /// </summary>
        string ModuleName { get; }

        #endregion Properties

        #region Methods
        /// <summary>
		/// Method used to ask module to load itself
		/// </summary>
		void Load(IModuleManager moduleManager, ICommandLine commandLine);

		/// <summary>
		/// Method used to ask module to unload itself
		/// </summary>
		void Unload();
        /// <summary>
        /// Get Epi Info application module memory region.
        /// </summary>
        /// <returns></returns>
        //IMemoryRegion GetMemoryRegion();
        #endregion Methods
	}
}
