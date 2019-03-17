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
    public class IfThenElseNode : ExpressionNode
    {
        public IfThenElseNode (IToken payload) : base (payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            if (GetChildAsExpression(0).ReturnType != null && GetChildAsExpression(0).ReturnType != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.InvalidExpressionType, TypesResources.Int, GetChildAsExpression(0).ReturnType, node: GetChildAsExpression(0));

            if (GetChildAsExpression(1).ReturnType != null && GetChildAsExpression(2).ReturnType != null)
                if (GetChildAsExpression(1).ReturnType != GetChildAsExpression(2).ReturnType)
                    Errors.AddSemanticError(SemanticErrorType.InvalidIfBody, GetChildAsExpression(1).ReturnType, GetChildAsExpression(2).ReturnType, this);
                else ReturnType = GetChildAsExpression(1).ReturnType;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var endLabel = gen.Generator.DefineLabel( );
            var elseLabel = gen.Generator.DefineLabel( );

            GetChildAsExpression(0).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Brfalse, elseLabel);

            GetChildAsExpression(1).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Br, endLabel);

            gen.Generator.MarkLabel(elseLabel);
            GetChildAsExpression(2).GenerateCode(gen);

            gen.Generator.MarkLabel(endLabel);
        }
    }
}
