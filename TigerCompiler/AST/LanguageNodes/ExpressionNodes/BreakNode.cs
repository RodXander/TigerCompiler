using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using TigerCompiler.ErrorHandling;
using TigerCompiler.Semantics;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class BreakNode : ExpressionNode
    {
        public BreakNode (IToken payload) : base (payload) { }

        public override void CheckSemantics (Scope scope) {
            ReturnType = TypesResources.NoReturn;
            
            ITree ancester = Parent;
            while (ancester != null) {
                if (ancester is ForNode || ancester is WhileNode)
                    return;
                if (ancester is ExpressionSeqNode)
                    (ancester as ExpressionSeqNode).ReturnType = TypesResources.NoReturn;
                ancester = ancester.Parent;
            }
            Errors.AddSemanticError(SemanticErrorType.InvalidBreakPosition, node: this);
        }

        public override void GenerateCode (CodeILGenerator gen) {
            gen.Generator.Emit(OpCodes.Br, gen.BreakPoints.Peek( ));
        }
    }
}
