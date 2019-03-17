using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.ErrorHandling;
using TigerCompiler.Semantics;
using System.Reflection.Emit;
using TigerCompiler.CodeGeneration;

namespace TigerCompiler.AST
{
    public class FunctionNode : ExpressionNode
    {
        public FunctionNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            var funcInfo = scope.GetFuncInfo(Children[0].Text);
            if (funcInfo != null) {
                ReturnType = funcInfo.ReturnTypeSemantic.Name;

                if (ChildCount - 1 != funcInfo.Parameters.Count)
                    Errors.AddSemanticError(SemanticErrorType.InvalidParametersNumber, funcInfo.Name, funcInfo.Parameters.Count.ToString( ), this);
                else
                    for (int i = 1; i < ChildCount; i++) {

                        string actualParameterType = GetChildAsExpression(i).ReturnType;
                        if (actualParameterType != null && actualParameterType != funcInfo.Parameters[i].ReturnTypeSemantic.Name)
                            Errors.AddSemanticError(SemanticErrorType.InvalidParameterType, i.ToString( ), funcInfo.Parameters[i].ReturnTypeSemantic.Name, this);
                    }

                if (Semantics.Scope.IsStandardFunction(Children[0].Text) &&
                    !StandardLibrary.UsedFunctions.Contains(Children[0].Text))
                    StandardLibrary.UsedFunctions.Add(Children[0].Text);
            }
            else
                Errors.AddSemanticError(SemanticErrorType.FunctionDoesNotExist, Children[0].Text, node: this);
        }

        public override void GenerateCode (CodeILGenerator gen) {
            for (int i = 1; i < ChildCount; i++)
                GetChildAsExpression(i).GenerateCode(gen);
            
            gen.Generator.EmitCall
            (
                OpCodes.Call, Scope.GetFuncInfo(Children[0].Text).FunctionBuilder,
                Scope.GetFuncInfo(Children[0].Text).Parameters.Values.Select(x => x.ReturnTypeSemantic.ReturnTypeGen).ToArray( )
            );
        }
    }
}
