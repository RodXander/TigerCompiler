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
    public class ArrayNode : TypeNode
    {
        public ArrayNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            var arrayName = Children[0].Text;
            var typeArrayInfo = scope.GetTypeInfo(arrayName);
            if (typeArrayInfo != null)
                if (!(typeArrayInfo is ArrayTypeInfo))
                    Errors.AddSemanticError(SemanticErrorType.TypeNoIndexable, arrayName, node: this);
                else
                    ReturnType = arrayName;
            else
                Errors.AddSemanticError(SemanticErrorType.TypeDoesNotExist, arrayName, node: this);

            string exprNumbElementsReturn = GetChildAsExpression(1).ReturnType;
            if (exprNumbElementsReturn != null && exprNumbElementsReturn != TypesResources.Int)
                Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, exprNumbElementsReturn, TypesResources.Int, GetChildAsExpression(1));

            string exprInitElemetsReturn = GetChildAsExpression(2).ReturnType;
            if (exprInitElemetsReturn != null && typeArrayInfo is ArrayTypeInfo) {
                if (exprInitElemetsReturn == TypesResources.Nil) {
                    if ((typeArrayInfo as ArrayTypeInfo).ElementsType.Name == TypesResources.Int)
                        Errors.AddSemanticError(SemanticErrorType.InvalidNilOperation, node: this);
                }
                else if (exprInitElemetsReturn != (typeArrayInfo as ArrayTypeInfo).ElementsType.Name)
                    Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, exprInitElemetsReturn, (typeArrayInfo as ArrayTypeInfo).ElementsType.Name, GetChildAsExpression(2));
            }
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var arrayInfo = (ArrayTypeInfo) Scope.GetTypeInfo(Children[0].Text);
            var elemNumb = gen.Generator.DeclareLocal(typeof(int));
            var array = gen.Generator.DeclareLocal(arrayInfo.ReturnTypeGen);

            GetChildAsExpression(1).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Stloc, elemNumb);

            gen.Generator.Emit(OpCodes.Ldloc, elemNumb);
            gen.Generator.Emit(OpCodes.Newarr, arrayInfo.ElementsType.ReturnTypeGen);
            gen.Generator.Emit(OpCodes.Stloc, array);

            var index = gen.Generator.DeclareLocal(typeof(int));
            gen.Generator.Emit(OpCodes.Ldc_I4_0);
            gen.Generator.Emit(OpCodes.Stloc, index);

            var initLabel = gen.Generator.DefineLabel( );
            var endLabel = gen.Generator.DefineLabel( );

            gen.Generator.MarkLabel(initLabel);
            gen.Generator.Emit(OpCodes.Ldloc, index);
            gen.Generator.Emit(OpCodes.Ldloc, elemNumb);
            gen.Generator.Emit(OpCodes.Beq, endLabel);

            gen.Generator.Emit(OpCodes.Ldloc, array);
            gen.Generator.Emit(OpCodes.Ldloc, index);
            GetChildAsExpression(2).GenerateCode(gen);
            gen.Generator.Emit(OpCodes.Stelem, arrayInfo.ElementsType.ReturnTypeGen);

            gen.Generator.Emit(OpCodes.Ldc_I4_1);
            gen.Generator.Emit(OpCodes.Ldloc, index);
            gen.Generator.Emit(OpCodes.Add);
            gen.Generator.Emit(OpCodes.Stloc, index);
            gen.Generator.Emit(OpCodes.Br, initLabel);

            gen.Generator.MarkLabel(endLabel);
            gen.Generator.Emit(OpCodes.Ldloc, array);
        }
    }
}
