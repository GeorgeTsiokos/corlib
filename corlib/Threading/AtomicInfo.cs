using Corlib.Threading;

namespace Corlib.Threading {

    public sealed class AtomicInfo<T> {

        public AtomicInfo (T minValue, T maxValue, T defaultValue) {
            MinValue = minValue;
            MaxValue = maxValue;
            DefaultValue = defaultValue;
        }

        /// <summary>The minimmum value supported by this type</summary>
        public T MinValue {
            get;
            private set;
        }

        /// <summary>The maximum value supported by this type</summary>
        public T MaxValue {
            get;
            private set;
        }

        /// <summary>The default value for this type</summary>
        public T DefaultValue {
            get;
            private set;
        }
    }
}