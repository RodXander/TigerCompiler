using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr.Runtime;
using TigerCompiler.ErrorHandling;
using TigerCompiler.Semantics;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    public class StringNode : TypeNode
    {
        public StringNode (IToken payload) : base(payload) { }

        public string RealString { get; set; }

        private string MatchProcessor (Match match) {
            switch (match.Value) {
                case @"\""":
                    return "\"";
                case @"\\":
                    return "\\";
                case @"\n":
                    return "\n";
                case @"\t":
                    return "\t";
                case @"\r":
                    return "\r";
                default: {
                        int number = 0;
                        if (Int32.TryParse(match.Groups[1].Value, out number))
                            if (number >= 32 && number <= 126)
                                return Char.ConvertFromUtf32(number);
                            else {
                                (this as StringNode).Token.CharPositionInLine += match.Groups[1].Index;
                                Errors.AddSemanticError(SemanticErrorType.InvalidScapeSequence, node: this);
                                ReturnType = null;
                            }
                        return "";
                    }
            }
        }

        public override void CheckSemantics (Scope scope) {
            ReturnType = TypesResources.String;
            
            string str = (this as StringNode).Text; str = str.Substring(1, str.Length - 2);
            foreach (var character in str)
                if ((int) character > 126)
                    Errors.AddSemanticError(SemanticErrorType.NonASCIIChar, node: this);

            string pattern = @"\\(""|\\|[ntr]|[0-9]{3}|\s*\\)";
            RealString = Regex.Replace(str, pattern, MatchProcessor);
        }

        public override void GenerateCode (CodeILGenerator gen) {
            gen.Generator.Emit(OpCodes.Ldstr, RealString);
        }
    }
}
