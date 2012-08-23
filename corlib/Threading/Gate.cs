using System;
using System.Threading;

namespace Corlib.Threading {

    /// <summary>
    /// A simple atomic gate
    /// </summary>
    public sealed class Gate {
        int _value;

        /// <summary>
        /// Initializes a new instance of the Gate class.
        /// </summary>
        /// <param name="opened">true to create as opened</param>
        public Gate (bool opened) {
            if (opened)
                _value = 1;
        }

        /// <summary>
        /// Initializes a new instance of the Gate class in the closed state.
        /// </summary>
        public Gate () {
            // does not chain, default to closed
        }

        /// <summary>
        /// Opens the gate. The gate must be in the closed state.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// thrown if the gate is already open
        /// </exception>
        public void Open () {
            if (!TryOpen ())
                throw new InvalidOperationException ();
        }

        /// <summary>
        /// Closes the gate. The gate must be in the open state.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// thrown if the gate is already closed
        /// </exception>
        public void Close () {
            if (!TryClose ())
                throw new InvalidOperationException ();
        }

        /// <summary>
        /// Returns true if the gate is open
        /// </summary>
        public bool IsOpened {
            get { return 1 == Interlocked.Add (ref _value, 0); }
        }

        /// <summary>
        /// Attempts to open the gate
        /// </summary>
        /// <returns>true if the operation was successful</returns>
        public bool TryOpen () {
            return 0 == Interlocked.CompareExchange (ref _value, 1, 0);
        }

        /// <summary>
        /// Attempts to close the gate
        /// </summary>
        /// <returns>true if the operation was successful</returns>
        public bool TryClose () {
            return 1 == Interlocked.CompareExchange (ref _value, 0, 1);
        }
    }
}