using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.ErrorHandling;
using TigerCompiler.Semantics;
using System.Reflection;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class RecordNode : TypeNode
    {
        public RecordNode (IToken payload) : base (payload) { }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);

            var recordTypeName = Children[0].Text;
            var typeRecordInfo = scope.GetTypeInfo(recordTypeName);
            if (typeRecordInfo != null)
                if (!(typeRecordInfo is RecordTypeInfo))
                    Errors.AddSemanticError(SemanticErrorType.TypeNoRecordable, recordTypeName, node: this);
                else
                    ReturnType = recordTypeName;
            else
                Errors.AddSemanticError(SemanticErrorType.TypeDoesNotExist, recordTypeName, node: this);

            if (typeRecordInfo is RecordTypeInfo) {
                if ((typeRecordInfo as RecordTypeInfo).Members.Count != (ChildCount - 1) / 2) {
                    Errors.AddSemanticError(SemanticErrorType.InvalidMembersNumber, ((ChildCount - 1) / 2).ToString( ), (typeRecordInfo as RecordTypeInfo).Members.Count.ToString( ), this);
                    return;
                }

                for (int i = 1, j = 0; i < ChildCount; i += 2, j++) {
                    if ((typeRecordInfo as RecordTypeInfo).Members[j].Item1 != Children[i].Text)
                        Errors.AddSemanticError(SemanticErrorType.RecordMemberDoesNotExist, recordTypeName, Children[i].Text, GetChildAsExpression(i));

                    else if ((Children[i + 1] as ExpressionNode).ReturnType != null) {
                        if ((Children[i + 1] as ExpressionNode).ReturnType == TypesResources.Nil) {
                            if ((typeRecordInfo as RecordTypeInfo).Members[j].Item2.Name == TypesResources.Int)
                                Errors.AddSemanticError(SemanticErrorType.InvalidNilOperation, node: GetChildAsExpression(i + 1));
                        }
                    }
                    else if ((typeRecordInfo as RecordTypeInfo).Members[j].Item2.Name != GetChildAsExpression(i + 1).ReturnType)
                        Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, (typeRecordInfo as RecordTypeInfo).Members[j].Item2.Name, GetChildAsExpression(i + 1).ReturnType, GetChildAsExpression(i + 1));
                }
            }
        }

        public override void GenerateCode (CodeILGenerator gen) {
            var recordInfo = (RecordTypeInfo) Scope.GetTypeInfo(Children[0].Text);
            var localVar = gen.Generator.DeclareLocal(recordInfo.ReturnTypeGen);

            gen.Generator.Emit(OpCodes.Newobj, recordInfo.ReturnTypeGen.GetConstructor(System.Type.EmptyTypes));
            gen.Generator.Emit(OpCodes.Stloc, localVar);

            for (int i = 2, j = 0; i < ChildCount; i += 2, j++)
			{
			    gen.Generator.Emit(OpCodes.Ldloc, localVar);
                GetChildAsExpression(i).GenerateCode(gen);
                gen.Generator.Emit(OpCodes.Stfld, recordInfo.ReturnTypeGen.GetField(recordInfo.Members[j].Item1));
			}
            gen.Generator.Emit(OpCodes.Ldloc, localVar);
        }
    }
}
