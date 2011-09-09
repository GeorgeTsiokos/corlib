using System.Reflection;

[assembly: AssemblyProduct ("http://corlib.codeplex.com/")]
[assembly: AssemblyCopyright ("Copyright © George Tsiokos 2011")]
[assembly: AssemblyTrademark ("License: Microsoft Reciprocal License (Ms-RL)")]
[assembly: AssemblyCompany ("This open-source library is not affiliated with Microsoft Corporation")]
[assembly: AssemblyCulture ("")]

#if EXPERIMENTAL
[assembly: AssemblyConfiguration ("EXPERIMENTAL")]
#elif DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#elif RELEASE
[assembly: AssemblyConfiguration("RELEASE")]
#else
[assembly: AssemblyConfiguration("UNKNOWN")]
#endif