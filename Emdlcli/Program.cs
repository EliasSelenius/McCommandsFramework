using System;

using Emdl;
using McDevtools;

using Nums;

namespace Emdlcli {
    class Program {
        static void Main(string[] args) {

            var save = new McSave("Void");

            var name = new System.IO.DirectoryInfo(".").Name;
            var dp = save.Datapack(name);
            dp.Clear();

            var project = new EmdlProject(".", dp);
            project.Build();

            Console.WriteLine("Hello World!");
        }
    }
}
