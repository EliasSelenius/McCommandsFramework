using System;
using System.Collections.Generic;
using System.Text;

using Pgen;
using McDevtools;

namespace Emdl {
    public class EmdlParser : Parser {

        private static readonly EmdlParser singelton;

        private const double scorefloatmultiplyer = 10000;

        static EmdlParser() {
            singelton = new EmdlParser();
        }

        private EmdlParser() { }

        public static void Parse(string sourceCode, McDatapack datapack) {
            var syntaxTree = singelton.Parse(sourceCode);
            Console.WriteLine(syntaxTree.AsText());

            for (int i = 0; i < syntaxTree.rootNode.childCount; i++) {
                createNamespace(datapack, syntaxTree.rootNode.GetChild(i));
            }
        }

        #region syntaxtree reading

        static void createNamespace(McDatapack pack, SyntaxTree.Node namespace_node) {
            var name = namespace_node.GetChild(0).content;
            var mcns = pack.Namespace(name);

            for (int i = 1; i < namespace_node.childCount; i++) {
                var n = namespace_node.GetChild(i);

                if (n.rule == singelton.function) createFunction(mcns, n);
                else if (n.rule == singelton.predicate) createPredicate(mcns, n);
                else throw new ParserException(n.rule.name + " was not expected here");
            }

        }

        static void createFunction(McNamespace mcns, SyntaxTree.Node function_node) {
            var name = function_node.GetChild(0).content;
            var scope = new EmdlScope(null, mcns.Function(name));
            loopStatements(scope, function_node);
        }

        static void createPredicate(McNamespace mcns, SyntaxTree.Node predicate_node) {
            var name = predicate_node.GetChild(0).content;
            var compund_node = predicate_node.GetChild(1);
        }

        static void loopStatements(EmdlScope scope, SyntaxTree.Node function_node) {
            scope.Overwrite("# transpiled from emdl\n");

            for (int i = 1; i < function_node.childCount; i++) {
                var n = function_node.GetChild(i);
                createStatement(scope, n);
            }
        }

        static void createStatement(EmdlScope scope, SyntaxTree.Node statment_node) {
            if (statment_node.rule == singelton.primitivecall) scope.AppendLine(statment_node.content.Substring(1));
            else if (statment_node.rule == singelton.ifblock) {
                var block = createBlock("if", scope, statment_node);
                scope.AppendLine("execute " + createCondition(scope, statment_node.GetChild(0)) + " run function " + scope.mcNamespace.name + ":" + block.name);
            } else if (statment_node.rule == singelton.whileblock) {
                var block = createBlock("while", scope, statment_node);
                var command = "execute " + createCondition(scope, statment_node.GetChild(0)) + " run function " + scope.mcNamespace.name + ":" + block.name;
                scope.AppendLine(command);
                block.AppendLine(command);
            } else if (statment_node.rule == singelton.assigment) createAssigment(scope, statment_node);
        }

        static void createAssigment(EmdlScope scope, SyntaxTree.Node assigment_node) {

            /*
                scenarios:
                    assignment from constant:
                        score:
                            x = 10
                        nbt:
                            foo.x = 10
                            foo = {x:10}
                    assignment from variable:
                        score:
                            x = foo.x // if foo.x is a numeric or boolean value
                        nbt:
                            foo.x = bar
                            x = foo.x // if foo.x is anything but a numeric or boolean value

             */


            var localname = getPath(assigment_node.GetChild(0));
            var localname_ispath = localname.Contains(".") || localname.Contains("[");
            var value_node = assigment_node.GetChild(1);
            var storagename = "emdl-locals:" + scope.mcNamespace.name + "/" + scope.name;

            if (value_node.rule == singelton.path) {
                // assignment from variable
                var value_path = getPath(value_node);
                var value_type = scope.getLocal(value_path) ?? throw new Exception(value_path + " does not exist in the current context");
               
                if (!localname_ispath && value_type == EmdlDataType.NumericOrBoolean) {
                    // score
                } else {
                    // nbt
                    //data modify storage wow:test rot set from storage wow:test dwa_dw
                    scope.AppendLine("data modify storage " + storagename + " " + localname + " set from storage " + storagename + " " + value_path);
                    scope.setLocal(localname, EmdlDataType.Nbt);
                }

            } else {
                // assignment from constant
                if (!localname_ispath && (value_node.rule == singelton.number)) {
                    // score
                    var scorename = "emdl_" + scope.mcNamespace.name + "_" + scope.name + "_" + localname;
                    scope.AppendLine("scoreboard players set " + scorename + " number " + (int)(double.Parse(value_node.content) * scorefloatmultiplyer));
                    scope.setLocal(localname, EmdlDataType.NumericOrBoolean);
                } else {
                    // nbt
                    scope.AppendLine("data modify storage " + storagename + " " + localname + " set value " + createValueAsText(value_node));
                    scope.setLocal(localname, EmdlDataType.Nbt);
                }
            }
             

            //data modify storage emdl-locals:haha/test data set value {my-numbers:[1,2,3,6,7]}
        }

       

