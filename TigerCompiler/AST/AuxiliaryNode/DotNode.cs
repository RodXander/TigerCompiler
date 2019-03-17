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
    public class DotNode : AuxiliaryNode
    {
        public DotNode (IToken payload) : base(payload) { }
    }
}
