using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.Linq;

namespace McDevtools.Nbt {
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


        public static string ToJsonText(NamedTag nbt) {

            var res = "\"" + nbt.Name + "\": ";
            var pl = nbt.payload;

            res += nbt.Type switch
            {
                TagType.End => "",
                TagType.Byte => ((sbyte)pl).ToString(),
                TagType.Short => ((short)pl).ToString(),
                TagType.Int => ((int)pl).ToString(),
                TagType.Long => ((long)pl).ToString() + "L",
                TagType.Float => ((float)pl).ToString() + "f",
                TagType.Double => ((double)pl).ToString() + "d",
                TagType.ByteArray => "",
                TagType.String => "\"" + pl.ToString() + "\"",
                TagType.List => "[\n" + ((pl as List<object>).Count == 0 ? " " : (pl as List<object>).Select(x => (x is Compound c ? c.ToJsonText() : x.ToString())).Aggregate((x, y) => x + ",\n" + y)) + "\n]",
                TagType.Compound => (pl as Compound).ToJsonText(),
                _ => throw new System.Exception()
            };

            return res;
        }

    }
}
