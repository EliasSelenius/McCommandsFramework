using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace McDevtools.Nbt {
    public class Compound : IEnumerable<NamedTag> {

        private readonly List<NamedTag> namedTags = new List<NamedTag>();

        public int Count => namedTags.Count;

        public object this[string key] {
            get => namedTags.Where(x => x.Name.Equals(key)).FirstOrDefault()?.Payload;
            set => namedTags.Find(x => x.Name.Equals(key)).Payload = value;
        }

        public void Add(NamedTag tag) => namedTags.Add(tag);
        public void Add(string key, object value) => namedTags.Add(new NamedTag(key, value));
        public bool Remove(string key) => namedTags.Remove(namedTags.Find(x => x.Name.Equals(key)));
        public bool ContainsKey(string key) => namedTags.Any(x => x.Name.Equals(key));
        public void Clear() => namedTags.Clear();
        public bool TryGetValue(string key, out object value) {
            value = this[key];
            return ContainsKey(key);
        }

        public string ToJsonText() => "{\n" + this.Select(x => NamedTag.ToJsonText(x)).Aggregate((x, y) => x + ",\n" + y) + "\n}";

        public IEnumerator<NamedTag> GetEnumerator() => namedTags.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => namedTags.GetEnumerator();
    }
}
