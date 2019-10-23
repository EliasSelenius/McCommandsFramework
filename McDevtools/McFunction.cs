using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace McDevtools {
    public class McFunction {
        private readonly string filepath;

        public McFunction(string path) {
            filepath = path;
        }


        public void Overwrite(string newcontent) => File.WriteAllText(filepath, newcontent);
        public void Overwrite(IEnumerable<string> lines) => File.WriteAllLines(filepath, lines);

        public void Append(string content) => File.AppendAllText(filepath, content);
        public void Append(IEnumerable<string> lines) => File.AppendAllLines(filepath, lines);

        public void AppendLine(string content) => Append(content + "\n");

        public void Clear() => File.WriteAllText(filepath, "");
    }
}
