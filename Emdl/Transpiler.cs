using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Pgen;

using McDevtools;

namespace Emdl {
    public static class Transpiler {

        public static readonly Lexer lexer = new Lexer(
            Lexrules.Whitespace,
            new Lexrule("function", "function"),
            new Lexrule("namespace", "namespace"),
            new Lexrule("identifier", "[a-z\\-_0-9]+"),
            new Lexrule("closeCurl", "}"),
            new Lexrule("openCurl", "{"),
            new Lexrule("primitiveCall", "/.*?\\n")
            );

        public static void Transpile(McDatapack datapack, string source) {

            var whitespace = Lexrules.Whitespace;
            var function = lexer.GetRule("function");
            var namespace_rule = lexer.GetRule("namespace");
            var identifier = lexer.GetRule("identifier");
            var openCurl = lexer.GetRule("openCurl");
            var closeCurl = lexer.GetRule("closeCurl");
            var primitiveCall = lexer.GetRule("primitiveCall");


            var tokens = lexer.Lex(source);
            var reader = new TokenReader(tokens);
            
            

            // a .mcfunction file
            void block(McNamespace ns, string name) {
                var func = ns.Function(name);
                func.Overwrite("# transpiled from emdl");
                func.AppendLine("\n");
                
                reader.AssertNext(openCurl);

                while (true) {
                    var t = reader.Next();
                    if (t.type == closeCurl) break;
                    if (t.type == whitespace) continue;


                    if (t.type == primitiveCall) {
                        func.Append(t.value.Substring(1));
                        continue;
                    }

                    throw new ParserException(t.type.name + " was not handled");
                }

            }


            // a namespace
            void parse_namespace(string name) {
                var ns = datapack.Namespace(name);

                reader.AssertNext(openCurl);
                
                while (true) {
                    var t = reader.Next();
                    if (t.type == closeCurl) break;
                    if (t.type == whitespace) continue;

                    if (t.type == function) {
                        reader.AssertNext(whitespace);
                        var fname = reader.AssertNext(identifier).value;
                        reader.AssertNext(whitespace);

                        block(ns, fname);
                        continue;
                    }

                    throw new ParserException(t.type.name + " was not handled");
                }
            }

            while (!reader.IsEnd) {
                var t = reader.Next();
                if (t.type == whitespace) continue;
                
                if (t.type == namespace_rule) {
                    reader.AssertNext(whitespace);
                    var nsname = reader.AssertNext(identifier).value;
                    reader.AssertNext(whitespace);

                    parse_namespace(nsname);

                    continue;
                }

            }
        }

       

    }
}
