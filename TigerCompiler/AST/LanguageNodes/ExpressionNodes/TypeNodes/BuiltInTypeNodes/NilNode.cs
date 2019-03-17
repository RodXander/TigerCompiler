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
    public class NilNode : TypeNode
    {
        public NilNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            ReturnType = TypesResources.Nil;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            gen.Generator.Emit(OpCodes.Ldnull);
        }
    }
}
