using System;
using System.Threading;

namespace Corlib.Threading {

    /// <summary>
    /// A UInt64 that can be accessed from multiple threads concurrently <see cref="AtomicUInt64.Create()"/>
    /// </summary>
    /// <remarks>
    /// By default, this type's value will start at (9,223,372,036,854,775,808). In addition, this type 
    /// will not throw an overflow exception. The value will reset to 0 when incremented 
    /// past (18,446,744,073,709,551,615). <see cref="AtomicUInt64.Info"/>
    /// </remarks>
    [CLSCompliant (false)]
    public struct AtomicUInt64 {
        static readonly AtomicInfo<ulong> __info = new AtomicInfo<ulong> (0, 18446744073709551615, 9223372036854775808);
        long _value;

        /// <summary>
        /// Detailed information about the atomic type
        /// </summary>
        public static AtomicInfo<ulong> Info {
            get { return __info; }
        }

        /// <summary>
        /// Creates a new value starting at 0
        /// </summary>
        /// <returns>a new value with the value 0</returns>
        public static AtomicUInt64 Create () {
            return new AtomicUInt64 () { Value = 0 };
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public ulong Value {
            get {
                return Calculate (Get ());
            }
            set {
                Set (Calculate (value));
            }
        }

        /// <summary>
        /// Increments the value
        /// </summary>
        /// <returns>the previous value</returns>
        public ulong IncrementAndReturn () {
            return Calculate (Interlocked.Increment (ref _value));
        }

        /// <summary>
        /// Increments the value
        /// </summary>
        public void Increment () {
            Interlocked.Increment (ref _value);
        }

        /// <summary>
        /// Decrements the value
        /// </summary>
        /// <returns>the previous value</returns>
        public ulong DecrementAndReturn () {
            return Calculate (Interlocked.Decrement (ref _value));
        }

        /// <summary>
        /// Decrements the value
        /// </summary>
        public void Decrement () {
            Interlocked.Decrement (ref _value);
        }

        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="value">the new value</param>
        /// <returns>the previous value</returns>
        public ulong SetAndReturn (ulong value) {
            return Calculate (Set (Calculate (value)));
        }

        /// <summary>
        /// Sets a specified value and returns the original value, as an atomic operation
        /// </summary>
        /// <param name="value">new value</param>
        /// <returns>old value</returns>
        public ulong Exchange (ulong value) {
            long value_ = Calculate (value);
            long original = Interlocked.Exchange (ref _value, value_);
            return Calculate (original);
        }

        /// <summary>
        /// Compares the type's value with <paramref name="comparand"/> for equality, 
        /// and if they're equal, replaces the value with <paramref name="value"/>
        /// </summary>
        /// <param name="value">the replacement value</param>
        /// <param name="comparand">the value to compare against</param>
        /// <returns>the original value</returns>
        public ulong CompareExchange (ulong value, ulong comparand) {
            long comparand_ = Calculate (comparand);
            long value_ = Calculate (value);
            long original = Interlocked.CompareExchange (ref _value, value_, comparand_);

            if (original.Equals (comparand_))
                return comparand;
            else
                return Calculate (original);
        }

        public static implicit operator ulong (AtomicUInt64 value) {
            return value.Value;
        }

        public static implicit operator AtomicUInt64 (ulong value) {
            return new AtomicUInt64 () { Value = value };
        }

        long Get () {
            return Interlocked.Read (ref _value);
        }

        long Set (long value) {
            return Interlocked.Exchange (ref _value, value);
        }

        static ulong Calculate (long value) {
            unchecked {
                return (ulong)(value + long.MinValue);
            };
        }

        static long Calculate (ulong value) {
            unchecked {
                return (long)(value + __info.DefaultValue);
            };
        }
    }
}