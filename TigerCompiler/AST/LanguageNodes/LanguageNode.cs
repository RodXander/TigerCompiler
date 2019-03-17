using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using TigerCompiler.Semantics;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public abstract class LanguageNode : TigerASTNode
    {
        public LanguageNode (IToken payload) : base(payload) { }

        public Scope Scope { get; set; }

        public abstract void CheckSemantics (Scope scope);

        public abstract void GenerateCode (CodeILGenerator gen);
    }
}
