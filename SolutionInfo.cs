using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Epi;

// Generic assembly settings
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("Centers for Disease Control and Prevention")]
[assembly: AssemblyProduct("Epi Info™")]
[assembly: AssemblyCopyright("© Centers for Disease Control and Prevention")]
[assembly: AssemblyTrademark("Epi Info is a trademark of CDC")]
[assembly: AssemblyCulture("")]

// For .NET 4.0 upgrade
[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]

//mark the types you want to export to COM with ComVisible(true)
[assembly: ComVisible(false)]

// Allow unsigned assemblies access to Epi Info assemblies
[assembly: AllowPartiallyTrustedCallers]

//define english/us as the neutral language
[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("7.2.*")]
[assembly: AssemblyFileVersion("7.2.2.12")]
[assembly: AssemblyInformationalVersion("7.2.2.12")]
[assembly: SatelliteContractVersion("7.0.0.0")]
[assembly: Epi.AssemblyReleaseDateAttribute("7/13/2018")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//       When specifying the KeyFile, the location of the KeyFile should be
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]
//[assembly: AssemblyKeyName("")]
