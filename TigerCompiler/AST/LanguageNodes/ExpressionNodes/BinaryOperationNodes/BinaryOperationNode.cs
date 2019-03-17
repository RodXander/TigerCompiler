using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.ErrorHandling;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    public abstract class BinaryOperationNode : ExpressionNode
    {
        public BinaryOperationNode (IToken payload) : base (payload) { }

        public abstract OpCode OperationOpCode { get; }

        public ExpressionNode LOperand { get { return Children[0] as ExpressionNode; } }
        public ExpressionNode ROperand { get { return Children[1] as ExpressionNode; } }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            if (LOperand.ReturnType != null && ROperand.ReturnType != null &&
                LOperand.ReturnType != TypesResources.Nil && ROperand.ReturnType != TypesResources.Nil &&
                LOperand.ReturnType != ROperand.ReturnType)
                Errors.AddSemanticError(SemanticErrorType.InvalidOperands, LOperand.ReturnType, ROperand.ReturnType, this);

            ReturnType = TypesResources.Int;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            LOperand.GenerateCode(gen);
            ROperand.GenerateCode(gen);
            gen.Generator.Emit(OperationOpCode);
        }
    }
}
