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
    public class NegateNode : ExpressionNode
    {
        public NegateNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);
            
            string exprReturnType = (Children[0] as ExpressionNode).ReturnType;
            if (exprReturnType != null && exprReturnType != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, exprReturnType, TypesResources.Int, Children[0] as TigerASTNode);
            else
                ReturnType = TypesResources.Int;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            gen.Generator.Emit(OpCodes.Ldc_I4_M1);
            GetChildAsExpression(0).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Mul);
        }
    }
}
