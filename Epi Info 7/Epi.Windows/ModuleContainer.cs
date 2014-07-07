using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Epi.Windows
{
        /// <summary>
        /// Container that encapsulates and tracks components of the module
        /// </summary>
		public class ModuleContainer : Container
        {
        IServiceProvider serviceProvider = null;

        /// <summary>
        /// The default container
        /// </summary>
        /// <param name="provider">The service provider</param>
        public ModuleContainer(IServiceProvider provider)
        {
            this.serviceProvider = provider;
        }

        /// <summary>
        /// Gets the service for a specified type                
        /// </summary>
        /// <returns>An object that represents a service for the specified type </returns>
        [System.Diagnostics.DebuggerStepThrough()]
        protected override object GetService(Type serviceType)
        {
            // first look in epi info
            object service = serviceProvider.GetService(serviceType);
            if (service == null)
            {
                // call into .net framework if service wasn't found
                service = base.GetService(serviceType);
            }
            return service;
        }
    }
}