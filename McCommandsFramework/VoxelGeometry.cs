using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Nums.Vectors;

using static System.Math;

namespace McCommandsFramework {
    public class VoxelGeometry {

        public const string DefaultBlock = "minecraft:stone";

        public Dictionary<string, List<Vec3>> Voxels = new Dictionary<string, List<Vec3>>();

        public float Scaler = 1;

        

        public static VoxelGeometry Parse(string filepath) {

            bool _parseVec(string s, out float[] o) {
                var strs = s.Split(' ');
                o = new float[strs.Length];
                for (int i = 0; i < strs.Length; i++) {
                    if (float.TryParse(strs[i], out float x)) {
                        o[i] = x;
                    } else {
                        return false;
                    }
                }
                return true;
            }
            


            var src = File.ReadAllLines(filepath);
            var res = new VoxelGeometry();

            string use = DefaultBlock; // if block is not specified, use deafult block
            bool mirrorX = false;
            bool mirrorY = false;
            bool mirrorZ = false;

            foreach (var l in src) {
                var line = l.Trim();
                if (!string.IsNullOrWhiteSpace(line)) {
                    if (line.StartsWith("use ")) {
                        use = line.Substring(4);
                        res.Voxels.Add(use, new List<Vec3>());
                        continue;
                    } else if (line.StartsWith("mirror ")) {
                        var axis = line.Substring(6);
                        if(axis.Contains("x")) {
                            mirrorX = true;
                        } else if (axis.Contains("y")) {
                            mirrorY = true;
                        } else if (axis.Contains("z")) {
                            mirrorZ = true;
                        } else if (axis.Contains("none")) {
                            mirrorX = mirrorY = mirrorZ = false;
                        }
                        continue;
                    } else if (line.StartsWith("//")) {
                        continue;
                    } else if (line.StartsWith("fill ")) {
                        var valstr = line.Substring(5);
                        if (_parseVec(valstr, out float[] array)) {
                            var v1 = new Vec3(array[0], array[1], array[2]);
                            var v2 = new Vec3(array[3], array[4], array[5]);

                            var min = new Vec3(Min(v1.x, v2.x), Min(v1.y, v2.y), Min(v1.z, v2.z));
                            var max = new Vec3(Max(v1.x, v2.x), Max(v1.y, v2.y), Max(v1.z, v2.z));

                            for (float x = min.x; x <= max.x; x++) {
                                for (float y = min.y; y <= max.y; y++) {
                                    for (float z = min.z; z <= max.z; z++) {
                                        _addVoxel(x, y, z);
                                    }
                                }
                            }

                        } else {
                            Console.WriteLine("Voxel Parsing error: " + line);
                        }
                    } else {
                        var charvalues = line.Split(' ');
                        if (float.TryParse(charvalues[0], out float x) && float.TryParse(charvalues[1], out float y) && float.TryParse(charvalues[2], out float z)) {
                            _addVoxel(x, y, z);

                        } else {
                            Console.WriteLine("Voxel Parsing error: " + line);
                        }
                    }
                }
            }

            void _addVoxel(float x, float y, float z) {
                // add voxel:
                res.Voxels[use].Add(new Vec3(x, y, z));
                // add mirrors:
                if (mirrorX && x != 0) {
                    res.Voxels[use].Add(new Vec3(-x, y, z));
                }
                if (mirrorY && y != 0) {
                    res.Voxels[use].Add(new Vec3(x, -y, z));
                }
                if (mirrorZ && z != 0) {
                    res.Voxels[use].Add(new Vec3(x, y, -z));
                }
            }

            Console.WriteLine("Voxel Parsing done with " + filepath);
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
