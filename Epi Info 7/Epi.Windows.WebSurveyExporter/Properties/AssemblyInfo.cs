using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Epi.Windows.WebSurveyExporter")]
[assembly: AssemblyDescription("Epi Info Web Survey Export Utility")]
[assembly: AssemblyConfiguration("")]

// Hardbindings
[assembly: Dependency("Epi.Core", LoadHint.Always)]
[assembly: Dependency("Epi.ImportExport", LoadHint.Always)]
[assembly: Dependency("Epi.Windows", LoadHint.Always)]
[assembly: Dependency("Epi.Windows.ImportExport", LoadHint.Always)]
