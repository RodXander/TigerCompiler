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
    public class FunctionDeclarationNode : DeclarationNode
    {
        public FunctionDeclarationNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            Scope = new Scope(scope);

            if (scope.GetFuncInfo(TypeName) != null || scope.GetVarInfo(TypeName) != null) {
                Errors.AddSemanticError(SemanticErrorType.IdentifierAlreadyExist, TypeName, node: this);
                return;
            }

            TypeInfo returnTypeInfo;
            if (Children[1] is ReturnTypeNode) {
                returnTypeInfo = scope.GetTypeInfo(Children[2].Text);
                if (returnTypeInfo == null) {
                    Errors.AddSemanticError(SemanticErrorType.TypeDoesNotExist, Children[2].Text, node: Children[2] as TigerASTNode);
                    return;
                }
            }
            else returnTypeInfo = scope.GetTypeInfo(TypesResources.NoReturn);

            int parametersCount = 1;
            int parametersDeclarationCounter = Children[1] is ReturnTypeNode ? 3 : 1;
            var parameters = new SortedDictionary<int, VarInfo>( );
            while (Children[parametersDeclarationCounter++] is TypeDeclarationFieldNode) {

                string parameterName = Children[parametersDeclarationCounter++].Text;
                if (Scope.GetVarInfo(parameterName, true) != null/* || Scope.GetFuncInfo(parameterName) != null*/) {
                    Errors.AddSemanticError(SemanticErrorType.IdentifierAlreadyExist, parameterName, node: Children[parametersDeclarationCounter - 1] as TigerASTNode);
                    return;
                }
                string parameterType = Children[parametersDeclarationCounter++].Text;
                var parameterTypeInfo = scope.GetTypeInfo(parameterType);
                if (parameterTypeInfo == null) {
                    Errors.AddSemanticError(SemanticErrorType.TypeDoesNotExist, parameterType, node: Children[parametersDeclarationCounter - 1] as TigerASTNode);
                    return;
                }

                var parameterInfo = new VarInfo { Name = parameterName, ReturnTypeSemantic = parameterTypeInfo, InsideAFunction = true };
                Scope.VarFuncScope.Add(parameterName, parameterInfo);
                parameters.Add(parametersCount++, parameterInfo);
            }
            WellFormed = true;
            scope.VarFuncScope.Add(TypeName, new FuncInfo { Name = TypeName, ReturnTypeSemantic = returnTypeInfo, Parameters = parameters });
        }

        public override void PostCheckSemantics (Scope scope) {
            var funcInfo = scope.GetFuncInfo(Children[0].Text);

            (Children[ChildCount - 1] as LanguageNode).CheckSemantics(Scope);

            string funcReturn = Children[1] is ReturnTypeNode ? Children[2].Text : TypesResources.NoReturn;
            string exprReturn = (Children[ChildCount - 1] as ExpressionNode).ReturnType;

            if (exprReturn != null && exprReturn != funcReturn)
                Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, funcReturn, exprReturn, this);
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var funcCodeILGen = new CodeILGenerator(Scope.GetFuncInfo(Children[0].Text).FunctionBuilder.GetILGenerator( ));
            funcCodeILGen.NestedClassesCounter = gen.NestedClassesCounter;
            funcCodeILGen.ProgramType = gen.ProgramType;
            funcCodeILGen.Module = gen.Module;

            int indexParams = 0;
            foreach (var parameter in Scope.GetFuncInfo(Children[0].Text).Parameters.Values) {
                parameter.VarLocalBuilder = funcCodeILGen.Generator.DeclareLocal(parameter.ReturnTypeSemantic.ReturnTypeGen);
                funcCodeILGen.Generator.Emit(OpCodes.Ldarg, indexParams);
                funcCodeILGen.Generator.Emit(OpCodes.Stloc, parameter.VarLocalBuilder);
                indexParams++;
            }

            (Children[ChildCount - 1] as ExpressionNode).GenerateCode(funcCodeILGen);
            
            if (Scope.GetFuncInfo(Children[0].Text).ReturnTypeSemantic.Name == TypesResources.NoReturn &&
                (Children[ChildCount - 1] as ExpressionNode).ReturnType != TypesResources.NoReturn)
                funcCodeILGen.Generator.Emit(OpCodes.Pop);

            funcCodeILGen.Generator.Emit(OpCodes.Ret);
        }
    }
}