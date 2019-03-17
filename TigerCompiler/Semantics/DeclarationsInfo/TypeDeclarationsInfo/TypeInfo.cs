using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.Semantics
{
    public class TypeInfo : ItemInfo
    {
        public Type ReturnTypeGen { get; set; }

        public override bool Equals (object obj) {
            return Name == ((TypeInfo) obj).Name;
        }
    }
}
