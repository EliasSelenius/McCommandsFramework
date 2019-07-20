using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Nums;
using Nums.Vectors;

namespace McCommandsFramework {
    class Program {


        static void Main(string[] args) {

            var dp = new McDatapack("Void", "testautopack");

            var g = VoxelGeometry.Parse("Files/Cube.txt");
            g.Scaler = 0.625f;
            g.RegisterAt(dp, "mytestmesh");

            var b = VoxelGeometry.Parse("Files/Boat.txt");
            b.Scaler = 0.625f;
            b.RegisterAt(dp, "boat");


            Console.Read();
        }



        public static void Test() {
            float[] voxels = new float[] {
                1, 1, 1,
                -1, 1, -1,
                1, 1, -1,
                -1, 1, 1
            };

            var sbld = new StringBuilder();
            var tbld = new StringBuilder();

            for (int i = 0; i < voxels.Length; i += 3) {
                var v = new Vec3(voxels[i], voxels[i + 1], voxels[i + 2]);
                v *= 0.625f;
                string tag = "box_child_" + v.x + "_" + v.y + "_" + v.z;

                sbld.AppendLine("summon minecraft:armor_stand ~ ~ ~ {Tags:[\"" + tag + "\"],ArmorItems: [{},{},{},{id:\"minecraft:stone\", Count:1}]}");

                tbld.AppendLine($"teleport @e[tag={tag}] ^{v.x} ^{v.y} ^{v.z}");
            }


            Console.WriteLine(sbld.ToString());
            Console.WriteLine(tbld.ToString());
        }   
        


        
    }
}
