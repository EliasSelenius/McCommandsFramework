using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JsonParser;

namespace McCommandsFramework {
    public class McTags {

        private readonly Dictionary<string, JObject> funcs = new Dictionary<string, JObject>();


        public JObject Function(string name) {
            if (!funcs.ContainsKey(name)) {
                funcs.Add(name, new JObject());
            }

            return funcs[name];
        }

    }
}
