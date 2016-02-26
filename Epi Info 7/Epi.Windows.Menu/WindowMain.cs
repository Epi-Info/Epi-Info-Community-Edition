using Epi.Windows.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Epi.Windows.Menu
{
    public partial class WindowMain : Form
    {
        private ElementHost host;
        MainWindow form = null;
        public WindowMain() 
        {
            InitializeComponent();
            //this.Text = "Epi Info 7 - Menu";// SharedStrings.MENU;
            host = new ElementHost();
            host.Dock = DockStyle.Fill; 
            form = new MainWindow();           
            host.Child = form;
            this.Controls.Add(host);
            this.FormClosed += new FormClosedEventHandler(WindowMain_FormClosed);
            form.Close += MainwindowClose;
        }       

        void WindowMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void MainwindowClose(object sender, System.Windows.RoutedEventArgs e)
        {
           // System.Windows.Application.Current.Shutdown();
           // ((System.Windows.Threading.DispatcherObject)(form.Parent)).Dispatcher.InvokeShutdown();
            this.Close();
        }

        private void WindowMain_Load(object sender, EventArgs e)
        {

        }

        

        

       
    }
}
