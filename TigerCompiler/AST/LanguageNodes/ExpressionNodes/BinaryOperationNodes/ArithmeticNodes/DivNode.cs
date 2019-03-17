using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class DivNode : ArithmeticNode
    {
        public DivNode (IToken payload) : base(payload) { }

        public override OpCode OperationOpCode { get { return OpCodes.Div; } }
    }
}
