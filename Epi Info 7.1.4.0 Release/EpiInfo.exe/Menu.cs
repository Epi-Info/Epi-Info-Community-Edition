using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EpiInfo.Plugin;

namespace Epi
{
    public partial class Menu : Form, EpiInfo.Plugin.IApplicationPluginHost, EpiInfo.Plugin.ICommandPluginHost
    {
        public Menu()
        {
            InitializeComponent();
        }

        #region IApplicationPluginHost Members

        public bool Register(IApplicationPlugin applicationPlugin)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICommandPluginHost Members

        public bool Register(ICommandPlugin commandPlugin)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
