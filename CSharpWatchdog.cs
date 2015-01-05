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

namespace CodeWatchdog
{
    public class CSharpWatchdog: Watchdog
    {
        // TODO: PascalCase for identifiers
        // TODO: camelCase for parameters
        // TODO: No underscore, hyphens etc. in identifiers
        
        // TODO: 4-character indents
        // TODO: No tabs
        // TODO: One statement per line
        // TODO: Parentheses for if, while, foreach, for
        
        // TODO: Comments on separate line, not at the end of a line
        // TODO: Begin comments with uppercase letter
        // TODO: End comment with a period.
        // TODO: One space between comment delimiter and text
        
        // TODO: Use var for common types and new statements.
        // TODO: Identifiers should not contain common types.
        // TODO: Use implicit typing with var in for, foreach
        
        // TODO: Prefer 'using' to 'try ... finally' for cleanups
        
        // TODO: /// comment classes
        // TODO: /// comment methods
        // TODO: /// comment public members
        
        public void Init()
        {
            STATEMENT_DELIMTER = char.Parse(";");
            START_BLOCK_DELIMITER = char.Parse("{");
            END_BLOCK_DELIMITER = char.Parse("}");
            STRING_DELIMITERS = new List<char>() {char.Parse("\"")};
            STRING_ESCAPE = char.Parse("\\");
        }
    }
}

