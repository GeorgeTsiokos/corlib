using CorLib.Threading;

namespace CorLib.Internal.Threading {

    internal sealed class AtomicInfo<T> : IAtomicInfo<T> {

        public AtomicInfo (T minValue, T maxValue, T defaultValue) {
            MinValue = minValue;
            MaxValue = maxValue;
            DefaultValue = defaultValue;
        }

        public T MinValue {
            get;
            private set;
        }

        public T MaxValue {
            get;
            private set;
        }

        public T DefaultValue {
            get;
            private set;
        }
    }
}