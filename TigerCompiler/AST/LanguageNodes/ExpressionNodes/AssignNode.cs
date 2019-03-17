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
    public class AssignNode : ExpressionNode
    {
        public AssignNode (IToken payload) : base(payload) { }

        public override void CheckSemantics (Scope scope) {
            if (GetChildAsExpression(0) is LvalueNode) {
                var possibleForVar = scope.GetVarInfo(GetChildAsExpression(0).Children[0].Text);
                if (possibleForVar != null && possibleForVar.InsideAFor) {
                    Errors.AddSemanticError(SemanticErrorType.InvalidForAssigment, node: this);
                    return;
                }
            }

            base.CheckSemantics(scope);
            ReturnType = TypesResources.NoReturn;

            string typeOfValueRight = GetChildAsExpression(1).ReturnType;
            string typeOfValueLeft = GetChildAsExpression(0).ReturnType;

            if (typeOfValueRight == TypesResources.NoReturn)
                Errors.AddSemanticError(SemanticErrorType.NoReturnValue, node: GetChildAsExpression(1));
            else if (typeOfValueRight != null && typeOfValueLeft != null)
                if (typeOfValueLeft != typeOfValueRight)
                    Errors.AddSemanticError(SemanticErrorType.IncompatibleTypes, typeOfValueLeft, typeOfValueRight, this);
        }

        public override void GenerateCode (CodeILGenerator gen) {
            GetChildAsExpression(1).GenerateCode(gen);
            (Children[0] as LvalueNode).Assign(gen);
        }
    }
}
