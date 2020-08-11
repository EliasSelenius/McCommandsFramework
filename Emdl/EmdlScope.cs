using System;
using System.Collections.Generic;
using System.Text;

using McDevtools;
using Pgen;

namespace Emdl {

    class EmdlScope {

        public readonly EmdlScope parent;
        public McNamespace mcNamespace => parent?.mcNamespace ?? block.mcNamespace;
        public string scope_name => block.name;

        private readonly McFunction block;
        private readonly Dictionary<string, EmdlDataType> locals = new Dictionary<string, EmdlDataType>();

        public EmdlScope(EmdlScope parent, McFunction block) {
            this.parent = parent; this.block = block;
        }

        public bool hasLocal(string name) => parent != null ? parent.hasLocal(name) || locals.ContainsKey(name) : locals.ContainsKey(name);
        public void setLocal(string name, EmdlDataType type) {
            if (parent != null && parent.hasLocal(name)) parent.setLocal(name, type);
            else locals[name] = type;
        }
        public EmdlDataType? getLocal(string name) => locals.ContainsKey(name) ? locals[name] : parent?.getLocal(name);


        public void Overwrite(string t) => block.Overwrite(t);
        public void AppendLine(string t) => block.AppendLine(t);

        public void loopStatements(SyntaxTree.Node function_node, int startindex = 1) {
            this.Overwrite("# transpiled from emdl\n");

            for (int i = startindex; i < function_node.childCount; i++) {
                var n = function_node.GetChild(i);
                createStatement(n);
            }
        }

        void createStatement(SyntaxTree.Node statment_node) {
            if (statment_node.rule == EmdlParser.instance.primitivecall) this.AppendLine(statment_node.content.Substring(1));
            else if (statment_node.rule == EmdlParser.instance.ifblock) {
                var block = createBlock("if", statment_node);
                this.AppendLine("execute " + createCondition(statment_node.GetChild(0)) + " run function " + this.mcNamespace.name + ":" + block.scope_name);
            } else if (statment_node.rule == EmdlParser.instance.whileblock) {
                var block = createBlock("while", statment_node);
                var command = "execute " + createCondition(statment_node.GetChild(0)) + " run function " + this.mcNamespace.name + ":" + block.scope_name;
                this.AppendLine(command);
                block.AppendLine(command);
            } else if (statment_node.rule == EmdlParser.instance.assigment) {
                createAssigment(statment_node);
            } 
            else if (statment_node.rule == EmdlParser.instance.nbt_declaration) create_NbtDeclaration(statment_node);
            else if (statment_node.rule == EmdlParser.instance.int_declaration) create_IntDeclaration(statment_node);
            else if (statment_node.rule == EmdlParser.instance.float_declaration) create_FloatDeclaration(statment_node);
            
        }
        
        EmdlScope createBlock(string name, SyntaxTree.Node block_node) {
            var block = this.mcNamespace.Function("internal/" + name + "_" + System.IO.Path.GetRandomFileName().Replace(".", ""));
            var innerscope = new EmdlScope(this, block);
            innerscope.loopStatements(block_node);
            return innerscope;
        }

        string createCondition(SyntaxTree.Node condition_node) {
            if (condition_node.childCount == 1) {
                var first = condition_node.GetChild(0);
                if (first.rule == EmdlParser.instance.namespacedid) {
                    return "if predicate " + getNamespacedId(this.mcNamespace, first);
                } else if (first.rule == EmdlParser.instance.bool_expression) {
                    string f(SyntaxTree.Node n) {
                        if (n.rule == EmdlParser.instance.identifier) {
                            return scope_name + "_" + n.content;
                        } else return n.content;
                    }

                    var left = first.GetChild(0);
                    var op = first.GetChild(1);
                    var right = first.GetChild(2);
                    return $"if score {f(left)} number {op.content} {f(right)} number";
                }
            }

            throw new ParserException("failed to parse condition statement");
        }

        static string getNamespacedId(McNamespace mcns, SyntaxTree.Node node) {
            if (node.childCount == 2) return node.GetChild(0).content + ":" + node.GetChild(1).content;
            return mcns.name + ":" + node.GetChild(0).content;
        }

        #region declarations
        void create_NbtDeclaration(SyntaxTree.Node declaration_node) {
            var var_name = declaration_node.GetChild(0).content;
            declare(EmdlDataType.Nbt, var_name);

            var value_node = declaration_node.GetChild(1);
            string append;
            if (value_node.rule == EmdlParser.instance.path) {
                append = "from storage "; // TODO: implement
            } else append = "value " + EmdlParser.createValueAsText(value_node);
            
            block.AppendLine("data modify storage emdl-locals:" + mcNamespace.name + "/" + scope_name + " " + var_name + " set " + append);
        }
        void create_IntDeclaration(SyntaxTree.Node declaration_node) {
            var var_name = scope_name + "_" + declaration_node.GetChild(0).content;
            declare(EmdlDataType.Int, var_name);

            var value_node = declaration_node.GetChild(1);

            if (value_node.rule == EmdlParser.instance.expression && value_node.childCount == 1) {
                var n = value_node.GetChild(0);
                if (n.rule == EmdlParser.instance.number) {
                    block.AppendLine($"scoreboard players set {var_name} number {n.content}");
                }
            }

            
        }
        void create_FloatDeclaration(SyntaxTree.Node declaration_node) {

        }
        void declare(EmdlDataType type, string name) {
            if (hasLocal(name)) throw new Exception(name + " is already declared");
            setLocal(name, type);
        }
        #endregion

        void createAssigment(SyntaxTree.Node assigment_node) {

        }

    }
}
