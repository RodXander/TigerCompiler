using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.Semantics;
using TigerCompiler.ErrorHandling;
using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class LetNode : ExpressionNode
    {
        public LetNode (IToken payload) : base(payload) { Dependencies = new Dictionary<string, List<string>>( ); }

        public Dictionary<string, List<string>> Dependencies { get; set; }

        public override void CheckSemantics (Scope scope) {
            Scope = scope;
            int i = 0;
            while (i < ChildCount && !(Children[i] is ExpressionSeqNode)) {

                if (Children[i] is VarDeclarationNode) {
                    (Children[i] as VarDeclarationNode).CheckSemantics(scope);
                    i++;
                }
                else if (Children[i] is FunctionDeclarationNode) {
                    var wellFormedFunctions = new List<FunctionDeclarationNode>( );
                    while (i < ChildCount && Children[i] is FunctionDeclarationNode) {
                        var funcDeclarationNode = (Children[i] as FunctionDeclarationNode);
                        funcDeclarationNode.CheckSemantics(scope);

                        if (funcDeclarationNode.WellFormed) wellFormedFunctions.Add(funcDeclarationNode);
                        i++;
                    }
                    foreach (var item in wellFormedFunctions) item.PostCheckSemantics(scope);
                }
                else {
                    var wellFormedTypes = new List<TypeDeclarationNode>( );
                    while (i < ChildCount && Children[i] is TypeDeclarationNode) {

                        var typeDeclarationNode = (Children[i] as TypeDeclarationNode);
                        typeDeclarationNode.CheckSemantics(scope);

                        if (typeDeclarationNode.WellFormed) wellFormedTypes.Add(typeDeclarationNode);
                        i++;
                    }
                    foreach (var item in wellFormedTypes) item.PostCheckSemantics(scope);
                }
            }

            if (i < ChildCount) {
                (Children[i] as ExpressionSeqNode).CheckSemantics(new Scope(scope));
                ReturnType = (Children[i] as ExpressionSeqNode).ReturnType;
            }
            else ReturnType = TypesResources.NoReturn;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var newProgramType = gen.ProgramType.DefineNestedType
            (
                String.Format("Program{0}", gen.NestedClassesCounter + 1),
                System.Reflection.TypeAttributes.NestedPublic
            );
            var newMain = newProgramType.DefineMethod
            (
                String.Format("Main{0}", gen.NestedClassesCounter + 1), 
                System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                System.Reflection.CallingConventions.Standard, 
                GetChildAsExpression(ChildCount - 1).ReturnType == TypesResources.NoReturn ? typeof(void) : typeof(object), System.Type.EmptyTypes
            );

            gen.Generator.EmitCall(OpCodes.Call, newMain, System.Type.EmptyTypes);

            var newCodeILGenerator = new CodeILGenerator(newMain.GetILGenerator( ));
            newCodeILGenerator.ProgramType = newProgramType;
            newCodeILGenerator.NestedClassesCounter++;
            newCodeILGenerator.Module = gen.Module;

            int i = 0;
            while (i < ChildCount && !(Children[i] is ExpressionSeqNode)) {
                if (Children[i] is VarDeclarationNode) {
                    (Children[i] as VarDeclarationNode).GenerateCode(newCodeILGenerator);
                    i++;
                }
                else if (Children[i] is FunctionDeclarationNode) {
                    int init = i;
                    while (i < ChildCount && Children[i] is FunctionDeclarationNode) {
                        CreateFunctions(newCodeILGenerator, (Children[i] as FunctionDeclarationNode).Children[0].Text);
                        i++;
                    }
                    i = init;
                    while (i < ChildCount && Children[i] is FunctionDeclarationNode) {
                        (Children[i] as FunctionDeclarationNode).GenerateCode(newCodeILGenerator);
                        i++;
                    }
                }
                else {
                    int init = i;
                    while (i < ChildCount && Children[i] is TypeDeclarationNode) {
                        if ((Children[i] as TypeDeclarationNode).Children[1] is RecordTypeDeclarationNode)
                            CreateRecordTypes(gen, (Children[i] as TypeDeclarationNode).Children[0].Text);
                        else if ((Children[i] as TypeDeclarationNode).Children[1] is ArrayTypeDeclarationNode)
                            CreateArrayTypes(newCodeILGenerator, (Children[i] as TypeDeclarationNode).Children[0].Text);
                        i++;
                    }
                    i = init;
                    while (i < ChildCount && Children[i] is TypeDeclarationNode) {
                        if ((Children[i] as TypeDeclarationNode).Children[1] is RecordTypeDeclarationNode)
                            (Children[i] as TypeDeclarationNode).GenerateCode(gen);
                        i++;
                    }
                }
            }

            GetChildAsExpression(ChildCount - 1).GenerateCode(newCodeILGenerator);
            newCodeILGenerator.Generator.Emit(OpCodes.Ret);
            newProgramType.CreateType( );
        }

        void CreateFunctions (CodeILGenerator gen, string name) {
            Scope.GetFuncInfo(name).FunctionBuilder = gen.ProgramType.DefineMethod
            (
                name, System.Reflection.MethodAttributes.Static | System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard,
                Scope.GetFuncInfo(name).ReturnTypeSemantic.ReturnTypeGen,
                Scope.GetFuncInfo(name).Parameters.Values.Select(x => x.ReturnTypeSemantic.ReturnTypeGen).ToArray( )
            );      
        }
        void CreateRecordTypes (CodeILGenerator gen, string name) {
            if (gen.Module.GetType(name) != null) return;

            TypeBuilder newType = gen.Module.DefineType(name, System.Reflection.TypeAttributes.Public);

            (Scope.GetTypeInfo(name) as RecordTypeInfo).RecordTypeBuilder = newType;
            Scope.GetTypeInfo(name).ReturnTypeGen = newType.AsType( );
        }
        void CreateArrayTypes (CodeILGenerator gen, string name) {
            var arrayTypeInfo = Scope.GetTypeInfo(name) as ArrayTypeInfo;

            if (arrayTypeInfo.ReturnTypeGen != null) return;

            if (arrayTypeInfo.ElementsType.ReturnTypeGen != null)
                Scope.GetTypeInfo(name).ReturnTypeGen = arrayTypeInfo.ElementsType.ReturnTypeGen.MakeArrayType( );
            else {
                if (arrayTypeInfo.ElementsType is RecordTypeInfo)
                    CreateRecordTypes(gen, arrayTypeInfo.ElementsType.Name);
                else
                    CreateArrayTypes(gen, arrayTypeInfo.ElementsType.Name);

                Scope.GetTypeInfo(name).ReturnTypeGen = arrayTypeInfo.ElementsType.ReturnTypeGen.MakeArrayType( );
            }
        }
    }
}
