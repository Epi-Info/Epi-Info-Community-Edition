using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Data;
using Epi.Data.MySQL;
using MySql.Data.MySqlClient;
using MySql.Data;

namespace Epi.Data.MySQL.Forms
{
    /// <summary>
    /// Partial class for Data.SqlServer form ExistingConnectionStringDialog 
    /// </summary>
    public partial class ExistingConnectionStringDialog : Epi.Data.MySQL.Forms.ConnectionStringDialog
    {   
        /// <summary>
        /// Default constructor
        /// </summary>
        public ExistingConnectionStringDialog()
        {
            InitializeComponent();
        }

        private void ExistingConnectionStringDialog_Load(object sender, EventArgs e)
        {
            this.Text = "Connect to a MySQL database";
            lblInstructions.Text = "Enter information to connect to the MySQL database.";
        }
    }
}

