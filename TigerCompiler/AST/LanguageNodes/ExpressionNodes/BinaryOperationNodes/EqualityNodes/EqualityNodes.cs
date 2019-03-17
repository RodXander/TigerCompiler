using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.ErrorHandling;

namespace TigerCompiler.AST
{
    public abstract class EqualityNodes : BinaryOperationNode
    {
        public EqualityNodes (IToken payload) : base(payload) { }

        public override void CheckSemantics (Semantics.Scope scope) {
            base.CheckSemantics(scope);

            if (LOperand.ReturnType == TypesResources.Nil && ROperand.ReturnType == TypesResources.Nil)
                Errors.AddSemanticError(SemanticErrorType.InvalidNilOperation, node: this);

            if (LOperand is EqualityNodes || LOperand is ComparisonNode) Errors.AddSemanticError(SemanticErrorType.NoLeftAssociate, node: this);
        }
    }
}
