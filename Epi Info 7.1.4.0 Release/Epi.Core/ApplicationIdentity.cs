using System;
using Epi;
using System.Reflection;

namespace Epi
{
    /// <summary>
    /// Application Identity class
    /// </summary>
    public class ApplicationIdentity
    {
        readonly Version assemblyVersion;
        readonly Version assemblyInformationalVersion;
        readonly Version assemblyFileVersion;
        readonly string company;
        readonly string product;
        readonly string copyright;
        readonly string releaseDate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a">CLR Application</param>
        public ApplicationIdentity(Assembly a)
        {
            assemblyVersion = a.GetName().Version;

            object[] customAttributes;

            customAttributes = a.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (customAttributes.Length == 1)
            {
                assemblyInformationalVersion = new Version(((AssemblyInformationalVersionAttribute)customAttributes[0]).InformationalVersion);
            }

            customAttributes = a.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            if (customAttributes.Length == 1)
            {
                assemblyFileVersion = new Version(((AssemblyFileVersionAttribute)customAttributes[0]).Version);
            }

            customAttributes = a.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (customAttributes.Length == 1)
            {
                product = ((AssemblyProductAttribute)customAttributes[0]).Product;
            }

            customAttributes = a.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (customAttributes.Length == 1)
            {
                company = ((AssemblyCompanyAttribute)customAttributes[0]).Company;
            }

            customAttributes = a.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (customAttributes.Length == 1)
            {
                copyright = ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
            }

            customAttributes = a.GetCustomAttributes(typeof(AssemblyReleaseDateAttribute), false);
            if (customAttributes.Length == 1)
            {
                releaseDate = ((AssemblyReleaseDateAttribute)customAttributes[0]).ReleaseDate;
            }

        }
        #region Public Properties
        /// <summary>
        /// Gets the version number of the application 
        /// </summary>
        public string Version
        {
            get
            {
                return assemblyInformationalVersion.ToString();
            }
        }

        /// <summary>
        /// Gets the version as an object
        /// </summary>
        public Version VersionObject
        {
            get
            {
                return assemblyInformationalVersion;
            }
        }

        /// <summary>
        /// Gets the build number of the application 
        /// </summary>
        public string Build
        {
            get
            {
                return assemblyVersion.Build.ToString();
            }
        }

        /// <summary>
        /// Gets the revision number of the application 
        /// </summary>
        public string Revision
        {
            get
            {
                return assemblyVersion.Revision.ToString();
            }
        }

        //        /// <summary>
        //        /// Gets the version release date if it is a release version. In development, returns today's date.
        //        /// </summary>
        //        public string VersionReleaseDate
        //        {
        //            get
        //            {
        //                //if (Global.IsInDevelopmentMode)
        //                //{
        //                //    return SharedStrings.DEV;
        //                //}
        //                //else
        //                //{
        //                    string releaseDate = System.Configuration.ConfigurationSettings.ApplicationIdentity[AppSettingKeys.VERSION_RELEASE_DATE];
        //                    if (string.IsNullOrEmpty(releaseDate))
        //                    {
        //                        return SharedStrings.DEV;
        //                    }
        //                    else
        //                    {
        //// $$$ TODO this is not locale neutral and will throw an error for other than US locale
        //                        return DateTime.Parse(releaseDate).ToShortDateString();
        //                    }
        //                //}
        //            }
        //        }

        /// <summary>
        /// App release date
        /// </summary>
        public string VersionReleaseDate
        {
            get { return releaseDate; }
        }

        /// <summary>
        /// Gets the application name
        /// </summary>
        public string AppName
        {
            get
            {
                return product;
            }
        }
        /// <summary>
        /// Gets the application suite name
        /// </summary>
        public string SuiteName
        {
            get
            {
                return product;
            }
        }

        /// <summary>
        /// Gets the company name
        /// </summary>
        public string Company
        {
            get
            {
                return company;
            }
        }

        /// <summary>
        /// Gets the company name
        /// </summary>
        public string Copyright
        {
            get
            {
                return copyright;
            }
        }

        #endregion Public Properties
    }
    /// <summary>
    /// Assembly Release Date Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AssemblyReleaseDateAttribute : Attribute
    {
        private string releaseDate;
        /// <summary>
        /// Build date of assembly.
        /// </summary>
        public string ReleaseDate
        {
            get { return releaseDate; }
            set { releaseDate = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="releaseDate">Assembly release date.</param>
        public AssemblyReleaseDateAttribute(string releaseDate)
        {
            this.releaseDate = releaseDate;
        }
    }
}
