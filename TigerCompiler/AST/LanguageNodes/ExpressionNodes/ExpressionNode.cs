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
    public class ExpressionNode : LanguageNode
    {
        public ExpressionNode (IToken payload) : base(payload) { }

        public string ReturnType { get; set; }
        public ExpressionNode GetChildAsExpression (int i) { return Children[i] as ExpressionNode; }

        public override void CheckSemantics (Scope scope) {
            Scope = scope;
            for (int i = 0; i < ChildCount; i++) {
                if (Children[i] is ExpressionNode) {
                    var newScope = new Scope(scope);
                    GetChildAsExpression(i).Scope = newScope;
                    GetChildAsExpression(i).CheckSemantics(newScope);
                }
            }

            for (int i = 0; i < ChildCount; i++)
                if (Children[i] is ExpressionNode) {
                    string currentChildReturnType = GetChildAsExpression(i).ReturnType;
                    if (currentChildReturnType != null)
                        if (scope.GetTypeInfo(currentChildReturnType) == null)
                            Errors.AddSemanticError(SemanticErrorType.ReturnTypeNoVisible, currentChildReturnType, node: GetChildAsExpression(i));
                }
        }

        public override void GenerateCode (CodeILGenerator gen) {
            GetChildAsExpression(0).GenerateCode(gen);
        }
    }
}
