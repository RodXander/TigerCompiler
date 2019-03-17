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
    public class TypeDeclarationNode : DeclarationNode
    {
        public TypeDeclarationNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            if (scope.GetTypeInfo(TypeName) != null) {
                Errors.AddSemanticError(SemanticErrorType.IdentifierAlreadyExist, TypeName, node: this);
                return;
            }
            WellFormed = true;
            scope.TypeScope.Add
            (
                TypeName,
                Children[1] is RecordTypeDeclarationNode
                ? new RecordTypeInfo { Name = TypeName }
                : Children[1] is ArrayTypeDeclarationNode
                ? new ArrayTypeInfo { Name = TypeName }
                : new TypeInfo { Name = TypeName }
            );
        }

        public override void PostCheckSemantics (Scope scope) {
            if (Children[1] is RecordTypeDeclarationNode) {
                var typeInfo = scope.GetTypeInfo(TypeName);

                int membersDeclarationCounter = 2, membersCounter = 0;
                while (membersDeclarationCounter < ChildCount && Children[membersDeclarationCounter++] is TypeDeclarationFieldNode) {

                    string memberName = Children[membersDeclarationCounter++].Text;
                    if ((typeInfo as RecordTypeInfo).Members.Values.Any(t => t.Item1 == memberName)) {
                        Errors.AddSemanticError(SemanticErrorType.IdentifierAlreadyExist, memberName, node: Children[membersDeclarationCounter - 1] as TigerASTNode);
                        return;
                    }
                    string memberType = Children[membersDeclarationCounter++].Text;
                    var memberTypeInfo = scope.GetTypeInfo(memberType);
                    if (memberTypeInfo == null) {
                        Errors.AddSemanticError(SemanticErrorType.TypeDoesNotExist, memberType, node: Children[membersDeclarationCounter - 1] as TigerASTNode);
                        return;
                    }
                    (typeInfo as RecordTypeInfo).Members.Add(membersCounter++, new Tuple<string, TypeInfo>(memberName, memberTypeInfo));
                }
            }
            else
            {
                if (Children[1] is AliasTypeDeclarationNode) {
                    var assignType = scope.GetTypeInfo(Children[2].Text);
                    if (assignType == null) {
                        Errors.AddSemanticError(SemanticErrorType.TypeDoesNotExist, Children[2].Text, node: Children[1] as TigerASTNode);
                        return;
                    }
                
                    (Parent as LetNode).Dependencies.Add
                    (
                        TypeName, 
                        (Parent as LetNode).Dependencies.ContainsKey(assignType.Name) ? new List<string>((Parent as LetNode).Dependencies[assignType.Name]) : new List<string>( )
                    );
                    (Parent as LetNode).Dependencies[TypeName].Add(assignType.Name);

                    var list = scope.TypeScope.Where(p => p.Value == scope.TypeScope[TypeName]).Select(p => p.Key).ToArray( );
                    for (int i = 0; i < list.Count(); i++)
                        scope.TypeScope[list[i]] = assignType;       
                        
                }
                else {
                    var typeInfo = scope.GetTypeInfo(TypeName);
                    var elementsType = scope.GetTypeInfo(Children[2].Text);
                    if (elementsType == null) {
                        Errors.AddSemanticError(SemanticErrorType.TypeDoesNotExist, Children[2].Text, node: Children[1] as TigerASTNode);
                        return;
                    }

                    (Parent as LetNode).Dependencies.Add
                    (
                        TypeName,
                        (Parent as LetNode).Dependencies.ContainsKey(elementsType.Name) ? new List<string>((Parent as LetNode).Dependencies[elementsType.Name]) : new List<string>( )
                    );
                    (Parent as LetNode).Dependencies[TypeName].Add(elementsType.Name);
                
                    (typeInfo as ArrayTypeInfo).ElementsType = elementsType;
                }

                foreach (var dependency in (Parent as LetNode).Dependencies[TypeName]) {
                    if ((Parent as LetNode).Dependencies.ContainsKey(dependency) &&
                        (Parent as LetNode).Dependencies[dependency].Contains(TypeName)) {
                        Errors.AddSemanticError(SemanticErrorType.CyclicDeclaration, TypeName, dependency, this);
                        break;
                    }
                }

            }
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var recordBuilder = (Scope.GetTypeInfo(TypeName) as RecordTypeInfo).RecordTypeBuilder;

            if (Children[1] is RecordTypeDeclarationNode) {
                foreach (var member in (Scope.GetTypeInfo(TypeName) as RecordTypeInfo).Members.Values)
                    recordBuilder.DefineField
                    (
                        member.Item1, member.Item2.ReturnTypeGen,
                        System.Reflection.FieldAttributes.Public
                    );

                recordBuilder.CreateType( );
                Scope.GetTypeInfo(TypeName).ReturnTypeGen = recordBuilder.AsType( );
            }
        }
    }
}