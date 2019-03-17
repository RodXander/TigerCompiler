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
    public class VarDeclarationNode : DeclarationNode
    {
        public VarDeclarationNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            var varInfo = scope.GetVarInfo(TypeName, true);
            if (varInfo != null || scope.GetFuncInfo(TypeName, true) != null) {
                Errors.AddSemanticError(SemanticErrorType.IdentifierAlreadyExist, varInfo.Name, node: this);
                return;
            }

            var typeInfo = new TypeInfo( );
            string exprInitVarReturn;
            if (Children[1] is ReturnTypeNode) {
                (Children[3] as ExpressionNode).CheckSemantics(new Scope(scope));

                typeInfo = scope.GetTypeInfo(Children[2].Text);
                if (typeInfo == null) {
                    Errors.AddSemanticError(SemanticErrorType.TypeDoesNotExist, Children[2].Text, node: Children[2] as TigerASTNode);
                    return;
                }
                
                exprInitVarReturn = (Children[3] as ExpressionNode).ReturnType;
                if (exprInitVarReturn != null) {

                    if (exprInitVarReturn == TypesResources.Nil && 
                       (typeInfo.Name == TypesResources.Int || typeInfo.Name == TypesResources.Nil)) {
                           Errors.AddSemanticError(SemanticErrorType.InvalidNilOperation, node: Children[3] as TigerASTNode);
                        return;
                    }
                    if (exprInitVarReturn == TypesResources.Nil || scope.GetTypeInfo(exprInitVarReturn).Equals(scope.GetTypeInfo(typeInfo.Name)))
                        scope.VarFuncScope.Add(TypeName, new VarInfo { 
                            Name = TypeName, ReturnTypeSemantic = typeInfo,
                        });
                    else
                        Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, typeInfo.Name, exprInitVarReturn, Children[3] as TigerASTNode);
                }
            }
            else {
                (Children[1] as ExpressionNode).CheckSemantics(new Scope(scope));

                exprInitVarReturn = (Children[1] as ExpressionNode).ReturnType;
                if (exprInitVarReturn != null) {

                    if (exprInitVarReturn == TypesResources.Nil) {
                        Errors.AddSemanticError(SemanticErrorType.InvalidNilOperation, node: Children[1] as TigerASTNode);
                        return;
                    }
                    if (exprInitVarReturn == TypesResources.NoReturn) {
                        Errors.AddSemanticError(SemanticErrorType.NoReturnValue, node: Children[1] as TigerASTNode);
                        return;
                    }

                    typeInfo = scope.GetTypeInfo(exprInitVarReturn);
                    if (typeInfo == null)
                        Errors.AddSemanticError(SemanticErrorType.ReturnTypeNoVisible, exprInitVarReturn, node: Children[1] as TigerASTNode);
                    else
                        scope.VarFuncScope.Add(TypeName, new VarInfo {
                            Name = TypeName,
                            ReturnTypeSemantic = typeInfo,
                        });
                }
            }
        }

        public override void PostCheckSemantics (Scope scope) {
            throw new NotSupportedException( );
        }

        public override void GenerateCode (CodeILGenerator gen) {
            (Children[ChildCount - 1] as LanguageNode).GenerateCode(gen);
            
            var field = gen.ProgramType.DefineField(TypeName, Scope.VarFuncScope[TypeName].ReturnTypeSemantic.ReturnTypeGen, System.Reflection.FieldAttributes.Static | System.Reflection.FieldAttributes.Public);
            gen.Generator.Emit(OpCodes.Stsfld, field);

            (Scope.VarFuncScope[TypeName] as VarInfo).VarFieldBuilder = field;
        }
    }
}