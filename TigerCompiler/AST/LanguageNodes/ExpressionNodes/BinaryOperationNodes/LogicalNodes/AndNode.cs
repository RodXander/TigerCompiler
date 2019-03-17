using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class AndNode : LogicalNode
    {
        public AndNode (IToken payload) : base(payload) { }

        public override void GenerateCode (CodeILGenerator gen) {
            var falseLabel = gen.Generator.DefineLabel( );
            var endLabel = gen.Generator.DefineLabel( );

            LOperand.GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Brfalse, falseLabel);

            ROperand.GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Brfalse, falseLabel);

            gen.Generator.Emit(OpCodes.Ldc_I4_1);
            gen.Generator.Emit(OpCodes.Br, endLabel);

            gen.Generator.MarkLabel(falseLabel);
            gen.Generator.Emit(OpCodes.Ldc_I4_0);

            gen.Generator.MarkLabel(endLabel);
        }
    }
}
