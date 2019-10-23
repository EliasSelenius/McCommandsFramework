using System.Text;
using System.Threading.Tasks;

namespace McDevtools.NBT {
    public class NamedTag {
        public readonly string Name;
        public TagType Type { get; private set; }
        private object payload;
        public object Payload {
            set {
                payload = value;
                Type = payload.GetNbtType();
            }
            get => payload;
        }

        internal NamedTag(string n, object p) {
            Name = n; Payload = p;
        }
    }
}
