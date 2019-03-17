using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.Semantics
{
    public class ArrayTypeInfo : TypeInfo
    {
        public TypeInfo ElementsType { get; set; }
    }
}
