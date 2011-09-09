#if EXPERIMENTAL
using System;

namespace CorLib {

    [AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = true), Experimental]    
    public sealed class ExperimentalAttribute : Attribute {

        public ExperimentalState State { get; set; }
        public string Details { get; set; }
    }
}
#endif