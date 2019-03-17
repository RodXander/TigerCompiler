using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.Semantics
{
    public class FuncInfo : VarFuncInfo
    {
        public SortedDictionary<int, VarInfo> Parameters { get; set; }
        public MethodBuilder FunctionBuilder { get; set; }
    }
}
