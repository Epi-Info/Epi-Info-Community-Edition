using System;
using System.Data;
using System.Reflection;
using Epi;
using Epi.DataSets;
using System.IO;
using Epi.Data;

namespace Epi.Data
{
    /// <summary>
    /// Class for Creating DBDrivers
    /// </summary>
    public static class DbDriverFactoryCreator
    {
        /// <summary>
        /// Known database Names
        /// </summary>
        public class KnownDatabaseNames
        {
            /// <summary>
            /// PHIN database
            /// </summary>
            public const string Phin = "PHIN"; 

            /// <summary>
            /// Application Data
            /// </summary>
            public const string AppData = "APPDATA";

            /// <summary>
            /// Translation to other languages
            /// </summary>
            public const string Translation = "TRANSLATION";
        }

        /// <summary>
        /// Create different type of concret DBFactory instance 
        /// </summary>
        /// <param name="dataDriverType"></param>
        /// <returns>A reference to an instance of an object that implments the IDbDriverFactory interface</returns>
        public static IDbDriverFactory GetDbDriverFactory(string dataDriverType)
        {
            Type typeFactory = null;
            IDbDriverFactory dbFactory = null;

            //Since MySQL is not recognized in Type.GetType(string), check for it specifically
            //otherwise, let the Type.GetType(string) return the correct type
            if (dataDriverType.Equals(SharedStrings.MYSQL_DATABASE_INFO))
            {
                try
                {
                    typeFactory = Type.GetType(Configuration.MySQLDriver);
                    //Assembly asd = Assembly.Load("Epi.Data.MySQL");
                    //typeFactory = asd.GetType("Epi.Data.MySQL.MySQLDBFactory");
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not load assembly for MySQL.  " + ex.StackTrace);
                }
            }
            else if (dataDriverType.Equals(SharedStrings.MONGODB_DATABASE_INFO))
            {
                try
                {
                    typeFactory = Type.GetType(Configuration.MongoDBDriver);
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not load assembly for MongoDB. " + ex.StackTrace);
                }
            }
            else if (dataDriverType.Equals("Epi.Data.EpiWeb.EpiWebFactory, Epi.Data.EpiWeb"))
            {
                try
                {
                    typeFactory = Type.GetType(Configuration.EpiInfoWebDriver);
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not load assembly for EpiInfoWeb. " + ex.StackTrace);
                }
            }
            else if (dataDriverType.Equals("Epi.Data.REDCap.REDCapFactory, Epi.Data.REDCap"))
            {
                try
                {
                    typeFactory = Type.GetType(Configuration.EpiInfoREDCapDriver);
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not load assembly for REDCap. " + ex.StackTrace);
                }
            }
            else if (dataDriverType.Equals("Epi.Data.RimportSPSS.RimportSPSSFactory, Epi.Data.RimportSPSS"))
            {
                try
                {
                    typeFactory = Type.GetType(Configuration.RimportSPSSDriver);
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not load assembly for RimportSPSS. " + ex.StackTrace);
                }
            }
            else if (dataDriverType.Equals("Epi.Data.RimportSAS.RimportSASFactory, Epi.Data.RimportSAS"))
            {
                try
                {
                    /// typeFactory = Type.GetType(Configuration.RimportSASDriver);
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not load assembly for RimportSAS. " + ex.StackTrace);
                }
            }
            else
			{
                try
                {
                    typeFactory = Type.GetType(dataDriverType);

                    if (typeFactory == null)
                    {
                        Configuration config = Configuration.GetNewInstance();
                        foreach (Epi.DataSets.Config.DataDriverRow row in config.DataDrivers)
                        {
                            string type = row.Type;                                                        

                            if (type == dataDriverType && row.FileName != null && !string.IsNullOrEmpty(row.FileName))
                            {
                                string dllFileName = row.FileName;
                                Assembly assemblyInstance = Assembly.LoadFrom(dllFileName);
                                Type[] types = assemblyInstance.GetTypes();

                                int indexOf = type.IndexOf(',');
                                type = type.Substring(0, indexOf).Trim();

                                foreach (Type t in types)
                                {
                                    if (t.FullName == type)
                                    {
                                        typeFactory = t;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (ApplicationException ex)
                {
                    throw new ApplicationException("Can not set typeFactory" + ex.StackTrace);
                }
            }

            try
            {
                dbFactory = Activator.CreateInstance(typeFactory, (object[])null) as IDbDriverFactory;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Can not create instance of dbFactory" + ex.StackTrace);
            }

            return dbFactory;
        }
    }
}
