using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: CLSCompliant (true)]
[assembly: AssemblyTitle ("CorLib")]
[assembly: AssemblyDescription ("")]
[assembly: ComVisible (false)]
[assembly: Guid ("88f7195a-e9d4-4d0f-9441-dfb4ec577e4c")]
[assembly: AssemblyVersion ("0.0.2.0")]
[assembly: AssemblyFileVersion ("0.0.2.0")]

#if RELEASE
[assembly: AssemblyKeyFile ("corlib.snk")]
//[assembly: AssemblyDelaySignAttribute (true)]
#endif