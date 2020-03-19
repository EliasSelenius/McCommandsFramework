using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

/*


    namespace
    ┣ advancements
    ┃ ┗ *.json
    ┣ functions
    ┃ ┗ *.mcfunction
    ┣ loot_tables
    ┃ ┗ *.json
    ┣ predicates
    ┃ ┗ *.json
    ┣ recipes
    ┃ ┗ *.json
    ┣ structures
    ┃ ┗ *.nbt
    ┗ tags

 */

namespace McDevtools {

    /// <summary>
    /// folder structure: <br />
    /// 
    /// namespace           <br />
    /// ┣ advancements      <br />
    /// ┃ ┗ *.json          <br />
    /// ┣ functions         <br />
    /// ┃ ┗ *.mcfunction    <br />
    /// ┣ loot_tables       <br />
    /// ┃ ┗ *.json          <br />
    /// ┣ predicates        <br />
    /// ┃ ┗ *.json          <br />
    /// ┣ recipes           <br />
    /// ┃ ┗ *.json          <br />
    /// ┣ structures        <br />
    /// ┃ ┗ *.nbt           <br />
    /// ┗ tags              <br />
    /// 
    /// </summary>
    public class McNamespace {


        public readonly string name;
        public readonly McDatapack datapack;
        private readonly Dictionary<string, McFunction> functions = new Dictionary<string, McFunction>();
        
        public string path => datapack.path + "/data/" + name;


        private DirectoryInfo Dir => new DirectoryInfo(path);
        private DirectoryInfo FuncsDir => new DirectoryInfo(path + "/functions");



        internal McNamespace(McDatapack pack, string name) {
            datapack = pack;
            this.name = name;

            if(!Directory.Exists(path + "/functions")) {
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

                var f = new McFunction(this, name); // d.FullName + $"/{name.Substring(i + 1)}.mcfunction");
                functions.Add(name, f);
                return f;
            }
        }

    }
}
