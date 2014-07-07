using System;
using Epi.Windows;
using EpiInfo.Plugin;

namespace Epi.Windows.Menu
{

	public class EntryPoint : IApplicationPlugin
	{

        private IApplicationPluginHost host;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();

            EntryPoint entryPoint = new EntryPoint();
            entryPoint.Start(args);
        }

        #region IApplicationPlugin Members

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public IApplicationPluginHost Host
        {
            get
            {
                return this.host;
            }
            set
            {
                this.host = value;
            }
        }

        public void Start(string[] args)
        {
            try
            {
                string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string executablePath = System.IO.Path.Combine(path, "EpiInfo.exe");
                string commandLine = "/l:Menu " + string.Join(" ", args);


//#if (DEBUG)
                ApplicationManager.Start(commandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
//#else
//            System.Diagnostics.Process.Start(executablePath, commandLine);
//#endif
            }
            catch
            {
                MsgBox.ShowError(SharedStrings.WARNING_APPLICATION_RUNNING);
            }
            finally
            {
            }
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
