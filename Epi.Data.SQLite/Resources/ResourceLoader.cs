using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Resources;
using System.Reflection;

namespace Epi.Data.SQLite
{
    /// <summary>
    /// Resource Loader
    /// </summary>
    public static class ResourceLoader
    {
        private static System.Resources.ResourceManager resourceMan;
        private static System.Globalization.CultureInfo resourceCulture;

        const string RESOURCE_NAMESPACE_BASE = "Epi.Data.Office.Resources";
        const string RESOURCE_ACCESS_2003 = RESOURCE_NAMESPACE_BASE + ".Access2003.mdb";
        const string RESOURCE_ACCESS_2007 = RESOURCE_NAMESPACE_BASE + ".Access2007.accdb";

        static Assembly resourceAssembly = typeof(ResourceLoader).Assembly;



        /// <summary>
        /// Get Access 2003 Templete
        /// </summary>
        /// <param name="filePath"></param>
        public static void ExtractAccess2003Template(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCE_ACCESS_2003))
                {
                    CopyStream(resourceStream, fs);
                    fs.Close();
                }
            }
        }
        /// <summary>
        /// Get Access 2007 Templete
        /// </summary>
        /// <param name="filePath"></param>
        public static void ExtractAccess2007Template(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(RESOURCE_ACCESS_2007))
                {
                    CopyStream(resourceStream, fs);
                    fs.Close();
                }
            }

        }
    

        #region private
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager(RESOURCE_NAMESPACE_BASE, typeof(ResourceLoader).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        private static System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }
        #endregion

       
      


        static void CopyStream(Stream inStream, Stream outStream)
        {
            using (BinaryWriter writer = new BinaryWriter(outStream))
            {
                using (BinaryReader reader = new BinaryReader(inStream))
                {
                    while (true)
                    {
                        byte[] buffer = reader.ReadBytes(short.MaxValue);
                        writer.Write(buffer);

                        // if done
                        if (buffer.Length < short.MaxValue) break;
                    }
                }
            }
        }
    }
}
