using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

/*
 
    datapack folder structure

    <datapack name> {
        pack.mcmeta
        data {
            <namespace> {
                functions {
                    func.mcfunction
                }
                predicates {}

            }
        }
    }
*/

namespace McDevtools {
    public class McDatapack {


        public readonly string name;
        public readonly McSave savefile;

        public string path => savefile.path + "/datapacks/" + name;

        private readonly Dictionary<string, McNamespace> namespaces = new Dictionary<string, McNamespace>();

        public Meta packmeta = new Meta { pack_format = 4, description = "" };

        
        
        private DirectoryInfo RootDir => new DirectoryInfo(path);
        private FileInfo PackMcMetaFile => new FileInfo(path + "/pack.mcmeta");
        private DirectoryInfo DataDir => new DirectoryInfo(path + "/data");


        public class Meta {
            public int pack_format;
            public string description;

            public string GetAsJson() {
                return "{ \"pack\": { \"pack_format\": " + pack_format + ", \"description\": \"" + description + "\" }}";
            }
        }


        internal McDatapack(McSave save, string name) { 
            savefile = save;
            this.name = name;

            init();
        }

        private void init() {
            if (Directory.Exists(path)) {
                var dir = new DirectoryInfo(path);
                
                // make sure data directory exists
                var datadir = dir.GetDirectories("data", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (datadir == null) {
                    datadir = dir.CreateSubdirectory("data");
                }

                // loop all namespaces in data dir
                foreach (var nsdir in datadir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)) {
                    namespaces.Add(nsdir.Name, new McNamespace(this, nsdir.Name));
                }

                // TODO: parse pack.mcmeta json file into packmeta


            } else {
                // init new
                var dir = Directory.CreateDirectory(path);
                File.WriteAllText(dir.FullName + "/pack.mcmeta", packmeta.GetAsJson());
                dir.CreateSubdirectory("data");
            }
        }


        public McNamespace Namespace(string name) {
            if(namespaces.ContainsKey(name)) {
                return namespaces[name];
            } else {
                DataDir.CreateSubdirectory(name);
                var ns = new McNamespace(this, name);
                namespaces.Add(name, ns);
                return ns;
            }
        }


        
            //var mctagsfuncs = rootdir.CreateSubdirectory("data/minecraft/tags/functions");

        
    }
}
