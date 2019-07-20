using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace McCommandsFramework {
    public class McDatapack {


        public static string GetDatapacksDirectoryPath(string save) => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/.minecraft/saves/{save}/datapacks";
        public static DirectoryInfo GetDatapacksDirectory(string save) => new DirectoryInfo(GetDatapacksDirectoryPath(save));


        private readonly string Path;
        private DirectoryInfo RootDir => new DirectoryInfo(Path);
        private FileInfo PackMcMetaFile => new FileInfo(Path + "/pack.mcmeta");
        private DirectoryInfo DataDir => new DirectoryInfo(Path + "/data");

        private readonly Dictionary<string, McNamespace> namespaces = new Dictionary<string, McNamespace>();


        public McDatapack(string save, string name) {
            Path = GetDatapacksDirectoryPath(save) + $"/{name}";

            if (Directory.Exists(Path)) {
                // Parse:
                var dir = new DirectoryInfo(Path);
                var datadir = dir.GetDirectories("data", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (datadir == null) {
                    datadir = dir.CreateSubdirectory("data");
                }

                foreach (var nsdir in datadir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)) {
                    namespaces.Add(nsdir.Name, new McNamespace(nsdir.FullName));
                }

            } else {
                // init new:
                var dir = Directory.CreateDirectory(Path);
                File.WriteAllText(dir.FullName + "/pack.mcmeta", Templates.packmetafile);
                dir.CreateSubdirectory("data");
            }
        }


        public McNamespace Namespace(string name) {
            if(namespaces.ContainsKey(name)) {
                return namespaces[name];
            } else {
                var ns = new McNamespace(DataDir.CreateSubdirectory(name).FullName);
                namespaces.Add(name, ns);
                return ns;
            }
        }


        public static void InitDatapack(string save, string name) {
            var rootdir = Directory.CreateDirectory(GetDatapacksDirectoryPath(save) + $"/{name}");
            File.WriteAllText(rootdir.FullName + "/pack.mcmeta", Templates.packmetafile);

            //var mctagsfuncs = rootdir.CreateSubdirectory("data/minecraft/tags/functions");

        }
    }
}
