using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Nums.Vectors;

namespace McCommandsFramework {
    public class VoxelGeometry {

        public const string DefaultBlock = "minecraft:stone";

        public Dictionary<string, List<Vec3>> Voxels = new Dictionary<string, List<Vec3>>();

        public float Scaler = 1;

        public static VoxelGeometry Parse(string filepath) {
            var src = File.ReadAllLines(filepath);
            var res = new VoxelGeometry();

            string use = DefaultBlock; // if block is not specified, use deafult block
            foreach (var line in src) {
                if (!string.IsNullOrWhiteSpace(line)) {
                    if (line.StartsWith("use ")) {
                        use = line.Substring(4);
                        res.Voxels.Add(use, new List<Vec3>());
                        continue;
                    }

                    var charvalues = line.Split(' ');
                    if (float.TryParse(charvalues[0], out float x) && float.TryParse(charvalues[1], out float y) && float.TryParse(charvalues[2], out float z)) {
                        res.Voxels[use].Add(new Vec3(x, y, z));
                    } else {
                        // parsing error...
                    }

                }
            }

            return res;
        }


        public void RegisterAt(McDatapack dp, string name) {

            var parenttag = $"{name}_parent";

            var sf = dp.Namespace("geometry").Function($"{name}/init");
            sf.Overwrite($"# auto generated geometry for initing {name}\n");
            sf.AppendLine($"summon minecraft:armor_stand ~ ~ ~ {{Tags:[\"{parenttag}\"]}}");

            var uf = dp.Namespace("geometry").Function($"{name}/update");
            uf.Overwrite($"# auto generated geometry for updateing {name}\n");

            var kf = dp.Namespace("geometry").Function($"{name}/kill");
            kf.Overwrite($"# auto generated geometry for destroying {name}\n");
            kf.AppendLine($"kill @e[tag={parenttag}]");

            dp.Namespace("std").Function("update").AppendLine($"execute at @e[tag={parenttag}] run function geometry:{name}/update");

            foreach (var kv in Voxels) {
                for (int i = 0; i < kv.Value.Count; i++) {
                    var pos = kv.Value[i] * Scaler;
                    var postag = $"{name}_child_{pos.x}_{pos.y}_{pos.z}";
                    sf.AppendLine($"summon minecraft:armor_stand ~{pos.x} ~{pos.y} ~{pos.z} {{Tags:[\"{postag}\"],ArmorItems: [{{}},{{}},{{}},{{id:\"{kv.Key}\", Count:1}}],Invisible:1b}}");
                    uf.AppendLine($"teleport @e[tag={postag}] ^{pos.x} ^{pos.y} ^{pos.z} ~ ~");
                    kf.AppendLine($"kill @e[tag={postag}]");
                }

            }
        }
    }
}
