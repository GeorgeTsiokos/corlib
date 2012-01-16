using System;
using System.Threading;
using System.Diagnostics.Contracts;

namespace CorLib {
    /// <summary>
    /// Nullable{TimeSpan} extension methods
    /// </summary>
    public static class NullableTimeSpanExtensions {
        /// <summary>
        /// Converts a nullable timespan to <see cref="System.Threading.Timeout.Infinite"/> when null or
        /// the total amount of milliseconds when it has a value. If it exceeds <see cref="Int32.MaxValue"/>
        /// it uses <see cref="Int32.MaxValue"/>
        /// </summary>
        /// <param name="timeout">the value to convert</param>
        /// <returns><see cref="System.Threading.Timeout.Infinite"/> or a positive int representing milliseconds</returns>
        public static int AsThreadingTimeout (this TimeSpan? timeout) {
            Contract.Ensures (Contract.Result<int> () >= -1);

            if (null == timeout)
                return Timeout.Infinite;
            else {
                double timeout_ = timeout.Value.TotalMilliseconds;
                if (timeout_ > int.MaxValue)
                    return int.MaxValue;
                else if (timeout_ < 0)
                    return 0;
                else
                    return (int)timeout_;
            }
        }
    }
}