using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.Semantics;
using TigerCompiler.ErrorHandling;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class ForNode : ExpressionNode
    {
        public ForNode (IToken payload) : base(payload) { }

        public ExpressionNode BottomBound { get { return Children[1] as ExpressionNode; } }
        public ExpressionNode UpBound { get { return Children[2] as ExpressionNode; } }

        public override void CheckSemantics (Scope scope) {
            var index = scope.GetVarInfo(Children[0].Text, true);
            if (index != null) {
                Errors.AddSemanticError(SemanticErrorType.IdentifierAlreadyExist, index.Name, node: this);
                return;
            }
            VarInfo indexVar = new VarInfo 
            {
                Name = Children[0].Text,
                ReturnTypeSemantic = scope.GetTypeInfo(TypesResources.Int),
                InsideAFor = true,
            };
            scope.VarFuncScope.Add(Children[0].Text, indexVar);

            base.CheckSemantics(scope);

            if (BottomBound.ReturnType != null && BottomBound.ReturnType != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.InvalidExpressionType, TypesResources.Int, BottomBound.ReturnType, BottomBound);
            if (UpBound.ReturnType != null && UpBound.ReturnType != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.InvalidExpressionType, TypesResources.Int, UpBound.ReturnType, UpBound);

            ReturnType = GetChildAsExpression(3).ReturnType;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var result = gen.Generator.DeclareLocal(typeof(object));

            var initFor = gen.Generator.DefineLabel( );
            var endFor = gen.Generator.DefineLabel( );
            gen.BreakPoints.Push(endFor);

            var index = gen.Generator.DeclareLocal(typeof(int));
            (Scope.GetVarInfo(Children[0].Text) as VarInfo).VarLocalBuilder = index;

            GetChildAsExpression(1).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Stloc, index);

            var upperBound = gen.Generator.DeclareLocal(typeof(int));
            gen.Generator.MarkLabel(initFor);
            GetChildAsExpression(2).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Stloc, upperBound);

            gen.Generator.Emit(OpCodes.Ldloc, index);
            gen.Generator.Emit(OpCodes.Ldloc, upperBound);
            gen.Generator.Emit(OpCodes.Bgt, endFor);

            GetChildAsExpression(3).GenerateCode(gen);
            if (GetChildAsExpression(3).ReturnType != TypesResources.NoReturn)
                gen.Generator.Emit(OpCodes.Stloc, result);

            gen.Generator.Emit(OpCodes.Ldc_I4_1);
            gen.Generator.Emit(OpCodes.Ldloc, index);
            gen.Generator.Emit(OpCodes.Add);
            gen.Generator.Emit(OpCodes.Stloc, index);
            gen.Generator.Emit(OpCodes.Br, initFor);
            gen.Generator.MarkLabel(endFor);

            if (GetChildAsExpression(3).ReturnType != TypesResources.NoReturn)
                gen.Generator.Emit(OpCodes.Ldloc, result);
            gen.BreakPoints.Pop( );
        }
    }
}
