using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Epi.Core")]
[assembly: AssemblyDescription("Epi Info Framework")]
[assembly: AssemblyConfiguration("")]

// framework satellites
[assembly: InternalsVisibleTo("Epi.Windows.ImportExport")]
[assembly: InternalsVisibleTo("Epi.Statistics")]
[assembly: InternalsVisibleTo("Epi.ImportExport")]
[assembly: InternalsVisibleTo("Epi.Windows")]
[assembly: InternalsVisibleTo("Epi.WPF")]
[assembly: InternalsVisibleTo("Epi.Core.GIS")]
[assembly: InternalsVisibleTo("Epi.Core.Analysis")]
[assembly: InternalsVisibleTo("Epi.Core.Enter")]

//executables
[assembly: InternalsVisibleTo("EpiInfo")]
[assembly: InternalsVisibleTo("Analysis")]
[assembly: InternalsVisibleTo("AnalysisDashboard")]
[assembly: InternalsVisibleTo("Dashboard")]
[assembly: InternalsVisibleTo("Enter")]
[assembly: InternalsVisibleTo("EpiReport")]
[assembly: InternalsVisibleTo("EpiMap")]
[assembly: InternalsVisibleTo("MakeView")]
[assembly: InternalsVisibleTo("Menu")]
[assembly: InternalsVisibleTo("TSetup")]

