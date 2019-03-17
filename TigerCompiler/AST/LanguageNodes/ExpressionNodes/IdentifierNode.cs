using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.Semantics;
using TigerCompiler.ErrorHandling;

namespace TigerCompiler.AST
{
    public class IdentifierNode : ExpressionNode
    {
        public IdentifierNode (IToken payload) : base (payload) { }

        public override void CheckSemantics (Scope scope) { }

        public override void GenerateCode (CodeILGenerator gen) {
            throw new NotSupportedException( );
        }
    }
}
