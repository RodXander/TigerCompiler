using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using System.Reflection.Emit;
using TigerCompiler.ErrorHandling;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    public class EqualNode : EqualityNodes
    {
        public EqualNode (IToken payload) : base(payload) { }

        public override OpCode OperationOpCode { get { return OpCodes.Ceq; } }

        public override void CheckSemantics (Scope scope) {
            base.CheckSemantics(scope);
        }
    }
}
