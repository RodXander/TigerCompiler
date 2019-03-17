using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.ErrorHandling;

namespace TigerCompiler.AST
{
    public class ArrayIndexNode : AuxiliaryNode
    {
        public ArrayIndexNode (IToken payload) : base(payload) { }
    }
}
