using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler
{
    public class CodeILGenerator
    {
        public CodeILGenerator ( ) {
            BreakPoints = new Stack<Label>( );
        }
        public CodeILGenerator (ILGenerator generator) { 
            BreakPoints = new Stack<Label>( );
            Generator = generator; 
        }

        public ILGenerator Generator { get; set; }
        public int NestedClassesCounter { get; set; }
        public TypeBuilder ProgramType { get; set; }
        public Stack<Label> BreakPoints { get; set; }
        public ModuleBuilder Module { get; set; }
    }
}
