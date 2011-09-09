using System;
using CorLib.Diagnostics;

namespace CorLib.Threading {

    /// <summary>
    /// Atomic type info
    /// </summary>
    public interface IAtomicInfo<T> {
        /// <summary>The minimmum value supported by this type</summary>
        T MinValue { get; }
        /// <summary>The maximum value supported by this type</summary>
        T MaxValue { get; }
        /// <summary>The default value for this type</summary>
        T DefaultValue { get; }
    }
}