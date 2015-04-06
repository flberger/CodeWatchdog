// This file is part of CodeWatchdog.
//
// Copyright (c) 2014, 2015 Florian Berger <mail@florian-berger.de>
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodeWatchdog.CamelCaseCSharpWatchdog
{
    /// <summary>
    /// A Watchdog implementation for C# that favors camelCase coding style.
    /// </summary>
    public class CamelCaseCSharpWatchdog: CodeWatchdog.Watchdog
    {
        /// <summary>
        /// Initialise the underlying Watchdog for C#.
        /// Register new error checks here by adding them to the appropriate delegate.
        /// </summary>
        public override void Init()
        {
            base.Init();
            
            parsingParameters = new ParsingParameters();

            errorCodeStrings = new Dictionary<int, string>();
            
            errorCodeStrings[(int)ErrorCodes.TabError] = "Tabs instead of spaces used for indentation";
            errorCodeStrings[(int)ErrorCodes.PascalCaseError] = "Identifier is not in PascalCase";
            errorCodeStrings[(int)ErrorCodes.CamelCaseError] = "Identifier is not in camelCase";
            errorCodeStrings[(int)ErrorCodes.SpecialCharacterError] = "Disallowed character used in identifier";
            errorCodeStrings[(int)ErrorCodes.MissingBracesError] = "Missing curly braces in if / while / foreach / for";
            errorCodeStrings[(int)ErrorCodes.MultipleStatementError] = "Multiple statements on a single line";
            errorCodeStrings[(int)ErrorCodes.CommentOnSameLineError] = "Comment not on a separate line";
            errorCodeStrings[(int)ErrorCodes.CommentNoSpaceError] = "No space between comment delimiter and comment text";
            errorCodeStrings[(int)ErrorCodes.ClassNotDocumentedError] = "Public class not documented";
            errorCodeStrings[(int)ErrorCodes.MethodNotDocumentedError] = "Public method not documented";
            errorCodeStrings[(int)ErrorCodes.InterfaceNamingError] = "Interface name does not begin with an I";
            
            statementHandler += Checks.TabError.Check;
            statementHandler += Checks.MultipleStatements.Check;
            // NOTE: With braces missing, the token is being reported as a statement.
            statementHandler += Checks.MissingBraces.Check;
            statementHandler += Checks.SpecialCharacter.Check;
            statementHandler += Checks.IdentifierPascalCase.Check;
            statementHandler += Checks.CamelCase.Check;
            
            commentHandler += Checks.CommentOnSameLine.Check;
            commentHandler += Checks.CommentNoSpace.Check;
            
            startBlockHandler += Checks.ClassNotDocumented.Check;
            startBlockHandler += Checks.ClassPascalCase.Check;
            startBlockHandler += Checks.EnumPascalCase.Check;
            startBlockHandler += Checks.InterfaceNaming.Check;
            startBlockHandler += Checks.MethodNotDocumented.Check;
            startBlockHandler += Checks.MethodPascalCase.Check;
            startBlockHandler += Checks.PropertyPascalCase.Check;

            return;
        }

        /// <summary>
        /// Gets a possible identifier from a statement.
        /// </summary>
        /// <returns>The possible identifier, or an empty string.</returns>
        public static string GetPossibleIdentifier(string statement)
        {
            string possibleIdentifier = "";
            
            Match firstMatch = Regex.Match(statement, @"\s+\w+(<[\w, ]+>)?\s+(\w+)\s*$");
            
            // Ignore "as" casts.
            //
            if (firstMatch.Success && !statement.Contains(" as "))
            {
                possibleIdentifier = firstMatch.Groups[2].Value;
                
                Logging.Debug("Possible identifier: " + possibleIdentifier);
            }
            else
            {
                Match secondMatch = Regex.Match(statement, @"\s+\w+(<[\w, ]+>)?\s+(\w+)\s*=");
                
                if (secondMatch.Success)
                {
                    possibleIdentifier = secondMatch.Groups[2].Value;
                    
                    Logging.Debug("Possible identifier: " + possibleIdentifier);
                }
            }
            
            return possibleIdentifier;
        }

        public static string GetPossibleBlockIdentifier(string blockType, string startBlock)
        {
            string blockIdentifier = "";
            
            if (startBlock.Contains(blockType))
            {
                Match blockTypeNameMatch = Regex.Match(startBlock, @"\W" + blockType + @"\s+(\w+)");
                
                if (blockTypeNameMatch.Success)
                {
                    blockIdentifier = blockTypeNameMatch.Groups[1].Value;
                    
                    Logging.Debug(blockType + " name: " + blockIdentifier);
                }
            }
            
            return blockIdentifier;
        }

        public static string GetPossibleMethodName(string startBlock)
        {
            string methodName = "";
            
            // Guards
            
            if (!(startBlock.Contains("(")
                  && startBlock.Contains(")")))
            {
                return methodName;
            }
            
            // Work
            
            Match methodNameMatch = Regex.Match(startBlock, @"\w+\s+(\w+)\s*\(");

            if (methodNameMatch.Success)
            {
                methodName = methodNameMatch.Groups[1].Value;
            }

            // TODO: Use central reserved words list.
            //
            if (methodName == "if"
                || methodName == "else"
                || methodName == "while"
                || methodName == "foreach"
                || methodName == "for"
                || methodName == "get"
                || methodName == "set"
                || methodName == "try"
                || methodName == "catch"
                || methodName == "delegate"
                || methodName == "using"
                || methodName == "public"
                || methodName == "switch")
            {
                methodName = "";
            }

            if (methodName != "")
            {
                Logging.Debug("Method name: " + methodName);
            }

            return methodName;
        }        
    }
}
