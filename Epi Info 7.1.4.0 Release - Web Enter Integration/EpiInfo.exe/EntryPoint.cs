using System;
using System.IO;
using Epi.Windows;
using EpiInfo.Plugin;
using System.Reflection;

namespace Epi.Windows.EpiInfo
{
	/// <summary>
	/// Main entry point for Epi Info application
	/// </summary>
	public class EntryPoint : IApplicationPluginHost//, ICommandPluginHost 
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
            System.Windows.Forms.Application.EnableVisualStyles();

            EntryPoint entryPoint = new EntryPoint();
            entryPoint.StartUp(args);
		}

        private void StartUp(string[] args)
        {
            string PluginAssemblyName = System.Configuration.ConfigurationManager.AppSettings["PluginAssemblyName"].ToString();

            IApplicationPlugin ApplicationPlugin = null;

            // to do set the enter interpreter
            // application domain.
            Assembly a = Assembly.Load(PluginAssemblyName);
            // Get the type to use.
            Type myType = a.GetType(System.Configuration.ConfigurationManager.AppSettings["PluginStartupClass"].ToString());

            // Create an instance.
            ApplicationPlugin = (IApplicationPlugin)Activator.CreateInstance(myType);
            ApplicationPlugin.Host = this;


            WriteGC(ApplicationPlugin);
            // instantiate module launcher instance
            //ApplicationManager.Start(args);
            ApplicationPlugin.Start(args);
        }

        public bool Register(IApplicationPlugin applicationPlugin)
        {
            applicationPlugin.Host = this;
            return true;
        }


        private void WriteGC(object o)
        {
            Console.WriteLine("******* Garbage Collector Status ******");
            Console.WriteLine("Estimated bytes on heap: {0}", GC.GetTotalMemory(false));
            Console.WriteLine("This OS has {0} object generations.\n", GC.MaxGeneration + 1);
            Console.WriteLine("Generation of parameter object is: {0}",GC.GetGeneration(o));
        }

    }
}