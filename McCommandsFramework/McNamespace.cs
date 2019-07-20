using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace McCommandsFramework {
    public class McNamespace {

        private readonly string Path;
        private DirectoryInfo Dir => new DirectoryInfo(Path);
        private DirectoryInfo FuncsDir => new DirectoryInfo(Path + "/functions");

        private readonly Dictionary<string, McFunction> functions = new Dictionary<string, McFunction>();

        

        public McNamespace(string path) {
            Path = path;
            if(!Directory.Exists(Path + "/functions")) {
                Dir.CreateSubdirectory("functions");
            }
            // TODO: parse exsisting files
        }

        public McFunction Function(string name) {
            if (functions.ContainsKey(name)) {
                return functions[name];
            } else {

                DirectoryInfo d = FuncsDir;
                int i = name.LastIndexOf('/');
                if(i != -1) {
                    string subdirs = name.Substring(0, i);
                    d = FuncsDir.CreateSubdirectory(subdirs); // does this overwrite if the subdirs already exsist?
                }

                var f = new McFunction(d.FullName + $"/{name.Substring(i + 1)}.mcfunction");
                functions.Add(name, f);
                return f;
            }
        }

    }
}
