using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Epi.Windows.Dialogs;


namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// The SelectDSNDialog class
    /// </summary>
    public partial class SelectDSNDialog : DialogBase
	{
		/// <summary>
		/// 
		/// </summary>
        [Obsolete("Use of default constructor not allowed", false)]
		public SelectDSNDialog()
		{
			InitializeComponent();
		}

		/* [DllImport("ODBC32.DLL", EntryPoint="SQLDataSources", SetLastError=true, CharSet=CharSet.Ansi, ExactSpelling=true, CallingConvention=CallingConvention.StdCall)] 
		private static extern short SQLDataSources( IntPtr EnvironmentHandle, ushort Direction, StringBuilder ServerName, short BufferLength1, ref short NameLength1Ptr, StringBuilder Description, 	short BufferLength2, ref short NameLength2Ptr); 
		[DllImport("ODBC32.DLL", EntryPoint="SQLAllocEnv", SetLastError=true,  CharSet=CharSet.Ansi, ExactSpelling=true,  CallingConvention=CallingConvention.StdCall)] 
		private static extern Int32 SQLAllocEnv(ref IntPtr env); 
		[DllImport("ODBC32.DLL", EntryPoint="SQLFreeEnv", SetLastError=true,  CharSet=CharSet.Ansi, ExactSpelling=true,  CallingConvention=CallingConvention.StdCall)] 
		private static extern Int32 SQLFreeEnv(IntPtr hEnv); 
		private const Int64 SQL_SUCCESS = 0;    // ODBC sucess 
		private const Int64 SQL_ERROR = -1;     // ODBC error 
		private const Int32 SQL_FETCH_NEXT = 1; // ODBC Move Next 

		private void GetdsnTable() 
		{ 
			Int16 RetCode, DSNLen = 0, DrvLen = 0; 
			StringBuilder DSNItem, DRVItem; 
			string DSN = string.Empty;
			IntPtr hEnv = IntPtr.Zero; 


			dsnTable.Clear(); 


			if (SQLAllocEnv(ref hEnv) != SQL_ERROR) 
			{ 
				do 
				{ 
					DSNItem = new StringBuilder(1024); 
					DRVItem = new StringBuilder(1024); 


					RetCode = SQLDataSources(hEnv, SQL_FETCH_NEXT, DSNItem, 1024, ref DSNLen, 
						DRVItem, 1024, ref DrvLen); 


					if (DSNItem != null) 
					{ 
						DSNItem.Length = DSNLen; 
						DSN = DSNItem.ToString(); 


						if ((DSN.Length > 0) && (DSN != new String(' ', 1024))) 
						{ 
							DRVItem.Length = DrvLen; 
							dsnTable.Add(DSN, DSN + " (" + DRVItem + ")"); 
						} 
					} 

				} 
				while (RetCode == SQL_SUCCESS); 
				SQLFreeEnv(hEnv); 
			} 
		}

		private void SelectDSNDialog_Load(object sender, System.EventArgs e)
		{
			GetdsnTable();
			
			foreach(string key in dsnTable.Keys)
			{
				cmbDataSource.Items.Add(dsnTable[key].ToString());
			}

		} 
		*/
		private void LoadDSNs() 
		{ 
			RegistryKey rootKey; 
			RegistryKey key; 
			String[] systemDsn; 
			// Attach System DSNs 
			rootKey = Registry.LocalMachine; 
			key = rootKey.OpenSubKey("SOFTWARE\\ODBC\\ODBC.INI\\ODBC Data Sources"); 
			systemDsn = key.GetValueNames(); 
			foreach(String dsnName in systemDsn) 
			{ 
				cmbDataSource.Items.Add(dsnName); 
			} 
			key.Close(); 
			rootKey.Close(); 


			/*// Attach User DSNs 

			rootKey = Registry.CurrentUser; 
			key = rootKey.OpenSubKey("SOFTWARE\\ODBC\\ODBC.INI\\ODBC Data Sources"); 
			systemDsn = key.GetValueNames(); 
			foreach(String dsnName in systemDsn) 
			{ 
				cmbDsn.Items.Add(dsnName); 
			} 
			key.Close(); 
			rootKey.Close(); 
			*/
		}

		private void SelectDSNDialog_Load(object sender, System.EventArgs e)
		{
			LoadDSNs();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
		
		}

		private void btnCreateNewDSN_Click(object sender, System.EventArgs e)
		{
		
		}

		private void btnReconfigureDSN_Click(object sender, System.EventArgs e)
		{
		
		}

	}
}



    

