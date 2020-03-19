using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace McDevtools {
    public class McFunction {

        public readonly string name;
        public readonly McNamespace mcNamespace;

        public string path => mcNamespace.path + "/functions/" + name + ".mcfunction";

        internal McFunction(McNamespace ns, string name) {
            mcNamespace = ns;
            this.name = name;
        }


        public void Overwrite(string newcontent) => File.WriteAllText(path, newcontent);
        public void Overwrite(IEnumerable<string> lines) => File.WriteAllLines(path, lines);

        public void Append(string content) => File.AppendAllText(path, content);
        public void Append(IEnumerable<string> lines) => File.AppendAllLines(path, lines);

        public void AppendLine(string content) => Append(content + "\n");

        public void Clear() => File.WriteAllText(path, "");
    }
}
