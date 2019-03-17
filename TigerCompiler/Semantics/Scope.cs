using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.AST;

namespace TigerCompiler.Semantics
{
    public class Scope
    {
        public Scope PreviousScope { get; set; }

        public Scope ( ) 
        {
            /***** Inicializando los tipos y funciones basicas *****/
            #region Built-in types
            TypeScope.Add(TypesResources.Int, new BuiltInTypeInfo { Name = TypesResources.Int, ReturnTypeGen = typeof(int) });
            TypeScope.Add(TypesResources.String, new BuiltInTypeInfo { Name = TypesResources.String, ReturnTypeGen = typeof(string) });
            TypeScope.Add(TypesResources.Nil, new BuiltInTypeInfo { Name = TypesResources.String });
            TypeScope.Add(TypesResources.NoReturn, new BuiltInTypeInfo { Name = TypesResources.NoReturn, ReturnTypeGen = typeof(void) });
            #endregion

            #region Standard Library
            var parameters = new SortedDictionary<int, VarInfo>( );
            parameters.Add(1, new VarInfo { Name = "s", ReturnTypeSemantic = TypeScope[TypesResources.String] });
            VarFuncScope.Add("print", new FuncInfo { Name = "print", ReturnTypeSemantic = TypeScope[TypesResources.NoReturn], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "i", ReturnTypeSemantic = TypeScope[TypesResources.Int] });
            VarFuncScope.Add("printi", new FuncInfo { Name = "printi", ReturnTypeSemantic = TypeScope[TypesResources.NoReturn], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "s", ReturnTypeSemantic = TypeScope[TypesResources.String] });
            VarFuncScope.Add("ord", new FuncInfo { Name = "ord", ReturnTypeSemantic = TypeScope[TypesResources.Int], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "i", ReturnTypeSemantic = TypeScope[TypesResources.Int] });
            VarFuncScope.Add("chr", new FuncInfo { Name = "chr", ReturnTypeSemantic = TypeScope[TypesResources.String], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "s", ReturnTypeSemantic = TypeScope[TypesResources.String] });
            VarFuncScope.Add("size", new FuncInfo { Name = "size", ReturnTypeSemantic = TypeScope[TypesResources.Int], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "s", ReturnTypeSemantic = TypeScope[TypesResources.String] });
            parameters.Add(2, new VarInfo { Name = "f", ReturnTypeSemantic = TypeScope[TypesResources.Int] });
            parameters.Add(3, new VarInfo { Name = "n", ReturnTypeSemantic = TypeScope[TypesResources.Int] });
            VarFuncScope.Add("substring", new FuncInfo { Name = "substring", ReturnTypeSemantic = TypeScope[TypesResources.String], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "s1", ReturnTypeSemantic = TypeScope[TypesResources.String] });
            parameters.Add(2, new VarInfo { Name = "s2", ReturnTypeSemantic = TypeScope[TypesResources.String] });
            VarFuncScope.Add("concat", new FuncInfo { Name = "concat", ReturnTypeSemantic = TypeScope[TypesResources.String], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "i", ReturnTypeSemantic = TypeScope[TypesResources.Int] });
            VarFuncScope.Add("not", new FuncInfo { Name = "not", ReturnTypeSemantic = TypeScope[TypesResources.Int], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "i", ReturnTypeSemantic = TypeScope[TypesResources.Int] });
            VarFuncScope.Add("exit", new FuncInfo { Name = "exit", ReturnTypeSemantic = TypeScope[TypesResources.NoReturn], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "s", ReturnTypeSemantic = TypeScope[TypesResources.String] });
            VarFuncScope.Add("printline", new FuncInfo { Name = "printline", ReturnTypeSemantic = TypeScope[TypesResources.NoReturn], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            parameters.Add(1, new VarInfo { Name = "i", ReturnTypeSemantic = TypeScope[TypesResources.Int] });
            VarFuncScope.Add("printiline", new FuncInfo { Name = "printiline", ReturnTypeSemantic = TypeScope[TypesResources.NoReturn], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            parameters.Clear( );
            VarFuncScope.Add("getline", new FuncInfo { Name = "getline", ReturnTypeSemantic = TypeScope[TypesResources.String], Parameters = new SortedDictionary<int, VarInfo>(parameters) });

            StandardLibrary = new List<FuncInfo>(VarFuncScope.Values.Cast<FuncInfo>( ));
            #endregion
        }
        public Scope (Scope scope) { PreviousScope = scope; }

        public Dictionary<string, TypeInfo> TypeScope = new Dictionary<string, TypeInfo>( );
        public Dictionary<string, VarFuncInfo> VarFuncScope = new Dictionary<string, VarFuncInfo>( );
        public static List<FuncInfo> StandardLibrary;

        public static bool IsStandardFunction (string name) { return StandardLibrary.Select(x => x.Name).Contains(name); }

        public VarInfo GetVarInfo (string variable, bool shallow = false, int callerVariableCounter = int.MaxValue) {
            var possibleVar = new VarFuncInfo( );

            if (VarFuncScope.TryGetValue(variable, out possibleVar) && possibleVar is VarInfo) return possibleVar as VarInfo;
            else if (PreviousScope != null && !shallow) return PreviousScope.GetVarInfo(variable);
            else return null;
        }
        public FuncInfo GetFuncInfo (string function, bool shallow = false) {
            var possibleFunc = new VarFuncInfo( );
            
            if (VarFuncScope.TryGetValue(function, out possibleFunc) && possibleFunc is FuncInfo) return possibleFunc as FuncInfo;
            else if (PreviousScope != null && !shallow) return PreviousScope.GetFuncInfo(function);
            else return null;
        }
        public TypeInfo GetTypeInfo (string type, bool shallow = false) {
            var possibleType = new TypeInfo( );
            
            if (TypeScope.TryGetValue(type, out possibleType)) 
                return possibleType is RecordTypeInfo 
                     ? possibleType as RecordTypeInfo 
                     : possibleType is ArrayTypeInfo 
                     ? possibleType as ArrayTypeInfo 
                     : possibleType as TypeInfo;
            else if (PreviousScope != null && !shallow) return PreviousScope.GetTypeInfo(type);
            else return null;
        }
    }
}
