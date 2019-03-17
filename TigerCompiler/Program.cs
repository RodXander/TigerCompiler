using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using TigerCompiler.AST;
using TigerCompiler.Semantics;
using TigerCompiler.ErrorHandling;
using TigerCompiler.CodeGeneration;
using System.IO;
using System.Reflection.Emit;

namespace TigerCompiler
{
    class Program
    {
        static void Main (string[] args) {
            InitTigerConsole( );

            ANTLRFileStream input;
            string address = string.Empty;
            while (true)
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("\nArrastre el archivo a compilar o escriba su dirección...\n\n");
                    address = Console.ReadLine();
                    address = address[0] == '"' ? address.Substring(1, address.Length - 2) : address;
                }
                else
                {
                    address = args[0];
                    args = new string[0];
                }

                try
                {
                    input = new ANTLRFileStream(address);
                }
                catch (Exception)
                {
                    Console.WriteLine("\nDirección inválida...");
                    continue;
                }

                TigerLexer lexer = new TigerLexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                TigerParser parser = new TigerParser(tokens);

                parser.TreeAdaptor = new Adaptor();
                var result = parser.program();

                if (Errors.Count == 0) (result.Tree as LanguageNode).CheckSemantics(new Scope());
                if (Errors.Count == 0) GenerateCode(result.Tree as LanguageNode, Path.ChangeExtension(address, ".exe"));

                if (Errors.Count != 0)
                {
                    Environment.ExitCode = 1;
                    Console.WriteLine("\nHan ocurrido algunos errores");
                    Console.WriteLine(Errors.Print());
                }
                else Console.WriteLine("\nCompilación satisfactoria");
                // Descomentar la siguiente linea cuando se usa el TigerTester
                Console.ReadLine();
            }
        }

        static void InitTigerConsole ( ) {
            Console.Title = "Tiger Compiler";
            Console.WriteLine("Tiger Compiler {0}", Assembly.GetEntryAssembly( ).GetName( ).Version);
            Console.WriteLine("Copyright (c) Osvaldo Saez Lombira\n");
        }

        static void GenerateCode (LanguageNode root, string outputPath) {
            CodeILGenerator gen = new CodeILGenerator( );

            string name = Path.GetFileNameWithoutExtension(outputPath);
            string filename = Path.GetFileName(outputPath);
            AssemblyName assemblyName = new AssemblyName(name);
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
                                                                                     AssemblyBuilderAccess.RunAndSave,
                                                                                     Path.GetDirectoryName(outputPath));
            
            ModuleBuilder moduleBuilder = assembly.DefineDynamicModule(name, filename);
            gen.Module = moduleBuilder;
            TypeBuilder programType = moduleBuilder.DefineType("Program0");
            gen.ProgramType = programType;

            StandardLibrary.Generate(programType);

            MethodBuilder mainMethod = programType.DefineMethod("Main0", MethodAttributes.Static, typeof(int), System.Type.EmptyTypes);
            assembly.SetEntryPoint(mainMethod);
            gen.Generator =  mainMethod.GetILGenerator( );

            var res = gen.Generator.DeclareLocal(typeof(int));

            gen.Generator.Emit(OpCodes.Ldc_I4_0);
            gen.Generator.Emit(OpCodes.Stloc, res);

            gen.Generator.BeginExceptionBlock( );

            root.GenerateCode(gen);
            if ((root.Children[0] as ExpressionNode).ReturnType != TypesResources.NoReturn)
                gen.Generator.Emit(OpCodes.Pop);

            gen.Generator.BeginCatchBlock(typeof(Exception));
            PropertyInfo message = typeof(Exception).GetProperty("Message");
            MethodInfo getMessage = message.GetGetMethod( );
            gen.Generator.Emit(OpCodes.Callvirt, getMessage);
            MethodInfo writeLine = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });
            gen.Generator.Emit(OpCodes.Call, writeLine);
            MethodInfo readLine = typeof(Console).GetMethod("ReadLine");
            gen.Generator.Emit(OpCodes.Call, readLine);
            gen.Generator.Emit(OpCodes.Pop);

            gen.Generator.Emit(OpCodes.Ldc_I4_1);
            gen.Generator.Emit(OpCodes.Stloc, res);

            gen.Generator.EndExceptionBlock( );

            gen.Generator.Emit(OpCodes.Ldloc, res);
            gen.Generator.Emit(OpCodes.Ret);

            programType.CreateType( );
            moduleBuilder.CreateGlobalFunctions( );
            assembly.Save(filename);
        }
    }
}
