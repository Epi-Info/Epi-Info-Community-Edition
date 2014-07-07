using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Utilities")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("01288c90-bb66-4a81-9059-20dd8ef0b6d0")]


// Hardbindings
[assembly: Dependency("Epi.Core", LoadHint.Always)]
[assembly: Dependency("Epi.Windows", LoadHint.Always)]
