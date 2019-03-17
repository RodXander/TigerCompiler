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
    public class IfThenNode : ExpressionNode
    {
        public IfThenNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            if (GetChildAsExpression(0).ReturnType != null && GetChildAsExpression(0).ReturnType != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.InvalidExpressionType, TypesResources.Int, GetChildAsExpression(0).ReturnType, GetChildAsExpression(0));

            if (GetChildAsExpression(1).ReturnType != null && GetChildAsExpression(1).ReturnType != TypesResources.NoReturn)
                Errors.AddSemanticError(SemanticErrorType.InvalidExpressionType, TypesResources.NoReturn, GetChildAsExpression(1).ReturnType, GetChildAsExpression(1));
            else
                ReturnType = TypesResources.NoReturn;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var endLabel = gen.Generator.DefineLabel( );

            GetChildAsExpression(0).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Brfalse, endLabel);
            
            GetChildAsExpression(1).GenerateCode(gen);
            gen.Generator.MarkLabel(endLabel);
        }
    }
}
