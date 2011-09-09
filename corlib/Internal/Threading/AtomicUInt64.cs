using System;
using System.Threading;
using CorLib.Threading;

namespace CorLib.Internal.Threading {

    internal struct AtomicUInt64 : IAtomic<ulong> {
        static readonly AtomicInfo<ulong> __info = new AtomicInfo<ulong> (0, 18446744073709551615, 9223372036854775808);
        long _value;

        public IAtomicInfo<ulong> Info {
            get { return __info; }
        }

        public static AtomicUInt64 Create () {
            return new AtomicUInt64 () { Value = 0 };
        }

        public ulong Value {
            get {
                return Calculate (Get ());
            }
            set {
                Set (Calculate (value));
            }
        }

        public ulong IncrementAndReturn () {
            return Calculate (Interlocked.Increment (ref _value));
        }

        public void Increment () {
            Interlocked.Increment (ref _value);
        }

        public ulong DecrementAndReturn () {
            return Calculate (Interlocked.Decrement (ref _value));
        }

        public void Decrement () {
            Interlocked.Decrement (ref _value);
        }

        /// <returns>previous value</returns>
        public ulong SetAndReturn (ulong value) {
            return Calculate (Set (Calculate (value)));
        }

        long Get () {
            return Interlocked.Read (ref _value);
        }

        long Set (long value) {
            return Interlocked.Exchange (ref _value, value);
        }

        static ulong Calculate (long value) {
            unchecked { value += long.MinValue; };
            return (ulong)value;
        }

        static long Calculate (ulong value) {
            unchecked { value += long.MaxValue; };
            return (long)value;
        }

        public ulong Exchange (ulong value) {
            long value_ = Calculate (value);
            long original = Interlocked.Exchange (ref _value, value_);
            return Calculate (original);
        }

        public ulong CompareExchange (ulong value, ulong comparand) {
            long comparand_ = Calculate (comparand);
            long value_ = Calculate (value);
            long original = Interlocked.CompareExchange (ref _value, value_, comparand_);

            if (original.Equals (comparand_))
                return comparand;
            else
                return Calculate (original);
        }
    }
}