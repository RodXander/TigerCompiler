using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.AST;
using Antlr.Runtime;
using TigerCompiler.Semantics;

namespace TigerCompiler.ErrorHandling
{
    public static class Errors
    {   
        static List<string> _errors = new List<string> ();

        public static void AddSintacticError (RecognitionException exc, string[] tokenNames) {
            _errors.Add (String.Format ("({0}, {1}): {2}", exc.Line, exc.CharPositionInLine, GetSintacicErrorMessage(exc, tokenNames)));
        }
        public static void AddSemanticError (SemanticErrorType error, string extraInfoOne = "", string extraInfoTwo = "", TigerASTNode node = null) {
            _errors.Add(String.Format("({0}, {1}): {2}", node.Token.Line, node.Token.CharPositionInLine, GetSemanticErrorMessage(error, extraInfoOne, extraInfoTwo, node)));
        }
        
        public static int Count { get { return _errors.Count; } }

        static string GetSintacicErrorMessage (RecognitionException exc, string[] tokenNames) {
            if (exc is UnwantedTokenException)
                return String.Format("Token inesperado, '{0}'", (exc as UnwantedTokenException).UnexpectedToken.Text);
            else if (exc is MissingTokenException)
                if ((exc as MissingTokenException).MissingType >= 0) {
                    string tokenName = tokenNames[(exc as MissingTokenException).MissingType];
                    return String.Format("Se esperaba '{0}', pero se ha encontrado '{1}'", ErrorsAdditionalInfo.TokenTexts[tokenName], exc.Token.Text);
                }
            else if (exc is MismatchedTokenException)
                if ((exc as MismatchedTokenException).Expecting >= 0) {
                    string tokenName = tokenNames[(exc as MismatchedTokenException).Expecting];
                    return String.Format("Se esperaba '{0}'", ErrorsAdditionalInfo.TokenTexts[tokenName]);
                }
            else if (exc is NoViableAltException) {
                return "La expresión no es una construcción válida";
            }
            return "Ha ocurrido un error de naturaleza sintáctica en la posición dada";
        }
        static string GetSemanticErrorMessage (SemanticErrorType error, string extraInfoOne = "", string extraInfoTwo = "", TigerASTNode node = null) {
            switch (error) {
                case SemanticErrorType.UndefinedIdentifier:
                    return String.Format("El nombre '{0}' no existe en el contexto actual", extraInfoOne);
                case SemanticErrorType.InvalidBreakPosition:
                    return "No existe un bucle del cual salir";
                case SemanticErrorType.IncompatibleTypes:
                    return String.Format("El tipo '{0}' es incompatible con el tipo '{1}'", extraInfoOne, extraInfoTwo);
                case SemanticErrorType.NoReturnValue:
                    return "La expresión a asignar no posee un valor de retorno";
                case SemanticErrorType.InvalidDotAccess:
                    return String.Format("El tipo '{0}' no posee un miembro llamado '{1}'", extraInfoOne, extraInfoTwo);
                case SemanticErrorType.ReturnTypeNoVisible:
                    return String.Format("El tipo '{0}' que retorna la expresión contenida no es visible en este contexto", extraInfoOne);
                case SemanticErrorType.FunctionDoesNotExist:
                    return String.Format("La función '{0}' no existe en el contexto actual", extraInfoOne);
                case SemanticErrorType.InvalidParametersNumber:
                    return String.Format("La función '{0}' posee '{1}' parámetros, pero se han especificado '{2}'", extraInfoOne, extraInfoTwo, node.ChildCount - 1);
                case SemanticErrorType.InvalidParameterType:
                    return String.Format("En la función '{0}' el tipo del parámetro '{1}' se ha especificado como '{2}', pero se declaró como '{3}'",
                                        (node.Children[0] as LanguageNode).Token.Text, extraInfoOne, (node.Children[Int32.Parse(extraInfoOne)] as ExpressionNode).ReturnType, extraInfoTwo);
                case SemanticErrorType.TypeNoIndexable:
                    return String.Format("El tipo '{0}' no es indexable", extraInfoOne);
                case SemanticErrorType.TypeNoRecordable:
                    return String.Format("El tipo '{0}' no es un record", extraInfoOne);
                case SemanticErrorType.TypeDoesNotExist:
                    return String.Format("El tipo '{0}' no existe en el contexto actual", extraInfoOne);
                case SemanticErrorType.InvalidScapeSequence:
                    return "Secuencia de escape inválida";
                case SemanticErrorType.RecordMemberDoesNotExist:
                    return String.Format("El record '{0}' no posee un miembro llamado '{1}' o se encuentra fuera de lugar", extraInfoOne, extraInfoTwo);
                case SemanticErrorType.IdentifierAlreadyExist:
                    return String.Format("El identificador '{0}' ya existe en el contexto actual", extraInfoOne);
                case SemanticErrorType.CyclicDeclaration:
                    return String.Format("Declaración cíclica entre '{0}' y '{1}'", extraInfoOne, extraInfoTwo);
                case SemanticErrorType.CyclicDeclarationNoInfo:
                    return String.Format("Declaración cíclica entre dos {0}", extraInfoOne);
                case SemanticErrorType.InvalidNilOperation:
                    return "La expresión 'nil' solo puede ser asignada a records, arrays o strings";
                case SemanticErrorType.InvalidNilComparison:
                    return "Uso inválido de la expresión 'nil' en una operación de comparación";
                case SemanticErrorType.InvalidOperandsComparer:
                    return "Las operaciones: >, <, >=, <=, solo se permiten entre dos expresiones de tipo 'string' o 'int'";
                case SemanticErrorType.InvalidOperands:
                    return String.Format("Las expresiones tienen tipos diferentes en una operación binaria: '{0}' y '{1}'", extraInfoOne, extraInfoTwo);
                case SemanticErrorType.InvalidExpressionType:
                    return String.Format("La expresión debe tener tipo '{0}', sin embargo tiene tipo '{1}'", extraInfoOne, extraInfoTwo);
                case SemanticErrorType.InvalidIfBody:
                    return String.Format("Los tipos de retorno dentro del 'if' no son iguales: '{0}' y '{1}'", extraInfoOne, extraInfoTwo);
                case SemanticErrorType.InvalidForAssigment:
                    return "Dentro de una expresión 'for' no se puede reasignar el valor de la variable índice";
                case SemanticErrorType.NoLeftAssociate:
                    return "Las operaciones '=' y '<>' no asocian a la izquierda";
                case SemanticErrorType.InvalidMembersNumber:
                    return String.Format("Se han especificado '{0}' miembros para este tipo, pero se declararon '{1}'", extraInfoOne, extraInfoTwo);
                case SemanticErrorType.NonASCIIChar:
                    return "Caracter no ASCII presente en el 'string'";
                default:
                    return "Ha ocurrido un error de naturaleza semántica en la posición dada";
            }
        }

        // QUITAR LUEGO!!!!!!!!!!!
        public static void ClearAllErrors () {
            _errors.Clear ();
        }

        public static string Print () {
            string result = String.Empty;
            foreach (var item in _errors) {
                result += item + "\n";
            }
            return result;
        }
    }

    public enum SemanticErrorType { UndefinedIdentifier, InvalidBreakPosition, IncompatibleTypes, NoReturnValue, DotAccessInArrayType, InvalidDotAccess,
        ReturnTypeNoVisible, FunctionDoesNotExist, InvalidParametersNumber, InvalidParameterType, TypeNoIndexable, TypeNoRecordable, TypeDoesNotExist,
        InvalidScapeSequence, RecordMemberDoesNotExist, IdentifierAlreadyExist, CyclicDeclaration, CyclicDeclarationNoInfo, InvalidNilOperation, InvalidOperandsComparer, 
        InvalidOperands, InvalidExpressionType, InvalidIfBody, InvalidForAssigment, NoLeftAssociate, InvalidMembersNumber, InvalidNilComparison, NonASCIIChar }
}
