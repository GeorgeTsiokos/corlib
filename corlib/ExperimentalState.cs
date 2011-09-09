#if EXPERIMENTAL
using System;

namespace CorLib {

    [Experimental]
    public enum ExperimentalState {
        None,
        RequiresAdditionalTesting,
        RequiresFeedback,
        Prerelease
    }
}
#endif