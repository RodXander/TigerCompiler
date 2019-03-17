using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime.Tree;
using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public abstract class TigerASTNode : CommonTree
    {
        public TigerASTNode (IToken payload) : base(payload) { }
    }
}
