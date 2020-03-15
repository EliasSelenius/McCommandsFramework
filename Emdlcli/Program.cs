using System;

using Emdl;
using McDevtools;

namespace Emdlcli {
    class Program {
        static void Main(string[] args) {

            var dp = new McDatapack("Void", "emdltest");

            Transpiler.Transpile(dp, System.IO.File.ReadAllText("file1.emdl"));

            Console.WriteLine("Hello World!");
        }
    }
}
