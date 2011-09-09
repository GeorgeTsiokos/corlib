using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CorLib.Diagnostics {

    public sealed class TraceSourceFactory : IDisposable {

        static readonly Lazy<TraceSourceFactory> __default = new Lazy<TraceSourceFactory> ();
        readonly HashSet<string> _names = new HashSet<string> ();
        readonly Subject<string> _namesSubject = new Subject<string> ();

        public static TraceSourceFactory Default {
            get { return __default.Value; }
        }

        public IObservable<string> Names {
            get { return _namesSubject.AsObservable (); }
        }

        public TraceSource Create (string name) {
            if (_names.Add (name))
                _namesSubject.OnNext (name);

            return new TraceSource (name);
        }

        public TraceSource Create (Type type) {
            if (type == null)
                throw new ArgumentNullException ("type", "type is null.");
            return Create (type.FullName);
        }

        public TraceSource Create<T> () {
            return Create (typeof (T));
        }

        public TraceSource Create () {
            return Create ("corlib");
        }
        
        public void Dispose () {
            _names.Clear ();
            _namesSubject.Dispose ();
        }
    }
}