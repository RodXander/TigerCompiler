using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.Semantics
{
    public class VarFuncInfo : ItemInfo
    {
        public TypeInfo ReturnTypeSemantic { get; set; }
    }
}
