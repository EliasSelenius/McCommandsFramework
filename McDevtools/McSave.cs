using System;
using System.Collections.Generic;
using System.Text;

using System.IO;


/*
 
    API description:

    McSave
    -> McDatapack
    -> Structures (saved by structure blocks)



*/


namespace McDevtools {
    public class McSave {

        public readonly string name;
        public readonly string path;

        private readonly Dictionary<string, McDatapack> datapacks = new Dictionary<string, McDatapack>();

        public McSave(string name) {
            this.name = name;
            path = PathsHelper.mcSavesDirectory + name;

            if (!Directory.Exists(path)) {
                throw new Exception("Save file does'nt exist");
            }
        }

        public VoxelGeometry Structure(string name) {
            return VoxelGeometry.FromStructureNBT(Nbt.Decoder.Decode(path + "/generated/minecraft/structures/" + name + ".nbt"));
        }

        public McDatapack Datapack(string name) {
            if (datapacks.ContainsKey(name)) return datapacks[name];

            var dp = new McDatapack(this, name);
            datapacks[name] = dp;
            return dp;
        }


    }
}
