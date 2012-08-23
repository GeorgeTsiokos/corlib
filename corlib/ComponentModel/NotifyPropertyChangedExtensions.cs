using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Diagnostics.Contracts;

namespace Corlib.ComponentModel {

    public static class NotifyPropertyChangedExtensions {

        /// <summary>
        /// Converts a <see cref="INotifyPropertyChanged"/>'s PropertyChanged event to an observable sequence of string
        /// </summary>
        /// <param name="notifyPropertyChanged">source</param>
        /// <returns>a sequence of strings</returns>
        public static IObservable<string> GetPropertyChangedObservable (this INotifyPropertyChanged notifyPropertyChanged) {
            Contract.Requires (null != notifyPropertyChanged);

            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs> (eh =>
                notifyPropertyChanged.PropertyChanged += eh, eh =>
                    notifyPropertyChanged.PropertyChanged -= eh).Select (ea =>
                        ea.EventArgs.PropertyName);
        }
    }
}