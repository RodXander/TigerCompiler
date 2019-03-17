using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    public abstract class DeclarationNode : LanguageNode
    {
        public DeclarationNode (IToken payload) : base(payload) { }

        public bool WellFormed { get; set; }
        public string TypeName { get { return Children[0].Text; } }

        public override void CheckSemantics (Scope scope) { Scope = scope; }
        public abstract void PostCheckSemantics (Scope scope);
    }
}
