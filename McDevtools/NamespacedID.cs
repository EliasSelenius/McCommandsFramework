using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace McDevtools {

    /// <summary>
    /// Namespaced identifiers are used as plain string to reference blocks, items, entity types, recipes, functions, advancements, tags, and various other objects in vanilla Minecraft. Interestingly, block states are not namespaced.
    /// </summary>
    public class NamespacedID {

        public string Text => Namespace + ":" + ID;

        public string Namespace;
        public string ID;

        public NamespacedID(string _namespace, string id) {
            Namespace = _namespace; ID = id;
            validate(Text);
        }

        public NamespacedID(string text) {
            var i = text.IndexOf(':');
            Namespace = text.Substring(0, i);
            ID = text.Substring(i + 1);
            validate(Text);
        }

        public static implicit operator NamespacedID(string str) => new NamespacedID(str);
        public static implicit operator string(NamespacedID nid) => nid.Text;

        private static readonly Regex regex;

        static NamespacedID() {
            regex = new Regex("[a-z\\-_0-9]+:[a-z\\.\\-_0-9/]", RegexOptions.Compiled);
        }

        private static void validate(string nid) {
            if (!regex.IsMatch(nid))
                throw new Exception(nid + " is not a valid namespaced identifier");
        }

        public override bool Equals(object obj) => obj is NamespacedID nid && nid.Text.Equals(this.Text);

        public override int GetHashCode() => Text.GetHashCode();

        public override string ToString() => Text;
    }
}
