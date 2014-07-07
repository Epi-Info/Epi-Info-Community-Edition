using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using Epi.Collections;

namespace Epi
{
	/// <summary>
	/// Epi Info application module abstract type
	/// </summary>
    public abstract class ModuleBase : IModule
    {
        #region Private Attributes
        //private Project currentProject;
        //private IProjectManager projectManager;

        /// <summary>
        /// Memory Region
        /// </summary>
        protected IMemoryRegion memoryRegion;
        #endregion Private Attributes

        #region Protected Attributes
        /// <summary>
        /// Command line
        /// </summary>
        protected ICommandLine commandLine;
        
        /// <summary>
        /// Module Manager
        /// </summary>
		protected IModuleManager moduleManager;
        #endregion Protected Attributes

        #region Constructors
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ModuleBase()
		{
            memoryRegion = new MemoryRegion();
        }
        #endregion

        #region Public Properties
        
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Returns Memory Region
        /// </summary>
        /// <returns></returns>
        public IMemoryRegion GetMemoryRegion()
        {
            /*
            if (Epi.Core.Interpreter.Reduction.MemoryRegion == null)
            {
                Epi.Core.Interpreter.Reduction.MemoryRegion = new MemoryRegion();
            }

            return Epi.Core.Interpreter.Reduction.MemoryRegion;*/

            return null;
        }
        #endregion Public Methods

        #region Protected IModule Implementation

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="moduleManager">Module Manager</param>
        /// <param name="commandLine">Command line</param>
        protected virtual void Load(IModuleManager moduleManager, ICommandLine commandLine)
        {
            this.moduleManager = moduleManager;
            this.commandLine = commandLine;
        }

		/// <summary>
		/// Signal load completion and register with module manager
		/// </summary>
		protected void OnLoaded()
		{
			// module is responsible for formally loading itself into the application using
			// the module manager.
			if (this.moduleManager != null)
			{
				this.moduleManager.Attach(this);
			}
		}
		
        /// <summary>
        /// Request module to unload
        /// </summary>
		protected abstract void Unload();

        /// <summary>
        /// Signal unload completion and unregister from module manager
        /// </summary>
		protected void OnUnloaded()
		{
			// module is also responsible for formally unloading itself from the application 
			// using the module manager. If module manager is null, there is a chance that
            // we've already unloaded the module using the module manager
			if (this.moduleManager != null)
			{
				this.moduleManager.Detach(this);

                // prevent module from attempting to unload from module manager a second time
                this.moduleManager = null;
			}
        }

        /// <summary>
        /// Return type of module to caller
        /// </summary>
        protected abstract string ModuleName { get; }
        
        /// <summary>
        /// Dispose method
        /// </summary>
        public virtual void Dispose()
        {
            // make sure module is unloaded
            this.OnUnloaded();
        }
        #endregion

        #region Explicit IProjectManager Implementation


        #endregion

        #region Public Service Provider Implementation
        /// <summary>
		/// <para> Returns an object that represents a service provided by the
		/// <see cref="T:System.ComponentModel.Component" /> or by its <see cref="T:System.ComponentModel.Container" />.
		/// </para>
		/// </summary>
		/// <param name="serviceType">A service provided by the <see cref="T:System.ComponentModel.Component" />. </param>
		/// <returns>
		/// <para> An <see cref="T:System.Object" /> that represents a service provided by the
		/// <see cref="T:System.ComponentModel.Component" />.
		/// </para>
		/// <para> This value is <see langword="null" /> if the <see cref="T:System.ComponentModel.Component" /> does not provide the
		/// specified service.</para>
		/// </returns>
        [System.Diagnostics.DebuggerStepThrough()]
		public virtual object GetService(Type serviceType)
		{
			object service = null;

            if (serviceType == this.GetType())
            {
                return this;
            }
            else if (serviceType == typeof(IModule))
            {
                return this;
            }
            else if (serviceType == typeof(ICommandLine))
            {
                return this.commandLine;
            }
            // chain of responsibility pattern
            else if (this.moduleManager != null)
            {
                service = this.moduleManager.GetService(serviceType);
            }
        
			return service;
        }
        #endregion

        #region Project Management
        ///// <summary>
        ///// Module's active project object
        ///// </summary>
        //Project IProjectHost.CurrentProject
        //{
        //    get
        //    {
        //        return currentProject;
        //    }
        //    set
        //    {
        //        currentProject = value;

        //    }

        //}


        /// <summary>
        /// Returns module manager
        /// </summary>
        public IModuleManager ModuleManager
        {
            get
            {
                return moduleManager;
            }
        }

        /// <summary>
        /// Command line
        /// </summary>
        public ICommandLine CommandLine
        {
            get
            {
                return commandLine;
            }
        }

        //public IProjectManager ProjectManager
        //{
        //    get
        //    {
        //        return projectManager;
        //    }
        //}
        #endregion

        #region Explicit IModule Routing
        // explicit interface implementation is used to hide members from intellisense, while
        // still allow polymorphism
        string IModule.ModuleName 
        {
            get
            {
                return this.ModuleName;
            }
        }

        void IModule.Load(IModuleManager moduleManager, ICommandLine commandLine)
		{
            this.Load(moduleManager, commandLine);
		}

		void IModule.Unload()
		{
			this.Unload();
        }
        #endregion

    }
}
