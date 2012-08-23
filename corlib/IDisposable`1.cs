using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Corlib {
    /// <summary>
    /// Encapsulates a value that can be disposed of (signaled)
    /// </summary>
    /// <remarks>
    /// Enables consumers to signal when they are done with the value 
    /// through the use of the <see cref="IDisposable.Dispose"/> method
    /// </remarks>
    public interface IDisposable<T> : IDisposable {
        /// <summary>
        /// The encapsulated value
        /// </summary>
        T Value { get; }
    }
}