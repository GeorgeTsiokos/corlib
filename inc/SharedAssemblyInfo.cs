using System.Reflection;

[assembly: AssemblyProduct ("https://github.com/HiTEC/corlib")]
[assembly: AssemblyCopyright ("Copyright © George Tsiokos 2011")]
[assembly: AssemblyTrademark ("License: Microsoft Reciprocal License (Ms-RL)")]
[assembly: AssemblyCompany ("This open-source library is not affiliated with Microsoft Corporation")]
[assembly: AssemblyCulture ("")]
[assembly: AssemblyVersion ("0.0.2.0")]
[assembly: AssemblyFileVersion ("0.0.2.6")]
//TODO: nuget should use file version

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif

#if !DEBUG
[assembly: AssemblyKeyFile (@"..\corlib.snk")]
//[assembly: AssemblyDelaySignAttribute (true)]
#endif