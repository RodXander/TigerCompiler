using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.Semantics;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class IntNode : TypeNode
    {
        public IntNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            ReturnType = TypesResources.Int;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            gen.Generator.Emit(OpCodes.Ldc_I4, Int32.Parse(Text));
        }
    }
}
