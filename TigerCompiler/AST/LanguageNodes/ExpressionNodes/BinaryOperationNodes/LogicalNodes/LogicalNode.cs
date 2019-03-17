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
    public abstract class LogicalNode : BinaryOperationNode
    {
        public LogicalNode (IToken payload) : base (payload) { }

        public override OpCode OperationOpCode { get { throw new NotSupportedException( ); } }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            if (LOperand.ReturnType != null && LOperand.ReturnType != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, LOperand.ReturnType, TypesResources.Int, LOperand);
            if (ROperand.ReturnType != null && ROperand.ReturnType != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, ROperand.ReturnType, TypesResources.Int, ROperand);
        }
    }
}
