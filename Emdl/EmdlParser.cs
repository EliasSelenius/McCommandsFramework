using System;
using System.Collections.Generic;
using System.Text;

using Pgen;
using McDevtools;
using McDevtools.Nbt;

namespace Emdl {
    public class EmdlParser : Parser {

        public static readonly EmdlParser instance;

        public const int scoredecimalPlaces = 4;
        public static double nbt2score => Math.Pow(10, scoredecimalPlaces);
        public static double score2nbt => 1 / nbt2score;

        static EmdlParser() {
            instance = new EmdlParser();
        }

        private EmdlParser() { }



#pragma warning disable IDE0051 // Remove unused private members

        #region Tokens
        // Tokens
        [Rule("\\s+"), Skip] public Lexrule whitespace;
        [Rule(":")] public Lexrule colon;
        [Rule(",")] public Lexrule comma;
        [Rule("\\.")] public Lexrule punct;
        [Rule("{")] public Lexrule opencurl;
        [Rule("}")] public Lexrule closecurl;
        [Rule("\\[")] public Lexrule opensq;
        [Rule("\\]")] public Lexrule closesq;
        [Rule("\\(")] public Lexrule openbrack;
        [Rule("\\)")] public Lexrule closebrack;
        [Rule("#")] public Lexrule hash;

        // datatypes
        [Rule("entity")] public Lexrule datatype_entity;
        [Rule("nbt")] public Lexrule datatype_nbt;
        [Rule("int")] public Lexrule datatype_int;
        [Rule("float")] public Lexrule datatype_float;

        // comments
        [Rule("//.*?\\n"), Skip] public Lexrule line_comment;
        [Rule("/\\*(?:.*?\\n?)*\\*/"), Skip] public Lexrule block_comment;

        // boolean operations
        [Rule("<=", true)] public Lexrule lteq;
        [Rule(">=", true)] public Lexrule gteq;
        [Rule("<", true)] public Lexrule lt;
        [Rule(">", true)] public Lexrule gt;
        [Rule("==", true)] public Lexrule iseq;

        [Rule("=")] public Lexrule eq;
        [Rule("\\+", true)] public Lexrule plus;
        [Rule("-", true)] public Lexrule minus;
        [Rule("\\*", true)] public Lexrule mult;
        [Rule("/", true)] public Lexrule div;



        // values
        [Rule("(?:\"\")|(?:\".*?[^\\\\]\")", true)] public Lexrule @string;
        [Rule("(?:\\d+\\.\\.\\d+)", true)] public Lexrule range;
        [Rule("-?\\d+(?:\\.\\d+)?", true)] public Lexrule number;
        [Rule("true|false", true)] public Lexrule boolean;


        [Rule("public", createNode: true)] public Lexrule accessmodifier;

        [Rule("[a-z0-9_-]+", true)] public Lexrule identifier;

        [Rule("^\\$.*?\\n", true)] public Lexrule primitivecall;

        #endregion

        // parse rules
        [Rule("namespace*")] public Parserule main;

        [Rule("identifier colon identifier | identifier", createNode: true)] public Parserule namespacedid;
        [Rule("hash namespacedid", createNode: true)] public Parserule tag;
        [Rule("identifier subpath*", createNode: true)] public Parserule path;
        [Rule("punct identifier | opensq number closesq")] public Parserule subpath;

        [Rule("'namespace' identifier opencurl programentity* closecurl", createNode: true)] public Parserule @namespace;


        // Program entities. e.g function, predicate
        [Rule("function | predicate")] public Parserule programentity;
        [Rule("tag* accessmodifier? 'function' identifier opencurl statement* closecurl", createNode: true)] public Parserule function;
        [Rule("'predicate' identifier compund", createNode: true)] public Parserule predicate;


        // Statements. e.g if, while, ...
        [Rule("primitivecall | ifblock | whileblock | assigment | declaration")] public Parserule statement;
        [Rule("'if' condition opencurl statement* closecurl", createNode: true)] public Parserule ifblock;
        [Rule("'while' condition opencurl statement* closecurl", createNode: true)] public Parserule whileblock;

