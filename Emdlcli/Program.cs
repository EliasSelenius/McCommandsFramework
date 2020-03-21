using System;

using Emdl;
using McDevtools;

using Nums;

namespace Emdlcli {
    class Program {
        static void Main(string[] args) {

            var save = new McSave("Void");

            var dp = save.Datapack("planet-generator");

            {
                var f = save.Structure("bigtest1");
                f.Offset(new vec3(0, 0, 32));
                var boat = save.Structure("bigtest0").Merge(f);
                boat.Offset(new vec3(-5, 0, -20));
                boat.RegisterAt(dp, "wow");
            }


            var project = new EmdlProject("emdlProj/", dp);
            project.Build();

            Console.WriteLine("Hello World!");
        }
    }
}
