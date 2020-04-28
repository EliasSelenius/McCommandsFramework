using McDevtools;
using Pgen;
using System;
using System.Collections.Generic;
using System.Text;

namespace Emdl {
    public abstract class Node {

        public abstract void Parse(TokenReader reader);

        public abstract void Transpile(McDatapack datapack);

    }


    public class ForLoop : Node {

        private static int idCount = 0;


        public readonly int id;
        public readonly string namespaceName;
        public readonly string functionName;
        public string scopeName => functionName + "_loop_" + id; 


        public ForLoop(string ns, string func) {
            namespaceName = ns;
            functionName = func;
            id = ++idCount;
        }

        public override void Parse(TokenReader reader) {
        }

        public override void Transpile(McDatapack datapack) {
            var ns = datapack.Namespace(namespaceName);
            var func = ns.Function(functionName);
            var scope = ns.Function(scopeName);

            func.AppendLine("/function " + namespaceName + ":" + scopeName);

        }
    }


}
