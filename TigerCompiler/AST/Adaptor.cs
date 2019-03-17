using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime.Tree;
using Antlr.Runtime;
using System.Reflection;
using TigerCompiler.AST;

namespace TigerCompiler.AST
{
    public class Adaptor : CommonTreeAdaptor
    {
        public override object Create (IToken payload) {
            if (payload == null)
                return new NilNode(payload);

            FieldInfo[] fields = typeof(TigerParser).GetFields( );
            foreach (var field in fields) {
                if (field.IsStatic && (int) field.GetRawConstantValue( ) == payload.Type) 
                {
                    string fieldName = field.Name[0].ToString();
                    for (int i = 1; i < field.Name.Length; i++) {
                        if (field.Name[i] == '_')
                            fieldName += Char.ToUpper(field.Name[++i]);
                        else
                            fieldName += Char.ToLower(field.Name[i]);

                    }
                    string typeName = string.Format("TigerCompiler.AST.{0}Node", fieldName);
                    Type type = Assembly.GetExecutingAssembly( ).GetType(typeName);
                    return Activator.CreateInstance(type, payload);
                }
            }

            return base.Create(payload);
        }
    }
}
