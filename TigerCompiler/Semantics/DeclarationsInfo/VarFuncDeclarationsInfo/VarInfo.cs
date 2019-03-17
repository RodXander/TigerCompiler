using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.Semantics
{
    public class VarInfo : VarFuncInfo
    {
        public bool InsideAFor { get; set; }
        public bool InsideAFunction { get; set; }
        public FieldBuilder VarFieldBuilder { get; set; }
        public LocalBuilder VarLocalBuilder { get; set; }
    }
}
