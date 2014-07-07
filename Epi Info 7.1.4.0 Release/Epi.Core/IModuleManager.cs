using System;
using System.Collections;

namespace Epi
{
	/// <summary>
	/// Interface class for the disposable Module Manager.
	/// </summary>
    public interface IModuleManager : IServiceProvider, IDisposable 
	{
		/// <summary>
		/// Factory method to create all module instances
		/// </summary>
        /// <param name="typeName">
        ///     The assembly-qualified name of the type to get. See System.Type.AssemblyQualifiedName.
        ///     If the type is in the currently executing assembly or in Mscorlib.dll, it
        ///     is sufficient to supply the type name qualified by its namespace.
        ///</param>
		/// <returns>A Reference to an instance of an object that implements IModule</returns>
		IModule CreateModuleInstance(string typeName);
		
		/// <summary>
		/// Formally load application module. See Observer pattern.
		/// </summary>
		/// <param name="module"></param>
		void Attach(IModule module);

		/// <summary>
        /// Formally unload application module. See Observer pattern.
		/// </summary>
		/// <param name="module"></param>
		void Detach(IModule module);

        /// <summary>
        /// Request that all modules unload
        /// </summary>
        void UnloadAll();

	}
}
