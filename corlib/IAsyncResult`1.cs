using System;

namespace CorLib {

    /// <summary>
    /// Represents the complete status of an asynchronous operation
    /// </summary>
    public interface IAsyncResult<T> : IAsyncResult, IObserver<T> {
        /// <summary>
        /// Returns true if the operation has completed and result has been set
        /// </summary>
        bool HasValue { get; }
        /// <summary>
        /// Returns the result of the async operation
        /// </summary>
        /// <exception cref="InvalidOperationException">occurs when the async operation does not have a result value</exception>
        /// <exception cref="AggregateException">thrown when the operation encounters exception(s)</exception>
        T Result { get; }
        /// <summary>
        /// Returns the result of an async operation, if the operation is complete and returned a value
        /// </summary>
        /// <param name="value">the result of the async operation</param>
        /// <returns>true if the operation has completed and there was a result</returns>
        bool TryGetValue (out T value);
        /// <summary>
        /// Throws an <see cref="AggregateException"/> if any exception(s) occured during the operation
        /// </summary>
        void ThrowIfExceptionEncountered ();
        /// <summary>
        /// Gets the exception(s) associated with the operation
        /// </summary>
        AggregateException AggregateException { get; }
    }
}