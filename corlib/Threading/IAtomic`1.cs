using System;

namespace CorLib.Threading {

    /// <summary>
    /// A value that can be accessed by multiple threads concurrently
    /// </summary>
    /// <typeparam name="T">the type of the value</typeparam>
    [Obsolete]
    public interface IAtomic<T> {
        /// <summary>
        /// Detailed information about the atomic type
        /// </summary>
        AtomicInfo<T> Info { get; }
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        T Value { get; set; }
        /// <summary>
        /// Increments the value
        /// </summary>
        /// <returns>the previous value</returns>
        T IncrementAndReturn ();
        /// <summary>
        /// Increments the value
        /// </summary>
        void Increment ();
        /// <summary>
        /// Decrements the value
        /// </summary>
        /// <returns>the decremented value</returns>
        T DecrementAndReturn ();
        /// <summary>
        /// Decrements the value
        /// </summary>
        void Decrement ();
        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">the new value</param>
        /// <returns>the previous value</returns>
        T SetAndReturn (T value);
        /// <summary>
        /// Sets a specified value and returns the original value, as an atomic operation
        /// </summary>
        /// <param name="value">new value</param>
        /// <returns>old value</returns>
        T Exchange (T value);
        /// <summary>
        /// Compares the type's value with <paramref name="comparand"/> for equality, 
        /// and if they're equal, replaces the value with <paramref name="value"/>
        /// </summary>
        /// <param name="value">the replacement value</param>
        /// <param name="comparand">the value to compare against</param>
        /// <returns></returns>
        T CompareExchange (T value, T comparand);
    }
}