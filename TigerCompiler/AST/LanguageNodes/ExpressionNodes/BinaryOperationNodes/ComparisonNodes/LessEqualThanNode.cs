using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class LessEqualThanNode : ComparisonNode
    {
        public LessEqualThanNode (IToken payload) : base(payload) { }

        public override OpCode OperationOpCode { get { throw new NotSupportedException( ); } }

        public override void GenerateCode (CodeILGenerator gen) {
            var trueLabel = gen.Generator.DefineLabel( );
            var endLabel = gen.Generator.DefineLabel( );

            LOperand.GenerateCode(gen);
            ROperand.GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Ble, trueLabel);

            gen.Generator.Emit(OpCodes.Ldc_I4_0);
            gen.Generator.Emit(OpCodes.Br, endLabel);

            gen.Generator.MarkLabel(trueLabel);
            gen.Generator.Emit(OpCodes.Ldc_I4_1);

            gen.Generator.MarkLabel(endLabel);
        }
    }
}
