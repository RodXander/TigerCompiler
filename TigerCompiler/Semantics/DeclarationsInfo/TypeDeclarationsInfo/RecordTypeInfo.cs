using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.Semantics
{
    public class RecordTypeInfo : TypeInfo
    {
        SortedDictionary<int, Tuple<string, TypeInfo>> _members = new SortedDictionary<int, Tuple<string, TypeInfo>>( );
        public SortedDictionary<int, Tuple<string, TypeInfo>> Members { get { return _members; } }

        public TypeBuilder RecordTypeBuilder { get; set; }
    }
}
