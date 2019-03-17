using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.ErrorHandling;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    public abstract class ComparisonNode : BinaryOperationNode
    {
        public ComparisonNode (IToken payload) : base (payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);
            if (LOperand.ReturnType == TypesResources.Nil || ROperand.ReturnType == TypesResources.Nil)
                Errors.AddSemanticError(SemanticErrorType.InvalidNilComparison, node: this);
            if (LOperand.ReturnType != TypesResources.Int && LOperand.ReturnType != TypesResources.String)
                Errors.AddSemanticError(SemanticErrorType.InvalidOperandsComparer, node: this);
            if (LOperand is EqualityNodes || LOperand is ComparisonNode) Errors.AddSemanticError(SemanticErrorType.NoLeftAssociate, node: this);
        }
    }
}
