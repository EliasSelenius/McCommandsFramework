using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace McDevtools.NBT {
    public class Compound : IDictionary<string, object> {

        private readonly List<NamedTag> namedTags = new List<NamedTag>();

        public IEnumerable<KeyValuePair<string, object>> KeyValuePairs => namedTags.Select(x => new KeyValuePair<string, object>(x.Name, x.Payload));

        public int Count => namedTags.Count;

        public ICollection<string> Keys => (ICollection<string>)namedTags.Select(x => x.Name);

        public ICollection<object> Values => (ICollection<object>)namedTags.Select(x => x.Payload);

        public bool IsReadOnly => false;

        public object this[string key] {
            get => namedTags.Where(x => x.Name.Equals(key)).FirstOrDefault()?.Payload;
            set => namedTags.Find(x => x.Name.Equals(key)).Payload = value;
        }

        public void Add(NamedTag tag) => namedTags.Add(tag);
        public void Add(string key, object value) => namedTags.Add(new NamedTag(key, value));
        public void Add(KeyValuePair<string, object> item) => Add(item.Key, item.Value);

        public bool Remove(string key) => namedTags.Remove(namedTags.Find(x => x.Name.Equals(key)));
        public bool Remove(KeyValuePair<string, object> item) => namedTags.Remove(namedTags.Find(x => x.Name.Equals(item.Key) && x.Payload == item.Value));

        public bool ContainsKey(string key) => namedTags.Any(x => x.Name.Equals(key));
        public bool Contains(KeyValuePair<string, object> item) => namedTags.Any(x => x.Name.Equals(item.Key) && x.Payload == item.Value);

        public bool TryGetValue(string key, out object value) {
            value = this[key];
            return ContainsKey(key);
        }

        public void Clear() => namedTags.Clear();

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => KeyValuePairs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => KeyValuePairs.GetEnumerator();
    }
}
