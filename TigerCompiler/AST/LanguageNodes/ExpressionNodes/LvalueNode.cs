using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.ErrorHandling;
using TigerCompiler.Semantics;
using System.Reflection.Emit;
using System.Reflection;

namespace TigerCompiler.AST
{
    public class LvalueNode : ExpressionNode
    {
        public LvalueNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            var identifierName = Children[0].Text;
            var initialIdentifier = scope.GetVarInfo(identifierName);
            if (initialIdentifier == null) {
                Errors.AddSemanticError(SemanticErrorType.UndefinedIdentifier, identifierName, node: GetChildAsExpression(0));
                return;
            }

            var lastTypeInfo = initialIdentifier.ReturnTypeSemantic;
            for (int i = 1; i < ChildCount; i++) {
                if (Children[i] is DotNode) {
                    if (lastTypeInfo is RecordTypeInfo) {
                        i++;
                        var newType = (lastTypeInfo as RecordTypeInfo).Members.Values.FirstOrDefault(t => t.Item1 == Children[i].Text);
                        if (newType != null) {
                            lastTypeInfo = newType.Item2;
                            continue;
                        }
                        else Errors.AddSemanticError(SemanticErrorType.InvalidDotAccess, lastTypeInfo.Name, Children[i].Text, GetChildAsExpression(i));
                    }
                    else Errors.AddSemanticError(SemanticErrorType.TypeNoRecordable, lastTypeInfo.Name, node: GetChildAsExpression(i - 1));
                }
                else {
                    if (lastTypeInfo is ArrayTypeInfo) {
                        i++;
                        var indexExprReturnType = GetChildAsExpression(i).ReturnType;
                        if (indexExprReturnType == TypesResources.Int) {
                            lastTypeInfo = (lastTypeInfo as ArrayTypeInfo).ElementsType;
                            continue;
                        }
                        else Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, indexExprReturnType, TypesResources.Int, GetChildAsExpression(i));
                    }
                    else Errors.AddSemanticError(SemanticErrorType.TypeNoIndexable, lastTypeInfo.Name, node: GetChildAsExpression(i - 1));
                }
                return;
            }
            ReturnType = lastTypeInfo.Name;
        }

        public override void GenerateCode (CodeILGenerator gen) {
            if ((Scope.GetVarInfo(Children[0].Text) as VarInfo).InsideAFor) {
                gen.Generator.Emit(OpCodes.Ldloc, (Scope.GetVarInfo(Children[0].Text) as VarInfo).VarLocalBuilder);
                return;
            }

            object kindOfVar;
            if ((Scope.GetVarInfo(Children[0].Text) as VarInfo).InsideAFunction)
                kindOfVar = (Scope.GetVarInfo(Children[0].Text) as VarInfo).VarLocalBuilder;
            else
                kindOfVar = (Scope.GetVarInfo(Children[0].Text) as VarInfo).VarFieldBuilder;

            if (kindOfVar is FieldBuilder)
                gen.Generator.Emit(OpCodes.Ldsfld, kindOfVar as FieldBuilder);
            else
                gen.Generator.Emit(OpCodes.Ldloc, kindOfVar as LocalBuilder);

            var lastType = Scope.GetVarInfo(Children[0].Text).ReturnTypeSemantic;
            for (int i = 1; i < ChildCount; i += 2) {
                if (Children[i] is DotNode) {
                    gen.Generator.Emit
                    (
                        OpCodes.Ldfld,
                        (lastType as RecordTypeInfo).ReturnTypeGen.GetField(Children[i + 1].Text) as FieldInfo
                    );

                    lastType = (lastType as RecordTypeInfo).Members.Values.First(x => x.Item1 == Children[i + 1].Text).Item2;
                }
                else {
                    GetChildAsExpression(i + 1).GenerateCode(gen);
                    
                    gen.Generator.Emit(OpCodes.Ldelem, (lastType as ArrayTypeInfo).ElementsType.ReturnTypeGen);
                    
                    lastType = (lastType as ArrayTypeInfo).ElementsType;
                }
            }
        }

        public void Assign (CodeILGenerator gen) {
            var firstEntry = Scope.GetVarInfo(Children[0].Text).VarFieldBuilder;

            if (ChildCount == 1) gen.Generator.Emit(OpCodes.Stsfld, firstEntry as FieldInfo);

            else {
                var lastType = Scope.GetVarInfo(Children[0].Text).ReturnTypeSemantic;

                var exprToAssign = gen.Generator.DeclareLocal(Scope.GetTypeInfo((Parent as ExpressionNode).GetChildAsExpression(1).ReturnType).ReturnTypeGen);
                gen.Generator.Emit(OpCodes.Stloc, exprToAssign);
                gen.Generator.Emit(OpCodes.Ldsfld, firstEntry as FieldInfo);

                for (int i = 1; i < ChildCount - 2; i += 2) {
                    if (Children[i] is DotNode) {
                        gen.Generator.Emit
                        (
                            OpCodes.Ldfld,
                            (lastType as RecordTypeInfo).ReturnTypeGen.GetField(Children[i + 1].Text)
                        );
                        lastType = (lastType as RecordTypeInfo).Members.Values.First(x => x.Item1 == Children[i + 1].Text).Item2;
                    }
                    else {
                        GetChildAsExpression(i + 1).GenerateCode(gen);
                        gen.Generator.Emit(OpCodes.Ldelem, (lastType as ArrayTypeInfo).ElementsType.ReturnTypeGen);

                        lastType = (lastType as ArrayTypeInfo).ElementsType;
                    }
                }

                if (Children[ChildCount - 2] is DotNode) {
                    gen.Generator.Emit(OpCodes.Ldloc, exprToAssign);
                    gen.Generator.Emit
                    (
                        OpCodes.Stfld,
                        (lastType as RecordTypeInfo).ReturnTypeGen.GetField(Children[ChildCount - 1].Text)
                    );
                }
                else {
                    GetChildAsExpression(ChildCount - 1).GenerateCode(gen);
                    gen.Generator.Emit(OpCodes.Ldloc, exprToAssign);
                    gen.Generator.Emit(OpCodes.Stelem, (lastType as ArrayTypeInfo).ElementsType.ReturnTypeGen);
                }
            }
        }
    }
}
