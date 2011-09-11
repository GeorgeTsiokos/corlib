using System.Reactive.Subjects;

namespace CorLib.PubSub {

    public sealed class Topic {

        readonly string _name;
        readonly ISubject<TopicHttpResponse> _stream;


        public Topic (string name, ISubject<TopicHttpResponse> stream) {
            _name = name;
            _stream = stream;
        }

        public string Name {
            get { return _name; }
        }

        public ISubject<TopicHttpResponse> Stream {
            get {
                return _stream;
            }
        }
    }
}