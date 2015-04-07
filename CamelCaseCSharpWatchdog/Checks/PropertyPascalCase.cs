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
using System.Text.RegularExpressions;

namespace CodeWatchdog.CamelCaseCSharpWatchdog.Checks
{
    public class PropertyPascalCase
    {
        public static void Check(string startBlock, Watchdog wd)
        {
            if (!startBlock.Contains("class ")
                && !startBlock.Contains("interface ")
                && !startBlock.Contains("enum ")
                && !startBlock.Contains("(")
                && !startBlock.Contains(")"))
            {
                // Assuming it's a property
                
                string propertyName = "";
                
                Match propertyNameMatch = Regex.Match(startBlock, @"\s+(\w+)\s*$");
                
                if (propertyNameMatch.Success)
                {
                    propertyName = propertyNameMatch.Groups[1].Value;
                    
                    Logging.Debug("Property name: " + propertyName);
                }
                
                // TODO: Use central reserved words list.
                // TODO: Check if any of these aren't already being ruled out by braces etc. checks above
                //
                if (propertyName != "if"
                    && propertyName != "else"
                    && propertyName != "while"
                    && propertyName != "foreach"
                    && propertyName != "for"
                    && propertyName != "get"
                    && propertyName != "set"
                    && propertyName != "try"
                    && propertyName != "catch"
                    && propertyName != "delegate"
                    && propertyName != "using"
                    && propertyName != "switch"
                    && propertyName.Length > 2
                    && char.IsLower(propertyName, 0))
                {
                    wd.IncreaseCount((int)ErrorCodes.PascalCaseError);

                    // TODO: The line report is inaccurate, as several lines may have passed.
                    // HACK: Assuming the next line and using CheckedLinesOfCode + 1.
                    //
                    if (wd.woff != null)
                    {
                        wd.woff(string.Format("{0}: '{1}' (line {2})",
                                              wd.errorCodeStrings[(int)ErrorCodes.PascalCaseError],
                                              propertyName,
                                              wd.checkedLinesThisFile));
                    }
                }
            }

            return;
        }
    }
}

