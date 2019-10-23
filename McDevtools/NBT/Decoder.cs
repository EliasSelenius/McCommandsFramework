using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;

namespace McDevtools.NBT {
    public static class Decoder {


        private readonly static Dictionary<Type, TagType> TypeMap = new Dictionary<Type, TagType>() {
            { typeof(sbyte), TagType.Byte },
            { typeof(short), TagType.Short },
            { typeof(int), TagType.Int },
            { typeof(long), TagType.Long },
            { typeof(float), TagType.Float },
            { typeof(double), TagType.Double },
            { typeof(sbyte[]), TagType.ByteArray },
            { typeof(string), TagType.String },
            { typeof(List<object>), TagType.List },
            { typeof(Compound), TagType.Compound }
        };

        public static TagType GetNbtType(this object obj) {
            var type = obj.GetType();
            if (TypeMap.ContainsKey(type)) {
                return TypeMap[type];
            }
            throw new Exception(type + " is not supported");
        }


        public static NamedTag Decode(string filename) {
            NamedTag res = null;
            using (var s = new GZipStream(new FileStream(filename, FileMode.Open), CompressionMode.Decompress)) {
                var br = new BinaryReader(s);
                res = ReadNamedTag(br);
                br.Close();
            }
            return res;
        }

        public static NamedTag ReadNamedTag(BinaryReader br) {
            var type = (TagType)br.ReadSByte();
            var length = br.ReadBigEInt16();
            var name = br.ReadChars(length).AsString();
            return new NamedTag(name, ReadTag(type, br));
        }

        private static object ReadTag(TagType type, BinaryReader br) {
            switch (type) {
                case TagType.End: return null;
                case TagType.Byte: return br.ReadSByte();
                case TagType.Short: return br.ReadBigEInt16(); 
                case TagType.Int: return br.ReadBigEInt32(); 
                case TagType.Long: return br.ReadBigEInt64(); 
                case TagType.Float: return br.ReadBigESingle(); 
                case TagType.Double: return br.ReadBigEDouble(); 
                case TagType.ByteArray: throw new NotImplementedException();
                case TagType.String:
                    var strl = br.ReadBigEInt16();
                    return br.ReadChars(strl).AsString();
                case TagType.List:
                    var tagid = (TagType)br.ReadSByte();
                    var listlength = br.ReadBigEInt32();
                    var list = new List<object>();
                    for (int i = 0; i < listlength; i++) {
                        list.Add(ReadTag(tagid, br));
                    }
                    return list;
                case TagType.Compound:
                    var l = new Compound();
                    while (true) {
                        var tagtype = (TagType)br.ReadSByte();
                        if (tagtype == TagType.End) {
                            break;
                        }
                        var tagname = ReadTag(TagType.String, br) as string;
                        l.Add(new NamedTag(tagname, ReadTag(tagtype, br)));
                    }
                    return l;
                default:
                    throw new Exception(type + " was'nt processed");
            }
        }

        private static short ReadBigEInt16(this BinaryReader br) {
            var data = br.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }
        private static int ReadBigEInt32(this BinaryReader br) {
            var data = br.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
        private static long ReadBigEInt64(this BinaryReader br) {
            var data = br.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }
        private static float ReadBigESingle(this BinaryReader br) {
            var data = br.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }
        private static double ReadBigEDouble(this BinaryReader br) {
            var data = br.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }

        public static string AsString(this char[] cs) {
            var res = "";
            for (int i = 0; i < cs.Length; i++) {
                res += cs[i];
            }
            return res;
        }

    }
}
