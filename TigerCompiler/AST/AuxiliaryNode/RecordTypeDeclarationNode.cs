using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;

namespace TigerCompiler.AST
{
    public class RecordTypeDeclarationNode : AuxiliaryNode
    {
        public RecordTypeDeclarationNode (IToken payload) : base(payload) { }
    }
}