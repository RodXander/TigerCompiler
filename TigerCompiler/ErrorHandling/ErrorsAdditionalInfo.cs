using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.ErrorHandling
{
    public static class ErrorsAdditionalInfo
    {
        internal static Dictionary<string, string> TokenTexts = new Dictionary<string, string> ();
        static  ErrorsAdditionalInfo ()
        {
            TokenTexts.Add ("COMMA", ","); TokenTexts.Add ("COLON", ":"); TokenTexts.Add ("SEMICOLON", ";");
            TokenTexts.Add ("LPARENTHESIS", "("); TokenTexts.Add ("RPARENTHESIS", ")"); TokenTexts.Add ("LBRACKET", "[");
            TokenTexts.Add ("RBRACKET", "]"); TokenTexts.Add ("LBRACE", "{"); TokenTexts.Add ("RBRACE", "}");
            TokenTexts.Add ("DOT", "."); TokenTexts.Add ("PLUS", "+"); TokenTexts.Add ("MINUS", "-");
            TokenTexts.Add ("MULT", "*"); TokenTexts.Add ("DIV", "/"); TokenTexts.Add ("EQUAL", "=");
            TokenTexts.Add ("NOT_EQUAL", "<>"); TokenTexts.Add ("LESS_THAN", "<"); TokenTexts.Add ("LESS_THAN_EQUAL", "<=");
            TokenTexts.Add ("MORE_THAN", ">"); TokenTexts.Add ("MORE_THAN_EQUAL", ">="); TokenTexts.Add ("AND", "&");
            TokenTexts.Add ("OR", "|"); TokenTexts.Add ("ASSIGN", ":="); TokenTexts.Add ("ARRAY", "array");
            TokenTexts.Add ("BREAK", "break"); TokenTexts.Add ("ELSE", "else"); TokenTexts.Add ("END", "end");
            TokenTexts.Add ("FOR", "for"); TokenTexts.Add ("DO", "do"); TokenTexts.Add ("FUNCTION", "function");
            TokenTexts.Add ("IF", "if"); TokenTexts.Add ("IN", "in"); TokenTexts.Add ("LET", "let");
            TokenTexts.Add ("NIL", "nil"); TokenTexts.Add ("OF", "of"); TokenTexts.Add ("THEN", "then");
            TokenTexts.Add ("TO", "to"); TokenTexts.Add ("TYPE", "type"); TokenTexts.Add ("VAR", "var");
            TokenTexts.Add ("WHILE", "while");
        }
    }
}
