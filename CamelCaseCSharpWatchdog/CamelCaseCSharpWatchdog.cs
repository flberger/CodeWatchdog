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
            
            statementHandler += CheckStatement;
            commentHandler += CheckComment;
            startBlockHandler += CheckStartBlock;
        }
        
        /// <summary>
        /// Check a single statement.
        /// </summary>
        /// <param name="statement">A string containing a statement, possibly multi-line.</param>
        void CheckStatement(string statement, Watchdog wd)
        {
            // Identifiers
            //
            string possibleIdentifier = "";
            
            Match firstMatch = Regex.Match(statement,
                                           @"\s+\w+(<[\w, ]+>)?\s+(\w+)\s*$");
           
            // Ignore "as" casts.
            //
            if (firstMatch.Success && !statement.Contains(" as "))
            {
                possibleIdentifier = firstMatch.Groups[2].Value;
                
                Logging.Debug("Possible identifier: " + possibleIdentifier);
            }
            else
            {
                Match secondMatch = Regex.Match(statement,
                                                @"\s+\w+(<[\w, ]+>)?\s+(\w+)\s*=");
                
                if (secondMatch.Success)
                {
                    possibleIdentifier = secondMatch.Groups[2].Value;
                    
                    Logging.Debug("Possible identifier: " + possibleIdentifier);
                }
            }
            
            if (possibleIdentifier != ""
                && possibleIdentifier != "if"
                && possibleIdentifier != "else"
                && possibleIdentifier != "while"
                && possibleIdentifier != "foreach"
                && possibleIdentifier != "for"
                && !statement.Contains("using")
                && possibleIdentifier != "get"
                && possibleIdentifier != "set"
                && possibleIdentifier != "try"
                && possibleIdentifier != "catch"
                && possibleIdentifier != "delegate"
                && possibleIdentifier != "public"
                && possibleIdentifier != "switch")
            {
                // SpecialCharacterError
                //
                if (possibleIdentifier.Contains("_"))
                {
                    if (errorCodeCount.ContainsKey((int)ErrorCodes.SpecialCharacterError))
                    {
                        errorCodeCount[(int)ErrorCodes.SpecialCharacterError] += 1;
                    }
                    else
                    {
                        errorCodeCount[(int)ErrorCodes.SpecialCharacterError] = 1;
                    }
                    
                    // TODO: The line report is inaccurate, as several lines may have passed.
                    // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                    //
                    if (woff != null)
                    {
                        woff(string.Format("{0}: '{1}' (line {2})", errorCodeStrings[(int)ErrorCodes.SpecialCharacterError], possibleIdentifier, checkedLinesThisFile + 1));
                    }
                }
                else
                {
                    if (statement.Contains("const "))
                    {
                        // PascalCaseError
                        //
                        if (possibleIdentifier.Length > 2 && char.IsLower(possibleIdentifier, 0))
                        {
                            if (errorCodeCount.ContainsKey((int)ErrorCodes.PascalCaseError))
                            {
                                errorCodeCount[(int)ErrorCodes.PascalCaseError] += 1;
                            }
                            else
                            {
                                errorCodeCount[(int)ErrorCodes.PascalCaseError] = 1;
                            }
                            
                            // TODO: The line report is inaccurate, as several lines may have passed.
                            // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                            //
                            if (woff != null)
                            {
                                woff(string.Format("{0}: '{1}' (line {2})", errorCodeStrings[(int)ErrorCodes.PascalCaseError], possibleIdentifier, checkedLinesThisFile + 1));
                            }
                        }
                    }
                    else
                    {
                        // CamelCaseError
                        //
                        if (possibleIdentifier.Length > 2 && char.IsUpper(possibleIdentifier, 0))
                        {
                            if (errorCodeCount.ContainsKey((int)ErrorCodes.CamelCaseError))
                            {
                                errorCodeCount[(int)ErrorCodes.CamelCaseError] += 1;
                            }
                            else
                            {
                                errorCodeCount[(int)ErrorCodes.CamelCaseError] = 1;
                            }
                            
                            // TODO: The line report is inaccurate, as several lines may have passed.
                            // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                            //
                            if (woff != null)
                            {
                                woff(string.Format("{0}: '{1}' (line {2})", errorCodeStrings[(int)ErrorCodes.CamelCaseError], possibleIdentifier, checkedLinesThisFile + 1));
                            }
                        }
                    }
                }
            }
            
            // MissingBracesError
            // Check for closing brace, indicating the statement is complete.
            //
            if ((statement.Trim().StartsWith("if")
                || statement.Trim().StartsWith("else")
                || statement.Trim().StartsWith("while")
                || statement.Trim().StartsWith("foreach")
                || statement.Trim().StartsWith("for"))
                && statement.Contains(")"))
            {
                if (errorCodeCount.ContainsKey((int)ErrorCodes.MissingBracesError))
                {
                    errorCodeCount[(int)ErrorCodes.MissingBracesError] += 1;
                }
                else
                {
                    errorCodeCount[(int)ErrorCodes.MissingBracesError] = 1;
                }
                
                // TODO: The line report is inaccurate, as several lines may have passed.
                // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                //
                if (woff != null)
                {
                    woff(string.Format("{0} (line {1})", errorCodeStrings[(int)ErrorCodes.MissingBracesError], checkedLinesThisFile + 1));
                }
            }
            
            return;
        }
        
        /// <summary>
        /// Check a single comment.
        /// </summary>
        /// <param name="comment">A string containing a comment, possibly multi-line.</param>
        void CheckComment(string comment, string precedingInput)
        {
            // CommentOnSameLineError
            //
            if (checkedLinesThisFile > 1 && !precedingInput.Contains("\n"))
            {
                if (errorCodeCount.ContainsKey((int)ErrorCodes.CommentOnSameLineError))
                {
                    errorCodeCount[(int)ErrorCodes.CommentOnSameLineError] += 1;
                }
                else
                {
                    errorCodeCount[(int)ErrorCodes.CommentOnSameLineError] = 1;
                }
                
                if (woff != null)
                {
                    woff(string.Format("{0} (line {1})", errorCodeStrings[(int)ErrorCodes.CommentOnSameLineError], checkedLinesThisFile));
                }
            }
            
            // CommentNoSpaceError
            // Also include /// doc comments.
            // Ignore empty comments.
            // Ignore comments starting with "--", these are most probably auto-generated decorative lines.
            //
            if (!comment.Trim().EndsWith(parsingParameters.startCommentDelimiter)
                && !(comment.StartsWith(parsingParameters.startCommentDelimiter + " ")
                     || comment.StartsWith(parsingParameters.startCommentDelimiter + "/ "))
                && !comment.StartsWith(parsingParameters.startCommentDelimiter + "--"))
            {
                if (errorCodeCount.ContainsKey((int)ErrorCodes.CommentNoSpaceError))
                {
                    errorCodeCount[(int)ErrorCodes.CommentNoSpaceError] += 1;
                }
                else
                {
                    errorCodeCount[(int)ErrorCodes.CommentNoSpaceError] = 1;
                }
                
                if (woff != null)
                {
                    woff(string.Format("{0} (line {1})", errorCodeStrings[(int)ErrorCodes.CommentNoSpaceError], checkedLinesThisFile));
                }
            }
        }
        
        /// <summary>
        /// Checks the beginning of a block.
        /// </summary>
        /// <param name="startBlock">A string containing the start block.</param>
        void CheckStartBlock(string startBlock)
        {
            if (startBlock.Contains("class "))
            {
                // ClassNotDocumentedError
                //
                if (startBlock.Contains("public ")
                    && !previousToken.Contains(parsingParameters.startCommentDelimiter + "/")
                    && !previousToken.Contains("</summary>"))
                {
                    if (errorCodeCount.ContainsKey((int)ErrorCodes.ClassNotDocumentedError))
                    {
                        errorCodeCount[(int)ErrorCodes.ClassNotDocumentedError] += 1;
                    }
                    else
                    {
                        errorCodeCount[(int)ErrorCodes.ClassNotDocumentedError] = 1;
                    }
                    
                    if (woff != null)
                    {
                        woff(string.Format("{0} (line {1})", errorCodeStrings[(int)ErrorCodes.ClassNotDocumentedError], checkedLinesThisFile));
                    }
                }
                
                string className = "";
                
                Match classNameMatch = Regex.Match(startBlock, @"\Wclass\s+(\w+)");
                
                if (classNameMatch.Success)
                {
                    className = classNameMatch.Groups[1].Value;
                    
                    Logging.Debug("Class name: " + className);
                }
                
                // PascalCaseError
                //
                if (className.Length > 2
                    && char.IsLower(className, 0))
                {
                    if (errorCodeCount.ContainsKey((int)ErrorCodes.PascalCaseError))
                    {
                        errorCodeCount[(int)ErrorCodes.PascalCaseError] += 1;
                    }
                    else
                    {
                        errorCodeCount[(int)ErrorCodes.PascalCaseError] = 1;
                    }
                    
                    // TODO: The line report is inaccurate, as several lines may have passed.
                    // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                    //
                    if (woff != null)
                    {
                        woff(string.Format("{0}: '{1}' (line {2})", errorCodeStrings[(int)ErrorCodes.PascalCaseError], className, checkedLinesThisFile));
                    }
                }
            }
            else if (startBlock.Contains("enum "))
            {
                string enumName = "";
                
                Match enumNameMatch = Regex.Match(startBlock, @"enum\s+(\w+)");
                
                if (enumNameMatch.Success)
                {
                    enumName = enumNameMatch.Groups[1].Value;
                    
                    Logging.Debug("Enum name: " + enumName);
                }
                
                // PascalCaseError
                //
                if (enumName.Length > 2 && char.IsLower(enumName, 0))
                {
                    if (errorCodeCount.ContainsKey((int)ErrorCodes.PascalCaseError))
                    {
                        errorCodeCount[(int)ErrorCodes.PascalCaseError] += 1;
                    }
                    else
                    {
                        errorCodeCount[(int)ErrorCodes.PascalCaseError] = 1;
                    }
                    
                    // TODO: The line report is inaccurate, as several lines may have passed.
                    // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                    //
                    if (woff != null)
                    {
                        woff(string.Format("{0}: '{1}' (line {2})", errorCodeStrings[(int)ErrorCodes.PascalCaseError], enumName, checkedLinesThisFile));
                    }
                }
            }
            else if (startBlock.Contains("interface "))
            {
                string interfaceName = "";
                
                Match interfaceNameMatch = Regex.Match(startBlock, @"interface\s+(\w+)");
                
                if (interfaceNameMatch.Success)
                {
                    interfaceName = interfaceNameMatch.Groups[1].Value;
                    
                    Logging.Debug("Interface name: " + interfaceName);
                }
                
                // InterfaceNamingError
                //
                if (interfaceName.Length > 2 && !interfaceName.StartsWith("I"))
                {
                    if (errorCodeCount.ContainsKey((int)ErrorCodes.InterfaceNamingError))
                    {
                        errorCodeCount[(int)ErrorCodes.InterfaceNamingError] += 1;
                    }
                    else
                    {
                        errorCodeCount[(int)ErrorCodes.InterfaceNamingError] = 1;
                    }
                    
                    // TODO: The line report is inaccurate, as several lines may have passed.
                    // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                    //
                    if (woff != null)
                    {
                        woff(string.Format("{0}: '{1}' (line {2})", errorCodeStrings[(int)ErrorCodes.InterfaceNamingError], interfaceName, checkedLinesThisFile));
                    }
                }
            }
            else if (startBlock.Contains("(") && startBlock.Contains(")"))
            {
                // MethodNotDocumentedError
                //
                // Make sure 'public' is before the first brace.
                //
                if (startBlock.Split(Char.Parse("("))[0].Contains("public ")
                    && !previousToken.Contains(parsingParameters.startCommentDelimiter + "/")
                    && !previousToken.Contains("</summary>"))
                {
                    if (errorCodeCount.ContainsKey((int)ErrorCodes.MethodNotDocumentedError))
                    {
                        errorCodeCount[(int)ErrorCodes.MethodNotDocumentedError] += 1;
                    }
                    else
                    {
                        errorCodeCount[(int)ErrorCodes.MethodNotDocumentedError] = 1;
                    }
                    
                    if (woff != null)
                    {
                        woff(string.Format("{0} (line {1})", errorCodeStrings[(int)ErrorCodes.MethodNotDocumentedError], checkedLinesThisFile));
                    }
                }
                
                string methodName = "";
                
                Match methodNameMatch = Regex.Match(startBlock, @"\w+\s+(\w+)\s*\(");
                
                if (methodNameMatch.Success)
                {
                    methodName = methodNameMatch.Groups[1].Value;
                    
                    Logging.Debug("Method name: " + methodName);
                }
                
                // PascalCaseError
                //
                if (methodName.Length > 2
                    && char.IsLower(methodName, 0)
                    && methodName != "if"
                    && methodName != "else"
                    && methodName != "while"
                    && methodName != "foreach"
                    && methodName != "for"
                    && methodName != "get"
                    && methodName != "set"
                    && methodName != "try"
                    && methodName != "catch"
                    && methodName != "delegate"
                    && methodName != "using"
                    && methodName != "public"
                    && methodName != "switch")
                {
                    if (errorCodeCount.ContainsKey((int)ErrorCodes.PascalCaseError))
                    {
                        errorCodeCount[(int)ErrorCodes.PascalCaseError] += 1;
                    }
                    else
                    {
                        errorCodeCount[(int)ErrorCodes.PascalCaseError] = 1;
                    }
                    
                    // TODO: The line report is inaccurate, as several lines may have passed.
                    // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                    //
                    if (woff != null)
                    {
                        woff(string.Format("{0}: '{1}' (line {2})", errorCodeStrings[(int)ErrorCodes.PascalCaseError], methodName, checkedLinesThisFile));
                    }
                }
            }
            else if (!startBlock.Contains("if")
                     && !startBlock.Contains("else")
                     && !startBlock.Contains("while")
                     && !startBlock.Contains("foreach")
                     && !startBlock.Contains("for")
                     && !startBlock.Contains("get")
                     && !startBlock.Contains("set")
                     && !startBlock.Contains("try")
                     && !startBlock.Contains("catch")
                     && !startBlock.Contains("delegate")
                     && !startBlock.Contains("using")
                     && !startBlock.Contains("public")
                     && !startBlock.Contains("switch"))
            {
                // Assuming it's a property
                
                string propertyName = "";
                
                Match propertyNameMatch = Regex.Match(startBlock, @"\s+(\w+)\s*$");
                
                if (propertyNameMatch.Success)
                {
                    propertyName = propertyNameMatch.Groups[1].Value;
                    
                    Logging.Debug("Property name: " + propertyName);
                }
                
                // PascalCaseError
                //
                if (propertyName.Length > 2 && char.IsLower(propertyName, 0))
                {
                    if (errorCodeCount.ContainsKey((int)ErrorCodes.PascalCaseError))
                    {
                        errorCodeCount[(int)ErrorCodes.PascalCaseError] += 1;
                    }
                    else
                    {
                        errorCodeCount[(int)ErrorCodes.PascalCaseError] = 1;
                    }
                    
                    // TODO: The line report is inaccurate, as several lines may have passed.
                    // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                    //
                    if (woff != null)
                    {
                        woff(string.Format("{0}: '{1}' (line {2})", errorCodeStrings[(int)ErrorCodes.PascalCaseError], propertyName, checkedLinesThisFile));
                    }
                }
            }
            
            return;
        }
    }
}

