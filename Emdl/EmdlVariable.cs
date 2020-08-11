using System;
using System.Collections.Generic;
using System.Text;

namespace Emdl {

    public enum EmdlDataType {
        Nbt,
        Float,
        Int,
        Entity
    }

    abstract class EmdlVariable {
        public abstract EmdlDataType type { get; }
        public string name;
        public abstract string set();
        public abstract string initializationCodeFrom();
    }
    /*
    class EmdlInt : EmdlVariable {
        public override EmdlDataType type => EmdlDataType.Int;

        // scoreboard players set name number 0
        // scoreboard players operation y number = x number

        public override string initializationCode() {
            return "scoreboard players set " + name + " number " + 
        }
    }

    class EmdlFloat : EmdlVariable {
        public override EmdlDataType type => EmdlDataType.Float;

        public override string initializationCode() {
            throw new NotImplementedException();
        }
    }

    class EmdlNbt : EmdlVariable {
        public override EmdlDataType type => EmdlDataType.Nbt;

        public override string initializationCode() {
            throw new NotImplementedException();
        }
    }*/
}
