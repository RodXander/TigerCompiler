using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class PlusNode : ArithmeticNode
    {
        public PlusNode (IToken payload) : base(payload) { }

        public override OpCode OperationOpCode { get { return OpCodes.Add; } }
    }
}
