using System;

namespace CorLib.Reactive.Concurrency {

    [Flags]
    public enum CancellationCheckMode {
        None = 0,
        OnNow = 1,
        OnSchedule = 2,
        OnExecute = 4
    }
}