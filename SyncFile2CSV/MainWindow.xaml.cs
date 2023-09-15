using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security;
using System.Security.Cryptography;
using System.Xml;
using System.Windows.Forms;
using System.IO;

namespace SyncFile2CSV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
    	private const string initVectorDroid = "00000000000000000000000000000000";
		private const string saltDroid = "00000000000000000000";

		private string listSeparator = ",";
		string pathCandidate = string.Empty;

        public MainWindow()
        {
            string[] args = Environment.GetCommandLineArgs();

            InitializeComponent();
            listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            if (args.Length > 1)
            {
                if (args[1] != null)
                {
                    pathCandidate = args[1];

                    if (System.IO.File.GetAttributes(pathCandidate) != System.IO.FileAttributes.Directory)
                    {
                        fileName.Text = pathCandidate;
                    }

                    if (System.IO.File.GetAttributes(pathCandidate) == System.IO.FileAttributes.Directory)
                    {
                        folderPath.Text = pathCandidate;
                    }
                }

                if (args[2] != null)
                {
                    passwordBox1.Password = args[2];
                }

				run.IsEnabled = !passwordBox1.Password.Equals(string.Empty);

                Run();

				System.Windows.Forms.Application.Exit();
			}
        }

        private void browseFiles_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".epi7";
            dlg.Filter = "Epi Info Sync File (.epi7)|*.epi7";

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                fileName.Text = dlg.FileName;
            }
        }

		private void folderPath_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			dialog.ShowDialog();

			folderPath.Text = dialog.SelectedPath;
		}

		private void run_Click(object sender, RoutedEventArgs e)
        {
            Run();
		}

        private void Run()
        {
			if (fileName.Text != null && fileName.Text != string.Empty)
			{
				string output = ParseXML(fileName.Text);

				if (!string.IsNullOrEmpty(output))
				{
					string outputFile = System.IO.Path.GetTempPath() + "\\" + Guid.NewGuid().ToString() + ".csv";
					System.IO.File.AppendAllText(outputFile, output, Encoding.UTF8);
					System.Diagnostics.Process.Start(outputFile);
				}
			}

			if (folderPath.Text != null && folderPath.Text != string.Empty)
			{
				DirectoryInfo syncFileFolderDirectoryInfo = new DirectoryInfo(folderPath.Text);

                string folderName = System.IO.Path.Combine(syncFileFolderDirectoryInfo.FullName, DateTime.Now.ToString("yyyyMMddHHmmssfff"));

				DirectoryInfo targetFolderDirectoryInfo = Directory.CreateDirectory(folderName);

				foreach (var syncFile in syncFileFolderDirectoryInfo.GetFiles("*.epi7"))
				{
				    string output = ParseXML(syncFile.FullName);

				    if (!string.IsNullOrEmpty(output))
				    {
					    string outputFileName = System.IO.Path.Combine
                        (
                            targetFolderDirectoryInfo.FullName,
                            System.IO.Path.GetFileNameWithoutExtension(syncFile.Name) + ".csv"
                        );
					    
                        File.AppendAllText(outputFileName, output, Encoding.UTF8);
				    }
				}
			}
		}

		private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public static string DecryptJava(string cipherText, string password)
        {
            int _keyLengthInBits = 128;

            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            AesManaged aes = new AesManaged();
            aes = new AesManaged();

            aes.KeySize = _keyLengthInBits;
            aes.BlockSize = 128;

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = HexStringToByteArray(initVectorDroid);
            aes.Key = new System.Security.Cryptography.Rfc2898DeriveBytes(password, HexStringToByteArray(saltDroid), 1000).GetBytes(_keyLengthInBits / 8);

            ICryptoTransform transform = aes.CreateDecryptor();
            byte[] plainTextBytes = transform.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);

            return System.Text.Encoding.UTF8.GetString(plainTextBytes);
        }

        private static byte[] HexStringToByteArray(string s)
        {
            var r = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                r[i / 2] = (byte)Convert.ToInt32(s.Substring(i, 2), 16);
            }
            return r;
        }

        private string ParseXML(string filePath)
        {
            string xmlText;

            string password = passwordBox1.Password;
            string encrypted = System.IO.File.ReadAllText(filePath);

            try
            {
                xmlText = DecryptJava(encrypted, password);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Invalid password or sync file.");
                return null;
            }
            //string xmlText = System.IO.File.ReadAllText(filePath);

            //return xmlText.Replace("&", "&amp;");

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmlText.Replace("&","&amp;"));

            StringBuilder sb = new StringBuilder();
            List<string> fieldNames = new List<string>();

            sb.Append("GlobalRecordId" + listSeparator);
            if (doc.ChildNodes[0].ChildNodes[0].Attributes.Count > 1)
            {
                sb.Append("FKEY" + listSeparator);
            }
            if (doc.ChildNodes[0].Name.ToLower().Equals("surveyresponses"))
            {
                foreach (XmlElement docElement in doc.ChildNodes[0].ChildNodes)
                {
                    if (docElement.Name.ToLower().Equals("surveyresponse") && docElement.Attributes.Count > 0 && docElement.Attributes[0].Name.ToLower().Equals("surveyresponseid"))
                    {
                        string surveyResponseId = docElement.Attributes[0].Value;

                        foreach (XmlElement surveyElement in docElement.ChildNodes)
                        {
                            if (surveyElement.Name.ToLower().Equals("page") && surveyElement.Attributes.Count > 0 && surveyElement.Attributes[0].Name.ToLower().Equals("pageid"))
                            {
                                foreach (XmlElement pageElement in surveyElement.ChildNodes)
                                {
                                    if (pageElement.Name.ToLower().Equals("responsedetail"))
                                    {
                                        string fieldName = string.Empty;
                                        if (pageElement.Attributes.Count > 0)
                                        {
                                            fieldName = pageElement.Attributes[0].Value;
                                            if (!fieldNames.Contains(fieldName))
                                            {
                                                fieldNames.Add(fieldName);
                                                sb.Append(fieldName + listSeparator);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append(Environment.NewLine);

                foreach (XmlElement docElement in doc.ChildNodes[0].ChildNodes)
                {
                    if (docElement.Name.ToLower().Equals("surveyresponse") && docElement.Attributes.Count > 0 && docElement.Attributes[0].Name.ToLower().Equals("surveyresponseid"))
                    {
                        string surveyResponseId = docElement.Attributes[0].Value;

                        sb.Append(surveyResponseId + listSeparator);
                        if (docElement.Attributes.Count > 1)
                        {
                            sb.Append(docElement.Attributes[1].Value + listSeparator);
                        }
                        foreach (XmlElement surveyElement in docElement.ChildNodes)
                        {
                            if (surveyElement.Name.ToLower().Equals("page") && surveyElement.Attributes.Count > 0 && surveyElement.Attributes[0].Name.ToLower().Equals("pageid"))
                            {
                                foreach (XmlElement pageElement in surveyElement.ChildNodes)
                                {
                                    if (pageElement.Name.ToLower().Equals("responsedetail"))
                                    {
                                        string fieldValue = pageElement.InnerText;
                                        if (fieldValue.Equals("Yes"))
                                        {
                                            sb.Append("TRUE" + listSeparator);
                                        }
                                        else if (fieldValue.Equals("No"))
                                        {
                                            sb.Append("FALSE" + listSeparator);
                                        }
                                        else if (fieldValue.ToLower().Contains("/epiinfo/"))
                                        {
                                            string[] splits = fieldValue.Split('/');
                                            string mediaFileName = splits[splits.Length - 1];
                                            sb.Append("\"=HYPERLINK(\"\"media\\" + mediaFileName + "\"\",\"\"<CLICK HERE>\"\")\"" + listSeparator);
                                        }
                                        else
                                        {
                                            sb.Append("\"" + fieldValue + "\"" + listSeparator);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(Environment.NewLine);
                }                
            }
            return sb.ToString();
            
        }

		private void fileName_TextChanged(object sender, TextChangedEventArgs e)
		{
			run.IsEnabled = !fileName.Text.Equals(string.Empty) && !passwordBox1.Password.Equals(string.Empty);
		}

		private void folderPath_TextChanged(object sender, TextChangedEventArgs e)
		{
			run.IsEnabled = !folderPath.Text.Equals(string.Empty) && !passwordBox1.Password.Equals(string.Empty);
		}

		private void passwordBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            run.IsEnabled = !fileName.Text.Equals(string.Empty) && !passwordBox1.Password.Equals(string.Empty);
        }


	}
}
