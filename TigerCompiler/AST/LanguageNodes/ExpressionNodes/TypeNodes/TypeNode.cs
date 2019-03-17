using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;

namespace TigerCompiler.AST
{
    public abstract class TypeNode : ExpressionNode
    {
        public TypeNode (IToken payload) : base (payload) { }
    }
}