        // conditions
        [Rule("iseq | lteq | gteq | lt | gt")] public Parserule bool_op;
        [Rule("identifier | number")] public Parserule bool_expr_entity;
        [Rule("bool_expr_entity bool_op bool_expr_entity", createNode: true)] public Parserule bool_expression;
        [Rule("bool_expression | namespacedid", createNode: true)] public Parserule condition;


        // declaration
        [Rule("datatype_entity | datatype_nbt | datatype_int | datatype_float")] public Parserule datatype;
        [Rule("nbt_declaration | int_declaration | float_declaration")] public Parserule declaration;
        [Rule("datatype_nbt identifier eq value | datatype_nbt identifier eq path", createNode: true)] public Parserule nbt_declaration;

        [Rule("datatype_int identifier eq number_value", createNode: true)] public Parserule int_declaration;
        [Rule("datatype_float identifier eq number_value", createNode: true)] public Parserule float_declaration;
        [Rule("path | expression")] public Parserule number_value;

        // assigments
        [Rule("path sub_assig", createNode: true)] public Parserule assigment;
        [Rule("eq expression | eq value | eq path")] Parserule sub_assig;

        // aritmetic expression
        [Rule("plus | minus | mult | div")] public Parserule arith_op;
        [Rule("expr", createNode: true)] public Parserule expression;
        [Rule("expr_entity sub_expr* | openbrack expression closebrack")] public Parserule expr;
        [Rule("number | identifier")] public Parserule expr_entity;
        [Rule("arith_op expr")] public Parserule sub_expr;


        // NBT & JSON
        [Rule("number | boolean | string | compund | list")] public Parserule value;

        // compund:
        [Rule("opencurl compund_item* keyvaluepair closecurl | opencurl closecurl", createNode: true)] public Parserule compund;
        [Rule("keyvaluepair comma")] public Parserule compund_item;
        [Rule("identifier colon value", createNode: true)] public Parserule keyvaluepair;

        // list
        [Rule("opensq list_item* value closesq | opensq closesq", createNode: true)] public Parserule list;
        [Rule("value comma")] public Parserule list_item;



#pragma warning restore IDE0051 // Remove unused private members



        public static void Parse(string sourceCode, McDatapack datapack) {
            var syntaxTree = instance.Parse(sourceCode);
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

                if (n.rule == instance.function) createFunction(mcns, n);
                else if (n.rule == instance.predicate) createPredicate(mcns, n);
                else throw new ParserException(n.rule.name + " was not expected here");
            }

        }

        static void createFunction(McNamespace mcns, SyntaxTree.Node function_node) {

            var name = "private/";
            var index = 0;
            while (true) {
                var n = function_node.GetChild(index);
                if (n.rule == instance.accessmodifier) {
                    name = "";
                } else if (n.rule == instance.tag) {
                    

                } else if (n.rule == instance.identifier) {
                    name += n.content;
                    break;
                }
                index++;
            }

            var scope = new EmdlScope(null, mcns.Function(name));
            scope.loopStatements(function_node, index);
        }

        static void createPredicate(McNamespace mcns, SyntaxTree.Node predicate_node) {
            var name = predicate_node.GetChild(0).content;
            var compund_node = predicate_node.GetChild(1);
        }


        internal static string getPath(SyntaxTree.Node path_node) {
            var res = "";
            for (int i = 0; i < path_node.childCount; i++) {
                var c = path_node.GetChild(i);
                var co = c.content;
                if (c.rule == instance.number) res += "[" + co + "]";
                else res += "." + co;
            }
            return res.Substring(1);
        }

        

        internal static string createValueAsText(SyntaxTree.Node value_node) {
            if (value_node.rule == instance.compund) {
                var res = "{";
                for (int i = 0; i < value_node.childCount; i++) {
                    var kv = value_node.GetChild(i);
                    res += kv.GetChild(0).content + ":" + createValueAsText(kv.GetChild(1)) + ",";
                }
                return res.Substring(0, res.Length - 1) + "}";
            }

            if (value_node.rule == instance.list) {
                var res = "";
                for (int i = 0; i < value_node.childCount; i++) {
                    res += ", " + createValueAsText(value_node.GetChild(i));
                }
                return "[" + res.Substring(2) + "]";
            }

            return value_node.content;
        }

        


        #endregion



    }
}
