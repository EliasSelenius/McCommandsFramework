using System;
using System.Collections.Generic;
using System.Text;

using McDevtools;

namespace Emdl {
    
    public enum EmdlDataType {
        Nbt,
        NumericOrBoolean, 
        Entity
    }


    class EmdlScope {



        public readonly EmdlScope parent;
        public McNamespace mcNamespace => parent?.mcNamespace ?? block.mcNamespace;
        public string name => block.name;

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

    }
}
