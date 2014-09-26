using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Epi.Data;

namespace Epi.ImportExport
{
    public static class ImportExportHelper
    {
        /// <summary>
        /// Compresses an Epi Info 7 data package.
        /// </summary>
        /// <param name="fi">The file info for the raw data file that will be compressed.</param>
        public static void CompressDataPackage(FileInfo fi)
        {
            using (FileStream inFile = fi.OpenRead())
            {
                // Prevent compressing hidden and already compressed files.
                if ((File.GetAttributes(fi.FullName) & FileAttributes.Hidden)
                        != FileAttributes.Hidden & fi.Extension != ".gz")
                {
                    using (FileStream outFile = File.Create(fi.FullName + ".gz"))
                    {
                        using (GZipStream Compress = new GZipStream(outFile,
                                CompressionMode.Compress))
                        {
                            // Copy the source file into the compression stream.
                            byte[] buffer = new byte[4096];
                            int numRead;
                            while ((numRead = inFile.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                Compress.Write(buffer, 0, numRead);
                            }
                            Compress.Close();
                            Compress.Dispose();
                        }
                    }
                }
            }
        }

        public static string Zip(string text)
        {
            byte[] buffer = System.Text.Encoding.Unicode.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        public static string UnZip(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);
            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (System.IO.Compression.GZipStream zip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return System.Text.Encoding.Unicode.GetString(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Decompresses an Epi Info 7 data package.
        /// </summary>
        /// <param name="fi">The file info for the compressed file to be decompressed.</param>
        public static void DecompressDataPackage(FileInfo fi)
        {
            using (FileStream inFile = fi.OpenRead())
            {
                // Get original file extension, for example "doc" from report.doc.gz.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length - fi.Extension.Length) + ".mdb";

                using (FileStream outFile = File.Create(origName))
                {
                    using (GZipStream Decompress = new GZipStream(inFile,
                            CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            outFile.Write(buffer, 0, numRead);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursive function used to find if a view is a descendant of a given parent
        /// </summary>
        /// <param name="view">The view to check</param>
        /// <param name="parentView">The parent view</param>
        /// <returns>bool</returns>
        public static bool IsFormDescendant(View view, View parentView)
        {
            if (view.ParentView == null)
            {
                if (view.Name == parentView.Name)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (view.ParentView.Name == parentView.Name)
            {
                return true;
            }
            else
            {
                return ImportExportHelper.IsFormDescendant(view.ParentView, parentView);
            }
        }

        /// <summary>
        /// Recursive function used to find at what level a form is in the form heirarchy. Parent
        /// forms are 0, child forms are 1, grandchildren are 2, and so on.
        /// </summary>
        /// <param name="view">The view to check</param>
        /// <param name="parentView">The parent view</param>
        /// <param name="level">The current level</param>
        /// <returns>int; represents the 'level' of the form in the hierarchy</returns>
        public static int GetFormDescendantLevel(View view, View parentView, int level)
        {
            if (view.ParentView == null)
            {
                if (view.Name == parentView.Name)
                {
                    return 0; // 0 indicates a parent (top-level) form
                }
                else
                {
                    return -1; // -1 indicates the form isn't in the hierarchy
                }
            }
            else if (view.ParentView.Name == parentView.Name)
            {
                return ++level;
            }
            else
            {
                return ImportExportHelper.GetFormDescendantLevel(view.ParentView, parentView, level + 1);
            }
        }

        /// <summary>
        /// Orders the columns in a DataTable by the given ordering method.
        /// </summary>
        /// <param name="table">The table whose columns should be sorted.</param>
        /// <param name="columnSortOrder">The order by which the columns should be sorted.</param>
        /// <param name="view">The Epi Info 7 form</param>
        public static void OrderColumns(DataTable table, ColumnSortOrder columnSortOrder, View view = null)
        {
            #region Input Validation
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            #endregion // Input Validation

            if (columnSortOrder == ColumnSortOrder.None)
            {
                return;
            }

            bool isUsingEpiProject = (view == null);
            bool sortByTabOrder = (columnSortOrder == ColumnSortOrder.TabOrder);

            if (!sortByTabOrder || !isUsingEpiProject)
            {
                // alphabetical sort
                List<string> columnNames = new List<string>();

                foreach (DataColumn dc in table.Columns)
                {
                    columnNames.Add(dc.ColumnName);
                }

                columnNames.Sort();

                if (columnNames.Contains("UniqueKey"))
                {
                    columnNames.Remove("UniqueKey");
                    columnNames.Add("UniqueKey");
                }
                else if (columnNames.Contains("UNIQUEKEY"))
                {
                    columnNames.Remove("UNIQUEKEY");
                    columnNames.Add("UNIQUEKEY");
                }

                for (int i = 0; i < columnNames.Count; i++)
                {
                    string columnName = columnNames[i];
                    table.Columns[columnName].SetOrdinal(i);
                }
            }
            else if (sortByTabOrder && isUsingEpiProject)
            {
                // tab order sort
                Dictionary<string, int> columnNames = new Dictionary<string, int>();
                int runningTabIndex = 0;

                foreach (Page page in view.Pages)
                {
                    SortedDictionary<double, string> fieldsOnPage = new SortedDictionary<double, string>();

                    foreach (Epi.Fields.Field field in page.Fields)
                    {
                        if (field is Epi.Fields.RenderableField && field is Epi.Fields.IDataField)
                        {
                            Epi.Fields.RenderableField renderableField = field as Epi.Fields.RenderableField;
                            if (renderableField != null)
                            {
                                double tabIndex = renderableField.TabIndex;
                                while (fieldsOnPage.ContainsKey(tabIndex))
                                {
                                    tabIndex = tabIndex + 0.1;
                                }

                                fieldsOnPage.Add(tabIndex, field.Name);
                            }
                        }
                    }

                    foreach (KeyValuePair<double, string> kvp in fieldsOnPage)
                    {
                        columnNames.Add(kvp.Value, runningTabIndex);
                        runningTabIndex++;
                    }
                }

                if (columnNames.ContainsKey("UniqueKey"))
                {
                    columnNames["UniqueKey"] = runningTabIndex + 1;
                }
                else if (columnNames.ContainsKey("UNIQUEKEY"))
                {
                    columnNames["UNIQUEKEY"] = runningTabIndex + 1;
                }
                else
                {
                    columnNames.Add("UNIQUEKEY", runningTabIndex + 1);
                }

                int newRunningTabIndex = 0; // to prevent arg exceptions; TODO: Fix this better
                foreach (KeyValuePair<string, int> kvp in columnNames)
                {
                    if (table.Columns.Contains(kvp.Key))
                    {
                        table.Columns[kvp.Key].SetOrdinal(newRunningTabIndex);
                        newRunningTabIndex++;
                    }
                }
            }
        }
    }
}
