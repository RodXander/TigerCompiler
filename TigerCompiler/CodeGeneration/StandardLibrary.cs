using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using TigerCompiler.Semantics;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace TigerCompiler.CodeGeneration
{
    public static class StandardLibrary
    {
        public static List<string> UsedFunctions = new List<string>( );

        public static void Generate (TypeBuilder type) {
            foreach (var function in Scope.StandardLibrary.Where(x => UsedFunctions.Contains(x.Name))) {
                var functionBuilder = type.DefineMethod
                (
                    function.Name, System.Reflection.MethodAttributes.Static | System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard,
                    function.ReturnTypeSemantic.ReturnTypeGen,
                    function.Parameters.Values.Select(x => x.ReturnTypeSemantic.ReturnTypeGen).ToArray( )
                );
                function.FunctionBuilder = functionBuilder;
                GenerateBodies(function.Name, functionBuilder);
            }
        }

        static void GenerateBodies (string name, MethodBuilder functionBuilder) {
            ILGenerator gen = functionBuilder.GetILGenerator( );
            switch (name) {
                case "print": {
                        PrintFunction(gen);
                        break;
                    }

                case "printi": {
                        PrintiFunction(gen);
                        break;
                    }
                case "ord": {
                        OrdFunction(gen);
                        break;
                    }
                case "chr": {
                        ChrFunction(gen);
                        break;
                    }

                case "size": {
                        SizeFunction(gen);
                        break;
                    }

                case "substring": {
                        SubstringFunction(gen);
                        break;
                    }

                case "concat": {
                        ConcatFunction(gen);
                        break;
                    }
                case "not": {
                        NotFunction(gen);
                        break;
                    }

                case "exit": {
                        ExitFunction(gen);
                        break;
                    }

                case "getline": {
                        GetLineFunction(gen);
                        break;
                    }

                case "printline": {
                        PrintLineFunction(gen);
                        break;
                    }
                case "printiline": {
                    PrintiLineFunction(gen);
                    break;
                    }
            }
        }

        #region Function Bodies
        static void PrintFunction (ILGenerator gen) {
            MethodInfo writeMethod = typeof(Console).GetMethod("Write", new[] { typeof(string) });
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, writeMethod);
            gen.Emit(OpCodes.Ret);
        }

        static void PrintiFunction (ILGenerator gen) {
            MethodInfo writeMethod = typeof(Console).GetMethod("Write", new[] { typeof(int) });
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, writeMethod);
            gen.Emit(OpCodes.Ret);
        }

        static void OrdFunction (ILGenerator gen) {
            var emptyStringLabel = gen.DefineLabel( );
            var endLabel = gen.DefineLabel( );

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldstr, "");
            gen.Emit(OpCodes.Beq, emptyStringLabel);

            MethodInfo getCharsMethod = typeof(string).GetMethod("get_Chars", new[] { typeof(int) });
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Call, getCharsMethod);

            MethodInfo toInt32Method = typeof(Convert).GetMethod("ToInt32", new[] { typeof(char) });
            gen.Emit(OpCodes.Call, toInt32Method);
            gen.Emit(OpCodes.Br, endLabel);

            gen.MarkLabel(emptyStringLabel);
            gen.Emit(OpCodes.Ldc_I4_M1);

            gen.MarkLabel(endLabel);
            gen.Emit(OpCodes.Ret);
        }

        static void ChrFunction (ILGenerator gen) {
            gen.Emit(OpCodes.Ldarg_0);
            var convertFromUtf32Method = typeof(Char).GetMethod("ConvertFromUtf32");
            gen.Emit(OpCodes.Call, convertFromUtf32Method);
            gen.Emit(OpCodes.Ret);
        }

        static void SizeFunction (ILGenerator gen) {
            gen.Emit(OpCodes.Ldarg_0);
            var lengthMethod = typeof(string).GetMethod("get_Length");
            gen.Emit(OpCodes.Call, lengthMethod);
            gen.Emit(OpCodes.Ret);
        }

        static void SubstringFunction (ILGenerator gen) {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            var substringMethod = typeof(string).GetMethod("Substring", new[] { typeof(int), typeof(int) });
            gen.Emit(OpCodes.Call, substringMethod);
            gen.Emit(OpCodes.Ret);
        }

        static void ConcatFunction (ILGenerator gen) {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            gen.Emit(OpCodes.Call, concatMethod);
            gen.Emit(OpCodes.Ret);
        }

        static void NotFunction (ILGenerator gen) {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Ret);
        }

        static void ExitFunction (ILGenerator gen) {
            gen.Emit(OpCodes.Ldarg_0);
            var exitMethod = typeof(Environment).GetMethod("Exit", new[] { typeof(int) });
            gen.Emit(OpCodes.Call, exitMethod);
            gen.Emit(OpCodes.Ret);
        }

        static void GetLineFunction (ILGenerator gen) {
            var readLineMethod = typeof(Console).GetMethod("ReadLine");
            gen.Emit(OpCodes.Call, readLineMethod);
            gen.Emit(OpCodes.Ret);
        }

        static void PrintLineFunction (ILGenerator gen) {
            var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, writeLineMethod);
            gen.Emit(OpCodes.Ret);
        }

        static void PrintiLineFunction (ILGenerator gen) {
            var writeLineMethod = typeof(Console).GetMethod("WriteLine", new[] { typeof(int) });
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Call, writeLineMethod);
            gen.Emit(OpCodes.Ret);
        }
        #endregion

    }
}
