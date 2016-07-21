using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.OleDb;

namespace Utilities.AppDataExtractor
{
    /// <summary>
    /// Output.txt Code Generator
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TextWriter tw = new StreamWriter("Output.txt");
            tw.WriteLine("#region Generated Code");
            // const string dbName = @"C:\Documents and Settings\KKM4\My Documents\Visual Studio 2005\Projects\AppDataExtractor\AppDataExtractor\AppData.mdb";
            //const string dbName = "AppData.mdb";
            const string dbName = @"C:\Epi Info 7 Development\Epi.Core\Resources\AppData.mdb";
            string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbName;
            OleDbConnection conn = new OleDbConnection(connString);
            conn.Open();
            DataTable schema = conn.GetSchema("Tables", new string[] { null, null, null, "Table" });
            foreach (DataRow row in schema.Rows)
            {
                string tableName = row["TABLE_NAME"].ToString();
                string propertyName = tableName + "DataTable";
                string fieldName = ToCamelCase(propertyName);
                string className = "DataSets.AppDataSet." + propertyName;
                tw.WriteLine("private " + className + "  " + fieldName + ";");
                tw.WriteLine("public " + className + "  " + propertyName);
                tw.WriteLine("{");
                tw.WriteLine("get");
                tw.WriteLine("{");
                tw.WriteLine("if (" + fieldName + " == null)");
                tw.WriteLine("{");
                tw.WriteLine(fieldName + " = new " + className + "();");
                string leftPart = fieldName + ".Add" + tableName + "Row(";
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select * from " + tableName;
                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                foreach (DataRow row2 in table.Rows)
                {
                    string statement = leftPart;
                    for (int col = 0; col < table.Columns.Count; col++)
                    {

                        if (row2[col] is DBNull)
                        {
                            statement += " string.Empty";
                        }
                        else if (row2[col] is string)
                        {
                            string val = row2[col].ToString();
                            if (val == string.Empty)
                                statement += " string.Empty";
                            else
                                statement += "@\"" + val + "\"";
                        }
                        else if (row2[col] is bool)
                        {
                            if (((bool)row2[col]) == true) statement += "true";
                            else statement += "false";
                        }
                        else statement += row2[col].ToString();
                        statement += ", ";
                    }
                    // Remove the last comma
                    statement = statement.Remove(statement.Length - 2, 2);
                    statement += ");";
                    tw.WriteLine(statement);
                }
                tw.WriteLine("}");
                tw.WriteLine("return (" + fieldName + ");");
                tw.WriteLine("}");
                tw.WriteLine("}");
                tw.WriteLine(Environment.NewLine + Environment.NewLine);
            }
            tw.WriteLine("#endregion Generated Code");
            tw.Close();
            conn.Close();
        }
        private static string ToCamelCase(string str)
        {
            return (str.Substring(0, 1).ToLowerInvariant() + str.Substring(1, str.Length - 1));
        }
        private static string ToPascalCase(string str)
        {
            return (str.Substring(0, 1).ToUpperInvariant() + str.Substring(1, str.Length - 1));
        }
    }
}