        static EmdlScope createBlock(string name, EmdlScope scope, SyntaxTree.Node block_node) {
            var block = scope.mcNamespace.Function(name + "_" + System.IO.Path.GetRandomFileName().Replace(".", ""));
            var innerscope = new EmdlScope(scope, block);
            loopStatements(innerscope, block_node);
            return innerscope;
        }

        static string getPath(SyntaxTree.Node path_node) {
            var res = "";
            for (int i = 0; i < path_node.childCount; i++) {
                var c = path_node.GetChild(i);
                var co = c.content;
                if (c.rule == singelton.number) res += "[" + co + "]";
                else res += "." + co;
            }
            return res.Substring(1);
        }

        static string getNamespacedId(McNamespace mcns, SyntaxTree.Node node) {
            if (node.childCount == 2) return node.GetChild(0).content + ":" + node.GetChild(1).content;
            return mcns.name + ":" + node.GetChild(0).content;
        }

        static string createValueAsText(SyntaxTree.Node value_node) {
            if (value_node.rule == singelton.compund) {
                var res = "{";
                for (int i = 0; i < value_node.childCount; i++) {
                    var kv = value_node.GetChild(i);
                    res += kv.GetChild(0).content + ":" + createValueAsText(kv.GetChild(1)) + ",";
                }
                return res.Substring(0, res.Length - 1) + "}";
            }

            if (value_node.rule == singelton.list) {
                var res = "";
                for (int i = 0; i < value_node.childCount; i++) {
                    res += ", " + createValueAsText(value_node.GetChild(i));
                }
                return "[" + res.Substring(2) + "]";
            }

            return value_node.content;
        }

        static string createCondition(EmdlScope scope, SyntaxTree.Node condition_node) {
            if (condition_node.childCount == 1) {
                var first = condition_node.GetChild(0);
                if (first.rule == singelton.namespacedid) {
                    return "if predicate " + getNamespacedId(scope.mcNamespace, first);
                }
            }

            throw new ParserException("failed to parse condition statement");
        }


        #endregion


#pragma warning disable IDE0051 // Remove unused private members

        // Tokens
        [Rule("\\s+"), Skip] Lexrule whitespace;
        [Rule(":")] Lexrule colon;
        [Rule(",")] Lexrule comma;
        [Rule("\\.")] Lexrule punct;
        [Rule("{")] Lexrule opencurl;
        [Rule("}")] Lexrule closecurl;
        [Rule("\\[")] Lexrule opensq;
        [Rule("\\]")] Lexrule closesq;
        [Rule("<=")] Lexrule lteq;
        [Rule(">=")] Lexrule gteq;
        [Rule("=")] Lexrule eq;
        [Rule("<")] Lexrule lt;
        [Rule(">")] Lexrule gt;


        // values
        [Rule("(?:\"\")|(?:\".*?[^\\\\]\")", true)] Lexrule @string;
        [Rule("(?:\\d+\\.\\.\\d+)", true)] Lexrule range;
        [Rule("-?\\d+(?:\\.\\d+)?", true)] Lexrule number;
        [Rule("true|false", true)] Lexrule boolean;

        // comments
        [Rule("//.*?\\n"), Skip] Lexrule line_comment;
        [Rule("/\\*(?:.*?\\n?)*\\*/"), Skip] Lexrule block_comment;

        [Rule("[a-z0-9_-]+", true)] Lexrule identifier;

        [Rule("^/.*?\\n", true)] Lexrule primitivecall;

        [Rule("namespace*")] Parserule main;

        [Rule("identifier colon identifier | identifier", createNode:true)] Parserule namespacedid;
        [Rule("identifier subpath*",createNode:true)] Parserule path;
        [Rule("punct identifier | opensq number closesq")] Parserule subpath;

        [Rule("'namespace' identifier opencurl programentity* closecurl", createNode: true)] Parserule @namespace;

        // Program entities. e.g function, predicate
        [Rule("function | predicate")] Parserule programentity;
        [Rule("'function' identifier opencurl statement* closecurl", createNode: true)] Parserule function;
        [Rule("'predicate' identifier compund", createNode:true)] Parserule predicate;


        // Statements. e.g if, while, ...
        [Rule("primitivecall | ifblock | whileblock | assigment")] Parserule statement;
        [Rule("'if' condition opencurl statement* closecurl", createNode:true)] Parserule ifblock;
        [Rule("'while' condition opencurl statement* closecurl", createNode: true)] Parserule whileblock;

        [Rule("namespacedid", createNode:true)] Parserule condition;

        // assigments
        [Rule("path eq value | path eq path",createNode:true)] Parserule assigment;



        // NBT & JSON
        [Rule("number | boolean | string | compund | list")] Parserule value;
        
        // compund:
        [Rule("opencurl compund_item* keyvaluepair closecurl | opencurl closecurl", createNode: true)] Parserule compund;
        [Rule("keyvaluepair comma")] Parserule compund_item;
        [Rule("identifier colon value", createNode:true)] Parserule keyvaluepair;

        // list
        [Rule("opensq list_item* value closesq | opensq closesq", createNode:true)] Parserule list;
        [Rule("value comma")] Parserule list_item;



#pragma warning restore IDE0051 // Remove unused private members


    }
}
