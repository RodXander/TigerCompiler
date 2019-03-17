using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using System.Reflection.Emit;
using TigerCompiler.Semantics;
using TigerCompiler.ErrorHandling;

namespace TigerCompiler.AST
{
    public class NotEqualNode : EqualityNodes
    {
        public NotEqualNode (IToken payload) : base(payload) { }

        public override OpCode OperationOpCode { get { throw new NotSupportedException( ); } }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var falseLabel = gen.Generator.DefineLabel( );
            var endLabel = gen.Generator.DefineLabel( );

            LOperand.GenerateCode(gen);
            ROperand.GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Beq, falseLabel);

            gen.Generator.Emit(OpCodes.Ldc_I4_1);
            gen.Generator.Emit(OpCodes.Br, endLabel);

            gen.Generator.MarkLabel(falseLabel);
            gen.Generator.Emit(OpCodes.Ldc_I4_0);

            gen.Generator.MarkLabel(endLabel);
        }
    }
}
