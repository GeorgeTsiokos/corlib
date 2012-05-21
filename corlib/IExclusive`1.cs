using System;

namespace Corlib {
    /// <summary>
    /// Encapsulates a value for exclusive access
    /// </summary>
    /// <remarks>
    /// The <see cref="IDisposable.Dispose"/> cancels any 
    /// outstanding scheduled actions and frees resources
    /// healed by the object. To create a value for exclusive access,
    /// 
    /// </remarks>
    public interface IExclusive<T> : IDisposable {
        /// <summary>
        /// Schedules an action to take place with exclusive access to the value
        /// </summary>
        IDisposable Schedule (Action<T> value);
    }
}