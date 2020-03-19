using System;

using Emdl;
using McDevtools;

namespace Emdlcli {
    class Program {
        static void Main(string[] args) {

            var save = new McSave("Void");

            var dp = save.Datapack("planet-generator");

            save.Structure("bigtest").RegisterAt(dp, "hahatest");


            var project = new EmdlProject("emdlProj/", dp);
            project.Build();

            Console.WriteLine("Hello World!");
        }
    }
}
