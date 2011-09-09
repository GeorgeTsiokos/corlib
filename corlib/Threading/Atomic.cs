using CorLib.Internal.Threading;
using System;

namespace CorLib.Threading {

    /// <summary>
    /// Creates new values that can be accessed by multiple threads concurrently
    /// </summary>
    public static class Atomic {

        /// <summary>
        /// Returns a new atomic UInt64
        /// </summary>
        /// <remarks>
        /// By default, this type's value will start at (0). In addition, this type 
        /// will not throw an overflow exception. The value will reset to 0 when 
        /// incremented past (18,446,744,073,709,551,615).
        /// <see cref="IAtomic{T}.Info"/>
        /// </remarks>
        /// <returns>a new instance</returns>
        [CLSCompliant (false)]
        public static IAtomic<ulong> CreateUInt64 () {
            return AtomicUInt64.Create ();
        }
    }
}