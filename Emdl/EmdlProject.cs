using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using McDevtools;

namespace Emdl {
    public class EmdlProject {

        public readonly string Path;
        public readonly McDatapack output;

        public EmdlProject(string path, McDatapack datapack) {
            Path = path;
            output = datapack;
        }

        public void InitializeConfig() {

        }

        /// <summary>
        /// transpiles and builds datapack
        /// </summary>
        public void Build() {
            foreach (var item in Directory.GetFiles(Path, "*.emdl", SearchOption.AllDirectories)) {
                Transpiler.Transpile(output, File.ReadAllText(item));
            }
        }
    
    }
}
