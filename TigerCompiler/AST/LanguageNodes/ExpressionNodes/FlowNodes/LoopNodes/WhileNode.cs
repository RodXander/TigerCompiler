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
    public class WhileNode : ExpressionNode
    {
        public WhileNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            if (GetChildAsExpression(1).ReturnType != null && GetChildAsExpression(0).ReturnType != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.InvalidExpressionType, TypesResources.Int, GetChildAsExpression(0).ReturnType, GetChildAsExpression(0));

            if (GetChildAsExpression(1).ReturnType != null && GetChildAsExpression(1).ReturnType != TypesResources.NoReturn)
                Errors.AddSemanticError(SemanticErrorType.InvalidExpressionType, TypesResources.NoReturn, GetChildAsExpression(1).ReturnType, GetChildAsExpression(1));
            else
                ReturnType = TypesResources.NoReturn;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var endLoopLabel = gen.Generator.DefineLabel( );
            var initLoopLabel = gen.Generator.DefineLabel( );

            gen.BreakPoints.Push(endLoopLabel);

            gen.Generator.MarkLabel(initLoopLabel);
            GetChildAsExpression(0).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Brfalse, endLoopLabel);

            GetChildAsExpression(1).GenerateCode(gen);

            gen.Generator.Emit(OpCodes.Br, initLoopLabel);
            
            gen.Generator.MarkLabel(endLoopLabel);
            gen.BreakPoints.Pop( );
        }
    }
}
