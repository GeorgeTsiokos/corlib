using System;

namespace Corlib {
    /// <summary>
    /// Encapsulates a read-only value
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