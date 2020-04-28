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
            
            Lexrules.String,
            new Lexrule("range", "\\d+\\.\\.\\d+"),
            Lexrules.Number,

            new Lexrule("function", "function"),
            new Lexrule("namespace", "namespace"),
            new Lexrule("predicate", "predicate"),
            new Lexrule("identifier", "[A-Za-z0-9_-]+"),

            new Lexrule("colon", ":"),
            new Lexrule("comma", ","),
            new Lexrule("openSqbr", "\\["),
            new Lexrule("closeSqbr", "\\]"),
            new Lexrule("openCurl", "{"),
            new Lexrule("closeCurl", "}"),

            new Lexrule("line_comment", false, "//.*?$", System.Text.RegularExpressions.RegexOptions.Multiline),
            new Lexrule("block_comment", false, "/\\*.*?\\*/", System.Text.RegularExpressions.RegexOptions.Singleline),
            
            new Lexrule("primitiveCall", "^/[^/].*?\\n"),
            new Lexrule("loop", "for|foreach|while")
            

            );

        public static void Transpile(McDatapack datapack, string source) {

            var whitespace = Lexrules.Whitespace;
            var string_rule = Lexrules.String;
            var range = lexer.GetRule("range");
            var function = lexer.GetRule("function");
            var namespace_rule = lexer.GetRule("namespace");
            var predicate = lexer.GetRule("predicate");
            var colon = lexer.GetRule("colon");
            var comma = lexer.GetRule("comma");
            var line_comment = lexer.GetRule("line_comment");
            var block_comment = lexer.GetRule("block_comment");
            var identifier = lexer.GetRule("identifier");
            var openSqbr = lexer.GetRule("openSqbr");
            var closeSqbr = lexer.GetRule("closeSqbr");
            var openCurl = lexer.GetRule("openCurl");
            var closeCurl = lexer.GetRule("closeCurl");
            var primitiveCall = lexer.GetRule("primitiveCall");
            var loop = lexer.GetRule("loop");

            var tokens = lexer.Lex(source);
            var reader = new TokenReader(tokens);
            
            

            // a .mcfunction file
            void parse_block(McNamespace ns, string name) {
                var func = ns.Function(name);
                func.Overwrite("# transpiled from emdl");
                func.AppendLine("\n");
                
                reader.AssertNext(openCurl);

                while (true) {
                    var t = reader.Next();
                    if (t.type == closeCurl) break;
                    if (t.type == whitespace) continue;
                    if (t.type == line_comment) continue;
                    if (t.type == block_comment) continue;


                    if (t.type == primitiveCall) {
                        func.Append(t.value.Substring(1));
                        continue;
                    }


                    if (t.type == loop) {
                        
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

                        parse_block(ns, fname);
                        continue;
                    }

                    if (t.type == predicate) {
                        reader.AssertNext(whitespace);
                        var fname = reader.AssertNext(identifier).value;
                        reader.AssertNext(whitespace);
                        reader.Next();
                        var c = parse_compund();
                        var s = stringify(c);
                        continue;
                    }

                    throw new ParserException(t.type.name + " was not handled");
                }
            }

            Dictionary<string, object> parse_compund() {
                reader.Assert(openCurl);

                var res = new Dictionary<string, object>();

                var expectcomma = false;

                while (true) {
                    var t = reader.Next();

                    if (t.type == closeCurl) break;
                    if (t.type == whitespace) continue;

                    if (expectcomma) {
                        reader.Assert(comma);
                        expectcomma = false;
                        continue;
                    }

                    if (t.type == identifier) {
                        reader.AssertNext(colon);
                        reader.Next();
                        reader.SkipIf(whitespace);
                        res.Add(t.value, parse_value());
                        expectcomma = true;
                        continue;
                    }


                    throw new ParserException("unexpected token: " + t.type.name);
                }
                return res;
            }

            List<object> parse_list() {
                reader.Assert(openSqbr);
                var res = new List<object>();

                var expectcomma = false;

                while (true) {
                    var t = reader.Next();

                    if (t.type == closeSqbr) break;
                    if (t.type == whitespace) continue;

                    if (expectcomma) {
                        reader.Assert(comma);
                        expectcomma = false;
                        continue;
                    }

                    res.Add(parse_value());
                    expectcomma = true;
                }

                return res;
            }

            object parse_value() {
                var t = reader.Current;

                if (t.type == openCurl) {
                    return parse_compund();
                } else if (t.type == openSqbr) {
                    return parse_list();
                } else if (t.type == string_rule) {
                    return t.value.Trim('"');
                }

                throw new ParserException("unexpected token: " + t.type.name);
            }

            string stringify(object obj) {
                if (obj is string s) {
                    return "\"" + s + "\"";
                } else if (obj is Dictionary<string, object> c) {
                    if (c.Count == 0) return "{}";
                    return "{" + c.Select(x => "\"" + x.Key + "\":" + stringify(x.Value)).Aggregate((x, y) => x + "," + y) + "}";
                } else if (obj is List<object> l) {
                    if (l.Count == 0) return "[]";
                    return "[" + l.Select(x => stringify(x)).Aggregate((x, y) => x + "," + y);
                }

                // this should never happen
                throw new Exception(obj.GetType().Name + " can not be stringified");
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


                throw new ParserException("unexpected token: " + t.type.name);
            }
        }

       

    }
}
