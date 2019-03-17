using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.ErrorHandling;
using TigerCompiler.Semantics;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class ExpressionSeqNode : ExpressionNode
    {
        public ExpressionSeqNode (IToken payload) : base (payload) { }
        
        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            if (ReturnType == null) ReturnType = ChildCount == 0 ? TypesResources.NoReturn : GetChildAsExpression(ChildCount - 1).ReturnType;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            for (int i = 0; i < ChildCount; i++) {
                GetChildAsExpression(i).GenerateCode(gen);
                if (i < ChildCount - 1 && GetChildAsExpression(i).ReturnType != TypesResources.NoReturn) gen.Generator.Emit(OpCodes.Pop);
            }
        }
    }
}
